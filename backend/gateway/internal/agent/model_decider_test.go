package agent

import (
	"context"
	"errors"
	"io"
	"log/slog"
	"net/http"
	"net/http/httptest"
	"strings"
	"testing"

	"github.com/DOS/Second-Spawn/backend/gateway/internal/character"
	"github.com/DOS/Second-Spawn/backend/gateway/internal/llm"
)

func TestModelBackedDeciderUsesProviderJSONIntent(t *testing.T) {
	provider := &fakeProvider{
		content: `{"action":"say","say":"I will keep watch near the hub.","reason":"social update is allowed and safe","confidence":0.73}`,
	}
	decider := NewModelBackedDecider(provider, llm.ModelHaikuFast)

	decision, err := decider.Decide(context.Background(), modelDecisionTestRequest([]ActionType{ActionMove, ActionSay, ActionStop}))
	if err != nil {
		t.Fatalf("expected model decision without error: %v", err)
	}

	if decision.Action != ActionSay {
		t.Fatalf("expected say decision, got %#v", decision)
	}
	if decision.Source != DecisionSourceModel || decision.SourceReason != "validated_model_intent" {
		t.Fatalf("expected model source, got %#v", decision)
	}
	if provider.lastRequest.Model != llm.ModelHaikuFast {
		t.Fatalf("expected haiku model, got %s", provider.lastRequest.Model)
	}
	if !strings.Contains(provider.lastRequest.System, "Return exactly one JSON object") {
		t.Fatalf("expected JSON-only system prompt, got %q", provider.lastRequest.System)
	}
	if !strings.Contains(provider.lastRequest.Messages[0].Content, `"safe_radius":5`) {
		t.Fatalf("expected world snapshot in user prompt, got %q", provider.lastRequest.Messages[0].Content)
	}
}

func TestDecodeDecisionJSONRejectsTrailingText(t *testing.T) {
	_, err := DecodeDecisionJSON(`{"action":"stop","confidence":1} trailing`)
	if err == nil {
		t.Fatal("expected trailing text to be rejected")
	}
}

func TestModelBackedDeciderFallsBackForInvalidOrUnavailableModel(t *testing.T) {
	tests := []struct {
		name       string
		provider   llm.Provider
		ctx        context.Context
		wantReason string
	}{
		{
			name:       "provider error",
			provider:   &fakeProvider{err: errors.New("provider unavailable")},
			ctx:        context.Background(),
			wantReason: "provider_error",
		},
		{
			name:       "provider prose",
			provider:   &fakeProvider{content: "I cannot help"},
			ctx:        context.Background(),
			wantReason: "decode_error",
		},
		{
			name:       "provider empty content",
			provider:   &fakeProvider{content: ""},
			ctx:        context.Background(),
			wantReason: "decode_error",
		},
		{
			name:       "provider invalid decision",
			provider:   &fakeProvider{content: `{"action":"attack","target_id":"enemy-1","reason":"not allowed","confidence":0.9}`},
			ctx:        context.Background(),
			wantReason: "validate_error",
		},
		{
			name:       "context canceled",
			provider:   &fakeProvider{content: `{"action":"say","say":"unused","confidence":0.5}`, honorCancellation: true},
			ctx:        canceledContext(),
			wantReason: "provider_error",
		},
		{
			name:       "anthropic 429",
			provider:   statusProvider(t, http.StatusTooManyRequests),
			ctx:        context.Background(),
			wantReason: "provider_error",
		},
		{
			name:       "anthropic 500",
			provider:   statusProvider(t, http.StatusInternalServerError),
			ctx:        context.Background(),
			wantReason: "provider_error",
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			decider := newTestDecider(tt.provider)
			decision, err := decider.Decide(tt.ctx, modelDecisionTestRequest([]ActionType{ActionMove, ActionStop}))
			if err != nil {
				t.Fatalf("expected fallback without error: %v", err)
			}

			if decision.Action != ActionMove {
				t.Fatalf("expected deterministic move fallback, got %#v", decision)
			}
			if decision.Source != DecisionSourceFallback || decision.SourceReason != tt.wantReason {
				t.Fatalf("expected fallback source %q, got %#v", tt.wantReason, decision)
			}
		})
	}
}

func modelDecisionTestRequest(allowed []ActionType) DecisionRequest {
	return DecisionRequest{
		Context: character.AgentContext{
			Player: character.PlayerProfile{PlayerID: "player-1", DisplayName: "JOY"},
			Body: character.BodyProfile{
				Characteristics: character.CharacterTraits{Sociability: 2},
				Time:            character.BodyTimeState{RemainingSeconds: 3600, MaxSeconds: 86400},
				AgentPolicy:     character.AgentPolicy{StopWhenBodyTimeBelow: 300},
				Soul:            character.SoulProfile{Name: "Scout One", CoreDrive: "map safe routes"},
			},
		},
		WorldSnapshot: WorldSnapshot{
			ZoneID:          "prototype-hub",
			Position:        Vector2{X: 0, Z: 0},
			SafeRadius:      5,
			BodyTimeSeconds: 3600,
		},
		Allowed: allowed,
	}
}

type fakeProvider struct {
	content           string
	err               error
	honorCancellation bool
	lastRequest       llm.ChatRequest
}

func (p *fakeProvider) Name() string {
	return "fake"
}

func (p *fakeProvider) Chat(ctx context.Context, req llm.ChatRequest) (*llm.ChatResponse, error) {
	p.lastRequest = req
	if p.honorCancellation {
		select {
		case <-ctx.Done():
			return nil, ctx.Err()
		default:
		}
	}
	if p.err != nil {
		return nil, p.err
	}
	return &llm.ChatResponse{
		Provider: "fake",
		Model:    req.Model,
		Content:  p.content,
	}, nil
}

func newTestDecider(provider llm.Provider) *ModelBackedDecider {
	return NewModelBackedDeciderWithLogger(provider, llm.ModelHaikuFast, slog.New(slog.NewTextHandler(io.Discard, nil)))
}

func canceledContext() context.Context {
	ctx, cancel := context.WithCancel(context.Background())
	cancel()
	return ctx
}

func statusProvider(t *testing.T, status int) llm.Provider {
	t.Helper()

	api := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, _ *http.Request) {
		http.Error(w, "provider unavailable", status)
	}))
	t.Cleanup(api.Close)

	return llm.NewAnthropicProviderWithEndpoint("test-key", api.URL)
}
