// Package agent defines the offline AI agent decision contract.
package agent

import (
	"errors"
	"fmt"
	"math"
	"strings"

	"github.com/DOS/Second-Spawn/backend/gateway/internal/character"
)

type ActionType string

const (
	ActionStop     ActionType = "stop"
	ActionMove     ActionType = "move"
	ActionAttack   ActionType = "attack"
	ActionInteract ActionType = "interact"
	ActionSay      ActionType = "say"
)

const (
	DecisionSourceModel    = "model"
	DecisionSourceFallback = "fallback"
)

// DecisionRequest is the bounded input sent to the LLM layer.
type DecisionRequest struct {
	Context       character.AgentContext `json:"context"`
	WorldSnapshot WorldSnapshot          `json:"world_snapshot"`
	Allowed       []ActionType           `json:"allowed"`
}

// WorldSnapshot is safe, non-authoritative context for model reasoning.
// The game server still validates every returned decision.
type WorldSnapshot struct {
	ZoneID          string        `json:"zone_id"`
	Position        Vector2       `json:"position"`
	SafeRadius      float32       `json:"safe_radius"`
	NearbyTargets   []WorldTarget `json:"nearby_targets"`
	NearbyObjects   []WorldObject `json:"nearby_objects"`
	DangerLevel     int           `json:"danger_level"`
	BodyTimeSeconds int64         `json:"body_time_seconds"`
}

type Vector2 struct {
	X float32 `json:"x"`
	Z float32 `json:"z"`
}

type WorldTarget struct {
	ID       string  `json:"id"`
	Kind     string  `json:"kind"`
	Distance float32 `json:"distance"`
	Threat   int     `json:"threat"`
}

type WorldObject struct {
	ID          string  `json:"id"`
	Kind        string  `json:"kind"`
	DisplayName string  `json:"display_name,omitempty"`
	Role        string  `json:"role,omitempty"`
	Affinity    int     `json:"affinity,omitempty"`
	Hostility   int     `json:"hostility,omitempty"`
	Distance    float32 `json:"distance"`
}

// Decision is the structured output from the LLM layer.
// It is not authoritative gameplay state.
type Decision struct {
	Action       ActionType        `json:"action"`
	TargetID     string            `json:"target_id,omitempty"`
	Move         *Vector2          `json:"move,omitempty"`
	Say          string            `json:"say,omitempty"`
	Reason       string            `json:"reason,omitempty"`
	Confidence   float32           `json:"confidence"`
	Source       string            `json:"source,omitempty"`
	SourceReason string            `json:"source_reason,omitempty"`
	Data         map[string]string `json:"data,omitempty"`
}

var (
	ErrActionNotAllowed = errors.New("agent action is not allowed")
	ErrInvalidDecision  = errors.New("agent decision is invalid")
)

func ValidateDecision(req DecisionRequest, decision Decision) error {
	if !isAllowed(req.Allowed, decision.Action) {
		return fmt.Errorf("%w: %s", ErrActionNotAllowed, decision.Action)
	}
	if decision.Confidence < 0 || decision.Confidence > 1 {
		return fmt.Errorf("%w: confidence must be between 0 and 1", ErrInvalidDecision)
	}

	switch decision.Action {
	case ActionStop:
		return nil
	case ActionMove:
		if decision.Move == nil {
			return fmt.Errorf("%w: move action requires coordinates", ErrInvalidDecision)
		}
		return nil
	case ActionAttack:
		if strings.TrimSpace(decision.TargetID) == "" {
			return fmt.Errorf("%w: attack action requires target_id", ErrInvalidDecision)
		}
		if !hasTarget(req.WorldSnapshot.NearbyTargets, decision.TargetID) {
			return fmt.Errorf("%w: target_id is not in nearby targets", ErrInvalidDecision)
		}
		return nil
	case ActionInteract:
		if strings.TrimSpace(decision.TargetID) == "" {
			return fmt.Errorf("%w: interact action requires target_id", ErrInvalidDecision)
		}
		if !hasObject(req.WorldSnapshot.NearbyObjects, decision.TargetID) {
			return fmt.Errorf("%w: target_id is not in nearby objects", ErrInvalidDecision)
		}
		return nil
	case ActionSay:
		if strings.TrimSpace(decision.Say) == "" {
			return fmt.Errorf("%w: say action requires text", ErrInvalidDecision)
		}
		if strings.TrimSpace(decision.TargetID) != "" && !hasObjectKind(req.WorldSnapshot.NearbyObjects, decision.TargetID, "nearby_actor") {
			return fmt.Errorf("%w: say target_id is not a nearby actor", ErrInvalidDecision)
		}
		return nil
	default:
		return fmt.Errorf("%w: unknown action", ErrInvalidDecision)
	}
}

