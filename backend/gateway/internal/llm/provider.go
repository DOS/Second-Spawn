package llm

import "context"

// Provider abstracts an LLM backend (Anthropic, OpenAI, Convai).
// All providers MUST be invoked through this interface so that:
// - rate limiting + token budgeting happen in one place (the gateway),
// - prompt injection defense applies uniformly,
// - the Unity client never holds provider API keys.
type Provider interface {
	// Name returns the provider identifier (e.g. "anthropic", "openai", "convai").
	Name() string

	// Chat sends a structured prompt and returns the model's response.
	// Implementations MUST never return raw model output as a "do this"
	// instruction - the caller in internal/intent validates intent
	// against game state before any state mutation.
	Chat(ctx context.Context, req ChatRequest) (*ChatResponse, error)
}

// Model identifies which model variant to call.
// Phase 1: Convai handles NPC dialogue; Anthropic + OpenAI come online phase 2.
type Model string

const (
	ModelHaikuFast    Model = "claude-haiku-4-5"  // NPC chat (fast, cheap) - phase 2
	ModelSonnetSmart  Model = "claude-sonnet-4-6" // boss / quest-critical dialog - phase 2
	ModelOpenAIVoice  Model = "gpt-realtime"      // voice NPC via ephemeral token - phase 2
	ModelConvaiPhase1 Model = "convai-default"    // phase 1 NPC dialogue
)

// ChatRequest is the gateway-internal LLM call shape.
// PlayerID is required for rate limiting + per-player token budget.
// NPCID is required so the gateway can fetch per-NPC memory + ground the
// prompt in current world state for that NPC.
type ChatRequest struct {
	PlayerID  string
	NPCID     string
	Model     Model
	System    string
	Messages  []Message
	MaxTokens int
}

type Message struct {
	Role    string // "user", "assistant"
	Content string
}

type ChatResponse struct {
	Provider     string
	Model        Model
	Content      string
	InputTokens  int
	OutputTokens int
}
