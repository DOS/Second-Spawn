# ADR 0001: Adopt Photon Fusion 2 as networking framework

**Status:** Accepted
**Date:** 2026-05-13
**Decision maker:** JOY

## Context

Need to choose a networking framework for a hybrid MMO + top-down ARPG with instance-based zones (~20 players), dungeons, and 50v50 guild PvP. Solo developer with AI assistance.

## Options considered

1. **Photon Fusion 2** - Commercial, mature, prediction + lag-comp built-in, dedicated server support
2. **Netcode for GameObjects** - Unity official, good for small co-op, weaker for persistent zones
3. **FishNet** - Free, server-authoritative, performant, smaller community than Fusion
4. **Mirror** - Free, mature, OK for MMO-lite

## Decision

**Photon Fusion 2**

## Rationale

- JOY's team already shipped MetaDOS (Battle Royale 100 CCU) on Fusion 2
- Inherited Fusion 2 boilerplate from MetaDOS saves 2-4 weeks of setup
- AI agent (Claude / Codex) has more training data on Photon than FishNet / Mirror
- Photon is commercially mature, has dedicated server overview, BR200 sample (200 players / 60Hz proven)
- Free tier (Photon Cloud 20 CCU) sufficient for prototype, paid plans scale

## Consequences

- Locked into Photon Cloud pricing for scaling (100 CCU $95/mo, 500 CCU $475/mo, or self-host dedicated server with license)
- Production must use Server Mode dedicated headless, NOT Host Mode
- Reference: Fusion BR200 sample for interest management patterns
- MetaDOS gameplay code is NOT reusable (BR is different genre from MMO ARPG), only network boilerplate
