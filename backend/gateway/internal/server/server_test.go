package server

import (
	"bytes"
	"context"
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"strings"
	"testing"
	"time"

	"github.com/DOS/Second-Spawn/backend/gateway/internal/agent"
	"github.com/DOS/Second-Spawn/backend/gateway/internal/auth"
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
			"long_term_goals": ["survive the next expedition"],
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
				"display_name": "user-1",
				"second_balance_seconds": 604800,
				"reincarnation_count": 0
			},
			"body": {
				"body_id": "body-user-1",
				"archetype_id": "prototype-hunter",
				"visual_prefab_key": "prototype-random",
				"visual_variant": 9,
				"appearance": {
					"body_type": "synthetic_hunter",
					"body_parts": {
						"head": "crossline-optic-head",
						"face": "masked-rangefinder-face",
						"torso": "ranged-survey-torso",
						"arms": "steady-ranged-arms",
						"legs": "survey-runner-legs"
					},
					"skin": "graphite",
					"material": "matte-carbon",
					"marks": ["signal-burn", "survey-chevron"]
				},
				"inhabitation": {
					"source_actor_id": "npc-crossline-hunter-4445",
					"previous_role": "Ranged survey body",
					"inhabited_by_player": true
				},
				"equipment": {
					"primary_weapon": "none",
					"equipment_visual_id": 0,
					"weapon_visual_key": "crossbow",
					"weapon_family": "ranged",
					"combat_stance": "ranged_crossbow",
					"socket": "hands"
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
				"story": {
					"origin": "A hunter body calibrated for perimeter work.",
					"role": "Ranged survey body",
					"conflict": "It trusts patterns more than people.",
					"rumor": "Its optics still receive a signal from a silent district."
				},
				"animation_capabilities": {
					"supports_jump": false,
					"supports_roll": true,
					"supports_melee": false,
					"supports_ranged": true,
					"weapon_stance": "ranged_crossbow"
				},
				"time": {
					"remaining_seconds": 3600,
					"max_seconds": 86400,
					"danger_drain_rate": 1
				},
				"lifecycle": "alive",
				"identity": {
					"public_name": "Crossline Surveyor 5104",
					"callsign": "npc-crossline-hunter-5104",
					"public_role": "Ranged survey body",
					"faction_title": "Relay Runner",
					"profession": "Perimeter scout",
					"age_years": 34,
					"age_band": "adult",
					"home_base": "Gate Seraph Hub",
					"reputation_summary": "Known by the hub as reliable but time-poor."
				},
				"client_debug_note": "Unity may send prompt-safe body fields before the gateway contract catches up.",
				"skills": [{
					"id": "skill-scout",
					"name": "Perimeter Scout",
					"category": "profession",
					"rank": 1,
					"summary": "Reads perimeter threats."
				}],
				"agents": [{
					"id": "agent-offline-player",
					"mode": "offline_player_agent",
					"priority": 1,
					"routine": "Scout low-risk routes and preserve BodyTime.",
					"allowed_activities": ["explore"],
					"forbidden_activities": ["start_pvp"]
				}],
				"tools": [{
					"name": "move",
					"category": "intent",
					"intent": "move",
					"requires_validation": true
				}],
				"heartbeat": {
					"cadence_seconds": 60,
					"last_seen_at": "2026-05-16T00:00:00Z",
					"offline_session_state": "online",
					"last_action_summary": "Standing near the hub gate.",
					"fallback_state": "none"
				},
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
					"long_term_goals": ["survive the next expedition"],
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

func TestAgentDecideRateLimitPerPlayer(t *testing.T) {
	decider := &staticAgentDecider{
		decision: agent.Decision{
			Action:       agent.ActionSay,
			Say:          "First request is allowed.",
			Reason:       "say is allowed for this request",
			Confidence:   0.8,
			Source:       agent.DecisionSourceModel,
			SourceReason: "validated_model_intent",
		},
	}
	srv := NewWithDependencies(&config.Config{
		Env:                         "test",
		LLMRateLimitPerPlayerPerMin: 1,
		LLMTokenBudgetPerPlayerDay:  0,
	}, nil, decider)

	first := httptest.NewRecorder()
	srv.Routes().ServeHTTP(first, newAgentDecideRequest("rate-user"))
	if first.Code != http.StatusOK {
		t.Fatalf("expected first decision 200, got %d: %s", first.Code, first.Body.String())
	}

	second := httptest.NewRecorder()
	srv.Routes().ServeHTTP(second, newAgentDecideRequest("rate-user"))
	if second.Code != http.StatusTooManyRequests {
		t.Fatalf("expected second decision 429, got %d: %s", second.Code, second.Body.String())
	}
	if !bytes.Contains(second.Body.Bytes(), []byte(`"reason":"rate_limit_exceeded"`)) {
		t.Fatalf("expected rate limit reason, got %s", second.Body.String())
	}
	if second.Header().Get("Retry-After") == "" {
		t.Fatal("expected Retry-After header on rate-limited response")
	}
	if decider.calls != 1 {
		t.Fatalf("expected decider to be called once, got %d", decider.calls)
	}
}

func TestAgentDecideRateLimitUsesTrustedAuthSubject(t *testing.T) {
	decider := &staticAgentDecider{
		decision: agent.Decision{
			Action:       agent.ActionSay,
			Say:          "Authenticated subject owns the limiter key.",
			Reason:       "say is allowed for this request",
			Confidence:   0.8,
			Source:       agent.DecisionSourceModel,
			SourceReason: "validated_model_intent",
		},
	}
	srv := NewWithDependencies(&config.Config{
		Env:                         "test",
		LLMRateLimitPerPlayerPerMin: 1,
		LLMTokenBudgetPerPlayerDay:  0,
	}, nil, decider)
	srv.auth = staticAuthVerifier{playerID: "auth-user"}

	first := httptest.NewRecorder()
	srv.Routes().ServeHTTP(first, withBearer(newAgentDecideRequest("body-profile-1")))
	if first.Code != http.StatusOK {
		t.Fatalf("expected first decision 200, got %d: %s", first.Code, first.Body.String())
	}

	second := httptest.NewRecorder()
	srv.Routes().ServeHTTP(second, withBearer(newAgentDecideRequest("body-profile-2")))
	if second.Code != http.StatusTooManyRequests {
		t.Fatalf("expected second decision to use auth subject limit key, got %d: %s", second.Code, second.Body.String())
	}
	if !bytes.Contains(second.Body.Bytes(), []byte(`"player_id":"auth-user"`)) {
		t.Fatalf("expected limit response to name trusted auth subject, got %s", second.Body.String())
	}
	if decider.calls != 1 {
		t.Fatalf("expected decider to be called once, got %d", decider.calls)
	}
}

func TestAgentDecideRequiresAuthWhenVerifierConfigured(t *testing.T) {
	srv := NewWithDependencies(&config.Config{
		Env:                         "test",
		LLMRateLimitPerPlayerPerMin: 1,
	}, nil, &staticAgentDecider{})
	srv.auth = staticAuthVerifier{playerID: "auth-user"}

	rec := httptest.NewRecorder()
	srv.Routes().ServeHTTP(rec, newAgentDecideRequest("body-profile-1"))
	if rec.Code != http.StatusUnauthorized {
		t.Fatalf("expected missing auth to return 401, got %d: %s", rec.Code, rec.Body.String())
	}
}

func TestAgentDecideRejectsOversizedBearerBeforeDecider(t *testing.T) {
	decider := &staticAgentDecider{}
	srv := NewWithDependencies(&config.Config{Env: "test", SupabaseJWTSecret: "test-secret"}, nil, decider)
	req := newAgentDecideRequest("body-profile-1")
	req.Header.Set("Authorization", "Bearer "+strings.Repeat("a", 7*1024))

	rec := httptest.NewRecorder()
	srv.Routes().ServeHTTP(rec, req)
	if rec.Code != http.StatusUnauthorized {
		t.Fatalf("expected oversized auth to return 401, got %d: %s", rec.Code, rec.Body.String())
	}
	if decider.calls != 0 {
		t.Fatalf("expected decider not to be called, got %d", decider.calls)
	}
}

func TestAgentDecideTokenBudgetPerPlayer(t *testing.T) {
	decider := &staticAgentDecider{
		decision: agent.Decision{
			Action:     agent.ActionSay,
			Say:        "This should not be reached.",
			Confidence: 0.8,
		},
	}
	srv := NewWithDependencies(&config.Config{
		Env:                         "test",
		LLMRateLimitPerPlayerPerMin: 0,
		LLMTokenBudgetPerPlayerDay:  agentDecisionOutputTokenReserve - 1,
	}, nil, decider)

	rec := httptest.NewRecorder()
	srv.Routes().ServeHTTP(rec, newAgentDecideRequest("budget-user"))
	if rec.Code != http.StatusTooManyRequests {
		t.Fatalf("expected decision 429, got %d: %s", rec.Code, rec.Body.String())
	}
	if !bytes.Contains(rec.Body.Bytes(), []byte(`"reason":"token_budget_exhausted"`)) {
		t.Fatalf("expected token budget reason, got %s", rec.Body.String())
	}
	if decider.calls != 0 {
		t.Fatalf("expected decider not to be called, got %d", decider.calls)
	}
}

func TestAgentDecisionLimiterResetsWindows(t *testing.T) {
	now := time.Date(2026, 5, 17, 12, 0, 30, 0, time.UTC)
	limiter := newAgentDecisionLimiter(&config.Config{
		LLMRateLimitPerPlayerPerMin: 1,
		LLMTokenBudgetPerPlayerDay:  500,
	}, func() time.Time { return now })

	if allowed, result := limiter.Allow("reset-user", 400); !allowed {
		t.Fatalf("expected first request allowed, got %+v", result)
	}
	if allowed, result := limiter.Allow("reset-user", 1); allowed || result.Reason != "rate_limit_exceeded" {
		t.Fatalf("expected same-minute rate limit, allowed=%t result=%+v", allowed, result)
	}

	now = now.Add(time.Minute)
	if allowed, result := limiter.Allow("reset-user", 100); !allowed {
		t.Fatalf("expected next-minute request allowed, got %+v", result)
	}

	now = now.Add(time.Minute)
	if allowed, result := limiter.Allow("reset-user", 1); allowed || result.Reason != "token_budget_exhausted" {
		t.Fatalf("expected same-day budget exhaustion, allowed=%t result=%+v", allowed, result)
	}

	now = now.Add(24 * time.Hour)
	if allowed, result := limiter.Allow("reset-user", 400); !allowed {
		t.Fatalf("expected next-day budget reset, got %+v", result)
	}
}

func TestAgentDecisionLimiterPrunesExpiredPlayerState(t *testing.T) {
	now := time.Date(2026, 5, 17, 12, 0, 0, 0, time.UTC)
	limiter := newAgentDecisionLimiter(&config.Config{
		LLMRateLimitPerPlayerPerMin: 10,
	}, func() time.Time { return now })

	if allowed, result := limiter.Allow("old-user", 1); !allowed {
		t.Fatalf("expected old user request allowed, got %+v", result)
	}
	now = now.Add(agentDecisionLimitStateTTL + time.Minute)
	if allowed, result := limiter.Allow("new-user", 1); !allowed {
		t.Fatalf("expected new user request allowed, got %+v", result)
	}
	if _, ok := limiter.players["old-user"]; ok {
		t.Fatal("expected stale player limiter state to be pruned")
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
	calls          int
}

func (d *staticAgentDecider) Decide(_ context.Context, req agent.DecisionRequest) (agent.Decision, error) {
	d.calls++
	for _, action := range req.Allowed {
		if action == agent.ActionStop {
			d.stopWasAllowed = true
			break
		}
	}
	return d.decision, nil
}

type staticAuthVerifier struct {
	playerID string
}

func (v staticAuthVerifier) Verify(_ context.Context, _ string) (auth.Identity, error) {
	return auth.Identity{PlayerID: auth.PlayerID(v.playerID)}, nil
}

func newAgentDecideRequest(playerID string) *http.Request {
	return httptest.NewRequest(http.MethodPost, "/v1/agent/decide", bytes.NewReader([]byte(`{
		"context": {
			"player": {
				"player_id": "`+playerID+`",
				"display_name": "Budget Test"
			}
		},
		"world_snapshot": {
			"zone_id": "hub",
			"position": {"x": 0, "z": 0},
			"safe_radius": 5,
			"body_time_seconds": 3600
		},
		"allowed": ["say"]
	}`)))
}

func withBearer(req *http.Request) *http.Request {
	req.Header.Set("Authorization", "Bearer test-token")
	return req
}
