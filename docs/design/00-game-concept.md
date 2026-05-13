# Game Concept: SECOND SPAWN

*Created: 2026-05-14*
*Status: Draft (bootstrapped from CLAUDE.md)*

---

## Elevator Pitch

> A near-future post-apocalyptic top-down ARPG where your character keeps playing when you log off — an LLM-driven AI agent quests, farms, and socializes with NPCs and other players' agents on your behalf. Death is permanent for the body; reincarnation transfers your consciousness to a new synthetic body via SECOND tokens, resetting progression in a roguelike-MMO hybrid set in the MetaDOS universe.

---

## Core Identity

| Aspect | Detail |
| ---- | ---- |
| **Genre** | Hybrid MMO + Top-down ARPG (NOT full open-world MMORPG) |
| **Platform** | PC (initial) - Windows. Cross-platform later. |
| **Target Audience** | ARPG players who like progression-reset roguelike loops + LLM-driven NPC interaction; MMO-curious solo players who can't commit 4-hour grind sessions |
| **Player Count** | Multiplayer 4-20 per instance zone, 50v50 guild PvP at later phases |
| **Session Length** | 30-90 min active play; offline AI agent extends progress without active play |
| **Monetization** | [TODO: JOY decide - SECOND token gating reincarnation cost is the proposed sink; cosmetic NFT marketplace is a candidate] |
| **Estimated Scope** | Vertical slice 3-6 months; full vision multi-year |
| **Comparable Titles** | Diablo IV, Path of Exile 2, Lost Ark (combat); EVE Online (player-driven economy aspiration); Black Desert (open trades); novel reference: AI agent autoplay has no direct comparable |

---

## Core Fantasy

You are a Hunter in a 2050 post-apocalyptic world where consciousness can be transferred to synthetic bodies via Nibirium-enhanced biotech. You explore, fight, and progress through a 6-tier cultivation system. When you log off, you don't disappear from the world - your character keeps playing through an AI agent shaped by your history. Death is meaningful (the body dies) but not final (you reincarnate, partially carrying cultivation tier across bodies).

The fantasy is "your character has a life that does not pause when yours does."

---

## Unique Hook

It's like Diablo IV with persistent online zones, AND ALSO **your character keeps playing when you are offline** - an LLM-driven agent farms, quests, and socializes with NPCs and other players' agents on your behalf, and **death triggers reincarnation** with a partial-reset roguelike progression loop.

The hook passes the "and also" test on two axes simultaneously:
1. AI agent autoplay (offline persistence in a multiplayer ARPG is near-unique)
2. Reincarnation as the death loop (instead of corpse run / repair cost / equipment loss)

---

## Player Experience Analysis (MDA)

### Target Aesthetics (ranked, top 3)

| Rank | Aesthetic | How We Deliver It |
| ---- | ---- | ---- |
| 1 | **Challenge** | Cultivation tier-up gates, dungeon bosses with LLM-driven dialogue + adaptive behavior, permanent body death |
| 2 | **Discovery** | Layered MetaDOS lore (Nibirium, consciousness transfer, faction history); LLM NPCs reveal world state through dialogue; emergent stories from agent behaviors |
| 3 | **Fellowship** | 4-20 player zones, guild PvP 50v50, party invites via Supabase Realtime, agent-to-agent socialization across timezones |
| N/A | Sensation | Stylized low-poly art (Synty / Quaternius); not a sensory-pleasure-first game |
| N/A | Submission | Active play is intentionally engaging; relaxed offline progress is delegated to the AI agent rather than the player |

### Core Mechanics (3-5 systems generating the dynamics)

1. Top-down ARPG action combat (Opsive Ultimate Character Controller)
2. LLM-driven NPC dialogue with server-validated intent (Convai phase 1, custom Go gateway phase 2)
3. AI agent autonomous control of player character when offline (server-authoritative, capability-capped)
4. Reincarnation via SECOND token cost (consciousness transfer, partial cultivation tier carryover)
5. Cultivation 6-tier progression (Awakening -> Ascension), gated by milestones

---

## Player Motivation Profile (SDT)

| Need | How This Game Satisfies It | Strength |
| ---- | ---- | ---- |
| **Autonomy** | Choose how to spend time online vs delegate to AI agent; choose reincarnation timing; choose cultivation path | Core |
| **Competence** | Cultivation tier-up is the explicit mastery ladder; combat skill ceiling via Opsive controller depth | Core |
| **Relatedness** | LLM NPCs remember you across sessions; guild + zone fellowship; agent-to-agent socialization | Supporting |

### Player Type Appeal (Bartle)

- [x] **Achievers** - cultivation tier ladder + AI agent farming offline = constant progression
- [x] **Explorers** - MetaDOS lore depth + emergent NPC interactions to discover
- [x] **Socializers** - guild + multi-instance zones + LLM NPCs as social actors
- [ ] **Killers / Competitors** - 50v50 guild PvP exists but NOT primary loop in vertical slice

---

## Core Loop

### Moment-to-Moment (30 sec)
ARPG action combat: dodge, attack, ability, kite, kill. Top-down camera. Hunter character with 1 NFT skin equipped.

### Short-Term (5-15 min)
Quest segment or dungeon room: clear group of enemies, loot, advance to next encounter. LLM-driven NPC dialogue at quest checkpoints.

### Session-Level (30-120 min)
Complete a quest line or dungeon clear; converse with hub-town NPCs (LLM-driven); cultivate (consume Nibirium / advance tier progress); plan offline agent behavior before logout.

### Long-Term Progression
- Cultivation tier 1 -> 6 (Awakening, Enhancement, Core Formation, Evolution, Transcendence, Ascension)
- NFT Hunter skin collection
- Guild + faction reputation
- Reincarnation cycles - each death keeps partial cultivation but resets equipment / quest state

