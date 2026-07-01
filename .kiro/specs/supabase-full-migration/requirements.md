# Naar-Noor: Complete Supabase Migration & Integration

## Executive Summary
Migrate Naar-Noor backend from SQL Server to Supabase (PostgreSQL) with full integration of Supabase Auth, Storage, and Realtime features. The backend is already using PostgreSQL—this spec ensures full production-readiness, security best practices, and comprehensive feature implementation.

**Status**: 95% Complete - Cleanup & Production Hardening Required

---

## 1. Current State Assessment

### ✅ Already Implemented
- PostgreSQL database via Supabase (Npgsql 8.0.0)
- Supabase Authentication Service (REST API)
- Supabase Storage Service (chef-images, menu-item-images buckets)
- Supabase Realtime Service (WebSocket subscriptions)
- EF Core 8.0 with proper migration system
- Entity models properly configured for PostgreSQL
- Dependency injection all configured
- Feature flags in appsettings (ENABLE_SUPABASE_*)

### ⚠️ Incomplete/Outstanding
- Docker Compose: Still references removed SQL Server (FIXED ✅)
- Environment variable configuration inconsistencies
- Row-Level Security (RLS) policies not implemented in Supabase
- Realtime channel integration incomplete on frontend
- Storage bucket access policies need hardening
- API route authentication middleware incomplete

---

## 2. Business Requirements

### 2.1 Authentication System
**Requirement**: User authentication via Supabase Auth with email/password flow

- Users can register with email and password
- Email verification flow (optional confirmation)
- Password reset functionality
- Session token management (JWT)
- User profile data management
- Role-based access (future: admin, chef, customer)

### 2.2 Data Storage & Retrieval
**Requirement**: Reliable data persistence in PostgreSQL with full ACID compliance

- Reservations with concurrent booking handling
- Menu items with dietary preferences
- Chef profiles with images
- Orders with order items (atomic transactions)
- Reviews with approval workflow
- Contact inquiries
- Audit trail (CreatedAt, UpdatedAt timestamps)

### 2.3 File Storage
**Requirement**: Secure image uploads with CDN delivery

- Chef profile images (JPEG, PNG, max 5MB)
- Menu item photos (JPEG, PNG, max 10MB)
- Public URL generation for web display
- Automatic cleanup when records deleted
- CORS-enabled for frontend access

### 2.4 Real-Time Updates
**Requirement**: Live data synchronization for:

- Order status changes
- Reservation confirmations/cancellations
- New reviews appearing on menu items
- Table availability updates
- Admin notifications

### 2.5 Security Requirements
**Requirement**: Production-grade security hardening

- API requires authentication for data modifications
- Supabase RLS policies enforce data isolation
- Rate limiting on auth endpoints
- HTTPS-only communication
- Secure secret management (environment variables)
- CORS properly configured
- SQL injection prevention (EF Core parameterized queries)

---

## 3. Scope Boundaries

### In Scope
- Database connectivity via PostgreSQL/Supabase
- Authentication service implementation
- Storage service for images
- Realtime subscription infrastructure
- API endpoint security
- Environment configuration
- Docker compose cleanup
- Database migration strategy

### Out of Scope
- Frontend Supabase SDK integration (client-side realtime)
- Email service configuration (Supabase built-in)
- Analytics & monitoring dashboards
- Advanced audit logging system
- Multi-tenancy implementation
- Data backup/restore procedures

---

## 4. Technical Architecture

### 4.1 Technology Stack
- **Framework**: ASP.NET Core 8.0 LTS
- **Database**: PostgreSQL 14+ (Supabase hosted)
- **ORM**: Entity Framework Core 8.0.11
- **Database Driver**: Npgsql 8.0.0
- **Auth Provider**: Supabase (REST API)
- **File Storage**: Supabase Storage (REST API)
- **Real-time**: Supabase Realtime (WebSocket)
- **Containerization**: Docker & Docker Compose

