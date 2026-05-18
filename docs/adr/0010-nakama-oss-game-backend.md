# ADR 0010: Start with Nakama OSS as the game backend

**Status:** Accepted
**Date:** 2026-05-15
**Decision maker:** JOY

## Context

SECOND SPAWN needs more than generic persistence. The game needs account-linked
profiles, social features, durable character state, inventory, progression,
activity logs, and eventually guilds, parties, matchmaking, leaderboards, and
LiveOps hooks.

Earlier ADR 0002 selected Supabase as the first backend baseline because it was
already familiar from DOS.Me and minimized the stack. Further research compared
Nakama OSS, Heroic Cloud, Satori, PlayFab, and AccelByte. JOY also tested local
Nakama admin access and confirmed the OSS route is feasible for development.

## Decision

Adopt **Nakama OSS** as the game backend foundation for SECOND SPAWN.

Use:

- **Nakama OSS** for game backend APIs, game accounts/session bridge, social
  primitives, storage objects, leaderboards, groups/guild candidates, and
  server-side game backend modules.
- **Photon Fusion 2 dedicated server** for authoritative in-zone movement,
  combat, physics, and tick simulation. Nakama does not replace Fusion.
- **`api.dos.ai` model service** for AI and LLM work only: model calls,
  prompt safety, provider routing, token budgets, voice token minting, and LLM
  decision filtering. Do not put game profile, inventory, matchmaking, guild,
  wallet mutation, or gameplay APIs in the model service.
- **Supabase** as a compatible sidecar where it still earns its place:
  DOS.Me-style identity bridge, wallet/profile integration, storage, analytics,
  or external product data. Do not assume Supabase Realtime is combat sync.
- **Postgres** as the durable database under Nakama. Development may use a local
  Postgres container or an approved Supabase Postgres project if connection
  behavior and isolation are verified.

## Rationale

- Nakama OSS is purpose-built for online game backend work while staying
  self-hostable and source-available for agent inspection.
- It avoids the early managed-cost floor of Heroic Cloud, where Nakama starts
  around USD 1,200/month and Satori starts around USD 600/month.
- It avoids PlayFab's usage-billing uncertainty for write-heavy game state and
  avoids deeper Azure lock-in for custom server logic.
- It avoids AccelByte's higher managed-platform cost and enterprise-oriented
  sales motion during the solo-founder prototype phase.
- It gives AI coding agents a concrete local backend to inspect, test, and
  extend through repo-owned modules and Docker-based development.
- It fits the open-source project posture better than a closed managed backend
  being the only source of truth.

## Consequences

- ADR 0002 is superseded for the default game backend decision. Supabase remains
  available as a sidecar, not the primary game backend baseline.
- Backend code must distinguish **game backend** from **model service**. Nakama
  is the game backend. `api.dos.ai` is the shared DOS.AI AI/LLM model service.
- Supabase Auth remains the external identity source for the first prototype.
  Nakama receives accounts only through its `beforeAuthenticateCustom` runtime
  hook, which verifies the Supabase access token directly with Supabase Auth. It
  must not trust a raw Supabase user ID from the Unity client.
- Fusion remains authoritative for real-time gameplay. Nakama stores durable
  state and serves backend APIs, but it does not directly trust client actions.
- LLM output still never mutates state directly. `api.dos.ai` and game-server
  validation remain mandatory before Nakama storage is updated.
- Heroic Cloud, Hiro, Satori, PlayFab, AccelByte, OpenAuth, or any other
  replacement stack still require a new ADR and JOY approval.
- If Nakama OSS operations become too heavy, the upgrade path is Heroic Cloud or
  another managed platform, not rewriting gameplay code blindly.
- Nakama OSS is not assumed to be a multi-node cluster. Multiple Nakama OSS
  instances must not share one logical Nakama database as a fake horizontal
  scale-out plan. If the project shards Nakama OSS, each shard gets its own
  logical database, even if those databases live on the same physical Postgres
  server or cluster.

## Implementation Notes

- Keep game backend custom logic in Nakama runtime modules by default. A
  separate custom game backend requires a new ADR.
- Treat `backend/nakama/` as the repo-owned game backend runtime. It may call
  `api.dos.ai`, but it must not become a generic model gateway or bypass
  server-side gameplay validation.
- Keep Nakama runtime modules under `backend/nakama/`.
- Use Nakama `beforeAuthenticateCustom` to validate Supabase access tokens
  directly against Supabase Auth, then rewrite the incoming custom auth request
  to a stable Nakama custom ID.
- Store secrets only in local `.env` or deployment secret managers.
- Keep local Docker config public-safe with placeholders only.
- Keep module code small and testable because JOY is a non-coder and agents
  must be able to review it.
- For early scale, prefer explicit region or world shards:
  `Nakama asia-1 -> nakama_asia_1`, `Nakama asia-2 -> nakama_asia_2`,
  `Nakama us-1 -> nakama_us_1`. A shared global identity and entitlement layer
  may route players to shards, but shard-local Nakama databases own current
  body, stats, inventory, `BodyTime`, memories, relationships, NPC state, and
  agent runtime logs.
- Mature open-source Nakama multi-node clustering was not found in the May 2026
  backend research pass. The known community attempt,
  `doublemo/nakama-cluster`, is archived and too small to rely on for
  production. Heroic Enterprise/Heroic Cloud remains the official clustering
  path unless a future ADR chooses a custom distributed service.

## Alternatives Considered

### Supabase-first thin backend

Simple and familiar, but it pushes too much game-specific backend work onto
custom services once guilds, parties, matchmaking, and agent activity logs grow.

### PlayFab

Strong managed game backend and generous early tier, but custom logic usually
routes through CloudScript or Azure Functions. Usage billing can become hard to
predict for write-heavy MMO-style data.

### Heroic Cloud

Best operational path for managed Nakama, but the minimum monthly cost is too
high for the current prototype stage.

### AccelByte

Enterprise-grade and strong for large studios, but too expensive and too heavy
for the current solo-founder stage.

## Revisit Criteria

Revisit this decision if:

- Nakama OSS slows prototype delivery more than it accelerates backend work.
- Operations become the main blocker before product-market validation.
- Managed pricing becomes acceptable because the game has revenue or funding.
- Supabase-only implementation proves enough for the actual vertical slice.
- A future backend provider offers materially better AI-agent development
  ergonomics, open-source compatibility, and predictable cost.

## Cross-References

- [ADR 0002: Supabase as backend](0002-supabase-backend.md)
- [ADR 0003: LLM safety architecture](0003-llm-safety-architecture.md)
- [ADR 0004: AI Agent control of player character when offline](0004-ai-agent-offline-control.md)
- [Character Profile, Soul, and Agent Memory](../design/10-character-profile-agent-memory.md)
