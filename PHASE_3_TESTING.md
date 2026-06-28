# Phase 3: Testing & Validation

**Status**: ✅ READY FOR EXECUTION  
**Date**: June 28, 2026  
**Duration**: 1-2 hours (all tests)  

---

## Phase 3 Overview

Phase 3 validates that Phase 2 production hardening is working correctly:
- Integration tests verify database with RLS
- Load testing validates performance
- End-to-end tests verify user workflows
- Staging validation ensures production readiness

---

## Task 3.1: Integration Tests with PostgreSQL/Supabase

**Status**: ⏳ READY TO EXECUTE

### Backend Integration Tests

**Location**: `api-server/tests/NaarNoor.Infrastructure.Tests/`

**Run Tests**:
```bash
cd api-server

# Run all integration tests
dotnet test --filter "Category=Integration" --configuration Release

# Or run specific category
dotnet test --filter "Category=Integration&Namespace~Persistence" --configuration Release

# With detailed output
dotnet test --filter "Category=Integration" --configuration Release --logger "console;verbosity=detailed"
```

**What's Tested**:
- ✅ User registration and email validation
- ✅ User login and token generation
- ✅ Order creation with multiple items (transaction)
- ✅ Concurrent reservation bookings (conflict detection)
- ✅ Chef profile CRUD operations
- ✅ Menu item filters (vegetarian, vegan, gluten-free)
- ✅ Review submission and approval workflow
- ✅ Contact inquiry creation
- ✅ Authentication token expiration
- ✅ Data consistency after failed operations

**Expected Result**:
```
Test Run Successful.
Total tests: XX
Passed: XX
Failed: 0
```

**Verification**: 
- All tests should PASS ✅
- No timeouts
- No database connection errors

---

## Task 3.2: Load Testing with k6

**Status**: ⏳ READY TO EXECUTE

### Install k6

**Windows** (via Scoop):
```bash
scoop install k6
```

Or download: https://k6.io/docs/getting-started/installation/

**Verify Installation**:
```bash
k6 --version
# Expected: k6 v0.X.X
```

### Run Load Test Script

**Location**: `scripts/load-test.js`

**Run Test**:
```bash
k6 run scripts/load-test.js
```

**What's Tested**:
- 100 concurrent virtual users
- 10 minute test duration
- 3 stages: ramp-up (2m) → sustained (5m) → ramp-down (2m)

**Test Scenarios**:
1. User registration (10 req/sec)
2. User login (10 req/sec)
3. Browse menu items (20 req/sec)
4. Create order (5 req/sec)
5. Get orders (10 req/sec)

**Expected Results**:

| Metric | Target | Status |
|--------|--------|--------|
| p95 Response Time | < 500ms | ⏳ Test |
| p99 Response Time | < 1000ms | ⏳ Test |
| Error Rate | < 0.1% | ⏳ Test |
| Throughput | > 100 req/sec | ⏳ Test |

**Success Criteria**:
```
✓ http_req_duration: p(95)<500 ...................... OK
✓ http_req_duration: p(99)<1000 .................... OK
✓ http_req_failed: rate<0.1 ....................... OK
```

**If Load Test Fails**:
1. Check backend is responding
2. Verify database connections
3. Check rate limiting isn't triggering
4. Review RunASP logs for errors

---

## Task 3.3: End-to-End Testing (Cypress)

**Status**: ⏳ READY TO EXECUTE

### Cypress Test Files

Located in: `naar-noor/cypress/e2e/`

**Test Files**:
- `auth.cy.ts` - Registration, login, logout
- `menu-search.cy.ts` - Browse and filter menu
- `orders.cy.ts` - Create and track orders
- `reservation-workflow.cy.ts` - Make reservations
- `reviews.cy.ts` - Submit and view reviews
- `navigation.cy.ts` - Navigation and routing

### Run E2E Tests

**Headless Mode** (CI/CD):
```bash
cd naar-noor

# Run all E2E tests
npm run cypress:run

# Run specific test
npm run cypress:run -- --spec "cypress/e2e/auth.cy.ts"

# With detailed reporter
npm run cypress:run -- --reporter mochawesome
```

**Interactive Mode** (Development):
```bash
cd naar-noor

# Opens Cypress UI
npm run cypress:open

# Then select test files to run
```

**What's Tested**:
- ✅ User registration with validation
- ✅ Email verification (if required)
- ✅ Login and session management
- ✅ Menu browsing with filters
- ✅ Order placement and tracking
- ✅ Reservation booking
- ✅ Review submission
- ✅ Real-time updates
- ✅ Error handling
- ✅ Navigation between pages

**Expected Result**:
```
✓ All tests passing
✓ Video recordings created
✓ No console errors
✓ Performance metrics acceptable
```

**If Tests Fail**:
1. Check backend is running
2. Verify frontend environment variables
3. Check CORS headers
4. Review browser console for errors

---

## Task 3.4: Staging Environment Validation

**Status**: ⏳ READY TO EXECUTE

### Health Check

**Verify All Services**:
```bash
# Backend health
curl -i https://naar-noor.runasp.net/health

# Frontend loads
curl -i https://naar-noor.vercel.app

# Database connectivity
curl -i https://naar-noor.runasp.net/api/menu-items

# RLS working
curl -i https://naar-noor.runasp.net/api/orders
```

