package server

import (
	"encoding/json"
	"net/http"
	"time"

	"github.com/DOS/Second-Spawn/backend/gateway/internal/config"
)

// Server is the HTTP entrypoint for the LLM gateway.
// All LLM provider calls (Anthropic, OpenAI, Convai) are funneled through
// here. The Unity client never holds API keys - it calls this gateway with
// a Supabase JWT, the gateway validates intent server-side, then proxies
// to the chosen provider.
type Server struct {
	cfg *config.Config
}

func New(cfg *config.Config) *Server {
	return &Server{cfg: cfg}
}

// Routes registers all HTTP handlers. Keep this file small - real handler
// logic lives in internal/llm, internal/intent, internal/auth.
func (s *Server) Routes() http.Handler {
	mux := http.NewServeMux()
	mux.HandleFunc("GET /healthz", s.handleHealth)
	mux.HandleFunc("GET /readyz", s.handleReady)
	// TODO once internal/llm + internal/intent + internal/auth are
	// implemented, wire them here:
	//   mux.Handle("POST /v1/npc/chat", s.handleNPCChat())
	//   mux.Handle("POST /v1/agent/decide", s.handleAgentDecide())
	//   mux.Handle("POST /v1/intent/validate", s.handleIntentValidate())
	return mux
}

func (s *Server) handleHealth(w http.ResponseWriter, r *http.Request) {
	writeJSON(w, http.StatusOK, map[string]any{
		"status": "ok",
		"env":    s.cfg.Env,
		"ts":     time.Now().UTC().Format(time.RFC3339),
	})
}

func (s *Server) handleReady(w http.ResponseWriter, r *http.Request) {
	// TODO add real readiness checks once dependencies wired:
	// - Supabase JWT secret present
	// - Redis reachable
	// - At least one LLM provider key present (Anthropic / OpenAI / Convai)
	writeJSON(w, http.StatusOK, map[string]any{"ready": true})
}

func writeJSON(w http.ResponseWriter, status int, body any) {
	w.Header().Set("Content-Type", "application/json; charset=utf-8")
	w.WriteHeader(status)
	_ = json.NewEncoder(w).Encode(body)
}