### Retention Hooks
- **Curiosity**: AI agent activity log (what did your character do while you slept?)
- **Investment**: cultivation tier progress + NFT-locked equipment
- **Social**: guild obligations, zone friend agent encounters
- **Mastery**: tier-up gates that test combat + crafting

---

## Game Pillars (preview - see [01-pillars.md](01-pillars.md))

1. **AI agent 24/7** - the character is always playing
2. **Reincarnation, not respawn** - death has weight; SECOND token cost
3. **LLM as world citizen, not chatbot** - NPCs are server-validated agents in the world
4. **Server-authoritative gameplay** - public open-source repo means anti-cheat assumes attacker has full source

---

## Inspiration

| Reference | What We Take | What We Do Differently |
| ---- | ---- | ---- |
| Diablo IV | Top-down ARPG action combat, item / loot loops | No paragon board grind; reincarnation replaces seasonal reset |
| Path of Exile 2 | Skill / passive depth, build expression | Less crafting-first; more character-narrative-driven via LLM NPCs |
| Lost Ark | Multi-instance zone hubs, guild raids structure | Smaller zone size (~20 players); LLM agents instead of scripted NPCs |
| EVE Online | Player-driven economy, persistent universe consequence | Solo-friendly via AI agent; not a single-shard nightmare |
| MetaDOS (BR) | Hunter skin NFT system, Photon Fusion 2 networking patterns | Different genre (ARPG, not BR); persistent zones, not match rounds |

**Non-game inspirations**: Altered Carbon (consciousness transfer), Westworld (synthetic bodies + emergent NPC behavior), Ghost in the Shell (cyberpunk biotech tone)

---

## Target Player Profile

| Attribute | Detail |
| ---- | ---- |
| **Age range** | 22-40 |
| **Gaming experience** | Mid-core to hardcore ARPG / MMO veteran |
| **Time availability** | 3-6 hours/week active play; wants offline progression to feel meaningful |
| **Platform preference** | PC (Steam audience) |
| **Current games they play** | Diablo IV, PoE 2, Last Epoch, Throne and Liberty, EVE Online |
| **What they're looking for** | Progress without grind treadmill; novel social mechanics; not full-MMO time commitment |
| **What would turn them away** | Pay-to-win; gacha mechanics; LLM dialogue feeling chat-bot-ish (un-grounded in world state) |

---

## Technical Considerations

| Consideration | Assessment |
| ---- | ---- |
| **Engine** | Unity 6.5 beta (currently `6000.5.0b7`) + URP. JOY chose beta for newest features. |
| **Networking** | Photon Fusion 2 (Server Mode dedicated for production; Host Mode + Photon Cloud free 20 CCU for dev) |
| **Persistence** | Supabase Postgres (profile, inventory, quest, NFT lock state, cultivation tier) |
| **LLM** | Convai phase 1 (NPC dialogue) -> Go LLM gateway phase 2 (Haiku 4.5 for NPC chat, Sonnet 4.6 for boss / cultivation master). Server-side intent validation only. |
| **NFT** | DOS Chain via thirdweb-api MCP. Wallet auth, escrow contracts, Hunter skin / weapon / pet inventory. |
| **Art** | Synty / Quaternius stylized low-poly + reused MetaDOS Hunter skins |
| **Key technical risks** | LLM intent validation at scale; AI agent server tick load; NFT-Unity inventory sync latency |

See [docs/ARCHITECTURE.md](../ARCHITECTURE.md) for system diagram + critical invariants.

---

## Risks and Open Questions

### Design Risks
- AI agent offline play may feel either invisible (player doesn't notice progress) or invasive (agent does things player wouldn't choose)
- Cultivation tier pacing: too slow = grind; too fast = no mastery feeling
- LLM NPCs may feel chatbot-like if they don't ground in world state (location, quest progress, faction)

### Technical Risks
- LLM cost at scale (offline AI agents alone could 10x cost vs active players)
- Photon Fusion 2 dedicated server cost when scaling beyond 20 CCU
- NFT escrow latency between Unity equip action and DOS Chain confirmation

### Market Risks
- Open-source AGPL-3.0 + AI agent autoplay is unproven combo for retention monetization
- ARPG audience is saturated (D4, PoE 2, Last Epoch)

### Scope Risks
- Solo dev + AI agent (Claude Code) + 3-6 month vertical slice on novel architecture is tight
- 3rd-party assets (Opsive UCC, Behavior Designer, Convai) may not be tested against Unity 6.5 beta

### Open Questions (need JOY input later)
- SECOND token economy: reincarnation cost, source, sink. (Open Decision Point in CLAUDE.md.)
- Hunter NFT integration: Option 1 (preset hero) vs Hybrid 1+3 (modular pieces)
- Voice NPC vendor: OpenAI Realtime vs ElevenLabs vs self-host
- Final game name (SECOND SPAWN is codename)

---

## MVP Definition (Vertical Slice)

See [02-vertical-slice-spec.md](02-vertical-slice-spec.md).

**Core hypothesis**: A solo player can experience all 3 USPs (AI agent autoplay, reincarnation, cultivation tier-up) inside a single zone within 30 minutes of first play, without requiring out-of-game tutorials.

---

## Next Steps

- [ ] JOY review and refine this concept doc (especially monetization line + open questions)
- [ ] Finalize [01-pillars.md](01-pillars.md) (preview pillars listed above need design tests added)
- [ ] Build vertical slice per [02-vertical-slice-spec.md](02-vertical-slice-spec.md)
- [ ] Per-system GDDs as systems are designed (cultivation already started in [04-cultivation-system.md](04-cultivation-system.md); combat, AI agent, reincarnation, NFT escrow, LLM NPC pending)
