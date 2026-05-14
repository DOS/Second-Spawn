# SECOND SPAWN

> Hybrid MMO + Top-down ARPG with AI-driven NPCs and AI agent for offline players. Set in the MetaDOS universe (~2050 post-apocalyptic sci-fi).

**Status:** Pre-alpha, vertical slice in development

**Genre:** Action RPG, Multiplayer Online, Sci-fi Cyberpunk

## Core Features

- **AI Agent 24/7** - Your character keeps playing when you are offline. An LLM-driven agent farms, quests, and socializes on your behalf.
- **Reincarnation** - Death is permanent for the body. Transfer your consciousness to a new synthetic body using SECOND tokens. Progression resets - this is a roguelike-MMO hybrid.
- **Time-as-Currency** - Time is both your current body's survival resource and a spendable economy resource, adapted from MetaDOS.
- **Sci-fi Cultivation** - 6-tier progression system (Awakening -> Ascension), explained through Nibirium-enhanced biotech and consciousness science.
- **LLM-Powered NPCs** - NPCs remember you, have personality, and react to your history.
- **NFT Integration** - Inherit assets from the MetaDOS universe. Hunter skins, weapons, pets on DOS Chain.

## Tech Stack

- Unity 6.5 beta + URP
- Photon Fusion 2 (dedicated server mode in production)
- Supabase (auth, Postgres, realtime, storage)
- Go LLM gateway (server-authoritative LLM intent validation)
- DOS Chain (NFT, wallet auth via thirdweb)
- Convai (phase 1 NPC dialogue) -> custom LLM (phase 2)

## Repository Structure

```
/Unity/           Unity project (Assets, Packages, ProjectSettings)
/backend/        Go LLM gateway, server-side services
/docs/            Design docs, ADRs, architecture
/.claude/         AI agent context, templates, conventions
```

Public docs are published from `/docs` to GitBook:

<https://dos.gitbook.io/second-spawn/>

## Build (early stage)

Requirements:

- Unity 6.5 beta `6000.5.0b7`
- Git LFS
- Photon Fusion 2 app ID
- Supabase project (or local Postgres for offline dev)

```bash
git clone https://github.com/DOS/Second-Spawn.git
cd Second-Spawn
git lfs install
git lfs pull
```

Open the `Unity/` subfolder in Unity Hub, let it compile, then configure Photon app ID and Supabase URL in `Unity/Assets/_SecondSpawn/Settings/SecondSpawnConfig.asset`.

## Contributing

Contributors welcome. See `CONTRIBUTING.md`. Good first issues labeled in GitHub Issues.

## License

- **Code:** AGPL-3.0 (see `LICENSE`). If you fork and run this code as a network service (which a multiplayer game is), you must release your source modifications under AGPL-3.0 too.
- **Assets:** CC-BY-NC 4.0 (see `LICENSE-ASSETS`). No commercial use.
- **NFT-related logic and assets:** Reserved by DOS.AI ecosystem rights. Not licensed under AGPL/CC-BY-NC.

This dual license combination is deliberate: anyone can study, contribute, and self-host for personal use, but commercial forks must either contribute back or seek a separate commercial license from DOS.AI.

## Brand

Part of the **DOS.AI** product family. Universe shared with MetaDOS.