### 4.2 Database Schema (PostgreSQL Types)
```sql
-- All tables use uuid primary keys
-- All timestamps are 'timestamp with time zone'
-- Decimals use numeric(10,2) for currency

Chefs (id, name, title, bio, image_url, specialty, is_active, sort_order, created_at, updated_at)
MenuItems (id, name, description, price, category, is_vegetarian, is_vegan, is_gluten_free, is_available, image_url, sort_order, created_at, updated_at)
Reservations (id, customer_name, email, phone_number, reservation_date, reservation_time, party_size, status, special_requests, created_at, updated_at)
Orders (id, customer_name, email, phone_number, notes, type, delivery_address, table_reservation_name, status, total_amount, created_at, updated_at)
OrderItems (id, order_id, menu_item_id, menu_item_name, unit_price, quantity, created_at, updated_at) -> FK: Orders.id
Reviews (id, customer_name, rating, comment, is_approved, source, created_at, updated_at)
ContactInquiries (id, name, email, phone_number, subject, message, is_read, created_at, updated_at)
```

### 4.3 Service Layer Architecture
```
ISupabaseAuthService
  ├── RegisterUserAsync(email, password)
  ├── LoginUserAsync(email, password)
  ├── VerifyTokenAsync(token)
  ├── GetCurrentUserAsync(token)
  ├── UpdateUserEmailAsync(userId, email)
  ├── UpdateUserPasswordAsync(userId, password)
  └── ResetPasswordAsync(email)

ISupabaseStorageService
  ├── UploadImageAsync(bucket, fileName, fileData, contentType)
  ├── UploadChefImageAsync(chefId, imageData, contentType)
  ├── UploadMenuItemImageAsync(menuItemId, imageData, contentType)
  ├── DeleteFileAsync(bucket, filePath)
  ├── DeleteChefImageAsync(chefId)
  ├── DeleteMenuItemImageAsync(menuItemId)
  ├── ListFilesAsync(bucket, prefix)
  └── GetPublicUrl(bucket, filePath)

ISupabaseRealtimeService
  ├── SubscribeToOrderUpdatesAsync(orderId, onUpdate, onError)
  ├── SubscribeToReservationUpdatesAsync(reservationId, onUpdate, onError)
  ├── SubscribeToReviewUpdatesAsync(menuItemId, onUpdate, onError)
  ├── SubscribeToTableAvailabilityAsync(onUpdate, onError)
  ├── UnsubscribeAsync(subscriptionId)
  ├── BroadcastMessageAsync(channel, eventName, payload)
  └── ReconnectAsync()
```

### 4.4 API Security Architecture
```
Request → CORS Middleware → Auth Middleware (JWT Verification) → Authorization → Route Handler
                                         ↓
                            Bearer Token in Authorization Header
                            Validated against Supabase
```

---

## 5. Configuration Management

### 5.1 Environment Variables (Production)
```bash
# Database
POSTGRESQL_CONNECTION_STRING=Host=db.YOUR_PROJECT.supabase.co;Port=5432;Database=postgres;User Id=postgres;Password=YOUR_PASSWORD_HERE

# Supabase
SUPABASE_URL=https://YOUR_PROJECT.supabase.co
SUPABASE_ANON_KEY=YOUR_ANON_KEY
SUPABASE_SERVICE_ROLE_KEY=YOUR_SERVICE_ROLE_KEY

# API
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:80
API_CORS_ORIGINS=https://naarnooreats.com,https://www.naarnooreats.com

# Features
ENABLE_SWAGGER=false
ENABLE_HEALTH_CHECK=true
ENABLE_SUPABASE_AUTH=true
ENABLE_SUPABASE_STORAGE=true
ENABLE_SUPABASE_REALTIME=true
```

### 5.2 appsettings.json Structure
```json
{
  "Logging": { /* production logging config */ },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=...",
    "SupabasePostgresql": "Host=..."
  },
  "Supabase": {
    "Url": "https://YOUR_PROJECT.supabase.co",
    "AnonKey": "YOUR_ANON_KEY_HERE",
    "ServiceRoleKey": "YOUR_SERVICE_ROLE_KEY_HERE"
  },
  "Features": {
    "EnableSwagger": false,
    "EnableHealthCheck": true,
    "EnableSupabaseAuth": true,
    "EnableSupabaseStorage": true,
    "EnableSupabaseRealtime": true
  }
}
```

---

## 6. Security Hardening

