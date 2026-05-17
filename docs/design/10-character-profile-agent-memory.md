# Character Profile, Soul, and Agent Memory

*Status: Prototype implemented*
*Created: 2026-05-15*
*Author: Codex*
*Last Verified: 2026-05-17 against `AGENTS.md`, ADR 0003, ADR 0004, `08-time-as-currency.md`, Cloud Run staging gateway, Nakama runtime smoke, and Unity Play Mode profile sync*

> **Quick reference** - Layer: `Persistence / AI Agent` - Priority: `MVP foundation` - Key deps: `Auth`, `Fusion server authority`, `LLM gateway`, `Time-as-Currency`, `Reincarnation`

---

## Purpose

This document defines the first durable character data model for SECOND SPAWN:

- account-level player profile
- current synthetic body profile
- gameplay stats
- soul/personality profile for the LLM agent
- compact memory records for agent context
- player-owned offline-agent policy
- NPC-like body profiles that can later receive a player consciousness

The goal is to make every active character body feel like a real world actor, without giving the LLM authority over game state.

Important spawn rule: a player does not spawn as an empty account shell. The
player enters a current body, which may be implemented as an NPC-like synthetic
body with its own stats, characteristics, soul profile, memory, BodyTime, and
activity history. Player identity survives across bodies. Body-specific state
can be replaced on reincarnation or consciousness transfer.

---

## Core Rule

The LLM reads profile, soul, policy, memory, and world state. It emits structured intent. The server validates the intent. Only the server mutates gameplay state.

This preserves:

- open-source anti-cheat assumptions
- Hard Rule #2: never let LLM mutate authoritative game state
- Hard Rule #3: no provider keys in Unity
- the reincarnation fantasy, where identity survives but bodies are replaceable

---

## Data Layers

| Layer | Survives Reincarnation? | Owner | Purpose |
| ---- | ---- | ---- | ---- |
| `PlayerProfile` | Yes | Auth / backend | Account identity, display name, wallet link, moderation handles |
| `SoulProfile` | Yes | Player + backend validation | Personality, long-term goals, behavior style for offline AI |
| `AgentPolicy` | Yes | Player | What the offline agent is allowed to do while player is away |
| `BodyProfile` | No | Game server | Current synthetic body, visual archetype, BodyTime, lifecycle |
| `CharacterStats` | Mostly no | Game server | Combat and movement-affecting numbers for current body |
| `MemoryRecord` | Yes, with decay | Backend | Small curated memory facts for LLM context |
| `AgentRuntime` | Yes, across bodies until reset policy exists | Backend | Counters for profile bootstrap, activity, decisions, fallback decisions, and offline time |
| `AgentActivity` | Yes, bounded recent history | Backend | Compact audit trail for offline-agent sessions and Unity/Nakama bootstrap events |

NPCs and player-controlled bodies use the same body profile shape. The
difference is authority and ownership: a player-controlled body receives human
input or offline-agent intent for that player, while an NPC body receives NPC
brain intent. Both still pass through server validation before gameplay state
changes.

---

## Player Profile

`PlayerProfile` is account-level identity. It should stay small.

Required fields for the first implementation:

| Field | Meaning |
| ---- | ---- |
| `player_id` | Nakama user ID issued after Supabase Auth bridge verification |
| `display_name` | Public player name |
| `wallet_address` | Optional DOS Chain wallet link |
| `created_at` | Account creation timestamp |

Do not store LLM prompt text, inventory blobs, or current body state here.

Identity bridge rule:

1. Supabase Auth creates the external identity, including anonymous prototype
   users.
2. Unity sends the Supabase access token to Nakama custom auth.
3. Nakama verifies that token directly with Supabase Auth before creating or
   loading the game account.
4. Nakama stores the resulting stable custom ID as the game account binding.

Unity must not send a plain `supabase_user_id` as a trusted account selector.

---

## Body Profile

`BodyProfile` represents the current synthetic vessel.

Required fields:

| Field | Meaning |
| ---- | ---- |
| `body_id` | Unique current body ID |
| `archetype_id` | Gameplay archetype or class key |
| `visual_prefab_key` | Local Unity visual prefab key, used for random spawn visuals later |
| `body_time` | Current BodyTime state |
| `stats` | Current level and combat stats |
| `lifecycle` | `alive`, `dying`, `reincarnating`, or `dead` |
| `created_at` | Body creation timestamp |

`BodyProfile` is replaced on reincarnation. The server decides which values carry forward.

---

## Character Stats

Start with a small stat surface:

| Stat | Purpose |
| ---- | ---- |
| `level` | Local body level |
| `vitality` | Health scaling |
| `force` | Physical damage |
| `agility` | Movement and attack cadence |
| `focus` | Energy and ability use |
| `resilience` | Damage mitigation |
| `max_health` | Derived or cached health cap |
| `max_energy` | Derived or cached energy cap |
| `attack_power` | Derived or cached attack output |
| `defense_power` | Derived or cached defense output |

