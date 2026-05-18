# Game Design Document: SECOND SPAWN

*Status: Pre-alpha GDD*
*Created: 2026-05-16*
*Last updated: 2026-05-18*
*Source of truth level: Consolidates current design decisions from `docs/design/`, `AGENTS.md`, and accepted architecture direction. Per-system docs remain authoritative for implementation details.*

---

## 1. Document Purpose

This Game Design Document defines the current pre-alpha product shape for SECOND SPAWN. It is intended for future contributors, reviewers, agents, and technical implementers who need a single readable summary before opening the deeper per-system documents.

This is not marketing copy and it does not lock balance numbers. Economy costs, SECOND sources, TIME tuning, carryover percentages, and similar values remain open unless explicitly decided in an ADR or per-system design document.

When this GDD conflicts with a newer ADR or per-system design doc, prefer the newer, more specific document and update this GDD afterward.

### Modern GDD Inputs

This document follows a modern living-GDD shape rather than a static monolithic spec. The structure borrows from current GitBook and Heroic Labs guidance: start with vision, core loop, systems, content, UX, production scope, risks, and business constraints. It also reuses useful MetaDOS GDD patterns where they fit SECOND SPAWN: clear overview copy, match or session flow, currency taxonomy, cosmetics and account progression separation, and detailed feature notes once a system graduates from concept to implementation.

---

## 2. Game Overview

SECOND SPAWN is a near-future, post-disaster top-down ARPG with MMO-style instanced zones. The player controls a Hunter whose neural imprint can transfer between Frames. The game is set in the MetaDOS universe around 2050 and uses a bright sci-fi progression-fantasy tone built around systems, ranking, awakening, body limits, AI society, and TIME pressure.

The core promise is:

> Your character has a life that does not pause when yours does.

When the player is offline, a bounded AI agent can continue controlling the character under server authority. Death permanently destroys the current Frame, but the surviving imprint can transfer to a new Frame through the reincarnation flow. TIME is the life medium that keeps Frames running, and SECOND is the unit used to measure, store, and trade it. Current-body level and stats are the vertical-slice progression baseline.

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
| AI model service | `api.dos.ai` for model calls and safety |
| Phase 1 NPC dialogue | Convai SDK for MVP NPC dialogue |
| Chain integration | DOS Chain via thirdweb for wallet, NFT, and SECOND surfaces |

### Current Implementation Snapshot - 2026-05-18

This snapshot tracks what exists in the running prototype today. It is not a
promise that the same UI or tuning will ship.

| Area | Current State |
| ---- | ---- |
| Unity scene | `ZoneTest_Hub` can enter Play Mode and spawn a Fusion local player. |
| Player visible stats | The prototype HUD shows level, HP, energy, attack, defense, agility, prototype `BodyTime` / TIME, lifecycle, SECOND balance, and reincarnation count. |
| Player profile sync | Unity loads the Nakama profile and applies current-body stats, prototype `BodyTime` / TIME, lifecycle, SECOND balance, reincarnation count, and visual key to the authoritative local `NetworkPlayer`. |
| Default player body | New profiles start at level 1 with server-selected stats from the prototype body archetype pool. |
| TIME loop | Nakama supports prototype earn, spend, drain, duplicate-earn cooldown, zero-time death, and activity logging. Current implementation field names still use `BodyTime`. |
| Reincarnation loop | Nakama supports dead-body reincarnation into a fresh prototype body. The current test balance is 7 days of SECOND and the current test cost is 5 days. |
| NPC/actor profiles | NPC-like actors can have their own body, stats, traits, soul, memory, relationships, policy, runtime, and activity records. |
| Permanent NPCs | `ZoneTest_Hub` can seed and display 10 permanent prototype NPC Frames from Nakama, each with fixed body identity and visual variant. |
| Prototype NPC brain | Permanent NPC brains sense nearby actors, call the model-backed Nakama decision path, speak through model-selected `say` intent, and persist model speech through Nakama memory and relationship records. |
| Backend foundation | Nakama owns durable game profile state, model-decision RPCs, and server-side intent validation before gameplay systems consume LLM output. |
| Not implemented yet | Real combat damage, enemy loot, quest rewards, production HUD, player-vs-player time loot, wallet escrow, dungeon boss, and dedicated server deployment. |

---

## 3. Target Fantasy

The player fantasy is to become a Hunter who survives inside a Frame economy where bodies are replaceable, identity imprints are durable, TIME is a life medium, and SECOND is the measurable unit of that life.

The player should feel:

- Their character continues to matter even when they log off.
- Death has weight because the body is gone, not because the account is erased.
- Level and stats give readable ARPG growth while deeper body progression is redesigned later.
- TIME is not just a timer. It is the material form of life pressure and a
  resource.
- SECOND can become contested because it measures and transfers TIME. Future
  combat or zone rules may let players loot measured TIME from other users, but
  only through validated server-authoritative outcomes.
- NPCs and agents are world citizens, not detached chatbots.
- The world is dangerous because all gameplay state is server-authoritative and consequences persist.

---

## 4. Target Audience

SECOND SPAWN targets mid-core PC players who enjoy ARPG combat, visible system progression, and social online worlds, but may not have time for traditional MMO grind schedules.

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
| 1 | Server-authoritative gameplay | The public open-source game assumes attackers can read the code. The server owns movement, combat, inventory, TIME / SECOND economy, reincarnation, and world state. |
| 2 | AI agent 24/7 | When the player is offline, the character can keep acting through a bounded AI agent with the same capability and rate-limit constraints. |
| 3 | Reincarnation, not respawn | Death destroys the Frame. A surviving neural imprint transfers to a new Frame with partial continuity and meaningful reset. |
| 4 | TIME is life, SECOND is money | TIME is the life medium bodies need to live. SECOND is the unit and currency used to measure, store, transfer, spend, and reward TIME. |
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

