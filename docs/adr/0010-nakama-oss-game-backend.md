# ADR 0010: Nakama OSS as primary game backend

**Status:** Accepted
**Date:** 2026-05-15
**Decision maker:** JOY

## Context

SECOND SPAWN needs more than plain app persistence. The vertical slice already points toward online ARPG / MMO-lite systems: player profiles, inventory, wallet, social state, leaderboards, matchmaking-adjacent flows, server-side validation, and future guild systems. ADR 0002 accepted Supabase because it was the smallest known stack, but further backend review showed a high risk that a Supabase-only approach would become a custom game backend built from scratch.

Nakama OSS is open source, self-hostable, Unity-compatible, and designed for online games. Hiro and Satori are separate Heroic Labs products with commercial / license dependencies and are not part of this decision.

## Decision

Adopt **Nakama OSS** as the primary game backend direction for SECOND SPAWN.

Initial implementation lives in `backend/nakama/` and uses the official Heroic Labs Docker image, not copied source code or a fork. The monorepo stores only project configuration and custom SECOND SPAWN runtime modules.

Supabase remains available for identity bridge, app/admin tooling, and DOS.Me ecosystem integration. Nakama owns its own database schema. For MVP production, the accepted shape is to host Nakama inside the Supabase project using dedicated role `nakama_second` and isolated schema `second`. This keeps operations simple while preserving a clean API boundary. A separate managed Postgres is a later operational isolation option, not a scale fix by itself.

Photon Fusion remains the authoritative gameplay networking layer. Nakama is not responsible for frame-level movement, combat simulation, or Fusion object authority.

## Architecture

```text
Unity client
  |                  Photon Fusion gameplay traffic
  |------------------------------------------------> Fusion host / dedicated server
  |
  |                  auth / profile / social / meta RPCs
  |------------------------------------------------> Nakama OSS
                                                        |
                                                        v
                                      Supabase Postgres schema `second`

Supabase Auth / app data
  ^                          Go LLM Gateway
  |                          ^
  | identity bridge          | NPC / offline-agent LLM calls
  +--------------------------+
```

## Scope

Included now:

- Local Docker Compose setup for Nakama + Postgres.
- Nakama Prometheus metrics port exposed for future monitoring and alerting.
- Verified Supabase Session Pooler schema-isolated mode using schema `second`.
- Pinned Nakama version and matching TypeScript runtime dependency.
- Minimal TypeScript runtime module to prove module loading.
- Documentation for local operation and upgrade policy.

Deferred:

- Unity SDK integration.
- Supabase JWT verification / custom authentication bridge.
- Production secret rotation and non-default Nakama keys. The secret names are documented in `backend/nakama/.env.example`, but values are not committed.
- Inventory, wallet, profile, quest, and SECOND token RPCs.
- Production deployment hardening, including Prometheus Alertmanager or Grafana Telegram alerts.
- Heroic Cloud migration after traction.
- Hiro and Satori.

## Alternatives considered

### Supabase-only thin backend

**Pros:** familiar, fast to start, reuses DOS.Me knowledge.

**Cons:** game-specific features would be custom-built: inventory rules, wallets, matchmaking-adjacent flows, social graph rules, leaderboards, and server-side game RPCs.

**Rejection reason:** too likely to become a homemade weaker Nakama as the project grows.

### PlayFab

**Pros:** mature managed game backend, broad feature set, Microsoft ecosystem.

**Cons:** managed lock-in, less open-source aligned, less suitable for a public repo that wants self-hostable infrastructure.

**Rejection reason:** worse fit for open-source ownership and DOS ecosystem control.

### AccelByte

**Pros:** strong enterprise game backend.

**Cons:** heavier vendor relationship and scope than a solo founder vertical slice needs.

**Rejection reason:** too enterprise-heavy for the current milestone.

### Nakama OSS

**Pros:** game-specific backend primitives, self-hostable, open source, Unity SDK, custom runtime, can be run locally and deployed on VPS.

**Cons:** new operational surface, separate database, and future complexity if Supabase identity bridge is mishandled.

**Acceptance reason:** best balance of game-backend fit, ownership, and solo-founder maintainability.

## Managed backend comparison findings

Pricing review date: 2026-05-15. Treat vendor prices as volatile and refresh before any production contract.

The backend comparison intentionally separated **Nakama OSS** from **Nakama on Heroic Cloud**:

- Nakama OSS is the current start point. Code, Docker Compose config, runtime modules, version pins, and local development flow stay in this repository. This is the best fit for AI coding agents because agents can inspect, edit, test, and reason about most of the game backend without relying on a vendor dashboard.
- Nakama on Heroic Cloud is a later managed migration path, not the current implementation shape. Heroic Cloud manages runtime infrastructure, database operations, backups, load balancers, metrics, and scaling. SECOND SPAWN can still keep custom server module source in the repo, but the cloud runtime and database operations are not repo-owned Docker infrastructure.

Cost and fit notes:

- **Nakama OSS:** lowest direct platform cost and strongest repo ownership. Main cost is operations effort: hosting, backups, monitoring, upgrades, and scale testing.
- **Nakama on Heroic Cloud:** observed minimum production tier is **$1,200/month** for Nakama. It has no DAU/MAU/CCU limit in the plan description, but capacity is still bounded by selected CPU/database resources. It does **not** include SECOND SPAWN's Unity/Fusion dedicated game servers.
- **Satori:** separate managed LiveOps product, observed minimum **$600/month** plus event-based ingestion pricing. It is not part of the MVP baseline.
- **PlayFab:** attractive free/startup path and broad Microsoft game-backend feature set, but production cost is usage-metered across events, profile reads/writes, statistics, Economy V2, lobby, matchmaking, Party, and Multiplayer Servers. For 100k MAU it can be cheap if calls are batched and sparse, or expensive if inventory/events/writes are frequent. Custom logic often moves into PlayFab/Azure execution paths, which is less transparent for AI agents than repo-owned Nakama modules.
- **AccelByte AGS:** strongest turnkey studio platform among the managed options, including online services and optional AccelByte Multiplayer Servers. Public estimates for 100k MAU show roughly **$1,689/month** for Multiplayer or **$2,420/month** for Complete, but AccelByte bills on daily PCCU, not MAU. For a long-session MMO/ARPG, real PCCU can make cost materially higher. This is a later studio-scale option, not a solo-founder MVP default.

Decision implication:

- Start with **Nakama OSS** to preserve open-source credibility, repo-level control, and AI-agent operability.
- Do **not** start with Heroic Cloud just because it is managed. Use it after traction when operations risk costs more than the monthly platform fee.
- Do **not** add Satori until SECOND SPAWN has real LiveOps needs.
- Revisit PlayFab only if Microsoft/Xbox platform benefits become strategically important enough to accept lock-in and usage-meter risk.
- Revisit AccelByte only if the project has studio-scale budget, launch pressure, and a need for turnkey backend operations.

## Consequences

### Positive

- Reduces the chance of building core game-backend primitives from scratch.
- Keeps game backend self-hostable and compatible with the open-source project direction.
- Provides a clean place for server-authoritative meta-game RPCs.
- Keeps Fusion focused on gameplay simulation.
- Avoids an extra managed Postgres service for MVP production by using schema isolation in Supabase.

### Negative

- Adds Nakama as a backend API layer and operational surface.
- Requires clear ownership boundaries to avoid duplicated profile/inventory state.
- Requires Docker and service operations earlier in the project.
- Requires discipline to keep schema `second` private and inaccessible through client-side Supabase APIs.

### Risks

- **Identity split:** Supabase user IDs and Nakama user IDs can drift.
  - Mitigation: define a single mapping contract before Unity login integration.
- **Over-scoping backend too early:** MMORPG backend ambition can overwhelm prototype work.
  - Mitigation: only add RPCs needed by the current milestone.
- **Commercial feature temptation:** Hiro/Satori may look convenient but change the cost profile.
  - Mitigation: defer until pricing and license review.
- **Supabase schema exposure:** Supabase warns that RLS is disabled on Nakama tables.
  - Mitigation: do not expose schema `second` through Supabase client APIs; treat it as private backend data accessed only by Nakama role `nakama_second`.
- **Shared database blast radius:** Nakama and app data share the Supabase project in MVP production.
  - Mitigation: monitor connection and query pressure, keep backups clear, and move schema `second` to a separate managed Postgres only if operational isolation becomes necessary.

## Validation criteria

- `docker compose config` succeeds in `backend/nakama/`.
- Nakama and Postgres start locally through Docker Compose.
- Nakama Console is reachable on `127.0.0.1:7351`.
- The custom TypeScript runtime logs a successful load message.
- The `secondspawn_health` RPC returns a JSON health response after a test client authenticates.
- Supabase Session Pooler mode can run Nakama migrations into schema `second`, start Nakama, and serve `secondspawn_health`.

## Related decisions

- ADR 0001: Photon Fusion 2
- ADR 0002: Supabase as backend
- ADR 0003: LLM safety architecture
- ADR 0004: AI agent offline control
- ADR 0008: Codex primary agent workflow
