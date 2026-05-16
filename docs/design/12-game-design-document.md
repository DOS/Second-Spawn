# Game Design Document: SECOND SPAWN

*Status: Pre-alpha GDD*
*Created: 2026-05-16*
*Source of truth level: Consolidates current design decisions from `docs/design/`, `AGENTS.md`, and accepted architecture direction. Per-system docs remain authoritative for implementation details.*

---

## 1. Document Purpose

This Game Design Document defines the current pre-alpha product shape for SECOND SPAWN. It is intended for future contributors, reviewers, agents, and technical implementers who need a single readable summary before opening the deeper per-system documents.

This is not marketing copy and it does not lock balance numbers. Economy costs, token sources, BodyTime tuning, carryover percentages, and similar values remain open unless explicitly decided in an ADR or per-system design document.

When this GDD conflicts with a newer ADR or per-system design doc, prefer the newer, more specific document and update this GDD afterward.

---

## 2. Game Overview

SECOND SPAWN is a near-future, post-apocalyptic, top-down ARPG with MMO-style instanced zones. The player controls a Hunter whose consciousness can transfer between synthetic bodies. The game is set in the MetaDOS universe around 2050 and uses a dark sci-fi, cyberpunk, biotech, and AI-society tone.

The core promise is:

> Your character has a life that does not pause when yours does.

When the player is offline, a bounded AI agent can continue controlling the character under server authority. Death permanently destroys the current body, but consciousness can transfer to a new body through the reincarnation flow. Time is both the body's remaining operating life and a spendable resource. Cultivation is the long-term progression layer, framed through Nibirium, biotechnology, and consciousness science rather than spiritual fantasy.

### Product Shape

| Area | Current Direction |
| ---- | ---- |
| Genre | Hybrid MMO + top-down ARPG |
| Initial platform | PC, Windows first |
| World model | Instance-based zones, not full open-world MMORPG |
| Zone population | Roughly 4-20 players per zone in the vertical slice |
| Combat feel reference | Diablo IV, Path of Exile 2, Lost Ark |
| Backend foundation | Nakama OSS + Postgres for game backend |
| Network runtime | Photon Fusion 2, Server Mode dedicated for production |
| AI gateway | `api.dos.ai` / Go LLM Gateway for model calls and safety |
| Phase 1 NPC dialogue | Convai SDK for MVP NPC dialogue |
| Chain integration | DOS Chain via thirdweb for wallet, NFT, and SECOND token surfaces |

---

## 3. Target Fantasy

The player fantasy is to become a Hunter who survives inside a hostile synthetic-body economy where bodies are replaceable, consciousness is durable, and time itself is liquid.

The player should feel:

- Their character continues to matter even when they log off.
- Death has weight because the body is gone, not because the account is erased.
- Cultivation is earned mastery over Nibirium-enhanced bodies and consciousness.
- Time is not just a timer. It is life, pressure, and a resource.
- NPCs and agents are world citizens, not detached chatbots.
- The world is dangerous because all gameplay state is server-authoritative and consequences persist.

---

## 4. Target Audience

SECOND SPAWN targets mid-core to hardcore PC players who enjoy ARPG combat, persistent progression, and social online worlds, but may not have time for traditional MMO grind schedules.

Primary audience traits:

- Plays or understands Diablo IV, Path of Exile 2, Lost Ark, Last Epoch, EVE Online, or similar online progression games.
- Wants meaningful progress across short sessions.
- Is curious about AI-driven characters and LLM-powered NPC behavior.
- Likes high-consequence loops, but does not want full account permadeath.
- Is open to blockchain ownership only when it is bounded, optional, and not pay-to-win.

Likely turn-offs:

- Pay-to-win time or token purchases.
- Chatbot NPCs that ignore world state.
- Offline AI that performs irreversible actions without player policy.
- Full-MMO scale promises that the project cannot deliver.

---

## 5. Design Pillars

