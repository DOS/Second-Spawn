# SECOND SPAWN - Prototype LLM Gateway Contract

Prototype Go HTTP service that fronts LLM-style calls from the Unity game
server and routes model-backed decisions through the shared `api.dos.ai`
gateway when configured.
The Unity client and the dedicated game server never hold LLM API keys -
all calls are routed through this gateway. It is designed to enforce the
following boundaries; the prototype has not wired every item yet:

- Supabase JWT authentication (per-player identity)
- Per-player rate limits + daily token budget (Redis)
- Server-side intent validation (no LLM-driven state mutation)
- Prompt injection defense (reuses DOSafe patterns)
- Provider routing through `api.dos.ai` for model-backed decisions, with
  Convai reserved for general NPC dialogue in phase 1

Reuses the operational pattern of `D:\Projects\DOSRouter` (the Go LLM
router JOY already operates for DOSafe / DOS.AI).

Boundary:

- Production AI/LLM calls should go through the shared `api.dos.ai` Go gateway.
- Game backend logic belongs in Nakama OSS runtime modules under
  `backend/nakama/`.
- Do not add profile, inventory, matchmaking, guild, wallet mutation, or
  gameplay APIs here.

## Run locally

```bash
cd backend/gateway
cp .env.example .env       # then fill in the real secrets
make run
curl localhost:8090/readyz
```

Optional agent-decision env:

- `DOS_AI_API_KEY` enables model-backed JSON decisions through `api.dos.ai`.
- `DOS_AI_BASE_URL` defaults to `https://api.dos.ai/v1`.
- `ANTHROPIC_API_KEY` is a local prototype fallback only.
- `AGENT_DECISION_MODEL` defaults to `dos-ai`.
- Without a provider key, `/v1/agent/decide` keeps using the deterministic
  fallback path.

## Test

```bash
make test
```

CI runs `make test` on every PR (see `.github/workflows/backend-test.yml`).

## Build production image

```bash
make docker
docker run --rm -p 8090:8090 --env-file .env second-spawn-gateway:local
```

Local development defaults to `:8090` because CoplayDev MCP for Unity uses
`localhost:8080`.

The preferred prototype deploy target is Google Cloud Run. The same image can
move to a VPS later if the gateway needs co-location with dedicated game server
infrastructure. Image is distroless, runs as non-root, no shell.

Cloud Run injects `PORT`; local development still defaults to `:8090`.
See `docs/setup/game-gateway-cloud-run.md`.

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
    ├── agent/              # offline-agent decision contract
    ├── auth/               # Supabase JWT verification
    ├── llm/                # provider interface (Anthropic, OpenAI, Convai)
    ├── character/          # profile, soul, stats, and agent memory contract
    └── intent/             # structured intent schema + validator contract
```

`internal/` packages are not importable outside this module - keeps the
public API of the gateway minimal (just the HTTP routes).

## Current prototype routes

The scaffold compiles and has prototype handlers for:

- `GET /readyz`
- `GET /v1/characters/{playerID}/context`
- `PUT /v1/characters/{playerID}/soul`
- `POST /v1/characters/{playerID}/memory`
- `POST /v1/agent/decide`
- `POST /v1/npc/chat`
- `POST /v1/voice/session`

`POST /v1/agent/decide` calls a DOS.AI-backed JSON intent decider when
`DOS_AI_API_KEY` is configured. Local development without a DOS.AI key,
provider errors, and invalid model output fall back to deterministic prototype
decisions so the vertical slice remains playable.

Persistent storage, full route-level Supabase JWT enforcement, and rate limiting
are still open work. Nakama custom authentication is handled inside
`backend/nakama/`, not through this gateway.

See:
- `internal/llm/provider.go` for the provider interface
- `internal/llm/anthropic.go` for the Anthropic Messages API provider
- `internal/intent/intent.go` for the intent contract
- `internal/auth/auth.go` for the JWT verifier interface
