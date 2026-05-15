# Nakama Backend Setup

SECOND SPAWN uses Nakama OSS as the primary game-backend direction. The local setup lives in:

```text
backend/nakama/
```

See [ADR 0010](../adr/0010-nakama-oss-game-backend.md) for the decision.

## Local Services

- Nakama OSS: `registry.heroiclabs.com/heroiclabs/nakama:3.38.0`
- Nakama runtime types: `nakama-common#v1.45.0`
- Postgres: `postgres:16.14-alpine`

Nakama owns its own Postgres database. Do not point Nakama at the Supabase app database.

## Run Locally

```powershell
cd backend/nakama
docker compose up --build
```

Endpoints:

- Nakama HTTP API: `http://127.0.0.1:7350`
- Nakama Console: `http://127.0.0.1:7351`
- Nakama Prometheus metrics: `http://127.0.0.1:9100`
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

## Supabase Schema-Isolated Mode

For MVP production, Nakama can run against a Supabase project if it uses a dedicated role and isolated schema.

Tested on 2026-05-15:

- Supabase Session Pooler worked on port `5432`.
- Schema name: `second`.
- Role name: `nakama_second`.
- Nakama 3.38.0 migrations created 20 tables in schema `second`.
- Nakama server started against the Supabase pooler.
- `secondspawn_health` RPC returned a valid response.

Setup shape:

```sql
create schema if not exists second;
create role nakama_second login password '<secret>';
grant connect on database postgres to nakama_second;
grant usage, create on schema second to nakama_second;
alter role nakama_second in database postgres set search_path = second, public;
```

Use the Supabase **Session Pooler** URI for Nakama. Do not use Transaction Pooler for the running Nakama service.

Connection username format:

```text
nakama_second.<supabase-project-ref>
```

Connection address shape:

```text
nakama_second.<supabase-project-ref>:<secret>@<region>.pooler.supabase.com:5432/postgres?sslmode=require
```

Do not commit this connection string. Store it in production secrets.

Local run against Supabase:

```powershell
cd backend/nakama
Copy-Item .env.example .env
# Fill NAKAMA_DATABASE_ADDRESS in .env with the Session Pooler connection string.
docker compose -f docker-compose.supabase.yml up --build
```

## Alerting

Prometheus can scrape Nakama metrics from port `9100`. Telegram notifications should be handled by Prometheus Alertmanager or Grafana Alerting. Bot tokens and chat IDs must stay in local secrets, not in Git.
