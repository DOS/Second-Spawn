# Cultivation System

*Status: Draft (bootstrapped from CLAUDE.md sections "Cultivation System")*
*Created: 2026-05-14*
*Implements Pillar*: AI agent 24/7 + Reincarnation, not respawn

> **Quick reference** - Layer: `Feature` (depends on Combat, Persistence) - Priority: `MVP` (tier 1-2 for vertical slice) - Key deps: `Cultivation persistence, Combat, Reincarnation flow`

---

## Summary

The cultivation system is the central long-term progression mechanic of SECOND SPAWN. Players advance through 6 tiers (Awakening -> Ascension), each unlocking new combat capabilities, social standing, and gameplay zones. Cultivation is sci-fi-framed (Nibirium absorption, biotech, consciousness science) - international-friendly, NOT Chinese-cultivation-novel framing.

Cultivation tier carries forward across reincarnation (partial), making it the durable identity of the player even as bodies, equipment, and locations change.

---

## Player Fantasy

You are a Hunter slowly mastering the absorption and integration of Nibirium - a substance that lets the body and consciousness exceed normal human limits. Each tier-up is earned, not bought. By Ascension, you are near-divine in this post-apocalyptic world.

The fantasy is **earned mastery in a world where most cannot advance past tier 1**.

---

## Detailed Design

### The 6 Tiers

| Tier | Name | What It Means | Slice Scope |
| --- | ---- | ---- | ---- |
| 1 | **Awakening** | Activate Nibirium absorption. Body recognizes the substance. Combat: baseline human capabilities. | ✓ Slice |
| 2 | **Enhancement** | Body strengthening. Physical attributes amplified (speed, strength, resilience). Combat: 1-2 amplified abilities. | ✓ Slice |
| 3 | Core Formation | Energy core formation. Player has internal Nibirium core that powers active abilities (cooldown-based). | Defer post-slice |
| 4 | Evolution | DNA / special ability evolution. Unique mutations (player chooses path). Combat: 1 unique mutation ability. | Defer post-slice |
| 5 | Transcendence | Beyond human limits. Fly, brief invulnerability windows, perception expansion. | Defer post-slice |
| 6 | Ascension | Near-divine. End-game. Acts as gameplay equivalent of "raid tier" character. | Defer post-slice |

### Sci-fi Framing (NOT Chinese cultivation)

