# SECOND SPAWN - Architecture

High-level architecture overview. For detailed component design see `docs/design/` and ADRs in `docs/adr/`.

## System Diagram (logical)

```
+-------------------+   +------------------------+   +----------------------+
|   Unity Client    |   |  AI Agent (offline)    |   | OpenClaw Agent NPC   |
|  (player online)  |   |  controls character    |   | user-owned agent     |
|                   |   |  when player away      |   | as world actor       |
+---------+---------+   +-----------+------------+   +----------+-----------+
          |                         |                           |
          |      Photon Fusion 2 (tick 30Hz) / validated intents |
          v                         v                           v
   +-------------------------------------------------+
   |        Dedicated Game Server (headless)         |
   |        - Authoritative game state               |
   |        - Combat / movement / physics            |
   |        - Zone instance management               |
   |        - Validates all action intents           |
   +-------+------------------+----------------------+
           |                  |
           |                  | LLM intent request
           v                  v
   +---------------+   +----------------------------+
   | Nakama OSS    |   | api.dos.ai model service |
   | - Game APIs   |   | - Convai (phase 1)         |
   | - Social      |   | - Anthropic + OpenAI (P2)  |
   | - Storage     |   | - RAG memory retrieval     |
   | - Postgres    |   | - AI rate limit + safety   |
   +---------------+   +-------------+--------------+
           |                         |
           v                         v
   +---------------+   +----------------------------+
   | DOS Chain     |   | Supabase Sidecar / Redis   |
   | (NFT, wallet) |   | - Identity bridge          |
   | via thirdweb  |   | - External profile data    |
   |               |   | - Rate limit / cache       |
   +---------------+   +----------------------------+
```

## Component Responsibilities

### Unity Client

- Render, input, local prediction
- Communicates only with Fusion server and approved auth/backend endpoints
- NEVER calls LLM API directly
- NEVER holds API keys
- Receives state updates via Fusion tick

### Dedicated Game Server (Photon Fusion 2 Server Mode)

- Source of truth for in-zone state (position, HP, combat, drops)
- Source of truth for `BodyTime` earn, spend, drain, transfer, and expiration
- Validates every action intent (from player input or AI agent)
- Persists durable state through Nakama/Postgres (snapshots + events)
- Triggers `api.dos.ai` for NPC dialogue when triggered
- Manages zone lifecycle (load / unload / spawn)
- Tick rate: 30Hz (60Hz for boss encounters if needed)

### AI Agent (offline player simulation)

- Runs as a game-server worker, Nakama runtime task, or separate worker if needed
- Subscribes to Fusion server state for offline characters
- Decision loop: read state -> reason via LLM -> emit action intent
- Subject to same server validation as a real player
- Inherits player's current body stats, persona, and history

### OpenClaw-Connected NPC

- User-owned OpenClaw agent connected into SECOND SPAWN as an NPC-like world actor
- May appear as a companion, hub NPC, merchant-like persona, quest-adjacent character, or social world citizen
- Bound to Nakama identity, consent scope, moderation state, rate limits, and activity logs
- Uses `api.dos.ai` for prompt safety, provider routing, and memory context
- Emits dialogue or structured intent only
- Never mutates gameplay state directly; Fusion server validates every in-world action

### Nakama OSS Game Backend

- Game backend APIs for profile, inventory, quest progress, activity logs, and social features
- Postgres-backed durable storage
- Candidate home for groups, leaderboards, matchmaking, and party flows
- Extensible through Nakama server runtime modules for auth hooks, RPCs,
  inventory, profile, stats, social, matchmaking, leaderboards, activity logs,
  and moderation
- May bridge to Supabase or DOS.Me identity patterns where useful
- Does not replace Photon Fusion 2 server authority for movement, combat, or physics
- Default home for game backend custom logic. Do not add a separate game API
  gateway unless a Nakama module is the wrong tool for the feature.

#### Nakama OSS Scaling Boundary

- Nakama OSS is treated as a single-node game backend per shard. Do not run
  multiple Nakama OSS instances against the same logical Nakama database and
  assume this is horizontal scaling. A shared database does not coordinate
  realtime sessions, presence, match state, chat fanout, matchmaker state,
  scheduled jobs, or split-brain recovery.
- Multiple shards may share the same physical Postgres server or cluster, but
  each shard should use a separate logical database and a separate Nakama OSS
  instance, for example `nakama_asia_1`, `nakama_asia_2`, and `nakama_us_1`.
