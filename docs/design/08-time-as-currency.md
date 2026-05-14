# Time-as-Currency

*Status: Draft*
*Created: 2026-05-14*
*Author: Codex*
*Last Verified: 2026-05-14 against MetaDOS wiki and SECOND SPAWN concept docs*
*Implements Pillar: Time is life, time is money*

> **Quick reference** - Layer: `Core Economy` - Priority: `Vertical Slice` - Key deps: `Reincarnation`, `Cultivation`, `Loot`, `Supabase persistence`, `Server-authoritative gameplay`

---

## Summary

Time-as-Currency is a signature MetaDOS mechanic adapted for SECOND SPAWN. Time is both a survival resource attached to the current body and a spendable currency used for tactical choices, recovery, and progression pressure.

In MetaDOS Battle Royale, time was the player's life timer and could be earned from enemies or special locations, then spent on weapons, supplies, or teammates. In SECOND SPAWN, the same concept becomes a persistent ARPG pressure system tied to synthetic-body lifespan, reincarnation, and offline-agent risk.

---

## Design Intent

The player should feel that time is not an abstract clock. It is the body's remaining operating life.

Every major decision should quietly ask:

- Do I spend time now to get stronger?
- Do I conserve time to survive the dungeon?
- Do I risk letting my offline agent farm longer?
- Do I reincarnate now with a controlled cost, or push the current body until it collapses?

---

## Player Fantasy

Your body is a rented vessel with a countdown built into its biology. You can earn more time by fighting, looting, completing objectives, or extracting Nibirium energy. You can spend time to buy supplies, stabilize allies, power certain systems, or delay body failure.

The fantasy is not "gold with another name." It is "your life is liquid."

---

## Core Rules

1. Each active body has a `BodyTime` value.
2. `BodyTime` counts down during active danger states.
3. `BodyTime` can be earned from combat, objective rewards, and special world sources.
4. `BodyTime` can be spent on selected gameplay actions.
5. If `BodyTime` reaches zero, the body dies and reincarnation flow begins.
6. Time changes are server-authoritative.
7. The client never grants or spends time directly.
8. LLM NPCs and offline agents may request time-related actions only through validated intents.

---

## SECOND SPAWN Adaptation

| MetaDOS Battle Royale Concept | SECOND SPAWN Adaptation |
| ---- | ---- |
| Time is a match survival timer | Time is synthetic-body lifespan and combat pressure. |
| Loot time from knocked-down enemies | Earn time from enemies, Nibirium nodes, quests, dungeon objectives, or cultivation events. |
| Spend time on weapons, ammo, armor, supplies | Spend time on field services, emergency stabilization, dungeon shortcuts, body repairs, or agent automation policies. |
| Give time to knocked-down teammates | Transfer time to party members or stabilize a dying body. |
| Running out of time means death | Running out of time triggers body death and reincarnation. |

---

## Vertical Slice Scope

The first vertical slice should implement a very small version:

- One `BodyTime` meter on the player.
- Time decreases only inside a designated danger area or dungeon room.
- Killing enemies or completing a small objective grants time.
- A test vendor or shrine lets the player spend time on one useful service.
- Reaching zero time triggers the reincarnation placeholder flow.
- Offline AI agent activity can consume time while farming.

No marketplace, tokenomics, or complex exchange rate is required for the first pass.

---

## Currency Model

`BodyTime` and `SECOND token` are related but not the same thing.

| Resource | Meaning | Scope | Persistence |
| ---- | ---- | ---- | ---- |
| `BodyTime` | Remaining operating life of the current body | Moment-to-moment gameplay | Dies with the body unless converted by a rule |
| `SECOND token` | Reincarnation and economy token | Account / wallet economy | Persists across bodies |
| Cultivation tier | Durable consciousness progression | Character identity | Partially carries across reincarnation |

Design rule: `BodyTime` creates tactical pressure. `SECOND token` gates reincarnation economy. Do not merge them unless a future ADR explicitly changes the economy.

---

