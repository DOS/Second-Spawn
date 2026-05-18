package llm

import (
	"context"
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"testing"
)

func TestNewDOSAIProviderReturnsNilWithoutKey(t *testing.T) {
	if provider := NewDOSAIProvider("", ""); provider != nil {
		t.Fatalf("expected nil provider without key, got %#v", provider)
	}
}

func TestDOSAIProviderChatUsesOpenAICompatibleShape(t *testing.T) {
	var seenAuth string
	var seenBody openAIChatCompletionRequest

	api := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		seenAuth = r.Header.Get("Authorization")
		if r.URL.Path != "/v1/chat/completions" {
			t.Fatalf("unexpected path %q", r.URL.Path)
		}
		if err := json.NewDecoder(r.Body).Decode(&seenBody); err != nil {
			t.Fatalf("decode request body: %v", err)
		}

		w.Header().Set("Content-Type", "application/json")
		_, _ = w.Write([]byte(`{
			"model": "claude-haiku-4.5",
			"choices": [{"message": {"role": "assistant", "content": "{\"action\":\"stop\",\"confidence\":1}"}}],
			"usage": {"prompt_tokens": 12, "completion_tokens": 6}
		}`))
	}))
	defer api.Close()

	provider := NewDOSAIProvider("dos_sk_test", api.URL+"/v1")
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

	if seenAuth != "Bearer dos_sk_test" {
		t.Fatalf("expected bearer auth header, got %q", seenAuth)
	}
	if seenBody.Model != string(ModelHaikuFast) || seenBody.MaxTokens != 64 {
		t.Fatalf("unexpected request body: %#v", seenBody)
	}
	if len(seenBody.Messages) != 2 || seenBody.Messages[0].Role != "system" || seenBody.Messages[1].Role != "user" {
		t.Fatalf("expected system plus user messages, got %#v", seenBody.Messages)
	}
	if resp.Provider != "dosai" || resp.Content != `{"action":"stop","confidence":1}` {
		t.Fatalf("unexpected response: %#v", resp)
	}
	if resp.InputTokens != 12 || resp.OutputTokens != 6 {
		t.Fatalf("unexpected token usage: %#v", resp)
	}
}
