# Character Stat and Relationship System

*Status: Design baseline*
*Created: 2026-05-18*
*Last updated: 2026-05-18*

---

## Purpose

This document defines the baseline character stat, presentation, identity, and
relationship model for SECOND SPAWN.

The goal is to keep body gameplay readable, backend-owned, and useful to both
ARPG combat and LLM-driven NPC behavior without letting the LLM bypass game
authority. It is a design contract, not a final combat spreadsheet.

---

## Design Goals

- Keep the MVP stat model compact enough for Nakama, Unity, and future Fusion
  server validation.
- Separate body-bound combat stats from soul identity, AI personality,
  relationship state, memory, and policy.
- Support Diablo IV and Path of Exile 2 style ARPG readability without copying
  their full UX complexity.
- Make defensive scaling clear: show ratings and effective percentages where
  useful, but keep formula constants tunable by content level.
- Keep LLM-facing stats descriptive and bounded. Stats can shape prompts,
  context, and validated rolls, but never grant direct authority.
- Avoid Nibirium, cultivation XP, Lucky Hit, and wisdom-like overlap in the
  vertical slice.

---

## Non-Goals

- No cultivation tier, Nibirium XP, or hidden body-absorption progression.
- No client-authoritative stats, rewards, currency, TIME, or relationship
  mutation.
- No stat can expand `AgentPolicy`, moderation boundaries, tool access, or
  server authority.
- No final full combat formula in this document.
- No final reincarnation carryover economy for relationships or memory.

---

## Current Source of Truth

The MVP backend uses six canonical body-bound core stats:

- `strength`
- `agility`
- `endurance`
- `perception`
- `focus`
- `presence`

Older serialized prototype fields remain compatibility aliases until Unity
networked stats are renamed safely:

- `force`
- `vitality`
- `resilience`

The broader brainstorm candidates `intelligence`, `charisma`, `luck`, and
`dexterity` are not current MVP backend contract fields. They are documented
below as deferred candidates so the design discussion is not lost.

---

## Layer Taxonomy

| Layer | Examples | Authority |
| ---- | ---- | ---- |
| Core Stats | `strength`, `agility`, `endurance`, `perception`, `focus`, `presence` | Game backend, then Fusion server for live combat |
| Secondary Stats | HP, energy, attack power, armor, elemental resistance, dodge, crit | Derived or cached by backend and server simulation |
| Social Attributes | Appeal band, reputation, faction standing | Backend-owned profile and presentation data |
| Body Presentation | Visual tags, intimidation tags, style, voice profile | Backend-owned, Unity-rendered |
| Identity | Name, callsign, profession, gender identity, pronouns, age fields | Backend-owned identity layer |
| Relationship Ledger | Trust, affection, hostility, fear, respect, debt, familiarity | Backend-owned per-target records |
| Character Traits | Curiosity, courage, discipline, aggression, sociability | Backend-owned agent context |
| Memory Records | Bounded event summaries and evidence links | Backend-owned, LLM-readable |
| Agent Policy | Allowed action and risk surface | Backend-owned hard limit |

---

## Runtime Contract

The current prototype runtime should keep these fields stable:

| Field | Type | Notes |
| ---- | ---- | ---- |
| `level` | integer | Current body level, not durable soul level |
| `strength` | integer | Canonical core stat |
| `agility` | integer | Canonical core stat |
| `endurance` | integer | Canonical core stat |
| `perception` | integer | Canonical core stat |
| `focus` | integer | Canonical core stat |
| `presence` | integer | Canonical core stat |
| `force` | integer | Legacy alias for prototype compatibility |
| `vitality` | integer | Legacy alias for prototype compatibility |
| `resilience` | integer | Legacy alias for prototype compatibility |
| `max_health` | integer | Derived or cached |
| `max_energy` | integer | Derived or cached |
| `attack_power` | integer | Derived or cached |
| `defense_power` | integer | Derived or cached |

New gameplay systems should read the canonical six. Legacy aliases should only
exist at compatibility boundaries.

---

