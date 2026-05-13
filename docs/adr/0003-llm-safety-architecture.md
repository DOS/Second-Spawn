# ADR 0003: LLM safety architecture - server-authoritative intent validation

**Status:** Accepted
**Date:** 2026-05-13
**Decision maker:** JOY

## Context

Game has LLM-driven NPCs and an LLM-driven AI agent that controls the player's character when offline. Open source means anyone can read the code and craft adversarial prompts. NFT economy means cheating has real money value.

## Decision

1. **All LLM calls go through a Go gateway, never directly from Unity client.**
2. **API keys (Anthropic, OpenAI, Convai, ElevenLabs) live only in gateway env.** Never in client, never reachable from client.
3. **LLM output is parsed into structured intent (JSON schema enforced).** Free-form text is only for display.
4. **Server validates every intent before applying state changes.** LLM cannot grant items, gold, XP, or mutate progression directly.
5. **Rate limit per player + per NPC.** Token budget cap, request count cap.
6. **Prompt injection defense reuses DOSafe patterns** (input sanitization, system prompt isolation, tool result fencing).

## Rationale

- Anthropic / OpenAI APIs charge per token - unbounded LLM = unbounded cost
- Open source repo = adversarial users can read prompts and try injection
- NFT economy = item duplication via LLM exploit = real money loss
- "LLM as the brain, server as the authority" is industry standard for LLM-enabled games

## Consequences

- All NPC dialogue paths must define structured intent schemas (quest accept, item give, dialogue choice, etc.)
- Server must implement validators for each intent type
- Gateway must handle LLM provider failover (Anthropic down -> OpenAI fallback)
- AI agent (offline player) inherits same intent validation - it cannot do what a real player cannot
- Voice NPC must use ephemeral tokens (OpenAI Realtime API short-lived tokens), never client-side API key
- Sentis on-device AI (phase 3) is OK for perception (object detection, classification) but NEVER for state-mutating decisions
