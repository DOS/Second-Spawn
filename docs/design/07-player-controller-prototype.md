# Prototype Design: Networked Player Controller

*Status: In progress*
*Created: 2026-05-14*
*Author: Codex*
*Last Verified: 2026-05-15 against local Photon Pirate Adventure 2.0.12 sample review and Simple KCC 2.0.15 package metadata*

> **Quick reference** - Layer: `Core` - Priority: `MVP` - Key deps: `Photon Fusion 2`, `Unity Input System`, `ZoneTest_Hub`, `SecondSpawnConfig`

---

## Purpose

This prototype proves the smallest useful SECOND SPAWN gameplay foundation: a network-spawned player can move in a top-down ARPG scene under Fusion authority, with a readable camera and no Unity console errors.

This is not the combat system. This is the movement and authority contract that combat, cultivation, reincarnation, AI agent control, and future asset-controller integrations must respect.

---

## Design Decision

Build a **project-owned minimal networked controller first**, then convert the placeholder player to Photon Fusion Simple KCC before evaluating Opsive Ultimate Character Controller. Opsive should prove value against a working Fusion-native controller path rather than become the baseline by default.

Rationale:

- The game is open-source multiplayer, so authority rules are more important than controller feature depth.
- The first prototype needs known behavior that agents can reason about.
- Pirate Adventure shows a useful Fusion-native top-down path with Simple KCC, FSM states, runner physics queries, and compact network input.
- Opsive may still be useful, but it should prove value against the project baseline and Simple KCC spike rather than become the baseline by default.

---

## Player Experience

The prototype should feel like the first rough blockout of a top-down ARPG:

- The player appears in the hub test scene.
- Input response is immediate enough to judge direction and momentum.
- Movement is planar, readable, and camera-stable.
- The player can stop, turn, and move diagonally without fighting the controls.
- Multiplayer correctness matters more than animation polish.

The placeholder avatar can be a cube, capsule, or simple Hunter stand-in.

---

## Scope

### In Scope

- Fusion-spawned local player in `ZoneTest_Hub`
- Top-down movement on the XZ plane
- Keyboard movement input
- Local player camera follow
- Basic speed tuning through serialized fields
- Console-clean Play Mode verification
- Clear ownership between client input and authoritative state

### Out of Scope

- Combat
- Abilities
- Animation controller
- Opsive UCC dependency
- Click-to-move pathfinding
- NavMesh
- Character stats beyond movement speed
- Remote-player interpolation polish
- Dedicated Server Mode deployment
- Persistence, inventory, NFT, LLM, or AI agent logic

---

## Control Model

| Input | Behavior |
| ---- | ---- |
| `W` / up | Move forward relative to world north for first prototype. |
| `S` / down | Move backward relative to world south. |
| `A` / left | Move left. |
| `D` / right | Move right. |
| Diagonal input | Normalize movement so diagonal speed is not faster. |
| No input | Stop movement immediately for the first pass. |

Camera-relative movement can be added after the first pass if the camera angle makes world-relative controls feel wrong.

---

## Authority Contract

1. The client collects input only.
2. The client sends movement intent through Fusion input.
3. The networked player applies movement in Fusion simulation code.
4. Gameplay state is owned by the server or host authority.
5. The client may render prediction, but it must not become the durable source of position.
6. Future AI agent control must be able to emit the same movement intent shape as a human player.

---

## Components

| Component | Responsibility | Existing or New |
| ---- | ---- | ---- |
| `NetworkRunnerSetup` | Starts Fusion dev session and owns runner lifecycle. | Existing Phase B |
| `NetworkInputProvider` | Collects movement input per Fusion tick. | Existing Phase B, may extend |
| `NetworkPlayer` | Applies Fusion input to Simple KCC and owns session player state. | Existing Phase B, extended |
| `PlayerSpawner` | Spawns one player object per joined player. | Existing Phase B |
| `PlayerCameraFollow` | Keeps camera pointed at local player. | New if no suitable existing component |
| `Player_NetworkCube.prefab` | Placeholder networked player prefab with Simple KCC. | Existing Phase B, evolved |

---

## Movement Tuning

