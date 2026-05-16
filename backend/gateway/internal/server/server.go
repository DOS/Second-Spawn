package server

import (
	"encoding/json"
	"errors"
	"net/http"
	"strings"
	"time"

	"github.com/DOS/Second-Spawn/backend/gateway/internal/agent"
	"github.com/DOS/Second-Spawn/backend/gateway/internal/character"
	"github.com/DOS/Second-Spawn/backend/gateway/internal/config"
)

// Server is the HTTP entrypoint for the LLM gateway.
// All LLM provider calls (Anthropic, OpenAI, Convai) are funneled through
// here. The Unity client never holds API keys - it calls this gateway with
// a Supabase JWT, the gateway validates intent server-side, then proxies
// to the chosen provider.
type Server struct {
	cfg   *config.Config
	store character.Store
}

func New(cfg *config.Config) *Server {
	return NewWithStore(cfg, character.NewMemoryStore())
}

func NewWithStore(cfg *config.Config, store character.Store) *Server {
	if store == nil {
		store = character.NewMemoryStore()
	}
	return &Server{cfg: cfg, store: store}
}

// Routes registers all HTTP handlers. Keep this file small - real handler
// logic lives in internal/llm, internal/intent, internal/auth.
func (s *Server) Routes() http.Handler {
	mux := http.NewServeMux()
	mux.HandleFunc("GET /healthz", s.handleHealth)
	mux.HandleFunc("GET /readyz", s.handleReady)
	mux.HandleFunc("GET /v1/characters/{playerID}/context", s.handleGetAgentContext)
	mux.HandleFunc("PUT /v1/characters/{playerID}/soul", s.handleUpdateSoul)
	mux.HandleFunc("POST /v1/characters/{playerID}/memory", s.handleAddMemory)
	mux.HandleFunc("POST /v1/agent/decide", s.handleAgentDecide)
	mux.HandleFunc("POST /v1/npc/chat", s.handleNPCChat)
	mux.HandleFunc("POST /v1/voice/session", s.handleVoiceSession)
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

func (s *Server) handleGetAgentContext(w http.ResponseWriter, r *http.Request) {
	ctx, err := s.store.GetOrCreateContext(r.Context(), r.PathValue("playerID"))
	if err != nil {
		writeError(w, err)
		return
	}

	writeJSON(w, http.StatusOK, ctx)
}

type updateSoulRequest struct {
	Soul            character.SoulProfile     `json:"soul"`
	Characteristics character.CharacterTraits `json:"characteristics"`
	AgentPolicy     character.AgentPolicy     `json:"agent_policy"`
}

func (s *Server) handleUpdateSoul(w http.ResponseWriter, r *http.Request) {
	var req updateSoulRequest
	if err := decodeJSON(r, &req); err != nil {
		writeJSON(w, http.StatusBadRequest, map[string]any{"error": err.Error()})
		return
	}

	ctx, err := s.store.UpdateSoul(r.Context(), r.PathValue("playerID"), req.Soul, req.Characteristics, req.AgentPolicy)
	if err != nil {
		writeError(w, err)
		return
	}

	writeJSON(w, http.StatusOK, ctx)
}

func (s *Server) handleAddMemory(w http.ResponseWriter, r *http.Request) {
	var req character.MemoryRecord
	if err := decodeJSON(r, &req); err != nil {
		writeJSON(w, http.StatusBadRequest, map[string]any{"error": err.Error()})
		return
	}

	ctx, err := s.store.AddMemory(r.Context(), r.PathValue("playerID"), req)
	if err != nil {
		writeError(w, err)
		return
	}

	writeJSON(w, http.StatusCreated, ctx)
}

func (s *Server) handleAgentDecide(w http.ResponseWriter, r *http.Request) {
	var req agent.DecisionRequest
	if err := decodeJSON(r, &req); err != nil {
		writeJSON(w, http.StatusBadRequest, map[string]any{"error": err.Error()})
		return
	}

	if strings.TrimSpace(req.Context.Player.PlayerID) == "" {
		ctx, err := s.store.GetOrCreateContext(r.Context(), "dev-player")
		if err != nil {
			writeError(w, err)
			return
		}
		req.Context = ctx
	}
	req.Allowed = ensureStopAllowed(req.Allowed)
	decision := agent.DecidePrototype(req)
	if err := agent.ValidateDecision(req, decision); err != nil {
		writeJSON(w, http.StatusUnprocessableEntity, map[string]any{"error": err.Error(), "decision": decision})
		return
	}

	writeJSON(w, http.StatusOK, decision)
}

type npcChatRequest struct {
	PlayerID string `json:"player_id"`
	NPCID    string `json:"npc_id"`
	Message  string `json:"message"`
}

type npcChatResponse struct {
	PlayerID       string `json:"player_id"`
	NPCID          string `json:"npc_id"`
	Text           string `json:"text"`
	VoiceAvailable bool   `json:"voice_available"`
	Provider       string `json:"provider"`
}

func (s *Server) handleNPCChat(w http.ResponseWriter, r *http.Request) {
	var req npcChatRequest
	if err := decodeJSON(r, &req); err != nil {
		writeJSON(w, http.StatusBadRequest, map[string]any{"error": err.Error()})
		return
	}
	if strings.TrimSpace(req.PlayerID) == "" {
		req.PlayerID = "dev-player"
	}
	if strings.TrimSpace(req.NPCID) == "" {
		req.NPCID = "prototype-npc"
	}

	ctx, err := s.store.GetOrCreateContext(r.Context(), req.PlayerID)
	if err != nil {
		writeError(w, err)
		return
	}

	text := prototypeNPCReply(ctx, req.Message)
	writeJSON(w, http.StatusOK, npcChatResponse{
		PlayerID:       req.PlayerID,
		NPCID:          req.NPCID,
		Text:           text,
		VoiceAvailable: false,
		Provider:       "prototype-text",
	})
}

type voiceSessionResponse struct {
	VoiceAvailable         bool   `json:"voice_available"`
	Provider               string `json:"provider"`
	RequiresEphemeralToken bool   `json:"requires_ephemeral_token"`
	Reason                 string `json:"reason"`
}

func (s *Server) handleVoiceSession(w http.ResponseWriter, r *http.Request) {
	writeJSON(w, http.StatusOK, voiceSessionResponse{
		VoiceAvailable:         false,
		Provider:               "openai-realtime",
		RequiresEphemeralToken: true,
		Reason:                 "voice contract is wired; ephemeral token minting waits for provider credentials",
	})
}

func writeJSON(w http.ResponseWriter, status int, body any) {
	w.Header().Set("Content-Type", "application/json; charset=utf-8")
	w.WriteHeader(status)
	_ = json.NewEncoder(w).Encode(body)
}

func decodeJSON(r *http.Request, target any) error {
	defer r.Body.Close()
	decoder := json.NewDecoder(r.Body)
	decoder.DisallowUnknownFields()
	return decoder.Decode(target)
}

func writeError(w http.ResponseWriter, err error) {
	status := http.StatusInternalServerError
	if errors.Is(err, character.ErrPlayerIDRequired) || errors.Is(err, character.ErrMemoryInvalid) {
		status = http.StatusBadRequest
	}
	writeJSON(w, status, map[string]any{"error": err.Error()})
}

func ensureStopAllowed(actions []agent.ActionType) []agent.ActionType {
	for _, action := range actions {
		if action == agent.ActionStop {
			return actions
		}
	}
	return append(actions, agent.ActionStop)
}

func prototypeNPCReply(ctx character.AgentContext, message string) string {
	message = strings.TrimSpace(message)
	if message == "" {
		return "I can hear you. Tell me what you want this body to remember."
	}

	name := ctx.Body.Soul.Name
	if strings.TrimSpace(name) == "" {
		name = "your agent"
	}
	return name + " remembers the shape of that intent. For now I can answer in text; voice will come through the gateway once ephemeral provider tokens are enabled."
}
