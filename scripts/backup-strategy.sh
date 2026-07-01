#!/bin/bash
# Backup and Disaster Recovery Strategy for Naar-Noor
# This document outlines backup procedures and recovery targets

cat << 'EOF'
═══════════════════════════════════════════════════════════════════════════
NAAR-NOOR: BACKUP & DISASTER RECOVERY STRATEGY
═══════════════════════════════════════════════════════════════════════════

## RTO/RPO TARGETS
- RTO (Recovery Time Objective): 4 hours (maximum downtime acceptable)
- RPO (Recovery Point Objective): 1 hour (maximum data loss acceptable)

## BACKUP STRATEGY

### 1. DATABASE BACKUPS (Supabase PostgreSQL)
Supabase automatically handles:
✓ Daily incremental backups
✓ Weekly full backups
✓ 30-day retention
✓ Point-in-time recovery (PITR)

Manual verification:
- Check backup status: https://supabase.com/dashboard/project/YOUR_PROJECT/settings/backups
- Test restore monthly: Verify you can access backup snapshots

### 2. APPLICATION CODE
- Stored in GitHub (version control)
- All commits backed up (git push)
- Docker images stored in container registry

### 3. FILE STORAGE (Chef/Menu Images)
- Stored in Supabase Storage bucket
- Automatic S3-backed redundancy
- Verify: Check storage usage in Supabase dashboard

## RECOVERY PROCEDURES

### SCENARIO 1: Database Corruption
1. Access Supabase Dashboard
2. Go to Settings → Backups
3. Click "Restore from backup"
4. Select restore point (within last 7 days)
5. Wait for restore to complete (typically 30-60 minutes)
6. Verify data integrity
7. RTO: ~1 hour

### SCENARIO 2: Complete Application Failure
1. Pull latest code from GitHub: git clone https://github.com/Mostafa-SAID7/Naar-Noor.git
2. Rebuild Docker images: docker-compose build
3. Deploy: docker-compose up -d
4. Verify health checks: curl http://localhost:8080/health
5. RTO: ~30 minutes

### SCENARIO 3: Accidental Data Deletion
1. Request point-in-time recovery from Supabase
2. Specify time before deletion occurred
3. Supabase restores database to that point
4. RTO: ~2 hours
5. RPO: ~1 hour (depending on backup interval)

## TESTING SCHEDULE

- Monthly: Test database restore procedure
- Quarterly: Test full application recovery
- After major updates: Verify backups are working

## MONITORING & ALERTS

Set up in production:
- Backup failure alerts
- Disk space monitoring
- Database size tracking

## DEPLOYMENT: Multi-Region Recommendation

For future enhanced DR:
1. Primary: Primary region (current)
2. Standby: Secondary region (async replication)
3. Failover: Automatic or manual DNS switch

Current: Single region (acceptable for initial launch)
Future: Consider multi-region for enterprise SLA

═══════════════════════════════════════════════════════════════════════════
EOF
