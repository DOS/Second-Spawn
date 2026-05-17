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

Optional local Play Mode debug env:

```text
SECOND_SPAWN_ENABLE_DEBUG_BODYTIME=true
```

This enables the prototype fatal BodyTime drain source used by Unity smoke
tools to force the death -> reincarnation loop. Leave it disabled outside
local development. Real PvP, contested-zone, or player-loot time transfers must
come from server-validated combat or zone events, not client self-reporting.

Use `local.example.yml` as the public-safe local config template. Keep real
per-machine config outside git. Nakama expects `runtime.env` as key-value
entries such as:

```yaml
runtime:
  env:
    - "SUPABASE_URL=https://your-project.supabase.co"
    - "SUPABASE_PUBLISHABLE_KEY=sb_publishable_..."
    - "SECOND_SPAWN_ENABLE_DEBUG_BODYTIME=false"
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
  current body, soul, policy, BodyTime, level/stats, memory context, runtime
  stats, Frame identity/skills/agents/tools/heartbeat layers, and bounded agent
  activity log
- `secondspawn_memory_add` - add or deduplicate compact memory records
- `secondspawn_soul_update` - update soul, characteristics, and agent policy
- `secondspawn_agent_activity_add` - append a bounded agent activity event and
  update runtime counters for offline sessions or Unity-side bootstrap
- `secondspawn_agent_decide` - deterministic safe fallback decision for local
  agent control when the LLM gateway is unavailable, with runtime counters
- `secondspawn_chat_send` - append a bounded prototype message to a named hub
  channel; this is an RPC storage log, not the final realtime socket
- `secondspawn_chat_list` - read recent bounded messages for a named prototype
  hub channel
- `secondspawn_reward_claim` - claim an allowlisted prototype enemy or objective
  reward from the server-owned reward catalog; Unity sends only the objective
  ID and Nakama decides the BodyTime amount
- `secondspawn_openclaw_bind` - bind a user-owned external OpenClaw agent to a
  server-owned Frame actor through `frame_actor_id`, `connected_agent_id`,
  consent, moderation, connection status, and rate limit metadata
- `secondspawn_openclaw_context_get` - return the structured Frame context an
  OpenClaw agent may read: identity, soul, body, bounded memory, policy,
  requestable intents, and heartbeat
- `secondspawn_openclaw_intent_submit` - record a requested OpenClaw intent as
  `pending_validation` without mutating authoritative game state
- `secondspawn_openclaw_heartbeat` - update bridge connection status and Frame
  heartbeat/audit state for the connected external agent