**Expected**:
- All return 200 OK
- No 500 errors
- No timeout errors

### Smoke Tests

**Quick Validation**:
1. Register new user
2. Login with credentials
3. Browse menu items
4. Create order
5. Check reservation page
6. Submit review

### Performance Validation

**Measure Response Times**:
```bash
# Measure p95 response time
curl -w "@curl-format.txt" -o /dev/null -s https://naar-noor.runasp.net/api/menu-items
```

**Expected**:
- p50: < 100ms
- p95: < 500ms
- p99: < 1000ms

### Rate Limiting Validation

**Verify Rate Limiting Active**:
```bash
# Send 6 requests
for i in {1..6}; do
  curl -X POST https://naar-noor.runasp.net/api/auth/register \
    -H "Content-Type: application/json" \
    -d "{\"email\":\"test$i@example.com\",\"password\":\"Test123!\"}" \
    -w "Status: %{http_code}\n"
done
```

**Expected**: Status 429 on 6th request

### Data Isolation Validation

**Verify RLS Working**:
1. Register User A
2. Create Order from User A
3. Login as User B
4. Verify User B cannot see User A's orders
5. Verify User B can only see own orders

### CORS Validation

**Verify CORS Headers**:
```bash
curl -i -H "Origin: https://naar-noor.vercel.app" \
  https://naar-noor.runasp.net/api/menu-items
```

**Expected Headers**:
- Access-Control-Allow-Origin: https://naar-noor.vercel.app
- Access-Control-Allow-Methods: GET, POST, PUT, DELETE
- Access-Control-Allow-Headers: Content-Type, Authorization

### Logging Validation

**Check JSON Logs**:
- Verify logs in Compact JSON format
- Check for @t, @m, @l fields
- Ensure no sensitive data logged

---

## Execution Order

**Recommended Sequence**:

1. **Integration Tests** (10-15 min)
   ```bash
   cd api-server
   dotnet test --filter "Category=Integration" --configuration Release
   ```

2. **Health Checks** (5 min)
   - Verify all endpoints responding
   - Check database connectivity

3. **E2E Tests** (15-20 min)
   ```bash
   cd naar-noor
   npm run cypress:run
   ```

4. **Load Testing** (15 min)
   ```bash
   k6 run scripts/load-test.js
   ```

5. **Staging Validation** (10 min)
   - Manual smoke tests
   - RLS verification
   - Performance measurement

**Total Time**: ~60-75 minutes

---

## Success Criteria (Sign-Off)

Phase 3 complete when:

- [ ] Integration tests: All PASS
- [ ] E2E tests: All PASS
- [ ] Load test: p95 < 500ms, error rate < 0.1%
- [ ] Health checks: All 200 OK
- [ ] Rate limiting: Returns 429 on limit
- [ ] CORS headers: Present and correct
- [ ] RLS working: Data isolation verified
- [ ] Logs: JSON format verified
- [ ] No error logs: All clean

---

## Troubleshooting

### Integration Tests Failing

**Check**:
1. Database tables created (7 tables)
2. RLS policies applied (13 policies)
3. Connection string correct
4. Supabase service running

**Run**:
```bash
# Check connection
dotnet test --filter "Category=Integration" --configuration Release --logger "console;verbosity=detailed"
```

### E2E Tests Failing

**Check**:
1. Backend running (health check passes)
2. Frontend environment variables set
3. CORS headers present
4. Browser console for errors

**Retry**:
```bash
npm run cypress:run -- --spec "cypress/e2e/auth.cy.ts"
```

### Load Test Errors

**Check**:
1. Backend responding
2. No rate limiting triggering
3. Database not overloaded

**Reduce Load**:
Edit `scripts/load-test.js` and reduce user count

### Performance Below Targets

**Optimize**:
1. Check database query performance
2. Add indexes if needed
3. Review slow queries
4. Check memory usage

---

## Next: Phase 4 (Production Deployment)

After Phase 3 validation complete and all tests pass:

**Phase 4 Tasks**:
1. Deploy backend to RunASP (already done)
2. Deploy frontend to Vercel
3. Post-deployment validation
4. Monitoring setup
5. Alert configuration

---

## Documentation

- Integration test results: `api-server/tests/TestResults/`
- Load test report: k6 console output
- Cypress videos: `naar-noor/cypress/videos/`
- Cypress screenshots: `naar-noor/cypress/screenshots/`

---

## Time Estimate

| Task | Time | Status |
|------|------|--------|
| 3.1 Integration Tests | 15 min | ⏳ Ready |
| 3.2 Load Testing | 15 min | ⏳ Ready |
| 3.3 E2E Tests | 20 min | ⏳ Ready |
| 3.4 Staging Validation | 10 min | ⏳ Ready |
| **Total** | **~60 min** | ⏳ Ready |

---

**Status**: ✅ Phase 3 READY TO EXECUTE

**Next Action**: Start Task 3.1 (Integration Tests)

```bash
cd api-server
dotnet test --filter "Category=Integration" --configuration Release
```
