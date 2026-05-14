# ADR 0007: Photon Fusion 2.0.12 - Unity 6.5 beta API incompatibility

**Status:** Open - blocking, awaiting JOY decision
**Date:** 2026-05-14
**Deciders:** JOY (sole decision-maker, solo dev)

## Context

While running the Phase A smoke test (`_NetworkBootstrap` GameObject + `NetworkRunnerSetup` component in SampleScene -> Play mode -> verify Photon Cloud handshake), Unity 6.5 beta `6000.5.0b7` raised multiple `CS0619` hard errors in Photon Fusion 2.0.12 source files. Unity 6.5 redesigned several editor + scene-graph APIs and marked the old surface as `[Obsolete(true)]` (hard error, not suppressible via `csc.rsp -nowarn:0619`).

Errors surfaced in two waves:

### Wave 1: Runtime - Fusion.Unity.cs + NetworkSceneManagerDefault.cs (PATCHED)

3 errors all at `Scene.handle` implicit-int-cast call sites. Patched in this commit by replacing `scene.handle` with `(int)scene.handle.GetRawData()` (verified compile-clean):

- `Assets/Photon/Fusion/Runtime/Fusion.Unity.cs` line 3744 (GetHashCode)
- `Assets/Photon/Fusion/Runtime/Fusion.Unity.cs` line 3901 (StringBuilder.Append)
- `Assets/Photon/Fusion/Runtime/NetworkSceneManagerDefault.cs` line 591 (assignment)

Each patch is marked with a `// SECOND SPAWN PATCH (2026-05-14)` comment block citing Unity 6.5's deprecation note + a "restore to ..." instruction for when Photon ships an upstream fix.

### Wave 2: Editor - Fusion.Unity.Editor.cs (NOT YET PATCHED)

5+ errors across multiple deprecated Unity 6.5 APIs in the Fusion editor surface. Symptoms span more than just SceneHandle:

| Line | Deprecated symbol | New replacement |
|---|---|---|
| 2185 | `Object.GetInstanceID()` | `GetEntityId()` |
| 7526 | `SerializedProperty.objectReferenceInstanceIDValue` | `objectReferenceEntityIdValue` |
| 9931 | `SerializedProperty.objectReferenceInstanceIDValue` | `objectReferenceEntityIdValue` |
| 12162-12163 | `EditorApplication.hierarchyWindowItemOnGUI` | `hierarchyWindowItemByEntityIdOnGUI` |
| 12195 | `SceneHandle.implicit operator int(SceneHandle)` + `EntityId.implicit operator int(EntityId)` | `(int)x.GetRawData()` (and equivalent for EntityId) |
| 12243 | `SceneHandle.implicit operator SceneHandle(int)` | `SceneHandle.FromRawData(ulong)` |

Plus 6+ CS0618 warnings (deprecated `FindObjectsSortMode`, `FindFirstObjectByType`, etc.) that are not yet errors but will be in future Unity versions.

These errors block the project from compiling, which means Play mode cannot enter, which means we cannot smoke-test Fusion. Phase A is blocked.

## Decision (PENDING JOY)

Four options on the table:

### Option A: Patch all remaining Photon source errors

Mirror the Wave 1 approach for Wave 2 + warnings. Apply `(int).GetRawData()`, `objectReferenceEntityIdValue`, `hierarchyWindowItemByEntityIdOnGUI`, `GetEntityId()`, `SceneHandle.FromRawData(ulong)` substitutions across 7+ Editor sites + 10+ warning sites.

Pros: stays on Unity 6.5 beta per ADR 0005 + Photon Fusion 2 per ADR 0001 + 0006. No reversal of prior decisions.

Cons:

- 17+ source-level edits to a third-party SDK. Each edit is fragile + risks breaking inspector functionality (we cannot easily verify all editor windows still work).
- Maintenance burden compounds when Photon ships Fusion 2.0.13+ - we lose the upstream fix path for each patched line.
- Editor errors may cascade into inspector / project-window rendering bugs we won't catch until they hit at runtime.

