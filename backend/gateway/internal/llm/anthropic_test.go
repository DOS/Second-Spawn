package llm

import (
	"context"
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"testing"
)

func TestNewAnthropicProviderReturnsNilWithoutKey(t *testing.T) {
	if provider := NewAnthropicProvider(""); provider != nil {
		t.Fatalf("expected nil provider without key, got %#v", provider)
	}
}

func TestAnthropicProviderChatUsesMessagesAPIShape(t *testing.T) {
	var seenKey string
	var seenVersion string
	var seenBody anthropicMessageRequest

	api := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		seenKey = r.Header.Get("x-api-key")
		seenVersion = r.Header.Get("anthropic-version")
		if err := json.NewDecoder(r.Body).Decode(&seenBody); err != nil {
			t.Fatalf("decode request body: %v", err)
		}

		w.Header().Set("Content-Type", "application/json")
		_, _ = w.Write([]byte(`{
			"content": [{"type": "text", "text": "{\"action\":\"stop\",\"confidence\":1}"}],
			"usage": {"input_tokens": 12, "output_tokens": 6}
		}`))
	}))
	defer api.Close()

	provider := NewAnthropicProviderWithEndpoint("test-key", api.URL)
	resp, err := provider.Chat(context.Background(), ChatRequest{
		PlayerID:  "player-1",
		NPCID:     "offline-agent",
		Model:     ModelHaikuFast,
		System:    "Return JSON.",
		Messages:  []Message{{Role: "user", Content: "state"}},
		MaxTokens: 64,
	})
	if err != nil {
		t.Fatalf("expected chat response: %v", err)
	}

	if seenKey != "test-key" {
		t.Fatalf("expected API key header, got %q", seenKey)
	}
	if seenVersion != "2023-06-01" {
		t.Fatalf("expected anthropic-version header, got %q", seenVersion)
	}
	if seenBody.Model != string(ModelHaikuFast) || seenBody.MaxTokens != 64 {
		t.Fatalf("unexpected request body: %#v", seenBody)
	}
	if resp.Content != `{"action":"stop","confidence":1}` {
		t.Fatalf("unexpected response content: %q", resp.Content)
	}
	if resp.InputTokens != 12 || resp.OutputTokens != 6 {
		t.Fatalf("unexpected token usage: %#v", resp)
	}
}
