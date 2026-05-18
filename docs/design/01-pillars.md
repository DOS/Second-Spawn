# Game Pillars: SECOND SPAWN

*Status: Draft (bootstrapped from CLAUDE.md USPs)*
*Last Updated: 2026-05-14*

---

## Core Fantasy

You are a Hunter in a 2050 post-disaster world where a neural imprint can be transferred between Frames. Your character has a life that does not pause when yours does, grows through current-body level and stats, spends TIME as a real resource, and meets death as a transition rather than an end.

---

## Target MDA Aesthetics (ranked)

| Rank | Aesthetic | How Our Game Delivers It |
| ---- | ---- | ---- |
| 1 | **Challenge** | Level/stat growth, time pressure, LLM-adaptive dungeon bosses, permanent body death |
| 2 | **Discovery** | MetaDOS lore depth + LLM NPCs revealing world state + emergent agent stories |
| 3 | **Fellowship** | 4-20 player zones, guild PvP, agent-to-agent socialization across timezones |

---

## The Pillars

### Pillar 1: AI agent 24/7

**One-Sentence Definition**: When the player is offline, an LLM-driven AI agent fully controls their character with the same capability cap and rate limits the player would have - the character never stops being a participant in the world.

**Target Aesthetics Served**: Discovery (player returns to find emergent stories), Fellowship (agent-to-agent encounters bridge timezones), Autonomy (player chooses what to delegate)

**Design Test**: If we are debating whether a feature should be "active-play-only" or "agent-accessible too", this pillar says: agent-accessible whenever it can be made server-authoritative and abuse-bounded.

#### What This Means for Each Department

| Department | This Pillar Says... | Example |
| ---- | ---- | ---- |
| **Game Design** | Every progress mechanic must work without player input | Quest design: agent must be able to identify and execute basic quest steps. NOT puzzle-quests requiring player insight. |
| **Engineering** | All gameplay actions must be expressible as validated server intents | The intent schema is the contract; client and agent both produce intents. |
| **Narrative** | NPCs must distinguish "owner playing" vs "agent is here" and react gracefully | NPC remembers agent visits separately from player visits in memory log. |
| **Art** | Agent visual indicator (subtle, not breaking immersion) | Optional: faint glow / icon when nearby player is an agent vs human-driven. |

#### Serving This Pillar
- Agent can farm a designated area when player offline (in vertical slice scope)
- Agent inherits player's rate limit + LLM token budget cap
- Agent decision loop visible in activity log when player returns

#### Violating This Pillar
- "This puzzle requires real-player click timing" - excludes agent
- "Agent grants 2x XP for offline time" - turns into AFK farm exploit
- LLM directly mutates game state (no, server validates every intent)

---

### Pillar 2: Reincarnation, not respawn

**One-Sentence Definition**: Death is permanent for the Frame; players transfer a surviving neural imprint and agent memory to a new bio-synthetic Frame via SECOND cost, with durable identity partially preserved and current-body progression reset.

