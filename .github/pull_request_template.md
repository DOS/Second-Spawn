<!-- Thanks for contributing to Second Spawn! Fill in the relevant sections.
     If something does not apply, write "n/a" rather than deleting the section. -->

## What does this PR do?

<!-- 1-3 sentences. The "why" matters more than the "what". -->

## Linked issue / ADR

<!-- e.g. closes #42, motivated by docs/adr/0007-X.md -->

## Touched areas

- [ ] Unity client (`Unity/`)
- [ ] Dedicated server build flags / CI
- [ ] Nakama runtime (`backend/nakama/`)
- [ ] Supabase schema / RLS policies
- [ ] DOS Chain integration / NFT contracts
- [ ] AI agent runtime
- [ ] Design docs (`docs/design/`) or ADRs (`docs/adr/`)
- [ ] CI / project tooling

## Test plan

<!-- For Unity: which scenes were play-tested, on which Unity version, with how many concurrent players? -->
<!-- For Nakama runtime: `cd backend/nakama && npm run build && npm test` output, plus any local smoke. -->
<!-- For server-authority changes: confirm no logic moved to client. -->

## Server-authority check (mandatory if touching gameplay)

- [ ] No new gameplay logic runs on the Unity client
- [ ] No new API key embedded in the Unity client
- [ ] LLM outputs are validated as intent server-side, never auto-applied
- [ ] If this PR adds a new state mutation path, it goes through the
      Nakama runtime validator before any gameplay system consumes it

## Reviewer pass

JOY is non-coder per `.claude/CLAUDE.md` Hard Rule #7. Before requesting
merge, the AI agent reviewer (Claude Code `/code-review` skill, or
Codex CLI rescue) MUST have given a pass on this PR. Drop the review
summary in a comment.

- [ ] AI agent reviewer pass attached