## Core Stats

| Stat | Meaning | Main Use |
| ---- | ---- | ---- |
| `strength` | Physical output, heavy weapon force, carry capacity, brute impact | Melee damage, stagger pressure, heavy tool use |
| `agility` | Movement quality, handling, reaction, attack cadence, dodge scaling | Move speed, dodge rating, attack speed hooks |
| `endurance` | Body durability, recovery, energy reserve, survival tolerance | HP, energy, recovery, BodyTime efficiency hooks |
| `perception` | Sensor quality and awareness input, not LLM intelligence | Detection, weak-point reads, social cue input, threat awareness |
| `focus` | Concentration, panic resistance, instruction stability, pressure tolerance | Channeling, interruption resistance, agent consistency under stress |
| `presence` | Active social force and command weight | Persuasion, negotiation, leadership, intimidation, crowd effect hooks |

### Why Not Wisdom

Do not add `wisdom` as a core stat for MVP. Wisdom-like behavior is distributed
across `perception`, `focus`, `SoulProfile`, `CharacterTraits`, `FrameMemory`,
and `RelationshipLedger`.

### Why Not Accuracy

Do not expose `accuracy` as a player-facing secondary stat for MVP. If future
combat needs hit checks, use a backend-only `hit_reliability` value and keep the
player-facing model centered on direct-hit avoidance through dodge.

---

## Deferred Candidate Stats

These are useful ideas, but they should not be added to the current backend
contract without a migration and balance pass.

| Candidate | Current MVP Placement | Revisit When |
| ---- | ---- | ---- |
| `intelligence` | Represented through profession, skill, memory, and agent context, not body core stats | Technical builds need a readable stat for hacking, crafting, analysis, or device use |
| `charisma` | Folded into `presence` | Social builds need a clear split between influence, command, intimidation, and charm |
| `luck` | Deferred | Loot, crit variance, rare events, and TIME rewards need a server-owned variance stat with strict caps |
| `dexterity` | Folded into `agility` | Precision weapons, finesse tools, or handling builds need separation from raw movement |

If `luck` is added later, it must never mint loot, TIME, or SECOND directly. It
can only bias backend-approved rolls inside explicit caps.

---

## Secondary Stats

Secondary stats are derived, cached, or server-computed values. They can appear
in UI once combat and tuning are ready.

### Offensive

| Stat | Notes |
| ---- | ---- |
| `attack_power` | Baseline direct physical or weapon output |
| `skill_power` | Ability scaling budget |
| `attack_speed` | Animation and attack cadence budget |
| `crit_chance` | Chance for direct hits to crit |
| `crit_damage` | Bonus multiplier for crits |
| `cooldown_reduction` | Ability cadence modifier |
| `resource_cost_reduction` | Energy or ability cost modifier |

### Defensive

| Stat | Notes |
| ---- | ---- |
| `max_health` | Current body HP ceiling |
| `max_energy` | Current body action resource ceiling |
| `armor_rating` | Physical mitigation rating |
| `metal_resistance` | Kim elemental resistance rating |
| `wood_resistance` | Moc elemental resistance rating |
| `water_resistance` | Thuy elemental resistance rating |
| `fire_resistance` | Hoa elemental resistance rating |
| `earth_resistance` | Tho elemental resistance rating |
| `dodge_rating` | Rating converted to capped dodge chance |
| `dodge_chance` | Effective chance after conversion |

### Body, TIME, and Agent Support

| Stat | Notes |
| ---- | ---- |
| `body_time_drain_rate` | How quickly this body burns TIME in relevant states |
| `body_time_efficiency` | Modifier for survival cost and drain hooks |
| `body_stability` | Injury, mutation, overload, or degradation budget |
| `recovery_rate` | Health or energy recovery hook |
| `sensor_range` | How far the actor can sense relevant entities |
| `threat_detection` | Backend and prompt context for danger awareness |
| `stealth_detection` | Backend and prompt context for hidden targets |
| `social_read` | How much social cue context the agent receives |
| `instruction_stability` | How well the agent keeps owner policy under pressure |
| `stress_resistance` | How hard pressure must push before behavior changes |