### 6.1 Database Security
- [x] PostgreSQL via Supabase (managed service)
- [ ] Row-Level Security (RLS) policies enabled
- [ ] Service role key restricted to backend only
- [ ] Anon key restricted to public operations
- [ ] Column-level encryption for sensitive fields (future)

### 6.2 API Security
- [x] JWT authentication on protected routes
- [ ] Rate limiting implementation (e.g., 100 req/min per IP)
- [ ] CORS properly configured (not * in production)
- [ ] HTTPS enforced
- [ ] Security headers (CSP, X-Frame-Options, etc.)
- [ ] Input validation on all endpoints
- [ ] Output encoding for XSS prevention

### 6.3 Storage Security
- [ ] Storage bucket policies enforcing authentication
- [ ] File type validation (whitelist: jpg, png only)
- [ ] File size limits enforced
- [ ] Automatic cleanup of orphaned files
- [ ] CDN caching headers properly set

### 6.4 Secret Management
- [x] Secrets in environment variables only
- [ ] No hardcoded secrets in code
- [ ] Secrets rotation policy (quarterly)
- [ ] Audit logging of secret access (future)

---

## 7. Testing Strategy

### 7.1 Unit Tests
- Service layer: Auth, Storage, Realtime
- Repository pattern
- Entity configurations
- Validation rules

### 7.2 Integration Tests
- Database connectivity
- Migration execution
- CRUD operations per entity
- Authentication flow
- Storage upload/download

### 7.3 End-to-End Tests (Cypress)
- User registration flow
- Login/logout
- Order placement
- Reservation booking
- File uploads

### 7.4 Performance Tests
- Database query optimization
- N+1 query prevention
- Connection pooling
- API response times (< 200ms target)

---

## 8. Deployment Strategy

### 8.1 Production Environment
- **Hosting**: Docker containers on cloud platform (AWS ECS, Azure Container Instances, etc.)
- **Database**: Supabase PostgreSQL (managed)
- **Container Registry**: Docker Hub or cloud provider registry
- **CI/CD**: GitHub Actions workflows (already configured)

### 8.2 Deployment Checklist
```
Pre-Deployment:
  [ ] All tests passing (unit, integration, E2E)
  [ ] Code review approved
  [ ] Security scan passed
  [ ] Performance tests passed
  [ ] Database migrations tested on staging

Deployment:
  [ ] Environment variables configured correctly
  [ ] Docker image built and pushed
  [ ] Rolling deployment (0-downtime)
  [ ] Database migrations applied
  [ ] Health checks passing
  [ ] API endpoints responding

Post-Deployment:
  [ ] Smoke tests run
  [ ] Monitoring alerts active
  [ ] Logs reviewed for errors
  [ ] Database replication verified
```

### 8.3 Rollback Plan
- Keep previous Docker image tagged
- Database migration rollback scripts prepared
- Supabase backup before migration
- Health check monitoring 30 minutes post-deployment

---

## 9. Monitoring & Observability

### 9.1 Logging
- Application logs: Structured (JSON) format
- Database query logs (slow query threshold: 1000ms)
- API request/response logging
- Error tracking with stack traces
- Audit logging for auth/security events

### 9.2 Metrics
- API response times (percentiles: p50, p95, p99)
- Database connection pool usage
- Error rates (5xx, 4xx by endpoint)
- Database query performance
- Storage upload/download success rates

### 9.3 Alerting
- Critical: API down, database connection failed
- Warning: High error rate, slow response times, storage errors
- Info: Deployment events, configuration changes

---

## 10. Maintenance & Operations

### 10.1 Database Maintenance
- Weekly backup verification
- Monthly VACUUM/ANALYZE on PostgreSQL (handled by Supabase)
- Index monitoring and optimization
- Connection pool tuning

### 10.2 Security Updates
- Weekly security patch review
- NuGet package updates (monthly)
- Docker base image updates
- Supabase service updates

### 10.3 Performance Optimization
- Query performance reviews (monthly)
- Database index tuning
- API caching strategy implementation
- Storage optimization (cleanup old files)

---

## 11. Success Criteria

