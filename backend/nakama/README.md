# Second Spawn Nakama backend

Local Nakama OSS setup for SECOND SPAWN.

This folder stores project configuration and custom runtime modules only. Do not copy or fork Nakama source into this repository. The server is pulled from the official Heroic Labs Docker image.

## Services

- Nakama OSS: `registry.heroiclabs.com/heroiclabs/nakama:3.38.0`
- Nakama runtime types: `nakama-common#v1.45.0`
- Postgres: `postgres:16.14-alpine`

Nakama owns its own Postgres database. Do not point it at the Supabase app database.

## Local run

```powershell
cd backend/nakama
docker compose up --build
```

Endpoints:

- Nakama HTTP API: `http://127.0.0.1:7350`
- Nakama Console: `http://127.0.0.1:7351`
- Nakama Prometheus metrics: `http://127.0.0.1:9100`
- Console credentials: `admin` / `password`
- Local Postgres: `127.0.0.1:5433`

## Runtime modules

Custom server logic lives under `modules/src/`. The Docker build compiles TypeScript to JavaScript and copies the output into `/nakama/data/modules/build` inside the Nakama container.

Current RPCs:

- `secondspawn_health` - returns a small JSON health response and proves custom runtime loading.

The REST RPC payload must be sent as a JSON string. For example, send `"{}"` rather than `{}`.

## Upgrade policy

When upgrading Nakama:

1. Read the official Nakama release notes.
2. Update the Docker image tag in `Dockerfile`.
3. Update `nakama-runtime` in `modules/package.json` to the matching `nakama-common` version from the compatibility matrix.
4. Rebuild locally and verify the health RPC.

## Boundaries

- Photon Fusion remains authoritative for gameplay simulation.
- Nakama handles game backend and meta-game services.
- Supabase remains identity / app / admin layer only unless a future ADR changes this.
- Hiro and Satori are deferred until license and pricing review.

## Alerting

Nakama exposes Prometheus metrics on port `9100`. Telegram alerts should be wired through Prometheus Alertmanager or Grafana Alerting with secrets stored outside Git.
