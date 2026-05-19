# Gate, Dungeon, and Pioneer Charter System

*Status: Concept design*
*Created: 2026-05-19*
*Source of truth level: Design direction for dungeon discovery, first-clear prestige, and in-game dungeon revenue loops. Implementation details still require TDDs before build.*

---

## 1. Purpose

This document defines the working direction for SECOND SPAWN's Gate and dungeon economy. It turns the manga and manhwa-inspired "first clear" fantasy into a bounded in-game system that can support competitive dungeon discovery, guild prestige, and server-owned rewards without creating real-world revenue-share promises.

Working names:

- Dungeon portal: `Gate`, `Breach`, or `Shard Dungeon`
- First-clear right: `Pioneer Charter`
- In-game revenue share: `Clearance Royalty`

`Pioneer Charter` is the temporary canonical name until naming is revisited.

---

## 2. Reference Pool

Core references:

- Solo Leveling: ranked Gates, Hunters, guild pressure, public emergency stakes.
- Omniscient Reader's Viewpoint: scenario rules, hidden conditions, sponsors, and clear conditions. A major global reference for meta-system storytelling.
- Leveling with the Gods: Tower climbing, god-level stakes, regression knowledge, and rank escalation.
- The Player Who Can't Level Up: Gate and Tower pressure around player identity, plus a leveling exception hook.
- My S-Class Hunters: Hunter society, guild politics, support value, and relationship-driven S-rank networks.
- SSS-Class Revival Hunter: death-loop cost, memory continuity, skill acquisition, and emotional consequences.
- Second Life Ranker: Tower factions, inherited knowledge, guild power, and route optimization.
- Return to Player: modern Earth becoming a game-like survival scenario.

Secondary references:

- Murim Login: modern dungeon society plus a second rule layer.
- Solo Max-Level Newbie: old game knowledge becoming real-world survival advantage.
- The Advanced Player of the Tutorial Tower: tutorial tower, first-generation clear prestige, and broad global popularity.
- Kill the Hero: guild betrayal, dungeon politics, and anti-hero reward pressure.
- Return of the Disaster-Class Hero: public disaster stakes and high-rank hero return.
- Seoul Station's Necromancer: dungeon-for-cash, returner power, and modern gate normalization.

Do not use Tomb Raider King as a primary reference. It has relic and claim-right ideas, but it is not a strong tone or popularity anchor for SECOND SPAWN.

---

## 3. Common Genre Mechanics

The shared genre skeleton across these works:

| Mechanic | Common Pattern | SECOND SPAWN Adaptation |
| ---- | ---- | ---- |
| World rule layer | Modern Earth gains Gates, Towers, Scenarios, System windows, or dungeon rules | Gates are Nibiru-influenced breaches with server-owned rules and clear conditions |
| Rank ladder | Hunters, players, dungeons, monsters, items, and guilds use F/E/D/C/B/A/S or similar ranks | Use Frame rank, Gate rank, Charter rank, and later agent clearance grade |
| Leveling | Combat and clears grant level, stats, skill slots, or class evolution | Current-body level and stats grow inside the Frame, with reincarnation reset rules |
| Leveling exception | A protagonist cannot level, levels alone, regresses, copies skills, or knows hidden rules | Keep exceptions as content hooks, not baseline player power creep |
| Skill layers | Active skills, passive skills, unique skills, title skills, and hidden-condition skills | Split into Frame skills, Soul/title records, Agent behavior policy, and Gate rewards |
| Dungeon loop | Scan rank, register party, enter, discover hidden rule, clear boss/objective, receive reward | Fusion validates combat; Nakama records clear logs, rewards, and Pioneer Charter eligibility |
| Hidden knowledge | Regression, reader knowledge, game knowledge, diaries, or memory lets the hero exploit rules | Use NPC memory, Gate intel, agent logs, and previous clear records as learnable knowledge |
| Guild and association | Guilds bid for Gates, associations regulate rank, S-ranks become strategic assets | Future guilds can register Gate attempts and hold Charters, but not in the vertical slice |
| First clear prestige | First clear, no-death clear, solo clear, hidden clear, and fastest clear become social proof | Pioneer Charter starts as server-owned prestige, then may add capped in-game royalty |
| Disaster escalation | Failed Gates can break, leak monsters, or trigger public crisis | High-rank Gates can become timed regional events later |

Mechanics to avoid copying directly:

