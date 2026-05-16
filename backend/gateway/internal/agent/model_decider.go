package agent

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"io"
	"strings"

	"github.com/DOS/Second-Spawn/backend/gateway/internal/character"
	"github.com/DOS/Second-Spawn/backend/gateway/internal/llm"
)

type Decider interface {
	Decide(ctx context.Context, req DecisionRequest) (Decision, error)
}

type PrototypeDecider struct{}

func (PrototypeDecider) Decide(_ context.Context, req DecisionRequest) (Decision, error) {
	return DecidePrototype(req), nil
}

type ModelBackedDecider struct {
	provider  llm.Provider
	model     llm.Model
	maxTokens int
}

func NewModelBackedDecider(provider llm.Provider, model llm.Model) *ModelBackedDecider {
	if strings.TrimSpace(string(model)) == "" {
		model = llm.ModelHaikuFast
	}

	return &ModelBackedDecider{
		provider:  provider,
		model:     model,
		maxTokens: 400,
	}
}

func (d *ModelBackedDecider) Decide(ctx context.Context, req DecisionRequest) (Decision, error) {
	fallback := DecidePrototype(req)
	if d == nil || d.provider == nil {
		return fallback, nil
	}

	resp, err := d.provider.Chat(ctx, llm.ChatRequest{
		PlayerID:  req.Context.Player.PlayerID,
		NPCID:     "offline-agent",
		Model:     d.model,
		System:    modelDecisionSystemPrompt(),
		Messages:  []llm.Message{{Role: "user", Content: buildModelDecisionUserPrompt(req)}},
		MaxTokens: d.maxTokens,
	})
	if err != nil {
		return fallback, nil
	}

	decision, err := DecodeDecisionJSON(resp.Content)
	if err != nil {
		return fallback, nil
	}
	if err := ValidateDecision(req, decision); err != nil {
		return fallback, nil
	}

	return decision, nil
}

func DecodeDecisionJSON(content string) (Decision, error) {
	content = strings.TrimSpace(content)
	content = strings.TrimPrefix(content, "```json")
	content = strings.TrimPrefix(content, "```")
	content = strings.TrimSuffix(content, "```")
	content = strings.TrimSpace(content)

	var decision Decision
	decoder := json.NewDecoder(strings.NewReader(content))
	decoder.DisallowUnknownFields()
	if err := decoder.Decode(&decision); err != nil {
		return Decision{}, fmt.Errorf("decode agent decision json: %w", err)
	}
	var extra struct{}
	if err := decoder.Decode(&extra); err != io.EOF {
		return Decision{}, fmt.Errorf("decode agent decision json: trailing content")
	}

	return decision, nil
}

func modelDecisionSystemPrompt() string {
	return strings.Join([]string{
		"You are the SECOND SPAWN offline-agent decision node.",
		"Return exactly one JSON object and no prose.",
		"Your JSON must match this shape:",
		`{"action":"stop|move|attack|interact|say","target_id":"optional","move":{"x":0,"z":0},"say":"optional","reason":"short safety-grounded reason","confidence":0.0}`,
		"Never grant items, currency, XP, BodyTime, quest progress, inventory, wallet actions, or authoritative state.",
		"Choose only an action present in the allowed list.",
		"Use stop when policy, body time, danger, or uncertainty makes action unsafe.",
	}, "\n")
}

func buildModelDecisionUserPrompt(req DecisionRequest) string {
	worldJSON, err := json.Marshal(req.WorldSnapshot)
	if err != nil {
		worldJSON = []byte(`{"error":"world_snapshot_unavailable"}`)
	}
	allowedJSON, err := json.Marshal(req.Allowed)
	if err != nil {
		allowedJSON = []byte(`["stop"]`)
	}

	var b bytes.Buffer
	b.WriteString("Agent context:\n")
	b.WriteString(character.BuildAgentContextPrompt(req.Context, 10))
	b.WriteString("\n\nAllowed actions JSON:\n")
	b.Write(allowedJSON)
	b.WriteString("\n\nWorld snapshot JSON:\n")
	b.Write(worldJSON)
	b.WriteString("\n\nReturn only the decision JSON object.")
	return b.String()
}
