# SECOND SPAWN - Project Context

This file is the primary context for any AI coding agent working on this repository. Read fully before making changes.

## Game Overview

- **Name (codename, may rename pre-launch):** SECOND SPAWN
- **Genre:** Hybrid MMO + Top-down ARPG (NOT full open-world MMORPG)
- **References:** Diablo IV, Path of Exile 2, Lost Ark
- **Setting:** Near-future ~2050, post-apocalyptic, MetaDOS universe
- **Tone:** Dark sci-fi, cyberpunk, cultivation-progression, AI NPC society

## Three Core USPs (DO NOT LOSE TRACK)

1. **AI Agent 24/7** - When the player is offline, an LLM-driven AI agent fully controls their character (farms, quests, socializes with NPCs and other players' agents). When the player returns, they take over control. This is a near-unique feature in MMO/ARPG space.
2. **Reincarnation with progression reset** - Death is permanent for the body. Consciousness transfers to a new body via SECOND token or special item. Progression resets (roguelike-MMO hybrid). Cultivation tier may carry over partial.
3. **Consciousness transfer to NPC/synthetic bodies** - Sci-fi explanation (mind upload, synthetic bodies, Nibirium-enhanced cloning). NOT spiritual reincarnation.

## Cultivation System (sci-fi, not Chinese-style)

6 tiers:

1. Awakening - Activate Nibirium absorption
2. Enhancement - Body strengthening
3. Core Formation - Energy core formation
4. Evolution - DNA / special ability evolution
5. Transcendence - Beyond human limits
6. Ascension - Near-divine

International-friendly framing. Explained via science (Nibirium, biotech, consciousness transfer).

## Gameplay Architecture

- Instance-based zones, ~20 players per zone
- Dungeon / instance separate
- Guild PvP up to 50v50
- Top-down ARPG action combat
- Pet system: NFT-based, 1 equip slot, NOT looted from bosses (marketplace + breeding only)
- Mount system: movement only, no mounted combat (reduce animation workload)

## Tech Stack (FINAL)

### Engine

- Unity 6 LTS + URP
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

- **Supabase Auth** (reuse DOS.Me pattern, do not invent new auth)
- **Supabase Postgres** (durable state: profile, inventory, quest progress, NFT lock state, cultivation tier)
- **Supabase Realtime** (chat global, presence, friend, party invite, notification - NOT for combat / movement sync)
- **Supabase Storage** (avatar, screenshot, UGC)
- **Go LLM Gateway** (reuse DOSRouter pattern, self-host VPS, low-latency)
- **Redis** (session, rate limit, transient cache)

### LLM

**Phase 1 (MVP):**

- Convai SDK in Unity for NPC dialogue
- Limit: no full custom LLM control, accept Convai cost

**Phase 2 (post-MVP):**

- Migrate to Go gateway, models:
  - Haiku 4.5 for NPC chat (fast, cheap)
  - Sonnet 4.6 for boss / quest / cultivation master dialog
- RAG memory: Supabase pgvector or Qdrant
- Voice: OpenAI Realtime API via ephemeral token (NOT API key in client) OR ElevenLabs
- Client AI: Unity Sentis for small perception (optional, phase 3)

### LLM Safety (CRITICAL)

- **Server-side intent validation ONLY** - never trust LLM output as authoritative
- **Never let LLM directly mutate game state** (no auto-grant item, gold, XP)
- Per-NPC memory budget cap
- Rate limit per player (LLM token + request count)
- Prompt injection defense (reuse DOSafe patterns)
- All LLM calls go through Go gateway, never direct from Unity client

### AI Agent for Offline Players (CORE FEATURE)

- LLM-driven autonomous agent controls player character when offline
- Agent operates within Fusion server tick (server-authoritative)
- Agent decision loop: pull state from Fusion -> reason via LLM gateway -> emit action intent -> server validates -> apply
- Anti-abuse: agent inherits player's rate limit + capability cap
- Agent persona: derived from player history + character cultivation tier
- Agent death = body death = reincarnation triggered (same as player death)

### NFT / Blockchain

- **Chain:** DOS Chain (DOS ecosystem chain)
- **MCP integration:** thirdweb-api MCP (already in JOY's tool stack)
- **Inherited from MetaDOS:**
  - Hunter skins - Option 1 (preset hero characters, may hybrid with Option 3 later)
  - Weapons
  - Pets (1 equip slot, marketplace + breeding only, no boss drop)
- **Wallet auth:** Sign-message pattern via thirdweb or Supabase + DOS Chain
- **In-game lock:** Escrow contract when equipped, release on unequip
- **SECOND token:** Used for reincarnation cost (token economy needs design)

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
- **Backend Go gateway:** VPS or Modal
- **LLM API:** Convai phase 1, then Anthropic + OpenAI phase 2
- **Monitoring:** Sentry (error) + Grafana (metrics)

### Testing

- Unity Multiplayer Play Mode (in-editor multi-instance)
- Unity Test Framework (unit + integration)
- Bot load test (Fusion bots simulating 50-100 players per zone)

## AI Agent Tools

### Primary

- **Claude Code Desktop (Windows native)** - main dev driver
- **Coplay unity-mcp** (CoplayDev/unity-mcp) - bridges Claude to Unity Editor (asset, scene, script, component manipulation)
- **Codex CLI rescue skill** - 2nd opinion / refactor / review when Claude is stuck (use `codex:rescue` skill, NOT standalone Codex App)

### Optional

- VS Code Claude extension (for code-heavy backend / gateway tasks, NOT Unity scene work)
- Rider or VS Code as C# code reader (not main IDE - Unity Editor is)

### MCP Servers in Use

- Coplay unity-mcp (Unity Editor bridge)
- thirdweb-api (DOS Chain wallet, NFT logic)
- Supabase MCP (database, auth, edge functions)
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
- Cultivation tier progression

### Recommended Reading List

- Photon Fusion BR200 sample documentation
- Photon Fusion Dedicated Server overview
- Opsive Ultimate Character Controller getting started
- Behavior Designer manual
- Convai Unity SDK docs
- Unity Multiplayer Play Mode tutorial
- Coplay unity-mcp + Claude Code setup guide
- DOSRouter Go gateway pattern (JOY's existing repo)
- DOS.Me Supabase auth pattern (JOY's existing repo)

## Project Conventions

### Naming

- Repo: `Second-Spawn` (matches GitHub repo name as-is)
- Repo root: `D:\Projects\Second-Spawn`
- Unity project subfolder: `D:\Projects\Second-Spawn\unity` (NOT at repo root - multi-stack repo: Unity at `unity/`, Go gateway at `backend/`, docs at `docs/`)
- C# code: Microsoft conventions (PascalCase classes, camelCase fields)
- Branches: `feat/<short-desc>`, `fix/<short-desc>`, `chore/<short-desc>`

### Documentation Language

- All code, comments, docs, commits, PR titles, README, ROADMAP: **English**
- Communication with JOY (this user): Vietnamese (per global CLAUDE.md)
- No em-dashes anywhere - use `-` (hyphen) only

### Git Workflow

- Default branch: `main`
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
- AI agent control (simple: agent farms one designated area when player offline)
- 2 cultivation tiers playable (Awakening + Enhancement)
- NFT Hunter skin equip + escrow
- Multiplayer 4-20 players per zone
- Basic chat (Supabase Realtime)

OUT of scope for vertical slice:

- Guild / PvP
- Marketplace
- Pet breeding
- Multiple zones
- Tier 3-6 cultivation
- Voice NPC
- Full quest system

## Hard Rules (project-specific, in addition to global CLAUDE.md)

1. **NEVER copy MetaDOS gameplay code.** Extract patterns only. Reference path: `D:\Projects\MetaDOS` (read-only).
2. **NEVER let LLM mutate authoritative game state.** Server validates all intent.
3. **NEVER put API keys (Anthropic, OpenAI, Convai, ElevenLabs) in Unity client.** All LLM calls go through Go gateway.
4. **NEVER use Host Mode for production.** Server Mode dedicated only.
5. **NEVER add Nakama, OpenAuth, or new auth / social stack.** Reuse Supabase + DOS.Me patterns.
6. **NEVER change Unity Asset Serialization away from Force Text.** Breaks LFS + diff.
7. **NEVER claim "done" without reviewer pass** (JOY is non-coder, cannot review code himself).

## Open Decision Points (need JOY input later)

- Final game name (SECOND SPAWN is codename, may rename after vertical slice playable)
- SECOND token economy design (cost per reincarnation, source, sink)
- Hunter NFT integration approach: Option 1 (preset hero) vs Hybrid 1+3 (modular pieces)
- Phase 2 LLM model split (when to use Haiku vs Sonnet)
- Voice NPC vendor (OpenAI Realtime vs ElevenLabs vs self-host)
- Dedicated server hosting (Hetzner specs, region)
- Photon Fusion 2 license tier when scaling beyond Cloud free 20 CCU