| Parameter | Initial Value | Safe Range | Notes |
| ---- | ---- | ---- | ---- |
| Move speed | `6.0` units/sec | `3.0 - 10.0` | Fast enough to test ARPG feel in a small hub. |
| Rotation speed | Instant or `720` deg/sec | `360 - 1080` | Instant is acceptable for placeholder. |
| Acceleration | Instant | TBD | Add acceleration only if instant stop/start feels too arcade. |
| Camera height | `12` units | `8 - 18` | Depends on zone blockout scale. |
| Camera pitch | `45 - 60` degrees | `35 - 65` | Must keep player and travel direction readable. |

These are prototype values, not final game balance.

---

## Implementation Plan

### Phase 1: Baseline Movement

- [ ] Review existing `NetworkPlayer`, `NetworkInputProvider`, and `PlayerSpawner`.
- [ ] Keep `Player_NetworkCube.prefab` as the placeholder unless the existing prefab blocks movement.
- [ ] Ensure movement input is normalized.
- [ ] Keep movement code small and obvious.
- [ ] Verify Play Mode spawns one controllable player.

### Phase 2: Camera Follow

- [ ] Add or configure a top-down follow camera.
- [ ] Follow only the local player.
- [ ] Avoid camera ownership assumptions that break remote clients.
- [ ] Verify scene view and game view readability.

### Phase 3: Prototype Feel Pass

- [ ] Tune speed and camera height.
- [ ] Confirm no diagonal speed exploit.
- [ ] Confirm player stops predictably.
- [ ] Record remaining feel issues in this doc.

### Phase 4: Simple KCC Spike

- [x] Import the official Fusion Simple KCC addon in a separate branch/commit only after baseline passes.
- [x] Convert movement from raw networked position updates to KCC-backed movement.
- [ ] Validate with Unity 6.5 beta and current Fusion 2.1.1 release candidate.
- [ ] Compare movement feel and authority clarity against the baseline.
- [ ] Decide whether Simple KCC becomes the MVP controller.

### Phase 5: Opsive Evaluation Branch

- [ ] Import Opsive UCC in a separate branch/commit only after the Simple KCC spike.
- [ ] Check Unity 6.5 beta compatibility and console state.
- [ ] Compare Opsive movement/combat/camera value against the baseline and Simple KCC spike.
- [ ] Decide whether Opsive becomes core, optional, or deferred.

---

## Verification Checklist

- [ ] Unity opens project without compile errors.
- [ ] `ZoneTest_Hub` enters Play Mode.
- [ ] Fusion runner starts a dev session.
- [ ] Local player spawns through `PlayerSpawner`.
- [ ] WASD moves the local player.
- [ ] Diagonal speed is normalized.
- [ ] Camera follows the local player.
- [ ] Console has 0 errors after entering and exiting Play Mode.
- [ ] No new secret or private key is added to Unity assets.

---

## Known Limitations

- Movement is not final combat movement.
- Placeholder visuals are acceptable.
- No animation blend tree is expected.
- Dedicated Server Mode is not validated by this prototype.
- Remote-client feel may need a later Multiplayer Play Mode pass.

---

## Follow-Up Design Docs

| Future Doc | Trigger |
| ---- | ---- |
| Combat Prototype Design | After player movement and camera are stable. |
| Camera Design | If camera behavior becomes deeper than one follow component. |
| Pirate Adventure Reference Review | Completed after local sample inspection. |
| Simple KCC Spike Report | After official Simple KCC addon import and smoke test. |
| Opsive Evaluation Report | After isolated Opsive import and smoke test. |
| Offline AI Agent Movement Contract | Before AI agent can control the same player actor. |

---

## Cross-References

| This Document References | Target Doc | Specific Element Referenced | Nature |
| ---- | ---- | ---- | ---- |
| Prototype shape | `06-overview-design.md` | Minimal controller first, Simple KCC spike second, Opsive evaluation third | Scope dependency |
| Reference sample | `09-pirate-adventure-reference-review.md` | Pirate Adventure controller and FSM patterns | Pattern dependency |
| Networking rules | `05-networking-architecture.md` | Network input and server authority | Rule dependency |
| Pillar priority | `01-pillars.md` | Server-authoritative gameplay | Rule dependency |
| Unity conventions | `../setup/unity-conventions.md` | Prefab, scene, and asmdef organization | Ownership handoff |
