package server

import (
	"bytes"
	"context"
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"testing"

	"github.com/DOS/Second-Spawn/backend/gateway/internal/agent"
	"github.com/DOS/Second-Spawn/backend/gateway/internal/config"
)

func TestHandleHealth(t *testing.T) {
	srv := New(&config.Config{Env: "test", ListenAddr: ":0"})
	req := httptest.NewRequest(http.MethodGet, "/healthz", nil)
	rec := httptest.NewRecorder()

	srv.Routes().ServeHTTP(rec, req)

	if rec.Code != http.StatusOK {
		t.Fatalf("expected 200, got %d", rec.Code)
	}

	var body map[string]any
	if err := json.NewDecoder(rec.Body).Decode(&body); err != nil {
		t.Fatalf("decode body: %v", err)
	}
	if body["status"] != "ok" {
		t.Errorf("expected status=ok, got %v", body["status"])
	}
	if body["env"] != "test" {
		t.Errorf("expected env=test, got %v", body["env"])
	}
}

func TestHandleReady(t *testing.T) {
	srv := New(&config.Config{Env: "test"})
	req := httptest.NewRequest(http.MethodGet, "/readyz", nil)
	rec := httptest.NewRecorder()

	srv.Routes().ServeHTTP(rec, req)

	if rec.Code != http.StatusOK {
		t.Fatalf("expected 200, got %d", rec.Code)
	}
}

func TestCharacterContextLifecycle(t *testing.T) {
	srv := New(&config.Config{Env: "test"})

	getReq := httptest.NewRequest(http.MethodGet, "/v1/characters/player-1/context", nil)
	getRec := httptest.NewRecorder()
	srv.Routes().ServeHTTP(getRec, getReq)
	if getRec.Code != http.StatusOK {
		t.Fatalf("expected context 200, got %d: %s", getRec.Code, getRec.Body.String())
	}

	updateBody := []byte(`{
		"soul": {
			"name": "Scout One",
			"core_drive": "map safe paths",
			"temperament": "curious",
			"combat_style": "avoid fights",
			"social_style": "plain",
			"moral_boundaries": ["do not spend body time"],
			"long_term_goals": ["reach Enhancement"],
			"player_notes": "test note",
			"reincarnation_lore": "synthetic continuity"
		},
		"characteristics": {
			"curiosity": 9,
			"courage": 4,
			"empathy": 7,
			"discipline": 8,
			"aggression": 2,
			"sociability": 6
		},
		"agent_policy": {
			"enabled": true,
			"mode": "safe_scout",
			"max_session_seconds": 900,
			"allow_body_time_spend": false,
			"allow_risky_combat": false,
			"preferred_activities": ["talk"],
			"forbidden_activities": ["pvp"],
			"stop_when_body_time_below": 600
		}
	}`)
	updateReq := httptest.NewRequest(http.MethodPut, "/v1/characters/player-1/soul", bytes.NewReader(updateBody))
	updateRec := httptest.NewRecorder()
	srv.Routes().ServeHTTP(updateRec, updateReq)
	if updateRec.Code != http.StatusOK {
		t.Fatalf("expected update 200, got %d: %s", updateRec.Code, updateRec.Body.String())
	}

	memoryReq := httptest.NewRequest(http.MethodPost, "/v1/characters/player-1/memory", bytes.NewReader([]byte(`{
		"kind": "preference",
		"summary": "JOY prefers direct prototype progress overnight.",
		"importance": 8
	}`)))
	memoryRec := httptest.NewRecorder()
	srv.Routes().ServeHTTP(memoryRec, memoryReq)
	if memoryRec.Code != http.StatusCreated {
		t.Fatalf("expected memory 201, got %d: %s", memoryRec.Code, memoryRec.Body.String())
	}
	if !bytes.Contains(memoryRec.Body.Bytes(), []byte("direct prototype progress")) {
		t.Fatalf("expected persisted memory in response, got %s", memoryRec.Body.String())
	}
}