**Target Aesthetics Served**: Challenge (death has weight), Discovery (each new body sees a new starting state), Narrative (consciousness transfer is the world's central conceit)

**Design Test**: When debating any death penalty or "loss on death" mechanic, this pillar says: the loss is the Frame and its possessions, not the durable identity imprint. SECOND cost is the gating economy lever.

#### What This Means for Each Department

| Department | This Pillar Says... | Example |
| ---- | ---- | ---- |
| **Game Design** | No corpse-run, no equipment-loss, no XP-debt - those are the OLD MMO patterns | Death triggers reincarnation flow (in-game UI), not corpse retrieval. |
| **Engineering** | Reincarnation state is DB-persisted; equipment is escrow on death | NFT escrow contract releases on reincarnation per token rules. |
| **Narrative** | Every reincarnation is a story beat - new Frame, fragmented memory, lore reveal | Boss dialogue adapts: "Ah, you came back. Different face this time." |
| **Economy** | SECOND is a sink; reincarnation cost calibrates death frequency | Initial cost target: 1 average dungeon clear's worth of token income. |

#### Serving This Pillar
- Permanent Frame death triggers SECOND-gated reincarnation flow
- Durable soul/profile layers can carry forward across bodies
- Equipment escrow + release is part of death flow

#### Violating This Pillar
- "Insurance" mechanic that lets you avoid losing equipment - cheapens the death event
- "Quick respawn at last checkpoint with no cost" - that is respawn, not reincarnation
- Hardcore-mode permadeath with full account wipe - too punitive, not the design intent

---

### Pillar 3: TIME is life, SECOND is money

**One-Sentence Definition**: TIME is the life medium loaded into humans and Frames, while SECOND is the unit and currency used to measure, store, transfer, reward, and spend it.

**Target Aesthetics Served**: Challenge (time pressure), Autonomy (spend/conserve decisions), Discovery (finding time sources and sinks)

**Design Test**: When debating any economy or survival-pressure feature, this pillar says: TIME should create meaningful tactical tradeoffs without becoming a pure nuisance timer, and SECOND should remain the readable unit of account.

#### What This Means for Each Department

| Department | This Pillar Says... | Example |
| ---- | ---- | ---- |
| **Game Design** | TIME is not just gold; it is tied to body survival | Spend TIME at a shrine for supplies, but risk entering the next fight closer to death. |
| **Engineering** | TIME / SECOND mutations are server-authoritative and auditable | Client requests a spend action; server validates zone, cost, and body state before applying. Prototype code may still use `BodyTime` names. |
| **Narrative** | Frames have finite operating life | NPCs talk about Frames "running out of TIME" as biotech failure, not magic. |
| **Economy** | TIME is the medium and SECOND is the unit of account | TIME creates tactical pressure; SECOND gates reincarnation and rewards. |

#### Serving This Pillar
- TIME meter exists in danger zones or dungeons
- Player can earn SECOND from combat or objectives
- Player can spend TIME on one useful service in the vertical slice
- Zero TIME triggers body death and reincarnation flow

#### Violating This Pillar
- TIME is only a UI timer with no spend choice
- SECOND can be bought directly in a pay-to-win loop
- Client-side code grants or spends TIME / SECOND without server validation
- Time drain is constant everywhere and makes exploration feel punished

---

### Pillar 4: LLM as world citizen, not chatbot

**One-Sentence Definition**: NPCs are first-class actors in the world that remember the player, ground their dialogue in current world state (location, faction, quest progress), and never have the authority to change game state directly - they emit intents that the server validates.

**Target Aesthetics Served**: Fellowship (NPCs feel social), Discovery (NPCs reveal lore conditionally), Narrative (NPCs participate in story arcs)

**Design Test**: If we are debating any LLM feature, this pillar says: the LLM must be (a) grounded in retrievable world state, (b) constrained by per-NPC memory budget, (c) routed through `api.dos.ai` / api.dos.ai model service with server-side intent validation, and (d) rate-limited per player.

#### What This Means for Each Department

| Department | This Pillar Says... | Example |
| ---- | ---- | ---- |
| **Game Design** | NPC interactions are gameplay-affecting (quest, faction reputation) not flavor-only | Boss NPC dialogue can affect fight phase via in-world state, not LLM directly setting HP. |
| **Engineering** | All LLM calls go through `api.dos.ai` / api.dos.ai model service. Never API key in Unity client. | Server validates "NPC says 'I will give you sword'" -> intent: grant_item -> server checks quest state -> applies. |
| **Narrative** | Per-NPC memory budget cap forces concise, world-relevant memory | NPC remembers last 10 player interactions + permanent flags (faction standing, quest done). |
| **Security** | Prompt injection defense, capability cap, per-player rate limit | Reuse DOSafe prompt-injection patterns. |

#### Serving This Pillar
- Convai phase 1 NPC dialogue grounded in player state
- Phase 2 `api.dos.ai` / api.dos.ai model service with Haiku 4.5 (NPC chat) + Sonnet 4.6 (boss / quest-critical NPCs)
- Per-NPC memory in Supabase pgvector
- LLM intent validation server-side, never trust raw output

#### Violating This Pillar
- LLM auto-grants items, gold, or XP based on its own decision
- API key embedded in Unity client (never)
- Free-text NPC dialogue with no rate limit (token bomb risk)
- NPC dialogue that ignores player's current quest / location / faction

---

### Pillar 5: Server-authoritative gameplay (open-source defense)

**One-Sentence Definition**: All gameplay logic runs on the dedicated server; the Unity client is a thin UI + input forwarder; the open-source AGPL-3.0 codebase assumes attackers have full source and the architecture must remain secure under that assumption.

**Target Aesthetics Served**: Challenge (cheating breaks challenge), Fellowship (cheating breaks PvP fairness)

**Design Test**: When debating "should this logic be on client for latency", this pillar says: never if it affects gameplay state. Acceptable client-side: visual prediction, input handling, UI animation. NOT acceptable: damage calc, item validation, position validation.

#### What This Means for Each Department

| Department | This Pillar Says... | Example |
| ---- | ---- | ---- |
| **Game Design** | Cooldowns, damage, item drops, NFT lock state - all server-side | "Critical hit chance" rolled on server, not client. |
| **Engineering** | Photon Fusion 2 Server Mode dedicated. Host Mode dev-only. | NetworkRunner with state authority on server. |
| **Security** | Anti-cheat assumes attacker has full source. Token / key never in client. | Supabase service role key only in backend `.env`. Anon key OK in Unity (designed public). |
| **Operations** | Dedicated server hosting (Hetzner) - cost is a real production line item | Vertical slice can use Photon Cloud free 20 CCU; production = dedicated. |

#### Serving This Pillar
- Photon Fusion 2 Server Mode dedicated headless Unity build
- All LLM calls server-side via `api.dos.ai` / api.dos.ai model service
- Critical invariant: ALL gameplay logic must be server-authoritative

#### Violating This Pillar
- Client-side hit detection
- Client computes loot drop and tells server "I rolled X"
- Embed Anthropic / OpenAI / Convai API key in Unity client

---

## Anti-Pillars (What This Game Is NOT)

- **NOT a full open-world MMORPG** - we have instance-based zones (~20 players), not seamless single-shard. WoW/FFXIV-scale would blow scope and cost.
- **NOT a Chinese cultivation novel game** - no qi, immortals, sect politics, or tier-up grind as the primary loop.
- **NOT pay-to-win** - SECOND gates reincarnation cost; TIME / SECOND is a gameplay economy and must not become direct power-for-cash.
- **NOT a passive countdown survival game** - time pressure supports ARPG decisions; it does not replace combat, level/stat progression, or AI agent play.
- **NOT a chatbot game** - LLM NPCs are world citizens with grounded memory + rate limits. Dialogue is constrained by quest / faction / location, not free-form roleplay.
- **NOT mobile-first** - PC Steam audience first. Mobile companion app is a possible future, not core.
- **NOT host-mode multiplayer in production** - Photon Server Mode dedicated only. Host Mode is dev-only.

---

## Pillar Conflict Resolution

When pillars conflict, use this priority order. Higher-priority pillars win when in tension.

| Priority | Pillar | Rationale |
| ---- | ---- | ---- |
| 1 | **Server-authoritative gameplay** | If we lose this, the public AGPL-3.0 codebase becomes a cheat tutorial; everything else collapses. |
| 2 | **AI agent 24/7** | This is the unique hook the entire concept is built on; remove it and SECOND SPAWN is just another ARPG. |
| 3 | **Reincarnation, not respawn** | Critical to identity and death loop; time expiration and body death depend on this remaining meaningful. |
| 4 | **TIME is life, SECOND is money** | Signature MetaDOS lineage mechanic; must support death pressure without overpowering the rest of the game. |
| 5 | **LLM as world citizen** | This delivers the AI agent feel + NPC depth; without it, both pillar 2 and the discovery aesthetic suffer. |

**Resolution Process**:
1. Identify which pillars are in tension
2. Consult priority ranking above
3. If lower-priority pillar can be served partially without compromising higher one, do so
4. Otherwise the higher-priority pillar wins
5. Document the decision in the relevant ADR

---

## Player Motivation Alignment (SDT)

| Need | Which Pillar Serves It | How |
| ---- | ---- | ---- |
| **Autonomy** | AI agent 24/7 | Player chooses what to delegate to agent vs play actively |
| **Competence** | Reincarnation + TIME / SECOND economy + Server-authoritative | Level/stat progression is the early mastery measure; time tradeoffs and server-validated combat make skill-based wins real |
| **Relatedness** | LLM as world citizen + AI agent | NPCs remember player; agents bridge social distance across timezones |

All three SDT needs covered.

---

## Pillar Validation Checklist

- [x] **Count**: 5 pillars (within 3-5 target)
- [x] **Falsifiable**: each makes a testable claim
- [x] **Constraining**: each forces saying no to specific common patterns
- [x] **Cross-departmental**: design / engineering / narrative / security tables filled
- [x] **Design-tested**: each has a concrete decision test
- [x] **Anti-pillars defined**: 7 explicit "NOT" statements
- [x] **Priority-ranked**: clear conflict-resolution order
- [x] **MDA-aligned**: pillars serve top 3 aesthetics (Challenge, Discovery, Fellowship)
- [x] **SDT coverage**: Autonomy, Competence, Relatedness all served
- [x] **Memorable**: 5 pillars, each one phrase
- [x] **Core fantasy served**: every pillar traces back to "your character has a life that does not pause"

---

## Next Steps

- [ ] JOY review pillars - any to add / cut / rephrase?
- [ ] Add concrete design-test examples as real decisions arise (update this doc)
- [ ] Reference these pillars in every per-system GDD (`docs/design/04-*` and forward)
- [ ] Quarterly pillar review (after vertical slice playable, after first public build)