SECOND SPAWN takes place in the MetaDOS universe around the 2050 era, after
DOS Labs proved that human life-time, weapon testing, Hunters, and global
spectacle could become one economy. This is not another MetaDOS battle royale.
MetaDOS is the tournament layer of the same technology stack: humans enter AMB
cocoons, control Hunter Frames, and compete for SECOND. SECOND SPAWN
is what happens when that cocoon, Frame, agent-brain, and TIME economy stops
being only spectacle and becomes the survival infrastructure of the real world.

The world should feel readable, energetic, and progression-driven rather than
overly bleak. The core audience is closer to fans of system, ranking,
awakening, tower, dungeon, and level-up stories in manga, manhwa, and manhua
than to fans of dystopian survival fiction. Danger exists, but the fantasy is
about growth, agency, clever rules, and becoming stronger inside a visible
system.

Human survival is shaped by biotech scarcity, body markets, AI governance,
post-disaster infrastructure, and factions arguing over who should own the
technologies that measure, extend, transfer, or spend life.

Tone requirements:

- Bright near-future sci-fi with high-stakes progression, not bleak dystopian
  survival.
- System-story readability: ranks, gates, quests, TIME, SECOND, reincarnation,
  offline-agent policy, and NPC knowledge should feel like legible rules the
  player can learn and exploit.
- Manga, manhwa, and manhua progression fantasy references are useful for
  pacing and player fantasy: Solo Leveling, Omniscient Reader's Viewpoint,
  Tower of God, and Skeleton Soldier Could Not Protect the Dungeon.
- Biotech and consciousness science replace magic, but should still serve the
  same emotional role as awakening, second chances, hidden rules, and growth.
- Death and reincarnation should feel costly and dramatic, but not oppressive.
- AI NPC society should feel socially alive, colorful, and system-aware while
  still bounded by game rules.
- Nibiru may appear as the historical event and source material behind TIME,
  but TIME is the player-facing life medium. Do not add Nibiru-derived XP,
  cultivation tiers, rituals, or vertical-slice progression.

### MetaDOS Continuity / Timeline

This timeline is inherited from the MetaDOS GDD and adapted for SECOND SPAWN:

| Year | Anchor | SECOND SPAWN Relevance |
| ---- | ---- | ---- |
| 2030 | A meteorite called Nibiru explodes in an airburst over Canada, changing the environment in the affected region. | The Nibiru event becomes the origin point for TIME research, disaster recovery, extraction, and corporate control. |
| 2030s | DOS Labs becomes the only legitimate company able to manage and exploit Nibiru-derived material. | DOS Labs gains the leverage to reshape energy, biotech, warfare, and public policy. |
| 2040 | DOS Labs discovers that Nibiru-derived material can sustain human life through AMB technology, using CT-like body scanning and an under-arm biological monitor that displays remaining life time. | TIME becomes a measurable life medium, and SECOND becomes the unit used to count it. |
| 2040s | Avax wants the life-extension technology to broadly benefit humanity. Dr.J betrays him, monopolizes DOS Labs, and turns the corporation toward profit and influence. Avax survives and escapes with core technologies. | The world inherits a fracture between open survival technology and corporate life-time control. |
| 2050 | DOS Labs creates MetaDOS, the Tournament of the Century, held every 4 years and watched globally. Players enter AMB cocoons and control Hunter Frames in a tournament layer where winners can receive prolonged life time, money, fame, or other resources. | MetaDOS normalizes SECOND as prize, currency, and measured life-time, while training Hunter Frames and agent brains through spectacle. |
| After MetaDOS | SECOND SPAWN begins from the consequences of that system rather than repeating the tournament format. | The focus moves from cocoon-controlled tournament Frames to real-world Frames, reincarnation, AI agents, NPC societies, and contested zones. |

### Key Lore Anchors

- Frames: Bio-synthetic human bodies grown to hold TIME, host an agent brain,
  and accept a neural imprint. Frames are not pure robots.
- Actor bodies: Any Frame or biological vessel represented as a world actor,
  including NPC bodies, player-inhabited bodies, offline-agent bodies, and
  OpenClaw-connected social actors.
- Hunter Frames: Combat-focused Frames registered, trained, or derived from the
  MetaDOS tournament system. Not every Frame is a Hunter.
- Neural imprint transfer: The sci-fi basis of reincarnation.
- Nibiru-derived material: The hidden source behind clean nuclear-scale energy,
  TIME extraction, and life-extension breakthroughs. Public-facing docs should
  prefer Nibiru, TIME, and SECOND instead of adding a separate mineral name.
  This material is lore and infrastructure, not the current progression
  currency.
- DOS Labs: The corporate power that industrialized TIME extraction, AMB life-time
  monitoring, MetaDOS spectacle, weapons testing, and Hunter recruitment.
- Avax and Dr.J: A founding fracture in DOS Labs. Their conflict frames the
  larger question of whether life-extension and consciousness technologies are
  public survival tools or corporate control mechanisms.
- AMB cocoon and life-time monitor: The MetaDOS-era technology that let humans
  remain in controlled pods while operating Hunter Frames and seeing remaining
  life-time as a countable system value.
- TIME: The life medium humans and Frames need to operate. TIME is treated
  like matter or fuel in this universe, not just an abstract clock.
