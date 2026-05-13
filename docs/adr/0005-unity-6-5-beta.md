# ADR 0005: Use Unity 6.5 beta (`6000.5.0b7`) instead of Unity 6.0 LTS

**Status:** Accepted
**Date:** 2026-05-14
**Deciders:** JOY (sole decision-maker, solo dev)

## Context

When initializing the Unity project, the default Editor available in the
local Unity Hub install was Unity 6.5 beta build `6000.5.0b7`, not
Unity 6.0 LTS (`6000.0.x`). The original project setup runbook
([.claude/NEXT_STEPS.md](../../.claude/NEXT_STEPS.md), pre-edit) and
the project context ([.claude/CLAUDE.md](../../.claude/CLAUDE.md),
pre-edit) both specified "Unity 6 LTS".

Two options were on the table:

- **Option A: Roll back to Unity 6.0 LTS.** Stable, supported, all
  3rd-party assets (Opsive Ultimate Character Controller, Behavior
  Designer, Convai, Photon Fusion 2) are tested against this version.
  Predictable for a 3-6 month vertical slice timeline. Ecosystem
  packages (URP 17.x, Input System) are stable on 6.0.
- **Option B: Stay on Unity 6.5 beta `6000.5.0b7`.** Newer features,
  some performance improvements, but breaking changes between beta
  builds (b7 -> b8 -> RC -> GA) are likely; 3rd-party assets may have
  un-tested behavior; ecosystem packages may not have stable releases
  for 6.5 yet.

The default recommendation (per Anthropic Claude assistant and Codex
CLI second-pass) was Option A.

## Decision

**Option B - stay on Unity 6.5 beta `6000.5.0b7`.**

JOY explicitly chose to keep the beta install rather than roll back.

## Rationale

Per JOY's reasoning at decision time:

1. The local install is already on `6000.5.0b7`. Rolling back means an
   extra Unity Hub install + Editor download, which is non-trivial
   bandwidth on the JOY workstation.
2. JOY prioritizes "newest features" tradeoff over "predictable
   stability". JOY treats himself as the only hard constraint - if a
   beta build blocks progress, he can roll back at that time.
3. Vertical slice scope is small enough that breaking changes between
   beta builds are tolerable (he has CI running per
   `.github/workflows/unity-build.yml` which will catch upgrade
   regressions).
4. JOY is the sole user of the build. There is no contributor base yet
   that would be hurt by beta instability.

This is a mutable decision. Re-evaluate if any of the following:

- A 3rd-party asset (Opsive UCC, Behavior Designer, Convai, Photon
  Fusion 2) breaks on a beta build update
- A specific Unity 6.5 feature is the reason to stay (we should name it
  here when that becomes true; today it is "newest features in general")
- The beta upgrade cycle costs more developer time than expected

## Consequences

### Positive

- Newest Unity features (whatever they are at any given time)
- No bandwidth + time cost to roll back today

### Negative

- Risk of breaking changes between beta builds during the 3-6 month
  slice timeline
- Risk that 3rd-party assets break unpredictably
- Some ecosystem packages may not have 6.5-compatible releases
- Bug reports against this project may include "but it works in 6.0
  LTS, why are you on beta?"

### Mitigations

- Pin the exact build (`6000.5.0b7`) in
  [.claude/CLAUDE.md](../../.claude/CLAUDE.md) and
  [Unity/ProjectSettings/ProjectVersion.txt](../../Unity/ProjectSettings/ProjectVersion.txt)
  so anyone reproducing the env knows the target.
- Unity CI workflow (`.github/workflows/unity-build.yml`) targets the
  pinned version - catches regressions at PR time.
- JOY agrees to roll back to 6.0 LTS if any 3rd-party asset
  integration becomes blocked by the beta version.

## Reference

- Hard Rule #6 in [.claude/CLAUDE.md](../../.claude/CLAUDE.md): NEVER
  change Unity Asset Serialization away from Force Text. Both 6.0 LTS
  and 6.5 beta default to Force Text, so this rule is unaffected by the
  version choice.
- See related ADRs:
  [0001 Photon Fusion 2](0001-photon-fusion-2.md),
  [0002 Supabase backend](0002-supabase-backend.md).
