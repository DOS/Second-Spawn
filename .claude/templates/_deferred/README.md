# Deferred templates

These templates were copied from [Claude-Code-Game-Studios](https://github.com/Donchitos/Claude-Code-Game-Studios) but are not part of the active working set for the current pre-alpha / vertical-slice milestone.

They are kept (not deleted) so they can be promoted back to `.claude/templates/` (top-level) when the milestone trigger that needs them actually fires. Removing them now would force re-downloading from the upstream CCGS repo, which may drift or move.

## How to promote a template back

```powershell
# Example: economy launch is approaching, need release-notes template
git mv .claude/templates/_deferred/release-notes.md .claude/templates/
# Edit, fill in content, commit.
```

After promoting, also remove its line from the table below to keep this README honest.

## Promotion-back triggers

Promote a template back from `_deferred/` to top-level `.claude/templates/` when the corresponding trigger fires.

### Launch / public build

| Template | Promote when |
|---|---|
| `changelog-template.md` | First version-tagged build (alpha 0.1 or similar) |
| `release-notes.md` | First public playable build (Steam Next Fest, demo, etc.) |
| `release-checklist-template.md` | Same as above, paired with release-notes |
| `pitch-document.md` | Outreach to funders, publishers, or hiring contributors |

### Art / audio commission

| Template | Promote when |
|---|---|
| `art-bible.md` | Commissioning art beyond Synty/Quaternius asset packs |
| `sound-bible.md` | Hiring or commissioning a sound designer / composer |

### Narrative scope opens

| Template | Promote when |
|---|---|
| `narrative-character-sheet.md` | Writing dialogue or backstory beyond placeholder NPCs |
| `faction-design.md` | Adding more than the placeholder MetaDOS factions |

### UI / UX scope grows

| Template | Promote when |
|---|---|
| `hud-design.md` | HUD work starts (combat HUD, mini-map, status bars) |
| `ux-spec.md` | Designing menus, flows, settings panels |
| `interaction-pattern-library.md` | UI patterns repeat across 5+ screens |
| `accessibility-requirements.md` | Pre-beta, accessibility audit needed |
| `player-journey.md` | After core loop validates, mapping full new-player flow |

### Balance / performance measurement

| Template | Promote when |
|---|---|
| `difficulty-curve.md` | Tuning combat / progression / quest difficulty |

(Related skills `balance-check` and `perf-profile` live in `.claude/skills/`; do not need promotion.)

### Milestone wrap-up

| Template | Promote when |
|---|---|
| `post-mortem.md` | Vertical slice ships - run a post-mortem |
| `prototype-report.md` | After running a focused prototype experiment |

(`vertical-slice-report.md` stays top-level - currently the active milestone.)

### Risk / test / evidence tracking

| Template | Promote when |
|---|---|
| `risk-register-entry.md` | When architectural risks need tracked status (likelihood / mitigation / owner). Recommended early due to LLM + NFT + Fusion + Supabase novel stack. |
| `test-evidence.md` | When multi-surface testing (Unity Test Framework + Fusion bots + MCP Playwright UI + Go gateway HTTP) needs archived evidence per JOY hard rule "verify before reporting done". Recommended once first feature is testable. |
| `test-plan.md` | When test surface grows beyond ad-hoc inline runs |

### Backfill / reverse documentation

| Template | Promote when |
|---|---|
| `architecture-doc-from-code.md` | Reverse-documenting existing code into ADRs |
| `design-doc-from-implementation.md` | Same, for design docs |
| `concept-doc-from-prototype.md` | Same, for concept docs |

### Architecture review / drift detection

| Template | Promote when |
|---|---|
| `architecture-traceability.md` | When ADR + GDD count grows enough that drift between docs and code is a real risk (rule of thumb: 8+ ADRs or 5+ GDDs) |

### Operations / live service

| Template | Promote when |
|---|---|
| `incident-response.md` | First live multiplayer staging deployment with real players |

### Multi-role team handoff

| Template | Promote when |
|---|---|
| `collaborative-protocols/design-agent-protocol.md` | Recruiting human contributors who design |
| `collaborative-protocols/implementation-agent-protocol.md` | Recruiting human contributors who code |
| `collaborative-protocols/leadership-agent-protocol.md` | Establishing a leadership / decision-making layer beyond JOY |

### Project management overhead

| Template | Promote when |
|---|---|
| `project-stage-report.md` | Periodic stage status reports requested by stakeholders |

### Skill meta-testing

| Template | Promote when |
|---|---|
| `skill-test-spec.md` | Building or testing custom Claude / Codex skills for this repo |

## Re-audit schedule

Re-audit this `_deferred/` set after:

1. Unity vertical slice playable (3-6 months from initial setup)
2. First public build
3. Any time the active top-level set feels too thin or too cluttered

The point is: cost of keeping these is near-zero (small markdown files), cost of re-downloading or rewriting if needed is non-zero. Defer beats delete.