- SECOND: The unit, currency, and tokenized measure of TIME. Reincarnation,
  rewards, costs, and loaded body life should be expressed in SECOND.
- Time loot: A future PvP or contested-zone rule can allow measured TIME,
  denominated in SECOND, to be taken from another user after server-validated
  combat, escrow, or zone events. Clients, LLMs, and connected agents must never
  self-report or grant this loot.
- Hunter cosmetics, cards, badges, weapons, and pets: MetaDOS inheritance
  candidates for account, NFT, cosmetic, and collection layers. They should not
  bypass server authority or current economy rules.

### SECOND SPAWN Story Premise

After MetaDOS, DOS Labs no longer only sells spectacle. It has shown the world
that TIME can be measured, wagered, rewarded, and used to recruit the strongest
survivors. SECOND SPAWN asks what happens when the same world pushes beyond
cocoon play and lets Frames, including Hunter-derived combat Frames,
become real-world survival infrastructure.

A Frame is not a character slot. It is a TIME-powered bio-synthetic human body
with its own agent brain and capacity for a neural imprint. A human in an AMB
cocoon can override and train a compatible Frame. When the human disconnects,
the Frame can continue through bounded agent behavior. A Hunter Frame is the
combat/tournament subset registered or derived from MetaDOS. Outside the
tournament layer, many other Frames exist as civilians, workers, companions,
merchants, or social citizens. When a Frame dies, the Second Spawn Protocol
transfers the surviving neural imprint, agent memory, policy, and identity
signature into a new compatible Frame. Reincarnation is data transfer, not
spiritual resurrection.

The narrative tension is practical, not mystical: if bodies can be manufactured,
inhabited, retired, and replaced, then every faction wants to decide who gets a
body, who owns its time, what memories survive, and whether an agent acting in a
body is a tool, a citizen, or a liability.

### Vertical Slice Narrative Hooks

Initial slice framing should remain flexible, but the first playable arc can
use these hooks:

- First hub: A guarded settlement or converted body facility near a contested
  TIME-rich Nibiru-influenced zone, where surviving humans, Frame citizens, Hunters,
  technicians, and agent-run NPCs trade quests, rankings, rumors, and survival
  services.
- First Frame: The player wakes inside a Frame that was prepared,
  recovered, or reassigned by the hub. The exact source remains open, but it
  should make the player feel they inherited a vessel with an agent brain,
  limits, and possible history rather than creating a blank hero. It may be
  Hunter-derived for combat readability, but not every Frame in the world should
  be called a Hunter.
- Why TIME matters: The body's operating life is visible, limited, and measured
  in SECOND. TIME is both a survival medium and a tactical resource for
  services, recovery, access, or risk decisions.
- What NPCs know: Hub NPCs understand that MetaDOS made life-time public and
  valuable. Some remember DOS Labs propaganda, some distrust Frames,
  some treat offline agents as workers, and some fear consciousness transfer as
  identity theft.
- Faction tension: DOS Labs loyalists, Avax-aligned technologists, independent
  Hunters, local settlement authorities, black-market body brokers, and
  self-directed AI/NPC communities can all want different rules for bodies,
  memory, TIME, and SECOND.
- First questline: The player proves the new body can survive, recovers
  TIME or body records from a danger area, meets an NPC who questions the
  player's identity, and sees evidence that offline agents can help or make
  costly mistakes.

### First 30 Minutes Narrative

The first playable experience should teach the premise through action, not a
lore wall:

1. The player wakes in a hub clinic, body facility, or settlement as a Frame
   that has just accepted a neural imprint.
2. A hub NPC confirms the player is not in a normal MetaDOS match. MetaDOS was
   the tournament layer. This is the real-world layer using the same AMB, Frame,
   TIME, SECOND, and agent-brain stack.
3. The UI shows TIME measured in SECOND. The player learns that the Frame is
   alive, limited, and running on loaded TIME.
4. The player enters a nearby danger zone, fights one simple enemy, and earns
   SECOND or restores a small amount of TIME.
5. The player spends TIME once on a useful service, shortcut, repair,
   stabilization, or objective interaction.
6. The player meets an NPC or agent-run Frame who hints that some Frames had
   lives, policies, or agent memories before a player ever entered them.
7. A controlled death or near-death moment introduces the Second Spawn Protocol:
   the Frame is lost, but the surviving imprint and agent memory can move into a
   new compatible Frame through a SECOND-gated flow.
8. Before logout, the player sets a basic offline-agent policy so the Frame can
   continue under bounded server-validated behavior.

---

## 7. Core Loop

### Moment-to-Moment Loop

1. Move through a top-down ARPG space.
2. Read enemy threats and positioning.
3. Attack, dodge, reposition, and use abilities.
4. Earn combat rewards such as loot, level/stat progress, SECOND, TIME recovery, or quest progress.
5. Make tactical spend decisions around health, TIME, supplies, and objectives.

### Session Loop

1. Enter a hub, zone, or dungeon.
2. Pick a goal: quest step, dungeon room, level/stat progress, TIME recovery, NPC interaction, or agent policy adjustment.
3. Fight and interact inside a server-authoritative zone.
4. Return to the hub, upgrade, adjust policy, or reincarnate if needed.
5. Log out with an offline-agent policy that controls what the AI may attempt.

### Long-Term Loop

1. Advance level and body-specific stats.
2. Reincarnate across bodies while keeping selected durable identity and memories.
3. Improve player skill and build knowledge.
4. Collect or equip approved NFT-linked skins, weapons, or pets where applicable.
5. Build social and faction relationships with players, NPCs, and connected agents.