## Spend Candidates

| Spend | Slice? | Notes |
| ---- | ---- | ---- |
| Stabilize self near death | Yes | Gives the player a clear reason to spend time. |
| Stabilize party member | Later | Needs party flow. |
| Buy emergency supplies | Yes | Use one simple vendor/shrine. |
| Open dungeon shortcut | Later | Good for risk/reward once dungeon layout exists. |
| Extend offline agent session | Later | Strong tie to AI agent 24/7, but too risky for first implementation. |
| Body repair / delay reincarnation | Later | Must not erase the reincarnation pillar. |

---

## Earn Candidates

| Earn Source | Slice? | Notes |
| ---- | ---- | ---- |
| Enemy kill | Yes | Keep value small and server-owned. |
| Objective completion | Yes | Clear reward moment. |
| Nibirium node | Later | Connects to cultivation economy. |
| Quest reward | Later | Needs quest persistence. |
| Party assist | Later | Needs party and contribution rules. |
| Offline agent farming | Later | Needs abuse limits and activity log. |

---

## Server Authority

Every time mutation must be validated by server-side code.

Allowed intent shapes:

- `RequestSpendBodyTime`
- `RequestTransferBodyTime`
- `RequestClaimBodyTimeReward`
- `RequestReincarnateAfterTimeExpired`

The server verifies:

- Player identity and body ownership.
- Current zone and danger state.
- Source of reward or spend.
- Cooldowns and anti-abuse limits.
- Whether the body is already dead or reincarnating.

---

## AI Agent Interaction

Offline agents can interact with time only through the same validated intent model as human players.

Design constraints:

- Agent cannot spend time on irreversible actions without a policy chosen by the player.
- Agent activity should make time risk visible in the return log.
- Agent should not farm infinite time. Earn and spend rates must be capped.
- Agent death from time expiration triggers the same reincarnation flow as player death.

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
| ---- | ---- | ---- | ---- |
| Current `BodyTime` | HUD | Real-time or tick-smoothed | Always in danger zones, optional in safe hub |
| Time gain/loss events | Floating text + log | On event | Combat, vendor, shrine, objective |
| Time spend confirmation | Modal or hold-to-confirm | On irreversible spend | Any spend that can cause death risk |
| Time expiration warning | HUD pulse + audio | Threshold based | Under 20%, under 10%, under 5% |
| Agent time usage | Activity log | On player return | Offline agent session summary |

---

## Open Questions

| Question | Owner | Timing | Current Lean |
| ---- | ---- | ---- | ---- |
| Does `BodyTime` tick down everywhere or only in danger zones? | JOY | Before implementation | Danger zones first. |
| Can players transfer time in solo slice? | JOY | Before party feature | Defer until party exists. |
| Can `BodyTime` convert to `SECOND token` or vice versa? | JOY | Economy design phase | Keep separate for now. |
| How visible should the `In Time` inspiration be? | JOY | Narrative pass | Mechanic inspiration only, not direct theme copy. |

---

## Cross-References

| This Document References | Target Doc | Specific Element Referenced | Nature |
| ---- | ---- | ---- | ---- |
| Game identity | `00-game-concept.md` | Unique hooks and MetaDOS lineage | Design dependency |
| Pillar rules | `01-pillars.md` | Time-as-currency, server-authoritative gameplay | Rule dependency |
| Slice scope | `02-vertical-slice-spec.md` | Reincarnation and AI agent acceptance | Scope dependency |
| Systems map | `03-systems-index.md` | Economy and reincarnation systems | Build dependency |
| Cultivation | `04-cultivation-system.md` | Durable progression across reincarnation | Data dependency |

## External References

- [MetaDOS Wiki: Time as Currency](https://wiki.metados.com/gameplay/time-as-currency) - original MetaDOS mechanic where time is both survival timer and currency.
- `In Time` (2011) - mechanic inspiration for time as life and spendable resource. SECOND SPAWN adapts the idea through synthetic-body lifespan and does not copy the film's setting.