### Option B: Downgrade Unity to 6.0 LTS

Reverses [ADR 0005](0005-unity-6-5-beta.md) (which JOY deliberately chose beta over LTS). Unity 6.0 LTS still has the old `Scene.handle` / `EntityId` / etc. APIs that Fusion 2.0.12 was tested against, so the SDK works clean.

Pros:

- Most pragmatic + reversible. Unblocks Phase A immediately. Zero source patches to 3rd-party.
- Aligns with JOY's "newest features in general" rationale's escape hatch in ADR 0005: *"Re-evaluate if beta blocks progress"* - this exact scenario.
- Long-term stability for the 3-6 month vertical slice.

Cons:

- JOY loses access to Unity 6.5 beta features (which were not specifically named, so loss is unclear).
- Bandwidth + time to download Unity 6.0 LTS Editor + reopen project (project files compatible since 6000.x both paths).

### Option C: Wait for Photon Fusion 2.0.13+ Unity 6.5 compat

Pros: zero work today. Photon will eventually ship a Fusion build that compiles on Unity 6.5+.

Cons: blocks vertical slice indefinitely. Photon's release cadence is not committed for Unity 6.5 specifically; could be weeks to months. Losing 2-3 weeks waiting + no fallback if it slips further.

### Option D: Switch off Photon Fusion 2 entirely

Adopt Unity Netcode for GameObjects / Netcode for Entities, or Mirror, or another framework. Reverses ADR 0001 (locked in Photon Fusion 2 explicitly) + ADR 0006 (locked in scratch-from-scratch on Fusion).

Pros: clean dependency on Unity-first APIs that won't drift like 3rd-party SDKs.

Cons:

- Massive ADR rewrite + redo of `docs/design/05-networking-architecture.md` + redo of all 3 networking scaffold scripts.
- MetaDOS reference for BR200 patterns becomes irrelevant.
- AI agent for offline players design assumed Fusion `[Networked]` + tick model.
- Loses 1-2 weeks already invested in Fusion setup.

## Recommendation

**Option B (downgrade to Unity 6.0 LTS)** unless JOY has a specific Unity 6.5 beta feature in mind that the project needs.

Rationale:

1. Pragmatic. Single Unity Hub install operation. No code edits.
2. Aligns with the explicit re-evaluation trigger in [ADR 0005](0005-unity-6-5-beta.md): *"Re-evaluate if beta blocks progress"*.
3. Avoids Option A's 17+ patches that compound maintenance cost.
4. Avoids Option C's indefinite block.
5. Avoids Option D's framework rewrite that would invalidate weeks of work.

If JOY wants to stay on Unity 6.5 beta despite this: pick Option A and mình applies all ~17 patches across one focused commit, accepting the maintenance debt.

If JOY is willing to wait but doesn't want to downgrade: pick Option C and we proceed with non-Fusion work (Go gateway, design docs, Supabase setup) until Photon ships.

## Consequences (per option)

- **B chosen** (likely): supersedes ADR 0005's Unity 6.5 beta choice. Open ADR 0008 to record the decision + rationale.
- **A chosen**: keep Wave 1 patches + add Wave 2 patches (~17 lines across editor source). Document each with `SECOND SPAWN PATCH` markers.
- **C chosen**: park Phase A indefinitely. Re-evaluate weekly via Photon release notes check.
- **D chosen**: rewrite ADR 0001, ADR 0006, GDD 05, and 3 networking scripts. New ADR documents framework choice + migration plan.

## References

- [ADR 0001: Photon Fusion 2 networking](0001-photon-fusion-2.md)
- [ADR 0005: Unity 6.5 beta over Unity 6.0 LTS](0005-unity-6-5-beta.md)
- [ADR 0006: Fusion 2 scratch over template](0006-fusion-2-scratch-over-template.md)
- [GDD 05: Networking Architecture](../design/05-networking-architecture.md)
- Photon Fusion 2.0.12-Stable-1861 (current installed version)
- Unity 6.5 beta `6000.5.0b7` (current installed Editor)