### Controls, Camera, and Game Feel

SECOND SPAWN should feel like a modern top-down ARPG first, with networking and AI systems supporting that feel instead of replacing it.

Current direction:

- Camera: top-down or high isometric combat view with strong battlefield readability.
- Movement: direct character movement suitable for mouse-and-keyboard first, controller later.
- Combat verbs: move, basic attack, use skill, dodge or reposition, interact, talk, and stop.
- Targeting: readable enemy threat indicators and clear intent feedback.
- Session feel: compact, responsive, and tactical rather than slow MMORPG tab-target combat.

Open feel decisions:

- Exact camera height and angle: [TODO: prototype]
- Mouse movement vs WASD default: [TODO: JOY input]
- Dodge roll, dash, or movement skill baseline: [TODO: prototype]
- Ability slot count for the first Hunter class: [TODO: prototype]

---

## 8. Player Lifecycle

The player is not a blank avatar shell. The player is a durable consciousness profile that enters a current NPC-like Frame at spawn. That Frame can already have its own body-level constraints, stat bias, characteristics, memory hooks, neural imprint, loaded TIME measured in SECOND, lifecycle state, and agent runtime state. If the Frame is combat-registered or derived from the MetaDOS tournament system, it can be treated as a Hunter Frame; otherwise it remains a broader actor body.

The design must support many NPCs and many player-controlled bodies using the same broad actor-profile shape. The difference is ownership and authority: a player may inhabit and control one current body, while world NPCs, offline agents, and OpenClaw-connected actors are governed by their own policy and validation paths.

The character is split into durable identity and current-body state.

| Layer | Meaning | Survives Reincarnation |
| ---- | ---- | ---- |
| Player profile | Account, display name, moderation handles, wallet link | Yes |
| Soul profile | Personality, goals, behavior style, long-term agent guidance | Yes |
| Agent policy | Player-approved offline behavior limits | Yes |
| Memory records | Compact curated memories for LLM context | Yes, with decay rules later |
| Agent runtime | Bounded operational counters, recent activity, fallback tracking | Yes, bounded |
| Body profile | Current Frame or biological body, visual archetype, loaded TIME, lifecycle | No |
| Body characteristics | Current-body tendencies such as curiosity, courage, discipline, aggression, and sociability | Mostly no |
| Character stats | Current body combat and movement stats | Mostly no |
| Equipment and local inventory | Body-bound owned or equipped state | Reset or reconciled through escrow rules |

The gameplay design should preserve the idea that a body is temporary, but the player's authored identity and selected durable profile layers persist.

### Actor Profile Bundle

Every important NPC-like actor should eventually resolve to a bundle with clear ownership:

| Bundle Piece | Purpose |
| ---- | ---- |
| `FrameIdentity` | Public-facing name, callsign, role, faction title, and reputation summary |
| `BodyProfile` | Current vessel, archetype, visual key, lifecycle, loaded TIME, and body-bound state |
| `BodyPresentation` | Appeal band, visual tags, intimidation tags, and presentation style |
| `CharacterStats` | Body-bound core stats, derived combat stats, movement, health, energy, defense, and level values |
| `CharacterTraits` | Numeric personality tendencies for human-believable agent decisions, including empathy, honesty, cunning, loyalty, ambition, self-preservation, courage, discipline, aggression, mercy, curiosity, sociability, paranoia, greed, pragmatism, and vengefulness |
| `BodyStory` | Short origin, role, conflict, and rumor hooks for the specific body |
| `SoulProfile` | Durable identity, name, drive, temperament, goals, and moral boundaries |
| `RelationshipLedger` | Per-target social state such as trust, affection, hostility, fear, respect, debt, and familiarity |
| `MemoryRecord` | Bounded memories used by LLM and deterministic agent context |
| `AgentPolicy` or NPC policy | What the actor is allowed to attempt |
| `AgentRuntime` | Counters, fallback visibility, and recent operational state |
| `AgentActivity` | Player-facing or operator-facing audit summary |

Server-side systems decide which parts are editable, inherited, generated, or
read-only for each actor type. Profession and social state should be split
across the bundle: public role and reputation in `FrameIdentity`, apparent age
and body markers in `BodyProfile`, passive first-impression surface in
`BodyPresentation`, usable job abilities in combat/profession systems until a
real `FrameSkill` layer is needed, per-target social state in
`RelationshipLedger`, memory evidence in `MemoryRecord`, and relationships that
become core motivations in `SoulProfile`.

`FrameSkill` and `FrameAgents` are names we may still use later, but they are
not required backend layers for the MVP. For OpenClaw-connected NPCs, the
external OpenClaw instance owns its own agent files and routines. The game only
stores the control binding, policy, allowed intent schema, bounded context, and
runtime audit state.

The current NPC design target is a human-believable agent model, not a narrow
scripted quest-giver model. Each important NPC should have a stable trait vector,
`FrameSoul`, `FrameMemory`, relationship ledger, BodyTime state, current needs,
mood or stress context, body presentation data, and validated action surface. See
[13-human-believable-npc-agent-model.md](13-human-believable-npc-agent-model.md).

Target character-model taxonomy:

The MVP backend uses six canonical body-bound core stats: `strength`, `agility`,
`endurance`, `perception`, `focus`, and `presence`. The older serialized keys
`vitality`, `force`, and `resilience` remain compatibility aliases until the
Unity networked prototype stats are renamed safely.

- Core stats: body-bound gameplay numbers such as strength, agility, endurance,
  perception, focus, and presence. Do not add wisdom as a core stat and do not
  expose accuracy as a player-facing stat for MVP.
