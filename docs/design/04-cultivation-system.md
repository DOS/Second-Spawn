# Deferred Advanced Body Progression

*Status: Deferred*
*Updated: 2026-05-17*

> This document replaces the old cultivation / Nibiru-derived XP draft. That mechanic
> is no longer part of the vertical slice because it behaved too much like a
> traditional XP bar with sci-fi naming.

---

## Decision

SECOND SPAWN will not implement cultivation tiers, Nibiru-derived XP, tier-up rituals,
or Cultivation Master progression in the current vertical slice.

The slice uses:

- `CharacterStats.level` as the baseline progression value.
- Body-bound stats such as vitality, force, agility, focus, resilience,
  max health, max energy, attack power, and defense power.
- TIME / SECOND and reincarnation as the signature systemic loop.

Advanced body or soul progression remains a future design space, but it needs a
fresh brainstorm and a stronger mechanic before implementation.

---

## Why This Was Cut

The previous draft centered on:

1. earning Nibiru-derived XP from combat,
2. filling a progress counter,
3. completing a tier-up ritual,
4. carrying some tier state across reincarnation.

That loop was too familiar and did not add enough unique value on top of a
standard level system. It also risked distracting from the stronger pillars:
offline AI agents, reincarnation, TIME / SECOND, and many NPC-like actors with
separate profiles.

---

## Current Progression Baseline

For the vertical slice, progression is intentionally simpler:

| Layer | Scope |
| ---- | ---- |
| Level | Main player-facing progression value for the current body |
| Stats | Body-specific combat tuning and profile identity |
| TIME / SECOND | TIME is the current body's operating life; SECOND is how it is measured and rewarded |
| Reincarnation | Body replacement and current-body reset |
| Soul/profile | Durable identity layer, exact carryover rules still open |

Level and stats are enough until combat, reincarnation, and TIME / SECOND feel good.

---

## Future Redesign Direction

When this system returns, it should avoid "kill enemies to fill a named XP bar."
Candidate directions to explore later:

- body compatibility and instability,
- risky mutation choices,
- body scars or defects after failed breakthroughs,
- soul imprints that survive reincarnation,
- NPC or faction consequences tied to body experiments,
- offline-agent risk stories that alter the body without granting direct power.

None of these are approved for implementation yet.

---

## Implementation Rules

- Do not add `cultivation`, `progress_xp`, Nibiru-derived XP, tier names, or tier-up
  RPCs to the runtime model.
- Do not show cultivation tier in UI or HUD.
- Do not let LLMs grant level, stats, TIME, SECOND, inventory, quest progress,
  token rewards, or any future body progression directly.
- Keep advanced body progression behind a future design review.

---

## Open Questions

| Question | Owner | Timing |
| ---- | ---- | ---- |
| What makes advanced body progression unique enough to return? | JOY + design agents | Post-slice brainstorm |
| Which identity layer survives reincarnation besides account profile? | JOY | Reincarnation MVP |
| Should body defects, scars, or mutation risks exist? | JOY | Post-slice brainstorm |