---

## Defense Scaling

SECOND SPAWN should use rating-based defense with diminishing conversion. This
keeps high values useful without letting defense become a permanent full block.

Example direction:

```text
armor_dr = armor_rating / (armor_rating + armor_scale)
element_dr = resistance_rating / (resistance_rating + resistance_scale)
final_damage = raw_damage * (1 - armor_dr) * (1 - element_dr) * active_modifiers
```

`armor_scale` and `resistance_scale` should be content-level constants or tier
constants. The player-facing UI can show:

- rating value
- effective reduction against same-level content
- warnings when higher-tier content reduces effective mitigation

This borrows the useful idea from modern ARPGs without copying a complicated
UX stack.

---

## Dodge Rules

Dodge should exist without a player-facing accuracy stat.

Example direction:

```text
dodge_chance = max_dodge * dodge_rating / (dodge_rating + dodge_scale)
```

Rules:

- Dodge applies to direct hit attempts only.
- Dodge does not fully avoid damage over time.
- Dodge does not fully avoid ground hazards.
- Dodge does not fully avoid aura damage.
- Dodge does not bypass guaranteed boss mechanics.
- Dodge should have an effective cap.
- Future backend-only hit reliability may exist, but should not become a
  player-facing accuracy stat in MVP.

---

## Appeal and Body Presentation

`Appeal` is not a core stat and not a beauty score. It is a body presentation
attribute used for first impression, social framing, and LLM context.

Recommended fields:

| Field | Notes |
| ---- | ---- |
| `appeal_band` | Broad range such as `plain`, `striking`, `uncanny`, `iconic`, `intimidating`, or `engineered` |
| `appeal_tags` | Short tags such as `scarred`, `polished`, `warm`, `cold`, `predatory`, `official`, or `streetwise` |
| `visual_tags` | Body silhouette, faction styling, cosmetic cues |
| `intimidation_tags` | Threat presentation, armor style, weapon impression |
| `presentation_style` | How the character tends to carry themselves |
| `voice_profile` | Optional voice or speech surface hint |

Presence and Appeal should stay separate:

- `presence` is active social pressure: command, persuade, threaten, lead.
- `appeal` is passive first impression: how the body reads before action.

Neither can bypass consent, moderation, faction rules, or server validation.

---

## Identity Fields

Identity should not be compressed into stats.

Recommended identity fields:

| Field | Notes |
| ---- | ---- |
| `display_name` | Current public name |
| `callsign` | Short battlefield or street identifier |
| `profession` | Role such as courier, medic, scavenger, sentinel, broker |
| `faction_title` | Public role inside a faction |
| `reputation_summary` | Short public reputation hint |
| `gender_identity` | Identity value when relevant |
| `pronouns` | Display and dialogue support |
| `identity_age` | Age of the identity or persona |
| `soul_continuity_age` | Time since this consciousness became continuous |
| `memory_span` | How far reliable memory extends |

Recommended body profile fields:

| Field | Notes |
| ---- | ---- |
| `apparent_age` | How old the body appears |
| `chronological_body_age` | How long this body has existed |
| `body_sex_marker` | Biological marker or synthetic equivalent where relevant |
| `synthetic_marker` | Whether the body is synthetic, cloned, rebuilt, or unknown |

---

## Relationship Ledger

Relationships are part of the character model, not dialogue flavor. They should
be per-target, multi-axis records.

Recommended fields:

| Field | Range | Meaning |
| ---- | ---- | ---- |
| `affinity` | -100 to 100 | Overall pull or warmth toward the target |
| `hostility` | 0 to 100 | Desire to oppose, harm, sabotage, or reject |
| `trust` | 0 to 100 | Belief that the target keeps promises and shares truth |
| `fear` | 0 to 100 | Perceived danger from the target |
| `respect` | 0 to 100 | Recognition of strength, competence, or status |
| `debt` | -100 to 100 | Obligation owed or owed by the target |
| `familiarity` | 0 to 100 | How known and predictable the target feels |
| `affection` | 0 to 100 | Personal warmth, care, or attachment |
| `attachment` | 0 to 100 | Dependency, loyalty, or bond strength |
| `rivalry` | 0 to 100 | Competitive tension that can coexist with respect |
| `last_tone` | enum | Last interaction tone such as `friendly`, `tense`, `hostile`, `intimate`, `transactional`, or `protective` |
| `tags` | list | Facts such as `mentor`, `rival`, `saved-by-target`, `debtor`, `suspect`, or `old-crew` |
| `memory_refs` | list | Memory record IDs that justify current relationship values |

Do not collapse relationships into one `like_score`. A character can respect
someone they hate, fear someone they trust, love someone who betrayed them, or
owe a debt to a rival.

Relationship updates must come from server-validated events. The LLM can
propose a relationship or memory update, but Nakama owns whether it is accepted
and persisted.

---

## Reincarnation and Carryover

Reincarnation replaces or retires the current body. It should not copy every
body-bound value forward.

Default direction:

| Layer | Carryover |
| ---- | ---- |
| Core Stats | No, unless a future explicit rule says otherwise |
| Secondary Stats | No |
| Body Presentation | No, follows the new body |
| Identity | Mostly yes, if it belongs to the durable soul or player identity |
| SoulProfile | Yes, with future tuning |
| Relationship Ledger | Only by approved carryover rule, likely with decay |
| Memory Records | Selected records only, with decay and curation |
| Agent Policy | Yes, when owner-bound |
| Agent Runtime | Partial, mostly counters and audit state |

World NPCs may remember the same soul in a new body, but that should be a
designed rule, not an automatic profile copy.

---

## LLM and Backend Safety

Stats can shape the context an LLM receives, but they do not grant authority.

Rules:

- The client sends intent, never authoritative mutation.
- Nakama and Fusion server compute or validate stat effects.
- `focus` must never weaken prompt-injection defense.
- `presence` and `appeal` must never bypass consent, moderation, or faction
  rules.
- `perception` controls what context is available, not how intelligent the
  model is.
- A stronger LLM provider must not turn a low-perception Frame into an
  omniscient character.
- Tool access lives in `AgentPolicy` and server validation, not stats.
- Memory and relationship writes are accepted only through backend rules.

---

## MVP Cut Line

For the vertical slice:

- Ship the canonical six core stats.
- Keep legacy aliases only at compatibility boundaries.
- Use existing derived fields: `max_health`, `max_energy`, `attack_power`, and
  `defense_power`.
- Document defense, dodge, and resistance formula direction, but do not treat
  this document as final balance.
- Use RelationshipLedger MVP fields: `affinity`, `hostility`, `trust`, `fear`,
  `respect`, `debt`, and `familiarity`.
- Add `appeal_band` and simple presentation tags when the body profile schema is
  migrated.
- Do not add `luck`, `charisma`, `intelligence`, or `dexterity` as MVP backend
  contract fields.

---

## Migration Plan

1. Document the stat and relationship baseline.
2. Add schema fields in backward-compatible form.
3. Dual-read canonical fields and aliases until Unity prototype state is
   renamed safely.
4. Update Unity HUD and debug panels to show canonical names.
5. Add relationship and presentation fields to Nakama storage with optimistic
   concurrency.
6. Move combat formulas into a dedicated combat TDD before real damage goes
   live.
7. Remove legacy aliases only after all Unity and backend call sites stop
   depending on them.

---

## Open Questions

- Should `presence` remain the single social core stat, or should a future
  system split it into `presence` and `charisma`?
- Should `luck` exist at all, given economy, loot, TIME, and SECOND risk?
- Which relationship axes survive reincarnation, and how much decay should
  apply?
- Should Appeal be visible in player-facing UI, debug-only, or only used for
  agent context?
- What are the initial armor, resistance, and dodge conversion constants for
  same-level content?
- How should profession and future skill systems scale without becoming another
  hidden XP layer?
