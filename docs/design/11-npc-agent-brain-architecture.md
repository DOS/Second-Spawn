# NPC Agent Brain Architecture

*Status: Prototype direction*
*Created: 2026-05-16*
*Author: Codex*

> **Quick reference** - Layer: `AI Agent / NPC` - Priority: `Prototype foundation` - Key deps: Fusion server authority, Nakama game backend, api.dos.ai / Go LLM Gateway, Behavior Designer, character memory

---

## Purpose

SECOND SPAWN needs NPCs and offline player agents that feel alive, but still obey game-server authority. This document defines the brain architecture before the prototype grows into a pile of one-off scripts.

The core design is:

```text
Sense -> Context -> Decide -> Validate -> Act -> Reflect
```

The LLM is only one node in the graph. It never owns movement, combat, inventory, TIME, SECOND, quest state, or any other authoritative mutation.

---

## Framework Direction

### Recommended Hybrid

| Layer | Pattern / Framework Inspiration | Why |
| ---- | ---- | ---- |
| Backend brain orchestration | LangGraph-style state graph | Durable, stateful, inspectable agent flow with explicit nodes and transitions |
| Unity NPC execution | Behavior Tree / Opsive Behavior Designer | Mature Unity authoring model for NPC action execution, conditions, decorators, and designer-visible debugging |
| Agent identity and memory | OpenClaw-style workspace/persona/skills boundary | Useful concepts for `Soul`, `Memory`, `Policy`, and connected OpenClaw agents |
| Provider wrapper | OpenAI Agents SDK-style guardrails/tracing | Useful later for tool guardrails, output validation, and traceability around model calls |

Do not embed a general-purpose desktop agent runtime directly in Unity. Game NPCs need bounded capabilities, predictable ticks, and server validation.

---

## Why Not Pure OpenClaw

OpenClaw is valuable as an external user-agent ecosystem and as a mental model for persona, workspace, skills, channels, and sessions.

It is not the best in-game NPC brain runtime because:

- It is designed around operating-system/workspace automation, not server-authoritative gameplay ticks.
- Skills and tools can be broad, while game NPC capabilities must be narrow and validated.
- Its session model is useful for external OpenClaw-connected NPCs, but the Fusion server still needs final say on every in-world action.

Second Spawn should support OpenClaw agents connecting into the world, but the game runtime should expose a constrained bridge, not hand the game world directly to OpenClaw.

---

## Brain Graph

```text
Bootstrap
  -> Sense
  -> LoadContext
  -> Decide
  -> ValidateIntent
  -> Act
  -> Reflect
  -> Cooldown
  -> Sense
```

### Bootstrap

Creates or loads:

- `PlayerProfile` or NPC identity
- current body profile
- `SoulProfile`
- `AgentPolicy`
- top memories
- local Unity actor reference

### Sense

Builds a safe world snapshot:

- zone ID
- position
- nearby interactables
- nearby threats
- visible player/NPC actors
- current TIME measured in SECOND
- current high-level state

Never sends raw Unity scene objects or client-owned claims as trusted facts.

### LoadContext

Loads compact context from the game backend:

- soul
- characteristics
- policy
- top memories
- current body status

### Decide

Returns one structured intent:

| Intent | Meaning |
| ---- | ---- |
| `stop` | Do nothing or pause |
| `move` | Move toward bounded target |
| `say` | Produce social text |
| `interact` | Request interaction with nearby object |
| `attack` | Request attack against nearby allowed target |

### ValidateIntent

Checks intent shape and policy:

- action is allowed in this state
- target exists in the safe snapshot
- move target is inside allowed radius
- loaded TIME, cooldown, and risk policy allow the action
- no direct economy/inventory/quest mutation is requested

### Act

Maps intent into game execution:

- `move` -> Fusion input / NPC navigation
- `say` -> chat bubble / voice contract
- `interact` -> server-side interaction request
- `attack` -> server-side combat request
- `stop` -> idle

### Reflect

Writes compact memory only when useful:

- major player preference
- quest commitment
- social relationship change
- combat lesson
- safety event

No raw transcripts by default.

---

## Unity Prototype Shape

The first prototype is local-only:

- `PrototypeAgentBrain` drives one local NPC actor in the hub.
- It uses Nakama RPCs for game profile, soul, policy, and compact memory when a
  Nakama session exists.
- It uses the Cloud Run gateway for prototype LLM/chat/voice contracts and falls
  back to Nakama deterministic decisions if the LLM gateway is unavailable.
- It moves inside a small patrol radius.
- It can speak with a text bubble and prototype voice cue.
- It does not mutate game state.

Later production shape:

- Behavior Designer handles action execution trees.
- `api.dos.ai` / Go LLM Gateway hosts AI graph nodes and provider calls.
- Nakama stores profile, policy, memory, consent, moderation, and audit logs.
- Fusion server validates and applies in-world actions.

---

## OpenClaw Agent Bridge

User-owned OpenClaw agents can become in-world NPC-like actors through a bridge:

```text
OpenClaw agent -> Pull Frame context -> Submit intent -> Nakama policy/moderation -> Fusion validated action
```

The game does not import or execute the OpenClaw agent's workspace files. The
external agent keeps its own `AGENTS.md`, `SOUL.md`, `MEMORY.md`, tools, and
private reasoning files. SECOND SPAWN exposes a structured read model for the
controlled Frame and accepts structured intent requests.

Required game-side contract:

| Contract Piece | Purpose |
| ---- | ---- |
| `FrameIdentity` | Public role, callsign, profession, faction title, and reputation context |
| `FrameSoul` | Bounded behavior style and durable motivation for prompt context |
| `FrameBody` | Current body, stats, TIME, lifecycle, equipment, and world snapshot |
| `FrameMemory` | Bounded summaries and relationship facts |
| `FramePolicy` | Player or server-owned constraints |
| `FrameTools` | Allowed request schema, not executable tools |
| `FrameHeartbeat` | Connection state and last-decision observability |
| Control binding | `frame_actor_id`, `controller_type`, `connected_agent_id`, owner, consent, moderation, and rate limit |

Deferred or unnecessary for MVP:

- Mirroring OpenClaw `AGENTS.md` into a `FrameAgents` backend layer.
- Mirroring OpenClaw `SKILL.md` into a `FrameSkill` backend layer before the
  combat/profession systems need structured skill records.
- Importing raw OpenClaw `.md` files into Nakama storage.

Allowed first roles:

- social hub NPC
- companion observer
- quest-adjacent dialogue actor

Disallowed until later:

- inventory mutation
- economy mutation
- TIME spending
- combat authority
- quest completion authority

---

## Implementation Rule

Every brain implementation must keep these boundaries:

1. LLM output is intent, not state.
2. Unity client can visualize and request, not authorize.
3. Gateway can validate shape and policy, not own authoritative world state.
4. Fusion server owns movement/combat/world application.
5. Nakama owns durable game profile, memory, policy, moderation, and audit.

---

## Current Prototype Acceptance

- [x] Character profile, soul, policy, and memory exist in Nakama runtime RPCs.
- [x] Gateway keeps prototype LLM/chat/voice contracts separate from game backend.
- [x] Unity can call gateway from Play Mode.
- [x] Unity can authenticate to Nakama with local device fallback.
- [x] Local player agent prototype can move through bounded input intent.
- [x] Local NPC brain exists in scene and runs the brain loop.
- [x] Brain loop logs phase transitions in a debug-friendly way.
- [x] NPC can patrol and speak without Unity console errors.
- [x] Backend decision endpoint is upgraded from deterministic fallback to model-backed JSON intent.