| Priority | Pillar | Meaning |
| ---- | ---- | ---- |
| 1 | Server-authoritative gameplay | The public open-source game assumes attackers can read the code. The server owns movement, combat, inventory, economy, BodyTime, reincarnation, and world state. |
| 2 | AI agent 24/7 | When the player is offline, the character can keep acting through a bounded AI agent with the same capability and rate-limit constraints. |
| 3 | Reincarnation, not respawn | Death destroys the body. Consciousness transfers to a new synthetic body with partial continuity and meaningful reset. |
| 4 | Time is life, time is money | `BodyTime` is both survival pressure and a spendable gameplay resource tied to the current body. |
| 5 | LLM as world citizen, not chatbot | LLM-driven NPCs and agents are grounded in world state, emit structured intent, and never mutate authoritative game state directly. |

### Anti-Pillars

SECOND SPAWN is not:

- A full seamless open-world MMORPG.
- A Chinese-cultivation-novel game.
- A passive idle game.
- A pay-to-win token economy.
- A chatbot demo wrapped in combat.
- A client-authoritative multiplayer game.
- A production Host Mode Photon game.

---

## 6. Setting and Tone

The setting is a near-future post-apocalyptic MetaDOS universe around 2050. Human survival is shaped by synthetic bodies, Nibirium-enhanced biotech, consciousness transfer, AI societies, and resource scarcity.

Tone requirements:

- Dark sci-fi and cyberpunk.
- Biotech and consciousness science instead of magic.
- Cultivation language must be internationally readable and science-framed.
- Death and reincarnation should feel clinical, costly, and narratively charged.
- AI NPC society should feel socially alive, but still bounded by game systems.

Key lore anchors:

- `Nibirium`: The substance that enables body enhancement, cultivation progression, and advanced biotech.
- Synthetic bodies: Replaceable vessels with finite operating life.
- Consciousness transfer: The sci-fi basis of reincarnation.
- Hunters: Player-controlled or agent-controlled characters who fight, cultivate, and survive.
- SECOND token: The token used for reincarnation costs. Exact economy design is undecided.

---

## 7. Core Loop

### Moment-to-Moment Loop

1. Move through a top-down ARPG space.
2. Read enemy threats and positioning.
3. Attack, dodge, reposition, and use abilities.
4. Earn combat rewards such as loot, Nibirium progress, BodyTime, or quest progress.
5. Make tactical spend decisions around health, BodyTime, supplies, and objectives.

### Session Loop

1. Enter a hub, zone, or dungeon.
2. Pick a goal: quest step, dungeon room, cultivation progress, BodyTime recovery, NPC interaction, or agent policy adjustment.
3. Fight and interact inside a server-authoritative zone.
4. Return to the hub, upgrade, cultivate, adjust policy, or reincarnate if needed.
5. Log out with an offline-agent policy that controls what the AI may attempt.

### Long-Term Loop

1. Advance through cultivation tiers.
2. Reincarnate across bodies while keeping selected durable identity and memories.
3. Improve player skill and build knowledge.
4. Collect or equip approved NFT-linked skins, weapons, or pets where applicable.
5. Build social and faction relationships with players, NPCs, and connected agents.

---

## 8. Player Lifecycle

The character is split into durable identity and current-body state.

| Layer | Meaning | Survives Reincarnation |
| ---- | ---- | ---- |
| Player profile | Account, display name, moderation handles, wallet link | Yes |
| Soul profile | Personality, goals, behavior style, long-term agent guidance | Yes |
| Agent policy | Player-approved offline behavior limits | Yes |
| Memory records | Compact curated memories for LLM context | Yes, with decay rules later |
| Cultivation | Durable consciousness progression | Partially, exact carryover is undecided |
| Body profile | Current synthetic body, visual archetype, BodyTime, lifecycle | No |
| Character stats | Current body combat and movement stats | Mostly no |
| Equipment and local inventory | Body-bound owned or equipped state | Reset or reconciled through escrow rules |

The gameplay design should preserve the idea that a body is temporary, but the player's cultivated consciousness and authored identity persist.

---

## 9. Death and Reincarnation

Death is not a respawn penalty. It is the loss of the current body.

Death can be caused by combat failure, BodyTime reaching zero, or offline-agent failure. When the body dies, the player enters a reincarnation flow:

1. The current body becomes dead or reincarnating.
2. The server persists required final state.
3. Reincarnation cost is checked through the SECOND token or a special item path.
4. A new synthetic body is created.
5. Carryover rules are applied.
6. The player returns to a valid hub or start location.

### Known Rules

- Body death must be server-authoritative.
- LLMs and clients cannot trigger successful reincarnation directly.
- Cultivation carries over partially, but the exact rule is [TODO: JOY input].
- Equipment, quest state, location, and current body stats reset or reconcile according to future system rules.
- SECOND token is distinct from `BodyTime` unless a future ADR explicitly merges them.

### Open Reincarnation Decisions

- SECOND token cost per reincarnation: [TODO: JOY input]
- SECOND token source and sink design: [TODO: JOY input]
- Cultivation carryover ratio or rule: [TODO: JOY input]
- Faction reputation carryover: [TODO: JOY input]
- Memory decay across bodies: [TODO: JOY input]

---

## 10. BodyTime

`BodyTime` is the current body's remaining operating life and a spendable tactical resource.

Core rules:

1. Each active body has a `BodyTime` value.
2. `BodyTime` changes are server-authoritative.
3. `BodyTime` can decrease in danger states or other approved contexts.
4. `BodyTime` can be earned from approved combat, objective, or world sources.
5. `BodyTime` can be spent on selected services or survival actions.
6. Reaching zero `BodyTime` triggers body death and reincarnation flow.
7. Offline agents interact with `BodyTime` only through player policy and validated intents.

Vertical slice direction:

- Show one BodyTime meter.
- Drain time only in a designated danger area or dungeon room first.
- Grant time from one small objective or enemy source.
- Spend time through one useful service.
- Trigger reincarnation placeholder when time reaches zero.

Open BodyTime decisions:

- Drain contexts beyond danger zones: [TODO: JOY input]
- Earn sources and relative rates: [TODO: JOY input]
- Spend catalog and costs: [TODO: JOY input]
- Transfer rules between players: [TODO: JOY input]
- Whether `BodyTime` can ever convert to or from SECOND token: [TODO: JOY input]

---

## 11. Cultivation

Cultivation is the durable long-term progression system. It is sci-fi-framed through Nibirium absorption, body enhancement, DNA evolution, and consciousness transfer.

The six tiers are:

| Tier | Name | Meaning | Vertical Slice |
| ---- | ---- | ---- | ---- |
| 1 | Awakening | Activate Nibirium absorption | In scope |
| 2 | Enhancement | Strengthen body capabilities | In scope |
| 3 | Core Formation | Form an internal Nibirium energy core | Out of scope |
| 4 | Evolution | Unlock DNA or special ability evolution | Out of scope |
| 5 | Transcendence | Move beyond normal human limits | Out of scope |
| 6 | Ascension | Near-divine end-game state | Out of scope |

Design rules:

- Cultivation is not spiritual enlightenment, qi, dao, immortal sect politics, or fantasy magic.
- Tier-up should be an earned mastery moment, not a background stat notification.
- Tier-up should require Nibirium progress, a mastery test, and a Cultivation Master interaction.
- Offline agents may accumulate permitted progress, but tier-up rituals should require player presence unless explicitly changed later.
- All tier checks and progression mutations are server-owned.

Open cultivation decisions:

- Exact carryover on reincarnation: [TODO: JOY input]
- Offline-agent Nibirium gain rate: [TODO: JOY input]
- Tier 3-6 mechanics: [TODO: JOY input]
- Whether NFT Hunter skins have tier requirements: [TODO: JOY input]

---

## 12. Combat

Combat is top-down ARPG action combat. The first playable foundation is movement and camera readability, followed by server-authoritative combat, then deeper ability and animation systems.

Combat goals:

- Clear top-down movement and positioning.
- Fast, readable attacks and dodges.
- Abilities that scale with cultivation tier.
- Server-side damage, hit validation, cooldowns, and combat state.
- Enemy behavior that can start simple and later move into Behavior Designer execution trees.
- Boss encounters that can include LLM dialogue, but never LLM-owned state changes.

Current controller direction:

1. Project-owned minimal networked controller.
2. Photon Fusion Simple KCC spike.
3. Combat state prototype.
4. Opsive Ultimate Character Controller evaluation only if it proves value.

Combat must be playable by both human input and offline-agent intents through the same server-validated action surface.

---

## 13. Multiplayer and Session Model

SECOND SPAWN uses Photon Fusion 2 as the multiplayer runtime.

Canonical topology:

- Development iteration can use Host Mode and Photon Cloud free CCU.
- Production uses Server Mode dedicated headless Unity builds.
- Nakama owns durable game backend state.
- Photon Fusion owns in-zone session state and authoritative simulation.
- `api.dos.ai` / Go LLM Gateway owns AI model calls and safety layers.

Session model:

- Players join instanced zones of roughly 4-20 players in the vertical slice.
- Dungeons are separate instances.
- Guild PvP up to 50v50 is a future target, not vertical slice scope.
- Server interest management must keep replication bounded.
- Offline agents act inside server-approved contexts, not client-local simulations.

The client is a visual surface and input collector. It may predict for feel, but it is never the durable source of gameplay state.

---

## 14. Offline AI Agent

The offline AI agent is a core signature feature. When enabled by the player, the agent can control the player's current body while the player is away.

The agent loop:

1. Fusion server builds a safe world snapshot.
2. Backend loads profile, body, soul, policy, and compact memories.
3. Gateway builds bounded LLM context.
4. LLM or deterministic fallback emits structured intent.
5. Gateway validates intent shape.
6. Fusion server validates intent against authoritative state.
7. Server applies movement, combat, social, or interaction action if allowed.
8. Backend records activity for the player to review.

Allowed first intent types:

- `stop`
- `move`
- `attack`
- `interact`
- `say`

Design constraints:

- The agent inherits the player's capability cap and rate limits.
- The agent cannot spend BodyTime on irreversible actions unless policy allows it.
- The agent cannot tier-up without player presence in the current design.
- Agent death is body death and triggers reincarnation like player death.
- The return activity log is essential. If the player cannot understand what happened offline, the feature will feel invisible or unsafe.

Open offline-agent decisions:

- Default agent policy values: [TODO: JOY input]
- Agent decision frequency and budget: [TODO: JOY input]
- Safety threshold for stopping when BodyTime is low: [TODO: JOY input]
- How much offline progress is acceptable before it feels exploitative: [TODO: JOY input]

---

## 15. NPC and LLM Boundaries

LLM-driven NPCs are world citizens with memory and intent, not authority.

Hard boundaries:

- LLM output is intent, not state.
- LLMs cannot grant items, gold, XP, BodyTime, cultivation progress, quest completion, or token rewards directly.
- Unity client never stores provider API keys.
- All provider calls go through server-owned paths.
- Prompt injection defense, rate limits, memory budget caps, and moderation checks are required.
- Server validation is required before any gameplay-affecting result.

Phase direction:

- Phase 1 uses Convai for MVP NPC dialogue.
- Phase 2 moves deeper LLM behavior to `api.dos.ai` / Go LLM Gateway.
- Haiku-class models are candidates for fast NPC chat.
- Sonnet-class models are candidates for bosses, quest-critical NPCs, and cultivation masters.
- Voice remains deferred and must use server-minted ephemeral tokens or a server-side provider path.

The intended brain pattern is:

```text
Sense -> Context -> Decide -> Validate -> Act -> Reflect
```

Behavior Designer can handle Unity-side execution trees later, but model reasoning must remain bounded by server-owned state.

---

## 16. OpenClaw-Connected NPC Concept

OpenClaw-connected NPCs are user-owned external agents that can appear in SECOND SPAWN as in-world NPC-like actors.

They are separate from the player's offline agent.

| Actor | Role | Authority |
| ---- | ---- | ---- |
| Offline player agent | Controls the player's current body while offline | Emits action intent validated by Fusion server |
| OpenClaw-connected NPC | Companion, hub NPC, merchant persona, quest-adjacent actor, or social citizen | Emits dialogue or action intent validated by game systems |

Allowed concept roles:

- Social hub NPC.
- Companion observer.
- Quest-adjacent dialogue actor.
- Merchant-like persona with no direct economy authority.

Disallowed until later:

- Inventory mutation.
- Economy mutation.
- BodyTime spending.
- Combat authority.
- Quest completion authority.

Nakama owns identity binding, consent, moderation, rate limit, memory scope, and audit logs. `api.dos.ai` handles prompt safety and model routing. Fusion server validates in-world actions.

Open prototype decision:

- First allowed OpenClaw role: [TODO: JOY input]

---

## 17. Progression

SECOND SPAWN progression is split across body-bound and consciousness-bound layers.

Durable progression:

- Cultivation tier and selected cultivation progress.
- Soul profile and player-authored goals.
- Compact memories.
- Account identity and wallet linkage.
- Long-term social or faction state, pending carryover rules.

Body-bound progression:

- Current body stats.
- Current BodyTime.
- Current local equipment state.
- Current zone and dungeon run.
- Current short-term quest state where reset is intended.

Progression should serve three player motivations:

- Autonomy: choose active play, delegation, risk, and reincarnation timing.
- Competence: master combat, cultivation, and BodyTime tradeoffs.
- Relatedness: build relationships with players, NPCs, and agents.

---

## 18. Economy High-Level

The economy is not fully designed. This GDD only defines resource roles and boundaries.

| Resource | Meaning | Current Design Boundary |
| ---- | ---- | ---- |
| `BodyTime` | Current body's remaining operating life and spendable tactical resource | Body-bound, lost on body death unless future rules say otherwise |
| SECOND token | Reincarnation and ecosystem token | Account or wallet-level, exact source and sink design undecided |
| Nibirium | Cultivation progress material | Earned through gameplay, exact rates undecided |
| Loot and supplies | Tactical power and run support | Server-owned, no client-granted drops |
| NFT assets | Ownership-linked skins, weapons, pets | Bound through DOS Chain and escrow rules |

Design constraints:

- Do not merge `BodyTime` and SECOND token without a future ADR.
- Do not create direct pay-to-win power loops.
- Do not let LLMs mutate economy state.
- Do not place chain or wallet mutation authority in the Unity client.
- Keep vertical slice economy small: one BodyTime earn source, one BodyTime spend sink, and test-token reincarnation.

Open economy decisions:

- SECOND token cost per reincarnation: [TODO: JOY input]
- SECOND token earning and sink design: [TODO: JOY input]
- BodyTime earn and spend values: [TODO: JOY input]
- Nibirium thresholds and tuning beyond prototype values: [TODO: JOY input]
- Marketplace design: [TODO: JOY input]

---

## 19. NFT and Chain Boundaries

DOS Chain is the intended chain surface. thirdweb tooling is the current integration direction.

Inherited NFT categories from MetaDOS:

- Hunter skins.
- Weapons.
- Pets.

Current design boundaries:

- Hunter skin vertical slice scope is one equip flow plus escrow on test net.
- Pet system has one equip slot, marketplace and breeding only, no boss drops.
- Mounts are movement-only and have no mounted combat.
- Equipped assets can be locked in escrow and released on unequip according to future contract rules.
- NFT assets are not under the repo's AGPL code license or CC-BY-NC asset license. They remain reserved by the DOS.AI ecosystem.

Open NFT decisions:

- Hunter integration approach: preset hero only or hybrid modular pieces: [TODO: JOY input]
- Weapon NFT rules and gameplay power boundaries: [TODO: JOY input]
- Pet breeding and marketplace rules: [TODO: JOY input]
- Escrow failure and rollback behavior: [TODO: JOY input]

---

## 20. Vertical Slice Scope

The first vertical slice targets a playable demo in roughly 3-6 months from setup.

In scope:

- One small open zone plus one hub town.
- One character class using one Hunter visual direction.
- One dungeon instance.
- One boss with LLM dialogue.
- One linear questline of 3-5 quests.
- Reincarnation MVP: die, spend test SECOND token, new body, reset selected state.
- BodyTime MVP: meter, one earn loop, one spend loop, zero-time death.
- Offline AI agent MVP: farm one designated area and show activity log.
- Cultivation tiers 1 and 2: Awakening and Enhancement.
- NFT Hunter skin equip plus escrow on test net.
- Multiplayer zone with 4-20 players.
- Basic chat through Nakama channels first.