func TestAgentDecidePrototype(t *testing.T) {
	srv := New(&config.Config{Env: "test"})

	req := httptest.NewRequest(http.MethodPost, "/v1/agent/decide", bytes.NewReader([]byte(`{
		"context": {
			"player": {
				"player_id": "user-1",
				"display_name": "user-1"
			},
			"body": {
				"body_id": "body-user-1",
				"archetype_id": "prototype-hunter",
				"visual_prefab_key": "prototype-random",
				"equipment": {
					"primary_weapon": "none",
					"equipment_visual_id": 0
				},
				"stats": {
					"level": 1,
					"vitality": 10,
					"force": 8,
					"agility": 8,
					"focus": 8,
					"resilience": 8,
					"max_health": 100,
					"max_energy": 50,
					"attack_power": 10,
					"defense_power": 5
				},
				"characteristics": {
					"curiosity": 6,
					"courage": 5,
					"empathy": 5,
					"discipline": 5,
					"aggression": 3,
					"sociability": 5
				},
				"time": {
					"remaining_seconds": 3600,
					"max_seconds": 86400,
					"danger_drain_rate": 1
				},
				"cultivation": {
					"tier": "Awakening",
					"progress_xp": 0
				},
				"lifecycle": "alive",
				"agent_policy": {
					"enabled": true,
					"mode": "observe_and_keep_safe",
					"max_session_seconds": 1800,
					"allow_body_time_spend": false,
					"allow_risky_combat": false,
					"preferred_activities": ["explore"],
					"forbidden_activities": ["start_pvp"],
					"stop_when_body_time_below": 900
				},
				"soul": {
					"name": "user-1",
					"core_drive": "survive",
					"temperament": "careful",
					"combat_style": "avoid risky fights",
					"social_style": "brief",
					"moral_boundaries": ["do not betray allies"],
					"long_term_goals": ["reach Enhancement"],
					"player_notes": "prototype",
					"reincarnation_lore": "synthetic continuity"
				},
				"memory": [],
				"agent_runtime": {
					"profile_bootstrapped_at": "2026-05-16T00:00:00Z",
					"last_profile_bootstrap_at": "2026-05-16T00:00:00Z",
					"last_activity_at": "2026-05-16T00:00:00Z",
					"activity_count": 1,
					"decision_count": 2,
					"fallback_decision_count": 1,
					"move_intent_count": 1,
					"say_intent_count": 0,
					"stop_intent_count": 1,
					"interact_intent_count": 0,
					"offline_seconds": 45
				},
				"agent_activity": [{
					"id": "activity-bootstrap",
					"kind": "profile_bootstrap",
					"summary": "Initial profile was created.",
					"occurred_at": "2026-05-16T00:00:00Z",
					"source": "nakama"
				}]
			}
		},
		"world_snapshot": {
			"zone_id": "hub",
			"position": {"x": 0, "z": 0},
			"safe_radius": 5,
			"body_time_seconds": 3600
		},
		"allowed": ["move", "say"]
	}`)))
	rec := httptest.NewRecorder()
	srv.Routes().ServeHTTP(rec, req)
	if rec.Code != http.StatusOK {
		t.Fatalf("expected decision 200, got %d: %s", rec.Code, rec.Body.String())
	}
	if !bytes.Contains(rec.Body.Bytes(), []byte(`"action":"move"`)) &&
		!bytes.Contains(rec.Body.Bytes(), []byte(`"action":"say"`)) {
		t.Fatalf("expected move or say decision, got %s", rec.Body.String())
	}
	if !bytes.Contains(rec.Body.Bytes(), []byte(`"source":"fallback"`)) {
		t.Fatalf("expected fallback source in response, got %s", rec.Body.String())
	}
}

