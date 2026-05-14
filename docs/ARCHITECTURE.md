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
   +---------------+   +----------------------------+
   | Supabase      |   | Go LLM Gateway             |
   | - Auth        |   | - Convai (phase 1)         |
   | - Postgres    |   | - Anthropic + OpenAI (P2)  |
   | - Realtime    |   | - RAG memory (pgvector)    |
   | - Storage     |   | - Rate limit + safety      |
   +---------------+   +-------------+--------------+
           |                         |
           v                         v
   +---------------+   +----------------------------+
   | DOS Chain     |   | Redis                      |
   | (NFT, wallet) |   | - Session                  |
   | via thirdweb  |   | - Rate limit               |
   |               |   | - Transient cache          |
   +---------------+   +----------------------------+
```

## Component Responsibilities

### Unity Client

- Render, input, local prediction
- Communicates only with Fusion server and Supabase Auth
- NEVER calls LLM API directly
- NEVER holds API keys
- Receives state updates via Fusion tick

### Dedicated Game Server (Photon Fusion 2 Server Mode)

- Source of truth for in-zone state (position, HP, combat, drops)
- Source of truth for `BodyTime` earn, spend, drain, transfer, and expiration
- Validates every action intent (from player input or AI agent)
- Persists durable state to Supabase Postgres (snapshots + events)
- Triggers LLM gateway for NPC dialogue when triggered
- Manages zone lifecycle (load / unload / spawn)
- Tick rate: 30Hz (60Hz for boss encounters if needed)

### AI Agent (offline player simulation)

- Runs as separate process (could be Go service co-located with gateway)
- Subscribes to Fusion server state for offline characters
- Decision loop: read state -> reason via LLM -> emit action intent
- Subject to same server validation as a real player
- Inherits player's character cultivation tier + persona + history

### Supabase Backend

- **Auth:** Reuse DOS.Me pattern (email / wallet / OAuth)
- **Postgres:** durable state (profile, inventory, quest progress, NFT lock state, cultivation tier, character history, reincarnation and time-as-currency events)
- **Realtime:** chat global, presence, friend list, party invite, notification (NOT combat / movement)
- **Storage:** avatar, screenshot, UGC

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

## Open Architecture Questions

- AI agent scheduling: 1 process per agent vs pooled worker?
- Fusion server sharding: 1 process per zone vs multiple zones per process?
- LLM cost cap per player per day (need to design)
- Hot reload for cultivation balance changes (config in Postgres vs git)
- Replay system for cheat investigation (record Fusion ticks)
