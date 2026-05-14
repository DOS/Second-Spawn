# ADR 0006: Build Fusion 2 integration from scratch (extract patterns from BR200 + Tanknarok, do not copy template)

**Status:** Accepted
**Date:** 2026-05-14
**Deciders:** JOY (sole decision-maker, solo dev)

## Context

ADR 0001 already locked in Photon Fusion 2 as the networking framework.
This ADR is about HOW to integrate Fusion 2 into SECOND SPAWN given four
constraints:

1. **License**: SECOND SPAWN code is AGPL-3.0. Photon Asset Store
   templates (BR200, Karts, Tanknarok) ship under Photon's standard
   Asset Store EULA, which is not AGPL-3.0 compatible if their template
   source is incorporated wholesale into our repo.
2. **Hard Rule pattern in CLAUDE.md / AGENTS.md**: "NEVER copy MetaDOS
   gameplay code. Extract patterns only." MetaDOS itself uses BR200 as a
   starting point. We apply the same discipline to Photon templates -
   read for pattern, write our own code.
3. **Solo dev velocity**: scratch-from-zero would burn 2-4 weeks of
   Fusion boilerplate that is well-solved in templates.
4. **Genre mismatch**: BR200 is a battle royale (round-based, large
   open arena, 200 player target). SECOND SPAWN is a hybrid MMO + ARPG
   (persistent instance-based zones, ~20 player target, dungeon
   instances, NFT inventory, AI agent for offline players). Direct
   template adoption would mean removing more than we keep.

Three options were considered:

### Option A: Drop in BR200 + delete what we don't need

Pros: 2-4 week head start, working multiplayer day one.
Cons: License risk (we redistribute Photon's sample source under AGPL),
locked into BR-flavored architecture decisions (round lifecycle, BR-specific
interest management, BR-style player spawn), large legacy surface to
maintain forever.

### Option B: Drop in Tanknarok + delete what we don't need

Closer genre match (top-down PvE waves resemble dungeon farming),
Hosted/Shared mode rather than dedicated. But same license + legacy
issues, and Tanknarok targets co-op not 20-player zones.

### Option C (chosen): Code from scratch, extract patterns from BR200 + Tanknarok + Fusion Starter

Pros:

- AGPL-3.0 clean - the source tree never contains Photon-template code.
- We design the network layer around SECOND SPAWN's pillars from day
  one (server-authoritative, AI agent intent flow, NFT escrow, AGPL
  open-source threat model).
- Pattern-extraction is already the discipline for MetaDOS (Hard Rule
  #1 in agent context), so this is consistent.
- Each Fusion concept lands in our code as we need it - no dead branches.

Cons:

- We lose 2-4 weeks of Fusion boilerplate that is well-solved upstream.
- We re-discover the obvious mistakes ourselves.

Net: pros outweigh cons for a 3-6 month vertical slice on AGPL-3.0
open-source.

## Decision

**Code from scratch.** The integration plan:

1. Install the official Photon Fusion 2 SDK as a Unity package (JOY
   downloads from photonengine.com - manual step). The SDK assemblies
   are referenced as compiled DLLs in Unity's plugins folder; this is
   NOT redistribution of Photon's source, so AGPL-3.0 compatibility is
   preserved.
2. Read the **BR200 sample** locally (Photon Asset Store install in a
   separate scratch project, not in this repo) for:
   - `NetworkRunner` startup flow
   - Server Mode dedicated build flag handling
   - Interest management for ~20 player zone-instance density
   - Lag compensation + tick rate (60Hz target)
   - Host migration logic
3. Read the **Tanknarok sample** locally for:
   - Top-down player controller networked input
   - Wave / enemy spawning patterns (will adapt for dungeon trash mobs)
4. Read the **Fusion Starter sample** for beginner-friendly idioms
   (well-commented per Photon).
5. Read **MetaDOS** (existing JOY repo at `D:\Projects\MetaDOS`,
   read-only reference) for:
   - JOY's prior Fusion 2 architecture decisions
   - Adaptations Photon's BR200 needed to ship
6. Implement our networking layer in `Assets/_SecondSpawn/Scripts/Networking/` (the
   `SecondSpawn.Networking` assembly we already scaffolded).
7. Each Fusion concept lands with a comment header citing the source
   sample + page reference, so reviewers can validate we extracted
   rather than copied.
8. The dedicated server build is the canonical production artifact -
   Host Mode is dev-only (Hard Rule #4).

## Rationale

The decision rests on four claims:

1. **License integrity is non-negotiable for an OSS repo with a brand
   audience.** Open-source contributors and downstream forks need to
   trust that the entire source tree is AGPL-3.0 with no carve-outs.
   Importing Photon template source would create a permanent legal grey
   area for every fork.
2. **The architecture should be designed by SECOND SPAWN, not by a
   battle royale template.** Our gameplay pillars (AI agent 24/7,
   reincarnation, LLM as world citizen, server-authoritative) impose
   network requirements that BR/PvE templates do not satisfy. Pattern
   extraction lets us pick what's good and leave the rest.
3. **The pattern-extraction discipline already works.** JOY has
   successfully built DOSafe and DOS.Me reusing patterns from public
   references without copying source; this is a continuation.
4. **The "2-4 week boilerplate cost" is overestimated for solo dev +
   AI agent.** Claude Code can rewrite well-documented patterns much
   faster than a human; the boilerplate is not really 2-4 weeks for
   this team.

## Consequences

### Positive

- AGPL-3.0 source tree stays clean. No future "is this Photon's code
  or ours?" audits.
- Architecture decisions are SECOND-SPAWN-shaped, not BR-shaped.
- Test surface is bounded to what we wrote, not the Photon template +
  our adaptations on top.
- Pattern-extraction reinforces the discipline that already exists for
  MetaDOS reference.

### Negative

- Slower than template-drop-in by 1-3 weeks in early stages.
- Forces us to re-discover some Fusion 2 idioms (mitigated by reading
  the BR200 / Tanknarok / Starter samples first).
- Requires us to document our own conventions in
  `docs/design/05-networking-architecture.md` rather than inheriting
  Photon's.

### Mitigations

- Use the Coplay Unity MCP bridge to write Fusion 2 code via Claude
  Code, which removes most of the boilerplate burden.
- Keep `docs/design/05-networking-architecture.md` current as Fusion
  decisions land.
- Each Fusion-related script header cites the sample + section we
  extracted the pattern from (so reviewers can validate).

## References

- ADR 0001: Photon Fusion 2 as networking framework
  ([0001-photon-fusion-2.md](0001-photon-fusion-2.md))
- ADR 0003: LLM safety architecture - server-authoritative intent
  ([0003-llm-safety-architecture.md](0003-llm-safety-architecture.md))
- ADR 0004: AI agent control of player character when offline
  ([0004-ai-agent-offline-control.md](0004-ai-agent-offline-control.md))
- Photon Fusion 2 samples overview: <https://www.photonengine.com/samples>
- Photon BR200 docs: <https://doc.photonengine.com/fusion/current/game-samples/br200/overview>
- Photon Tanknarok docs: <https://doc.photonengine.com/fusion/current/game-samples/fusion-tanknarok>
- MetaDOS reference: `D:\Projects\MetaDOS` (read-only)
- Per-system GDD: [docs/design/05-networking-architecture.md](../design/05-networking-architecture.md)
- SDK install how-to: [docs/setup/fusion-install.md](../setup/fusion-install.md)