- A protagonist-only power fantasy where one player invalidates all party and guild play.
- Infinite passive income from dungeon ownership.
- Client-side System windows that grant rewards without server validation.
- Rank inflation too early, especially SSS content before the combat game is stable.
- Hidden conditions that feel random instead of learnable from world clues, memory, or NPC intel.
- Guilds existing only as villains or disposable cannon fodder. SECOND SPAWN
  needs guilds, associations, and registries to feel like useful social
  infrastructure.

---

## 4. Design Goals

- Make dungeon discovery and first clear feel important enough to create social stories.
- Give players and guilds a reason to race for new Gates without turning the game into a passive idle economy.
- Support the ranked Gate fantasy while grounding the fiction in SECOND SPAWN's sci-fi TIME, SECOND, Frame, and Nibiru continuity.
- Keep all revenue in-game. Do not frame this as external passive income, securities-like revenue share, or real-money yield.
- Make first-clear rewards server-authoritative, auditable, revocable on exploit, and resistant to LLM or client self-reporting.
- Leave room for AI agents and OpenClaw-connected actors without letting autonomous agents capture the most economically sensitive rights by themselves in the MVP.

---

## 5. Fiction

Gates are unstable Nibiru-influenced breaches where space, ruined infrastructure, memory residue, and TIME density collapse into bounded combat instances. The public sees them as dungeon portals because they have ranks, entry rules, bosses, and clear conditions. DOS Labs, local authorities, guilds, and Frame operators treat them as both disaster zones and resource claims.

A Gate has:

- A physical or projected entry point in the world.
- A rank from F to S, with room for later S+, SS, and SSS escalation.
- A stability timer or outbreak risk.
- Clear conditions.
- Server-owned reward tables.
- A recorded first-clear ledger.

When a party clears a newly discovered Gate under validated conditions, the server may issue a `Pioneer Charter`. The Charter proves that the party opened the safe exploitation path for that Gate version.

Gate society can include public or semi-public institutions:

- Association-style registries for Frame rank, Gate access, and death liability.
- Guild bids or permits for dangerous Gate attempts.
- Insurance or escrow rules for body loss, party wipes, and TIME debt.
- Reputation effects when a party abandons, stabilizes, or exploits a Gate.

---

## 6. Dungeon Tiering

Initial public-facing ranks:

| Rank | Meaning | Early Gameplay Role |
| ---- | ---- | ---- |
| F | Weak anomaly | Solo-friendly tutorial or low-risk farm |
| E | Minor breach | Entry-level party content |
| D | Local threat | First meaningful dungeon clear race |
| C | Dangerous breach | Guild preparation starts to matter |
| B | Severe breach | Requires tuned builds and coordination |
| A | Regional threat | Seasonal competition target |
| S | Critical breach | Server event, public prestige, high validation burden |

Future escalation ranks can add S+, SS, and SSS after the game has enough content, economy controls, and live-ops support. Do not add those ranks to vertical-slice tuning yet.

Rank should be a public estimate, not absolute truth. A low-ranked Frame with
rare memory, strong equipment, better AI policy, or better Gate intel can
outperform the estimate. A high-ranked Gate can hide special rules, wrong scan
data, or disaster modifiers.

---

## 7. Pioneer Charter

A `Pioneer Charter` is a server-issued in-game right attached to a specific Gate, Gate version, and clear condition.

It can grant:

- First-clear title, badge, banner, or guild record.
- Public record: first human-led clear, first party clear, first no-death clear,
  first low-TIME clear, or first hidden clear.
- A time-limited `Clearance Royalty` paid in in-game resources when other players clear that same Gate version.
- Priority entry or discovery reputation, if later systems need it.
- Lore recognition from NPCs, factions, or hub boards.

It must not grant:

- Real-world revenue share.
- Direct external cash claim.
- Permanent uncapped passive yield.
- Authority to mutate dungeon reward tables.
- Authority to bypass entry rules, combat validation, or economy ledgers.

### Suggested Clearance Royalty Shape

These numbers are placeholders for balance discussion, not implementation-ready tuning.

| Gate Rank | Candidate Royalty | Candidate Duration |
| ---- | ---- | ---- |
| F-E | 1-2 percent of in-game Gate fee pool | 7 days |
| D-C | 2-5 percent of in-game Gate fee pool | 14 days |
| B-A | 5-8 percent of in-game Gate fee pool | 30 days |
| S | Up to 10 percent of in-game Gate fee pool | Season, Gate stabilization, or live-ops event duration |

The pool can be denominated in a server-owned in-game ledger, dungeon keys, crafting materials, guild contribution points, or another approved in-game resource. Use one pool first. Do not mix wallet, token, and item flows until the economy model is explicit. SECOND-denominated royalties require a later explicit economy decision.

---

## 8. Revenue Sources Are In-Game Only

