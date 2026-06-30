/// <reference types="cypress" />
import './commands';

// Clear localStorage before each test to prevent session bleed-through.
// Individual tests that need auth call cy.visitAuthenticated() which sets
// nn_session via onBeforeLoad — AFTER this clear.
beforeEach(() => {
  cy.clearLocalStorage();
});

export {};