### Functional
- ✅ All CRUD operations working via PostgreSQL/Supabase
- ✅ User registration and login functional
- ✅ File uploads to storage working
- [ ] Realtime subscriptions receiving updates
- [ ] Row-Level Security policies enforced

### Performance
- API response time < 200ms for 95% of requests
- Database queries < 100ms
- No N+1 query issues
- Connection pool optimized

### Security
- All secrets in environment variables
- No SQL injection vulnerabilities
- JWT token validation on all protected routes
- CORS properly configured
- API rate limiting active

### Reliability
- 99.9% uptime target
- Zero data loss events
- Successful rollback capability
- Monitoring and alerts active

---

## 12. Deliverables

### Code Deliverables
1. ✅ ApplicationDbContext.cs (PostgreSQL configured)
2. ✅ SupabaseAuthService.cs (fully implemented)
3. ✅ SupabaseStorageService.cs (fully implemented)
4. ✅ SupabaseRealtimeService.cs (fully implemented)
5. ✅ DependencyInjection.cs (all services registered)
6. ✅ Database migrations (v20260628044513)
7. ✅ Entity configurations
8. ✅ Repository pattern implementation
9. ✅ API Controllers with auth middleware
10. ✅ Unit & integration tests

### Configuration Deliverables
1. ✅ appsettings.json (PostgreSQL connection)
2. ✅ appsettings.Development.json
3. ✅ .env.example (updated to Supabase)
4. ✅ docker-compose.yml (PostgreSQL removed, Supabase only)
5. [ ] Production deployment guide
6. [ ] Security hardening checklist
7. [ ] Runbook for common operations

### Documentation
1. ✅ Code comments and XML documentation
2. [ ] API documentation (OpenAPI/Swagger)
3. [ ] Database schema documentation
4. [ ] Architecture decision records (ADRs)
5. [ ] Troubleshooting guide

---

## 13. Known Issues & Risks

### Issues
1. **docker-compose.yml**: SQL Server still referenced (FIXED ✅)
2. **Realtime**: Client-side integration incomplete (frontend responsibility)
3. **RLS Policies**: Not yet enabled on Supabase

### Risks
| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|-----------|
| Data loss during migration | Low | Critical | Backup Supabase before major changes |
| Auth token expiration not handled | Medium | High | Implement refresh token rotation |
| Storage quota exceeded | Low | High | Monitor storage usage, implement cleanup |
| Realtime connection lost | Medium | Medium | Implement reconnection logic with exponential backoff |
| CORS misconfiguration | Low | High | Comprehensive CORS testing |

---

## 14. Timeline & Phases

### Phase 1: Cleanup & Verification (Current)
- [x] Remove SQL Server from docker-compose.yml
- [ ] Verify all environment configurations
- [ ] Run full test suite
- [ ] Security audit

### Phase 2: Production Hardening
- [ ] Implement Row-Level Security policies
- [ ] Configure storage bucket access policies
- [ ] Add rate limiting
- [ ] Enhance monitoring & logging

### Phase 3: Testing & Validation
- [ ] Full end-to-end testing
- [ ] Performance load testing
- [ ] Security penetration testing
- [ ] Staging environment validation

### Phase 4: Deployment
- [ ] CI/CD pipeline verification
- [ ] Production deployment
- [ ] Health check monitoring
- [ ] Rollback procedures ready

### Phase 5: Optimization
- [ ] Performance tuning
- [ ] Cost optimization
- [ ] Documentation finalization
- [ ] Knowledge transfer

---

## 15. References & Resources

- [Supabase Documentation](https://supabase.com/docs)
- [Supabase Auth API](https://supabase.com/docs/guides/auth)
- [Supabase Storage](https://supabase.com/docs/guides/storage)
- [Supabase Realtime](https://supabase.com/docs/guides/realtime)
- [Entity Framework Core PostgreSQL](https://www.npgsql.org/efcore/)
- [ASP.NET Core Security](https://learn.microsoft.com/en-us/aspnet/core/security/)
- [PostgreSQL Best Practices](https://wiki.postgresql.org/wiki/Performance_Optimization)

---

## Sign-Off

**Prepared By**: Migration Team
**Status**: Ready for Implementation
**Last Updated**: June 28, 2026
**Next Review**: Upon Phase 1 Completion
