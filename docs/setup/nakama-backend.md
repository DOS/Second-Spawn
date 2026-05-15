# Nakama Backend Setup

SECOND SPAWN uses Nakama OSS as the primary game-backend direction. The local setup lives in:

```text
backend/nakama/
```

See [ADR 0010](../adr/0010-nakama-oss-game-backend.md) for the decision.

## Local Services

- Nakama OSS: `registry.heroiclabs.com/heroiclabs/nakama:3.38.0`
- Nakama runtime types: `nakama-common#v1.45.0`
- Postgres: `postgres:16-alpine`

Nakama owns its own Postgres database. Do not point Nakama at the Supabase app database.

## Run Locally

```powershell
cd backend/nakama
docker compose up --build
```

Endpoints:

- Nakama HTTP API: `http://127.0.0.1:7350`
- Nakama Console: `http://127.0.0.1:7351`
- Console credentials: `admin` / `password`
- Local Nakama Postgres: `127.0.0.1:5433`

## Health Check

Nakama service health:

```powershell
curl.exe -i http://127.0.0.1:7350/healthcheck
```

Runtime RPC smoke check:

1. Authenticate a local device through Nakama.
2. Call the `secondspawn_health` RPC with a string payload.

The RPC should return:

```json
{
  "ok": true,
  "service": "second-spawn-nakama",
  "userId": "<nakama-user-id>"
}
```

## Boundaries

- Photon Fusion remains authoritative for frame-level gameplay.
- Nakama handles game backend and meta-game services.
- Supabase remains identity / app / admin layer unless a future ADR changes this.
- Hiro and Satori are deferred until license and pricing review.
