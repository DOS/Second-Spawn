# Systems Index: SECOND SPAWN

*Status: Living index*
*Created: 2026-05-14*
*Last updated: 2026-05-19*

---

## Overview

SECOND SPAWN is a hybrid MMO + top-down ARPG. The mechanical scope spans:

- ARPG core (combat, movement, minimal controller baseline first; Opsive UCC evaluated after baseline)
- Multiplayer networking (Photon Fusion 2 dedicated server)
- Persistence (Nakama OSS + Postgres, with Supabase sidecar where useful)
- LLM NPCs (Convai phase 1, api.dos.ai model service phase 2)
- AI agent autoplay (server-side, capability-capped)
- OpenClaw-connected NPCs (user-owned agents as server-validated world actors)
- Level/stat progression
- Reincarnation loop (death -> SECOND -> new Frame)
- TIME / SECOND body lifespan economy
- Gate first-clear economy through server-issued Pioneer Charter records
- NFT integration (DOS Chain via thirdweb)
- Server-authoritative invariants (anti-cheat assumes open source)

This index enumerates every system the game needs, categorizes by Core/Gameplay/Progression/Economy/Persistence/UI/Audio/Narrative/Meta, assigns priority tier (MVP/VS/Alpha/Full), and dependency-sorts the build order.

---

## Systems Enumeration

