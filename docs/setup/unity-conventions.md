# Unity 6 Project Conventions for SECOND SPAWN

*Status: Living document - reviewed each milestone*
*Created: 2026-05-14*

This file codifies SECOND SPAWN's adoption of Unity 6+ best practices. JOY (the user) prefers the project gọn gàng + đúng chuẩn / convention. AI agents reading this MUST follow these rules when creating, renaming, or organizing any Unity asset, script, or folder.

## Source authorities

In priority order when conventions conflict:

1. JOY's explicit decisions (recorded in this doc and `.claude/CLAUDE.md` Hard Rules)
2. Unity Technologies official guidance
3. Tim D. Hoffmann unity-project-style-guide (community standard)
4. ADRs in `docs/adr/`
5. Per-system GDDs in `docs/design/`

## Folder structure (Assets/ root)

Current structure (committed as of 2026-05-14):

```text
Unity/Assets/
├── Photon/                       # 3rd-party SDK, isolated per Unity guidance
│   ├── Fusion/
│   ├── FusionDemos/
│   ├── FusionMenu/
│   └── PhotonLibs/
├── Scenes/
│   └── SampleScene.unity         # URP template default - rename when slice phase 2 starts
├── Scripts/                      # Custom code, one subfolder per assembly
│   ├── AI/                       # SecondSpawn.AI assembly
│   ├── Gameplay/                 # SecondSpawn.Gameplay
│   ├── Networking/               # SecondSpawn.Networking (Photon Fusion 2 wiring)
│   ├── NFT/                      # SecondSpawn.NFT (DOS Chain via thirdweb gateway)
│   ├── Settings/                 # SecondSpawn.Settings (ScriptableObject configs)
│   └── UI/                       # SecondSpawn.UI
├── Settings/                     # URP renderer + pipeline assets (Unity-managed)
└── InputSystem_Actions.inputactions   # Player input bindings (slice phase 2 uses this)
```

