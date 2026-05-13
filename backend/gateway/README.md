# SECOND SPAWN - LLM Gateway

Go HTTP service that fronts every LLM call from the Unity game server.
The Unity client and the dedicated game server never hold LLM API keys -
all calls are routed through this gateway, which enforces:

- Supabase JWT authentication (per-player identity)
- Per-player rate limits + daily token budget (Redis)
- Server-side intent validation (no LLM-driven state mutation)
- Prompt injection defense (reuses DOSafe patterns)
- Provider routing (Anthropic for boss / cultivation master dialogue,
  Convai for general NPC dialogue in phase 1)

Reuses the operational pattern of `D:\Projects\DOSRouter` (the Go LLM
router JOY already operates for DOSafe / DOS.AI).

## Run locally

```bash
cd backend/gateway
cp .env.example .env       # then fill in the real secrets
make run
curl localhost:8080/healthz
```

## Test

```bash
make test
```

CI runs `make test` on every PR (see `.github/workflows/backend-test.yml`).

## Build production image

```bash
make docker
docker run --rm -p 8080:8080 --env-file .env second-spawn-gateway:local
```

The production deploy target is a VPS (Hetzner) or Modal. Image is
distroless, runs as non-root, no shell.

## Package layout

```
backend/gateway/
├── main.go                 # process entry, signal handling, graceful shutdown
├── go.mod
├── Makefile
├── Dockerfile
├── .env.example
├── README.md (this file)
└── internal/
    ├── config/             # env var loader (no secrets in code)
    ├── server/             # HTTP routes, handlers, middleware
    ├── auth/               # Supabase JWT verification
    ├── llm/                # provider interface (Anthropic, OpenAI, Convai)
    └── intent/             # structured intent schema + validator contract
```

`internal/` packages are not importable outside this module - keeps the
public API of the gateway minimal (just the HTTP routes).

## Open work

The scaffold compiles and `/healthz` + `/readyz` work. Wire-up for the
real handlers (`/v1/npc/chat`, `/v1/agent/decide`, `/v1/intent/validate`)
is staged but commented out in `internal/server/server.go` until the LLM
provider implementations and Supabase JWT verifier are written.

See:
- `internal/llm/provider.go` for the provider interface
- `internal/intent/intent.go` for the intent contract
- `internal/auth/auth.go` for the JWT verifier interface
