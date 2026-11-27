# Self-hosting with Docker Compose (Raspberry Pi 5)

The app and Postgres run as two containers defined in [`docker-compose.yml`](../docker-compose.yml). Both images are multi-arch, so this works on the Pi (linux/arm64) and on x64.

## First start

1. Install Docker Engine and the Compose plugin on the Pi.
2. Clone the repo, then create a `.env` file next to `docker-compose.yml` (it is gitignored):

   ```dotenv
   POSTGRES_PASSWORD=<a long random password>
   APP_PORT=8080
   # Optional real keys; placeholders are used when omitted
   NINJAS_API_KEY=
   GEMINI_API_KEY=
   AZURE_COMMS_CONNSTR=
   ```

3. Build and start:

   ```bash
   docker compose up -d --build
   ```

The app waits for Postgres to report healthy, applies EF Core migrations (`Database__MigrateOnStartup=true`), seeds roles/admin/badges, then listens on `http://<pi>:${APP_PORT}`.

## Data

| Volume | Holds |
|---|---|
| `pgdata` | The Postgres database |
| `dpkeys` | ASP.NET data-protection keys (keeps users signed in across redeploys) |

`docker compose down` keeps both volumes; `docker compose down -v` **deletes** them.

## Backups

```bash
# Dump
docker compose exec -T db pg_dump -U waid -d waid -Fc > waid_$(date +%F).dump
# Restore into an empty database
docker compose exec -T db pg_restore -U waid -d waid --clean < waid_YYYY-MM-DD.dump
```

Schedule the dump with cron on the Pi and copy the files off the device.

## Updating

```bash
git pull
docker compose up -d --build
```

New migrations are applied automatically when the app container starts.

## Not covered yet

- TLS / a reverse proxy in front of the app (the container serves plain HTTP on 8080).
- Automated deploys from GitHub Actions to the Pi.
