# NaarNoor Testing Framework - Remediation Action Plan

**Date:** June 27, 2026  
**Based on:** Comprehensive Audit Report  
**Status:** Ready for Execution  
**Total Effort:** 40-54 hours (1-2 developer sprints)

---

## EXECUTIVE ACTION ITEMS

### 🔴 CRITICAL - Must Complete Before Production (Weeks 1-2)

#### Task 1: Configure Coverage Measurement (Coverlet)
- **Effort:** 4-6 hours
- **Owner:** [Assign]
- **Deadline:** End of Week 1
- **Deliverable:** `coverlet.runsettings` in repo root

**Steps:**
1. Create `coverlet.runsettings` with layer thresholds
2. Configure Cobertura format output
3. Set thresholds: Domain 85%, App 82%, Infra 78%, API 80%
4. Test coverage collection with `dotnet test`
5. Verify HTML report generation

**Files to Create:**
- `coverlet.runsettings` (25 lines)
- `scripts/generate-coverage-report.sh` (15 lines)

---

#### Task 2: Create GitHub Actions Workflow
- **Effort:** 6-8 hours
- **Owner:** [Assign]
- **Deadline:** End of Week 1
- **Deliverable:** `.github/workflows/tests.yml`

**Steps:**
1. Create `.github/workflows/tests.yml`
2. Configure backend test runs
3. Add coverage measurement
4. Add coverage gating (fail if below threshold)
5. Configure artifact collection
6. Test workflow on sample PR

**Files to Create:**
- `.github/workflows/tests.yml` (40 lines)
- `scripts/validate-coverage.sh` (25 lines)

---

#### Task 3: Create Pre-commit Hooks
- **Effort:** 2-3 hours
- **Owner:** [Assign]
- **Deadline:** End of Week 1
- **Deliverable:** `.husky/` directory configured

**Steps:**
1. Install husky: `npm install husky --save-dev`
2. Initialize husky: `npx husky install`
3. Create `.husky/pre-commit` hook
4. Configure to run tests before commit
5. Document setup in README

**Files to Create:**
- `.husky/pre-commit` (10 lines)
- `.husky/.gitignore` (1 line)
- `SETUP.md` section for hooks (15 lines)

---

#### Task 4: Extract Properties 7-9 into Dedicated Test Files
- **Effort:** 6 hours
- **Owner:** [Assign]
- **Deadline:** Mid Week 2
- **Deliverable:** 3 new test files

**Property 7: Repository CRUD Round-Trip**
- File: `api-server/tests/NaarNoor.Infrastructure.Tests/Repositories/RepositoryCrudPropertyTests.cs`
- Tests: Create, Read, Update, Delete operations
- Count: 6 property test methods

**Property 8: Query Pagination Correctness**
- File: `api-server/tests/NaarNoor.Application.Tests/Common/QueryPaginationPropertyTests.cs`
- Tests: Offset calculations, page bounds, total count
- Count: 6 property test methods

**Property 9: Transaction Atomicity**
- File: `api-server/tests/NaarNoor.Infrastructure.Tests/Persistence/TransactionAtomicityPropertyTests.cs`
- Tests: Rollback on error, all-or-nothing behavior
- Count: 6 property test methods

---

### 🟡 HIGH - Frontend Testing Foundation (Weeks 3-4)

#### Task 5: Configure Karma & Jasmine Setup
- **Effort:** 6-8 hours
- **Owner:** [Assign]
- **Deadline:** End of Week 3

**Steps:**
1. Install Karma: `npm install karma --save-dev`
2. Install Jasmine: `npm install jasmine --save-dev`
3. Install Istanbul coverage: `npm install istanbul karma-istanbul-reporter --save-dev`
4. Create `karma.conf.js`
5. Create sample Jasmine test
6. Verify coverage collection works

**Files to Create:**
- `karma.conf.js` (50 lines)
- `src/app/services/tests/sample.spec.ts` (template, 30 lines)

---

#### Task 6: Create Angular Service Tests (Properties 12-14)
- **Effort:** 8-10 hours
- **Owner:** [Assign]
- **Deadline:** End of Week 3

**Property 12: HTTP Communication Reliability**
- File: `src/app/services/tests/reservation.service.spec.ts`
- Tests: 5-6 test methods using HttpClientTestingModule

**Property 13: Service Error Handling**
- File: `src/app/services/tests/error-handling.spec.ts`
- Tests: 5-6 test methods for error scenarios

**Property 14: Response Caching Consistency**
- File: `src/app/services/tests/caching.spec.ts`
- Tests: 5-6 test methods for cache behavior

**Total:** 15-18 service test methods

---

