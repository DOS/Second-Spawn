package auth

import (
	"context"
	"crypto/hmac"
	"crypto/sha256"
	"encoding/base64"
	"net/http/httptest"
	"strings"
	"testing"
	"time"
)

func TestHS256VerifierAcceptsValidSupabaseToken(t *testing.T) {
	verifier := hs256Verifier{
		secret: []byte("test-secret"),
		now:    func() time.Time { return time.Unix(100, 0).UTC() },
	}
	token := signTestJWT(t, "test-secret", `{"alg":"HS256","typ":"JWT"}`, `{"sub":"player-1","role":"authenticated","email":"p@example.test","exp":200}`)

	identity, err := verifier.Verify(context.Background(), token)
	if err != nil {
		t.Fatalf("expected token to verify: %v", err)
	}
	if identity.PlayerID != "player-1" {
		t.Fatalf("expected player id from sub, got %q", identity.PlayerID)
	}
	if identity.Role != "authenticated" {
		t.Fatalf("expected role from token, got %q", identity.Role)
	}
}

func TestHS256VerifierRejectsInvalidSignature(t *testing.T) {
	verifier := hs256Verifier{
		secret: []byte("test-secret"),
		now:    func() time.Time { return time.Unix(100, 0).UTC() },
	}
	token := signTestJWT(t, "other-secret", `{"alg":"HS256","typ":"JWT"}`, `{"sub":"player-1","exp":200}`)

	if _, err := verifier.Verify(context.Background(), token); err != ErrInvalidJWT {
		t.Fatalf("expected ErrInvalidJWT, got %v", err)
	}
}

func TestHS256VerifierRejectsExpiredToken(t *testing.T) {
	verifier := hs256Verifier{
		secret: []byte("test-secret"),
		now:    func() time.Time { return time.Unix(300, 0).UTC() },
	}
	token := signTestJWT(t, "test-secret", `{"alg":"HS256","typ":"JWT"}`, `{"sub":"player-1","exp":200}`)

	if _, err := verifier.Verify(context.Background(), token); err != ErrInvalidJWT {
		t.Fatalf("expected ErrInvalidJWT, got %v", err)
	}
}

func TestFromRequestRejectsOversizedBearerToken(t *testing.T) {
	req := httptest.NewRequest("POST", "/v1/agent/decide", nil)
	req.Header.Set("Authorization", "Bearer "+strings.Repeat("a", maxBearerTokenBytes+1))

	if _, err := FromRequest(req); err != ErrInvalidJWT {
		t.Fatalf("expected ErrInvalidJWT, got %v", err)
	}
}

func TestHS256VerifierRejectsOversizedJWTPart(t *testing.T) {
	verifier := hs256Verifier{
		secret: []byte("test-secret"),
		now:    func() time.Time { return time.Unix(100, 0).UTC() },
	}
	token := strings.Repeat("a", maxJWTPartBytes+1) + ".payload.signature"

	if _, err := verifier.Verify(context.Background(), token); err != ErrInvalidJWT {
		t.Fatalf("expected ErrInvalidJWT, got %v", err)
	}
}

func signTestJWT(t *testing.T, secret string, header string, claims string) string {
	t.Helper()
	encodedHeader := base64.RawURLEncoding.EncodeToString([]byte(header))
	encodedClaims := base64.RawURLEncoding.EncodeToString([]byte(claims))
	unsigned := encodedHeader + "." + encodedClaims
	mac := hmac.New(sha256.New, []byte(secret))
	mac.Write([]byte(unsigned))
	return unsigned + "." + base64.RawURLEncoding.EncodeToString(mac.Sum(nil))
}
