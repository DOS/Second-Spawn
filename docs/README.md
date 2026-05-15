# SECOND SPAWN Documentation

SECOND SPAWN is a hybrid MMO + top-down ARPG set in the MetaDOS universe. The game combines offline AI-agent control, reincarnation through synthetic bodies, time-as-currency survival economy, LLM-driven NPCs, and server-authoritative multiplayer.

This documentation is the canonical public design and architecture source for the project. It is published through GitBook at:

<https://dos.gitbook.io/second-spawn/>

## Start Here

- [Game Concept](design/00-game-concept.md) - the high-level pitch, fantasy, audience, and risks.
- [Game Pillars](design/01-pillars.md) - the decision rules every feature must satisfy.
- [Vertical Slice Spec](design/02-vertical-slice-spec.md) - the first 3-6 month playable milestone.
- [Systems Index](design/03-systems-index.md) - the map of game systems and build order.
- [Architecture](ARCHITECTURE.md) - the logical system architecture and invariants.

## Current Prototype Focus

The current implementation focus is a thin, networked player-controller prototype:

- Minimal Fusion controller first.
- Simple KCC spike from Photon Pirate Adventure patterns second.
- Opsive Ultimate Character Controller evaluated only after that smaller Fusion-native path is tested.
- No large Unity asset imports until the movement, camera, and authority contract are verified.

Relevant docs:

- [Overview Design](design/06-overview-design.md)
- [Networked Player Controller Prototype](design/07-player-controller-prototype.md)
- [Pirate Adventure Reference Review](design/09-pirate-adventure-reference-review.md)

## Signature Features

1. **AI Agent 24/7** - the player's character keeps acting when the player is offline.
2. **Reincarnation** - death destroys the body, but consciousness transfers to a new synthetic body.
3. **Time-as-Currency** - time is both survival resource and spendable currency, inherited from the MetaDOS design lineage and adapted for SECOND SPAWN.
4. **LLM as World Citizen** - NPCs and agents reason through grounded, server-validated intents instead of free-form state mutation.

## Documentation Rules

- English is the canonical language for docs, code, commits, PRs, ADRs, and roadmap.
- GitBook handles translated views from the English canonical docs.
- `/docs` is public-facing through GitBook. Do not place private credentials, internal-only secrets, or unpublished partner details here.
