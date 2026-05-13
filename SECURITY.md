# Security Policy

## Reporting a Vulnerability

If you find a security issue in Second Spawn, please report it privately
via email to `joy@dos.ai` with a subject line starting with
`[SECOND-SPAWN-SECURITY]`. Do **not** open a public GitHub issue.

You will receive an acknowledgement within 72 hours. Please include:

- A description of the issue
- Steps to reproduce
- Affected component (Unity client, dedicated server, Go gateway,
  Supabase backend, NFT contracts, AI agent runtime)
- Your assessment of impact (information disclosure, RCE, cheat,
  economy abuse, etc.)
- Whether the issue has been disclosed elsewhere

## Scope

In scope:

- The Go LLM gateway (`backend/gateway/`) - auth bypass, prompt
  injection that affects state, rate limit bypass, secret leakage
- The Unity dedicated server build - server-authority bypass, cheat
  vectors that the open-source code makes possible
- LLM intent validation - any path where LLM output causes state
  mutation without server validation
- NFT escrow logic - lock / release race conditions, double-spend, etc.
- Supabase Row-Level Security policies (when published)

Out of scope:

- Convai SDK internals (report to Convai)
- Photon Fusion 2 SDK internals (report to Photon)
- Unity Editor / runtime issues (report to Unity)
- DOS Chain layer-1 issues (report to DOS Chain team)
- Cosmetic Unity client issues that do not affect gameplay or state
- Issues requiring a malicious server operator (the threat model
  assumes the dedicated server is trusted)

## Threat Model

This project ships its full source under AGPL-3.0. The threat model
assumes:

- Attackers have full source code access
- Attackers can run modified Unity clients
- Attackers cannot modify the dedicated server runtime
- Attackers cannot read Supabase service role keys, gateway secrets,
  or DOS Chain signing keys

This means **all gameplay logic must be server-authoritative** and
**no API key may live in the Unity client**. Bug reports that exploit
unauthoritative client logic are valid security issues, not feature
requests.

## Disclosure Timeline

We aim to:

- Acknowledge within 72 hours
- Provide a fix or mitigation timeline within 14 days
- Coordinate a public disclosure date with the reporter

For critical issues affecting live players, we may disclose faster.

## Credit

Security researchers who report valid issues will be credited in
release notes (with their permission) once a fix ships.
