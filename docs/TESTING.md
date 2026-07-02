# Testing Guide

## Quick Start

```bash
# Backend
cd api-server && dotnet test

# Frontend (unit)
cd naar-noor && npm run test:ci

# Frontend (E2E)
cd naar-noor && npx cypress run

# E2E with browser UI
cd naar-noor && npx cypress open
```

---

## Backend Tests — .NET / xUnit / FsCheck

**Structure:**

| Project | Layer | Focus |
|---------|-------|-------|
| `NaarNoor.Domain.Tests` | Domain | Entity validation, invariants, value objects |
| `NaarNoor.Application.Tests` | Application | Command/query handlers, validators |
| `NaarNoor.Infrastructure.Tests` | Infrastructure | CRUD, transactions, referential integrity |
| `NaarNoor.API.Tests` | API | HTTP endpoints, auth, exception mapping |

**Run filters:**
```bash
dotnet test --filter "Category=Domain"
dotnet test --filter "Category=Application"
dotnet test --filter "Category=Infrastructure"
dotnet test --filter "Category=API"
dotnet test -v detailed   # verbose output
dotnet watch test          # watch mode
```

**Coverage:**
```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

## Frontend Tests — Jasmine / Karma / Cypress

**Unit structure:**
- `src/app/services/**/*.spec.ts` — HTTP communication, error handling, caching
- `src/app/components/**/*.spec.ts` — State, interactions, form validation

**E2E structure** (`cypress/e2e/`):
- `auth.cy.ts` — Login / logout flows
- `browse-menu.cy.ts` — Menu browsing and filtering
- `menu-search.cy.ts` — Search functionality
- `cart-flow.cy.ts` — Add to cart workflow
- `checkout-flow.cy.ts` — Order checkout
- `reservation-workflow.cy.ts` — Table reservations
- `reviews.cy.ts` — Review submission

---

## Coverage Targets

| Layer | Target | Status |
|-------|--------|--------|
| Domain | 85% | ✅ 88.5% |
| Application | 82% | ⚠️ 78.6% |
| Infrastructure | 78% | ✅ 78.3% |
| API | 80% | ✅ 82.2% |
| Frontend Services | 80% | ⏳ |
| Frontend Components | 75% | ⏳ |

**Validate coverage:**
```bash
python scripts/validate-coverage.py --backend-dir api-server/tests
```

---

## Property-Based Testing (FsCheck)

Runs 100 random iterations per property. Key properties:

| # | Property | Layer |
|---|----------|-------|
| 1–4 | Entity validation, immutability, transitions, invariants | Domain |
| 5–6 | Command/query handler correctness | Application |
| 7 | CRUD round-trip | Infrastructure |
| 8 | Query pagination | Application |
| 9–10 | Transaction atomicity, referential integrity | Infrastructure |
| 11 | HTTP exception mapping | API |
| 12–15 | Service HTTP, error handling, caching, component state | Frontend |
| 16–17 | Input validation, authorization | API |