Design note: derived values may be cached for performance, but the server must own recalculation rules.

---

## Soul Profile

`SoulProfile` is the durable personality layer used by the offline LLM agent.

It is not a stat buff system. It should never grant combat advantages directly.

Fields:

| Field | Meaning |
| ---- | ---- |
| `name` | In-lore consciousness name |
| `core_drive` | The character's main motivation |
| `temperament` | Cautious, aggressive, curious, loyal, etc. |
| `combat_style` | Preferred combat posture for LLM decision context |
| `social_style` | How the agent speaks and socializes |
| `moral_boundaries` | Things the agent should not do |
| `long_term_goals` | Player-approved durable goals |
| `player_notes` | Short free-form guidance from player |
| `reincarnation_lore` | What the character remembers about prior bodies |

The LLM may use these fields to choose between valid intents, not to invent new abilities.

### NPC and Body Profile Rule

Every NPC-like actor that can think, speak, fight, or receive a player
consciousness needs its own profile bundle:

- `BodyProfile`
- `CharacterStats`
- `CharacterTraits`
- `SoulProfile`
- `MemoryRecord`
- `AgentPolicy` or NPC policy equivalent
- `AgentRuntime`
- `AgentActivity`

The vertical slice can store prototype NPC profiles using the same agent context
shape as player bodies. Later production work may split durable account data,
body templates, NPC definitions, and live body instances into separate storage
records, but the runtime contract should stay consistent: the agent always sees
the specific body it is currently controlling.

---

## Agent Policy

`AgentPolicy` is direct player control over offline behavior.

Vertical slice minimum:

| Field | Meaning |
| ---- | ---- |
| `enabled` | Offline agent on/off |
| `mode` | `idle`, `farm_safe_area`, `socialize`, or `quest_assist` |
| `max_session_seconds` | Maximum autonomous session length |
| `allow_body_time_spend` | Whether the agent may spend BodyTime |
| `allow_risky_combat` | Whether the agent may attack high-risk targets |
| `preferred_activities` | Player-prioritized actions |
| `forbidden_activities` | Explicitly disallowed actions |
| `stop_when_body_time_below` | BodyTime safety threshold |

The agent must stop or downgrade behavior when policy and world risk conflict.

---

## OpenClaw Agent NPC Bridge

A connected OpenClaw agent is a user-owned external agent that can appear in SECOND SPAWN as an NPC-like world actor.

This is not the same as the player's offline agent:

| Actor | Primary Owner | In-World Role | Authority |
| ---- | ---- | ---- | ---- |
| Offline player agent | Player character owner | Controls the player's current body while offline | Emits action intent; Fusion server validates |
| OpenClaw-connected NPC | OpenClaw agent owner | Companion, hub NPC, merchant-like persona, quest-adjacent social actor, or world citizen | Emits dialogue or action intent; Fusion server validates |

Minimum data contract:

| Field | Meaning |
| ---- | ---- |
| `connected_agent_id` | Stable ID for the OpenClaw agent |
| `owner_player_id` | Player who connected the agent |
| `display_name` | Public in-game agent name |
| `agent_kind` | Companion, hub_npc, merchant_persona, quest_actor, social_actor |
| `consent_scope` | What the owner allows the agent to do in-game |
| `moderation_state` | Active, limited, suspended, or blocked |
| `memory_scope` | Which memories can be read or written |
| `rate_limit_profile` | Token and action limits for this connected agent |

Safety rules:

- OpenClaw agents are untrusted external actors from the game server's point of view.
- They never mutate inventory, currency, quest, BodyTime, level/stats, combat, or world state directly.
- They may produce dialogue, social memory, and structured intent.
- Nakama owns identity binding, consent, moderation, rate limit, and activity logging.
- `api.dos.ai` / Go LLM Gateway owns prompt safety and model routing.
- Fusion server remains the final validator for any in-world action.

Design intent: OpenClaw agents should make SECOND SPAWN feel like an extension of the DOS.AI agent ecosystem, where users can bring their own agents into the world as social citizens rather than leaving them outside the game.

---

## Memory Records

`MemoryRecord` is the compact input set for agent context.

Memory kinds:

| Kind | Example |
| ---- | ---- |
| `preference` | Player prefers cautious farming over risky boss attempts |
| `quest` | Player promised to return to an NPC after finding a part |
| `relationship` | Player helped another agent yesterday |
| `combat` | Character struggles against ranged enemies |
| `system` | Tutorial or safety note the agent should remember |

Memory records should be short. Store summaries, not raw transcripts.