- A global identity or wallet directory may map accounts to home shards and
  owned entitlements. Shard-local databases own current body, stats, inventory,
  `BodyTime`, memories, relationships, NPC state, and agent runtime logs.
- Mature open-source multi-node Nakama clustering was not found in the May 2026
  research pass. The known community attempt (`doublemo/nakama-cluster`) is
  archived and too small to treat as production infrastructure.
- If SECOND SPAWN needs high-availability Nakama clustering, use Heroic
  Enterprise/Heroic Cloud or create a new ADR before building custom
  distributed backend services.

#### Nakama Operating Practices

- Keep game state writes server-owned by default. Unity may request an action,
  but Nakama runtime modules or the Fusion dedicated server decide whether the
  write is allowed.
- Split durable game state into explicit collections such as `player_profile`,
  `body_profile`, `body_stats`, `body_traits`, `soul_profile`, `npc_frame`,
  `memory_record`, `relationship_ledger`, `agent_activity`, `inventory`, and
  economy ledgers. Avoid one large mutable profile blob for unrelated systems.
- Use optimistic concurrency for sensitive storage writes whenever possible.
  `BodyTime`, SECOND balance, inventory, reincarnation, reward claims, and NPC
  relationship or memory updates must not be vulnerable to double-spend or
  duplicate-claim races.
- Keep append-only ledgers for sensitive systems. Current state is a snapshot;
  ledgers provide audit, debugging, replay, rollback, and abuse investigation.
- Separate client RPCs, internal server or worker RPCs, and admin RPCs. Client
  RPCs must be authenticated and narrow. Internal and admin RPCs require server
  secrets or deployment-only access.
- Export Nakama metrics and add application-level structured logs for AI
  decisions, fallback reasons, rate limits, token budget state, storage
  conflicts, reward claims, and BodyTime changes.
- Keep long-running agent simulation outside request handlers. Periodic offline
  agent or NPC sweeps should run in the Fusion server, a worker process, or a
  scheduler that calls bounded Nakama RPCs.
- Store production secrets only in deployment secret managers. Local developer
  config may use private ignored files, but public Docker and example configs
  must contain placeholders only.

### Supabase Sidecar

- Optional identity bridge, wallet/profile integration, storage, analytics, or external product data
- Can be used where it clearly reduces integration work
- Not the primary game backend baseline after ADR 0010
- Never used for combat / movement sync

### `api.dos.ai` Model Service

- All LLM calls go through here
- Multi-provider routing (Convai phase 1, Anthropic + OpenAI phase 2)
- RAG memory retrieval (Nakama/Postgres, Supabase pgvector, or Qdrant depending on later implementation)
- AI token and request rate limits
- Prompt injection defense
- Returns **structured intent**, never raw text trusted by server
- Server validates intent before applying
- Not the game backend. Do not add inventory, profile, matchmaking, guild,
  leaderboard, or gameplay mutation APIs here.

### DOS Chain (via thirdweb)

- NFT verification (Hunter skin, weapon, pet ownership)
- Wallet auth via sign-message
- Escrow contract when NFT is equipped in-game
- SECOND token transactions for reincarnation
- Future time-economy settlement only if a later ADR decides `BodyTime` can convert to or from token resources

### Redis

- Session cache (active Fusion sessions, player presence)
- Rate limit counters (LLM, login, etc.)
- Transient queues (NPC dialogue queue, agent action queue)

## Critical Invariants

1. **Server is the only authority.** Client + AI agent emit intents; server applies or rejects.
2. **LLM never mutates state directly.** LLM emits structured intent -> server validates -> applies.
3. **LLM API keys live only in `api.dos.ai` model service env.** Never in Unity client, never in Supabase Edge Function reaching the client.
4. **NFT lock is on-chain.** When equipped, escrow contract holds. Server reads on-chain state, does not assume off-chain.
5. **AI agent inherits player limits.** No agent can do what a real player cannot.
6. **Time mutations are server-authoritative.** `BodyTime` is gameplay state; client, LLM, and AI agents can only request validated time intents.

## Open Architecture Questions

- AI agent scheduling: 1 process per agent vs pooled worker?
- Fusion server sharding: 1 process per zone vs multiple zones per process?
- LLM cost cap per player per day (need to design)
- Hot reload for progression balance changes (config in Postgres vs git)
- Replay system for cheat investigation (record Fusion ticks)
