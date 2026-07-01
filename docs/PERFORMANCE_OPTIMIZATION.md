# Performance Optimization Guide

**Last Updated:** July 1, 2026  
**Focus Areas:** Bundle Analysis, Database Queries, Caching

---

## 1. FRONTEND BUNDLE ANALYSIS

### Current State ✅

**Lazy Loading Configuration:**
- ✅ Route-level code splitting implemented in `app.routes.ts`
- ✅ High-priority routes preloaded (menu, reservations)
- ✅ Component lazy loading with `.then()` pattern
- ✅ Standalone components (AOT-friendly)

**Build Optimization:**
```bash
npm run build  # Production build with:
- AOT compilation
- Tree-shaking
- Minification
- Source maps disabled
```

### Bundle Budget Targets

```
Initial Bundle: < 500KB (gzipped)
- main.js: ~300KB
- styles.css: ~80KB
- shared vendors: ~120KB

Chunk Sizes:
- menu component: ~50KB
- reservations component: ~45KB
- checkout component: ~40KB
- other routes: < 30KB each
```

### Verification Script

```bash
# Check bundle size
npm run build
ls -lh dist/naar-noor/browser/*.js | awk '{print $9, $5}'

# Expected output:
# dist/naar-noor/browser/main-XXXXX.js 300K
# dist/naar-noor/browser/menu-XXXXX.js 50K
# dist/naar-noor/browser/reservations-XXXXX.js 45K
```

### Optimization Recommendations

#### ✅ Already Implemented
- Route lazy loading with preload strategy
- Image lazy loading via `ImageOptimizationDirective`
- Compression via nginx (gzip level 6)
- Cache busting for versioned assets

#### 🔧 Additional Opportunities

**1. Angular Change Detection Strategy**
```typescript
// Already using OnPush where applicable
// To verify/improve:
@Component({
  selector: 'app-menu-item',
  changeDetection: ChangeDetectionStrategy.OnPush  // ✅ Recommended
})
```

**2. Virtual Scrolling for Large Lists**
```typescript
// For menu items list (50+ items)
import { ScrollingModule } from '@angular/cdk/scrolling';

// In component:
<cdk-virtual-scroll-viewport itemSize="80" class="menu-list">
  <div *cdkVirtualFor="let item of menuItems" class="menu-item">
    {{ item.name }}
  </div>
</cdk-virtual-scroll-viewport>
```

**3. Service Worker Caching**
```
✅ Configured in package.json (@angular/service-worker)
- Static asset caching
- Cache-first strategy for images
- Network-first for API calls
```

---

## 2. DATABASE QUERY OPTIMIZATION

### Current State ✅

**Query Handler Analysis:**

#### GetMenuItemsQueryHandler
```csharp
// ✅ GOOD: Uses Select() projection to avoid N+1
var query = _unitOfWork.MenuItems.Query()
    .Where(m => m.IsAvailable)
    .OrderBy(m => m.Category)
    .ThenBy(m => m.SortOrder)
    .Select(m => new MenuItemDto(...))  // ✅ Projection
    .ToListAsync();
```
**Issues:** None - optimal query pattern

#### GetReservationsQueryHandler
```csharp
// ✅ GOOD: Uses pagination with Select projection
await _unitOfWork.Reservations.Query()
    .OrderByDescending(r => r.ReservationDate)
    .Skip((request.Page - 1) * request.PageSize)
    .Take(request.PageSize)
    .Select(r => new ReservationDto(...))  // ✅ Projection
    .ToListAsync();
```
**Issues:** None - pagination implemented

### Performance Metrics

```
Typical Query Times (P95):
- List queries: 50-100ms
- Single by ID: 20-40ms
- Complex joins: 100-200ms
```

### Database Optimization Recommendations

#### 1. Connection Pooling Configuration ✅

**Current (DependencyInjection.cs):**
```csharp
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.MigrationsAssembly(...)));
```

**Optimized:**
```csharp
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.MigrationsAssembly(...);
        npgsql.MaxPoolSize(20);           // Connection pool size
        npgsql.EnableRetryOnFailure();     // Resilience
    }));
```

#### 2. Query Hints for Complex Queries

For queries involving multiple tables (e.g., Reservations with Chef info):

