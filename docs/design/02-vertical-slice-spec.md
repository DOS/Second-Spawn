# Vertical Slice Spec: SECOND SPAWN

*Status: Spec with prototype progress*
*Created: 2026-05-14*
*Last updated: 2026-05-17*
*Target completion: 3-6 months from setup (T+3 to T+6 from 2026-05-14)*

> Note: This is the SPEC version (planning the slice). After slice is built, rename to `02-vertical-slice-report.md` and fill the report template (build velocity, playtest results, recommendation PROCEED/PIVOT/KILL).

---

## Validation Question

Can a solo player, in their first 30 minutes of unguided play in a single zone, experience the signature hooks (AI agent autoplay, reincarnation, and time-as-currency) while also feeling basic ARPG level/stat progression AND can a 1-person team (JOY + AI agents) build this slice at representative quality in 3-6 months on the chosen tech stack (Unity 6.5 beta + Photon Fusion 2 + Nakama OSS + api.dos.ai / Go LLM Gateway + thirdweb)?

This is two questions in one: **does the design loop fun?** AND **is the architecture buildable?**

---

## Scope IN

| System | Slice Scope |
| ---- | ---- |
| **Zone** | 1 small open area + 1 hub town |
| **Character class** | 1 (use one of MetaDOS Hunter skins as preset hero) |
| **Dungeon instance** | 1 (single instance with 1 boss encounter) |
| **Boss with LLM dialogue** | 1 (Convai-driven, grounded in zone state) |
| **Quest line** | 1 (3-5 quests sequential) |
| **Reincarnation MVP** | Die -> SECOND token (test token, not real DOS Chain) -> respawn with current-body reset |
| **Time-as-currency MVP** | Body time meter, earn time from a small objective, spend time on one useful service, zero time triggers reincarnation placeholder |
| **AI agent autoplay** | Simple: agent farms one designated area when player offline. Visible activity log on return. |
| **Level/stat progression** | Basic current-body level and stat growth only |
| **NFT Hunter skin** | 1 skin equip flow + escrow contract (test net DOS Chain) |
| **Multiplayer** | 4-20 players per zone instance via Photon Fusion 2 |
| **Chat** | Basic global + zone via Nakama channels |
| **Voice NPC** | NOT in slice (defer phase 2) |

---

## Current Prototype Progress - 2026-05-17

Already implemented:

- Unity `ZoneTest_Hub` can spawn a Fusion local player.
- The spawned player has networked level, combat stats, BodyTime, lifecycle,
  SECOND balance, reincarnation count, visual key, and agent-control state.
- Prototype HUD displays level, HP, energy, attack, defense, agility,
  BodyTime, lifecycle, SECOND balance, and reincarnation count.
- Nakama profile bootstrap persists player profile, current body, stats, traits,
  soul, memory, agent policy, runtime, activity, BodyTime, and reincarnation
  counters.
- Unity can load the Nakama profile and apply current-body stats to the local
  authoritative player.
- BodyTime earn, spend, drain, zero-time death, and reincarnation are available
  through server-side prototype RPCs and Unity debug controls.
- NPC-like actor profiles exist, and `_AgentNPC_Prototype` can patrol, speak,
  and use the gateway decision path with deterministic fallback.

Still missing from the playable slice:

- Real combat damage and server-authoritative enemy rewards.
- Normal-play BodyTime earn and spend sources outside debug controls.
- Player-vs-player or contested-zone time-loot rules.
- Questline, dungeon, boss, and grounded dialogue content.
- Production HUD and reincarnation presentation flow.
- NFT skin equip and escrow.
- Dedicated server deployment and 4-20 player load validation.

---

## Scope OUT (explicitly cut from slice)

- Guild / PvP (50v50 deferred)
- Marketplace / trading (deferred)
- Pet system (NFT pets deferred to post-slice)
- Mount system (deferred)
- Multiple zones (1 zone only)
- Advanced body progression (deferred for redesign)
- Voice NPC (defer)
- Full quest system (linear quest line of 3-5; no branching, no factional choice)
- Crafting (deferred)
- Day / night cycle (deferred)
- Weather (deferred)

---

## Acceptance Criteria

The slice is considered "done" when ALL of the following are true and verified by JOY + Claude Code reviewer:

### Player experience (qualitative, playtest-verifiable)
- [ ] A first-time player can complete the full quest line in 30-60 minutes without out-of-game tutorials
- [ ] At least one playtester comments unprompted on the AI agent activity log being noticeable / interesting
- [ ] At least one playtester deliberately dies to test reincarnation and understands that the current body was replaced
- [ ] At least one playtester notices time-as-currency as a meaningful tradeoff, not just a timer
- [ ] LLM boss dialogue does NOT feel chatbot-y - testers believe the boss "knows" current zone state

