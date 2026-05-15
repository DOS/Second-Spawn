# SECOND SPAWN - Architecture

High-level architecture overview. For detailed component design see `docs/design/` and ADRs in `docs/adr/`.

## System Diagram (logical)

```
+-------------------+         +------------------------+
|   Unity Client    |         |  AI Agent (offline)    |
|  (player online)  |         |  controls character    |
|                   |         |  when player away      |
+---------+---------+         +-----------+------------+
          |                               |
          |  Photon Fusion 2 (tick 30Hz)  |
          v                               v
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
   +------------------------------+   +----------------------------+
   | Nakama OSS                   |   | Go LLM Gateway             |
   | - Game API backend           |   | - Convai (phase 1)         |
   | - Runtime RPC validation     |   | - Anthropic + OpenAI (P2)  |
   | - Profile / inventory        |   | - RAG memory (pgvector)    |
   | - Wallet / leaderboard       |   | - Rate limit + safety      |
   +--------------+---------------+   +-------------+--------------+
                  |                                 |
                  v                                 v
   +------------------------------+   +----------------------------+
   | Supabase                     |   | Redis                      |
   | - Auth / identity            |   | - Session                  |
   | - Schema `second` for Nakama |   | - Rate limit               |
   | - App/admin data             |   | - Transient cache          |
   | - Storage / Realtime         |   +----------------------------+
   +--------------+---------------+
                  |
                  v
   +---------------+
   | DOS Chain     |
   | (NFT, wallet) |
   | via thirdweb  |
   +---------------+
```

## Component Responsibilities

### Unity Client

- Render, input, local prediction
- Communicates with Fusion server for gameplay, Supabase Auth for identity, and Nakama APIs for game backend state
- NEVER reads or writes Nakama-owned tables directly through Supabase REST or Realtime
- NEVER calls LLM API directly
- NEVER holds API keys
- Receives state updates via Fusion tick

### Dedicated Game Server (Photon Fusion 2 Server Mode)

- Source of truth for in-zone state (position, HP, combat, drops)
- Source of truth for `BodyTime` earn, spend, drain, transfer, and expiration
- Validates every action intent (from player input or AI agent)
- Persists durable meta-game state through Nakama server RPCs, not direct client writes
- Triggers LLM gateway for NPC dialogue when triggered
- Manages zone lifecycle (load / unload / spawn)
- Tick rate: 30Hz (60Hz for boss encounters if needed)

### AI Agent (offline player simulation)

- Runs as separate process (could be Go service co-located with gateway)
- Subscribes to Fusion server state for offline characters
- Decision loop: read state -> reason via LLM -> emit action intent
- Subject to same server validation as a real player
- Inherits player's character cultivation tier + persona + history

### Nakama Game Backend

- Primary API backend for game-owned state
- Runtime RPCs for profile, inventory, wallet, leaderboard, social, and future matchmaking-adjacent systems
- Owns schema `second` in Supabase for MVP production, accessed by dedicated role `nakama_second`
- Local dev uses Docker Postgres so developers can reset Nakama data without touching Supabase
- Unity and tools talk to Nakama APIs, not directly to Nakama tables
- Prometheus metrics exposed for monitoring and future Telegram alerts through Alertmanager or Grafana
- Hiro and Satori are deferred until license and pricing review

### Supabase Platform

- **Auth:** identity provider and JWT issuer, reused from DOS.Me patterns where practical
- **Postgres:** hosts app/admin data and may host Nakama-owned schema `second` for MVP production
- **Realtime:** app-side chat, presence, friend list, party invite, notification only when not better handled by Nakama or Fusion
- **Storage:** avatar, screenshot, UGC
- **Security boundary:** schema `second` is private backend data; do not expose it to anon/authenticated Supabase client roles

### Go LLM Gateway (DOSRouter pattern)

- All LLM calls go through here
- Multi-provider routing (Convai phase 1, Anthropic + OpenAI phase 2)
- RAG memory retrieval (Supabase pgvector)
- Rate limit per player + per NPC
- Prompt injection defense
- Returns **structured intent**, never raw text trusted by server
- Server validates intent before applying

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
3. **API keys live only in Go gateway env.** Never in Unity client, never in Supabase Edge Function reaching the client.
4. **NFT lock is on-chain.** When equipped, escrow contract holds. Server reads on-chain state, does not assume off-chain.
5. **AI agent inherits player limits.** No agent can do what a real player cannot.
6. **Time mutations are server-authoritative.** `BodyTime` is gameplay state; client, LLM, and AI agents can only request validated time intents.
7. **Nakama-owned tables are private.** Unity clients never access schema `second` through Supabase APIs; Nakama is the API boundary.
8. **Supabase schema isolation is an MVP production option.** It is acceptable only with dedicated role `nakama_second`, Session Pooler or direct persistent connection, rotated secrets, and non-default Nakama keys.

## Open Architecture Questions

- AI agent scheduling: 1 process per agent vs pooled worker?
- Fusion server sharding: 1 process per zone vs multiple zones per process?
- LLM cost cap per player per day (need to design)
- Hot reload for cultivation balance changes (config in Postgres vs git)
- Replay system for cheat investigation (record Fusion ticks)
- Whether schema `second` remains in Supabase long term or moves to a separate managed Postgres for operational isolation