| # | System | Category | Priority | Status | Design Doc | Depends On |
| --- | ---- | ---- | ---- | ---- | ---- | ---- |
| 1 | NetworkRunner / Photon Fusion 2 setup | Core | MVP | Prototype | (TDD pending) | - |
| 2 | Player Controller (minimal baseline, Simple KCC spike, Opsive UCC evaluation later) | Core | MVP | Prototype | [07-player-controller-prototype.md](07-player-controller-prototype.md) | NetworkRunner |
| 3 | Camera (top-down ARPG) | Core | MVP | Prototype | (TDD pending) | Player Controller |
| 4 | Input system (Unity Input System) | Core | MVP | Prototype | - | Player Controller |
| 5 | Zone scene management (1 zone vertical slice) | Core | MVP | Prototype | (TDD pending) | NetworkRunner |
| 6 | Combat (ARPG action) | Gameplay | MVP | Not started | (TDD pending) | Player Controller, Networked state |
| 7 | NPC dialogue and human-believable NPC agent model | Gameplay | MVP | Design | [13-human-believable-npc-agent-model.md](13-human-believable-npc-agent-model.md) | api.dos.ai model service (phase 2 ready), Profile persistence |
| 8 | Quest system (linear, 3-5 quests slice scope) | Gameplay | VS | Not started | (TDD pending) | NPC dialogue, persistence |
| 9 | Dungeon instance (1 dungeon, 1 boss) | Gameplay | VS | Not started | (TDD pending) | Combat, NPC dialogue, Photon |
| 10 | Boss LLM dialogue (Convai grounded) | Gameplay | VS | Not started | (TDD pending) | NPC dialogue |
| 11 | AI agent for offline players (server-side) | Gameplay | VS | Prototype | [10-character-profile-agent-memory.md](10-character-profile-agent-memory.md) | NetworkRunner, api.dos.ai model service, intent schema |
| 37 | OpenClaw-connected NPC bridge (user-owned agents as NPC actors) | Gameplay / Meta | Alpha | Concept | [10-character-profile-agent-memory.md](10-character-profile-agent-memory.md) | Auth, Nakama, api.dos.ai model service, NPC dialogue, LLM safety |
| 12 | Level/stat progression | Progression | MVP | Prototype | [14-character-stat-and-relationship-system.md](14-character-stat-and-relationship-system.md) | Persistence |
| 13 | Reincarnation flow (death -> SECOND -> new Frame) | Progression | VS | Prototype | [12-game-design-document.md](12-game-design-document.md) | Level/stats, NFT escrow, Persistence |
| 14 | SECOND economy | Economy | VS | Not designed | (GDD pending - JOY input) | DOS Chain integration |
| 36 | TIME / SECOND economy | Economy | VS | Prototype | [08-time-as-currency.md](08-time-as-currency.md) | Reincarnation, Combat, Persistence |
| 38 | Gate first-clear and Pioneer Charter economy | Economy / Gameplay | Alpha | Concept | [15-gate-dungeon-pioneer-charter-system.md](15-gate-dungeon-pioneer-charter-system.md) | Dungeon, Combat, Persistence, Rewards, Anti-cheat |
| 15 | NFT inventory (Hunter Frame skin slice scope) | Economy | VS | Not started | (TDD pending) | thirdweb-api MCP, Persistence |
| 16 | NFT escrow (lock on equip, release on unequip) | Economy | VS | Not started | (TDD pending) | NFT inventory, DOS Chain |
| 17 | Loot / drop tables | Economy | VS | Not started | (TDD pending) | Combat, persistence |
| 18 | Profile persistence (Nakama OSS + Postgres) | Persistence | MVP | Prototype | [10-character-profile-agent-memory.md](10-character-profile-agent-memory.md) | Auth |
| 19 | Inventory persistence | Persistence | MVP | Not started | (TDD pending) | Profile, NFT inventory |
| 20 | Quest progress persistence | Persistence | MVP | Not started | (TDD pending) | Profile, Quest system |
| 21 | Level/stat persistence | Persistence | MVP | Prototype | [14-character-stat-and-relationship-system.md](14-character-stat-and-relationship-system.md) | Profile |
| 22 | Auth (Nakama + DOS Chain wallet, Supabase sidecar if useful) | Persistence | MVP | Prototype | (TDD pending - reuse DOS.Me pattern as identity bridge reference) | Nakama, thirdweb |
| 23 | HUD (combat, level/stats, TIME) | UI | VS | Prototype | (deferred template `_deferred/hud-design.md`) | Combat, Profile |
| 24 | Inventory UI | UI | VS | Not started | (deferred template `_deferred/ux-spec.md`) | Inventory persistence |
| 25 | NPC dialogue UI | UI | VS | Not started | (deferred) | NPC dialogue |
| 26 | Quest tracker UI | UI | VS | Not started | (deferred) | Quest system |
| 27 | Reincarnation UI | UI | VS | Not started | (deferred) | Reincarnation flow |
| 28 | AI agent activity log UI | UI | VS | Not started | (deferred) | AI agent |
| 29 | Audio (SFX, ambient, music - placeholder for slice) | Audio | VS | Not started | (deferred template `_deferred/sound-bible.md`) | - |
| 30 | Chat (Nakama channel first, Supabase Realtime sidecar only if useful) | Narrative / UI | VS | Not started | (TDD pending) | Nakama |
| 31 | LLM intent validation (api.dos.ai model service pattern) | Meta / Engineering | MVP | Prototype | [11-npc-agent-brain-architecture.md](11-npc-agent-brain-architecture.md) | LLM provider |
| 32 | LLM safety (rate limit, prompt injection defense) | Meta / Engineering | MVP | Partial | (TDD pending - reuse DOSafe patterns) | api.dos.ai model service |
| 33 | Anti-cheat / server-authority verification | Meta / Engineering | MVP | (Architectural) | [docs/ARCHITECTURE.md "Critical Invariants"](../ARCHITECTURE.md#critical-invariants) | All gameplay systems |
| 34 | Telemetry / monitoring (Sentry + Grafana) | Meta | Alpha | Deferred | - | All systems |
| 35 | Onboarding / tutorial | Meta | VS | Deferred (assume slice = no tutorial) | - | All gameplay systems |

**Total: 38 systems identified for slice scope and post-slice roadmap.**

---

## Categories (which apply to SECOND SPAWN)

| Category | Description | Count |
| ---- | ---- | ---- |
| **Core** | Foundation systems everything depends on | 5 (NetworkRunner, Controller, Camera, Input, Zone management) |
| **Gameplay** | The systems that make the game fun | 7 (Combat, NPC dialogue, Quest, Dungeon, Boss LLM, AI agent, OpenClaw-connected NPC bridge) |
| **Progression** | How the player grows over time | 2 (Level/stats, Reincarnation) |
| **Economy** | Resource creation and consumption | 6 (SECOND, TIME / SECOND economy, Pioneer Charter, NFT inventory, NFT escrow, Loot) |
| **Persistence** | Save state and continuity | 5 (Profile, Inventory, Quest, Level/stats, Auth) |
| **UI** | Player-facing information displays | 6 (HUD, Inventory UI, NPC dialogue UI, Quest tracker, Reincarnation UI, Agent log) |
| **Audio** | Sound and music systems | 1 (placeholder for slice) |
| **Narrative** | Story / dialogue delivery | 1 (Chat - dialogue system already in Gameplay row 7) |
| **Meta** | Outside-core-loop systems | 5 (LLM intent validation, LLM safety, Anti-cheat verification, Telemetry, Onboarding) |

---

## Dependency Map (build order)

### Foundation Layer (no gameplay dependencies)

1. NetworkRunner / Photon Fusion 2 setup (#1)
2. Auth (Nakama + DOS Chain wallet, Supabase sidecar if useful) (#22)
3. Profile persistence (#18)
4. api.dos.ai model service integration (DOSRouter pattern) (#31)
5. LLM safety (rate limit, prompt injection) (#32)

### Core Layer (depends on foundation)

6. Player Controller baseline / Simple KCC spike / Opsive evaluation (#2) - depends on: NetworkRunner
7. Camera (#3) - depends on: Player Controller
8. Input system (#4) - depends on: Player Controller
9. Zone scene management (#5) - depends on: NetworkRunner
10. Inventory persistence (#19) - depends on: Profile
11. Level/stat persistence (#21) - depends on: Profile

### Feature Layer (depends on core)

12. Combat (#6) - depends on: Player Controller, networked state
13. NPC dialogue (Convai + intent validation) (#7) - depends on: api.dos.ai model service
14. OpenClaw-connected NPC bridge (#37) - depends on: Auth, Nakama, api.dos.ai model service, NPC dialogue, LLM safety
15. Level/stat progression (#12) - depends on: Profile persistence, Combat
16. NFT inventory (#15) - depends on: Auth, thirdweb-api MCP
17. Chat (Nakama channel first, Supabase Realtime sidecar only if useful) (#30) - depends on: Auth, Nakama
18. Quest system (#8) - depends on: NPC dialogue, persistence
19. Dungeon instance (#9) - depends on: Combat, Photon

### Integration Layer (depends on features)

20. Boss LLM dialogue (#10) - depends on: NPC dialogue, Dungeon
21. NFT escrow (#16) - depends on: NFT inventory, DOS Chain
22. Loot / drop tables (#17) - depends on: Combat, Persistence
23. Reincarnation flow (#13) - depends on: Level/stats, NFT escrow, Persistence
24. TIME / SECOND economy (#36) - depends on: Reincarnation, Combat, Persistence
25. SECOND economy (#14) - depends on: DOS Chain integration, Reincarnation
26. AI agent for offline players (#11) - depends on: NetworkRunner, api.dos.ai model service, intent schema, Combat, TIME / SECOND economy
27. Gate first-clear and Pioneer Charter economy (#38) - depends on: Dungeon, Combat, Persistence, Rewards, Anti-cheat

### Presentation Layer (depends on features)

28. HUD (#23)
29. Inventory UI (#24)
30. NPC dialogue UI (#25)
31. Quest tracker UI (#26)
32. Reincarnation UI (#27)
33. AI agent activity log UI (#28)
34. Audio (#29)

### Meta Layer

35. Anti-cheat verification (#33) - cuts across everything
36. Quest progress persistence (#20) - depends on: Quest system
37. Telemetry (#34) - depends on: everything
38. Onboarding (#35) - depends on: all gameplay (deferred for slice)

---

## High-Risk Systems

| System | Risk Type | Risk Description | Mitigation |
| ---- | ---- | ---- | ---- |
| AI agent for offline players (#11) | Technical + Design | LLM cost at scale; agent feels invisible or invasive | Prototype early in slice; add visible activity log; capability cap |
| OpenClaw-connected NPC bridge (#37) | Product + Security | User-owned agents can create moderation, spam, prompt injection, and trust-boundary risk | Treat connected agents as untrusted external actors; require consent, identity binding, rate limit, moderation, and server validation |
| LLM intent validation (#31) + safety (#32) | Security | Open-source codebase + LLM = injection / abuse vector | Reuse DOSafe patterns; per-NPC memory cap; per-player rate limit |
| NFT escrow (#16) | Technical | Latency between Unity equip action and DOS Chain confirmation | Optimistic UI + reconcile-on-failure; cache lock state in Supabase |
| Reincarnation flow (#13) | Design | Carryover too generous = no death weight; too punitive = grind | Tune cost during slice playtests |
| TIME / SECOND economy (#36) | Design + Economy | Constant drain can feel oppressive; weak drain can feel invisible | Start with danger-zone drain, one earn source, one spend sink |
| Gate first-clear and Pioneer Charter economy (#38) | Economy + Anti-cheat | First-clear rewards can become exploit magnets or feel unfair if autonomous agents claim them while players sleep | Start with non-economic first-clear records; require server clear logs, caps, expiry, and human-led eligibility for economic Charters |
| Photon Fusion 2 dedicated server (#1) | Technical | Solo dev capacity to run dedicated infra | Slice uses Photon Cloud free 20 CCU; production migration is post-slice |
| Convai SDK in Unity (#7) | Technical | 3rd-party SDK may not test against Unity 6.5 beta | Have phase 2 fallback (`api.dos.ai` / api.dos.ai model service + custom LLM) ready in design |

---

## Recommended Design Order (per build phase)

Aligned with [02-vertical-slice-spec.md](02-vertical-slice-spec.md) build phases.

| Order | System | Phase | Effort | Note |
| --- | ---- | ---- | ---- | ---- |
| 1 | NetworkRunner setup (#1) | Phase 1 | M | Reference MetaDOS BR template |
| 2 | Auth (#22) | Phase 1 | M | Reuse DOS.Me Supabase pattern |
| 3 | Profile persistence (#18) | Phase 1 | S | |
| 4 | Player Controller baseline / Simple KCC spike / Opsive evaluation (#2) | Phase 2 | M | Build minimal Fusion controller first; evaluate Simple KCC next; evaluate Opsive in isolation after that |
| 5 | Camera + Input (#3, #4) | Phase 2 | S | Standard URP |
| 6 | Zone scene management (#5) | Phase 2 | M | |
| 7 | Combat (#6) | Phase 2 | L | Server-authoritative critical |
| 8 | api.dos.ai model service integration (#31) | Phase 2 | M | Reuse DOSRouter pattern |
| 9 | NPC dialogue + Convai (#7) | Phase 3 | L | First LLM integration |
| 10 | LLM safety (#32) | Phase 3 | M | Concurrent with #9 |
| 11 | Quest system (#8) | Phase 4 | L | |
| 12 | Dungeon instance (#9) | Phase 4 | L | |
| 13 | Boss LLM dialogue (#10) | Phase 4 | M | Layered on NPC dialogue |
| 14 | Reincarnation flow (#13) | Phase 5 | L | |
| 15 | TIME / SECOND economy (#36) | Phase 6 | M | TIME meter measured in SECOND, one earn source, one spend sink |
| 16 | NFT inventory (#15) + escrow (#16) | Phase 7 | L | DOS Chain test net |
| 17 | SECOND economy (#14) | Phase 7 | M | JOY input required first |
| 18 | AI agent for offline players (#11) | Phase 8 | XL | Highest-risk system |
| 19 | UI cluster (#23-#28) | Throughout phases 2-8 | XL | Build incrementally |
| 20 | Audio placeholder (#29) | Phase 9 | S | Slice-quality only |
| 21 | Chat (#30) | Phase 9 | M | Nakama channel first, Supabase sidecar only if useful |
| 22 | Polish + playtest | Phase 9 | XL | |
| 23 | Gate first-clear and Pioneer Charter economy (#38) | Post-slice / Alpha | L | Start with ledger and badge before in-game royalty |

Effort estimate: S = 1-3 days, M = 4-7 days, L = 1-2 weeks, XL = 2-4 weeks (solo dev + AI agent).

---

## Progress Tracker

| Metric | Count |
| ---- | ---- |
| Total systems identified | 38 |
| Systems with prototype implementation | 14 |
| Design docs started | 13 |
| Design docs reviewed | 0 |
| Design docs approved | 0 |
| MVP systems with TDD started | 0 |
| Vertical Slice systems with TDD started | 0 |
| Execution tracker | GitHub Projects recommended for issue, PR, review, and milestone tracking |

---

## Next Steps

- [ ] JOY review and approve this systems enumeration
- [ ] Per-system TDD as each system is started (use `templates/technical-design-document.md`)
- [ ] Re-run systems-index review at slice midpoint to add missed systems
- [ ] Decompose AI agent autoplay (#11) into sub-systems before Phase 7 (highest risk)
- [ ] Write TDD for Gate clear logs before implementing Pioneer Charter rewards
