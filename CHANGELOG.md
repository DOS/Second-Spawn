# SECOND SPAWN Changelog

All notable project changes are summarized here. The project has not cut a
versioned release tag yet, so entries are organized as pre-alpha snapshots.

## Unreleased

### Added

- Prototype NPC brain phase tracing for `Sense -> Decide -> Validate -> Act ->
  Reflect -> Cooldown`, with a serialized toggle for local debugging.
- Model-backed JSON intent path for `/v1/agent/decide`.
- Anthropic Messages API provider wiring for gateway-side agent decisions.
- Deterministic fallback when no provider key exists, the provider call fails,
  or the model returns invalid intent.
- Tests for model decision parsing, fallback behavior, Anthropic request shape,
  and endpoint decider injection.
- Fallback observability for model-backed decisions, including decision source
  metadata and structured warning logs.
- Nakama agent runtime counters for profile bootstrap, fallback decisions,
  action intent counts, activity count, and offline-agent seconds.
- Bounded Nakama `agent_activity` log with `secondspawn_agent_activity_add`.

### Changed

- Gateway config now supports `AGENT_DECISION_MODEL`.
- Gateway docs and Cloud Run env examples now describe the model-backed decision
  path.
- Agent design docs now mark brain phase tracing and model-backed JSON intent as
  implemented prototype foundation.
- Unity prototype brain now warns on gateway decision failures and escalates
  repeated failures to errors.
- Unity Nakama auth now bootstraps the player profile immediately and records a
  profile activity event after successful authentication.
- Nakama deterministic decision RPC now records runtime decision counters before
  returning prototype fallback intent.

### Verification

- `go test ./...` in `backend/gateway`.
- `npm test` in `backend/nakama`.
- Unity MCP refresh and Play Mode smoke for NPC brain phase traces.

### Known Issues

- PR #5 has merged into `dev`; the next review gate is the profile bootstrap
  and agent activity branch.
- Gateway route-level JWT enforcement is not complete.
- LLM rate limiting and token budget enforcement are tracked in issue #6.
- Real voice still waits for an ephemeral-token provider flow.

## Pre-Alpha Foundation Snapshot - 2026-05-16

### Added

- Unity 6.5 beta URP project under `Unity/` with `_SecondSpawn` asset namespace,
  assembly definitions, Git LFS conventions, and Force Text serialization.
- Photon Fusion 2 project baseline with network runner setup, player spawn,
  network input, top-down camera, animation bridge, and visual prefab catalog.
- Simple KCC prototype movement spike.
- Prototype hub scene `ZoneTest_Hub`.
- Go LLM gateway scaffold with health/readiness, character context, memory,
  soul update, NPC chat, voice-session contract, and agent decision routes.
- Nakama OSS backend base with TypeScript runtime modules, local config, custom
  Supabase-auth bridge, profile bootstrap, soul update, memory write, and agent
  decision RPC.
- Agent context, soul, character trait, BodyTime, cultivation, policy, and memory
  contracts.
- Unity gateway client with Nakama fallback, local memory seeding, NPC chat,
  speech bubble, and prototype voice cue.
- Local player agent prototype and local NPC agent brain prototype.
- Cloud Run deployment notes for the prototype gateway.
- Public GitBook docs, architecture docs, setup docs, and ADRs through ADR 0010.

### Changed

- Game backend direction moved from Supabase-first to Nakama OSS as the primary
  game backend, with Supabase kept as an identity and sidecar option.
- Development workflow moved to `dev` plus feature branches, with review gates
  before merging to stable `main`.
- Unity MCP workflow standardized around Codex as the primary daily operator and
  Claude as reviewer/escalation path.

### Fixed

- Tightened prototype agent authority and parser behavior after review.
- Scoped backend CI so the gateway can build without a committed `go.sum`.
- Relaxed and scoped markdown lint to match the public docs structure.
- Added a Fusion runtime access workaround for the Unity 6.5 beta compatibility
  path.

### Known Issues

- No shipped gameplay build yet.
- Paid asset imports are still pending.
- Route-level gateway auth, rate limiting, and durable gateway storage are not
  production-ready.
- Convai and real voice integration are still future work.
- Dedicated server build path is not ready yet.

### Metrics

- No release tags exist yet.
- Main foundation merge: `154ac15`.
- Current merged model-decision branch commit: `998637a`.
