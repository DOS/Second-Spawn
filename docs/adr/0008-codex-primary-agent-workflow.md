# ADR 0008: Adopt Codex-primary agent workflow

**Status:** Accepted
**Date:** 2026-05-14
**Deciders:** JOY (sole decision-maker, solo founder)

## Context

SECOND SPAWN is a solo-founder project where JOY is the vision holder and
non-code reviewer. The AI workflow has to optimize for sustained throughput,
low coordination overhead, and reliable verification.

Early setup used Claude Desktop Code mode as the main Unity operator because
Unity's official AI MCP path exposed a single direct-connection slot and was
already configured there. After evaluating the available options, the project
now has a working CoplayDev MCP for Unity connection over local HTTP at
`http://127.0.0.1:8080`. Codex can inspect scenes, read console logs, and make
targeted Unity changes through that path without relying on the unstable Unity
official MCP direct-connection flow.

Claude Code Max / Claude Desktop Code mode remains valuable, but its usage is
limited by subscription budget. During the 2026-05-14 Phase B networking
session, the handoff reported a projected Claude burn rate of about `$473/day`
if the same pace continued, with Phase B token data showing that continuous
Claude usage is too expensive for routine iteration. Codex has more available
capacity for daily work.

## Decision

SECOND SPAWN will use a **Codex-primary workflow**:

1. **Codex is the default daily operator** for code, docs, ADRs, setup files,
   backend work, Unity MCP inspection, targeted Unity scene edits, and repo
   hygiene.
2. **CoplayDev MCP for Unity is the primary Unity bridge** for Codex and Claude
   when an agent needs live Editor context.
3. **Claude Code Max / Claude Desktop Code mode is a specialty agent**, used
   for high-value review, architecture critique, brainstorm sessions, or
   specific Unity work where Codex gets stuck.
4. **Only one agent mutates Unity assets at a time.** If control switches, the
   previous agent leaves a handoff that includes branch, commits ahead/behind,
   dirty files, active scene, console state, MCP state, and next actions.
5. **No significant commit is considered done without reviewer pass.** This
   preserves Hard Rule #7 for JOY as a non-coder.

## Rationale

- Codex has enough available capacity to be the default implementation loop.
- The 2026-05-14 Phase B handoff provided empirical cost pressure, not just a
  preference: continuous Claude usage during Unity/Fusion setup was projected at
  roughly `$473/day`.
- CoplayDev MCP for Unity removes the previous hard dependency on Unity's
  official direct-connection slot for day-to-day Unity interaction.
- The same session debugged the Unity official MCP seat/cap/connection path and
  verified CoplayDev MCP over `localhost:8080` as the more reliable bridge for
  Codex.
- Claude Code Max remains most valuable when used as a scarce expert, not as a
  continuous background worker.
- A documented handoff protocol prevents two agents from editing Unity scenes,
  prefabs, or generated metadata at the same time.

## Alternatives Considered

### Alternative 1: Claude-primary workflow

Claude owns most Unity work and Codex acts as backup reviewer.

- **Pros:** Strong Unity operator feel, good for visual iteration.
- **Cons:** Burns Claude limit quickly, keeps Codex underused, and leaves the
  project fragile when Claude limit is exhausted.
- **Rejection reason:** Not sustainable for a solo founder with scarce Claude
  budget.

### Alternative 2: Split by domain only

Codex owns backend/docs; Claude owns all Unity work.

- **Pros:** Clean mental model.
- **Cons:** Artificial boundary now that Codex can use CoplayDev MCP for Unity;
  increases handoff overhead even for small Unity fixes.
- **Rejection reason:** Slower than necessary for daily iteration.

### Alternative 3: Multiple agents mutate Unity concurrently

Codex and Claude both connect and modify the Editor whenever they are available.

- **Pros:** Maximum theoretical parallelism.
- **Cons:** High risk of scene/prefab conflicts, stale console state, generated
  metadata churn, and unclear ownership.
- **Rejection reason:** Too risky for Unity projects and a non-code founder.

## Consequences

### Positive

- Better daily throughput without exhausting Claude limits.
- More consistent repo-level architecture and documentation discipline.
- Clearer ownership when Unity state changes.

### Negative

- Some Unity-heavy tasks may still need Claude escalation.
- Codex must be careful not to overreach on visual composition or package
  import work that benefits from a human-visible Editor session.

### Mitigations

- Use `docs/setup/agent-handoff.md` for every agent switch.
- Keep major Unity package imports as separate commits.
- Run reviewer pass before significant commits.
- Use Unity console and scene hierarchy checks before claiming Unity work is
  complete.

## Validation Criteria

- Codex can connect to CoplayDev MCP for Unity and read active scene plus
  console logs.
- Handoffs between Codex and Claude include enough context for the next agent
  to continue without rediscovery.
- Claude limit is reserved for review, architecture, brainstorm, and blocked
  Unity work.

## Related Decisions

- [ADR 0001: Adopt Photon Fusion 2 as networking framework](0001-photon-fusion-2.md)
- [ADR 0006: Build Fusion 2 integration from scratch](0006-fusion-2-scratch-over-template.md)
- [ADR 0007: Photon Fusion 2.0.12 - Unity 6.5 beta API incompatibility](0007-photon-fusion-2-0-12-unity-6-5-beta-incompat.md)
- [Agent handoff checklist](../setup/agent-handoff.md)
- 2026-05-14 MCP debug session: Unity official MCP connection instability,
  CoplayDev MCP `localhost:8080` verification, and Phase B Claude token/cost
  handoff.
