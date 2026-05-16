package agent

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"io"
	"log/slog"
	"strings"

	"github.com/DOS/Second-Spawn/backend/gateway/internal/character"
	"github.com/DOS/Second-Spawn/backend/gateway/internal/llm"
)

const (
	defaultModelDecisionMaxTokens = 400

	// modelDecisionMemoryItemCap bounds per-NPC memory items included in each model prompt.
	modelDecisionMemoryItemCap = 10
)

type Decider interface {
	Decide(ctx context.Context, req DecisionRequest) (Decision, error)
}

type PrototypeDecider struct{}

func (PrototypeDecider) Decide(_ context.Context, req DecisionRequest) (Decision, error) {
	return withSource(DecidePrototype(req), DecisionSourceFallback, "prototype_decider"), nil
}

type ModelBackedDecider struct {
	provider  llm.Provider
	model     llm.Model
	maxTokens int
	logger    *slog.Logger
}

func NewModelBackedDecider(provider llm.Provider, model llm.Model) *ModelBackedDecider {
	return NewModelBackedDeciderWithLogger(provider, model, slog.Default())
}

func NewModelBackedDeciderWithLogger(provider llm.Provider, model llm.Model, logger *slog.Logger) *ModelBackedDecider {
	if strings.TrimSpace(string(model)) == "" {
		model = llm.ModelHaikuFast
	}
	if logger == nil {
		logger = slog.Default()
	}

	return &ModelBackedDecider{
		provider:  provider,
		model:     model,
		maxTokens: defaultModelDecisionMaxTokens,
		logger:    logger,
	}
}

func (d *ModelBackedDecider) Decide(ctx context.Context, req DecisionRequest) (Decision, error) {
	fallback := DecidePrototype(req)
	if d == nil || d.provider == nil {
		if d != nil {
			d.warnFallback(ctx, req, "provider_unavailable", nil)
		}
		return withSource(fallback, DecisionSourceFallback, "provider_unavailable"), nil
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
		d.warnFallback(ctx, req, "provider_error", err)
		return withSource(fallback, DecisionSourceFallback, "provider_error"), nil
	}

	decision, err := DecodeDecisionJSON(resp.Content)
	if err != nil {
		d.warnFallback(ctx, req, "decode_error", err)
		return withSource(fallback, DecisionSourceFallback, "decode_error"), nil
	}
	if err := ValidateDecision(req, decision); err != nil {
		d.warnFallback(ctx, req, "validate_error", err)
		return withSource(fallback, DecisionSourceFallback, "validate_error"), nil
	}

	return withSource(decision, DecisionSourceModel, "validated_model_intent"), nil
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
	b.WriteString(character.BuildAgentContextPrompt(req.Context, modelDecisionMemoryItemCap))
	b.WriteString("\n\nAllowed actions JSON:\n")
	b.Write(allowedJSON)
	b.WriteString("\n\nWorld snapshot JSON:\n")
	b.Write(worldJSON)
	b.WriteString("\n\nReturn only the decision JSON object.")
	return b.String()
}

func (d *ModelBackedDecider) warnFallback(ctx context.Context, req DecisionRequest, reason string, err error) {
	if d == nil || d.logger == nil {
		return
	}

	attrs := []any{
		"reason", reason,
		"player_id", req.Context.Player.PlayerID,
		"model", string(d.model),
	}
	if err != nil {
		attrs = append(attrs, "error", err.Error())
	}

	d.logger.WarnContext(ctx, "agent decision falling back to deterministic intent", attrs...)
}

func withSource(decision Decision, source string, reason string) Decision {
	decision.Source = source
	decision.SourceReason = reason
	return decision
}
