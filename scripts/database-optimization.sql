-- Database Optimization Scripts for Naar-Noor
-- Run these against production PostgreSQL database
-- Date: July 1, 2026

-- ============================================================================
-- 1. CREATE PERFORMANCE INDEXES
-- ============================================================================

-- Reservations indexes (for common queries)
CREATE INDEX IF NOT EXISTS idx_reservations_date 
    ON "Reservations"("ReservationDate" DESC)
    WHERE "Status" != 'Cancelled';  -- Partial index for active reservations

CREATE INDEX IF NOT EXISTS idx_reservations_status 
    ON "Reservations"("Status");

CREATE INDEX IF NOT EXISTS idx_reservations_customer 
    ON "Reservations"("Email");

-- Menu items indexes
CREATE INDEX IF NOT EXISTS idx_menu_items_category 
    ON "MenuItems"("Category");

CREATE INDEX IF NOT EXISTS idx_menu_items_available 
    ON "MenuItems"("IsAvailable")
    WHERE "IsAvailable" = true;  -- Partial index for available items only

CREATE INDEX IF NOT EXISTS idx_menu_items_sort 
    ON "MenuItems"("Category", "SortOrder");

-- Reviews indexes
CREATE INDEX IF NOT EXISTS idx_reviews_approved 
    ON "Reviews"("IsApproved" DESC, "CreatedAt" DESC);

CREATE INDEX IF NOT EXISTS idx_reviews_created_at 
    ON "Reviews"("CreatedAt" DESC);

-- Chef indexes
CREATE INDEX IF NOT EXISTS idx_chefs_active 
    ON "Chefs"("IsActive");

-- Orders indexes (for aggregation queries)
CREATE INDEX IF NOT EXISTS idx_orders_created_at 
    ON "Orders"("CreatedAt" DESC);

CREATE INDEX IF NOT EXISTS idx_orders_status 
    ON "Orders"("Status");

-- ============================================================================
-- 2. ENABLE QUERY STATISTICS
-- ============================================================================

-- Create extension for query tracking (if not exists)
CREATE EXTENSION IF NOT EXISTS pg_stat_statements;

-- Set minimum duration for slow query log (log queries > 100ms)
ALTER SYSTEM SET log_min_duration_statement = 100;

-- Set query statistics to track
ALTER SYSTEM SET pg_stat_statements.track = 'all';

-- Apply changes
SELECT pg_reload_conf();

-- ============================================================================
-- 3. ANALYZE TABLE STATISTICS
-- ============================================================================

-- Update statistics for query planner optimization
ANALYZE "Reservations";
ANALYZE "MenuItems";
ANALYZE "Reviews";
ANALYZE "Chefs";
ANALYZE "Orders";
ANALYZE "ContactInquiries";

-- ============================================================================
-- 4. VERIFY INDEXES CREATED
-- ============================================================================

-- List all indexes on tables
SELECT
    tablename,
    indexname,
    indexdef
FROM pg_indexes
WHERE tablename IN ('Reservations', 'MenuItems', 'Reviews', 'Chefs', 'Orders')
ORDER BY tablename, indexname;

-- ============================================================================
-- 5. CHECK SLOW QUERIES (after running queries for a while)
-- ============================================================================

-- Top 10 slowest queries
SELECT
    query,
    calls,
    total_time,
    mean_time,
    max_time
FROM pg_stat_statements
WHERE query NOT LIKE '%pg_stat_statements%'
    AND query NOT LIKE '%pg_indexes%'
ORDER BY mean_time DESC
LIMIT 10;

-- ============================================================================
-- 6. CONNECTION POOL OPTIMIZATION
-- ============================================================================

-- Check current connections
SELECT
    datname,
    count(*) as connections,
    max_conn
FROM (
    SELECT datname, count(*) as connections
    FROM pg_stat_activity
    GROUP BY datname
) stats,
(
    SELECT setting::int as max_conn
    FROM pg_settings
    WHERE name = 'max_connections'
) limits
GROUP BY datname, max_conn
ORDER BY connections DESC;

