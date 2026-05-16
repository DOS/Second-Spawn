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

1. Add Unity Linux Dedicated Server Build Support for Unity `6000.5.0b7` via
   Unity Hub before dedicated server build work.
2. Import asset store packages in separate passes: Opsive UCC, then Behavior
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

## Current gateway and AI prototype state

- Cloud Run staging gateway URL:
  `https://second-spawn-gateway-535583621422.asia-southeast1.run.app`
- Unity `SecondSpawnConfig.asset`, `SecondSpawnConfig.cs`, and scene
  `_AgentGateway` point to the Cloud Run URL so JOY does not need to run a local
  gateway executable for prototype playtesting.
- Local fallback remains `backend/gateway` Docker image
  `second-spawn-gateway:local`.
- Prototype controls:
  - `P`: toggle prototype LLM-agent movement loop on the spawned player
  - `O`: send prototype NPC text chat
  - `V`: check voice-session contract
- Voice is a local prototype cue plus text bubble in Unity. Real TTS still
  requires server-side ephemeral token minting.
- On 2026-05-16, Cloud Run revision `second-spawn-gateway-00003-779` served
  100 percent of traffic. Smoke tests passed for `/readyz`,
  `/v1/characters/dev-player/context`, and duplicate memory POST dedupe.
- On 2026-05-16, CoplayDev MCP Play Mode verification spawned a generated
  Hammer Warrior visual with a clean console. Prototype agent input moved the
  spawned player from `x=1.50` to `x=14.03`, then cleared control.
- On 2026-05-16, `_AgentNPC_Prototype` was added to `ZoneTest_Hub` with a
  local prototype brain. MCP Play Mode verification confirmed the NPC patrols,
  speaks through the prototype text/voice cue path, and the console had no
  warnings or errors after the verification run.

## Current visual prototype state

- Random visual variants use generated prefabs under
  `Unity/Assets/_SecondSpawn/Art/Characters/GeneratedVisualsV2/`.
- Regenerate them from Unity with
  `Second Spawn > Art > Rebuild Generated Visual Prefabs`.
- Generated visual prefabs are standalone cleaned copies with local URP
  material copies under
  `Unity/Assets/_SecondSpawn/Art/Characters/GeneratedVisualsV2/Materials/`.
- Runtime visual loaders align renderer bounds to the actor ground plane after
  Animator pose application. The 2026-05-16 MCP check verified all 13 generated
  variants align to `minY=0.000` after the shared bounds alignment pass.
- The runtime pool currently includes RPG Character plus Warrior Pack Bundle
  1-3 variants, including Sorceress and Mage after URP material conversion.
  `Fighter Pack Bundle FREE` variants are excluded because Unity 6.5 logs
  pre-2019 serialized-file errors when loading their old controllers/materials.