- Secondary stats: derived gameplay values such as HP, energy, attack power,
  skill power, armor rating, five elemental resistance ratings, dodge rating,
  dodge chance, crit chance, crit damage, attack speed, move speed, cooldown
  reduction, BodyTime efficiency, sensor range, social read, and instruction
  stability.
- Social attributes: non-combat social surface such as appeal, reputation,
  faction standing, and relationship values. Appeal is a presentation attribute,
  not a beauty score and not a buildable combat stat.
- Identity: name, callsign, profession, gender identity, pronouns, identity age,
  soul continuity age, and memory span.
- Body profile: apparent age, chronological body age, and body sex or synthetic
  marker.
- Body presentation: appeal band, appeal tags, visual tags, intimidation tags,
  and presentation style.
- Relationships: multi-axis per-target values such as familiarity, trust,
  affection, hostility, fear, respect, debt, and rivalry. Do not collapse them
  into one `like_score`.

Prototype body-model decisions:

- A new account spawns into a server-selected NPC-like Frame from the
  permanent NPC Frame pool. The pool entries reference approved body archetypes
  and provide stable source actor IDs such as `npc-crossline-hunter-5104`.
- Each body can carry pre-existing story hooks, traits, stat bias, weapon
  visual, soul defaults, and a seed memory before the player enters it.
- When a player enters one of these Frames, Nakama persists the chosen source
  Frame as a `player_body` actor profile so it can keep body-level stats,
  memory, activity history, and identity metadata.
- Reincarnation currently selects a fresh body archetype after death and SECOND
  payment. Candidate rejection is deferred until the reincarnation UI exists.
- The body profile also stores animation capability flags so a model without a
  jump animation can still move while skipping broken jump visual triggers.

Open body-model decisions:

- Whether the player can reject a candidate body during reincarnation: [TODO: JOY input]
- How much body-level memory survives once the player leaves or the body dies: [TODO: JOY input]

---

## 9. Death and Reincarnation

Death is not a respawn penalty. It is the loss of the current body.

Death can be caused by combat failure, loaded TIME reaching zero, or offline-agent failure. When the body dies, the player enters a reincarnation flow:

1. The current body becomes dead or reincarnating.
2. The server persists required final state.
3. Reincarnation cost is checked through SECOND or a special item path.
4. A new compatible Frame is created or assigned.
5. The surviving neural imprint, agent memory, policy, and selected identity layers are transferred under carryover rules.
6. The player returns to a valid hub or start location.

### Known Rules

- Body death must be server-authoritative.
- LLMs and clients cannot trigger successful reincarnation directly.
- Equipment, quest state, location, and current body stats reset or reconcile according to future system rules.
- TIME is the life medium. SECOND is the unit and currency used to measure, store, and transfer TIME.
- Reincarnation should consume enough SECOND to create a new playable TIME package.
- Candidate reincarnation package is 5-7 days of playable body lifetime. The vertical-slice recommendation is 7 days by default, then tune toward 5 days only if early testing shows the loop is too forgiving.
- Current prototype values are intentionally test-only: 7 days starting SECOND
  balance and 5 days reincarnation cost.

### Open Reincarnation Decisions

- Default reincarnation package: 5 days or 7 days: [TODO: JOY input]
- Whether the SECOND cost directly seeds the new body's TIME, or only gates body creation while TIME is assigned separately: [TODO: JOY input]
- SECOND source and sink design beyond reincarnation: [TODO: JOY input]
- Faction reputation carryover: [TODO: JOY input]
- Body selection and candidate reroll rules: [TODO: JOY input]
- Memory decay across bodies: [TODO: JOY input]
- Reincarnation grace period after zero TIME: [TODO: JOY input]

---

## 10. TIME and SECOND

TIME is the life medium that humans and Frames need to live. SECOND is
the unit, currency, and tokenized measure used to store, transfer, reward, and
spend TIME. The current prototype still uses `BodyTime` in code and debug UI,
but player-facing docs should treat TIME and SECOND as the canon terms.

Canonical relationship:

> TIME is the medium. SECOND is how the world counts it.

Core rules:

1. Each active Frame has loaded TIME measured in SECOND.
2. TIME / SECOND changes are server-authoritative.
3. Loaded TIME can decrease in danger states or other approved contexts.
4. SECOND can be earned from approved combat, objective, or world sources.
5. Loaded TIME can be spent on selected services or survival actions.
6. Reaching zero loaded TIME triggers body death and reincarnation flow.
7. Offline agents interact with TIME only through player policy and validated intents.

Vertical slice direction:

- Show one TIME meter measured in SECOND.
- Drain time only in a designated danger area or dungeon room first.
- Grant time from one small objective or enemy source.
- Spend time through one useful service.
- Trigger reincarnation placeholder when time reaches zero.

Current prototype status:

- The HUD meter and lifecycle fields are visible in Play Mode.
- Nakama applies earn, spend, drain, and zero-time death through
  `secondspawn_bodytime_event`.
- A prototype debug panel can exercise the loop before combat rewards exist.
- Real enemy rewards and player time-loot are still future server-authoritative
  rules, not client-side grants.

Open TIME / SECOND decisions:

- Drain contexts beyond danger zones: [TODO: JOY input]
- Earn sources and relative rates: [TODO: JOY input]
- Spend catalog and costs: [TODO: JOY input]
- Transfer rules between players: [TODO: JOY input]
- Whether wallet SECOND can directly top up loaded TIME, and under what anti-abuse limits: [TODO: JOY input]

---

## 11. Level and Stats Progression