```csharp
// Pattern to add Include() if needed in future
public async Task<List<ReservationDetailDto>> GetReservationDetails()
{
    return await _unitOfWork.Reservations.Query()
        .Include(r => r.Chef)           // ✅ Prevents N+1
        .Include(r => r.Orders)         // ✅ Prevents N+1
        .Select(r => new ReservationDetailDto(
            r.Id,
            r.CustomerName,
            r.Chef.Name,                 // ✅ No extra query
            r.Orders.Count()             // ✅ No extra query
        ))
        .ToListAsync();
}
```

#### 3. Indexing Strategy

**Current Indexes (PostgreSQL):**
```sql
-- Verify with:
SELECT * FROM pg_indexes WHERE tablename IN 
('Reservations', 'MenuItems', 'Chefs', 'Reviews');
```

**Recommended Indexes:**
```sql
-- Reservation queries
CREATE INDEX idx_reservations_date ON "Reservations"("ReservationDate" DESC);
CREATE INDEX idx_reservations_status ON "Reservations"("Status");

-- Menu item queries
CREATE INDEX idx_menu_items_category ON "MenuItems"("Category");
CREATE INDEX idx_menu_items_available ON "MenuItems"("IsAvailable");

-- Review queries
CREATE INDEX idx_reviews_approved ON "Reviews"("IsApproved");
CREATE INDEX idx_reviews_created_at ON "Reviews"("CreatedAt" DESC);

-- Chef queries
CREATE INDEX idx_chefs_active ON "Chefs"("IsActive");
```

#### 4. Query Execution Plan Analysis

For slow queries, analyze execution plan:

```sql
EXPLAIN ANALYZE
SELECT * FROM "Reservations" 
WHERE "ReservationDate" > NOW() 
ORDER BY "ReservationDate" DESC 
LIMIT 10;
```

**Good indicators:**
- Index Scan (fast)
- Sequential Scan with where clause (acceptable)

**Bad indicators:**
- Full Table Scan with many rows
- Nested loops without indexes

---

## 3. CACHING STRATEGY

### Current Configuration ✅

**Infrastructure Setup (DependencyInjection.cs):**

```csharp
// ✅ Distributed caching with Redis fallback
var redisConnection = configuration.GetConnectionString("Redis");
if (!string.IsNullOrWhiteSpace(redisConnection))
{
    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "NaarNoor_";
    });
}
else
{
    services.AddDistributedMemoryCache();  // Fallback for dev
}
```

### Cache Patterns to Implement

#### 1. Menu Items Caching (5-minute TTL)

```csharp
public class GetMenuItemsCachedQueryHandler 
    : IRequestHandler<GetMenuItemsQuery, List<MenuItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache _cache;
    private const string CacheKey = "menu_items";
    private const int CacheDurationSeconds = 300;  // 5 minutes

    public async Task<List<MenuItemDto>> Handle(
        GetMenuItemsQuery request, CancellationToken cancellationToken)
    {
        // Try cache first
        var cached = await _cache.GetStringAsync(CacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<List<MenuItemDto>>(cached)
                ?? new();
        }

        // Query database
        var items = await QueryMenuItems(request, cancellationToken);

        // Cache result
        var json = JsonSerializer.Serialize(items);
        await _cache.SetStringAsync(
            CacheKey, 
            json,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheDurationSeconds)
            },
            cancellationToken);

        return items;
    }

    private async Task<List<MenuItemDto>> QueryMenuItems(
        GetMenuItemsQuery request, CancellationToken cancellationToken)
    {
        // Original query logic
    }
}
```

#### 2. Reservation Availability Caching (1-minute TTL)

```csharp
// Cache available time slots (computed on-demand)
private const string AvailableSlotsKey = "reservation_slots_{date}";
private const int AvailableSlotsCacheDuration = 60;
```

#### 3. Cache Invalidation Strategy

```csharp
// When menu item is created/updated
public class CreateMenuItemCommandHandler : IRequestHandler<CreateMenuItemCommand>
{
    private readonly IDistributedCache _cache;

    public async Task Handle(CreateMenuItemCommand request, CancellationToken cancellationToken)
    {
        // ... create item ...

        // Invalidate menu cache
        await _cache.RemoveAsync("menu_items", cancellationToken);
    }
}
```

