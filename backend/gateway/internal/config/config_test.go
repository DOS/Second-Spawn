package config

import (
	"strings"
	"testing"
)

func TestLoadRejectsLegacyCharacterRoutesInProduction(t *testing.T) {
	t.Setenv("GATEWAY_ENV", "production")
	t.Setenv("SUPABASE_URL", "https://example.supabase.co")
	t.Setenv("SUPABASE_JWT_SECRET", "test-secret")
	t.Setenv("DOS_AI_API_KEY", "test-dos-ai-key")
	t.Setenv("ENABLE_LEGACY_GATEWAY_CHARACTER_ROUTES", "true")

	_, err := Load()
	if err == nil {
		t.Fatal("expected production legacy route rejection")
	}
	if !strings.Contains(err.Error(), "ENABLE_LEGACY_GATEWAY_CHARACTER_ROUTES") {
		t.Fatalf("expected legacy route error, got %v", err)
	}
}