Vertical slice rule: the LLM receives only the top N memories by importance and recency.

---

## Agent Runtime and Activity

`AgentRuntime` is the compact operational counter block for the offline-agent
prototype. It is not authoritative gameplay state and must not be used to grant
items, XP, currency, BodyTime, or level/stat progress without a separate
server-side rule.

Tracked counters:

| Field | Meaning |
| ---- | ---- |
| `profile_bootstrapped_at` | First time the Nakama profile/body context was created |
| `last_profile_bootstrap_at` | Last time the profile bootstrap path refreshed the context |
| `last_activity_at` | Timestamp of the latest agent activity event |
| `activity_count` | Number of activity events accepted by Nakama |
| `decision_count` | Number of server-side prototype decisions returned |
| `fallback_decision_count` | Number of deterministic fallback decisions or reported fallback decisions |
| `move_intent_count` | Count of move intents returned or reported |
| `say_intent_count` | Count of say intents returned or reported |
| `stop_intent_count` | Count of stop intents returned or reported |
| `interact_intent_count` | Count of interact intents returned or reported |
| `offline_seconds` | Reported offline-agent session seconds |

`AgentActivity` is a bounded recent history list. Nakama stores the latest 32
activity records on the profile context. Current accepted kinds are:

- `profile_bootstrap`
- `offline_session`
- `agent_decision`
- `memory_sync`
- `manual_note`

Unity writes a `profile_bootstrap` activity after Nakama auth confirms that the
player profile exists. Nakama also records `agent_decision` activity when the
runtime decision RPC returns a deterministic prototype intent.

---

## Agent Context Prompt

Backend code now defines a prompt-safe context builder in:

`backend/gateway/internal/character/profile.go`

The builder converts a bounded `AgentContext` into stable key-value text. This is intentionally boring and deterministic so model behavior is easier to debug.

The context includes:

- player identity
- current body identity
- visual/archetype key
- lifecycle
- level and stats
- BodyTime budget
- agent policy
- soul fields
- top memory summaries

It does not include:

- API keys
- service-role credentials
- raw chat transcripts
- raw inventory blobs
- untrusted client-provided claims

---

## Agent Decision Contract

Backend code now defines the first bounded offline-agent decision contract in:

`backend/gateway/internal/agent/decision.go`

Allowed action types for the first implementation:

| Action | Meaning |
| ---- | ---- |
| `stop` | Do nothing or pause because policy/risk says to stop |
| `move` | Request movement toward a bounded position |
| `attack` | Request attack against a nearby allowed target |
| `interact` | Request interaction with a nearby allowed object |
| `say` | Produce dialogue/social text with no direct state mutation |

The gateway validator checks:

- action is in the allowed set for this request
- confidence is bounded from 0 to 1
- move actions include coordinates
- attack actions reference a nearby target from the safe snapshot
- interact actions reference a nearby object from the safe snapshot
- say actions include text

This does not replace authoritative game-server validation. It is only the first filter between model output and the gameplay server.

---

## Prototype Implementation Status

Implemented surfaces:

- `backend/nakama/modules/index.ts` is the current game-backend source for
  player profile, current body, soul, agent policy, BodyTime, level/stats, and
  compact memories. It also stores `agent_runtime` counters and a bounded
  `agent_activity` log. It exposes `secondspawn_profile_get`,
  `secondspawn_memory_add`, `secondspawn_soul_update`,
  `secondspawn_agent_activity_add`, and `secondspawn_agent_decide` runtime RPCs.
- Nakama runtime module tests cover Supabase custom-auth rewriting, profile
  bootstrap, memory dedupe, soul update clamping, deterministic fallback agent
  decisions, runtime counters, and activity logging.
- Local Unity Play Mode can use Nakama device auth as a development fallback
  when Supabase anonymous auth is not configured yet. Production account binding
  must use Supabase custom auth or a later approved identity ADR.
- `backend/gateway/internal/character` stores prototype `AgentContext` with
  profile, body, stats, characteristics, soul, policy, BodyTime,
  and compact memories for LLM-gateway fallback and standalone cloud smoke
  tests.
- Prototype memory writes deduplicate by memory kind and summary. Repeated
  Unity Play Mode sessions update the existing memory timestamp instead of
  appending the same seed memory again.
- `backend/gateway/internal/agent` validates model-backed JSON decisions from
  bounded context and safe world snapshots. If no provider key is configured,
  provider calls fail, or the model returns invalid intent, the endpoint falls
  back to deterministic prototype decisions.
- Cloud Run staging gateway:
  `https://second-spawn-gateway-535583621422.asia-southeast1.run.app`
- Unity `SecondSpawnGatewayClient` authenticates with Nakama, reads/writes
  Nakama profile memory when a Nakama session exists, and calls the cloud
  gateway for NPC text chat, voice-session contract, and prototype LLM decision.
