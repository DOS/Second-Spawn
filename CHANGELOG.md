# SECOND SPAWN Changelog

All notable project changes are summarized here. The project has not cut a
versioned release tag yet, so entries are organized as pre-alpha snapshots.

## Unreleased

### Added

- Character-model taxonomy covering core stats, derived stats, social
  attributes, body presentation, identity fields, and multi-axis relationships
  for NPC-like Frames and player-inhabitable bodies.
- Character stat and relationship system GDD covering the eight-stat MVP backend
  contract, deferred stat candidates, secondary stat direction, presentation
  attributes, relationship axes, and reincarnation carryover boundaries.
- Human-believable NPC agent design doc covering trait axes, relationship
  ledger, memory tiers, needs, mood, stress, proactive communication, and
  research anchors for LLM-driven NPC behavior.
- Unity `NetworkPlayer` now carries prototype level, combat stats, BodyTime,
  lifecycle, SECOND balance, reincarnation count, visual key, and agent-control
  state as networked fields.
- Prototype HUD now shows level, HP, energy, attack, defense, dexterity,
  BodyTime, lifecycle, SECOND balance, and reincarnation count.
- Unity `CharacterMemorySync` now applies Nakama profile body state onto the
  authoritative local player after profile load.
- Prototype BodyTime and reincarnation debug panel for exercising earn, spend,
  drain, zero-time death, and reincarnation from Play Mode.
- Actor profile registry for NPC-like actors, including body, stats, traits,
  soul, memory, policy, runtime, and activity state.
- Prototype NPC brain phase tracing for `Sense -> Decide -> Validate -> Act ->
  Reflect -> Cooldown`, with a serialized toggle for local debugging.
- Model-backed JSON intent path for `secondspawn_agent_decide`, implemented in
  the Nakama runtime and routed to `api.dos.ai`.
- Deterministic fallback when no `DOS_AI_API_KEY` exists, the model call fails,
  or the model returns invalid intent.
- Tests for Nakama model decision routing, fallback behavior, request shape,
  and intent validation.
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
- Nakama profile contracts now carry prototype Frame context fields
  for identity, policy-aware intent schema, runtime heartbeat, and debug
  visibility. Skill and agent-playbook fields remain prototype placeholders,
  not required MVP layers.
- OpenClaw design now treats external agent files as external-owned reasoning
  state; the game exposes only structured Frame context, control binding,
  policy, intent schema, and heartbeat/audit state.
- Current visible progression baseline is level and body-bound stats. Advanced
  body progression, cultivation tiers, and Nibiru-derived XP remain deferred.
- Nakama runtime config now supports `DOS_AI_API_KEY`, `DOS_AI_BASE_URL`, and
  `AGENT_DECISION_MODEL`.
- Agent design docs now mark brain phase tracing and model-backed JSON intent as
  implemented prototype foundation.
- Unity prototype brain now reads model or fallback source from Nakama,
  surfaces degraded fallback in nameplates, and escalates only repeated Nakama
  decision failures to errors.
- Unity Nakama auth now bootstraps the player profile immediately and records a
  profile activity event after successful authentication.
- Nakama deterministic decision RPC now records runtime decision counters before
  returning prototype fallback intent.
- Unity Nakama client now has a BodyTime event wrapper and exposes body
  lifecycle state in the shared profile DTO.
- Unity Nakama client now exposes SECOND balance, reincarnation count, and a
  Nakama reincarnation wrapper for prototype UI and playtest flows.
- Nakama player profile schema accepts `second_balance_seconds` and
  `reincarnation_count` in Unity-originated agent context payloads.
- Nakama storage writes now use create/update version handling compatible with
  refreshed local runtime state.

### Fixed

- Nakama `secondspawn_agent_decide` now accepts forward-compatible Unity body
  context fields for prompt-safe decisions.
- Nakama Frame identity now includes age band, age years, and home base so
  model-backed NPC decisions receive the same public identity summary Unity
  shows over permanent NPCs.
- Nakama now calls `api.dos.ai` via `DOS_AI_API_KEY` for model-backed agent
  decisions, replacing the separate Second Spawn Go adapter.
- Model-backed agent decisions now receive explicit proactive-social guidance:
  `AgentPolicy` decides when to initiate, SOUL shapes motive and voice, MEMORY
  shapes relationship context, and nearby actor data grounds who is present.
- Unity prototype NPC brains now include nearby Frame actors in the decision
  world snapshot, giving the model concrete social context when `say` is
  allowed.
- Unity prototype NPC brains now persist model-selected `say` intents through
  Nakama so NPC social actions become memory and relationship records instead
  of only local speech bubbles.
- Docs now align the GDD, NPC brain architecture, character profile design, and
  roadmap with the proactive NPC social path merged in PR #67.
- Permanent NPC nameplates now show the active brain source in Play Mode:
  `AI DOS.AI` for model-backed decisions, orange `AI FALLBACK` for degraded
  Nakama fallback, and red `AI ERROR` when no valid decision is available.
- Permanent NPC nameplates now cache camera, text, color, and layout updates,
  and the 10-NPC swarm disables phase-trace console logs by default to reduce
  Play Mode frame hitches.
- Failed NPC intent persistence now backs off before retrying and shortens
  warning text, reducing repeated console and network spam when Nakama rejects
  a model-selected social intent.
