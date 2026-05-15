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
