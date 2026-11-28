# Self-hosting on a Raspberry Pi 5 (Docker + Cloudflare Tunnel)

This guide takes a Pi that has only Docker and Docker Compose installed to a running app on a public HTTPS hostname, backed by a new, empty Postgres production database.

## How it fits together

```
Internet ──HTTPS──▶ Cloudflare ──tunnel──▶ cloudflared ─┐
                                                        │  CFTunnelBridge (external network)
                                                        ▼
                                                   app (waid-app:8080)
                                                        │  backend (internal network)
                                                        ▼
                                                   db (Postgres 17)
```

- **`backend`** is created by [`docker-compose.yml`](../docker-compose.yml). Only `app` and `db` are on it, and it's `internal`: nothing else can reach it, and the database can't reach the internet.
- **`CFTunnelBridge`** is created once by hand (step 2) and shared. `cloudflared` and `app` are both on it, so the tunnel can reach the app. The database is not on it.
- **No ports are opened on the Pi.** The only way in is through the tunnel, so you don't need port forwarding on the router.
- On first start, the app creates the whole database schema with EF Core migrations and seeds the roles, badges and admin account. No SQL is run by hand.

## What you need

- A Raspberry Pi 5 running a **64-bit** OS. Check with `uname -m`; it must say `aarch64`.
- Docker and Docker Compose. Check with `docker compose version`.
- A Cloudflare account with your domain added to it (its DNS managed by Cloudflare).
- Read access to this GitHub repo. If the repo is private, you also need a GitHub personal access token with `repo` read access.

Optional: if your user isn't in the `docker` group yet, add it so you don't need `sudo`:

```bash
sudo usermod -aG docker $USER && newgrp docker
```

## 1. Install git

```bash
sudo apt update && sudo apt install -y git
```

## 2. Create the shared tunnel network

```bash
docker network create CFTunnelBridge
```

You only do this once. Every stack that should be reachable through the tunnel joins this network as `external`.

## 3. Create the Cloudflare Tunnel and run cloudflared

1. In the Cloudflare dashboard, go to **Zero Trust → Networks → Tunnels → Create a tunnel**.
2. Choose **Cloudflared**, name the tunnel (for example `pi5`), and click save.
3. On the install screen, copy the **token**: the long string after `--token`. Ignore the install commands it shows.
4. On the Pi, give cloudflared its own small stack. It stays separate from the app because one tunnel can serve several apps.

   ```bash
   mkdir -p ~/cloudflared && cd ~/cloudflared
   echo "TUNNEL_TOKEN=<paste token here>" > .env && chmod 600 .env
   cat > docker-compose.yml <<'EOF'
   services:
     cloudflared:
       image: cloudflare/cloudflared:latest
       restart: unless-stopped
       command: tunnel --no-autoupdate run
       environment:
         TUNNEL_TOKEN: ${TUNNEL_TOKEN}
       networks:
         - CFTunnelBridge
   networks:
     CFTunnelBridge:
       external: true
   EOF
   docker compose up -d
   ```

5. Back in the dashboard, the tunnel should now show **Healthy**. Click **Next**, or go to the tunnel's **Public Hostname** tab, and add a hostname:

   | Field | Value |
   |---|---|
   | Subdomain / Domain | for example `waid` . `yourdomain.com` |
   | Service type | `HTTP` |
   | URL | `waid-app:8080` |

   Cloudflare terminates HTTPS and creates the DNS record for you. WebSockets, which the live leaderboard and chat (SignalR) use, work through the tunnel by default.

The hostname shows a 502 until the app is running (step 6). That's expected.

## 4. Get the code

```bash
cd ~
git clone https://github.com/jmcshane22/MonmouthHoldemCapstone.git waid
cd waid
```

If the repo is private, enter your GitHub username and use the personal access token as the password.

> Until the Postgres/Docker work is merged, check out its branch first: `git checkout feature/postgres-provider`.

## 5. Create the production secrets (`.env`)

Create `~/waid/.env` next to `docker-compose.yml`. It's gitignored.

```bash
cat > .env <<EOF
# Required
POSTGRES_PASSWORD=$(openssl rand -base64 32 | tr -d '/+=')

# Admin account seeded on first start (leave blank to skip)
ADMIN_EMAIL=admin@yourdomain.com
ADMIN_PASSWORD=<a strong password: upper, lower, digit, symbol, 6+ chars>

# Optional integrations (features stay off / use placeholders when left as-is)
NINJAS_API_KEY=
ENABLE_GEMINI=false
GEMINI_API_KEY=
USE_REAL_EMAILER=false
AZURE_COMMS_CONNSTR=
EOF
chmod 600 .env
nano .env   # fill in the admin email/password and any keys
```

