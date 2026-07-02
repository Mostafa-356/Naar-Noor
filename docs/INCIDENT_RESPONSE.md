# Incident Response

## Escalation Path

1. **Developer On-Call** → 15 min response
2. **Team Lead** → 1 hour response
3. **CTO** → Critical incidents only

**Channels:** Slack `#incidents` · Status page: `status.naar-noor.com` · GitHub Issues `[INCIDENT]` tag

---

## Runbooks

### DB Connection Failure
*Symptoms: API 503, timeout errors in logs*
```bash
echo $POSTGRESQL_CONNECTION_STRING       # verify env var set
psql "$POSTGRESQL_CONNECTION_STRING"     # test connection
docker-compose restart naar-noor-prod-api
curl http://localhost:8080/health
```
Check [status.supabase.com](https://status.supabase.com) first.

---

### High Error Rate (>1%)
*Symptoms: Error spike in monitoring, user reports*
```bash
git log --oneline -10                                      # check recent deploys
docker-compose logs naar-noor-prod-api | tail -100        # review logs
# If rollback needed:
git revert HEAD && docker-compose build && docker-compose up -d
```

---

### Out of Memory
*Symptoms: 502 errors, OOM in docker logs*
```bash
docker stats                    # identify bloated container
docker-compose restart          # quick fix
```
If persists: increase Docker memory limit in production config.

---

### Disk Space Exhausted
*Symptoms: 503s, DB write failures, "no space" in logs*
```bash
df -h
docker system prune --volumes
find /var/log -name "*.log" -mtime +30 -delete
docker-compose restart
```

---

### Frontend Not Loading
*Symptoms: Blank page, browser console errors*
```bash
docker-compose logs naar-noor-prod-web
curl -I https://naar-noor.com
docker-compose restart naar-noor-prod-web
```

---

## Alert Thresholds

| Metric | Threshold | Action |
|--------|-----------|--------|
| Error rate | > 1% | Alert immediately |
| Response time | > 500ms | Warning |
| CPU usage | > 80% | Alert |
| Memory usage | > 85% | Alert |
| Disk usage | > 90% | Alert |
| Health check fails | Any | Alert immediately |

---

## Status Update Template

```
SERVICE: [name]
STATUS: INVESTIGATING / IN PROGRESS / RESOLVED
IMPACT: [description]
ETA: [time]
LAST ACTION: [what was done]
```
Post updates every 15 minutes during active incident.

---

## Post-Incident Review

After every incident, document:
1. What happened + timeline
2. Root cause
3. Steps taken + resolution time
4. Prevention actions
5. Lessons learned