Level and current-body stats are the only approved progression layer for the
vertical slice.

Design rules:

- Level and stat mutations are server-owned.
- Level is body-bound unless a future reincarnation design explicitly carries a
  portion forward.
- Stats should support readable ARPG combat first: health, energy, attack,
  defense, speed, and survivability.
- Advanced body or soul progression is deferred until a fresh design pass.
- Do not implement cultivation tiers, Nibiru-derived XP, tier-up rituals, or
  Cultivation Master progression in the current slice.

Open progression decisions:

- Level curve and stat scaling: [TODO: prototype]
- Which stat values reset on reincarnation: [TODO: reincarnation MVP]
- Whether any level-derived account milestone survives body death: [TODO: JOY input]
- Future advanced body progression direction: [TODO: post-slice brainstorm]

---

## 12. Combat

Combat is top-down ARPG action combat. The first playable foundation is movement and camera readability, followed by server-authoritative combat, then deeper ability and animation systems.

Combat goals:

- Clear top-down movement and positioning.
- Fast, readable attacks and dodges.
- Abilities that scale with level and stats.
- Server-side damage, hit validation, cooldowns, and combat state.
- Enemy behavior that can start simple and later move into Behavior Designer execution trees.
- Boss encounters that can include LLM dialogue, but never LLM-owned state changes.

Current controller direction:

1. Project-owned minimal networked controller.
2. Photon Fusion Simple KCC spike.
3. Combat state prototype.
4. Opsive Ultimate Character Controller evaluation only if it proves value.

Combat must be playable by both human input and offline-agent intents through the same server-validated action surface.

Combat content that still needs its own feature spec:

- First Hunter class kit.
- Enemy taxonomy: trash, ranged, elite, boss, neutral hazard.
- Damage formula and defense formula.
- Skill cooldown and resource model.
- Boss phase rules and LLM dialogue trigger rules.

---

## 13. Multiplayer and Session Model

SECOND SPAWN uses Photon Fusion 2 as the multiplayer runtime.

Canonical topology:

- Development iteration can use Host Mode and Photon Cloud free CCU.
- Production uses Server Mode dedicated headless Unity builds.
- Nakama owns durable game backend state.
- Photon Fusion owns in-zone session state and authoritative simulation.
- `api.dos.ai` owns AI model calls and safety layers.

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
3. Nakama builds bounded LLM context.
4. LLM or deterministic fallback emits structured intent.
5. Nakama validates intent shape.
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
- The agent cannot spend TIME on irreversible actions unless policy allows it.
- Agent death is body death and triggers reincarnation like player death.
- The return activity log is essential. If the player cannot understand what happened offline, the feature will feel invisible or unsafe.

Open offline-agent decisions:

- Default agent policy values: [TODO: JOY input]
- Agent decision frequency and budget: [TODO: JOY input]
- Safety threshold for stopping when loaded TIME is low: [TODO: JOY input]
- How much offline progress is acceptable before it feels exploitative: [TODO: JOY input]

---

## 15. NPC and LLM Boundaries

LLM-driven NPCs are world citizens with memory and intent, not authority.

Hard boundaries:

- LLM output is intent, not state.
- LLMs cannot grant items, gold, XP, TIME, SECOND, level/stat progress, quest completion, or token rewards directly.
- Unity client never stores provider API keys.
- All provider calls go through server-owned paths.
- Prompt injection defense, rate limits, memory budget caps, and moderation checks are required.
- Server validation is required before any gameplay-affecting result.

Phase direction:

- Phase 1 uses Convai for MVP NPC dialogue.
- Phase 2 moves deeper LLM behavior to `api.dos.ai`.
- Haiku-class models are candidates for fast NPC chat.
- Sonnet-class models are candidates for bosses and quest-critical NPCs.
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
| OpenClaw-connected NPC | Companion, hub NPC, merchant persona, quest-adjacent actor, or social citizen | Pulls game context and emits dialogue or action intent validated by game systems |

The game does not need to mirror an OpenClaw workspace. OpenClaw `.md` files
remain outside the game and define the external agent's private reasoning. The
game exposes only structured Frame context and accepts only structured intent
requests.

Game-owned context exposed to OpenClaw:

- `FrameIdentity`: public name, callsign, role, profession, faction title, and reputation.
- `FrameSoul`: bounded motivation, style, goals, and moral boundaries.
- `FrameBody`: current vessel, stats, TIME, lifecycle, equipment, and world snapshot.
- `FrameMemory`: bounded summaries and relationship facts.
- `FramePolicy`: owner-approved constraints.
- `FrameTools`: request schema only, not executable tools.
- `FrameHeartbeat`: connection and last-decision status.

Minimum control binding:

| Field | Meaning |
| ---- | ---- |
| `frame_actor_id` | NPC-like Frame controlled by this bridge |
| `controller_type` | `game_ai`, `player`, `offline_agent`, or `openclaw` |
| `connected_agent_id` | Stable external OpenClaw agent ID |
| `owner_player_id` | Player who connected the agent |
| `connection_status` | Connected, disconnected, degraded, suspended, or blocked |
| `consent_scope` | What the owner allows |
| `moderation_state` | Active, limited, suspended, or blocked |
| `rate_limit_profile` | Token and action limits |

Allowed concept roles:

- Social hub NPC.
- Companion observer.
- Quest-adjacent dialogue actor.
- Merchant-like persona with no direct economy authority.

Disallowed until later:

- Inventory mutation.
- Economy mutation.
- TIME spending.
- Combat authority.
- Quest completion authority.

