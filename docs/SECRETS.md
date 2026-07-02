# Secrets Management

## ⚠️ Never Do This

- ❌ Commit `.env` files to git
- ❌ Hardcode secrets in `docker-compose.yml`, Dockerfile, or config files
- ❌ Pass secrets as CLI arguments (visible in `ps` output)
- ❌ Reuse secrets across dev / staging / production environments

---

## Required Secrets

| Variable | Where Used | Description |
|----------|-----------|-------------|
| `PGHOST` | Backend | PostgreSQL host |
| `PGPORT` | Backend | PostgreSQL port (default 5432) |
| `PGUSER` | Backend | Database user |
| `PGPASSWORD` | Backend | Database password |
| `PGDATABASE` | Backend | Database name |
| `SUPABASE_URL` | Backend | Supabase project URL |
| `SUPABASE_ANON_KEY` | Frontend | Public anon key |
| `SUPABASE_SERVICE_ROLE_KEY` | Backend | Private service role key |
| `JWT_SECRET_KEY` | Backend | Signing key for JWT tokens |
| `ASPNETCORE_ENVIRONMENT` | Backend | `Development` or `Production` |

---

## Option 1: Shell Environment (Dev Only)

```bash
export PGHOST="db.your-project.supabase.co"
export PGPASSWORD="your-actual-password"
export SUPABASE_URL="https://your-project.supabase.co"
export SUPABASE_ANON_KEY="your-anon-key"
export SUPABASE_SERVICE_ROLE_KEY="your-service-role-key"
export JWT_SECRET_KEY=$(openssl rand -base64 32)
docker-compose up
```

## Option 2: Docker Swarm Secrets (Production)

```bash
echo -n "your-db-password" | docker secret create supabase_db_password -
echo -n "your-anon-key"    | docker secret create supabase_anon_key -
```

Then reference in `docker-compose.yml`:
```yaml
services:
  naar-noor-prod-api:
    environment:
      SUPABASE_DB_PASSWORD_FILE: /run/secrets/supabase_db_password
```

## Option 3: Azure Key Vault / AWS Secrets Manager (Enterprise)

Retrieve secrets at runtime using your cloud provider's SDK. Never write them to disk or environment files.

---

## Verify No Secrets Are Exposed

```bash
git log --all --full-history -- "*.env"   # check git history
git grep -r "password" -- "*.json"        # scan config files
docker-compose config | grep -i secret    # review compose output
```

---

## Rotating Secrets

1. Generate new value (e.g., `openssl rand -base64 32` for JWT)
2. Update in your secrets store / environment
3. Restart affected services: `docker-compose restart naar-noor-prod-api`
4. Verify health: `curl http://localhost:8080/health`