#### Task 7: Create Angular Component Tests (Property 15)
- **Effort:** 8 hours
- **Owner:** [Assign]
- **Deadline:** Mid Week 4

**Property 15: Component State Management**

Files to Create:
- `src/app/components/reservation-form/reservation-form.spec.ts`
- `src/app/components/menu-list/menu-list.spec.ts`
- `src/app/components/order-display/order-display.spec.ts`
- Base class: `src/app/components/tests/component-test.base.ts`

**Tests per component:** 3-4 test methods
**Total:** 10-12 component test methods

---

#### Task 8: Create Cypress E2E Tests
- **Effort:** 8 hours
- **Owner:** [Assign]
- **Deadline:** End of Week 4

**E2E Coverage:**
- Login workflow
- Reservation creation flow
- Order placement flow
- Menu search and filtering
- Review submission

**Files to Create:**
- `cypress/e2e/auth.cy.ts` (30 lines)
- `cypress/e2e/reservations.cy.ts` (40 lines)
- `cypress/e2e/orders.cy.ts` (40 lines)
- `cypress/support/page-objects/` (4 files, 15 lines each)

**Total:** 10 E2E test scenarios

---

### 🟢 MEDIUM - Backend Property Tests (Week 5)

#### Task 9: Create Backend Security Properties (11, 16-20)
- **Effort:** 6-8 hours
- **Owner:** [Assign]
- **Deadline:** End of Week 5

**Property 11: API Exception Mapping**
- File: `api-server/tests/NaarNoor.API.Tests/ExceptionMappingPropertyTests.cs`
- Tests: Exception to HTTP status mapping

**Property 16: Input Validation Completeness**
- File: `api-server/tests/NaarNoor.API.Tests/InputValidationPropertyTests.cs`
- Tests: All controller inputs validated

**Property 17: Authorization Enforcement**
- File: `api-server/tests/NaarNoor.API.Tests/AuthorizationPropertyTests.cs`
- Tests: Policy enforcement

**Property 19: Injection Attack Prevention**
- File: `api-server/tests/NaarNoor.API.Tests/InjectionAttackPropertyTests.cs`
- Tests: SQL injection, XSS prevention

**Property 20: Sensitive Data Protection**
- File: `api-server/tests/NaarNoor.API.Tests/SensitiveDataPropertyTests.cs`
- Tests: No passwords in responses, secure headers

---

#### Task 10: Create Performance SLA Tests (Properties 21-22)
- **Effort:** 2-3 hours
- **Owner:** [Assign]
- **Deadline:** Mid Week 5

**Property 21: Query Performance SLA**
- File: `api-server/tests/NaarNoor.Application.Tests/PerformancePropertyTests.cs`
- Tests: Query execution time < 100ms

**Property 22: API Endpoint Response Time**
- File: `api-server/tests/NaarNoor.API.Tests/EndpointPerformancePropertyTests.cs`
- Tests: API response time < 500ms

---

### 🟢 LOW - Documentation (Week 6)

#### Task 11: Create Testing Documentation
- **Effort:** 8-10 hours
- **Owner:** [Assign]
- **Deadline:** End of Week 6

**Files to Create:**

1. `/docs/TESTING.md` (60 lines)
   - Quick start guide
   - Running tests locally
   - Interpreting test output

2. `/docs/TESTING_PROPERTIES.md` (80 lines)
   - Property-based testing concepts
   - How to write property tests
   - FsCheck generators
   - Common patterns

3. `/docs/TESTING_MOCKING.md` (70 lines)
   - Mock factories
   - Builder patterns
   - Creating test fixtures
   - Database seeding

4. `/docs/TESTING_COVERAGE.md` (50 lines)
   - Coverage interpretation
   - Coverage gates
   - Layer-specific goals
   - Improving coverage

5. `/docs/TESTING_TROUBLESHOOTING.md` (60 lines)
   - Common test failures
   - Debugging strategies
   - Async test issues
   - Mock configuration issues

**Total:** ~320 lines of documentation

---

## IMPLEMENTATION SEQUENCE

### Week 1: CI/CD Foundation
```
Monday-Wednesday: Coverage + CI/CD Workflow (10 hours)
  - Create coverlet.runsettings
  - Create GitHub Actions workflow
  - Set up coverage gating

Thursday-Friday: Testing Infrastructure (4 hours)
  - Create pre-commit hooks
  - Document setup process
  - Test end-to-end

Deliverable: Working CI/CD that blocks PRs with low coverage
```