Nakama owns identity binding, consent, moderation, rate limit, control binding,
memory scope, and audit logs. `api.dos.ai` handles prompt safety and model
routing when the game hosts model calls. Fusion server validates in-world
actions.

Open prototype decision:

- First allowed OpenClaw role: [TODO: JOY input]

---

## 17. Progression

SECOND SPAWN progression is split across body-bound and consciousness-bound layers.

Durable progression:

- Soul profile and player-authored goals.
- Compact memories.
- Account identity and wallet linkage.
- Long-term social or faction state, pending carryover rules.

Body-bound progression:

- Current body level and stats.
- Current loaded TIME.
- Current local equipment state.
- Current zone and dungeon run.
- Current short-term quest state where reset is intended.

Progression should serve three player motivations:

- Autonomy: choose active play, delegation, risk, and reincarnation timing.
- Competence: master combat, level/stat growth, and TIME tradeoffs.
- Relatedness: build relationships with players, NPCs, and agents.

---

## 18. Economy High-Level

The economy is not fully designed. This GDD only defines resource roles and boundaries.

| Resource | Meaning | Current Design Boundary |
| ---- | ---- | ---- |
| TIME | Life medium loaded into humans and Frames, measured in SECOND | Body-bound once loaded, lost on body death unless future rules say otherwise |
| SECOND | Unit, currency, and tokenized measure of TIME, used for rewards and reincarnation | Account or wallet-level, exact source and sink design undecided |
| Loot and supplies | Tactical power and run support | Server-owned, no client-granted drops |
| NFT assets | Ownership-linked skins, weapons, pets | Bound through DOS Chain and escrow rules |

Design constraints:

- Do not let clients, LLMs, or connected agents mutate TIME or SECOND directly.
- Do not create direct pay-to-win power loops.
- Do not let LLMs mutate economy state.
- Do not place chain or wallet mutation authority in the Unity client.
- Keep vertical slice economy small: one SECOND earn source, one TIME spend sink, and test-token reincarnation.

### TIME and SECOND Relationship

TIME and SECOND are two views of the same life economy:

- TIME is the life medium that keeps a body operating.
- SECOND is the unit and currency used to count, transfer, reward, and spend TIME.
- Reincarnation consumes SECOND and results in a new body with a playable TIME package.
- The working package range is 5-7 days. Seven days is the recommended vertical-slice default because it gives new players and offline-agent behavior enough room for testing.
- Direct wallet-to-body TIME top-up should not exist until the anti-abuse and economy model is explicit.

Open economy decisions:

- Default reincarnation package: 5 days or 7 days of playable lifetime: [TODO: JOY input]
- Whether SECOND directly seeds body TIME or only gates body creation: [TODO: JOY input]
- SECOND earning and sink design beyond reincarnation: [TODO: JOY input]
- TIME earn, spend, drain, and transfer values: [TODO: JOY input]
- Marketplace design: [TODO: JOY input]

### Loot, Items, and Cosmetics

Loot and itemization should support the ARPG loop without diluting the reincarnation pillar.

Vertical slice direction:

- Use a small item set first: basic weapon, armor or module, consumable, and one quest item.
- Gear found during play is body-bound unless a future rule says otherwise.
- Current-body gear should mostly reset or be reconciled on reincarnation.
- Durable cosmetics, achievements, titles, badges, and account progression can survive body death.
- NFT-linked assets must stay optional and bounded by server-side equip rules.

MetaDOS patterns worth reusing later:

- Clear separation between gameplay gear and cosmetic surfaces.
- Account-level badges, trackers, banners, frames, emotes, and profile presentation.
- Rarity language for cosmetics, not raw combat power.
- Feature-specific docs once a cosmetic or account system becomes implementation-ready.

Open item decisions:

- First prototype weapon archetypes are one-hand sword, staff, hammer, and
  crossbow through the body archetype pool. Final combat tuning is still open.
- Loot rarity names and count: [TODO: JOY input]
- Which gear survives reincarnation, if any: [TODO: JOY input]
- Cosmetic rarity model: [TODO: JOY input]

### UI, UX, and Onboarding

UI must make the signature systems legible before adding cosmetic depth.

Required vertical-slice UX flows:

- First login and character/profile bootstrap.
- TIME HUD measured in SECOND and low-time warning.
- Death and reincarnation screen.
- SECOND cost confirmation for reincarnation.
- Offline-agent policy setup.
- Offline-agent return report with recent activity.
- Basic NPC dialogue UI.
- Wallet/NFT equip status only where needed for the vertical slice.

First-time player experience:

1. Spawn in a safe hub by entering a current NPC-like Frame.
2. Learn movement and camera.
3. See TIME measured in SECOND but do not immediately panic.
4. Enter one danger area where TIME matters.
5. Fight one enemy, earn SECOND or spend TIME once.
6. Meet one NPC or boss dialogue moment.
7. Experience a controlled death or reincarnation tutorial.
8. Set a basic offline-agent policy before logging out.

Accessibility requirements for future passes:

- Readable TIME warnings beyond color alone.
- Remappable controls.
- Subtitle support for NPC dialogue and voice.
- UI scale options.
- Avoid time-critical wallet prompts in combat.

### Art and Audio Direction

Current direction:

- Visual style: bright near-future sci-fi, system-story progression fantasy,
  post-disaster environments, and stylized readability for production speed.
- Environment: ruined high-tech zones, synthetic-body facilities, biotech decay, hub town contrast.
- Character readability: silhouettes and ability effects must stay readable from top-down camera distance.
- Audio: tense biotech/sci-fi ambience, clear combat hits, distinct TIME warning sounds, restrained AI/NPC voice use.

