# Paid Asset Setup

*Status: Draft*
*Created: 2026-05-14*
*Author: Codex*

> **Quick reference** - Layer: `Setup` - Priority: `Local Development` - Key deps: Unity Package Manager, Unity Asset Store account

---

## Purpose

SECOND SPAWN is a public repository. Paid Unity Asset Store content must stay out of Git while remaining usable in JOY's local Unity project and private build machines.

---

## Installed Local Assets

| Asset | Source | Local Path | Git Policy | Current Use |
| ---- | ---- | ---- | ---- | ---- |
| RPG Character Mecanim Animation Pack | Unity Package Manager > My Assets | `Unity/Assets/ExplosiveLLC/` | Ignored, do not commit raw files | Prototype character and animation library |
| Warrior Pack Bundle 1-3 FREE | Unity Package Manager > My Assets | `Unity/Assets/ExplosiveLLC/` | Ignored, do not commit raw files | Prototype random visual variants |

---

## Import Steps

1. Open the Unity project at `Unity/`.
2. Open `Window > Package Manager`.
3. Select `My Assets`.
4. Download and import `RPG Character Mecanim Animation Pack`.
5. If the pack asks to load Input and Layer presets, skip it for now. SECOND SPAWN uses its own Fusion and Input System path.
6. Verify Unity console after import.

---

## Repository Rules

- Do not commit raw paid assets, including models, textures, animations, prefabs, demo scenes, sample scripts, or sample controllers from paid packs.
- Do commit project-owned scripts that integrate with locally installed assets.
- Do commit project-owned prefabs only when they do not embed paid asset source data.
- Do keep placeholder/open assets available so public contributors can open the project without licensed paid assets.
- Do document any required paid asset in this file before using it in a prototype.

---

## Animation Integration Rules

- Fusion or Simple KCC owns movement and networked position.
- Animation clips render state and intent only.
- Disable root motion for networked movement unless a future prototype explicitly proves an authority-safe root-motion workflow.
- Keep Animator parameters small and driven from project-owned controller state.
- Use `NetworkAnimatorBridge` on the networked player root to drive locally installed RPG Character Mecanim Animator parameters from replicated KCC movement.
- Do not add `RPGCharacterInputController`, `RPGCharacterInputSystemController`, `RPGCharacterMovementController`, or `SuperCharacterController` to the authoritative networked root. Those scripts are useful references and demo tooling, but SECOND SPAWN movement authority remains Fusion + Simple KCC.
- If a paid visual prefab is used locally, attach it as a child visual under the networked root and keep the committed project prefab usable without the paid asset installed.
- Visual loaders align renderer bounds to the actor ground plane at runtime. Do not fix tall/short character sinking with one-off hardcoded Y offsets unless the asset itself is broken.
- Generated visual prefabs use copied URP materials under `_SecondSpawn/Art/Characters/GeneratedVisualsV2/Materials`. Do not rely on Standard/Built-in shader materials at runtime.

---

## Generated Visual Prefabs

Runtime prototype visuals use generated project-owned prefabs under:

`Unity/Assets/_SecondSpawn/Art/Characters/GeneratedVisualsV2/`

These prefabs are generated from locally installed ExplosiveLLC character prefabs, then stripped of vendor/demo `MonoBehaviour`, `NavMeshAgent`, `CharacterController`, collider, joint, and rigidbody components. They keep the model hierarchy, weapons, Animator, and meshes while replacing vendor materials with local URP material copies. Fusion + Simple KCC remain the only movement authority.

To rebuild them in the Unity Editor:

1. Ensure the local ExplosiveLLC assets are imported.
2. Run `Second Spawn > Art > Rebuild Generated Visual Prefabs`.
3. Enter Play Mode and confirm the console is clean.

Current notes:

- The older `Fighter Pack Bundle FREE` prefabs are not in the runtime random pool because Unity 6.5 logs pre-2019 serialized-file errors when loading their controllers/materials. They can be reconsidered after local re-save or replacement with newer assets.
- `Sorceress Warrior` and `Mage Warrior` are allowed in the random pool after the generated prefab pass converts their source materials to URP material copies.