- Prototype NPC visuals now replace legacy animation controllers unless they
  expose the expected locomotion parameters, reducing imported asset controller
  warnings during Play Mode spawn.
- Generated NPC visual prefabs now serialize the shared prototype animator
  controller, and the visual rebuild tool preserves that rule for future imports.
- Prototype HUD now shows a lightweight FPS counter in the top-right corner with
  green, yellow, and red performance bands.
- Permanent NPC brain labels now include source reasons such as
  `dos_ai_http_502` or `dos_ai_unconfigured`, making degraded model decisions
  visible in Play Mode.
- Nakama agent decisions now default to the `dos-ai` model on `api.dos.ai`;
  Claude aliases are not the default path for the prototype NPC brain.
- Nakama character stats now use the eight canonical MVP core stats:
  `strength`, `dexterity`, `endurance`, `perception`, `focus`, `presence`,
  `intelligence`, and `luck`. Legacy `force`, `agility`, `vitality`, and
  `resilience` fields remain as compatibility aliases for the current Unity
  prototype.
- Removed the in-repo Second Spawn Go LLM adapter. Durable profile, soul,
  stats, memory, BodyTime, activity state, and model-backed intent validation
  now stay on the Nakama side of the backend boundary.
- Prototype debug panel hotkeys now use Unity Input System keyboard polling
  instead of the disabled legacy input API, stopping Play Mode console spam.
- Prototype Nakama world storage now scopes permanent NPC profiles and hub chat
  to the authenticated local session owner, allowing local seed/list RPCs to run.
- Permanent NPC world profile storage now uses a separate key prefix so it does
  not overwrite player source-body actor profiles with the same actor ID.
- Prototype debug hotkeys now tolerate legacy serialized `KeyCode` values still
  present in open Unity scene instances, preventing `Key` range exceptions.
- Prototype NPC markers now display their index, name, short actor ID, level,
  and role so dummy capsules can be matched back to Nakama permanent NPCs.
- Permanent NPC markers can now attach prototype agent brains from their own
  actor profiles, with staggered decision starts and fixed per-body visuals.
- The legacy single prototype guide in `ZoneTest_Hub` no longer auto-starts,
  so the scene focuses on the 10 persistent NPC agents.
- Visual body catalog now includes the 4 Fighter Pack variants in addition to
  the RPG Character and 12 Warrior variants, for 17 source body variants total.
- Visual body catalog now also includes the Crafter body as variant 17,
  bringing the local source body variant count to 18.
- Permanent NPC profiles now carry per-NPC identity, profession, apparent age,
  home base, stats, traits, soul, story, and seed memory instead of only
  archetype-level defaults.
- Prototype overhead NPC labels were reduced to compact two-line name and role
  plates so 10 spawned NPCs remain readable in Play Mode.
- Prototype overhead NPC labels now billboard toward the camera and hide past
  a configurable distance, while the persistent NPC debug panel can inspect
  each seeded NPC's profession, age, home base, stats, visual variant, soul, and
  first seed memory.
- Generated character prefabs now resolve from `_SecondSpawn/Prefabs` and
  generated materials from `_SecondSpawn/Materials`, while vendor asset packs
  remain immutable under `Assets/ExplosiveLLC`.
- Clean generated visual prefabs and URP materials now exist for variants
  13-17: Berserker, Female, Heavy, Male, and Crafter.
- Prototype NPC labels now render as camera-fixed screen-space nameplates, and
  the prototype HUD/debug text is larger for QHD Play Mode readability.
- Permanent NPC agent visuals now load only after the server profile has picked
  the fixed body variant, avoiding duplicate startup prefab/animator warmup.
- Prototype agent brain now avoids duplicate fallback HTTP calls; Nakama returns
  either a model-backed decision or a deterministic fallback in one RPC.

### Verification

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
- Nakama LLM rate limiting and token budget enforcement are tracked in issue #6.
- Real voice still waits for an ephemeral-token provider flow.

## Pre-Alpha Foundation Snapshot - 2026-05-16

### Added

- Unity 6.5 beta URP project under `Unity/` with `_SecondSpawn` asset namespace,
  assembly definitions, Git LFS conventions, and Force Text serialization.
- Photon Fusion 2 project baseline with network runner setup, player spawn,
  network input, top-down camera, animation bridge, and visual prefab catalog.
- Simple KCC prototype movement spike.
- Prototype hub scene `ZoneTest_Hub`.
- Nakama OSS backend base with TypeScript runtime modules, local config, custom
  Supabase-auth bridge, profile bootstrap, soul update, memory write, and agent
  decision RPC.
- Agent context, soul, character trait, BodyTime, level/stats, policy, and memory
  contracts.
- Unity Nakama client with local memory seeding, NPC chat, speech bubble, and
  prototype voice cue.
- Local player agent prototype and local NPC agent brain prototype.
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
- Scoped backend CI to the Nakama runtime build and test path.
- Relaxed and scoped markdown lint to match the public docs structure.
- Added a Fusion runtime access workaround for the Unity 6.5 beta compatibility
  path.

### Known Issues

- No shipped gameplay build yet.
- Paid asset imports are still pending.
- LLM rate limiting and token budgets are not production-ready.
- Convai and real voice integration are still future work.
- Dedicated server build path is not ready yet.

### Metrics

- No release tags exist yet.
- Main foundation merge: `154ac15`.
- Current merged model-decision branch commit: `998637a`.
