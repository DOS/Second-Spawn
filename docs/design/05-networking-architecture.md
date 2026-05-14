# Networking Architecture (Photon Fusion 2)

*Status: Draft with Phase B smoke-test implementation*
*Created: 2026-05-14*
*Implements Pillar*: AI agent 24/7, LLM as world citizen, Server-authoritative gameplay

> **Quick reference** - Layer: `Core` (foundation - everything else depends on this) - Priority: `MVP` - Key deps: `Supabase Auth (for JWT), Go gateway (for LLM intent)`

---

## Summary

Photon Fusion 2 in **Server Mode dedicated** is the canonical multiplayer runtime for SECOND SPAWN. The Unity client is a thin input + render surface; the dedicated Unity headless server is the authority for all gameplay state. The Go gateway (`backend/gateway/`) handles LLM and NFT intents; the Fusion server consumes validated intents and mutates `[Networked]` state.

The integration is built from scratch per ADR 0006 (no template drop-in). Patterns are extracted from BR200, Tanknarok, and Fusion Starter samples (read locally, not copied).

---

## Player Fantasy (what the network has to deliver)

The fantasy is "your character has a life that does not pause when yours does." This is the toughest networking requirement because:

- The character continues to exist + play when the human is offline (offline AI agent runs on the server)
- The world is persistent across player sessions (Supabase + Fusion state sync)
- Death is permanent for the body (server validates reincarnation flow + NFT escrow release)

Anything less than server-authoritative breaks the fantasy on day one of public release.

---

## Topology

```
┌──────────────────┐                    ┌─────────────────────────────┐
│  Unity Client    │                    │  Unity Dedicated Server     │
│  (thin)          │   Fusion 2 RPCs    │  (Linux headless, Hetzner)  │
│  - input         │ ◄──────────────────► - NetworkRunner             │
│  - rendering     │   (60Hz tick)      │  - Authoritative state      │
│  - prediction    │                    │  - Interest management      │
└────────┬─────────┘                    │  - Tick-driven simulation   │
         │ HTTPS                        │  - Offline AI agent loop    │
         │ + Supabase JWT               └──────────┬──────────────────┘
         ▼                                         │ HTTPS
┌──────────────────┐                               │ + Supabase JWT
│  Go LLM Gateway  │ ◄─────────────────────────────┘
│  (backend/       │
│   gateway/)      │ ──────────► Anthropic / OpenAI / Convai
│                  │ ──────────► thirdweb (DOS Chain)
└──────────────────┘ ──────────► Supabase (persistence side-channel)
```

The dedicated server NEVER trusts the client. Client predicts visually, server reconciles.

The dedicated server NEVER trusts the LLM. All LLM responses parse into structured intents (`backend/gateway/internal/intent/intent.go`); the server validates against authoritative state before mutating anything.

---

## Modes used

