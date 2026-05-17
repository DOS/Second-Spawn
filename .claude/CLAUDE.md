<!-- SISTER FILE: ../AGENTS.md - keep in sync. Claude Code reads this (.claude/CLAUDE.md) auto on session start; Codex CLI / Cursor / Copilot read AGENTS.md at repo root. Both files MUST be identical except this header. See Hard Rule #8. -->

# SECOND SPAWN - Project Context

This file is the primary context for any AI coding agent working on this repository. Read fully before making changes.

## Game Overview

- **Name (codename, may rename pre-launch):** SECOND SPAWN
- **Genre:** Hybrid MMO + Top-down ARPG (NOT full open-world MMORPG)
- **References:** Diablo IV, Path of Exile 2, Lost Ark
- **Setting:** Near-future ~2050, post-apocalyptic, MetaDOS universe
- **Tone:** Dark sci-fi, cyberpunk, body/soul survival, AI NPC society

## Four Signature Features (DO NOT LOSE TRACK)

1. **AI Agent 24/7** - When the player is offline, an LLM-driven AI agent fully controls their character (farms, quests, socializes with NPCs and other players' agents). When the player returns, they take over control. This is a near-unique feature in MMO/ARPG space.
2. **Reincarnation with progression reset** - Death is permanent for the body. Consciousness transfers to a new body via SECOND token or special item. Current-body progression resets in a roguelike-MMO hybrid loop. Durable soul/profile carryover rules are deferred until the reincarnation design is sharper.
3. **Time-as-Currency** - Time is both the current body's survival resource and a spendable economy resource, adapted from MetaDOS and the `In Time` inspiration. Running out of body time triggers death/reincarnation; spending time creates hard tactical tradeoffs.
4. **Consciousness transfer to NPC/synthetic bodies** - Sci-fi explanation (mind upload, synthetic bodies, cloning, and body imprinting). NOT spiritual reincarnation.

## Actor and Body Model (CORE)

- A player is a durable consciousness / soul profile, not a blank avatar shell.
- On spawn, the player inhabits a current NPC-like synthetic body. That body may already have its own profile, constraints, stats, traits, memory hooks, soul imprint, BodyTime, lifecycle state, and agent runtime state.
- The game must support many NPC-like actors and many player-inhabited bodies using one broad actor-profile model. Ownership and authority decide whether an actor is a world NPC, a player current body, an offline player agent, or an OpenClaw-connected actor.
- Each important actor body should eventually resolve to a bundle: `BodyProfile`, `CharacterStats`, `CharacterTraits`, `SoulProfile`, `MemoryRecord`, `AgentPolicy` or NPC policy, `AgentRuntime`, and `AgentActivity`.
- Reincarnation destroys or retires the current body. The durable player consciousness transfers into a new body, with only explicitly designed layers carrying over.

## Advanced Body Progression (DEFERRED)

The previous cultivation / Nibirium XP concept is explicitly deferred and must
not be implemented in the current vertical slice. It felt too close to a
traditional XP bar. The slice uses level and character stats as the progression
baseline.

Future advanced body or soul progression needs a fresh design pass before any
implementation. Do not add cultivation tiers, Nibirium XP, tier-up rituals, or
Cultivation Master mechanics without a new approved design update.

## Gameplay Architecture

- Instance-based zones, ~20 players per zone
- Dungeon / instance separate
- Guild PvP up to 50v50
- Top-down ARPG action combat
- Time-as-currency economy: body time can be earned, spent, transferred later, and lost on body death unless explicit conversion rules say otherwise
- OpenClaw-connected NPCs: a user may connect their own OpenClaw agent into the game as an NPC-like world actor, subject to identity, consent, moderation, rate limit, and server-side intent validation
- Pet system: NFT-based, 1 equip slot, NOT looted from bosses (marketplace + breeding only)
- Mount system: movement only, no mounted combat (reduce animation workload)

## Tech Stack (FINAL)

### Engine

- Unity 6.5 beta (currently `6000.5.0b8`) + URP. JOY explicitly chose beta over Unity 6.0 LTS for newest features; accept risk of breaking changes between beta builds and that some 3rd-party assets (Opsive UCC, Behavior Designer, Convai) may not be tested against this version yet. Re-evaluate if beta blocks progress.
- Force Text serialization (default Unity 6 - DO NOT change)

### Networking

- **Photon Fusion 2** (final, no debate)
  - Dev / iteration: Host Mode + Photon Cloud free 20 CCU
  - Production: Server Mode dedicated headless Unity build on VPS
  - Reference: Fusion BR200 sample for interest management + 200 player / 60Hz

### Existing Unity Assets (already purchased)

- Opsive Ultimate Character Controller (ARPG character + combat)
- Behavior Designer (NPC combat behavior tree, NOT dialogue AI)
- Convai (NPC dialogue - phase 1 only, migrate to custom LLM phase 2)
- TextMeshPro (UI)
- Unity ML-Agents (deferred, future research)

### Backend

- **Game backend:** Nakama OSS (ADR 0010). This is the default backend foundation for game APIs, social primitives, storage objects, activity logs, and future groups / leaderboards / matchmaking.
- **Nakama deployment mode:** self-hosted OSS for prototype and early development. Heroic Cloud is a future managed upgrade path only, not the current default.
- **Backend boundary:** Nakama is the game backend. `api.dos.ai` / Go LLM Gateway is the shared DOS.AI AI/LLM gateway only. Photon Fusion 2 dedicated server remains authoritative for in-zone movement, combat, physics, and tick simulation.
- **Custom game backend rule:** Do not create a separate game API gateway unless a Nakama runtime module cannot reasonably handle the feature. Default to Nakama server runtime modules (TypeScript / Go / Lua) for game backend extensions: auth hooks, RPCs, inventory, profile, stats, social, matchmaking, leaderboards, activity logs, and moderation. Initial Nakama modules use exact TypeScript 6.0.3 and emit Nakama-compatible JavaScript.
- **Supabase:** compatible sidecar for DOS.Me-style identity bridge, wallet/profile integration, storage, analytics, or external product data when useful. Supabase is no longer the primary game backend baseline.
- **Hiro / Satori:** Commercial / license-dependent candidates only. Do not assume they are open-source drop-in dependencies.
- **Postgres** (durable Nakama database; local container for development or approved Supabase Postgres project if isolation and connection behavior are verified)
- **Go LLM Gateway / `api.dos.ai`** (shared AI service; provider keys, model routing, prompt safety, voice token minting, AI-specific endpoints only)
- **Redis** (session, rate limit, transient cache)

### LLM

**Phase 1 (MVP):**

- Convai SDK in Unity for NPC dialogue
- Limit: no full custom LLM control, accept Convai cost

**Phase 2 (post-MVP):**

- Migrate LLM calls to `api.dos.ai` / Go LLM Gateway, models:
  - Haiku 4.5 for NPC chat (fast, cheap)
  - Sonnet 4.6 for boss / quest-critical dialog
- RAG memory: Supabase pgvector or Qdrant
- Voice: OpenAI Realtime API via ephemeral token (NOT API key in client) OR ElevenLabs
- Client AI: Unity Sentis for small perception (optional, phase 3)

### OpenClaw-Connected NPCs (CONCEPT)

- Each OpenClaw agent can become a user-owned NPC-like actor in SECOND SPAWN.
- A player may connect their OpenClaw agent to the game so it can appear as a companion, hub NPC, merchant-like persona, quest-adjacent character, or social world citizen.
- OpenClaw agents must never mutate authoritative game state directly. They emit dialogue or structured intent only.
- Nakama owns game identity, permissions, rate limits, activity logs, and moderation state for connected agents.
- Fusion server validates any in-world action intent before movement, interaction, combat, inventory, currency, quest, or BodyTime state changes.
- `api.dos.ai` / Go LLM Gateway handles provider calls, prompt safety, memory retrieval, and context shaping for OpenClaw-connected NPC behavior.
- This is an ecosystem bridge, not a replacement for NPC dialogue, offline player agents, or the game backend.

### LLM Safety (CRITICAL)

- **Server-side intent validation ONLY** - never trust LLM output as authoritative
- **Never let LLM directly mutate game state** (no auto-grant item, gold, XP)
- Per-NPC memory budget cap
- Rate limit per player (LLM token + request count)
- Prompt injection defense (reuse DOSafe patterns)
- All LLM calls go through `api.dos.ai` / Go LLM Gateway, never direct from Unity client

### AI Agent for Offline Players (CORE FEATURE)

- LLM-driven autonomous agent controls player character when offline
- Agent operates within Fusion server tick (server-authoritative)
- Agent decision loop: pull state from Fusion -> reason via LLM gateway -> emit action intent -> server validates -> apply
- Anti-abuse: agent inherits player's rate limit + capability cap
- Agent persona: derived from player history, current body stats, and player profile
- Agent death = body death = reincarnation triggered (same as player death)

### NFT / Blockchain

- **Chain:** DOS Chain (DOS ecosystem chain)
- **MCP integration:** thirdweb-api MCP (already in JOY's tool stack)
- **Inherited from MetaDOS:**
  - Hunter skins - Option 1 (preset hero characters, may hybrid with Option 3 later)
  - Weapons
  - Pets (1 equip slot, marketplace + breeding only, no boss drop)
- **Wallet auth:** Sign-message pattern via thirdweb, Nakama auth bridge, or Supabase + DOS Chain sidecar
- **In-game lock:** Escrow contract when equipped, release on unequip
- **SECOND token:** Used for reincarnation cost (token economy needs design). Keep distinct from `BodyTime` unless a future ADR explicitly merges them.

### Version Control

- **Primary:** GitHub Enterprise (1 seat, JOY has)
- **Repo:** DOS/Second-Spawn (public, https://github.com/DOS/Second-Spawn)
- **LFS:** GitHub Enterprise LFS (250GB storage + 250GB bandwidth free, sufficient 12-18 months)
- **Mirror:** GitLab self-hosted on JOY's workstation (pull-mirror from GitHub every 15 min)
- **Cold backup:** Backblaze B2 nightly sync via rclone
- **Code license:** AGPL-3.0 (copyleft, prevents commercial fork without contributing back). Multiplayer game = network service = AGPL fully triggers on any fork.
- **Asset license:** CC-BY-NC 4.0 (no commercial use)
- **NFT assets:** Reserved by DOS.AI ecosystem, not under AGPL/CC-BY-NC
- **Migration trigger:** Move LFS to Cloudflare R2 + Worker when GitHub LFS hits 200GB storage or 200GB monthly bandwidth

### Asset Pipeline

- Synty / Quaternius stylized low-poly for environment + non-Hunter NPC
- Reuse MetaDOS Hunter skins as preset hero characters
- Unity Asset Store animation packs

### Deploy

- **Game server:** Linux headless Unity build on Hetzner VPS, Dockerized
- **Nakama backend:** self-hosted OSS first; Heroic Cloud only if operations become worth paying for
- **AI/LLM gateway:** `api.dos.ai` shared Go gateway
- **LLM API:** Convai phase 1, then Anthropic + OpenAI phase 2
- **Monitoring:** Sentry (error) + Grafana (metrics)

### Testing

- Unity Multiplayer Play Mode (in-editor multi-instance)
- Unity Test Framework (unit + integration)
- Bot load test (Fusion bots simulating 50-100 players per zone)

## AI Agent Tools

### Primary Workflow

- **Codex (this environment)** - default daily operator for code, docs, ADRs, backend, repo hygiene, Unity MCP inspection, and targeted Unity scene/script edits. Codex has more available capacity than Claude Code Max and is the normal first stop for SECOND SPAWN work.
- **CoplayDev MCP for Unity** (`com.coplaydev.unity-mcp`) - primary Unity Editor bridge for agents. Preferred transport is HTTP Local at `http://127.0.0.1:8080` from `Window > MCP For Unity`. Configure clients from the MCP For Unity Client Configuration tab. For Codex, select `Codex` and click `Configure`.
- **Claude Code Max / Claude Desktop Code mode** - specialty agent for high-value architecture critique, code review, brainstorming, and Unity-heavy tasks where Codex gets blocked. Use scarce Claude budget deliberately.
- **Unity official MCP / Unity AI Assistant** (`com.unity.ai.assistant`) - optional / legacy path only. Do not treat it as the primary workflow because the 2026-05-14 debug session found seat/cap/connection instability compared with CoplayDev MCP.

### Sustainable MCP Workflow (post 2026-05-14 debug)

1. Codex is primary for day-to-day implementation. Claude Code Max is an escalation/reviewer, not the always-on driver.
2. Use CoplayDev MCP for Unity over `localhost:8080` as the normal Unity bridge.
3. Only one agent may mutate Unity scenes, prefabs, package imports, or project settings at a time. Read-only inspection by the next agent is OK after the previous agent reports dirty files and current console state.
4. Every agent switch must leave a handoff using `docs/setup/agent-handoff.md`.
5. Unity package imports happen one package per commit, in this order unless JOY changes it: Opsive Ultimate Character Controller, Behavior Designer, Convai.
6. Before claiming Unity work is complete, check the Unity console and active scene through MCP or the Editor.
7. Significant commits still require independent reviewer pass per Hard Rule #7.

### Optional

- VS Code Claude extension (for code-heavy backend / gateway tasks, NOT Unity scene work)
- Rider or VS Code as C# code reader (not main IDE - Unity Editor is)

### MCP Servers in Use

- Coplay unity-mcp (Unity Editor bridge)
- thirdweb-api (DOS Chain wallet, NFT logic)
- Supabase MCP (sidecar database, auth, edge functions when used)
- Cloudflare MCP (future R2 migration)

## Reference Materials

### MetaDOS Source (READ-ONLY, location: `D:\Projects\MetaDOS`)

- Battle Royale 100 CCU template using Photon Fusion 2
- Extract patterns ONLY (do not copy gameplay code):
  - NetworkRunner initialization
  - `[Networked]` property patterns
  - Tick rate, lag-compensation config
  - Host migration logic
  - Networked player spawning
  - Input authority vs state authority pattern
  - Project structure conventions
  - CI / CD setup (if exists)
  - ADR / docs / lessons learned (if exists)
- **NEVER modify MetaDOS files. Read-only reference.**
- Realistic head-start: 2-4 weeks of Fusion boilerplate setup, NOT 6 months
- Gameplay code (BR-specific) is NOT reusable - different genre

### What BR Template Does NOT Solve (build from scratch)

- Persistence layer (DB save player / world state)
- Economy (inventory, currency, vendor, drop tables)
- World lifecycle (zone load / unload, NPC spawn, day / night, weather)
- Quest system
- LLM NPC dialogue + intent validation
- Guild / social (phase 2)
- Reincarnation mechanic
- AI agent for offline players
- Advanced body progression beyond level/stats is deferred until redesigned

### Recommended Reading List

- Photon Fusion BR200 sample documentation
- Photon Fusion Dedicated Server overview
- Opsive Ultimate Character Controller getting started
- Behavior Designer manual
- Convai Unity SDK docs
- Unity Multiplayer Play Mode tutorial
- Coplay unity-mcp + Claude Code setup guide
- DOSRouter Go gateway pattern (JOY's existing repo)
- DOS.Me Supabase auth pattern (JOY's existing repo, reference for identity bridge only)

## Project Conventions

### Naming

- Repo: `Second-Spawn` (matches GitHub repo name as-is)
- Repo root: `D:\Projects\Second-Spawn`
- Unity project subfolder: `D:\Projects\Second-Spawn\Unity` (PascalCase, NOT at repo root - multi-stack repo: Unity at `Unity/`, Nakama modules at `backend/nakama/`, docs at `docs/`)
- C# code: Microsoft conventions (PascalCase classes, camelCase fields with `_` prefix for private serialized)
- Branches: `feat/<short-desc>`, `fix/<short-desc>`, `chore/<short-desc>`
- **Unity-specific conventions** (folder structure, asmdef pattern, scene organization, naming rules): see [docs/setup/unity-conventions.md](docs/setup/unity-conventions.md). MUST follow before creating, renaming, or organizing any Unity asset, script, or folder.
- **Agent handoff conventions** (Codex-primary workflow, Claude escalation, MCP ownership, commit handoff): see [docs/setup/agent-handoff.md](docs/setup/agent-handoff.md). MUST follow before switching agents or handing off Unity work.

### Documentation Language

- All code, comments, docs, commits, PR titles, README, ROADMAP: **English**
- Communication with JOY (this user): Vietnamese (per global CLAUDE.md)
- `/docs` is published publicly to GitBook at `https://dos.gitbook.io/second-spawn/`. Keep docs public-safe, English-canonical, and readable by non-repo visitors.
- Vietnamese companion notes may live under `docs/vi/`, but English docs are the source of truth. If Vietnamese notes conflict with English canonical docs, English wins.
- No em-dashes anywhere - use `-` (hyphen) only

### Git Workflow

- Default branch: `main`
- Stable branch: `main` only receives reviewed, smoke-tested changes from `dev`
- Daily working branch: `dev`
- Feature work: create a separate branch/worktree from `dev`, then PR back into `dev`
- Public repo open source from day 1
- License: AGPL-3.0 (code) + CC-BY-NC 4.0 (assets)
- All PRs reviewed via Claude Code review skill before merge
- JOY is non-coder - AI agent must verify with reviewer before claiming "done"

### Open Source Targeting

- Brand DOS.AI portfolio
- Recruit contributors (long-term, manage expectation low)
- Community trust + transparency

### Security Note for Open Source

- Code is public, asset can be reverse-engineered
- ALL gameplay logic must be server-authoritative
- Anti-cheat assumes attacker has full source
- LLM calls server-side only, API keys never in client

## Vertical Slice Plan (First Milestone)

Target: playable demo in 3-6 months

Scope:

- 1 zone (small open area + 1 hub town)
- 1 character class (use one of MetaDOS Hunter skins)
- 1 dungeon instance
- 1 boss with LLM dialogue (Convai)
- 1 questline (3-5 quests)
- Reincarnation MVP (die -> SECOND token -> respawn with reset)
- Time-as-currency MVP (body time meter, earn/spend loop, zero time triggers reincarnation placeholder)
- AI agent control (simple: agent farms one designated area when player offline)
- NFT Hunter skin equip + escrow
- Multiplayer 4-20 players per zone
- Basic chat (Nakama channels first, Supabase sidecar only if useful)

OUT of scope for vertical slice:

- Guild / PvP
- Marketplace
- Pet breeding
- Multiple zones
- Advanced body progression
- Voice NPC
- Full quest system

## Hard Rules (project-specific, in addition to global CLAUDE.md)

1. **NEVER copy MetaDOS gameplay code.** Extract patterns only. Reference path: `D:\Projects\MetaDOS` (read-only).
2. **NEVER let LLM mutate authoritative game state.** Server validates all intent.
3. **NEVER put API keys (Anthropic, OpenAI, Convai, ElevenLabs) in Unity client.** All LLM calls go through `api.dos.ai` / Go LLM Gateway.
4. **NEVER use Host Mode for production.** Server Mode dedicated only.
5. **NEVER add or replace backend / auth / social stack without an ADR and JOY approval.** Nakama OSS is the accepted game backend baseline per ADR 0010. Heroic Cloud, Hiro, Satori, OpenAuth, PlayFab, AccelByte, or a Supabase-first rollback require a new ADR.
6. **NEVER change Unity Asset Serialization away from Force Text.** Breaks LFS + diff.
7. **NEVER claim "done" without reviewer pass** (JOY is non-coder, cannot review code himself).
8. **ALWAYS edit BOTH `.claude/CLAUDE.md` and `AGENTS.md` together when updating project context.** They are sister files - Claude Code auto-loads CLAUDE.md, Codex CLI / Cursor / Copilot auto-load AGENTS.md. Edit one without the other = drift; the un-updated file lies to whichever agent reads it. Both files MUST be identical except for the sister-file comment header at line 1.

## Open Decision Points (need JOY input later)

- Final game name (SECOND SPAWN is codename, may rename after vertical slice playable)
- SECOND token economy design (cost per reincarnation, source, sink)
- BodyTime tuning (where time drains, how it is earned, how it can be spent, and whether it can convert to/from SECOND token)
- Advanced body or soul progression replacing the deferred concept
- Hunter NFT integration approach: Option 1 (preset hero) vs Hybrid 1+3 (modular pieces)
- Phase 2 LLM model split (when to use Haiku vs Sonnet)
- Voice NPC vendor (OpenAI Realtime vs ElevenLabs vs self-host)
- Backend deployment path: self-hosted Nakama OSS vs Heroic Cloud later. Hiro / Satori require license and pricing review before adoption.
- Dedicated server hosting (Hetzner specs, region)
- Photon Fusion 2 license tier when scaling beyond Cloud free 20 CCU