-- Current pool settings (view in appsettings.json)
-- MaxPoolSize: 20 (recommended for mid-scale app)
-- MinPoolSize: 5 (default)
-- Connection timeout: 30 seconds
-- Lifetime: 5 minutes (connection recycling)

-- ============================================================================
-- 7. TABLE STATISTICS AND MAINTENANCE
-- ============================================================================

-- Check table sizes
SELECT
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) as size
FROM pg_tables
WHERE schemaname NOT IN ('pg_catalog', 'information_schema')
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;

-- Check for missing indexes (unused indexes > 50MB)
SELECT
    schemaname,
    tablename,
    indexname,
    idx_scan,
    pg_size_pretty(pg_relation_size(indexrelid)) as idx_size
FROM pg_stat_user_indexes
WHERE idx_scan = 0
    AND pg_relation_size(indexrelid) > 50000
ORDER BY pg_relation_size(indexrelid) DESC;

-- ============================================================================
-- 8. CACHE INVALIDATION ON INSERT/UPDATE
-- ============================================================================

-- Create trigger function to invalidate menu cache when items change
CREATE OR REPLACE FUNCTION invalidate_menu_cache()
RETURNS TRIGGER AS $$
BEGIN
    -- In production, call cache invalidation API or use messaging
    -- For now, log the change for monitoring
    RAISE NOTICE 'Menu items cache should be invalidated. Changed at: %', NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create triggers (if cache invalidation logic is implemented)
DROP TRIGGER IF EXISTS menu_items_cache_invalidate ON "MenuItems";
CREATE TRIGGER menu_items_cache_invalidate
AFTER INSERT OR UPDATE ON "MenuItems"
FOR EACH ROW
EXECUTE FUNCTION invalidate_menu_cache();

-- ============================================================================
-- 9. QUERY PERFORMANCE MONITORING
-- ============================================================================

-- Create custom monitoring view
CREATE OR REPLACE VIEW v_slow_queries AS
SELECT
    query,
    calls,
    total_time as total_ms,
    ROUND(mean_time, 2) as avg_ms,
    ROUND(max_time, 2) as max_ms,
    CASE 
        WHEN mean_time > 1000 THEN 'CRITICAL'
        WHEN mean_time > 500 THEN 'WARNING'
        WHEN mean_time > 100 THEN 'MONITOR'
        ELSE 'OK'
    END as severity
FROM pg_stat_statements
WHERE query NOT LIKE '%pg_stat_statements%'
ORDER BY mean_time DESC;

-- Query the monitoring view
-- SELECT * FROM v_slow_queries WHERE severity != 'OK';

-- ============================================================================
-- 10. RECOMMENDATIONS FOR PRODUCTION
-- ============================================================================

/*
BEFORE PRODUCTION DEPLOYMENT, VERIFY:

1. ✅ All indexes created successfully (check pg_indexes view)
2. ✅ Statistics updated (ANALYZE run)
3. ✅ Slow query logging enabled (log_min_duration_statement = 100)
4. ✅ Connection pool sized correctly (MaxPoolSize = 20-30 for production)
5. ✅ Backup configured (daily incremental, weekly full)
6. ✅ Monitoring alerts set up for:
   - Query > 500ms
   - Connection pool > 80% usage
   - Disk space > 80% used
   - Replication lag > 1 second (if read replica)

ONGOING MAINTENANCE:

- Run VACUUM ANALYZE weekly
- Review slow queries monthly
- Adjust indexes based on query patterns
- Monitor cache hit rates
- Rotate slow query logs monthly

MONITORING QUERIES TO RUN REGULARLY:

1. Slow queries: SELECT * FROM v_slow_queries WHERE severity != 'OK';
2. Cache stats: SELECT * FROM pg_stat_statements ORDER BY mean_time DESC LIMIT 5;
3. Index usage: SELECT * FROM pg_stat_user_indexes ORDER BY idx_scan;
4. Table sizes: SELECT * FROM pg_tables ORDER BY pg_total_relation_size DESC;
*/
