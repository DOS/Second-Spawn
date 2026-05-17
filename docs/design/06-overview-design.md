# Overview Design: SECOND SPAWN Vertical Slice

*Status: Draft*
*Created: 2026-05-14*
*Author: Codex*
*Last Verified: 2026-05-14 against `AGENTS.md`, `00-game-concept.md`, `01-pillars.md`, `02-vertical-slice-spec.md`, and `05-networking-architecture.md`*

> **Quick reference** - Layer: `Core` - Priority: `Vertical Slice` - Key deps: `Photon Fusion 2`, `Nakama OSS`, `api.dos.ai / Go LLM Gateway`, `DOS Chain`, `Convai phase 1`

---

## Purpose

This document is the short "what are we building" design anchor for the first playable SECOND SPAWN prototype and vertical slice. It does not replace the concept, pillars, architecture, or per-system GDDs. It gives agents a single product-shape overview before they write code, import Unity assets, or propose scope.

---

## One-Sentence Game

SECOND SPAWN is a near-future, post-disaster, top-down ARPG MMO where a player's character keeps living through an AI agent while the player is offline, TIME acts as the life medium, SECOND acts as the unit/currency, and death transfers a neural imprint into a new bio-synthetic Frame instead of simply respawning.

---

## Target First Playable Experience

The first prototype should prove that the game can become a multiplayer ARPG before it tries to prove every unique system at once.

The intended first playable loop is:

1. Player opens `ZoneTest_Hub`.
2. Photon Fusion starts a local development session.
3. A placeholder networked player spawns.
4. The player moves in top-down ARPG style.
5. A top-down camera follows the controlled character.
6. The scene remains console-clean.

This is intentionally smaller than the vertical slice. It is the foundation that makes the vertical slice believable.

---

## Vertical Slice Promise

The 3-6 month vertical slice must let a solo player experience the identity-defining hooks within one compact zone:

| Hook | Slice Expression |
| ---- | ---- |
| AI agent 24/7 | The offline agent farms one designated area and produces an activity log visible when the player returns. |
| Reincarnation | Death consumes test SECOND and transfers a neural imprint to a new Frame with selected profile carryover. |
| TIME / SECOND economy | The current Frame has a TIME budget measured in SECOND that can be earned, spent, and depleted to trigger death pressure. |
| Neural imprint transfer | The death and spawn flow is framed as Frame transfer, not spiritual resurrection. |

The slice does not need large content volume. It needs a tight loop that proves the game's identity.

---

## Design Priorities

1. **Server-authoritative foundation first.** If movement and combat are not server-owned, the open-source multiplayer design collapses.
2. **Playable feel before asset complexity.** A placeholder capsule that moves correctly is more valuable than an imported controller that fights the networking model.
3. **One zone, one player archetype, one loop.** Every system should serve the single-zone vertical slice before generalizing.
4. **AI systems must be grounded in game state.** LLMs can talk, reason, and suggest intents; they cannot grant state directly.
5. **Reincarnation must feel like identity transfer, not a respawn button.** The prototype should preserve that framing even before the full economy exists.

---

## Player Controller Direction

The current direction is **minimal networked controller first, Fusion Simple KCC spike second, Opsive evaluation third**.

Opsive Ultimate Character Controller is already purchased and may still be useful for combat, animation, ability handling, or camera tooling. It is not currently treated as mandatory for the first prototype because:

- Fusion authority and input flow must be proven before adding a heavy third-party controller.
- Top-down ARPG movement may be simpler than the full Opsive UCC feature set.
- Unity 6.5 beta compatibility must be validated before betting the prototype on it.
- Photon's Pirate Adventure sample already demonstrates a smaller Fusion-native top-down controller path with Simple KCC.

The first prototype should create a small, project-owned movement contract. Simple KCC can then be tested against that contract. Opsive can be imported later and judged against both.

---

## Prototype Layers

