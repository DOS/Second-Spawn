# SECOND SPAWN Changelog

All notable project changes are summarized here. The project has not cut a
versioned release tag yet, so entries are organized as pre-alpha snapshots.

## Unreleased

### Added

- Unity `NetworkPlayer` now carries prototype level, combat stats, BodyTime,
  lifecycle, SECOND balance, reincarnation count, visual key, and agent-control
  state as networked fields.
- Prototype HUD now shows level, HP, energy, attack, defense, agility,
  BodyTime, lifecycle, SECOND balance, and reincarnation count.
- Unity `CharacterMemorySync` now applies Nakama profile body state onto the
  authoritative local player after profile load.
- Prototype BodyTime and reincarnation debug panel for exercising earn, spend,
  drain, zero-time death, and reincarnation from Play Mode.
- Actor profile registry for NPC-like actors, including body, stats, traits,
  soul, memory, policy, runtime, and activity state.
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
- Nakama `secondspawn_bodytime_event` RPC for prototype BodyTime earn, spend,
  and danger-zone drain events with source caps, retry idempotency, earn
  cooldown, activity logging, and zero-time body death.
- Nakama `secondspawn_reincarnate` RPC for a prototype zero-time death to fresh
  body flow using a 5-day SECOND cost against a 7-day starting test balance.
- Unity `6000.5.0b8` project baseline.
- PR review fallback policy for local `code-review`, Gemini, and Codex Cloud
  review availability.
- Roadmap tracking model for using GitHub Projects as the issue, PR, review,
  and milestone execution layer.
- GitHub Project #5 tracking guide with source-of-truth split, recommended
  fields, views, seed issues, and CLI project-scope note.
- Server-owned prototype body archetype pool for new player bodies and NPC-like
  actor profiles, including story hooks, stat bias, traits, soul defaults,
  equipment visuals, visual variants, and animation capability flags.
- Nakama now persists the source NPC-like Frame actor profile when a player
  first receives a body or reincarnates into a new body.
- Nakama now has a prototype permanent NPC Frame pool. New player bodies and
  reincarnated bodies are assigned from this server-owned pool instead of
  generating only ad-hoc source actor IDs.
- Nakama now exposes the first OpenClaw bridge RPCs for binding an external
  agent to a Frame actor, reading structured Frame context, submitting
  server-validated intent requests, and updating connection heartbeat state.
- Unity now has prototype OpenClaw bridge client wrappers and a Play Mode debug
  panel for binding an external agent to a Frame, reading context, submitting a
  pending `say` intent, and updating heartbeat state.
- Nakama now exposes bounded prototype hub chat RPCs, and Unity has a Play Mode
  debug panel for sending and refreshing hub chat messages.
- Nakama now exposes a server-owned prototype reward claim RPC for allowlisted
  enemy or objective rewards that grant BodyTime without trusting a
  client-supplied amount.
- Unity Body Lifecycle Debug now has a prototype reward claim control for
  exercising server-owned BodyTime rewards in Play Mode.
- Nakama now exposes server-owned permanent NPC seed/list RPCs and a prototype
  NPC-to-NPC interaction RPC that records dialogue, activity, and relationship
  memory on both actors.
- Nakama now exposes the LLM-driven NPC context and intent boundary:
  `secondspawn_npc_context_get` returns context plus interaction rules, and
  `secondspawn_npc_intent_submit` validates bounded `say` intents against
  distance, hostility, affinity, and repeated-interaction rules.
- Unity now has a Persistent NPC Debug panel for seeding, listing, and
  smoke-testing LLM-style NPC context and say-intent submission between
  permanent NPC Frames.
- Unity now spawns 10 visible permanent NPC prototype markers in `ZoneTest_Hub`
  by loading the server-owned Nakama NPC Frame list on Play Mode start.
- Prototype debug panels now default to hidden hotkey toggles so they no longer
  overlap the main HUD in Play Mode.

### Changed

- GDD and system docs now anchor SECOND SPAWN to the MetaDOS technology stack:
  AMB cocoons, bio-synthetic Frames, Hunter Frames, TIME as the life medium,
  SECOND as the unit/currency, manga/manhwa system-story progression tone, and
  vertical-slice story hooks for the first hub, first Frame, NPC knowledge, and
  faction tension.
