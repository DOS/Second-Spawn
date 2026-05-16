package agent

import (
	"context"
	"errors"
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

func TestModelBackedDeciderFallsBackWhenProviderDecisionIsInvalid(t *testing.T) {
	provider := &fakeProvider{
		content: `{"action":"attack","target_id":"enemy-1","reason":"not allowed","confidence":0.9}`,
	}
	decider := NewModelBackedDecider(provider, llm.ModelHaikuFast)

	decision, err := decider.Decide(context.Background(), modelDecisionTestRequest([]ActionType{ActionMove, ActionStop}))
	if err != nil {
		t.Fatalf("expected fallback without error: %v", err)
	}

	if decision.Action != ActionMove {
		t.Fatalf("expected deterministic move fallback, got %#v", decision)
	}
}

func TestModelBackedDeciderFallsBackWhenProviderErrors(t *testing.T) {
	provider := &fakeProvider{err: errors.New("provider unavailable")}
	decider := NewModelBackedDecider(provider, llm.ModelHaikuFast)

	decision, err := decider.Decide(context.Background(), modelDecisionTestRequest([]ActionType{ActionMove, ActionStop}))
	if err != nil {
		t.Fatalf("expected fallback without error: %v", err)
	}

	if decision.Action != ActionMove {
		t.Fatalf("expected deterministic move fallback, got %#v", decision)
	}
}

func TestDecodeDecisionJSONRejectsTrailingText(t *testing.T) {
	_, err := DecodeDecisionJSON(`{"action":"stop","confidence":1} trailing`)
	if err == nil {
		t.Fatal("expected trailing text to be rejected")
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
	content     string
	err         error
	lastRequest llm.ChatRequest
}

func (p *fakeProvider) Name() string {
	return "fake"
}

func (p *fakeProvider) Chat(_ context.Context, req llm.ChatRequest) (*llm.ChatResponse, error) {
	p.lastRequest = req
	if p.err != nil {
		return nil, p.err
	}
	return &llm.ChatResponse{
		Provider: "fake",
		Model:    req.Model,
		Content:  p.content,
	}, nil
}
