# Reference Review: Photon Fusion Pirate Adventure

*Status: Draft*
*Created: 2026-05-14*
*Author: Codex*
*Local Source Reviewed: `C:\Users\JOY\Downloads\fusion-pirate-adventure-2.0.12.zip`*

> **Quick reference** - Layer: `Prototype` - Priority: `MVP` - Key deps: `Photon Fusion 2`, `Fusion Simple KCC`, `Fusion FSM`

---

## Verdict

Do **not** buy an extra paid top-down Fusion template yet.

Photon's Pirate Adventure sample is enough as the first controller reference for SECOND SPAWN. It is closer to our immediate need than Opsive UCC or a paid top-down shooter template because it demonstrates a complete Fusion top-down loop:

- local input gathered into a compact Fusion input struct
- player state machine
- Simple KCC movement
- attack hit windows
- interactables
- pickups and hazards
- enemy search and chase behavior
- NavMesh-driven enemy movement
- Multi-Peer friendly runner dictionaries

Use it as a reference, not as code to copy.

---

## Compatibility Snapshot

| Area | Pirate Adventure | SECOND SPAWN Current State | Notes |
| ---- | ---- | ---- | ---- |
| Unity version | Unity `2021.3.45f2` | Unity `6000.5.0b8` | Sample must be treated as a pattern source, not imported wholesale. |
| Fusion version | `2.0.12 Stable 1861` | `2.1.1 Release-Candidate 2037` | API drift is possible. Validate in Unity after each integration step. |
| KCC | Simple KCC addon `2.0.15` DLL | Installed at `Unity/Assets/Photon/FusionAddons/SimpleKCC/` | Strong candidate for next controller prototype branch. |
| Topology | Shared Mode | Server Mode production, Host Mode dev | Do not copy Shared Mode authority assumptions. |
| Input system | Unity Input System `1.7.0` | Unity 6 project default input setup | Reuse the struct shape, not the generated action asset. |
| Art and sample assets | Pirate and third-party sample assets | SECOND SPAWN original assets | Do not import sample art into the public repo. |

---

## Useful Patterns To Adopt

### 1. Fusion Input Shape

Pirate Adventure keeps player input small:

- movement vector
- network buttons
- one place that gathers local input
- gameplay code reads only the tick input

SECOND SPAWN already has `NetworkInputData`. Keep evolving that shape instead of binding gameplay directly to keyboard, mouse, UI, LLM, or AI-agent sources.

### 2. Simple KCC For The Next Movement Branch

Pirate Adventure uses `Fusion.Addons.SimpleKCC.SimpleKCC` for player movement. The useful pattern is:

- normalize movement input
- build an XZ movement direction
- set look rotation from movement direction
- call `SimpleKCC.Move(...)` inside Fusion simulation state
- keep movement state-specific, for example idle, run, fall, hit, attack

This matches SECOND SPAWN better than jumping straight to Opsive UCC because the authority surface stays small and readable.

### 3. State Machine Boundary

Player and enemy behavior are separated into small states:

- `PlayerIdleState`
- `PlayerRunState`
- `PlayerAttackState`
- `PlayerHitState`
- `PlayerDeathState`
- `EnemyIdleState`
- `EnemyChaseState`
- `EnemyAttackState`

This is useful for SECOND SPAWN because reincarnation, AI-agent control, BodyTime, and combat can plug into state transitions without one large controller script.

### 4. Runner Physics Scene Queries

Combat and interaction checks use the runner physics scene:

- overlap capsule for pickups and hazards
- overlap sphere for interactables
- overlap sphere for attack hit windows
- raycast for enemy line of sight

This is the right mental model for Fusion. Query the simulation scene the runner owns, not random global gameplay state.

### 5. Enemy MVP Pattern

Pirate Adventure enemies are simple but useful:

- search nearest valid player
- validate distance and line of sight
- store target as networked state
- enable NavMeshAgent only on state authority
- stop movement during attack windows

This is enough for the first dungeon trash enemy prototype before Behavior Designer enters the project.

---

## Patterns To Avoid

### Shared Mode Authority

The sample uses Shared Mode and client-owned state authority. SECOND SPAWN production uses dedicated Server Mode. Do not copy:

- `GameMode.Shared` as the project default
- Shared Mode master-client authority transfer logic
- client-side authority assumptions for enemy or player ownership

### Wholesale Sample Import

Do not import the whole Pirate Adventure project. It targets an older Unity version, older Fusion build, sample art, sample scenes, and Shared Mode assumptions.

### Sample Economy Names

Pirate Adventure uses money and level as sample progression. SECOND SPAWN uses:

- `BodyTime`
- `SECOND token`
- level and stats
- reincarnation state
- durable inventory and quest state in Supabase

Keep the gameplay naming aligned to SECOND SPAWN.

---

## Recommended Next Step

Create a feature branch from `dev` for a **Simple KCC controller spike**:

1. Keep `Player_NetworkCube.prefab` or replace it with a capsule placeholder.
2. Convert current raw position movement into KCC-backed movement.
3. Add a tiny player movement state layer only if it stays simpler than the current script.
4. Verify Play Mode console is clean.
5. Run reviewer pass before merging.

Opsive UCC should move after this spike, not before it. If Simple KCC covers the MVP movement feel, Opsive can be reserved for animation, camera, or combat evaluation instead of becoming the core controller dependency.

---

## Decision Implication

For the vertical slice, the controller path should now be:

1. Minimal Fusion movement baseline.
2. Simple KCC spike based on Pirate Adventure patterns.
3. Combat state prototype.
4. Opsive UCC evaluation only if it clearly improves animation, abilities, or combat authoring enough to justify integration cost.

This keeps SECOND SPAWN friendly to solo development and AI agents: fewer hidden asset-store assumptions, smaller code surface, and clearer server-authoritative rules.

---

## Cross-References

| This Document References | Target Doc | Specific Element Referenced | Nature |
| ---- | ---- | ---- | ---- |
| Controller plan | `07-player-controller-prototype.md` | Simple KCC spike before Opsive import | Scope dependency |
| Overview plan | `06-overview-design.md` | Playable feel before asset complexity | Scope dependency |
| Networking rules | `05-networking-architecture.md` | Server-authoritative Fusion architecture | Rule dependency |
| Systems map | `03-systems-index.md` | Player Controller entry | Tracking dependency |