- Character profile docs now clarify where Frame professions, public roles,
  social relationships, reputation summaries, offline routines, and durable
  motivations belong across identity, skill, agent, memory, and soul layers.
- Character profile docs now define the ten `Frame*` agent-core layers with
  examples: user, identity, soul, body, memory, policy, heartbeat, tools, skill,
  and agents.
- Local Unity prototype can show a player with persisted profile stats after
  joining the hub scene.
- Unity profile sync now reloads the local visual model from the Nakama body
  profile, applies the server-selected weapon visual, and skips jump animation
  triggers for models marked as missing jump support.
- Unity visual intent playback now respects body melee and ranged animation
  capability flags for prototype attack and cast actions.
- Prototype HUD now includes a Frame Identity debug section for source actor,
  role, archetype, weapon stance, animation capabilities, soul, and story hook.
- Prototype HUD now includes agent runtime counters and recent activity rows
  from the synced Nakama body profile.
- Nakama and gateway profile contracts now carry prototype Frame context fields
  for identity, policy-aware intent schema, runtime heartbeat, and debug
  visibility. Skill and agent-playbook fields remain prototype placeholders,
  not required MVP layers.
- OpenClaw design now treats external agent files as external-owned reasoning
  state; the game exposes only structured Frame context, control binding,
  policy, intent schema, and heartbeat/audit state.
- Current visible progression baseline is level and body-bound stats. Advanced
  body progression, cultivation tiers, and Nibiru-derived XP remain deferred.
- Gateway config now supports `AGENT_DECISION_MODEL`.
- Gateway docs and Cloud Run env examples now describe the model-backed decision
  path.
- Agent design docs now mark brain phase tracing and model-backed JSON intent as
  implemented prototype foundation.
- Unity prototype brain now warns on gateway decision failures, recovers through
  Nakama deterministic decisions when available, and escalates only unrecovered
  repeated failures to errors.
- Unity Nakama auth now bootstraps the player profile immediately and records a
  profile activity event after successful authentication.
- Nakama deterministic decision RPC now records runtime decision counters before
  returning prototype fallback intent.
- Unity gateway client now has a Nakama BodyTime event wrapper and exposes body
  lifecycle state in the shared profile DTO.
- Unity gateway client now exposes SECOND balance, reincarnation count, and a
  Nakama reincarnation wrapper for prototype UI and playtest flows.
- Gateway player profile schema now accepts `second_balance_seconds` and
  `reincarnation_count` in Unity-originated agent context payloads.
- Nakama storage writes now use create/update version handling compatible with
  refreshed local runtime state.

### Fixed

- Prototype debug panel hotkeys now use Unity Input System keyboard polling
  instead of the disabled legacy input API, stopping Play Mode console spam.

### Verification

- `go test -count=1 ./...` and `go vet ./...` in `backend/gateway`.
- Cloud Run staging gateway `/readyz` and `/v1/agent/decide` smoke on revision
  `second-spawn-gateway-00008-cnn`.
- `npm run build` and `npm test` in `backend/nakama`.
- Local Nakama runtime `secondspawn_health` smoke with the current module.
- Unity Play Mode smoke for `ZoneTest_Hub`, including profile/stat sync,
  BodyTime UI, reincarnation debug flow, and NPC brain traces.
- Markdown lint and dash scan for docs updates.

### Known Issues

- Real combat damage, enemy rewards, loot drops, quest progress, and player
  time-loot from other users are not implemented yet.
- Prototype reward claims still use a debug RPC path. They are server-owned and
  capped, but not yet wired to Fusion-validated combat or objective completion.
- Deterministic permanent NPC interaction ticks are fallback smoke tests only.
  The intended NPC brain path is LLM context -> LLM-selected intent -> Nakama or
  Fusion validation -> activity and memory persistence.
- Unity UI is still prototype IMGUI, not production HUD.
- Supabase anonymous auth can be used when configured, but the local prototype
  still supports Nakama device fallback for development.
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
- Agent context, soul, character trait, BodyTime, level/stats, policy, and memory
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