- Unity `CharacterMemorySync` loads the Nakama profile and applies the current
  body state onto the authoritative local `NetworkPlayer`.
- The current player prototype starts at level 1 with vitality 10, force 8,
  agility 8, focus 8, resilience 8, health 100, energy 50, attack 10, and
  defense 5.
- The prototype HUD shows level, HP, energy, attack, defense, agility,
  BodyTime, lifecycle, SECOND balance, and reincarnation count.
- The current prototype account reserve starts with 604800 SECOND seconds and
  reincarnation costs 432000 SECOND seconds.
- Unity `PrototypeLLMAgentDriver` can toggle prototype agent control with `P`.
- Unity `PrototypeNPCChatClient` can trigger prototype NPC chat with `O` and
  voice-session status with `V`.
- Unity prototype speech uses a world-space text bubble plus local audio cue.
  This is not real TTS yet; provider-backed voice still requires an ephemeral
  token endpoint.

Current limitations:

- Gateway storage is in-memory and resets on Cloud Run revision restart. Nakama
  storage is the game-backend path for durable profile/memory.
- Most gateway routes are prototype-public. The Nakama runtime auth hook
  verifies Supabase access tokens for game login, but route-level JWT
  enforcement is still required before any non-local LLM or voice playtest.
- Agent decisions have an Anthropic-backed JSON intent path, but local
  development and provider failures still use deterministic fallback logic.
- Voice is a local cue only. Real voice waits for OpenAI Realtime or ElevenLabs
  server-side token minting.

---

## Random Visual Selection

Random model selection should use `visual_prefab_key`, not direct filesystem paths.

Flow:

1. Server selects a valid visual key from an approved archetype pool.
2. The spawned body stores that key in `BodyProfile`.
3. Unity resolves the key to a local visual prefab.
4. If the key is missing locally, Unity falls back to the default prototype visual and logs a warning.

This keeps spawn visuals deterministic across clients and avoids letting clients choose invalid models.

---

## LLM Control Flow

Initial offline-agent loop:

1. Fusion server snapshots safe world state.
2. Backend loads `PlayerProfile`, current `BodyProfile`, `SoulProfile`, `AgentPolicy`, and top memories.
3. Gateway builds LLM context.
4. LLM returns structured intent only.
5. Gateway validates the decision shape and allowed action set.
6. Server validates intent against current authoritative state.
7. Server applies allowed movement/combat/social action through the same path as player input.
8. Backend appends an activity log entry for player review.

The first playable version should support only a small intent set: move within safe bounds, attack allowed target, interact with allowed object, and stop.

---

## Open Questions

| Question | Owner | Timing | Current Lean |
| ---- | ---- | ---- | ---- |
| Should `SoulProfile` be editable at character creation only or anytime in hub? | JOY | Before profile UI | Editable in hub with history log |
| How many memories are included in the first LLM context? | Codex | During LLM prototype | 8-12 short summaries |
| Does reincarnation decay memories? | JOY | Before reincarnation MVP | Keep major memories, decay body-specific combat memories |
| Should visual randomization happen per account, per body, or per spawn? | JOY | Before model pool implementation | Per body, because body identity matters |
| Which profile fields stay in Nakama storage vs Supabase sidecar? | JOY + Codex | Before profile UI | Nakama owns game profile; sidecar only for external product data |
| Which OpenClaw agent roles are allowed in the first prototype? | JOY + Codex | Before OpenClaw bridge prototype | Start with social hub NPC or companion, no inventory or economy actions |

---

## Acceptance Criteria

- [x] Backend data contract exists for profile, body, stats, soul, policy, and memory.
- [x] LLM context builder is deterministic and bounded.
- [ ] Random visual selection has a server-owned key contract.
- [ ] OpenClaw-connected NPCs have identity binding, consent scope, moderation state, and rate limits before any prototype.
- [x] Unity never sends provider keys or direct state mutations.
- [x] Offline-agent intent flow uses the same network input shape as player input for prototype movement.
- [ ] Player can later inspect what the agent did and why.

---

## Cross-References

| This Document References | Target Doc | Specific Element Referenced | Nature |
| ---- | ---- | ---- | ---- |
| Pillar rules | `01-pillars.md` | AI agent 24/7, server authority | Rule dependency |
| Systems map | `03-systems-index.md` | Profile persistence and AI agent systems | Build dependency |
| LLM safety | `../adr/0003-llm-safety-architecture.md` | Intent validation | Security dependency |
| Offline agent | `../adr/0004-ai-agent-offline-control.md` | Server-side agent control | Architecture dependency |
| Time economy | `08-time-as-currency.md` | BodyTime policy and risk | Economy dependency |