// DecidePrototype is the deterministic fallback used before a real provider is
// connected. It keeps the vertical slice playable and preserves the same
// validated intent contract that an LLM provider must obey later.
func DecidePrototype(req DecisionRequest) Decision {
	if req.WorldSnapshot.BodyTimeSeconds > 0 &&
		req.Context.Body.AgentPolicy.StopWhenBodyTimeBelow > 0 &&
		req.WorldSnapshot.BodyTimeSeconds <= req.Context.Body.AgentPolicy.StopWhenBodyTimeBelow &&
		isAllowed(req.Allowed, ActionStop) {
		return Decision{
			Action:     ActionStop,
			Reason:     "body time is below the offline-agent safety threshold",
			Confidence: 1,
		}
	}

	if isAllowed(req.Allowed, ActionAttack) && req.Context.Body.AgentPolicy.AllowRiskyCombat {
		if target, ok := nearestTarget(req.WorldSnapshot.NearbyTargets); ok && target.Threat <= req.Context.Body.Characteristics.Courage {
			return Decision{
				Action:     ActionAttack,
				TargetID:   target.ID,
				Reason:     "nearby target is within policy risk tolerance",
				Confidence: 0.7,
			}
		}
	}

	if isAllowed(req.Allowed, ActionInteract) {
		if object, ok := nearestObject(req.WorldSnapshot.NearbyObjects); ok {
			return Decision{
				Action:     ActionInteract,
				TargetID:   object.ID,
				Reason:     "safe nearby object can be inspected",
				Confidence: 0.65,
			}
		}
	}

	if isAllowed(req.Allowed, ActionSay) && req.Context.Body.Characteristics.Sociability >= 6 {
		return Decision{
			Action:     ActionSay,
			Say:        fmt.Sprintf("%s is scouting the area and keeping the body safe.", req.Context.Body.Soul.Name),
			Reason:     "high sociability favors lightweight status communication",
			Confidence: 0.55,
		}
	}

	if isAllowed(req.Allowed, ActionMove) {
		return Decision{
			Action: ActionMove,
			Move: &Vector2{
				X: req.WorldSnapshot.Position.X + 1,
				Z: req.WorldSnapshot.Position.Z,
			},
			Reason:     "patrol one step inside the safe radius",
			Confidence: 0.6,
		}
	}

	return Decision{
		Action:     ActionStop,
		Reason:     "no safe allowed action is available",
		Confidence: 1,
	}
}

func isAllowed(actions []ActionType, action ActionType) bool {
	for _, allowed := range actions {
		if allowed == action {
			return true
		}
	}
	return false
}

func nearestTarget(targets []WorldTarget) (WorldTarget, bool) {
	if len(targets) == 0 {
		return WorldTarget{}, false
	}
	best := targets[0]
	for _, target := range targets[1:] {
		if target.Distance < best.Distance {
			best = target
		}
	}
	return best, !math.IsInf(float64(best.Distance), 0)
}

func nearestObject(objects []WorldObject) (WorldObject, bool) {
	if len(objects) == 0 {
		return WorldObject{}, false
	}
	best := objects[0]
	for _, object := range objects[1:] {
		if object.Distance < best.Distance {
			best = object
		}
	}
	return best, !math.IsInf(float64(best.Distance), 0)
}

func hasTarget(targets []WorldTarget, targetID string) bool {
	for _, target := range targets {
		if target.ID == targetID {
			return true
		}
	}
	return false
}

func hasObject(objects []WorldObject, objectID string) bool {
	for _, object := range objects {
		if object.ID == objectID {
			return true
		}
	}
	return false
}

func hasObjectKind(objects []WorldObject, objectID string, kind string) bool {
	for _, object := range objects {
		if object.ID == objectID && object.Kind == kind {
			return true
		}
	}
	return false
}
