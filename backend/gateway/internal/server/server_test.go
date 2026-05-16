package server

import (
	"bytes"
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"testing"

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