func TestAgentDecideUsesConfiguredDecider(t *testing.T) {
	decider := &staticAgentDecider{
		decision: agent.Decision{
			Action:       agent.ActionSay,
			Say:          "Model-backed intent is validated before use.",
			Reason:       "say is allowed for this request",
			Confidence:   0.8,
			Source:       agent.DecisionSourceModel,
			SourceReason: "validated_model_intent",
		},
	}
	srv := NewWithDependencies(&config.Config{Env: "test"}, nil, decider)

	req := httptest.NewRequest(http.MethodPost, "/v1/agent/decide", bytes.NewReader([]byte(`{
		"world_snapshot": {
			"zone_id": "hub",
			"position": {"x": 0, "z": 0},
			"safe_radius": 5,
			"body_time_seconds": 3600
		},
		"allowed": ["say"]
	}`)))
	rec := httptest.NewRecorder()
	srv.Routes().ServeHTTP(rec, req)

	if rec.Code != http.StatusOK {
		t.Fatalf("expected decision 200, got %d: %s", rec.Code, rec.Body.String())
	}
	if !bytes.Contains(rec.Body.Bytes(), []byte(`"action":"say"`)) {
		t.Fatalf("expected configured decider response, got %s", rec.Body.String())
	}
	if !bytes.Contains(rec.Body.Bytes(), []byte(`"source":"model"`)) {
		t.Fatalf("expected decision source in response, got %s", rec.Body.String())
	}
	if !decider.stopWasAllowed {
		t.Fatal("expected endpoint to add stop to allowed actions before deciding")
	}
}

func TestAgentDecideRejectsConfiguredDeciderActionOutsideAllowed(t *testing.T) {
	decider := &staticAgentDecider{
		decision: agent.Decision{
			Action:       agent.ActionAttack,
			TargetID:     "enemy-1",
			Reason:       "attack should be rejected for this request",
			Confidence:   0.8,
			Source:       agent.DecisionSourceModel,
			SourceReason: "validated_model_intent",
		},
	}
	srv := NewWithDependencies(&config.Config{Env: "test"}, nil, decider)

	req := httptest.NewRequest(http.MethodPost, "/v1/agent/decide", bytes.NewReader([]byte(`{
		"world_snapshot": {
			"zone_id": "hub",
			"position": {"x": 0, "z": 0},
			"safe_radius": 5,
			"body_time_seconds": 3600
		},
		"allowed": ["say"]
	}`)))
	rec := httptest.NewRecorder()
	srv.Routes().ServeHTTP(rec, req)

	if rec.Code != http.StatusUnprocessableEntity {
		t.Fatalf("expected decision 422, got %d: %s", rec.Code, rec.Body.String())
	}
	if !bytes.Contains(rec.Body.Bytes(), []byte(agent.ErrActionNotAllowed.Error())) {
		t.Fatalf("expected ErrActionNotAllowed response, got %s", rec.Body.String())
	}
}

func TestNPCChatPrototype(t *testing.T) {
	srv := New(&config.Config{Env: "test"})

	req := httptest.NewRequest(http.MethodPost, "/v1/npc/chat", bytes.NewReader([]byte(`{
		"player_id": "player-1",
		"npc_id": "npc-guide",
		"message": "remember this"
	}`)))
	rec := httptest.NewRecorder()
	srv.Routes().ServeHTTP(rec, req)
	if rec.Code != http.StatusOK {
		t.Fatalf("expected chat 200, got %d: %s", rec.Code, rec.Body.String())
	}
	if !bytes.Contains(rec.Body.Bytes(), []byte("voice")) {
		t.Fatalf("expected voice-ready chat response, got %s", rec.Body.String())
	}
}

type staticAgentDecider struct {
	decision       agent.Decision
	stopWasAllowed bool
}

func (d *staticAgentDecider) Decide(_ context.Context, req agent.DecisionRequest) (agent.Decision, error) {
	for _, action := range req.Allowed {
		if action == agent.ActionStop {
			d.stopWasAllowed = true
			break
		}
	}
	return d.decision, nil
}