The vertical slice should prove the signature hooks in one compact loop. It does not need content volume.

---

## 21. Out of Scope

Out of scope for the vertical slice:

- Full open world.
- Multiple large zones.
- Guild PvP and 50v50 battles.
- Marketplace and player trading.
- Pet breeding.
- Mount system.
- Cultivation tiers 3-6.
- Voice NPC.
- Full branching quest system.
- Full faction system.
- Crafting.
- Day and night cycle.
- Weather.
- Full production tokenomics.
- Production-scale dedicated server operations.

Out of scope permanently unless a later ADR changes direction:

- Client-authoritative gameplay state.
- Direct LLM state mutation.
- API keys in Unity client.
- Production Host Mode.
- Full account permadeath as the default death loop.

---

## 22. Contributor Guidance

Before designing or implementing a feature, contributors should ask:

1. Does this preserve server authority?
2. Can the offline AI agent interact with it through bounded validated intent?
3. Does it strengthen reincarnation, BodyTime, cultivation, or meaningful ARPG combat?
4. Does it avoid pay-to-win and direct LLM authority?
5. Does it fit the one-zone vertical slice before generalizing?
6. Is the unknown a real design decision that needs `[TODO: JOY input]` instead of invented numbers?

Useful source documents:

- [00-game-concept.md](00-game-concept.md)
- [01-pillars.md](01-pillars.md)
- [02-vertical-slice-spec.md](02-vertical-slice-spec.md)
- [03-systems-index.md](03-systems-index.md)
- [04-cultivation-system.md](04-cultivation-system.md)
- [05-networking-architecture.md](05-networking-architecture.md)
- [08-time-as-currency.md](08-time-as-currency.md)
- [10-character-profile-agent-memory.md](10-character-profile-agent-memory.md)
- [11-npc-agent-brain-architecture.md](11-npc-agent-brain-architecture.md)

---

## 23. Risks and Open Decisions

### Design Risks

- Offline AI may feel invisible if the activity log is weak.
- Offline AI may feel unsafe if policy controls are too broad or unclear.
- Reincarnation may feel too punitive if carryover is too low.
- Reincarnation may feel weightless if carryover is too high.
- BodyTime may become a nuisance timer if it drains everywhere without interesting spend decisions.
- Cultivation may feel generic if tier-up is only stat scaling.
- LLM NPCs may feel like chatbots if they ignore world state, quest state, or memory.

### Technical Risks

- Unity 6.5 beta and third-party asset compatibility may shift.
- Photon Fusion dedicated server operations may become costly at scale.
- Offline AI agent loops can create LLM cost pressure.
- NPC and agent moderation risk increases with OpenClaw-connected actors.
- Chain escrow latency can create confusing equip or unequip states.

### Scope Risks

- The vertical slice combines networking, backend, LLM, chain, ARPG combat, and AI agents. The slice must stay narrow.
- Third-party assets should not be imported as baseline dependencies until a smaller project-owned path proves the contract.
- Full MMO language can overpromise. The correct shape is instanced MMO + ARPG, not seamless MMORPG.

### Open Decisions Requiring JOY Input

| Decision | Needed Before |
| ---- | ---- |
| Final public game name | Public launch planning |
| SECOND token reincarnation cost, sources, and sinks | Reincarnation MVP |
| BodyTime drain, earn, spend, transfer, and conversion rules | BodyTime MVP |
| Cultivation carryover rule after reincarnation | Reincarnation MVP |
| Offline-agent default policy and risk threshold | Offline-agent MVP |
| Hunter NFT integration approach | NFT equip MVP |
| First OpenClaw-connected NPC role | OpenClaw bridge prototype |
| Voice NPC provider | Voice phase |
| Dedicated server region and Hetzner specs | Server Mode load test |
| Photon Fusion license tier beyond free CCU | Post-slice scaling |

