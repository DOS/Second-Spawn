# ADR 0009: Enable unsafe code for the Fusion networking assembly

**Status:** Accepted
**Date:** 2026-05-14
**Deciders:** JOY (sole decision-maker, solo founder)

## Context

SECOND SPAWN hosts Fusion gameplay code in asmdef assemblies such as
`SecondSpawn.Networking`, not in Unity's default `Assembly-CSharp`. Photon
Fusion 2.1.1-RC weaves `[Networked]` property accessors into those assemblies.

During the Phase B Play Mode smoke test, the generated accessors for
`SecondSpawn.Networking.NetworkPlayer` raised runtime exceptions:

```text
FieldAccessException: Field `Fusion.NetworkBehaviour:Ptr' is inaccessible from method `SecondSpawn.Networking.NetworkPlayer:get_NetworkedPosition ()'
```

`Unity/ProjectSettings/ProjectSettings.asset` already has project-wide
`allowUnsafeCode: 1`, but `Unity/Assets/_SecondSpawn/Scripts/Networking/SecondSpawn.Networking.asmdef`
had `"allowUnsafeCode": false`. Fusion's IL weaving uses pointer-backed runtime
state for `[Networked]` properties, so the asmdef that contains networked
behaviours must also allow unsafe code.

A previous `IgnoresAccessChecksTo("Fusion.Runtime")` experiment did not resolve
the Play Mode failure and has been removed.

## Decision

Enable unsafe code for the `SecondSpawn.Networking` assembly:

```json
"allowUnsafeCode": true
```

Keep project-wide `allowUnsafeCode: 1` in Unity Player Settings.

Do not use a CLR access-check bypass workaround for this issue.

## Rationale

- Photon Fusion's generated `[Networked]` accessors depend on pointer-backed
  state and require unsafe code.
- The fix is limited to the networking assembly that owns Fusion
  `NetworkBehaviour` types.
- Keeping asmdefs preserves project compile isolation and module ownership.
- Removing the failed access-bypass experiment avoids documenting or shipping a
  misleading workaround.

## Alternatives Considered

### Alternative 1: Move networking scripts to `Assembly-CSharp`

- **Pros:** Avoids asmdef-level unsafe configuration.
- **Cons:** Reverses project conventions, increases compile scope, and weakens
  module ownership.
- **Rejection reason:** The asmdef can be configured correctly.

### Alternative 2: Keep `IgnoresAccessChecksTo("Fusion.Runtime")`

- **Pros:** Theoretically bypasses runtime access checks.
- **Cons:** Play Mode testing showed the error still occurred. It is also more
  obscure and less aligned with Fusion's expected unsafe-code requirement.
- **Rejection reason:** Ineffective in this Unity/Fusion setup.

### Alternative 3: Patch Photon runtime DLL or source

- **Pros:** Could avoid changing project asmdef settings.
- **Cons:** Vendor patch maintenance burden and higher risk on Photon updates.
- **Rejection reason:** Not needed when asmdef unsafe code fixes the root cause.

## Consequences

### Positive

- Keeps asmdef-based project structure intact.
- Aligns `SecondSpawn.Networking` with Fusion's weaved `[Networked]` property
  runtime requirements.
- Avoids a brittle runtime access bypass.

### Negative

- Unsafe code is now enabled for the entire `SecondSpawn.Networking` assembly.
- Future networking code reviews must watch for accidental unsafe blocks beyond
  Fusion-generated accessors.

### Mitigations

- Do not hand-write unsafe code in SECOND SPAWN networking scripts unless a
  future ADR explicitly accepts it.
- Keep unsafe enabled only on assemblies that contain Fusion networked runtime
  code.
- If other asmdefs later contain `[Networked]` properties, enable unsafe there
  deliberately and update this ADR or create a follow-up ADR.

## Revert Criteria

Only revert `"allowUnsafeCode": true` in `SecondSpawn.Networking.asmdef` if all
of the following are true:

1. Photon Fusion `release_history.txt` or official release notes confirm unsafe
   asmdef compilation is no longer required for weaved `[Networked]`
   properties.
2. `SecondSpawn.Networking` remains listed in
   `NetworkProjectConfig.AssembliesToWeave`.
3. Phase B Play Mode smoke test passes with unsafe disabled.

## Validation Criteria

- Unity compiles without C# errors.
- Enter Play Mode in `ZoneTest_Hub`.
- Fusion Host session starts.
- Player join spawns `Player_NetworkCube`.
- No new `FieldAccessException` appears when `NetworkPlayer.Spawned()` or
  `NetworkPlayer.FixedUpdateNetwork()` accesses `[Networked]` properties.

## Related Decisions

- [ADR 0005: Use Unity 6.5 beta](0005-unity-6-5-beta.md)
- [ADR 0006: Build Fusion 2 integration from scratch](0006-fusion-2-scratch-over-template.md)
- [ADR 0007: Photon Fusion 2.0.12 - Unity 6.5 beta API incompatibility](0007-photon-fusion-2-0-12-unity-6-5-beta-incompat.md)