| Layer | Prototype Target | Notes |
| ---- | ---- | ---- |
| Network | Fusion Host Mode dev session, Server Mode preserved as production direction | Host Mode is dev-only. |
| Player | One networked placeholder player with top-down movement | No combat yet. |
| Camera | Top-down follow camera | Can be simple Cinemachine or custom follow. |
| Zone | `ZoneTest_Hub` only | No dungeon, no multi-zone. |
| Config | `SecondSpawnConfig.asset` exists with public-safe fields | No secrets in Unity. |
| Persistence | Not in first playable | Nakama OSS comes after movement baseline. |
| AI/LLM | Not in first playable | Design must keep path open. |
| TIME / SECOND economy | Not in first playable | First implementation belongs with reincarnation/progression, not movement. |
| NFT | Not in first playable | No chain dependency for movement prototype. |

---

## Out of Scope for First Prototype

- Opsive UCC import as a dependency for movement baseline
- Behavior Designer
- Convai
- Synty / Quaternius environment art packs
- Combat damage, loot, inventory, or item drops
- Nakama auth or profile persistence
- NFT ownership and escrow
- Offline AI agent behavior
- Dungeon instance
- Reincarnation mechanics
- TIME / SECOND economy

These are still vertical slice systems. They are only excluded from the first playable prototype.

---

## Build Sequence

| Step | Output | Exit Criteria |
| ---- | ---- | ---- |
| 1 | Minimal networked player controller | Player spawns and moves in Play Mode without console errors. |
| 2 | Top-down camera | Camera follows the local player and keeps the placeholder readable. |
| 3 | Prototype control feel pass | Movement feels crisp enough for ARPG iteration. |
| 4 | Simple KCC spike | Import official Fusion Simple KCC addon and compare it against the baseline. |
| 5 | Opsive evaluation branch | Import Opsive in isolation and compare value/cost against the baseline and Simple KCC spike. |
| 6 | Combat prototype | Add one basic attack only after movement is stable. |
| 7 | Persistence/auth prototype | Nakama profile and login once local gameplay loop exists. |

---

## Acceptance Criteria

- [ ] `ZoneTest_Hub` can enter Play Mode with no errors.
- [ ] One local player spawns through Fusion.
- [ ] The player can move in a top-down plane with responsive input.
- [ ] The camera follows the controlled player without jitter or disorientation.
- [ ] The implementation keeps server-authoritative movement boundaries explicit.
- [ ] No gameplay-affecting API key or secret is stored in Unity.
- [ ] The prototype can be reviewed without importing any large third-party asset.

---

## Open Questions

| Question | Owner | Timing | Current Lean |
| ---- | ---- | ---- | ---- |
| WASD, click-to-move, or both for first prototype? | JOY | Before movement polish | WASD first, click-to-move later. |
| Use Cinemachine for camera now? | Codex | During camera task | Use it if already available; otherwise simple custom follow. |
| Does Simple KCC become the MVP controller? | Codex + Claude reviewer | After Simple KCC spike | Likely candidate if it stays console-clean and server-authoritative. |
| Does Opsive become core or optional? | Codex + Claude reviewer | After isolated Opsive branch | Optional until proven worth the integration cost. |
| What is the first Hunter visual? | JOY | After movement baseline | Placeholder until MetaDOS skin import path is reviewed. |

---

## Cross-References

| This Document References | Target Doc | Specific Element Referenced | Nature |
| ---- | ---- | ---- | ---- |
| Core identity | `00-game-concept.md` | Elevator pitch and core fantasy | Design dependency |
| Pillar priority | `01-pillars.md` | Server-authoritative gameplay, AI agent 24/7, reincarnation, TIME / SECOND economy | Rule dependency |
| Slice scope | `02-vertical-slice-spec.md` | Build phases and acceptance criteria | Scope dependency |
| Systems order | `03-systems-index.md` | NetworkRunner, Player Controller, Camera, Input | Build dependency |
| Network contract | `05-networking-architecture.md` | NetworkRunnerSetup, NetworkPlayer, NetworkInputProvider | Technical dependency |
