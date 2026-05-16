package agent

import (
	"errors"
	"testing"
)

func TestValidateDecisionAllowsWhitelistedMove(t *testing.T) {
	req := DecisionRequest{
		Allowed: []ActionType{ActionMove, ActionStop},
	}

	err := ValidateDecision(req, Decision{
		Action:     ActionMove,
		Move:       &Vector2{X: 1, Z: 2},
		Confidence: 0.8,
	})
	if err != nil {
		t.Fatalf("expected move decision to validate: %v", err)
	}
}

func TestValidateDecisionRejectsActionOutsidePolicy(t *testing.T) {
	req := DecisionRequest{
		Allowed: []ActionType{ActionMove, ActionStop},
	}

	err := ValidateDecision(req, Decision{
		Action:     ActionAttack,
		TargetID:   "enemy-1",
		Confidence: 0.9,
	})
	if !errors.Is(err, ErrActionNotAllowed) {
		t.Fatalf("expected ErrActionNotAllowed, got %v", err)
	}
}

func TestValidateDecisionRequiresNearbyAttackTarget(t *testing.T) {
	req := DecisionRequest{
		Allowed: []ActionType{ActionAttack},
		WorldSnapshot: WorldSnapshot{
			NearbyTargets: []WorldTarget{{ID: "enemy-1"}},
		},
	}

	err := ValidateDecision(req, Decision{
		Action:     ActionAttack,
		TargetID:   "enemy-2",
		Confidence: 0.9,
	})
	if !errors.Is(err, ErrInvalidDecision) {
		t.Fatalf("expected ErrInvalidDecision, got %v", err)
	}
}

func TestValidateDecisionRequiresSayText(t *testing.T) {
	req := DecisionRequest{
		Allowed: []ActionType{ActionSay},
	}

	err := ValidateDecision(req, Decision{
		Action:     ActionSay,
		Confidence: 0.5,
	})
	if !errors.Is(err, ErrInvalidDecision) {
		t.Fatalf("expected ErrInvalidDecision, got %v", err)
	}
}
