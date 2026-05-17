# SECOND SPAWN Roadmap

Status: Pre-alpha, vertical slice foundation in development.

This roadmap tracks implementation status. Detailed design remains in `docs/`,
especially `docs/design/02-vertical-slice-spec.md` and
`docs/design/03-systems-index.md`.

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

## Current Review Gate

- [x] Merge PR #5: model-backed agent decisions and brain phase logging.
- [x] Review the profile bootstrap and agent activity branch.
- [x] Merge the profile bootstrap and agent activity branch into `dev` after
  backend tests, Unity compile, and reviewer verification.
- [x] Merge BodyTime event flow into `dev`.
- [x] Defer cultivation/Nibirium runtime progression from the current vertical
  slice.
- [x] Merge reincarnation placeholder flow into `dev`.

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
- [ ] Add Nakama channel-based basic chat for the vertical slice.
- [ ] Surface agent runtime stats and recent activity in an in-editor or
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
- [ ] Surface BodyTime, lifecycle, SECOND balance, reincarnation count, and
  debug reincarnation controls in the Unity prototype.
- [ ] Design server-authoritative PvP or contested-zone loot rules where
  BodyTime and SECOND can be taken from other users after validated combat or
  zone events. Clients and LLMs must never self-report this loot.
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