### Technical (verifiable in code + tests)
- [ ] Server-authoritative invariant: no client-side damage, position, or item validation. Verified by `code-review` skill pass on combat + inventory + NFT modules.
- [ ] LLM intent validation: every NPC action goes through `api.dos.ai` / Go LLM Gateway. No API key in Unity client. Verified by grep + security audit.
- [ ] AI agent inherits player rate limit + capability cap. Verified by integration test.
- [ ] NFT escrow on equip; release on unequip. Verified on DOS Chain test net.
- [ ] Photon Fusion 2 dedicated Server Mode build runs on Hetzner VPS, accepts 4-20 player connections in load test.
- [ ] Nakama/Postgres persists profile, inventory, quest progress, NFT lock state, level/stats, and reincarnation state across sessions.
- [ ] Multiplayer 4-20 players per zone holds 60Hz tick under load test (Fusion bots simulating 50 players for stress).

### Process (verifiable in repo state)
- [ ] All slice work merged to `main` via PR with `code-review` skill pass before merge.
- [ ] All ADRs that the slice motivated are written in `docs/adr/` (current count: 4; expect 6-10 by slice complete).
- [ ] Per-system GDDs in `docs/design/` for Combat, AI agent, Reincarnation, Time-as-currency, NFT escrow, LLM NPC. (Time-as-currency is drafted; advanced body progression is deferred.)
- [ ] Vertical Slice Report (`02-vertical-slice-report.md`) written with build velocity, playtest data, recommendation.

---

## Build Phases (target sequence)

| Phase | Target Weeks | Output |
| ---- | ---- | ---- |
| 1. Setup + first commit | T+0 to T+1 | Unity project + Photon SDK + Nakama OSS + api.dos.ai LLM contract + repo structure |
| 2. Networked player + zone | T+1 to T+4 | 1 zone Photon Fusion 2 multiplayer, Hunter skin spawn, minimal ARPG controller first, Opsive UCC evaluated after baseline |
| 3. NPC + LLM dialogue | T+4 to T+8 | Convai NPC in hub town, server-validated intent flow |
| 4. Quest + dungeon | T+8 to T+12 | 1 quest line + 1 dungeon + 1 boss (LLM dialogue) |
| 5. Reincarnation + level/stat persistence | T+12 to T+16 | Death -> SECOND token -> reincarnation flow, current-body reset, profile/stat persistence |
| 6. Time-as-currency | T+16 to T+18 | Body time meter, one earn source, one spend sink, zero-time reincarnation trigger |
| 7. NFT integration | T+18 to T+22 | Hunter skin equip + escrow on DOS Chain test net via thirdweb |
| 8. AI agent offline | T+22 to T+25 | Server-side agent that farms designated area for offline player |
| 9. Polish + playtest | T+25 to T+27 | Bug fixes, playtest sessions, vertical slice report |

These are estimates. Real velocity will be measured during slice and updated.

---

## Success Recommendation Outcomes

When slice is complete, decision tree:

- **PROCEED** to alpha milestone if: all acceptance criteria met, playtest sentiment positive, build velocity sustainable.
- **PIVOT** to revised design if: technical works but core loop falls flat (e.g., AI agent autoplay feels invisible, reincarnation feels punishing, time-as-currency feels like a nuisance timer).
- **KILL** if: tech stack proves unworkable solo (e.g., LLM cost runs 10x budget, dedicated server hosting infeasible).

---

## Out-of-band Decisions Required Before Phase Start

| Decision | Phase Blocked | JOY Owner |
| ---- | ---- | ---- |
| SECOND token economy (cost per reincarnation, source, sink) | Phase 5 | JOY (input later) |
| BodyTime economy tuning (drain, earn, spend, transfer, conversion rules) | Phase 6 | JOY (input later) |
| Hunter NFT integration approach (Option 1 vs Hybrid 1+3) | Phase 6 | JOY (input later) |
| Voice NPC vendor | NOT in slice | Defer |
| Hetzner VPS specs | Phase 8 (load test) | JOY |
| Photon Fusion 2 license tier (post-Cloud-free-20-CCU) | Post-slice | JOY |

---

## Reference Material

- High-level architecture: [docs/ARCHITECTURE.md](../ARCHITECTURE.md)
- Pillars: [01-pillars.md](01-pillars.md)
- Concept: [00-game-concept.md](00-game-concept.md)
- ADRs (current): [docs/adr/](../adr/)
- Setup runbook: [.claude/NEXT_STEPS.md](../../.claude/NEXT_STEPS.md)
- MetaDOS reference: `D:\Projects\MetaDOS` (read-only, BR template, extract patterns only)
