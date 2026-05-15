# Systems Index: SECOND SPAWN

*Status: Draft (bootstrapped from CLAUDE.md tech stack + gameplay arch)*
*Created: 2026-05-14*

---

## Overview

SECOND SPAWN is a hybrid MMO + top-down ARPG. The mechanical scope spans:

- ARPG core (combat, movement, minimal controller baseline first; Opsive UCC evaluated after baseline)
- Multiplayer networking (Photon Fusion 2 dedicated server)
- Persistence (Supabase Postgres + Realtime side-channel)
- LLM NPCs (Convai phase 1, Go gateway phase 2)
- AI agent autoplay (server-side, capability-capped)
- Cultivation 6-tier progression
- Reincarnation loop (death -> SECOND token -> new body)
- Time-as-currency body lifespan economy
- NFT integration (DOS Chain via thirdweb)
- Server-authoritative invariants (anti-cheat assumes open source)

This index enumerates every system the game needs, categorizes by Core/Gameplay/Progression/Economy/Persistence/UI/Audio/Narrative/Meta, assigns priority tier (MVP/VS/Alpha/Full), and dependency-sorts the build order.

---

## Systems Enumeration

| # | System | Category | Priority | Status | Design Doc | Depends On |
| --- | ---- | ---- | ---- | ---- | ---- | ---- |
| 1 | NetworkRunner / Photon Fusion 2 setup | Core | MVP | Not started | (TDD pending) | - |
| 2 | Player Controller (minimal baseline, Simple KCC spike, Opsive UCC evaluation later) | Core | MVP | Drafted | [07-player-controller-prototype.md](07-player-controller-prototype.md) | NetworkRunner |
| 3 | Camera (top-down ARPG) | Core | MVP | Not started | (TDD pending) | Player Controller |
| 4 | Input system (Unity Input System) | Core | MVP | Not started | - | Player Controller |
| 5 | Zone scene management (1 zone vertical slice) | Core | MVP | Not started | (TDD pending) | NetworkRunner |
| 6 | Combat (ARPG action) | Gameplay | MVP | Not started | (TDD pending) | Player Controller, Networked state |
| 7 | NPC dialogue (Convai SDK + intent validation) | Gameplay | MVP | Not started | (TDD pending) | Go gateway (phase 2 ready) |
| 8 | Quest system (linear, 3-5 quests slice scope) | Gameplay | VS | Not started | (TDD pending) | NPC dialogue, persistence |
| 9 | Dungeon instance (1 dungeon, 1 boss) | Gameplay | VS | Not started | (TDD pending) | Combat, NPC dialogue, Photon |
| 10 | Boss LLM dialogue (Convai grounded) | Gameplay | VS | Not started | (TDD pending) | NPC dialogue |
| 11 | AI agent for offline players (server-side) | Gameplay | VS | Not started | (TDD pending) | NetworkRunner, LLM gateway, intent schema |
| 12 | Cultivation 6-tier (slice: tier 1-2) | Progression | MVP | Drafted | [04-cultivation-system.md](04-cultivation-system.md) | Persistence |
| 13 | Reincarnation flow (death -> SECOND -> new body) | Progression | VS | Not started | (TDD pending) | Cultivation, NFT escrow, Persistence |
| 14 | SECOND token economy | Economy | VS | Not designed | (GDD pending - JOY input) | DOS Chain integration |
| 36 | Time-as-currency (`BodyTime`) | Economy | VS | Drafted | [08-time-as-currency.md](08-time-as-currency.md) | Reincarnation, Combat, Persistence |
| 15 | NFT inventory (Hunter skin slice scope) | Economy | VS | Not started | (TDD pending) | thirdweb-api MCP, Persistence |
| 16 | NFT escrow (lock on equip, release on unequip) | Economy | VS | Not started | (TDD pending) | NFT inventory, DOS Chain |
| 17 | Loot / drop tables | Economy | VS | Not started | (TDD pending) | Combat, persistence |
| 18 | Profile persistence (Supabase Postgres) | Persistence | MVP | Not started | (TDD pending) | Supabase Auth |
| 19 | Inventory persistence | Persistence | MVP | Not started | (TDD pending) | Profile, NFT inventory |
| 20 | Quest progress persistence | Persistence | MVP | Not started | (TDD pending) | Profile, Quest system |
| 21 | Cultivation tier persistence (carries through reincarnation) | Persistence | MVP | Not started | (TDD pending) | Profile |
| 22 | Auth (Supabase email + DOS Chain wallet) | Persistence | MVP | Not started | (TDD pending - reuse DOS.Me pattern) | Supabase, thirdweb |
| 23 | HUD (combat, cultivation tier, currency) | UI | VS | Not started | (deferred template `_deferred/hud-design.md`) | Combat, Cultivation |
| 24 | Inventory UI | UI | VS | Not started | (deferred template `_deferred/ux-spec.md`) | Inventory persistence |
| 25 | NPC dialogue UI | UI | VS | Not started | (deferred) | NPC dialogue |
| 26 | Quest tracker UI | UI | VS | Not started | (deferred) | Quest system |
| 27 | Reincarnation UI | UI | VS | Not started | (deferred) | Reincarnation flow |
| 28 | AI agent activity log UI | UI | VS | Not started | (deferred) | AI agent |
| 29 | Audio (SFX, ambient, music - placeholder for slice) | Audio | VS | Not started | (deferred template `_deferred/sound-bible.md`) | - |
| 30 | Chat (Supabase Realtime - global + zone) | Narrative / UI | VS | Not started | (TDD pending) | Supabase Realtime |
| 31 | LLM intent validation (Go gateway pattern) | Meta / Engineering | MVP | Not started | (TDD pending - reuse DOSRouter) | LLM provider |
| 32 | LLM safety (rate limit, prompt injection defense) | Meta / Engineering | MVP | Not started | (TDD pending - reuse DOSafe patterns) | Go gateway |
| 33 | Anti-cheat / server-authority verification | Meta / Engineering | MVP | (Architectural) | [docs/ARCHITECTURE.md "Critical Invariants"](../ARCHITECTURE.md#critical-invariants) | All gameplay systems |
| 34 | Telemetry / monitoring (Sentry + Grafana) | Meta | Alpha | Deferred | - | All systems |
| 35 | Onboarding / tutorial | Meta | VS | Deferred (assume slice = no tutorial) | - | All gameplay systems |

**Total: 36 systems identified for slice scope.**

---

## Categories (which apply to SECOND SPAWN)

| Category | Description | Count |
| ---- | ---- | ---- |
| **Core** | Foundation systems everything depends on | 5 (NetworkRunner, Controller, Camera, Input, Zone management) |
| **Gameplay** | The systems that make the game fun | 6 (Combat, NPC dialogue, Quest, Dungeon, Boss LLM, AI agent) |
| **Progression** | How the player grows over time | 2 (Cultivation, Reincarnation) |
| **Economy** | Resource creation and consumption | 5 (SECOND token, Time-as-currency, NFT inventory, NFT escrow, Loot) |
| **Persistence** | Save state and continuity | 5 (Profile, Inventory, Quest, Cultivation, Auth) |
| **UI** | Player-facing information displays | 6 (HUD, Inventory UI, NPC dialogue UI, Quest tracker, Reincarnation UI, Agent log) |
| **Audio** | Sound and music systems | 1 (placeholder for slice) |
| **Narrative** | Story / dialogue delivery | 1 (Chat - dialogue system already in Gameplay row 7) |
| **Meta** | Outside-core-loop systems | 5 (LLM intent validation, LLM safety, Anti-cheat verification, Telemetry, Onboarding) |

---

## Dependency Map (build order)

### Foundation Layer (no gameplay dependencies)

1. NetworkRunner / Photon Fusion 2 setup (#1)
2. Auth (Supabase + DOS Chain wallet) (#22)
3. Profile persistence (#18)
4. Go LLM gateway (DOSRouter pattern) (#31)
5. LLM safety (rate limit, prompt injection) (#32)

### Core Layer (depends on foundation)

6. Player Controller baseline / Simple KCC spike / Opsive evaluation (#2) - depends on: NetworkRunner
7. Camera (#3) - depends on: Player Controller
8. Input system (#4) - depends on: Player Controller
9. Zone scene management (#5) - depends on: NetworkRunner
10. Inventory persistence (#19) - depends on: Profile
11. Cultivation tier persistence (#21) - depends on: Profile

### Feature Layer (depends on core)

12. Combat (#6) - depends on: Player Controller, networked state
13. NPC dialogue (Convai + intent validation) (#7) - depends on: Go gateway
14. Cultivation system (#12) - depends on: Cultivation persistence, Combat
15. NFT inventory (#15) - depends on: Auth, thirdweb-api MCP
16. Chat (Supabase Realtime) (#30) - depends on: Auth, Supabase Realtime
17. Quest system (#8) - depends on: NPC dialogue, persistence
18. Dungeon instance (#9) - depends on: Combat, Photon

### Integration Layer (depends on features)

19. Boss LLM dialogue (#10) - depends on: NPC dialogue, Dungeon
20. NFT escrow (#16) - depends on: NFT inventory, DOS Chain
21. Loot / drop tables (#17) - depends on: Combat, Persistence
22. Reincarnation flow (#13) - depends on: Cultivation, NFT escrow, Persistence
23. Time-as-currency (#36) - depends on: Reincarnation, Combat, Persistence
24. SECOND token economy (#14) - depends on: DOS Chain integration, Reincarnation
25. AI agent for offline players (#11) - depends on: NetworkRunner, LLM gateway, intent schema, Cultivation, Combat, Time-as-currency

### Presentation Layer (depends on features)

26. HUD (#23)
27. Inventory UI (#24)
28. NPC dialogue UI (#25)
29. Quest tracker UI (#26)
30. Reincarnation UI (#27)
31. AI agent activity log UI (#28)
32. Audio (#29)

### Meta Layer

33. Anti-cheat verification (#33) - cuts across everything
34. Quest progress persistence (#20) - depends on: Quest system
35. Telemetry (#34) - depends on: everything
36. Onboarding (#35) - depends on: all gameplay (deferred for slice)

---

## High-Risk Systems

| System | Risk Type | Risk Description | Mitigation |
| ---- | ---- | ---- | ---- |
| AI agent for offline players (#11) | Technical + Design | LLM cost at scale; agent feels invisible or invasive | Prototype early in slice; add visible activity log; capability cap |
| LLM intent validation (#31) + safety (#32) | Security | Open-source codebase + LLM = injection / abuse vector | Reuse DOSafe patterns; per-NPC memory cap; per-player rate limit |
| NFT escrow (#16) | Technical | Latency between Unity equip action and DOS Chain confirmation | Optimistic UI + reconcile-on-failure; cache lock state in Supabase |
| Reincarnation flow (#13) | Design | Cultivation carryover too generous = no death weight; too punitive = grind | Tune cost during slice playtests |
| Time-as-currency (#36) | Design + Economy | Constant drain can feel oppressive; weak drain can feel invisible | Start with danger-zone drain, one earn source, one spend sink |
| Photon Fusion 2 dedicated server (#1) | Technical | Solo dev capacity to run dedicated infra | Slice uses Photon Cloud free 20 CCU; production migration is post-slice |
| Convai SDK in Unity (#7) | Technical | 3rd-party SDK may not test against Unity 6.5 beta | Have phase 2 fallback (Go gateway + custom LLM) ready in design |

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
| 8 | Go LLM gateway scaffold (#31) | Phase 2 | M | Reuse DOSRouter pattern |
| 9 | NPC dialogue + Convai (#7) | Phase 3 | L | First LLM integration |
| 10 | LLM safety (#32) | Phase 3 | M | Concurrent with #9 |
| 11 | Quest system (#8) | Phase 4 | L | |
| 12 | Dungeon instance (#9) | Phase 4 | L | |
| 13 | Boss LLM dialogue (#10) | Phase 4 | M | Layered on NPC dialogue |
| 14 | Cultivation system (#12) | Phase 5 | L | GDD already drafted at `04-cultivation-system.md` |
| 15 | Reincarnation flow (#13) | Phase 5 | L | |
| 16 | Time-as-currency (#36) | Phase 6 | M | BodyTime meter, one earn source, one spend sink |
| 17 | NFT inventory (#15) + escrow (#16) | Phase 7 | L | DOS Chain test net |
| 18 | SECOND token economy (#14) | Phase 7 | M | JOY input required first |
| 19 | AI agent for offline players (#11) | Phase 8 | XL | Highest-risk system |
| 20 | UI cluster (#23-#28) | Throughout phases 2-8 | XL | Build incrementally |
| 21 | Audio placeholder (#29) | Phase 9 | S | Slice-quality only |
| 22 | Chat (#30) | Phase 9 | M | Supabase Realtime |
| 23 | Polish + playtest | Phase 9 | XL | |

Effort estimate: S = 1-3 days, M = 4-7 days, L = 1-2 weeks, XL = 2-4 weeks (solo dev + AI agent).

---

## Progress Tracker

| Metric | Count |
| ---- | ---- |
| Total systems identified | 36 |
| Design docs started | 5 (cultivation, overview design, player controller prototype, time-as-currency, Pirate Adventure reference review) |
| Design docs reviewed | 0 |
| Design docs approved | 0 |
| MVP systems with TDD started | 0 |
| Vertical Slice systems with TDD started | 0 |

---

## Next Steps

- [ ] JOY review and approve this systems enumeration
- [ ] Per-system TDD as each system is started (use `templates/technical-design-document.md`)
- [ ] Re-run systems-index review at slice midpoint to add missed systems
- [ ] Decompose AI agent autoplay (#11) into sub-systems before Phase 7 (highest risk)