Open art/audio decisions:

- Exact stylization level for vertical slice: [TODO: prototype]
- First hub visual identity: [TODO: JOY input]
- First dungeon visual identity: [TODO: JOY input]
- Music direction and reference tracks: [TODO: JOY input]

### Content Inventory for Vertical Slice

The first slice should list content volume explicitly so scope cannot silently inflate.

| Content Type | Target Count | Notes |
| ---- | ---- | ---- |
| Hub area | 1 | Small safe zone with NPC, vendor or shrine, reincarnation entry point |
| Danger zone | 1 | TIME drain is visible here |
| Dungeon | 1 | Short instance with one readable objective |
| Player class | 1 | One playable body archetype set, server-selected from the prototype pool |
| Basic enemies | 1-2 | Enough to test combat and rewards |
| Elite or boss | 1 | Includes LLM dialogue trigger if feasible |
| Questline | 1 | 3-5 steps |
| NPCs | 10 prototype permanent Frames plus 2-3 authored quest roles | The 10 permanent Frames prove the actor-profile and AI-agent substrate. The vertical-slice quest should still focus on a smaller authored cast. |
| Items | 4-8 | Weapon, armor or module, consumable, objective item, optional cosmetic |
| Offline-agent policies | 2-3 | Observe, safe farm, stop below threshold |
| Reincarnation flow | 1 | Placeholder but server-authoritative |

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
- Reincarnation MVP: die, spend test SECOND, new body, reset selected state.
- TIME MVP: meter measured in SECOND, one earn loop, one spend loop, zero-time death.
- Offline AI agent MVP: farm one designated area and show activity log.
- Basic level and stat progression for the current body.
- NFT Hunter skin equip plus escrow on test net.
- Multiplayer zone with 4-20 players.
- Basic chat through Nakama channels first.

The vertical slice should prove the signature hooks in one compact loop. It does not need content volume.

### Current Completion Notes

Already proven in prototype:

- Fusion player spawn and movement in `ZoneTest_Hub`.
- Profile-backed player stats visible on the prototype HUD.
- Nakama profile, body, soul, memory, policy, runtime, activity, prototype
  `BodyTime` / TIME, and reincarnation storage paths.
- Prototype NPC/agent brain loop with gateway model decision and deterministic
  fallback.
- Proactive NPC social path: nearby actor sensing, model-selected `say` intent,
  Nakama target validation, and Nakama memory or relationship persistence.
- TIME and reincarnation smoke path through debug controls.

Still required before this feels like a game:

- First server-authoritative combat damage path.
- First enemy or objective that grants SECOND or restores TIME.
- First spend sink that is part of normal play instead of debug UI.
- First death/reincarnation presentation flow.
- First questline, dungeon, boss, and grounded dialogue beat.
- First player-vs-player or contested-zone time-loot rule, if included in the
  slice.

---

## 21. Out of Scope

Out of scope for the vertical slice:

- Full open world.
- Multiple large zones.
- Guild PvP and 50v50 battles.
- Marketplace and player trading.
- Pet breeding.
- Mount system.
- Advanced body progression.
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
3. Does it strengthen reincarnation, TIME / SECOND tradeoffs, level/stat progression, or meaningful ARPG combat?
4. Does it avoid pay-to-win and direct LLM authority?
5. Does it fit the one-zone vertical slice before generalizing?
6. Is the unknown a real design decision that needs `[TODO: JOY input]` instead of invented numbers?

Useful source documents:

- [00-game-concept.md](00-game-concept.md)
- [01-pillars.md](01-pillars.md)
- [02-vertical-slice-spec.md](02-vertical-slice-spec.md)
- [03-systems-index.md](03-systems-index.md)
- [04-cultivation-system.md](04-cultivation-system.md) - deferred advanced body progression
- [05-networking-architecture.md](05-networking-architecture.md)
- [08-time-as-currency.md](08-time-as-currency.md)
- [10-character-profile-agent-memory.md](10-character-profile-agent-memory.md)
- [11-npc-agent-brain-architecture.md](11-npc-agent-brain-architecture.md)
- [13-human-believable-npc-agent-model.md](13-human-believable-npc-agent-model.md)

---

## 23. Risks and Open Decisions

### Design Risks

- Offline AI may feel invisible if the activity log is weak.
- Offline AI may feel unsafe if policy controls are too broad or unclear.
- Reincarnation may feel too punitive if carryover is too low.
- Reincarnation may feel weightless if carryover is too high.
- TIME may become a nuisance timer if it drains everywhere without interesting spend decisions.
- Level/stat progression may feel generic if combat and reincarnation do not create meaningful decisions.
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
| Default reincarnation package: 5 days or 7 days of playable lifetime | Reincarnation MVP |
| Whether SECOND directly seeds body TIME or only gates body creation | Reincarnation MVP |
| SECOND sources and sinks beyond reincarnation | Reincarnation MVP |
| TIME drain, earn, spend, transfer, and conversion rules | TIME MVP |
| Future advanced body progression direction | Post-slice brainstorm |
| Offline-agent default policy and risk threshold | Offline-agent MVP |
| Hunter NFT integration approach | NFT equip MVP |
| First OpenClaw-connected NPC role | OpenClaw bridge prototype |
| Voice NPC provider | Voice phase |
| First class kit and skill slot count | Combat prototype |
| First hub and dungeon visual identity | Vertical slice art pass |
| Dedicated server region and Hetzner specs | Server Mode load test |
| Photon Fusion license tier beyond free CCU | Post-slice scaling |
