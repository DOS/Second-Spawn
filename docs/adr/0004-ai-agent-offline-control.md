# ADR 0004: AI Agent control of player character when offline

**Status:** Accepted (concept), implementation TBD
**Date:** 2026-05-13
**Decision maker:** JOY

## Context

Core USP: when a player is offline, an LLM-driven AI agent controls their character (farms, quests, socializes). Player returning takes over. This is a near-unique feature in the MMO / ARPG space.

## Decision

1. **AI agent runs server-side**, not on player's client.
2. **Agent operates within Fusion server tick** - it emits intents identical to player input.
3. **Server validates agent intents the same as player intents** - no privilege escalation.
4. **Agent persona derived from player history** (level/stats, quest progress, social graph) and configurable preferences.
5. **Agent death = body death = reincarnation triggered.** Player returns to find a reincarnated character if their old body died.
6. **Anti-abuse:** agent inherits player rate limits + capability caps. Agent cannot farm more efficiently than the player.

## Open Implementation Questions

- **Scheduling:** 1 process per agent vs pooled worker (Go goroutines)?
- **LLM cost cap:** per player per day? How to budget when 1000 players offline?
- **Decision rate:** how often does agent reason? Every 5 seconds? Every 1 minute? Adaptive?
- **Persona drift:** does agent personality evolve? Player feedback after takeover?
- **Player notification:** when agent's character dies, how is player notified? Push? Email?
- **Opt-out:** can player turn off AI agent (logout = inert character)?

## Rationale

- Differentiator that no major MMO has (closest: EVE Online passive training, but no autonomous actions)
- Generates always-on world economy (offline players still craft, gather, trade)
- Showcase for DOS.AI brand - AI agent technology productized
- Naturally cohesive with reincarnation lore (consciousness can be uploaded / handed to AI agent)

## Consequences

- LLM cost is a major operational concern - need per-player budget cap
- Server tick rate must handle N agents per zone in addition to N players
- Need replay / audit log for agent decisions (debugging + player complaints "my agent did something dumb")
- Game design must accommodate "what does an autonomous agent want to do?" - rewards, goals, fail-states
- Phase 1 prototype: agent farms one designated area only. Phase 2: full action space.
