# TIME and SECOND Economy

*Status: Prototype implemented under legacy `BodyTime` field names*
*Created: 2026-05-14*
*Author: Codex*
*Last Verified: 2026-05-17 against Nakama `BodyTime` runtime, Unity HUD, and reincarnation debug flow*
*Implements Pillar: TIME is life, SECOND is money*

> **Quick reference** - Layer: `Core Economy` - Priority: `Vertical Slice` - Key deps: `Reincarnation`, `Loot`, `Profile persistence`, `Server-authoritative gameplay`

---

## Summary

TIME is the life medium that humans and Frames need to live. SECOND is
the unit, currency, and tokenized measure used to store, transfer, reward, and
spend TIME.

The current prototype still uses `BodyTime` in code, RPC names, and debug UI.
That name is now an implementation detail. Player-facing docs should use TIME
and SECOND.

Canonical relationship:

> TIME is the medium. SECOND is how the world counts it.

---

## MetaDOS Lineage

MetaDOS is the tournament layer of the same technology stack:

1. Humans enter AMB cocoons.
2. They control MetaDOS-registered Hunter Frames.
3. Those Frames run on TIME measured in SECOND.
4. SECOND can be earned, spent, looted, displayed, and awarded.
5. The spectacle trains both Hunter Frames and their agent brains.

SECOND SPAWN adapts that same economy outside the tournament layer. Frames,
including Hunter-derived combat Frames, now operate in the real world. TIME is
no longer just a match timer. It is body survival, tactical pressure, and
contested value.

---

## Design Intent

The player should feel that TIME is not an abstract clock. It is loaded life.

Every major decision should quietly ask:

- Do I spend TIME now to get stronger?
- Do I conserve TIME to survive the dungeon?
- Do I risk letting my offline agent farm longer?
- Do I spend SECOND to reincarnate now, or push the current body until it collapses?

The fantasy is not "gold with another name." It is "your life is measurable."

---

## Core Rules

1. Each active actor body has loaded TIME measured in SECOND.
2. TIME / SECOND mutations are server-authoritative.
3. Loaded TIME can decrease in danger states or other approved contexts.
4. SECOND can be earned from approved combat, objective, or world sources.
5. Loaded TIME can be spent on selected gameplay actions.
6. Reaching zero loaded TIME kills the body and starts the reincarnation flow.
7. Offline agents interact with TIME only through player policy and validated intents.
8. LLM NPCs and connected agents may request TIME-related actions only through validated intents.

---

## Body Terminology

| Term | Meaning |
| ---- | ---- |
| Frame | Bio-synthetic human body grown to hold TIME, host an agent brain, and accept a neural imprint |
| Actor body | Broad term for any Frame or biological vessel represented as a world actor |
| Hunter Frame | Combat-focused Frame registered, trained, or derived from MetaDOS |
| NPC body | Actor body governed by NPC policy |
| Player current body | Actor body currently inhabited or controlled by a player consciousness |

Frames are not pure robots. Not every Frame is a Hunter. Hunter should be
reserved for Frames tied to MetaDOS, combat archetypes, or Hunter-derived role
identity.

---

## Vertical Slice Scope

The first vertical slice should implement a small version:

- One TIME meter measured in SECOND on the player HUD.
- TIME decreases only inside a designated danger area or dungeon room.
- Killing enemies or completing a small objective grants SECOND or restores TIME.
- A test vendor or shrine lets the player spend TIME on one useful service.
- Reaching zero TIME triggers the reincarnation placeholder flow.
- Offline AI agent activity can consume TIME while farming.

No marketplace, tokenomics, or complex exchange rate is required for the first
pass.

---

## Prototype Implementation Status

Implemented:

- Nakama `secondspawn_bodytime_event` validates prototype earn, spend, drain,
  and debug fatal-drain events.
- Prototype `BodyTime` is stored on the current body profile with remaining
  seconds, max seconds, and danger drain rate.
- Prototype `BodyTime` cannot be changed on a dead body before reincarnation.
- Duplicate earn events are rate-limited by source cooldown.
- Prototype `BodyTime` reaching zero marks the current body dead.
- Unity `CharacterMemorySync` can call the `BodyTime` RPC and apply the returned
  profile state to the authoritative local player.
- Unity prototype HUD displays `BodyTime` / TIME and lifecycle.
- Unity debug panel can exercise earn, spend, drain, fatal drain, and
  reincarnation while combat rewards are still absent.

Not implemented:

- Real enemy kills or objective completion rewards.
- Real spend sink inside normal play.
- Transfer between party members.
- Player-vs-player or contested-zone TIME loot.
- Direct wallet-to-body TIME top-up.
- Final tuning for drain, earn, and spend values.

---

## Economy Model

| Resource | Meaning | Scope | Persistence |
| ---- | ---- | ---- | ---- |
| TIME | Life medium loaded into the current body | Moment-to-moment gameplay | Body-bound once loaded |
| SECOND | Unit, currency, and tokenized measure of TIME | Account / wallet / reward economy | Persists across bodies |
| Level and stats | Current-body progression | Body power | Reset or partially carry only if a future rule says so |

Design rule: TIME creates tactical pressure. SECOND is the readable unit of
account for rewards, reincarnation, and costs.

---

## Spend Candidates

| Spend | Slice? | Notes |
| ---- | ---- | ---- |
| Stabilize self near death | Yes | Gives the player a clear reason to spend TIME. |
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
| Quest reward | Later | Needs quest persistence. |
| Party assist | Later | Needs party and contribution rules. |
| Offline agent farming | Later | Needs abuse limits and activity log. |

---

## Server Authority

Every TIME / SECOND mutation must be validated by server-side code.

Allowed concept intents:

- `RequestSpendTime`
- `RequestTransferTime`
- `RequestClaimSecondReward`
- `RequestReincarnateAfterTimeExpired`

Prototype implementation names may still use `BodyTime` until code is renamed.

The server verifies:

- Player identity and body ownership.
- Current zone and danger state.
- Source of reward or spend.
- Cooldowns and anti-abuse limits.
- Whether the body is already dead or reincarnating.

---

## AI Agent Interaction

Offline agents can interact with TIME only through the same validated intent
model as human players.

Design constraints:

- Agent cannot spend TIME on irreversible actions without a policy chosen by the player.
- Agent activity should make TIME risk visible in the return log.
- Agent should not farm infinite SECOND. Earn and spend rates must be capped.
- Agent death from TIME expiration triggers the same reincarnation flow as player death.

---

## UI Requirements

| Information | Display Location | Update Frequency | Condition |
| ---- | ---- | ---- | ---- |
| Current TIME measured in SECOND | HUD | Real-time or tick-smoothed | Always in danger zones, optional in safe hub |
| TIME gain/loss events | Floating text + log | On event | Combat, vendor, shrine, objective |
| TIME spend confirmation | Modal or hold-to-confirm | On irreversible spend | Any spend that can cause death risk |
| TIME expiration warning | HUD pulse + audio | Threshold based | Under 20%, under 10%, under 5% |
| Agent TIME usage | Activity log | On player return | Offline agent session summary |

---

## Open Questions

| Question | Owner | Timing | Current Lean |
| ---- | ---- | ---- | ---- |
| Does TIME tick down everywhere or only in danger zones? | JOY | Before implementation | Danger zones first. |
| Can players transfer TIME in solo slice? | JOY | Before party feature | Defer until party exists. |
| Can players loot measured TIME from other users? | JOY + Codex | Before PvP or contested-zone prototype | Allow only after server-validated combat or zone events. |
| Can wallet SECOND directly top up loaded TIME? | JOY | Economy design phase | Defer until anti-abuse model is explicit. |
| How visible should the `In Time` inspiration be? | JOY | Narrative pass | Mechanic inspiration only, not direct theme copy. |

---

## Cross-References

| This Document References | Target Doc | Specific Element Referenced | Nature |
| ---- | ---- | ---- | ---- |
| Game identity | `00-game-concept.md` | Unique hooks and MetaDOS lineage | Design dependency |
| Pillar rules | `01-pillars.md` | TIME / SECOND economy, server-authoritative gameplay | Rule dependency |
| Slice scope | `02-vertical-slice-spec.md` | Reincarnation and AI agent acceptance | Scope dependency |
| Systems map | `03-systems-index.md` | Economy and reincarnation systems | Build dependency |
| Level/stat progression | `10-character-profile-agent-memory.md` | Current body profile state | Data dependency |

## External References

- [MetaDOS Wiki: Time as Currency](https://wiki.metados.com/gameplay/time-as-currency) - original MetaDOS mechanic where time is both survival timer and currency.
- `In Time` (2011) - mechanic inspiration for time as life and spendable resource. SECOND SPAWN adapts the idea through synthetic-body lifespan and does not copy the film's setting.
