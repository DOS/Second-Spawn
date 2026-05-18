package config

import (
	"fmt"
	"os"
	"strings"
)

// Config holds runtime configuration loaded from environment variables.
// Secrets (API keys, JWT secrets) come from env only - never compiled in,
// never read from a checked-in file.
type Config struct {
	Env        string
	ListenAddr string

	SupabaseURL            string
	SupabaseJWTSecret      string
	SupabaseServiceRoleKey string

	AnthropicAPIKey    string
	DOSAIBaseURL       string
	DOSAIAPIKey        string
	OpenAIAPIKey       string
	ConvaiAPIKey       string
	AgentDecisionModel string

	RedisURL string

	LLMRateLimitPerPlayerPerMin int
	LLMTokenBudgetPerPlayerDay  int
}

// Load reads required env vars and returns a Config.
// Returns error if a required var is missing in production.
func Load() (*Config, error) {
	env := getEnv("GATEWAY_ENV", "development")

	cfg := &Config{
		Env:        env,
		ListenAddr: getEnv("GATEWAY_LISTEN_ADDR", defaultListenAddr()),

		SupabaseURL:            os.Getenv("SUPABASE_URL"),
		SupabaseJWTSecret:      os.Getenv("SUPABASE_JWT_SECRET"),
		SupabaseServiceRoleKey: os.Getenv("SUPABASE_SERVICE_ROLE_KEY"),

		AnthropicAPIKey:    os.Getenv("ANTHROPIC_API_KEY"),
		DOSAIBaseURL:       getEnv("DOS_AI_BASE_URL", "https://api.dos.ai/v1"),
		DOSAIAPIKey:        os.Getenv("DOS_AI_API_KEY"),
		OpenAIAPIKey:       os.Getenv("OPENAI_API_KEY"),
		ConvaiAPIKey:       os.Getenv("CONVAI_API_KEY"),
		AgentDecisionModel: getEnv("AGENT_DECISION_MODEL", "dos-ai"),

		RedisURL: getEnv("REDIS_URL", "redis://localhost:6379/0"),

		LLMRateLimitPerPlayerPerMin: getEnvInt("LLM_RATE_LIMIT_PER_PLAYER_PER_MIN", 30),
		LLMTokenBudgetPerPlayerDay:  getEnvInt("LLM_TOKEN_BUDGET_PER_PLAYER_DAY", 50000),
	}

	if env == "production" {
		var missing []string
		if cfg.SupabaseURL == "" {
			missing = append(missing, "SUPABASE_URL")
		}
		if cfg.SupabaseJWTSecret == "" {
			missing = append(missing, "SUPABASE_JWT_SECRET")
		}
		if cfg.DOSAIAPIKey == "" {
			missing = append(missing, "DOS_AI_API_KEY")
		}
		if len(missing) > 0 {
			return nil, fmt.Errorf("required env vars missing in production: %s", strings.Join(missing, ", "))
		}
	}

	return cfg, nil
}

func defaultListenAddr() string {
	port := strings.TrimSpace(os.Getenv("PORT"))
	if port == "" {
		return ":8090"
	}
	if strings.HasPrefix(port, ":") {
		return port
	}
	return ":" + port
}

func getEnv(key, fallback string) string {
	if v := os.Getenv(key); v != "" {
		return v
	}
	return fallback
}

func getEnvInt(key string, fallback int) int {
	v := os.Getenv(key)
	if v == "" {
		return fallback
	}
	var n int
	if _, err := fmt.Sscanf(v, "%d", &n); err != nil {
		return fallback
	}
	return n
}