Folders to add as content lands (do NOT create empty - empty folders break version control unless `.keep` placeholder is added; we follow Unity's recommendation to create only what is needed):

- `Materials/` - per asset type, not per character (Unity guidance: do NOT organize by domain)
- `Textures/`
- `Audio/`
- `Prefabs/`
- `Animations/`
- `Editor/` - editor-only scripts, with its own asmdef per Unity guidance

3rd-party assets (when imported beyond Photon):

- Opsive Ultimate Character Controller -> `Assets/Opsive/`
- Behavior Designer -> `Assets/BehaviorDesigner/`
- Convai -> `Assets/Convai/`
- Synty / Quaternius packs -> `Assets/Synty/<pack-name>/` or `Assets/Quaternius/<pack-name>/`

Each 3rd-party folder is treated as immutable; modifications go through wrapper scripts in `Assets/Scripts/...`.

## Naming conventions

| Element | Convention | Example | Rationale |
|---|---|---|---|
| Folder | `PascalCase`, no spaces | `Networking/`, `Scripts/Gameplay/` | Unity + community standard |
| C# script file | `PascalCase` matching the class name | `NetworkRunnerSetup.cs` | Unity requires file = class for `MonoBehaviour` |
| Class | `PascalCase` | `NetworkRunnerSetup` | C# .NET Framework Design Guidelines |
| Field (private + serialized) | `_camelCase` | `_moveSpeed`, `_runner` | Distinguishes from local variable; community standard |
| Field (public) | `PascalCase` | `CultivationTier`, `Hp` | C# property convention |
| Method | `PascalCase` | `FixedUpdateNetwork()` | C# convention |
| Interface | `IPascalCase` | `INetworkRunnerCallbacks` | C# convention |
| Asmdef | `Company.Feature` | `SecondSpawn.Networking` | Unity package guidance: no `_` prefix, broad domain term |
| Namespace | matches asmdef | `SecondSpawn.Networking` | RootNamespace field on asmdef auto-applies |
| Scene | `PascalCase`, descriptive scope | `ZoneTest_Hub.unity` (TBD), `Boss_Awakening.unity` | Underscore separates scope from variant |
| Prefab | `PascalCase`, descriptive | `Player_Hunter.prefab`, `NPC_CultivationMaster.prefab` | Underscore separates type from variant |
| ScriptableObject `.asset` | `PascalCase`, matching the class | `SecondSpawnConfig.asset` | Inspector readability |
| Material / Texture | `PascalCase`, suffix variant | `HunterArmor_Diffuse`, `HunterArmor_Normal` | Type at end per Unity guidance |
| Audio clip | `PascalCase`, descriptive | `Combat_HitImpact_01.wav` | Sequential variants use 2-digit numbers |

NEVER use spaces. NEVER use periods inside file names. NEVER prefix with numbers (`01_player.cs`) - sequential numbering is reserved for actual sequence variants (`PathNode00.prefab`, `PathNode01.prefab`).

## Assembly Definitions (.asmdef)

Following Unity's "one assembly per logical module" guidance + the scaffold already shipped:

| Asmdef | Path | RootNamespace | References |
|---|---|---|---|
| `SecondSpawn.Gameplay` | `Assets/Scripts/Gameplay/` | `SecondSpawn.Gameplay` | (Opsive UCC when imported) |
| `SecondSpawn.Networking` | `Assets/Scripts/Networking/` | `SecondSpawn.Networking` | `Fusion.Runtime`, `Fusion.Common`, `Fusion.Realtime`, `Unity.InputSystem` |
| `SecondSpawn.AI` | `Assets/Scripts/AI/` | `SecondSpawn.AI` | `SecondSpawn.Networking` |
| `SecondSpawn.UI` | `Assets/Scripts/UI/` | `SecondSpawn.UI` | `SecondSpawn.Gameplay` |
| `SecondSpawn.NFT` | `Assets/Scripts/NFT/` | `SecondSpawn.NFT` | (thirdweb-api MCP server-side; client-side will reference Supabase wallet auth wrapper) |
| `SecondSpawn.Settings` | `Assets/Scripts/Settings/` | `SecondSpawn.Settings` | (none - leaf, ScriptableObject configs) |

When adding editor-only scripts in any module:

- Create `Assets/Scripts/<module>/Editor/` subfolder
- Add asmdef `SecondSpawn.<Module>.Editor.asmdef` with platform Editor only
- Reference parent asmdef + UnityEditor

When a module needs Tests:

- Create `Assets/Scripts/<module>/Tests/` subfolder
- Add asmdef `SecondSpawn.<Module>.Tests.asmdef` with `optionalUnityReferences: ["TestAssemblies"]`
- Reference `SecondSpawn.<module>` + `nunit.framework`

## ScriptableObject pattern

Use ScriptableObject for:

- Per-environment config (`SecondSpawnConfig.asset` - already designed)
- Static gameplay data (cultivation tier definitions, NPC dialogue tables, item definitions)
- Per-zone settings (when slice phase 2 zones materialize)

Pattern:

```csharp
[CreateAssetMenu(fileName = "FooConfig", menuName = "Second Spawn/Foo Config")]
public sealed class FooConfig : ScriptableObject { ... }
```

`menuName` ALWAYS prefixed with `Second Spawn/` so the create menu groups SECOND SPAWN scriptables together (Inspector right-click > Create > Second Spawn > ...).

`fileName` matches the class name. Asset instances live in `Assets/Settings/<Name>.asset` for project-global configs, or under the relevant feature folder for feature-local configs.

## Scene organization

Apply to the first real scene (post slice phase 2 wiring; SampleScene.unity is the URP template and will be replaced):

```text
Scene root:
├── _Managers           # Underscore prefix: persistent objects pinned at top of hierarchy
│   └── NetworkRunnerSetup
├── _Cameras
│   └── PlayerCamera (top-down ARPG perspective)
├── _Lights
│   └── Directional Light
├── _UI
│   └── HUDController
├── World               # Static environment
└── _DynamicObjects     # Runtime-spawned content (player, NPCs, loot)
```

Underscore-prefixed groups stay at the top of the Hierarchy view (Unity sorts case-insensitive but underscores precede letters).

## Scripting conventions reinforced from Hard Rules

Cross-reference `.claude/CLAUDE.md` and `AGENTS.md` Hard Rules:

- **Hard Rule #2**: NEVER let LLM mutate authoritative game state. Server validates all intent. Code-level: every NPC dialogue path MUST go through `backend/gateway/internal/intent/intent.go` validation before any `[Networked]` field is written.
- **Hard Rule #3**: NEVER put API keys in Unity client. Code-level: `SecondSpawnConfig` only stores public-safe values (gateway URL, Supabase anon key, Photon App ID, DOS Chain RPC). Secrets stay in `backend/gateway/.env`.
- **Hard Rule #4**: NEVER use Host Mode for production. Code-level: `NetworkRunnerSetup.cs` selects mode by `Application.isBatchMode`. CI build flag `-batchmode -nographics -server` enforces.
- **Hard Rule #6**: NEVER change Asset Serialization away from Force Text. Pin in `ProjectSettings/EditorSettings.asset` (`m_SerializationMode: 2`).
- **Hard Rule #7**: NEVER claim "done" without reviewer pass. Per PR template `.github/pull_request_template.md`.

## Version control conventions

Already enforced via `.gitignore` + `.gitattributes`:

- `Library/`, `Logs/`, `Temp/`, `UserSettings/`, `*.csproj`, `*.sln`, `*.slnx`, `.vsconfig`, `.agents/`, `.claude/worktrees/` - gitignored
- LFS tracks `*.fbx`, `*.psd`, `*.png`, `*.jpg`, `*.wav`, `*.mp3`, `*.mp4`, `*.dll`, `*.unitypackage` and similar binaries
- Force Text serialization keeps `.unity`, `.prefab`, `.asset`, `.meta`, `.mat`, `.anim`, `.controller` etc. as readable YAML for diff
- Force LF line endings on text files (Unix convention; CI runs Linux)

When adding 3rd-party SDKs (like Photon Fusion 2.0.12 already imported), commit the entire SDK folder as-is. Do NOT exclude `Plugins/` or `Editor/` subfolders - they break the SDK at runtime.

## Audit: deviations from convention as of 2026-05-14

Items below are KNOWN deviations to address as work lands; not blockers for current scaffold.

| Deviation | Reason | Resolution |
|---|---|---|
| `Assets/Scenes/SampleScene.unity` is URP template default | Project just initialized | Rename to `ZoneTest_Hub.unity` when slice phase 2 networking goes into a real scene; rebuild hierarchy per "Scene organization" above |
| `Assets/Settings/SecondSpawnConfig.asset` instance not yet created | Domain reload gate - asmdef compiled but type not loaded into AppDomain at scaffold-write time | JOY clicks Editor menu Assets > Create > Second Spawn > Project Config + saves to `Assets/Settings/SecondSpawnConfig.asset`. One-time. |
| `Assets/Materials/`, `Assets/Textures/`, `Assets/Audio/`, `Assets/Prefabs/`, `Assets/Animations/` do not exist | No art assets imported yet | Create when first asset of that type lands. Do NOT pre-create empty (Unity guidance: empty folders break VCS) |
| No `Assets/Editor/` or per-module `Editor/` subfolders | No editor-only scripts yet | Add per the asmdef convention above when first editor tool is needed |
| No `Assets/Tests/` or per-module `Tests/` subfolders | No tests yet | Add per asmdef convention when first test lands; CI workflow `.github/workflows/unity-build.yml` already references `unity-test-runner` for both EditMode and PlayMode |
| Photon SDK demos / menu samples imported (`Photon/FusionDemos`, `Photon/FusionMenu`) | Default unitypackage import included them | Defer cleanup decision to post slice phase 2 - removing now risks breaking sample references; revisit after first real scene works |

## Open questions for JOY

| Question | Why it matters | Proposed default |
|---|---|---|
| Adopt `_Project` wrapper folder convention? (community style guide recommends `Assets/_Project/<all custom>/` so 3rd-party assets sort visibly below) | Cleaner Unity Editor Project view; non-trivial refactor cost | NOT adopt for now. Photon SDK is the only 3rd-party + clearly named. Revisit if 3+ 3rd-party SDKs land |
| Adopt `Assets/_Scripts/` underscore prefix? | Pins Scripts/ at top of Project view alphabetically | NOT adopt. PascalCase consistency wins; Project view sort is configurable per-user |
| Naming for per-zone scenes: `Zone_<Name>.unity` or `<Name>Zone.unity`? | Determines how scenes sort in Editor | `Zone_<Name>.unity` (group by type, then name) |
| Naming for boss scenes: `Boss_<Name>.unity` or part of zone scene? | Slice phase 4 (dungeon + boss) | `Boss_<Name>.unity` separate scene loaded additively when player enters dungeon |
| Photon FusionDemos + FusionMenu - keep, mark deferred, or remove? | They are sample scenes that bloat the project view | Mark deferred (move to `Assets/Photon/_Samples/` subfolder OR add to `.gitignore` if removed). Defer to JOY |

## References

- [Unity official: Best practices for organizing your Unity project](https://unity.com/how-to/organizing-your-project)
- [Unity official: Best practices for project organization and version control (Unity 6 edition)](https://unity.com/resources/best-practices-version-control-unity-6)
- [Unity Manual: Assembly Definitions (6000.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/cus-asmdef.html)
- [Anchorpoint: A guide to folder structures for Unity 6 projects](https://www.anchorpoint.app/blog/unity-folder-structure)
- [Tim D. Hoffmann - unity-project-style-guide](https://github.com/timdhoffmann/unity-project-style-guide)
- [Game Dev Beginner: How to structure your Unity project (best practice tips)](https://gamedevbeginner.com/how-to-structure-your-unity-project-best-practice-tips/)
- ADR 0001: Photon Fusion 2 (`docs/adr/0001-photon-fusion-2.md`)
- ADR 0005: Unity 6.5 beta over Unity 6.0 LTS (`docs/adr/0005-unity-6-5-beta.md`)
- ADR 0006: Fusion 2 scratch over template (`docs/adr/0006-fusion-2-scratch-over-template.md`)
- GDD 05: Networking Architecture (`docs/design/05-networking-architecture.md`)
