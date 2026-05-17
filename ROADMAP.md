# SECOND SPAWN Roadmap

Status: Pre-alpha, vertical slice foundation in development.
Last updated: 2026-05-17.

This roadmap tracks implementation status. Detailed design remains in `docs/`,
especially `docs/design/02-vertical-slice-spec.md` and
`docs/design/03-systems-index.md`.

## Tracking Model

- `ROADMAP.md` tracks public milestone status and should stay readable without
  GitHub access.
- `CHANGELOG.md` tracks what changed after code, docs, or prototype merges.
- `docs/design/12-game-design-document.md` tracks the living GDD and current
  design truth.
- GitHub Projects should track daily execution through issues, PRs, priorities,
  owners, milestone views, and review state.
- Primary execution project: [DOS Project #5](https://github.com/orgs/DOS/projects/5).
- GitHub issues should become the unit of work for roadmap items once they are
  ready to implement.
- Pull requests should link issues and move project status through automation
  where possible.

Recommended GitHub Project fields:

| Field | Values |
| ---- | ---- |
| Status | Inbox, Ready, In Progress, In Review, Blocked, Done |
| Area | Unity, Nakama, Gateway, AI Agent, Design, Docs, DevOps, Economy, Combat, UX |
| Milestone | Foundation, Vertical Slice, Alpha, Beta, Post-Launch |
| Priority | P0, P1, P2, P3 |
| Size | XS, S, M, L, XL |
| Review Gate | Not Ready, Local Review, Gemini, Codex, Approved, Waived |
| Verification | Not Run, Docs Lint, Backend Tests, Unity Smoke, Playtest |

Recommended views:

- Vertical Slice Board: group by Status and filter Milestone = Vertical Slice.
- Engineering Table: sort by Priority, Area, and Size.
- Review Queue: filter Status = In Review or Review Gate not Approved.
- Roadmap: timeline or roadmap view grouped by Milestone.
- Risks and Blockers: filter Status = Blocked or Priority = P0.

## Completed Foundation

- [x] Public repo structure with Unity in `Unity/`, backend modules in
  `backend/`, and public docs in `docs/`.
- [x] Unity 6.5 beta URP project scaffold with `_SecondSpawn` asset namespace,
  assembly definitions, Git LFS conventions, and Force Text serialization.
- [x] Photon Fusion 2 imported and upgraded to the current project baseline.
- [x] Basic networked player spawn path with Fusion runner setup, player
  prefab, input provider, top-down camera, and visual animation bridge.
- [x] Simple KCC controller spike for prototype movement.
- [x] Prototype hub scene `ZoneTest_Hub` with player spawn, ground setup, and
  local test objects.
- [x] Nakama OSS local backend base with TypeScript runtime modules, custom
  Supabase-auth bridge, profile bootstrap, soul update, memory write, and
  bounded agent decision RPC.
- [x] Go LLM gateway scaffold with health/readiness, character context, memory,
  soul update, NPC chat, voice-session contract, and agent decision routes.
- [x] Agent context contract covering player profile, body state, soul,
  traits, level/stats, BodyTime, agent policy, and compact memory.
- [x] Server-owned prototype body archetype pool for random NPC-like player
  bodies, story hooks, stat bias, weapon visuals, visual variants, and animation
  capability flags.
- [x] Prototype Unity gateway client with Nakama fallback, profile/memory sync,
  NPC chat, local voice cue, and speech bubble.
- [x] Local player agent prototype toggle for bounded movement intent.
- [x] Local NPC agent brain prototype with patrol, speech, and debug-friendly
  phase tracing.
- [x] Model-backed JSON intent path for `/v1/agent/decide` with Anthropic
  provider wiring and deterministic fallback when no provider key is present.
- [x] Cloud Run deployment notes for the prototype gateway.
- [x] Project docs and ADRs for Fusion, Unity 6.5 beta, Nakama OSS, LLM safety,
  AI offline control, agent workflow, and backend boundaries.
- [x] Backend tests for gateway contracts and Nakama runtime behavior.
- [x] Unity project baseline upgraded to Unity `6000.5.0b8`.
- [x] Cloud Run staging gateway smoke-tested with the current Unity player
  context payload.
- [x] Local Nakama runtime smoke-tested with the current TypeScript module.
- [x] PR review fallback policy documented for local `code-review`, Gemini, and
  Codex Cloud review availability.

## Current Playable Prototype Snapshot

- [x] `ZoneTest_Hub` enters Play Mode with a Fusion-spawned local player.
- [x] The spawned player has networked level, combat stats, BodyTime, lifecycle,
  SECOND balance, reincarnation count, visual key, and agent-control flag.
- [x] Unity prototype HUD shows level, HP, energy, attack, defense, agility,
  BodyTime, lifecycle, SECOND balance, and reincarnation count.
- [x] Unity prototype HUD shows synced agent runtime counters and recent
  activity rows from the Nakama body profile.
- [x] Unity `CharacterMemorySync` pulls the Nakama player profile and applies
  current-body stats, BodyTime, lifecycle, SECOND balance, reincarnation count,
  and visual key onto the authoritative local `NetworkPlayer`.
- [x] New player profile bootstrap creates a current body with level 1 stats:
  level 1 body stats selected from the server-owned prototype archetype pool.
- [x] Current-body profile sync reloads the Unity visual variant, applies the
  weapon visual, and disables jump animation triggers for body models marked as
  missing jump support.
- [x] Prototype account reserve starts with 604800 SECOND seconds, equal to
  7 days, and reincarnation currently costs 432000 SECOND seconds, equal to
  5 days.
- [x] BodyTime earn, spend, drain, zero-time death, and reincarnation debug
  controls exist in Play Mode for smoke testing.
- [x] Actor profile registry exists for NPC-like actors, including body, stats,
  traits, soul, memory, policy, runtime, and activity state.
- [x] Permanent prototype NPC Frames can be seeded and listed from Nakama as
  server-owned public actor profiles with durable memory and activity state.
- [x] Prototype NPC-to-NPC interaction ticks can record deterministic dialogue,
  relationship memory, and activity logs for two permanent NPC actors.
- [x] Permanent NPCs expose LLM decision context and accept validated `say`
  intents with backend relationship and distance rules.
- [x] `ZoneTest_Hub` spawns 10 visible permanent NPC prototype markers from the
  Nakama server-owned NPC Frame list on Play Mode start.
- [x] Nakama has prototype OpenClaw bridge RPCs for Frame binding, structured
  context read, pending intent submission, and heartbeat/audit updates.
- [x] Unity Play Mode has an OpenClaw bridge debug panel for binding, context
  read, pending `say` intent, and heartbeat smoke testing.
- [x] Nakama has bounded prototype hub chat RPCs, and Unity Play Mode has a
  debug panel for sending and refreshing hub chat messages.
- [x] Nakama has a server-owned prototype reward claim path for allowlisted
  enemy or objective rewards that grant BodyTime without client-supplied
  amounts.
- [x] `_AgentNPC_Prototype` can bind to an actor profile, patrol, speak, use the
  model-backed gateway decision path, and recover through Nakama deterministic
  fallback when the gateway is unavailable or rate-limited.
- [ ] Real combat damage, enemy rewards, loot drops, quest progress, and player
  time-loot from other users are not implemented yet.

## Current Review Gate

- [x] Merge PR #5: model-backed agent decisions and brain phase logging.
- [x] Review the profile bootstrap and agent activity branch.
- [x] Merge the profile bootstrap and agent activity branch into `dev` after
  backend tests, Unity compile, and reviewer verification.
- [x] Merge BodyTime event flow into `dev`.
- [x] Defer cultivation/Nibirium runtime progression from the current vertical
  slice.
- [x] Merge reincarnation placeholder flow into `dev`.
- [x] Merge Unity `6000.5.0b8` upgrade and backend smoke fixes into `dev`.
- [x] Merge PR review fallback policy into `dev`.

## Vertical Slice - Current Milestone

Goal: one playable zone, one class, one dungeon, BodyTime MVP, reincarnation
MVP, and a visible offline-agent prototype.

### Next Implementation Work

- [ ] Import paid character/controller assets one package per commit:
  Opsive Ultimate Character Controller, Behavior Designer, then Convai.
- [ ] Decide whether Simple KCC remains the MVP controller or becomes a
  temporary spike after Opsive evaluation.
- [ ] Replace prototype cube visuals with one approved Hunter-style player
  visual using the existing visual prefab catalog.
- [ ] Add a proper hub NPC prefab using the prototype NPC brain contract.
- [x] Add server-owned permanent NPC seed/list and prototype interaction RPCs.
- [x] Add a Unity Play Mode debug panel for permanent NPC seed/list and
  NPC-to-NPC interaction smoke testing.
- [x] Add a Unity Play Mode spawner that makes 10 permanent NPC Frames visible
  in the hub while proper prefabs and Fusion server spawning are pending.
- [x] Add the LLM-driven NPC context and intent boundary so NPC brains choose
  actions while Nakama validates range, hostility, affinity, and intent shape.
- [x] Add Nakama channel-based basic chat for the vertical slice.
- [x] Surface agent runtime stats and recent activity in an in-editor or
  prototype debug UI.
- [ ] Add route-level gateway authentication before non-local AI or voice
  playtests.
- [ ] Add per-player LLM rate limit and token-budget enforcement.
- [ ] Persist gateway-side prototype context or remove in-memory fallback once
  Nakama is the only source of durable game profile truth.
- [ ] Wire Convai phase 1 NPC dialogue through the server-side intent boundary.
- [x] Add BodyTime meter MVP with one earn source and one spend sink.
- [x] Add reincarnation placeholder flow: death -> SECOND token check ->
  respawn with current-body reset.
- [x] Surface BodyTime, lifecycle, SECOND balance, reincarnation count, and
  debug reincarnation controls in the Unity prototype.
- [ ] Design server-authoritative PvP or contested-zone loot rules where
  BodyTime and SECOND can be taken from other users after validated combat or
  zone events. Clients and LLMs must never self-report this loot.
- [x] Implement the first server-owned prototype reward path that can grant
  BodyTime from an allowlisted enemy or objective reward catalog.
- [ ] Wire the prototype reward path to Fusion-validated enemy kill or objective
  completion events instead of a debug claim RPC.
- [ ] Implement the first server-authoritative contested loot rule for taking
  BodyTime or SECOND from another user after a validated PvP or zone event.
- [ ] Add one dungeon instance with one boss and grounded dialogue.
- [ ] Add one Hunter NFT skin equip placeholder with DOS Chain escrow design
  still server-authoritative.
- [ ] Run Multiplayer Play Mode smoke for 2-4 local clients.
- [ ] Resolve Unity Fusion CodeGen Play Mode smoke blocker tracked in issue #7.
- [ ] Prepare Linux headless dedicated server build path.

## Alpha

- [ ] Move production AI/LLM traffic to the shared `api.dos.ai` gateway path.
- [ ] Add RAG memory for NPCs with Supabase pgvector or Qdrant.
- [ ] Expand quest system beyond the first vertical slice questline.
- [ ] Add multiple zones with travel.
- [ ] Add marketplace and NFT trade.
- [ ] Add guild system before PvP.
- [ ] Add voice NPC if ephemeral-token cost and reliability are acceptable.

## Beta

- [ ] Guild PvP up to 50v50.
- [ ] Pet breeding system.
- [ ] Movement-only mount system.
- [ ] Economy balancing.
- [ ] Live ops infrastructure.
- [ ] Public beta launch.

## Post-Launch

- [ ] Seasons and content updates.
- [ ] Modding support.
- [ ] Sentis on-device AI for client perception.
- [ ] Redesign advanced body or soul progression after the vertical slice.

## Deliberately Out of Scope

- Full open-world seamless MMO.
- Voxel or procedural world generation.
- Console ports in phase 1.
- VR support.
