/**
 * db-isolation.ts
 *
 * Dual-mode API helper for Naar & Noor E2E tests.
 *
 * MODE A — DATABASE_URL is set in Replit Secrets
 *   seedReferenceData() seeds the live database from fixtures, then verifies
 *   every fixture item is present with cy.assertMenuItemSeeded().
 *   dbAvailable is set to true.
 *   interceptXxx() functions create PASSTHROUGH intercepts — they alias the
 *   request so cy.wait('@alias') still works, but the real API responds.
 *   Tests are fully reproducible against the live backend.
 *
 * MODE B — DATABASE_URL is not set
 *   cy.task() returns { skipped: true }.
 *   dbAvailable is set to false.
 *   interceptXxx() functions inject a fixture body instead.
 *   cy.assertMenuItemSeeded() logs a skip — no assertion is made.
 *
 * ALWAYS STUBBED (never hits real external services):
 *   interceptPayment() — always returns fixture/payment-session.json
 *   interceptAuth()    — always returns fixture/auth-login.json or 401
 *
 * NOTE: Angular calls http://localhost:8080/api/... (cross-origin from the
 * Cypress baseUrl of localhost:5000), so all intercepts use absolute API URLs.
 *
 * NOTE: Cypress.env() is intentionally NOT used here because cypress.config.ts
 * sets allowCypressEnv: false (Cypress 15 security hardening).
 * DB availability is tracked via the module-level `dbAvailable` variable instead.
 */

const API = 'http://localhost:5000';

/**
 * Module-level flag: true when DATABASE_URL is configured and seeding succeeded.
 * Replaces Cypress.env('DB_AVAILABLE') — safe because allowCypressEnv is false.
 */
let dbAvailable = false;

// Names of every item in cypress/fixtures/menu.json.
// Used after seeding to give DB-level proof that each row was written.
const FIXTURE_MENU_NAMES = [
  'Lamb Rogan Josh',
  'Chicken Momos',
  'Dal Bhat',
  'Mango Lassi',
  'Gulab Jamun',
  'Sekuwa',
] as const;

// ─── Reference data seeding ──────────────────────────────────────────────────

/**
 * Call once in the global before() hook (support/e2e.ts).
 *
 * 1. Seeds menu items and chefs from fixtures into the live DB.
 * 2. Records outcome in the module-level dbAvailable flag.
 * 3. When DB is available, runs cy.assertMenuItemSeeded() for every fixture
 *    item — giving DB-level proof that each row was written, on top of the
 *    UI-level assertions in the individual test files.
 */
export function seedReferenceData(): void {
  cy.fixture('menu.json').then((items: unknown[]) => {
    cy.task('db:seed:menu', items, { log: false }).then((result) => {
      const r = result as { skipped?: boolean; seeded?: number };
      dbAvailable = !r.skipped;

      if (r.skipped) {
        cy.log('ℹ️  DB seeding skipped — set DATABASE_URL to enable live-API mode');
      } else {
        cy.log(`✅ DB seeded (${r.seeded} menu items) — verifying each row…`);

        // DB-level assertion: every fixture item must now be in the table.
        FIXTURE_MENU_NAMES.forEach((name) => {
          cy.assertMenuItemSeeded(name);
        });
      }
    });
  });

  cy.fixture('chefs.json').then((chefs: unknown[]) => {
    cy.task('db:seed:chefs', chefs, { log: false }).then((result) => {
      const r = result as { skipped?: boolean; seeded?: number };
      if (!r.skipped) cy.log(`✅ DB seeded (${r.seeded} chefs)`);
    });
  });
}

// ─── Conditional intercepts ───────────────────────────────────────────────────

/**
 * Alias `GET /api/menu*`.
 * Passes through to real API when DB is available; uses fixture stub otherwise.
 */
export function interceptMenu(alias = 'getMenu'): void {
  if (dbAvailable) {
    cy.intercept('GET', `${API}/api/menu*`).as(alias);
  } else {
    cy.intercept('GET', `${API}/api/menu*`, { fixture: 'menu.json' }).as(alias);
  }
}

/**
 * Alias `GET /api/chefs*`.
 * Passes through to real API when DB is available; uses fixture stub otherwise.
 */
export function interceptChefs(alias = 'getChefs'): void {
  if (dbAvailable) {
    cy.intercept('GET', `${API}/api/chefs*`).as(alias);
  } else {
    cy.intercept('GET', `${API}/api/chefs*`, { fixture: 'chefs.json' }).as(alias);
  }
}

/**
 * Alias `GET /api/reviews*`.
 * Passes through to real API when DB is available; uses fixture stub otherwise.
 */
export function interceptReviews(alias = 'getReviews'): void {
  if (dbAvailable) {
    cy.intercept('GET', `${API}/api/reviews*`).as(alias);
  } else {
    cy.intercept('GET', `${API}/api/reviews*`, { fixture: 'reviews.json' }).as(alias);
  }
}

/**
 * Alias `POST /api/reservations*`.
 * Passes through to real API when DB is available; stubs 201 otherwise.
 */
export function interceptReservationCreate(alias = 'createReservation'): void {
  if (dbAvailable) {
    cy.intercept('POST', `${API}/api/reservations*`).as(alias);
  } else {
    cy.intercept('POST', `${API}/api/reservations*`, {
      statusCode: 201,
      fixture: 'reservation.json',
    }).as(alias);
  }
}

/**
 * Always stubs `POST /api/payments/create-checkout-session`.
 * We never hit real Stripe in E2E tests.
 * The fixture returns { url: '/payment-success?session_id=...' } so the
 * Angular component redirects the browser to /payment-success.
 */
export function interceptPayment(alias = 'createPayment'): void {
  cy.intercept('POST', `${API}/api/payments/create-checkout-session*`, {
    statusCode: 200,
    fixture: 'payment-session.json',
  }).as(alias);
}

/**
 * Always stubs `POST /api/auth/login`.
 * We never use real credentials in E2E tests.
 */
export function interceptAuth(type: 'success' | 'failure', alias = 'login'): void {
  if (type === 'success') {
    cy.intercept('POST', `${API}/api/auth/login*`, {
      statusCode: 200,
      fixture: 'auth-login.json',
    }).as(alias);
  } else {
    cy.intercept('POST', `${API}/api/auth/login*`, {
      statusCode: 401,
      body: { message: 'Invalid credentials' },
    }).as(alias);
  }
}

/**
 * Always stubs `POST /api/auth/logout`.
 */
export function interceptLogout(alias = 'logout'): void {
  cy.intercept('POST', `${API}/api/auth/logout*`, {
    statusCode: 200,
    body: {},
  }).as(alias);
}
