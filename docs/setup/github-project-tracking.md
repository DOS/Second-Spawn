# GitHub Project Tracking

*Status: Active*
*Created: 2026-05-17*

This document defines how SECOND SPAWN uses GitHub Projects for day-to-day
execution tracking.

## Primary Project

- Project: [DOS Project #5](https://github.com/orgs/DOS/projects/5)
- Repository: [DOS/Second-Spawn](https://github.com/DOS/Second-Spawn)
- Purpose: issue, pull request, review, milestone, and verification tracking.

## Source-of-Truth Split

| Surface | Purpose |
| ---- | ---- |
| `ROADMAP.md` | Public milestone state and high-level progress |
| `CHANGELOG.md` | What changed after merges |
| `docs/design/12-game-design-document.md` | Living GDD and current design truth |
| GitHub issues | Unit of work ready for implementation or investigation |
| GitHub pull requests | Proposed changes and review history |
| GitHub Project #5 | Daily execution dashboard across issues and PRs |

GitHub Projects should not replace the GDD or ADRs. It should make the current
work queue visible and easier to monitor.

## Recommended Fields

| Field | Values |
| ---- | ---- |
| Status | Inbox, Ready, In Progress, In Review, Blocked, Done |
| Area | Unity, Nakama, AI Agent, Design, Docs, DevOps, Economy, Combat, UX |
| Milestone | Foundation, Vertical Slice, Alpha, Beta, Post-Launch |
| Priority | P0, P1, P2, P3 |
| Size | XS, S, M, L, XL |
| Review Gate | Not Ready, Local Review, Gemini, Codex, Approved, Waived |
| Verification | Not Run, Docs Lint, Backend Tests, Unity Smoke, Playtest |

## Recommended Views

- Vertical Slice Board: group by `Status`, filter `Milestone = Vertical Slice`.
- Engineering Table: sort by `Priority`, `Area`, and `Size`.
- Review Queue: filter `Status = In Review` or `Review Gate` not approved.
- Roadmap: timeline or roadmap layout grouped by `Milestone`.
- Risks and Blockers: filter `Status = Blocked` or `Priority = P0`.

## Repository Labels and Milestones

The repository mirrors the most important project fields with labels so issues
remain useful even before they are added to Project #5.

- Milestones: `Foundation`, `Vertical Slice`.
- Area labels: `area:unity`, `area:nakama`, `area:ai-agent`, `area:design`,
  `area:devops`, `area:economy`, `area:combat`, `area:docs`.
- Priority labels: `priority:p0`, `priority:p1`, `priority:p2`,
  `priority:p3`.
- Size labels: `size:xs`, `size:s`, `size:m`, `size:l`, `size:xl`.

## Current Seed Issues

Open issues that should be added to the project:

### Foundation

- [#6 Add Nakama agent-decision rate limits and token budgets](https://github.com/DOS/Second-Spawn/issues/6)
- [#7 Track Unity Fusion CodeGen AssetDatabase path error during Play Mode smoke](https://github.com/DOS/Second-Spawn/issues/7)
- [#9 Track Nakama runtime UUID helper migration](https://github.com/DOS/Second-Spawn/issues/9)
- [#13 Track shared LLM budget storage for multi-shard Nakama scale](https://github.com/DOS/Second-Spawn/issues/13)
- [#79 Harden Nakama storage permissions and optimistic concurrency](https://github.com/DOS/Second-Spawn/issues/79)
- [#80 Add append-only Nakama ledgers for economy and agent audit](https://github.com/DOS/Second-Spawn/issues/80)
- [#81 Add Nakama metrics and structured AI decision observability](https://github.com/DOS/Second-Spawn/issues/81)
- [#82 Separate Nakama client, internal worker, and admin RPC boundaries](https://github.com/DOS/Second-Spawn/issues/82)
- [#83 Define shard-ready Nakama account routing and database operations](https://github.com/DOS/Second-Spawn/issues/83)
- [#84 Define external scheduler path for offline-agent and NPC simulation](https://github.com/DOS/Second-Spawn/issues/84)

### Vertical Slice

- [#23 Import Opsive UCC in an isolated Unity pass](https://github.com/DOS/Second-Spawn/issues/23)
- [#24 Import Behavior Designer in an isolated Unity pass](https://github.com/DOS/Second-Spawn/issues/24)
- [#25 Import Convai in an isolated Unity pass](https://github.com/DOS/Second-Spawn/issues/25)
- [#26 Implement server-authoritative combat damage prototype](https://github.com/DOS/Second-Spawn/issues/26)
- [#27 Add first BodyTime reward source outside debug UI](https://github.com/DOS/Second-Spawn/issues/27)
- [#28 Add first BodyTime spend sink in normal play](https://github.com/DOS/Second-Spawn/issues/28)
- [#29 Design server-authoritative player time-loot rules](https://github.com/DOS/Second-Spawn/issues/29)
- [#30 Build prototype reincarnation presentation flow](https://github.com/DOS/Second-Spawn/issues/30)
- [#31 Add agent activity log prototype UI](https://github.com/DOS/Second-Spawn/issues/31)
- [#32 Add Nakama channel chat prototype](https://github.com/DOS/Second-Spawn/issues/32)
- [#33 Build first questline and hub NPC story beat](https://github.com/DOS/Second-Spawn/issues/33)
- [#34 Prototype first dungeon and boss encounter plan](https://github.com/DOS/Second-Spawn/issues/34)
- [#35 Add Hunter NFT skin equip placeholder and escrow design](https://github.com/DOS/Second-Spawn/issues/35)
- [#36 Prepare Linux headless dedicated server build path](https://github.com/DOS/Second-Spawn/issues/36)

## Automation Notes

GitHub CLI project automation requires a token with project scopes. If `gh`
returns a missing `read:project` or project scope error, refresh auth before
trying to inspect or mutate Project #5:

```powershell
gh auth refresh -s project
```

After the scope is available, use the project as the operating dashboard:

- Add new implementation issues to Project #5.
- Link pull requests back to issues.
- Move PR-linked issues into review when a PR opens.
- Mark verification field after docs lint, backend tests, Unity smoke, or
  playtest runs.
- Keep closed issues in the project for progress charts.