Notes:
- **`POSTGRES_PASSWORD` is only applied when the database volume is first created.** Changing it in `.env` later does not change the database's password; see [Troubleshooting](#troubleshooting).
- Keep a copy of `.env` somewhere safe, such as a password manager. Without it, you can't connect to the database.
- If you set `ENABLE_GEMINI=true`, you also need `GEMINI_API_KEY`. If you set `USE_REAL_EMAILER=true`, you also need `AZURE_COMMS_CONNSTR`; otherwise the app refuses to start.

## 6. Build and start

```bash
docker compose up -d --build
```

The first build compiles the app on the Pi and takes a few minutes. After that:

1. `db` starts with an empty volume, initialises Postgres and reports healthy.
2. `app` starts, applies all migrations, seeds the data and listens on port 8080. That port is visible only on the Docker networks.

Watch the app start:

```bash
docker compose logs -f app
```

On the very first start, EF logs two `fail: ... __EFMigrationsHistory_...` lines. They're harmless: EF checks for its history table before creating it. You're up once you see `Now listening on: http://[::]:8080`.

Check that everything is running:

```bash
docker compose ps        # db should be "healthy", app "running"
```

## 7. Verify

- Open `https://waid.yourdomain.com` and check that the home page loads.
- Sign in with `ADMIN_EMAIL` / `ADMIN_PASSWORD`.
- Upload a profile photo, then restart with `docker compose restart app` and check that it's still there.

## Day-to-day

### Data volumes

| Volume | Holds |
|---|---|
| `waid_pgdata` | The Postgres database |
| `waid_uploads` | User-uploaded profile photos |
| `waid_dpkeys` | ASP.NET data-protection keys, which keep users signed in across redeploys |

`docker compose down` keeps the volumes. **`docker compose down -v` deletes them, including the database.**

### Updating to a new version

```bash
cd ~/waid
git pull
docker compose up -d --build
```

New migrations are applied automatically when the app restarts. Take a backup first if the update includes database changes.

### Backups

Do a one-off dump:

```bash
mkdir -p ~/backups
docker compose -f ~/waid/docker-compose.yml exec -T db pg_dump -U waid -d waid -Fc > ~/backups/waid_$(date +%F).dump
```

To run a nightly dump at 03:00 and keep the last 14 days, run `crontab -e` and add:

```cron
0 3 * * * cd ~/waid && docker compose exec -T db pg_dump -U waid -d waid -Fc > ~/backups/waid_$(date +\%F).dump && find ~/backups -name 'waid_*.dump' -mtime +14 -delete
```

Copy `~/backups` off the Pi regularly, for example with rsync to another machine or cloud storage. A backup that lives only on the Pi's SD card or SSD won't survive that card or drive failing. Also back up the `waid_uploads` volume if profile photos matter to you.

To restore a dump into the running stack (this overwrites the current data):

```bash
docker compose stop app
docker compose exec -T db pg_restore -U waid -d waid --clean --if-exists < ~/backups/waid_YYYY-MM-DD.dump
docker compose start app
```

### Useful commands

```bash
docker compose logs -f app                      # app logs
docker compose exec db psql -U waid -d waid     # SQL shell
docker compose restart app                      # restart only the app
docker system prune                             # free disk space taken by old image builds
```

## Troubleshooting

| Symptom | Fix |
|---|---|
| The hostname shows **502 / Bad gateway** | Check `docker compose ps`. The app may still be starting. Make sure the tunnel URL is `waid-app:8080`, not `localhost`. Run `docker network inspect CFTunnelBridge` to check that both `cloudflared` and `waid-app-1` are listed. |
| `network CFTunnelBridge declared as external, but could not be found` | Step 2 was skipped. Run `docker network create CFTunnelBridge`. |
| `required variable POSTGRES_PASSWORD is missing` | `.env` isn't next to `docker-compose.yml`, or the variable is empty. |
| The app logs `password authentication failed for user "waid"` | `POSTGRES_PASSWORD` changed after the database was created. Put the old value back. If the database is still empty, you can instead run `docker compose down -v` to delete it and start over. |
| The app exits with `UseRealEmailerService is enabled but ...` | Set `AZURE_COMMS_CONNSTR`, or set `USE_REAL_EMAILER=false`. |
| `exec format error` | The OS is 32-bit. Reinstall a 64-bit Raspberry Pi OS. |
