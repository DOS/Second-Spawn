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

Supabase remains available for identity bridge, app/admin tooling, and DOS.Me ecosystem integration. Nakama owns its own database schema. The preferred clean production shape is a separate Nakama Postgres database, but a Supabase project can host Nakama for MVP if Nakama uses a dedicated role and the isolated `second` schema.

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
                                                   Nakama Postgres

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
- Production secret rotation and non-default Nakama keys.
- Inventory, wallet, profile, quest, and SECOND token RPCs.
- Production deployment hardening, including Prometheus Alertmanager or Grafana Telegram alerts.
- Hiro, Satori, and Heroic Cloud.

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

## Consequences

### Positive

- Reduces the chance of building core game-backend primitives from scratch.
- Keeps game backend self-hostable and compatible with the open-source project direction.
- Provides a clean place for server-authoritative meta-game RPCs.
- Keeps Fusion focused on gameplay simulation.

### Negative

- Adds a second backend database beside Supabase, unless MVP production uses a schema-isolated Supabase project.
- Requires clear ownership boundaries to avoid duplicated profile/inventory state.
- Requires Docker and service operations earlier in the project.

### Risks

- **Identity split:** Supabase user IDs and Nakama user IDs can drift.
  - Mitigation: define a single mapping contract before Unity login integration.
- **Over-scoping backend too early:** MMORPG backend ambition can overwhelm prototype work.
  - Mitigation: only add RPCs needed by the current milestone.
- **Commercial feature temptation:** Hiro/Satori may look convenient but change the cost profile.
  - Mitigation: defer until pricing and license review.

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
