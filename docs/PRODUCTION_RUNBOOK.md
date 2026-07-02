# Production Deployment Runbook

## Pre-Deployment Checklist

- [ ] Database backups taken
- [ ] All environment variables prepared
- [ ] SSL/TLS certificates ready
- [ ] DNS records verified
- [ ] Team notified of deployment window

---

## Step 1: Prepare Environment Variables

```bash
export PGHOST=prod-db-host.com
export PGPORT=5432
export PGUSER=prod_user
export PGPASSWORD=<secure-password>
export PGDATABASE=naar_noor_prod
export SUPABASE_URL=https://your-project.supabase.co
export SUPABASE_ANON_KEY=<anon-key>
export SUPABASE_SERVICE_ROLE_KEY=<service-role-key>
export JWT_SECRET_KEY=$(openssl rand -base64 32)
export JWT_ISSUER=NaarNoor
export JWT_AUDIENCE=NaarNoorApp
export ASPNETCORE_ENVIRONMENT=Production
```

---

## Step 2: Build Docker Images

```bash
cd /path/to/Naar-Noor

# Build images
docker-compose build

# Tag for production registry
docker tag naar-noor:backend registry.example.com/naar-noor:backend-v1.0
docker tag naar-noor:frontend registry.example.com/naar-noor:frontend-v1.0

# Push to registry
docker push registry.example.com/naar-noor:backend-v1.0
docker push registry.example.com/naar-noor:frontend-v1.0
```

---

## Step 3: Deploy to Production Server

```bash
# SSH to production server
ssh prod-user@prod-server.com

# Create deploy directory
mkdir -p /opt/naar-noor
cd /opt/naar-noor

# Copy docker-compose.yml
scp docker-compose.yml prod-user@prod-server.com:/opt/naar-noor/

# Update image references in docker-compose.yml
# Change: naar-noor:backend → registry.example.com/naar-noor:backend-v1.0
# Change: naar-noor:frontend → registry.example.com/naar-noor:frontend-v1.0
```

---

## Step 4: Run Database Migrations

```bash
# On production server, run one-time migrations
docker run --rm \
  -e PGHOST=$PGHOST \
  -e PGPORT=$PGPORT \
  -e PGUSER=$PGUSER \
  -e PGPASSWORD=$PGPASSWORD \
  -e PGDATABASE=$PGDATABASE \
  registry.example.com/naar-noor:backend-v1.0 \
  dotnet ef database update --project src/NaarNoor.Infrastructure

# Or manually:
cd /opt/naar-noor
docker-compose exec naar-noor-prod-api dotnet ef database update
```

---

## Step 5: Start Services

```bash
cd /opt/naar-noor

# Export environment variables
export PGHOST=...
export PGUSER=...
export PGPASSWORD=...
export PGDATABASE=...
export SUPABASE_URL=...
export SUPABASE_ANON_KEY=...
export SUPABASE_SERVICE_ROLE_KEY=...
export JWT_SECRET_KEY=...

# Start containers
docker-compose up -d

# Verify all services started
docker ps
```

---

## Step 6: Verify Deployment

```bash
# Check service health
curl http://localhost/health
curl http://localhost:8080/health

# Check logs
docker logs naar-noor-prod-web
docker logs naar-noor-prod-api
docker logs naar-noor-postgres

# Test API endpoints
curl -X GET http://localhost:8080/api/menu-items

# Test login
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"password123"}'
```

---

## Step 7: Configure Reverse Proxy (Nginx)

```bash
# Create Nginx config
cat > /etc/nginx/sites-available/naar-noor-prod <<'EOF'
upstream naar_noor_frontend {
    server localhost:80;
}

upstream naar_noor_api {
    server localhost:8080;
}

server {
    listen 443 ssl http2;
    server_name naar-noor.com;

    ssl_certificate /etc/ssl/certs/naar-noor.crt;
    ssl_certificate_key /etc/ssl/private/naar-noor.key;

    # API routes
    location /api/ {
        proxy_pass http://naar_noor_api;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # Frontend
    location / {
        proxy_pass http://naar_noor_frontend;
        proxy_set_header Host $host;
    }
}

server {
    listen 80;
    server_name naar-noor.com;
    return 301 https://$server_name$request_uri;
}
EOF

# Enable config
sudo ln -s /etc/nginx/sites-available/naar-noor-prod /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

---

## Step 8: Monitor Production

```bash
# Real-time logs
docker logs -f naar-noor-prod-api

# Check resource usage
docker stats naar-noor-prod-api naar-noor-prod-web naar-noor-postgres

# Database connections
docker exec naar-noor-postgres psql -U $PGUSER -d $PGDATABASE -c "SELECT count(*) FROM pg_stat_activity;"

# Uptime check (set up monitoring)
watch docker ps  # Monitor health status
```

---

## Rollback Procedure

```bash
# If issues occur, rollback to previous version
cd /opt/naar-noor

# Stop current services
docker-compose down

# Restore previous images
docker tag registry.example.com/naar-noor:backend-v0.9 naar-noor:backend
docker tag registry.example.com/naar-noor:frontend-v0.9 naar-noor:frontend

# Start previous version
docker-compose up -d

# Verify rollback
docker ps
curl http://localhost/health
```

---

## Post-Deployment Validation

- [ ] Frontend accessible at naar-noor.com
- [ ] API responding at naar-noor.com/api/health
- [ ] Database migrations applied
- [ ] User registration works
- [ ] User login works
- [ ] JWT tokens generate correctly
- [ ] All endpoints return 200 OK
- [ ] No error logs in docker logs
- [ ] SSL certificate valid
- [ ] DNS propagated

---

## Monitoring & Alerts

Set up alerts for:
- Container restart failures
- Disk space < 20%
- Database connection pool exhausted
- API response time > 2s
- Error rate > 1%

---

## Support Contact

- **On-Call**: [Phone/Slack]
- **Escalation**: [Manager Contact]
- **Incident Channel**: #production-incidents

---

## Rollback Decision Tree

| Symptom | Action |
|---------|--------|
| API not responding | Check `docker logs`, restart container |
| Database connection failed | Verify PGHOST, PGPASSWORD, database exists |
| High memory usage | Check for memory leaks, restart container |
| Frontend not loading | Check Nginx config, CORS settings |
| Auth failures | Verify JWT_SECRET_KEY set correctly |
| Critical issues persist | Execute rollback procedure |

