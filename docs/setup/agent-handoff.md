# Agent Handoff Checklist

*Status: Living document - update when the agent workflow changes*
*Created: 2026-05-14*

This checklist keeps Codex, Claude Code Max / Claude Desktop Code mode, and JOY
aligned while SECOND SPAWN is developed by a solo founder with AI agents.

## Default ownership

| Area | Default owner | Notes |
|---|---|---|
| Daily coding | Codex | Includes C#, backend, docs, ADRs, small Unity MCP edits, repo hygiene. |
| Unity MCP inspection | Codex | Read scene hierarchy, console logs, package state, and focused Editor state. |
| Unity visual composition | Codex first, Claude if blocked | Escalate when the work depends heavily on visual judgement or package-specific Editor flows. |
| Architecture critique | Claude Code Max or Codex reviewer pass | Use scarce Claude budget for high-leverage second opinions. |
| Brainstorming | Claude Code Max or Codex | Pick based on available budget and desired creative depth. |
| Final significant review | Independent agent | Do not let the author be the only reviewer before claiming done. |
| Vision and scope decisions | JOY | Agents propose concrete choices, JOY decides. |

## Unity MCP operating rules

1. Use CoplayDev MCP for Unity as the primary Unity bridge.
2. Keep the Unity Editor open at `D:\Projects\Second-Spawn\Unity`.
3. In `Window > MCP For Unity`, verify:
   - Transport: `HTTP Local`
   - HTTP URL: `http://127.0.0.1:8080`
   - Session: active
4. Only one agent may mutate Unity scenes, prefabs, package imports, or project
   settings at a time.
5. Read-only inspection by the next agent is allowed after the previous agent
   has finished its current write step and reported dirty files.
6. When a package import is needed, import one package per commit:
   - Opsive Ultimate Character Controller
   - Behavior Designer
   - Convai
7. After import or script edits, check the Unity console before moving on.

## Handoff message template

Paste this when switching agents:

```text
Continue Second-Spawn handoff.
Branch:
Ahead/behind:
Latest commits:
Dirty files:
Unity version:
Active scene:
MCP state:
Console summary:
What changed:
What was verified:
Known issues:
Next actions, in order:
Manual JOY actions:
Do not touch:
```

## Commit and review protocol

1. `main` is the stable branch. Daily work happens on `dev`.
2. Feature work starts from `dev` in a separate branch/worktree, then PRs back
   into `dev`.
3. Keep Unity package imports, code changes, docs changes, and scene edits in
   separate commits when practical.
4. Before a significant commit, run an independent review pass. JOY is a
   non-coder, so an agent reviewer must catch code and architecture issues.
5. The final update must say exactly what was verified and what was not.
6. Do not push until the current dirty state and console status are understood.

## Current manual JOY actions

1. Create `Assets/_SecondSpawn/Settings/SecondSpawnConfig.asset` via Unity:
   `Assets > Create > Second Spawn > Project Config`.
2. Add Unity Linux Dedicated Server Build Support for Unity `6000.5.0b7` via
   Unity Hub before dedicated server build work.
3. Import asset store packages in separate passes: Opsive UCC, then Behavior
   Designer, then Convai.

## Current Unity decisions

- `_NetworkBootstrap` lives at the scene root because Fusion and Unity
  `DontDestroyOnLoad` require root GameObjects.
- Runtime-spawned Fusion `NetworkObject` instances are not force-parented under
  `_DynamicObjects` in Phase B. Fusion scene ownership and parent sync require a
  proper networked parent strategy that will be designed later if needed.
- Photon Fusion spawning docs describe `onBeforeSpawned` as a pre-attach
  initialization hook for custom state, not a scene hierarchy ownership API.
  Local Fusion 2.1.1 source also moves spawned prefabs through the runner scene
  via the object provider, so root-level runtime `NetworkObject` instances are
  accepted for Phase B.
