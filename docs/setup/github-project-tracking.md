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
| Area | Unity, Nakama, Gateway, AI Agent, Design, Docs, DevOps, Economy, Combat, UX |
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

## Current Seed Issues

Open issues that should be added to the project:

- [#6 Track gateway agent-decision rate limiting and token budget](https://github.com/DOS/Second-Spawn/issues/6)
- [#7 Track Unity Fusion CodeGen AssetDatabase path error during Play Mode smoke](https://github.com/DOS/Second-Spawn/issues/7)
- [#9 Track Nakama runtime UUID helper migration](https://github.com/DOS/Second-Spawn/issues/9)
- [#13 Track distributed agent decision limiter storage](https://github.com/DOS/Second-Spawn/issues/13)

## Automation Notes

GitHub CLI project automation requires a token with project scopes. If `gh`
returns a missing `read:project` or project scope error, refresh auth before
trying to inspect or mutate Project #5:

```powershell
gh auth refresh -s read:project,project
```

After the scope is available, use the project as the operating dashboard:

- Add new implementation issues to Project #5.
- Link pull requests back to issues.
- Move PR-linked issues into review when a PR opens.
- Mark verification field after docs lint, backend tests, Unity smoke, or
  playtest runs.
- Keep closed issues in the project for progress charts.