| Mode | When | Notes |
|---|---|---|
| **Server Mode** (dedicated) | Production | Canonical. Unity headless Linux build on Hetzner VPS. Required for vertical slice acceptance. |
| **Host Mode** | Development iteration | Photon Cloud free 20 CCU. NEVER ship to production (Hard Rule #4). |
| **Shared Mode** | Not used | Implies client-authoritative chunks - violates Pillar 4. |
| **Single Player Mode** | Not used | This is an MMO; offline play is delegated to the AI agent (also server-side). |

---

## Tick rate + interest management

- **Tick rate target**: 60Hz simulation, 30Hz network send rate baseline. Revisit if profiler shows budget pressure.
- **Interest management**: ~20 players per zone instance. Each player's `NetworkObject` AOI radius is set per-zone (default 50m, configurable per-zone). Far players are not replicated, only positional summary + chat presence.
- **Reference patterns**: BR200 sample for AOI + interest management at scale (it targets 200, we target 20; we get cheap headroom).

---

## Networked types we will own

The actual implementations land in `Assets/_SecondSpawn/Scripts/Networking/` (assembly `SecondSpawn.Networking`). Phase B has a smoke-test implementation for runner startup, keyboard input, player spawn, and placeholder networked movement.

### `NetworkRunnerSetup` (MonoBehaviour, singleton)

- Owns the `NetworkRunner` lifecycle.
- Reads `SecondSpawnConfig` for Photon App ID + environment.
- Decides Server Mode vs Host Mode from `Application.isBatchMode` (server build is always batchmode + nographics).
- Wires the `NetworkInputProvider` (input collection per tick).

### `NetworkPlayer` (NetworkBehaviour)

- Holds `[Networked]` properties: position, rotation, cultivation tier, HP, stamina, current zone.
- Server-authoritative: client predicts visually, server overrides on next tick.
- Spawned on player join (server-side spawn flow), despawned on disconnect.
- Reincarnation flow keeps `cultivationTier` (partial) - see [04-cultivation-system.md](04-cultivation-system.md).

### `NetworkInputProvider` (MonoBehaviour)

- Collects Unity Input System input each tick.
- Sends a struct of: movement direction, ability slot, target id, intent flag (e.g. interact / equip / reincarnate).
- The dedicated server consumes this and validates against authoritative state.

### `NetworkZone` (NetworkBehaviour, per-zone)

- Represents a Photon Fusion 2 room = one SECOND SPAWN zone instance.
- Tracks active players (4-20), spawned NPCs, dungeon state.
- Validates zone-bound intents (e.g. you cannot equip a Hunter skin while inside a dungeon mid-fight; you cannot trigger reincarnation while in a quest cutscene).

### `IntentBridge` (MonoBehaviour, server-only)

- Receives validated intents from the Go gateway over HTTP+JWT.
- Translates intent into Fusion state mutation (e.g. `NPCGrantItem` -> add item to `NetworkPlayer.Inventory` after server-side checks).
- Never trusts the intent blindly - re-validates against current authoritative state.

### `OfflineAgentRunner` (server-only)

- Per offline player whose character is still in a zone, runs a server-side decision loop:
  pull state -> call Go gateway with capability-cap + rate-limit headers -> receive intent -> validate -> apply.
- Inherits the player's rate limit + LLM token budget (no double-charging).
- Death of agent = body death = reincarnation flow same as player (see [04-cultivation-system.md](04-cultivation-system.md)).

---

## RPC + state authority rules

1. All `[Networked]` properties are server-authoritative. Client cannot directly write.
2. RPCs from client to server are **suggestions, not commands**. The server is free to reject any RPC if validation fails.
3. RPCs from server to client are **state updates** (read-only on the receiving side).
4. `[Networked]` collections (inventory, quest progress) are mutated on the server. Client gets a callback when they change.

---

## Persistence boundary

Photon Fusion 2 manages **session state** (in-zone networked properties).
Supabase Postgres manages **durable state** (profile, inventory snapshot, quest progress, NFT lock state, cultivation tier).

The dedicated server flushes Supabase on:

- Player disconnect (final state save)
- Zone transition (player moves between zone instances)
- Periodic interval (every N minutes, configurable)
- On reincarnation transition (mandatory before despawn)

Crash safety: the latest Supabase snapshot is the source of truth on next session. Some in-zone progress may be lost on a server crash; we accept this for vertical slice and design proper resilience later.

---

## Security invariants (mandatory)

These are non-negotiable per the AGPL-3.0 open-source threat model + Pillar 4 (Server-authoritative gameplay). If any PR violates one, it fails review.

1. **No gameplay logic on the client.** Visual prediction OK; state changes NOT.
2. **No API keys in the Unity client.** Period. The Unity client only holds:
   - Supabase URL + anon key (public-safe)
   - Gateway base URL (public)
   - Photon App ID (semi-public, client-visible by design)
3. **All LLM calls server-side via Go gateway.** The dedicated server is the only thing that hits Anthropic / OpenAI / Convai.
4. **All NFT mutations server-side via Go gateway.** The dedicated server is the only thing that signs DOS Chain transactions.
5. **Rate limit + capability cap apply to AI agent the same way they apply to the player.** No "agent gets unlimited LLM tokens" - it inherits the offline player's budget.
6. **No `Host Mode` build in production.** CI staging build must use Server Mode dedicated; PR review checks this.

---

## Performance targets (vertical slice)

| Metric | Target | Stretch |
|---|---|---|
| Tick rate | 60Hz simulation | 60Hz net-send |
| Network send rate per player | 30Hz | 60Hz |
| Concurrent players per zone | 4-20 | 30 |
| Concurrent zones per dedicated server | 1-2 | 4 |
| AI agent decision frequency | 1 per 5-15 sec per offline player | 1 per 2 sec |
| Server tick CPU budget | 16ms (60Hz) | 8ms |
| Client frame CPU budget | 16ms (60Hz) | 8ms |

Numbers will be re-validated with Fusion bot load test (per `02-vertical-slice-spec.md` acceptance criteria).

---

## Open questions (need JOY input later)

- Per-zone interest management AOI radius (50m default, but depends on zone size)
- AI agent decision loop frequency (every 5s? 15s? adaptive based on activity?)
- Server crash policy: roll back to last Supabase snapshot vs accept in-zone loss
- Photon Fusion 2 license tier when scaling beyond Cloud free 20 CCU (also in CLAUDE.md Open Decision Points)
- Dedicated server region selection (Hetzner Helsinki vs Falkenstein? US East? affects latency for non-VN players)

---

## Implementation status

| Element | Status | Where |
|---|---|---|
| Photon Fusion 2 SDK installed | Installed: Fusion 2.1.1-RC after ADR 0007 resolution | `Unity/Assets/Photon/` |
| `NetworkRunnerSetup` | Phase B smoke-test implementation | `Unity/Assets/_SecondSpawn/Scripts/Networking/NetworkRunnerSetup.cs` |
| `NetworkPlayer` | Phase B placeholder networked player cube state | `Unity/Assets/_SecondSpawn/Scripts/Networking/NetworkPlayer.cs` |
| `NetworkInputProvider` | Phase B keyboard input provider | `Unity/Assets/_SecondSpawn/Scripts/Networking/NetworkInputProvider.cs` |
| `PlayerSpawner` | Phase B server-authoritative join spawn/despawn | `Unity/Assets/_SecondSpawn/Scripts/Networking/PlayerSpawner.cs` |
| `NetworkZone` | Not started | TBD |
| `IntentBridge` | Not started | will live next to `internal/intent` in backend/gateway concepts |
| `OfflineAgentRunner` | Not started | server-only, Phase 7 |
| Test scene | `ZoneTest_Hub.unity` with scene-root `_NetworkBootstrap` | `Unity/Assets/_SecondSpawn/Scenes/ZoneTest_Hub.unity` |
| Load test (Fusion bots) | Not started | Phase 8 |

---

## Reference reading list (BEFORE writing networking code)

In order, per ADR 0006:

1. [Fusion Starter docs](https://doc.photonengine.com/fusion/current/game-samples/) - read first for idioms
2. [BR200 overview](https://doc.photonengine.com/fusion/current/game-samples/br200/overview) - server architecture, dedicated mode, interest management
3. [Tanknarok overview](https://doc.photonengine.com/fusion/current/game-samples/fusion-tanknarok) - top-down controller patterns, wave spawn (close to dungeon trash mob pattern)
4. JOY's [MetaDOS](file:///D:/Projects/MetaDOS) (read-only) - what worked, what JOY's adaptations were
5. [Fusion Server Mode docs](https://doc.photonengine.com/fusion/current/manual/connection-and-matchmaking/server-mode)
6. [Fusion Interest Management addon docs](https://doc.photonengine.com/fusion/current/addons/interest-management/overview)

The scratch project for reading these samples should live OUTSIDE this repo (e.g. `D:/Projects/fusion-samples-scratch/`) so we never accidentally commit Photon's template source into AGPL-3.0 SECOND SPAWN.
