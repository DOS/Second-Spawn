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
	"errors"
	"net/http"
	"strings"
)

// PlayerID is the Supabase user ID extracted from the JWT.
type PlayerID string

// ErrMissingAuth is returned when the Authorization header is absent.
var ErrMissingAuth = errors.New("missing Authorization header")

// ErrInvalidJWT is returned when JWT verification fails (expired, bad
// signature, wrong issuer).
var ErrInvalidJWT = errors.New("invalid JWT")

// Verifier validates Supabase JWTs and extracts the player ID.
// The concrete impl uses the Supabase project's JWT secret (HS256) -
// no network call to Supabase per request, the secret is enough to
// verify locally.
type Verifier interface {
	Verify(ctx context.Context, jwt string) (PlayerID, error)
}

// FromRequest extracts the bearer token from an HTTP request.
// Returns ErrMissingAuth if the header is absent or malformed.
func FromRequest(r *http.Request) (string, error) {
	authHdr := r.Header.Get("Authorization")
	if authHdr == "" {
		return "", ErrMissingAuth
	}
	const prefix = "Bearer "
	if !strings.HasPrefix(authHdr, prefix) {
		return "", ErrMissingAuth
	}
	token := strings.TrimSpace(authHdr[len(prefix):])
	if token == "" {
		return "", ErrMissingAuth
	}
	return token, nil
}