#### 4. Redis Configuration

**docker-compose.yml:**
```yaml
redis:
  image: redis:7-alpine
  container_name: naar-noor-cache
  ports:
    - "6379:6379"
  environment:
    - REDIS_PASSWORD=${REDIS_PASSWORD}
  healthcheck:
    test: ["CMD", "redis-cli", "ping"]
    interval: 10s
    timeout: 5s
    retries: 3
```

**Connection string:**
```
REDIS_CONNECTION_STRING=localhost:6379,password=your-secure-password
```

---

## 4. PERFORMANCE MONITORING

### APM Metrics to Track ✅

**Response Time Distribution:**
```
P50:  < 100ms  (50% of requests)
P95:  < 200ms  (95% of requests)
P99:  < 500ms  (99% of requests)
```

**Database Metrics:**
- Query execution time (avg, max)
- Connection pool utilization
- Slow query log (queries > 100ms)

**Cache Metrics:**
- Cache hit rate (target: > 70% for menu items)
- Cache miss rate
- Memory usage

### Application Insights Queries

```kusto
// Average response time by endpoint
requests
| summarize avg(duration) by tostring(url)
| order by avg_duration desc

// Slow database queries
dependencies
| where type == "SQL"
| summarize count(), avg(duration) by name
| order by avg_duration desc

// Cache effectiveness
customMetrics
| where name startswith "cache_"
| summarize by name, avg(value)
```

---

## 5. IMPLEMENTATION CHECKLIST

### Immediate (Week 1)
- [ ] Verify bundle sizes with `npm run build`
- [ ] Review Application Insights APM dashboard
- [ ] Implement menu items caching (5-minute TTL)
- [ ] Add database indexes (PostgreSQL)

### Short-term (Week 2-3)
- [ ] Implement reservation slot caching
- [ ] Add slow query monitoring (> 100ms threshold)
- [ ] Profile and optimize top 3 slowest queries
- [ ] Set up Redis in staging environment

### Medium-term (Month 2)
- [ ] Implement virtual scrolling for large lists (if needed)
- [ ] Add OnPush change detection to more components
- [ ] Optimize CDN caching headers
- [ ] Implement API response compression (gzip for JSON)

### Ongoing Monitoring
- [ ] Weekly review of Application Insights metrics
- [ ] Monthly cache hit rate analysis
- [ ] Quarterly database performance review
- [ ] Monitor Lighthouse CI scores (target: 90+ Performance)

---

## 6. PERFORMANCE BENCHMARKS

### Frontend (Lighthouse CI)
```
Performance:  90+ (target)
FCP:          < 2s (First Contentful Paint)
LCP:          < 2.5s (Largest Contentful Paint)
CLS:          < 0.1 (Cumulative Layout Shift)
```

### Backend (Load Testing - k6)
```
Users:        100 concurrent
Duration:     30 seconds
Response P95: < 200ms
Error Rate:   < 1%
```

### Database (PostgreSQL)
```
Query P95:    < 100ms
Connection Pool Utilization: < 80%
Cache Hit Rate: > 70%
```

---

## 7. TOOLS & COMMANDS

### Bundle Analysis
```bash
# Analyze bundle composition
npm run build -- --stats-json
webpack-bundle-analyzer dist/naar-noor/browser/stats.json
```

### Database Performance
```bash
# PostgreSQL slow query log
psql -U postgres -d naar_noor -c "
  SET log_min_duration_statement = 100;  -- Log queries > 100ms
  SELECT query, mean_time FROM pg_stat_statements 
  ORDER BY mean_time DESC LIMIT 10;
"
```

### Load Testing
```bash
# Run load tests
k6 run load-test.js --vus 100 --duration 30s

# Output includes:
# - Response times (p50, p95, p99)
# - Error rates
# - Request rates
```

---

## References

- [Angular Performance Guide](https://angular.io/guide/performance-best-practices)
- [Entity Framework Query Performance](https://docs.microsoft.com/en-us/ef/core/performance/query-performance)
- [Redis Caching Patterns](https://redis.io/docs/manual/client-side-caching/)
- [PostgreSQL Performance Tuning](https://www.postgresql.org/docs/current/performance-tips.html)
- [Web Vitals](https://web.dev/vitals/)