`Clearance Royalty` is an in-game reward pool. Candidate sources:

- Gate entry fee.
- Dungeon key or attunement cost.
- Retry fee after party wipe.
- Guild expedition permit.
- Crafting or stabilization material sink.
- Later marketplace tax from Gate-specific loot, if marketplace systems exist.
- Body insurance or recovery fee, only after body-loss economics are explicit.

The first implementation should use a single server-owned in-game source. Recommended MVP candidate: a small percentage of the Gate entry or dungeon-key sink paid into a non-wallet in-game ledger value until the reward resource is explicitly approved.

---

## 9. Server Authority and Ledger Requirements

The server must record enough data to prove a clear:

- `gate_id`
- `gate_version`
- `gate_seed`
- `rank`
- `party_member_ids`
- `controller_types` for human, offline agent, OpenClaw, or game AI participation
- `clear_started_at`
- `clear_completed_at`
- `objective_state_hash`
- `combat_log_hash`
- `death_count`
- `reincarnation_count`
- `reward_ledger_id`
- `pioneer_charter_id`, if granted
- `exploit_review_state`

Clients, LLMs, and connected agents never grant a Charter. They can only emit intent or display server-owned results.

---

## 10. AI Agent Fairness

SECOND SPAWN's 24/7 AI agent feature makes first-clear rights unusually sensitive. If fully autonomous agents can claim all Pioneer Charters while players sleep, the feature may feel unfair.

MVP rule recommendation:

- Human-led or human-present clears can qualify for `Pioneer Charter`.
- Offline-agent-assisted clears can earn normal loot, activity progress, and memory, but should not claim the primary economic Charter unless a future policy explicitly allows it.
- OpenClaw-connected NPCs can participate as validated actors only within their consent and rate limits. They do not own Pioneer Charters by default.
- Server UI should label clear records by control mix: `Human-led`, `Agent-assisted`, `NPC-assisted`, or `Automated`.

Future variants can experiment with separate leaderboards:

- First human-led clear.
- First agent-assisted clear.
- First solo clear.
- First no-death clear.
- First low-TIME clear.
- First hidden-condition clear.

If agent participation is allowed later, the UI should disclose it rather than
hiding it. Automated clears can be impressive, but player-led discovery should
remain socially valuable.

---

## 11. Gameplay Loop

1. A Gate appears or is discovered.
2. Players inspect rank, rules, stability, entry cost, and suspected reward type.
3. Party enters the instance.
4. Fusion server validates movement, combat, death, and objective state.
5. Nakama records dungeon run state and economy events.
6. Clear condition is met.
7. Server validates first-clear eligibility.
8. If eligible, server issues a `Pioneer Charter`.
9. Later clears contribute to the in-game `Clearance Royalty` pool until duration or cap expires.

Hidden knowledge loop:

1. A run reveals partial Gate intel through NPC rumors, previous logs, failed
   party records, or agent observations.
2. The player chooses whether to share, sell, hide, or act on that knowledge.
3. The next run can discover hidden conditions without the game feeling random.
4. The server records which hidden condition was actually met.

---

## 12. Vertical Slice Scope

The vertical slice should not implement full Pioneer Charter economics yet. It can prepare the design surface.

Recommended vertical-slice scope:

- One F or E rank test dungeon.
- One clear condition.
- One server-owned clear log.
- One non-economic first-clear badge or debug record.
- No real Clearance Royalty payouts.
- No marketplace tax.
- No agent-only Charter eligibility.

The first economy implementation should come after combat, dungeon clear validation, append-only reward ledgers, and anti-exploit review states exist.

---

## 13. Open Questions

- Should Charter ownership belong to the party, party leader, guild, or split ledger?
- Should player-inhabited NPC-like Frames carry Charter fame after reincarnation, or should it stay on the durable player profile?
- Should high-rank Gates require public registration before entry?
- Should S-rank Gates be live events with server announcements?
- What is the first in-game royalty resource: SECOND, dungeon keys, guild contribution, crafting material, or a separate non-wallet ledger?
- How should invalidated clears claw back already-paid in-game royalties?

---

## 14. Implementation Notes

Do not implement this from the Unity client first. The build order should be:

1. Server-owned dungeon run ledger.
2. Server-owned clear validation from Fusion combat and objective state.
3. Append-only reward ledger.
4. First-clear badge or non-economic Charter record.
5. Clearance Royalty pool after economy caps and abuse review exist.

This system touches combat, dungeon instance, economy, persistence, UI, guilds, offline agents, and anti-cheat. Any production implementation should get a TDD before code changes.