### Week 2: Property Test Cleanup
```
Monday-Wednesday: Extract Properties 7-9 (6 hours)
  - Create dedicated test files
  - Move test logic
  - Update test data generators

Thursday-Friday: Validation (2 hours)
  - Run full test suite
  - Verify coverage thresholds
  - Update documentation

Deliverable: Clean property test organization matching spec
```

### Weeks 3-4: Frontend Framework
```
Week 3:
  Monday: Karma/Jasmine setup (4 hours)
  Tuesday-Thursday: Service tests (10 hours)
  Friday: Component base class (2 hours)

Week 4:
  Monday-Wednesday: Component tests (8 hours)
  Thursday-Friday: Cypress E2E (8 hours)

Deliverable: Full frontend test stack integrated with CI/CD
```

### Week 5: Backend Properties
```
Monday-Wednesday: Security properties (8 hours)
Thursday-Friday: Performance SLA tests (3 hours)

Deliverable: All 22 properties implemented
```

### Week 6: Documentation
```
Monday-Friday: Create 5 documentation files (8-10 hours)
  - 320 lines total
  - Covers all layers
  - Includes examples

Deliverable: Team can write and maintain tests independently
```

---

## SUCCESS CRITERIA

### Week 1 Checkpoint
- ✅ Coverage measurement working on local machines
- ✅ GitHub Actions workflow runs on PR
- ✅ Pre-commit hooks prevent commits with failing tests
- ✅ Properties 7-9 in dedicated files

### Week 2 Checkpoint
- ✅ All backend property tests passing
- ✅ Coverage gating enforced (failing PRs with <80% coverage)
- ✅ Test suite runs in <5 minutes locally
- ✅ CI/CD runs in <10 minutes on GitHub

### Week 4 Checkpoint
- ✅ Karma/Jasmine configured
- ✅ 15+ service tests passing
- ✅ 10+ component tests passing
- ✅ 10 E2E tests passing
- ✅ Frontend coverage in CI/CD

### Week 6 Checkpoint
- ✅ All 22 properties implemented
- ✅ Documentation complete
- ✅ New team members can write tests
- ✅ CI/CD blocking all inadequate submissions

### Production Ready Criteria
- ✅ 100% of property tests passing
- ✅ All layers meeting coverage thresholds
- ✅ CI/CD enforcing all gates
- ✅ Pre-commit hooks active
- ✅ Team documentation complete
- ✅ Zero known test infrastructure gaps

---

## RESOURCE REQUIREMENTS

### Team Size: 1-2 developers

**Full Effort:** 40-54 developer hours

**Timeline:** 6 weeks (1.5 sprints) with 1 developer, or 3-4 weeks with 2 developers

### Dependencies
- Access to GitHub Actions (already have)
- NuGet packages (Coverlet, xUnit - already have)
- npm packages (Karma, Jasmine, Istanbul - to be installed)
- Cypress for E2E testing (to be installed)

### Known Blockers
- None identified - all tooling is ready

---

## DECISION POINTS

**For User:**

1. **Critical Path Only or Full Scope?**
   - Option A: Critical path (CI/CD + Properties 7-9) = 12 weeks, 16-20 hours
   - Option B: Full scope (all items) = 6 weeks, 40-54 hours
   - **Recommendation:** Full scope for long-term maintainability

2. **Parallel Execution?**
   - Option A: Sequential (1 developer) = 6 weeks
   - Option B: Parallel (2 developers) = 3-4 weeks
   - **Recommendation:** 2 developers weeks 1-2 (CI/CD critical), then 1 developer weeks 3-6

3. **Documentation Scope?**
   - Option A: Minimal (inline comments only) = 0 hours, 2-3 hours
   - Option B: Full (5 comprehensive guides) = 8-10 hours
   - **Recommendation:** Full for team knowledge transfer

---

## NEXT STEPS

### Today (Before End of Day)
- [ ] Review this action plan
- [ ] Decide on execution scope
- [ ] Assign owners to Week 1 tasks
- [ ] Create GitHub issues for all tasks

### Tomorrow (Start Week 1)
- [ ] Create `coverlet.runsettings`
- [ ] Test coverage collection locally
- [ ] Begin GitHub Actions workflow

### End of Week 1
- [ ] Coverage gating working
- [ ] CI/CD passing all tests
- [ ] Pre-commit hooks active

---

## CONTACTS & ESCALATION

**If you have questions:**

1. **Coverage Configuration Issues** → Azure Pipelines / Coverlet docs
2. **GitHub Actions** → GitHub Actions documentation + lab
3. **Karma/Jasmine** → Angular testing guide + npm packages
4. **Cypress** → Cypress documentation

**If you hit blockers:**
- Escalate immediately with specific error messages
- Include: what you tried, what failed, expected vs. actual

