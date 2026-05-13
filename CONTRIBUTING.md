# Contributing to SECOND SPAWN

Thank you for considering contributing. SECOND SPAWN is built primarily by a solo developer with AI assistance, and contributors are welcome.

## Before You Start

This project uses AI coding agents (Claude Code) as the primary development driver. The `.claude/` directory contains context and conventions agents must follow.

## Quick Start

1. Fork and clone the repo
2. Install Git LFS: `git lfs install`
3. Pull LFS assets: `git lfs pull`
4. Open in Unity 6 LTS
5. Read `docs/ARCHITECTURE.md` and `.claude/CLAUDE.md`

## What to Work On

Look for issues labeled `good first issue`, `help wanted`, or `bug`.

Areas open for contribution:

- Quest system templates
- NPC dialogue prompt engineering
- Cultivation tier balancing
- UI polish
- Localization
- Documentation

Areas reserved for core team (do not submit PR without discussion):

- Network protocol design
- LLM gateway architecture
- NFT / blockchain integration
- Reincarnation / SECOND token economy

## Pull Request Process

1. Create a feature branch: `feat/<short-desc>`
2. Make changes following conventions in `.claude/CLAUDE.md`
3. All gameplay logic must be server-authoritative (anti-cheat for open source)
4. Run Unity tests: `Edit > Test Runner`
5. Open PR with description of what + why
6. Wait for AI code review + maintainer review

## Coding Conventions

- C#: Microsoft conventions (PascalCase classes, camelCase fields, `var` when type is obvious)
- All code, comments, docs in **English**
- No em-dashes in any text - use `-` only
- No unnecessary comments - code should be self-documenting

## Security Note

Open source means anyone can read the code. ALL gameplay must be server-authoritative. Never trust client input. Never put API keys in Unity client.

## License

By contributing, you agree your contributions are licensed under:

- **Code contributions:** AGPL-3.0 (see `LICENSE`)
- **Asset contributions:** CC-BY-NC 4.0 (see `LICENSE-ASSETS`)

You retain copyright on your contributions but grant the project (and downstream users complying with AGPL/CC-BY-NC) a perpetual license to use them.

If you cannot accept AGPL-3.0 (for example your employer policy bans AGPL), please discuss before submitting a PR. A Contributor License Agreement may be needed for some contributions.
