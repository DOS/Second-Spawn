# Prototype LLM Gateway Cloud Run Deployment

*Last Verified: 2026-05-16 against Google Cloud Run container deployment docs*

The SECOND SPAWN prototype LLM gateway runs as a containerized HTTPS service, not
as a local executable on JOY's workstation. Unity calls the public gateway URL,
while provider API keys and Supabase server secrets stay in Cloud Run
environment secrets.

This is not the game backend. Nakama is the game backend. Production LLM and AI
endpoints should move to the shared `api.dos.ai` Go gateway when ready.

## Why Cloud Run

- Runs the existing `backend/gateway/Dockerfile` without changing the Go app.
- Autoscaling to zero is fine for prototype traffic.
- Keeps LLM, voice, and Supabase service credentials server-side.
- Avoids local Windows executable approvals during Unity playtesting.
- Leaves room to move the same image to a VPS or Kubernetes later.

Vercel is useful for web frontends and lightweight serverless APIs, but Cloud Run
is the better default for this Go gateway because it uses the production
container directly.

## Runtime Contract

Cloud Run injects `PORT` into the container. The gateway now listens in this
order:

1. `GATEWAY_LISTEN_ADDR`, when explicitly set.
2. `PORT`, when Cloud Run injects it.
3. `:8090`, for local Docker and Unity editor development.

Keep `localhost:8080` reserved for CoplayDev MCP for Unity.

## Current Prototype Endpoint

The first staging revision is deployed here:

```text
https://second-spawn-gateway-535583621422.asia-southeast1.run.app
```

This URL is safe to store in Unity because it is only a public gateway endpoint,
not a secret. It currently runs prototype in-memory character context, prototype
agent decisions, text NPC chat, and the voice-session contract.

## First Deploy

Run from the repo root after `gcloud auth login` and project selection:

```bash
gcloud run deploy second-spawn-gateway \
  --source backend/gateway \
  --region asia-southeast1 \
  --allow-unauthenticated \
  --env-vars-file backend/gateway/deploy/cloudrun.env.yaml \
  --set-secrets ANTHROPIC_API_KEY=ANTHROPIC_API_KEY:latest
```

`--allow-unauthenticated` is intentional for the public game endpoint. Application
auth happens with Supabase JWTs once the Unity bearer-token path is wired. Do
not put Cloud Run behind IAM auth for normal game clients.

## Secret Creation

Create secrets once, then paste values interactively:

```bash
gcloud secrets create ANTHROPIC_API_KEY --replication-policy automatic
gcloud secrets versions add ANTHROPIC_API_KEY --data-file -
```

For the current prototype, provider keys can be omitted only if `GATEWAY_ENV` is
not `production`. Production must have real provider credentials and a wired
Supabase bearer-token path before setting `SUPABASE_JWT_SECRET`.

## Smoke Test

After deploy:

```bash
GATEWAY_URL="$(gcloud run services describe second-spawn-gateway --region asia-southeast1 --format 'value(status.url)')"
curl "$GATEWAY_URL/readyz"
curl "$GATEWAY_URL/v1/characters/dev-player/context"
curl "$GATEWAY_URL/v1/npc/chat" \
  -H "Content-Type: application/json" \
  --data '{"player_id":"dev-player","npc_id":"prototype-guide","message":"Can you remember me?"}'
```


Then set Unity's `SecondSpawnConfig.asset` gateway URL to `GATEWAY_URL` for
cloud-backed playtesting.

## Security Rules

- Never commit `.env`, service-role keys, provider API keys, or Supabase JWT
  secrets.
- Unity stores only public URLs and public-safe keys.
- LLM output still returns intent. The Fusion server remains the authority for
  gameplay state.
- Cloud Run URL is public, but all non-prototype mutating routes must require a
  Supabase JWT before vertical slice release.
