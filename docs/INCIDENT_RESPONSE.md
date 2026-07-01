# Incident Response & Runbooks

## On-Call Procedures

### Escalation Path
1. **Developer On-Call** (15 min SLA)
2. **Team Lead** (1 hour SLA)
3. **CTO** (for critical incidents)

### Communication
- **Alert Channel:** Slack #incidents
- **Status Page:** Update status.naar-noor.com
- **Incident Tracking:** GitHub Issues with [INCIDENT] tag

---

## Runbook 1: Database Connection Failure

**Symptoms:** API returns 503, database timeouts in logs

**Steps:**
1. Check Supabase status: https://status.supabase.com
2. Verify connection string: `echo $POSTGRESQL_CONNECTION_STRING`
3. Test connection: `psql "$POSTGRESQL_CONNECTION_STRING"`
4. Check connection pool: Backend logs for "pool exhausted"
5. Restart backend: `docker-compose restart naar-noor-prod-api`
6. Verify health: `curl http://localhost:8080/health`

**Recovery Time:** 5-10 minutes

---

## Runbook 2: High Error Rate (>1%)

**Symptoms:** Error rate spike in monitoring, user reports failures

**Steps:**
1. Check Sentry dashboard for error patterns
2. Identify affected endpoint: Filter errors by path
3. Check recent deployments: `git log --oneline -10`
4. Review logs: `docker-compose logs naar-noor-prod-api | tail -100`
5. If rollback needed:
   ```bash
   git revert HEAD
   docker-compose build
   docker-compose up -d
   ```
6. Monitor error rate for 15 minutes

**Recovery Time:** 10-30 minutes

---

## Runbook 3: Out of Memory

**Symptoms:** Services killed, 502 errors, `docker-compose logs` shows OOM

**Steps:**
1. Check memory usage: `docker stats`
2. Identify bloated container: Look for high memory usage
3. Check for memory leaks: Application Insights → Memory trends
4. Restart services: `docker-compose restart`
5. If persists, increase Docker memory limit in production config
6. Monitor memory for 30 minutes

**Recovery Time:** 5 minutes (restart), 1 hour (if config change needed)

---

## Runbook 4: Disk Space Exhausted

**Symptoms:** 503 errors, database writes failing, logs show "no space"

**Steps:**
1. Check disk usage: `df -h`
2. Find large files: `du -sh /*`
3. Clean Docker volumes: `docker system prune --volumes`
4. Delete old logs: `find /var/log -name "*.log" -mtime +30 -delete`
5. Restart services: `docker-compose restart`
6. Monitor disk space

**Prevention:** Set up disk space alerts (> 80% full)

---

## Runbook 5: Rate Limit Bypass / DDoS

**Symptoms:** Massive spike in requests, 429 Too Many Requests returned

**Steps:**
1. Check rate limiting config: DependencyInjection.cs
2. Identify attacker IP: Check nginx access logs
3. Block at firewall level (if available)
4. Increase rate limits temporarily: Update IpRateLimitOptions
5. Deploy updated config: `docker-compose up -d`
6. Review and revert after 24 hours

**Prevention:** Monitor request rates continuously

---

## Runbook 6: Frontend Not Loading

**Symptoms:** Users see blank page, browser console errors

**Steps:**
1. Check Nginx logs: `docker-compose logs naar-noor-prod-web`
2. Verify DNS resolution: `nslookup naar-noor.com`
3. Test connectivity: `curl -I https://naar-noor.com`
4. Check SSL certificate: Valid and not expired
5. Clear browser cache: Ctrl+Shift+Delete
6. Test with different browser/device
7. If problem persists: Restart Nginx `docker-compose restart naar-noor-prod-web`

**Recovery Time:** 5 minutes

---

## Post-Incident Review Template

After every incident:
```
INCIDENT REVIEW: [Date] [Time]
==================

1. What happened?
   - Timeline of events
   - Impact (downtime, affected users)

2. Root cause
   - What specifically went wrong?
   - Why did it happen?

3. What did we do?
   - Steps taken to resolve
   - How long it took

4. What will we do to prevent?
   - Process improvements
   - Code changes
   - Monitoring additions

5. Lessons learned
   - What surprised us?
   - What did we learn?
```

---

## Monitoring & Alerts

Set these thresholds:
- ✅ Error rate > 1%: Alert immediately
- ✅ Response time > 500ms: Alert (warning)
- ✅ CPU usage > 80%: Alert
- ✅ Memory usage > 85%: Alert
- ✅ Disk usage > 90%: Alert
- ✅ Database connections > 15: Alert
- ✅ Health check failures: Alert immediately

---

## Communication During Incidents

**Template for Status Updates:**
```
We are experiencing an issue with [SERVICE].
Status: [INVESTIGATING / IN PROGRESS / RESOLVED]
Impact: [DESCRIPTION]
ETA: [TIME]
Updates: [LATEST ACTIONS TAKEN]
```

Post updates every 15 minutes during active incident.

---

## Resources

- **Sentry Dashboard:** [sentry.io/naar-noor](https://sentry.io/naar-noor)
- **Supabase Dashboard:** [supabase.com/dashboard](https://supabase.com/dashboard)
- **Application Insights:** [portal.azure.com](https://portal.azure.com)
- **GitHub Issues:** [github.com/Mostafa-SAID7/Naar-Noor/issues](https://github.com/Mostafa-SAID7/Naar-Noor/issues)

---

**Last Updated:** July 1, 2026
**Next Review:** January 1, 2027
