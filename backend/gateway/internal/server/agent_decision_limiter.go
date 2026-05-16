package server

import (
	"strings"
	"sync"
	"time"

	"github.com/DOS/Second-Spawn/backend/gateway/internal/config"
)

type agentDecisionLimitResult struct {
	Error                string `json:"error"`
	Reason               string `json:"reason"`
	PlayerID             string `json:"player_id"`
	RetryAfterSeconds    int64  `json:"retry_after_seconds,omitempty"`
	TokenEstimate        int    `json:"token_estimate,omitempty"`
	TokenBudgetPerDay    int    `json:"token_budget_per_day,omitempty"`
	TokenBudgetUsedToday int    `json:"token_budget_used_today,omitempty"`
	TokenBudgetRemaining int    `json:"token_budget_remaining,omitempty"`
}

type agentDecisionLimiter struct {
	mu      sync.Mutex
	cfg     *config.Config
	now     func() time.Time
	players map[string]*agentDecisionLimitState
}

type agentDecisionLimitState struct {
	minuteStart time.Time
	minuteCount int
	day         string
	tokensUsed  int
	lastSeen    time.Time
}

const agentDecisionLimitStateTTL = 25 * time.Hour

func newAgentDecisionLimiter(cfg *config.Config, now func() time.Time) *agentDecisionLimiter {
	if cfg == nil {
		cfg = &config.Config{}
	}
	if now == nil {
		now = func() time.Time { return time.Now().UTC() }
	}
	return &agentDecisionLimiter{
		cfg:     cfg,
		now:     now,
		players: map[string]*agentDecisionLimitState{},
	}
}

func (l *agentDecisionLimiter) Allow(playerID string, tokenEstimate int) (bool, agentDecisionLimitResult) {
	if l == nil || l.cfg == nil || !l.enabled() {
		return true, agentDecisionLimitResult{}
	}

	playerID = normalizeLimitPlayerID(playerID)
	tokenEstimate = max(tokenEstimate, 1)
	now := l.now().UTC()
	minuteStart := now.Truncate(time.Minute)
	day := now.Format("2006-01-02")

	l.mu.Lock()
	defer l.mu.Unlock()

	l.pruneExpired(now)
	state := l.playerState(playerID, minuteStart, day)
	state.resetWindows(minuteStart, day)
	state.lastSeen = now
	if result, blocked := state.rateLimitResult(playerID, l.cfg.LLMRateLimitPerPlayerPerMin, now); blocked {
		return false, result
	}
	if result, blocked := state.tokenBudgetResult(playerID, tokenEstimate, l.cfg.LLMTokenBudgetPerPlayerDay); blocked {
		return false, result
	}

	state.minuteCount++
	state.tokensUsed += tokenEstimate
	return true, agentDecisionLimitResult{}
}

func (l *agentDecisionLimiter) enabled() bool {
	return l.cfg.LLMRateLimitPerPlayerPerMin > 0 || l.cfg.LLMTokenBudgetPerPlayerDay > 0
}

func (l *agentDecisionLimiter) playerState(playerID string, minuteStart time.Time, day string) *agentDecisionLimitState {
	state := l.players[playerID]
	if state != nil {
		return state
	}
	state = &agentDecisionLimitState{minuteStart: minuteStart, day: day}
	l.players[playerID] = state
	return state
}

func (l *agentDecisionLimiter) pruneExpired(now time.Time) {
	cutoff := now.Add(-agentDecisionLimitStateTTL)
	for playerID, state := range l.players {
		if !state.lastSeen.IsZero() && state.lastSeen.Before(cutoff) {
			delete(l.players, playerID)
		}
	}
}

func (s *agentDecisionLimitState) resetWindows(minuteStart time.Time, day string) {
	if !s.minuteStart.Equal(minuteStart) {
		s.minuteStart = minuteStart
		s.minuteCount = 0
	}
	if s.day != day {
		s.day = day
		s.tokensUsed = 0
	}
}

func (s *agentDecisionLimitState) rateLimitResult(playerID string, rateLimit int, now time.Time) (agentDecisionLimitResult, bool) {
	if rateLimit <= 0 || s.minuteCount < rateLimit {
		return agentDecisionLimitResult{}, false
	}
	retryAfter := s.minuteStart.Add(time.Minute).Sub(now)
	if retryAfter < time.Second {
		retryAfter = time.Second
	}
	return agentDecisionLimitResult{
		Error:             "agent decision rate limit exceeded",
		Reason:            "rate_limit_exceeded",
		PlayerID:          playerID,
		RetryAfterSeconds: int64(retryAfter.Seconds()),
	}, true
}

func (s *agentDecisionLimitState) tokenBudgetResult(playerID string, tokenEstimate int, tokenBudget int) (agentDecisionLimitResult, bool) {
	if tokenBudget <= 0 || s.tokensUsed+tokenEstimate <= tokenBudget {
		return agentDecisionLimitResult{}, false
	}
	return agentDecisionLimitResult{
		Error:                "agent decision token budget exhausted",
		Reason:               "token_budget_exhausted",
		PlayerID:             playerID,
		TokenEstimate:        tokenEstimate,
		TokenBudgetPerDay:    tokenBudget,
		TokenBudgetUsedToday: s.tokensUsed,
		TokenBudgetRemaining: max(tokenBudget-s.tokensUsed, 0),
	}, true
}

func normalizeLimitPlayerID(playerID string) string {
	playerID = strings.TrimSpace(playerID)
	if playerID == "" {
		return "unknown"
	}
	return playerID
}
