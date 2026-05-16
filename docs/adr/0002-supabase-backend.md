# ADR 0002: Supabase as backend (auth, persistence, realtime side-channel)

**Status:** Superseded by [ADR 0010: Nakama OSS Game Backend](0010-nakama-oss-game-backend.md)
**Date:** 2026-05-13
**Decision maker:** JOY

## Context

Need backend for auth, durable game state, social features (chat, presence, friend list), and storage. Solo developer, must minimize tech stack.

## Options considered

1. **Supabase** - Postgres, auth, realtime, storage in one platform. JOY already uses for DOS.Me.
2. **Nakama** - Game-specific backend (matchmaking, guilds, leaderboards built-in)
3. **PlayFab** - Microsoft-managed game backend
4. **Custom Postgres + custom auth + custom realtime** - Maximum control, maximum work

## Decision

**Supabase** for auth, Postgres durable state, realtime side-channel, storage. Defer Nakama until guild war / matchmaking complexity demands it.

## Rationale

- JOY already operates Supabase for DOS.Me - tribal knowledge, no new stack to learn
- Auth pattern (email / OAuth / wallet via DOS Chain) reusable from DOS.Me
- Postgres is enough for game state at MVP scale (<10k players)
- Supabase Realtime fits chat / presence / notifications (NOT combat sync - that is Fusion)
- Adding Nakama means 2 backend stacks to operate - solo dev cannot afford

## Consequences

- **Supabase Realtime is NOT used for combat / movement sync.** That is Photon Fusion's job.
- Game-specific features (matchmaking, guild system, leaderboards) implemented in custom Go services on top of Supabase.
- Migration to Nakama is possible later if Supabase becomes a bottleneck for guild war / cross-shard features.
- Phase 2 LLM gateway is a separate Go service, not Supabase Edge Function (latency + GPU local inference future-proofing).