The cultivation framing must remain sci-fi-grounded:
- Tier-up is **biotech / consciousness science**, not spiritual enlightenment
- Source of advancement: **Nibirium** (the world's central scarce resource)
- Body changes are **observable physical transformations**, not auras
- No qi, dao, sect politics, immortal cultivation tropes
- Reference: Altered Carbon, Westworld, Ghost in the Shell

This is critical for international audience reach and to avoid cultivation-novel niche framing.

### Tier-Up Mechanic

Each tier transition requires:

1. **Nibirium accumulation** - earned via combat kills, quest rewards, dungeon clears (server-tracked, not client-side)
2. **Tier challenge** - a specific encounter that tests player's mastery at current tier (defined per-tier)
3. **Tier-up ritual** - player triggers transition at a Cultivation Master NPC (LLM-driven, dialogue grounded in player history)

The Cultivation Master NPC is one of the **boss-tier LLM NPCs** (Sonnet 4.6 in phase 2 architecture, Convai-driven in slice phase 1).

### Reincarnation Carryover Rule

When player dies and reincarnates via SECOND token:
- **Cultivation tier**: carries over partially (TODO: JOY decide exact rule - candidates: full tier carries; one tier lost; tier kept but Nibirium accumulation toward next tier resets)
- **Equipment**: reset (escrow released, must re-acquire)
- **Quest progress**: reset
- **Location**: respawn at hub town
- **Faction reputation**: TODO JOY decide

The partial carryover is the **identity** of cultivation in this game: bodies are temporary, your cultivated consciousness is durable.

---

## Interactions with Other Systems

| System | Interface |
| ---- | ---- |
| **Combat (#6)** | Tier 2+ unlocks amplified abilities. Combat damage / health / movement scale per tier. |
| **NPC dialogue (#7)** | NPCs react to player tier (e.g., Tier 1 NPC: "You're new to Nibirium." Tier 5 NPC: "I felt your presence from across the zone.") |
| **Reincarnation (#13)** | Cultivation tier is the carryover variable. |
| **Cultivation persistence (#21)** | Tier + accumulation amount persisted in Supabase. Source of truth for ALL tier checks. |
| **AI agent (#11)** | Agent inherits player's current tier capabilities. Cannot tier-up while player offline (requires player-active tier-up ritual). |
| **NFT inventory (#15)** | Some Hunter skin equips may require minimum tier (TODO design rule). |
| **Quest system (#8)** | Quests gated by tier (e.g., "Cultivate to Enhancement" is itself a quest). |

---

## Formulas (TBD - tune during slice)

### Nibirium Accumulation Per Combat Kill

```
nibirium_gain = base_nibirium_per_enemy * (1 + tier_difficulty_modifier) * cultivation_bonus
```

| Variable | Type | Range | Source | Description |
| ---- | ---- | ---- | ---- | ---- |
| base_nibirium_per_enemy | int | 1-50 | enemy data file | Defined per enemy type |
| tier_difficulty_modifier | float | 0.5-3.0 | calculated | Higher when enemy tier > player tier |
| cultivation_bonus | float | 1.0-1.5 | persisted | Per-tier bonus from current tier |

**Expected output range**: 1-200 Nibirium per kill
**Edge case**: If player tier > enemy tier + 2, set to 0 (no easy farming high-tier from low-tier zones)

### Tier-Up Threshold

```
nibirium_required_for_next_tier = tier_base[current_tier] * (1 + slow_progression_factor)
```

Initial values (tune in slice):
- Tier 1 -> 2 (Awakening -> Enhancement): 500 Nibirium
- Tier 2 -> 3 (Enhancement -> Core Formation): 5,000 Nibirium (defer post-slice)
- Tier 3 -> 4: 50,000
- Tier 4 -> 5: 500,000
- Tier 5 -> 6: 5,000,000

**slow_progression_factor**: 0 in vertical slice; tunable later as economy mechanic.

---

## Edge Cases

| Scenario | Expected Behavior | Rationale |
| ---- | ---- | ---- |
| Player dies during tier-up ritual | Ritual reverts; Nibirium still accumulated; player must restart ritual after reincarnation | Tier-up is a player-active commitment, not a passive process. |
| AI agent attempts tier-up while player offline | Agent is blocked from triggering ritual; can accumulate Nibirium | Tier-up is an identity moment - player must be present. |
| Player attempts to skip tier (e.g., 1 -> 3 directly) | Blocked. Must complete tier-up ritual sequentially. | Cultivation is earned mastery. |
| Equipment requires higher tier than player has | Equip blocked; UI explains tier requirement | NFT escrow + tier check at server. |
| Player at Tier 6 (Ascension) - what's next? | End-game state. Reincarnation cycles continue but tier capped. | Tier 6 = horizontal end-game. |

---

## Tuning Knobs

| Parameter | Current Value | Safe Range | Effect of Increase | Effect of Decrease |
| ---- | ---- | ---- | ---- | ---- |
| Tier 1 -> 2 Nibirium threshold | 500 | 100-2000 | Slower early tier-up, more dungeon farming | Faster tier-up, less time at Awakening |
| Cultivation bonus per tier | 1.05 (5%) | 1.0-1.2 | Each tier feels more significant | Less differentiation between tiers |
| Reincarnation carryover ratio | TBD JOY | 0% (full reset) - 100% (full keep) | Death has less weight | Death has more weight |
| AI agent Nibirium gain rate | TBD | 0% - 100% of player rate | Agent farming more rewarding | Agent farming token-only, not progression |

---

## Visual / Audio Requirements (slice scope)

| Event | Visual Feedback | Audio Feedback | Priority |
| ---- | ---- | ---- | ---- |
| Nibirium absorbed (combat kill) | Subtle pulse on player + small floating "+N Nibirium" text | Soft pickup chime | Slice |
| Tier-up ritual start | Player kneels at Cultivation Master; visual aura forms | Master's LLM dialogue audio | Slice (placeholder) |
| Tier-up complete | Body transformation cinematic (~5 sec) | Tier-up music sting | Slice (placeholder) |
| AI agent gained Nibirium offline | Activity log entry visible on player return | - (UI only) | Slice |
| NPC reacts to player tier | NPC dialogue line acknowledges tier | LLM-generated speech | Slice |

---

## Game Feel

### Feel Reference

Tier-up should feel like the **Mass Effect renegade / paragon meter filling and unlocking** - a slow, observable accumulation that explodes into a clear narrative moment. NOT like grinding XP bars in WoW. The tier transition itself should feel like a **death-and-rebirth moment** within the same body (tonal sibling to reincarnation, but smaller).

### Feel Acceptance Criteria

- [ ] Players notice Nibirium accumulation without HUD glance (visual / audio cue is enough)
- [ ] Tier-up moment lands as story beat (NOT background notification)
- [ ] Cultivation Master dialogue feels grounded in player history (NOT generic congratulation)
- [ ] Tier 2 capabilities feel meaningfully different from Tier 1 (not just stat increase)

---

## Cross-References

| This Document References | Target GDD | Specific Element | Nature |
| ---- | ---- | ---- | ---- |
| "Combat damage scales per tier" | (TDD pending - Combat) | Damage formula | Data dependency |
| "Reincarnation carries cultivation tier" | (TDD pending - Reincarnation) | Carryover rule | State trigger |
| "AI agent inherits tier capabilities" | (TDD pending - AI agent) | Capability cap | Rule dependency |
| "Cultivation Master is boss-tier LLM NPC" | (TDD pending - NPC dialogue) | Boss-tier model selection | Architecture choice |

---

## Acceptance Criteria (slice scope)

- [ ] Player can advance from Tier 1 (Awakening) to Tier 2 (Enhancement) within first 30-60 min of play
- [ ] Tier-up ritual triggered at Cultivation Master NPC works end-to-end
- [ ] Nibirium accumulation persists across reincarnation cycles per JOY-decided carryover rule
- [ ] AI agent can accumulate Nibirium offline at the configured rate
- [ ] Server-authoritative invariant: tier check happens server-side, never client-trusted
- [ ] At least one playtester comments unprompted that the cultivation framing feels sci-fi (not Chinese-cultivation-novel)

---

## Open Questions (need JOY input)

| Question | Owner | Deadline |
| ---- | ---- | ---- |
| Reincarnation carryover ratio: 0% / partial / 100%? | JOY | Before phase 5 |
| AI agent Nibirium gain rate (% of player rate)? | JOY | Before phase 7 |
| Faction reputation carry across reincarnation? | JOY | Before phase 5 |
| Tier 3-6 detailed mechanics (defer to post-slice GDDs) | JOY | Post-slice |
| NFT Hunter skin tier requirements? | JOY | Before phase 6 |
