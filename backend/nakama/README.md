# SECOND SPAWN Nakama Backend

Nakama OSS is the game backend. `api.dos.ai` / Go LLM Gateway is the shared AI
gateway only. Nakama owns game sessions and verifies external identity through
its runtime module.

## Supabase Auth Bridge

The client flow is:

1. Unity signs the player into Supabase Auth. Anonymous sign-in is allowed for
   the first prototype.
2. Unity receives a Supabase access token.
3. Unity calls Nakama custom authentication with that access token as the
   temporary custom credential.
4. Nakama `beforeAuthenticateCustom` calls Supabase Auth directly:
   `GET {SUPABASE_URL}/auth/v1/user`.
5. Supabase verifies the access token and returns the authenticated user.
6. Nakama rewrites the incoming custom auth request to a stable custom ID and issues
   the Nakama session.

Do not let Unity send a raw Supabase user ID directly to Nakama custom auth.
That would let a modified client spoof another account.

## Runtime Environment

Required Nakama runtime env:

```text
SUPABASE_URL=https://your-project.supabase.co
SUPABASE_PUBLISHABLE_KEY=sb_publishable_...
```

Use `local.example.yml` as the public-safe local config template. Keep real
per-machine config outside git. Nakama expects `runtime.env` as key-value
entries such as:

```yaml
runtime:
  env:
    - "SUPABASE_URL=https://your-project.supabase.co"
    - "SUPABASE_PUBLISHABLE_KEY=sb_publishable_..."
```

If Supabase anonymous auth is not configured yet, the Unity prototype can fall
back to Nakama device auth so local Play Mode is not blocked. That fallback is
for local iteration only; production account binding must use Supabase custom
auth or a later approved identity ADR.

No game auth secret belongs in `api.dos.ai`. The LLM gateway can receive
already-validated game context from Nakama or the Fusion server when an AI call
is needed.

## Build and Test

```bash
cd backend/nakama
npm install
npm run build
npm test
```

The build outputs the Nakama JavaScript runtime entrypoint to:

```text
backend/nakama/build/index.js
```

Mount or copy the built JavaScript files into Nakama's runtime module path for
local development or deployment. The TypeScript source stays in
`backend/nakama/modules/`.

## Runtime RPCs

The current prototype module registers:

- `secondspawn_health` - unauthenticated smoke check through `runtime.http_key`
- `secondspawn_profile_get` - get or create the authenticated player's profile,
  current body, soul, policy, BodyTime, cultivation, and memory context
- `secondspawn_memory_add` - add or deduplicate compact memory records
- `secondspawn_soul_update` - update soul, characteristics, and agent policy
- `secondspawn_agent_decide` - deterministic safe fallback decision for local
  agent control when the LLM gateway is unavailable
