// Package auth handles Supabase JWT verification for incoming gateway
// requests. The Unity client authenticates with Supabase Auth (email or
// DOS Chain wallet sign-message), receives a JWT, and includes it in
// every gateway request via Authorization: Bearer <jwt>.
//
// Per Hard Rule #3 (CLAUDE.md): "NEVER put API keys in Unity client.
// All LLM calls go through Go gateway." The gateway validates the JWT
// to know which player is making the request, then enforces per-player
// rate limits + token budgets.
package auth

import (
	"context"
	"crypto/hmac"
	"crypto/sha256"
	"encoding/base64"
	"encoding/json"
	"errors"
	"net/http"
	"strings"
	"time"
)

// PlayerID is the Supabase user ID extracted from the JWT.
type PlayerID string

// Identity is the trusted player identity extracted from a verified Supabase
// access token.
type Identity struct {
	PlayerID    PlayerID
	Role        string
	Email       string
	IsAnonymous bool
	ExpiresAt   int64
}

// ErrMissingAuth is returned when the Authorization header is absent.
var ErrMissingAuth = errors.New("missing Authorization header")

// ErrInvalidJWT is returned when JWT verification fails (expired, bad
// signature, wrong issuer).
var ErrInvalidJWT = errors.New("invalid JWT")

const (
	maxAuthorizationHeaderBytes = 8 * 1024
	maxBearerTokenBytes         = 6 * 1024
	maxJWTPartBytes             = 3 * 1024
)

// Verifier validates Supabase JWTs and extracts the player ID.
// The concrete impl uses the Supabase project's JWT secret (HS256) -
// no network call to Supabase per request, the secret is enough to
// verify locally.
type Verifier interface {
	Verify(ctx context.Context, jwt string) (Identity, error)
}

// NewHS256Verifier returns a local Supabase-compatible JWT verifier.
func NewHS256Verifier(secret string) Verifier {
	secret = strings.TrimSpace(secret)
	if secret == "" {
		return nil
	}
	return hs256Verifier{secret: []byte(secret), now: func() time.Time { return time.Now().UTC() }}
}

// FromRequest extracts the bearer token from an HTTP request.
// Returns ErrMissingAuth if the header is absent or malformed.
func FromRequest(r *http.Request) (string, error) {
	authHdr := r.Header.Get("Authorization")
	if authHdr == "" {
		return "", ErrMissingAuth
	}
	if len(authHdr) > maxAuthorizationHeaderBytes {
		return "", ErrInvalidJWT
	}
	const prefix = "Bearer "
	if !strings.HasPrefix(authHdr, prefix) {
		return "", ErrMissingAuth
	}
	token := strings.TrimSpace(authHdr[len(prefix):])
	if token == "" {
		return "", ErrMissingAuth
	}
	if len(token) > maxBearerTokenBytes {
		return "", ErrInvalidJWT
	}
	return token, nil
}

type hs256Verifier struct {
	secret []byte
	now    func() time.Time
}

type jwtHeader struct {
	Algorithm string `json:"alg"`
	Type      string `json:"typ"`
}

type supabaseClaims struct {
	Subject     string `json:"sub"`
	Role        string `json:"role"`
	Email       string `json:"email"`
	IsAnonymous bool   `json:"is_anonymous"`
	ExpiresAt   int64  `json:"exp"`
}

func (v hs256Verifier) Verify(_ context.Context, jwt string) (Identity, error) {
	if len(jwt) > maxBearerTokenBytes {
		return Identity{}, ErrInvalidJWT
	}
	parts := strings.Split(jwt, ".")
	if len(parts) != 3 {
		return Identity{}, ErrInvalidJWT
	}
	for _, part := range parts {
		if part == "" || len(part) > maxJWTPartBytes {
			return Identity{}, ErrInvalidJWT
		}
	}

	var header jwtHeader
	if err := decodeJWTPart(parts[0], &header); err != nil {
		return Identity{}, ErrInvalidJWT
	}
	if header.Algorithm != "HS256" {
		return Identity{}, ErrInvalidJWT
	}

	mac := hmac.New(sha256.New, v.secret)
	mac.Write([]byte(parts[0]))
	mac.Write([]byte("."))
	mac.Write([]byte(parts[1]))
	expected := mac.Sum(nil)

	signature, err := base64.RawURLEncoding.DecodeString(parts[2])
	if err != nil || !hmac.Equal(signature, expected) {
		return Identity{}, ErrInvalidJWT
	}

	var claims supabaseClaims
	if err := decodeJWTPart(parts[1], &claims); err != nil {
		return Identity{}, ErrInvalidJWT
	}
	if strings.TrimSpace(claims.Subject) == "" {
		return Identity{}, ErrInvalidJWT
	}
	if claims.ExpiresAt > 0 && v.now().Unix() >= claims.ExpiresAt {
		return Identity{}, ErrInvalidJWT
	}

	return Identity{
		PlayerID:    PlayerID(claims.Subject),
		Role:        claims.Role,
		Email:       claims.Email,
		IsAnonymous: claims.IsAnonymous,
		ExpiresAt:   claims.ExpiresAt,
	}, nil
}

func decodeJWTPart(part string, target any) error {
	payload, err := base64.RawURLEncoding.DecodeString(part)
	if err != nil {
		return err
	}
	return json.Unmarshal(payload, target)
}
