# Morning brief - night session 2026-05-14

JOY went to sleep around midnight. Asked me to keep working autonomously.
This file is the wake-up summary so JOY does not have to scroll through
the chat log.

> **Important caveat**: Unity MCP tools are NOT loaded in the night
> session. JOY started the Unity MCP Server but the Claude Code session
> was already open - MCP servers register at session start, not on the
> fly. To use Unity MCP, restart Claude Code in this repo. After
> restart, follow Step 6a in `NEXT_STEPS.md` for the autonomous Unity
> setup script.

## What I did while you slept

All commits on `main`, pushed to `origin/main`.

| Commit | What |
| ---- | ---- |
| `840f951` | (Pre-sleep) AGENTS.md mirror of CLAUDE.md - Approach B |
| TBD | Backend Go gateway scaffold (`backend/gateway/`) - compiles + tests pass |
| TBD | CI workflows: `backend-test.yml`, `markdown-lint.yml`, `unity-build.yml` (manual trigger only until UNITY_LICENSE secret added) |
| TBD | ADR 0005: Unity 6.5 beta over Unity 6.0 LTS (records your decision so future agents know why we are on beta) |
| TBD | Project hygiene: `.editorconfig`, `SECURITY.md`, PR template, `.markdownlint.json` |
| TBD | NEXT_STEPS.md updated: marked Steps 1-4, 10, 11 done; Step 5 (Photon) blocked on App ID; Step 6 (Coplay) blocked on session restart |

(TBD commits will land in the batch-commit at end of session - see git log for actual hashes.)

### Backend Go gateway scaffold

Compiles + green tests. Layout:

```
backend/gateway/
├── main.go                    # signal handling, graceful shutdown
├── go.mod                     # Go 1.23
├── Makefile                   # run, test, build, vet, docker
├── Dockerfile                 # distroless, nonroot
├── .env.example               # template - real .env is gitignored
├── README.md
└── internal/
    ├── config/                # env var loader, prod requires SUPABASE + ANTHROPIC
    ├── server/                # routes /healthz, /readyz; real handlers staged
    ├── auth/                  # Supabase JWT verifier interface
    ├── llm/                   # provider interface (Anthropic, OpenAI, Convai)
    └── intent/                # structured intent schema (per Hard Rule #2)
```

Run locally: `cd backend/gateway && cp .env.example .env && make run`. Hit `localhost:8080/healthz`.

### CI workflows

- `.github/workflows/backend-test.yml` - runs `go vet` + `go test -race` on every backend/ change. Active immediately.
- `.github/workflows/markdown-lint.yml` - lints all `*.md` against `.markdownlint.json` (excludes `.claude/templates/`). Active immediately.
- `.github/workflows/unity-build.yml` - GameCI Unity test + Linux dedicated server build. **Staged but disabled**; only runs via `workflow_dispatch` (manual). To enable on push, add the `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD` secrets per [game.ci docs](https://game.ci/docs/github/activation), then uncomment the `push:` trigger block.

### ADR 0005

Records JOY's decision to stay on Unity 6.5 beta `6000.5.0b7` rather than roll back to Unity 6.0 LTS. Includes the trigger conditions for re-evaluating (3rd-party asset breakage, etc.).

## What I did NOT do

| Task | Why |
| ---- | ---- |
| Touch `Unity/` folder | Unity Editor was running per `tasklist`. Editing `Unity/Assets/*` while Editor is live risks `.meta` GUID race conditions. Skipped to avoid corruption. Will run via Unity MCP next session. |
| Install Photon Fusion 2 SDK | Need Photon App ID from JOY first (Step 5). Free 20 CCU plan should be fine for vertical slice. Sign up at <https://dashboard.photonengine.com>. |
| Create Supabase project | Step 7 needs JOY to create the Supabase project (reuse DOS.Me org per CLAUDE.md). |
| Touch `_deferred/` templates | Per Codex review (preserved as commit `dd03b5a`), these templates are kept in `_deferred/`, not deleted. |

## What I want JOY to do this morning

1. **Restart Claude Code** in this repo (`cd D:\Projects\Second-Spawn && claude`). This loads `unity-mcp` tools and unblocks Step 6.
2. **Sign up for Photon Fusion** (free tier) and paste the App ID into chat. I will then install the Fusion 2 SDK + drop the App ID into the right asset via Unity MCP.
3. **Create a Supabase project** (reuse DOS.Me org). Paste URL + anon key + service role key into chat. The service role key goes to `backend/gateway/.env` (already gitignored), anon goes to the Unity ScriptableObject.
4. **Confirm the design doc bootstrap** in `docs/design/00-04` reflects your vision. The 5 files were extracted from CLAUDE.md so they should match - but the open questions (SECOND token economy, Hunter NFT integration approach, voice NPC vendor, etc.) need your input.

## Decisions still waiting on JOY (open questions, no urgent deadline)

These are surfaced in `docs/design/00-game-concept.md` "Risks and Open Questions" + `docs/design/04-cultivation-system.md` "Open Questions":

- SECOND token economy: cost per reincarnation, source, sink
- Hunter NFT integration: Option 1 (preset hero) vs Hybrid 1+3 (modular pieces)
- Voice NPC vendor: OpenAI Realtime vs ElevenLabs vs self-host
- Final game name (SECOND SPAWN is codename)
- Reincarnation carryover ratio for cultivation tier
- Photon Fusion 2 license tier (post Cloud free 20 CCU)
- Hetzner VPS specs / region for dedicated server hosting

## File you can delete after reading this

`SESSION_BRIEF.md` is a one-shot morning briefing. Once read, either commit it as a long-term artifact under `docs/log/` or just delete. Up to JOY.
