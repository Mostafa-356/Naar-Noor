/// <reference types="cypress" />
import { LoginPage } from '../support/page-objects/LoginPage';

/**
 * Authentication E2E Tests
 *
 * API stubs:
 *   POST /api/auth/login  → 200 with fixtures/auth-login.json  (happy path)
 *                        → 401                                 (invalid-credentials path)
 *   POST /api/auth/logout → 200
 *
 * Logged-in state is detected via [data-user-menu] in the header
 * (added when auth.service.ts sets isLoggedIn = true).
 */

const VALID_EMAIL    = 'demo@example.com';
const VALID_PASSWORD = 'password123';

describe('Authentication E2E Tests', () => {
  describe('Login Workflow', () => {
    beforeEach(() => {
      cy.intercept('POST', '/api/auth/login*', {
        statusCode: 200,
        fixture: 'auth-login.json',
      }).as('login');
      LoginPage.visit();
    });

    it('should display the login page', () => {
      cy.title().should('contain', 'Login');
    });

    it('should show email and password fields', () => {
      LoginPage.getEmailInput().should('exist');
      LoginPage.getPasswordInput().should('exist');
    });

    it('should show the submit button', () => {
      LoginPage.getLoginButton().should('exist');
    });

    it('should login successfully with valid credentials', () => {
      LoginPage.login(VALID_EMAIL, VALID_PASSWORD);
      cy.wait('@login');
      LoginPage.verifyLoggedIn();
    });

    it('should reject invalid credentials', () => {
      cy.intercept('POST', '/api/auth/login*', {
        statusCode: 401,
        body: { message: 'Invalid credentials' },
      }).as('loginFail');
      LoginPage.login('wrong@example.com', 'wrongpassword');
      cy.wait('@loginFail');
      LoginPage.verifyErrorMessage('Invalid credentials');
    });

    it('should show error when email field is empty', () => {
      LoginPage.enterPassword(VALID_PASSWORD);
      LoginPage.getLoginButton().click();
      cy.contains('required', { matchCase: false }).should('be.visible');
    });

    it('should show error when password field is empty', () => {
      LoginPage.enterEmail(VALID_EMAIL);
      LoginPage.getLoginButton().click();
      cy.contains('required', { matchCase: false }).should('be.visible');
    });

    it('should show email-format error for invalid email', () => {
      LoginPage.getEmailInput().type('not-an-email').blur();
      cy.contains('email', { matchCase: false }).should('be.visible');
    });
  });

  describe('Logout Workflow', () => {
    beforeEach(() => {
      cy.intercept('POST', '/api/auth/login*', { statusCode: 200, fixture: 'auth-login.json' }).as('login');
      cy.intercept('POST', '/api/auth/logout*', { statusCode: 200, body: {} }).as('logout');
      LoginPage.visit();
      LoginPage.login(VALID_EMAIL, VALID_PASSWORD);
      cy.wait('@login');
      LoginPage.verifyLoggedIn();
    });

    it('should logout successfully', () => {
      LoginPage.clickLogout();
      LoginPage.verifyLoggedOut();
    });

    it('should navigate away from the user-menu area after logout', () => {
      LoginPage.clickLogout();
      cy.get('[data-user-menu]').should('not.exist');
    });
  });

  describe('Session Management', () => {
    beforeEach(() => {
      cy.intercept('POST', '/api/auth/login*', { statusCode: 200, fixture: 'auth-login.json' }).as('login');
    });

    it('should persist session on page refresh', () => {
      LoginPage.visit();
      LoginPage.login(VALID_EMAIL, VALID_PASSWORD);
      cy.wait('@login');
      LoginPage.verifyLoggedIn();
      cy.reload();
      LoginPage.verifyLoggedIn();
    });

    it('should persist session across navigation', () => {
      LoginPage.visit();
      LoginPage.login(VALID_EMAIL, VALID_PASSWORD);
      cy.wait('@login');
      cy.intercept('GET', '/api/menu*', { fixture: 'menu.json' });
      cy.visit('/menu');
      LoginPage.verifyLoggedIn();
    });

    it('should clear session data after logout', () => {
      cy.intercept('POST', '/api/auth/logout*', { statusCode: 200, body: {} }).as('logout');
      LoginPage.visit();
      LoginPage.login(VALID_EMAIL, VALID_PASSWORD);
      cy.wait('@login');
      LoginPage.clickLogout();
      cy.reload();
      LoginPage.verifyLoggedOut();
    });
  });
});
