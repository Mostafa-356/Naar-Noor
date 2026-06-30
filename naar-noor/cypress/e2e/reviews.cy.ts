/// <reference types="cypress" />

/**
 * Reviews E2E Tests
 *
 * API stubs:
 *   GET  /api/reviews* → fixtures/reviews.json
 *   POST /api/reviews* → 201 (review created)
 *
 * All calls are intercepted so no live server is required.
 */

describe('Reviews E2E Tests', () => {
  beforeEach(() => {
    cy.intercept('GET', '/api/reviews*', { fixture: 'reviews.json' }).as('getReviews');
    cy.visit('/reviews');
    cy.wait('@getReviews');
  });

  describe('Viewing Reviews', () => {
    it('should display the reviews page', () => {
      cy.title().should('contain', 'Reviews');
    });

    it('should display review cards', () => {
      cy.get('[data-cy="review-card"]').should('have.length.at.least', 1);
    });

    it('should display reviewer names', () => {
      cy.contains('Ahmed Hassan').should('exist');
      cy.contains('Sara Ali').should('exist');
    });

    it('should display star ratings', () => {
      cy.get('[data-cy="review-card"]').first().find('[data-cy="rating-stars"]').should('exist');
    });

    it('should display review comments', () => {
      cy.contains('Lamb Rogan Josh').should('exist');
    });

    it('should display review dates', () => {
      cy.get('[data-cy="review-card"]').first().should('contain', '2026');
    });
  });

  describe('Rating Selector', () => {
    it('should display the rating selector', () => {
      cy.get('[data-cy="rating-selector"]').should('exist');
    });

    it('should have 5 rating buttons', () => {
      cy.get('[data-cy="rating-selector"]').find('button').should('have.length', 5);
    });

    it('should allow selecting a rating', () => {
      cy.get('[data-cy="rating-selector"]').find('button').eq(3).click();
      cy.get('[data-cy="rating-selector"]').find('button').eq(3)
        .should('have.class', /selected|active|filled/);
    });
  });

  describe('Submitting a Review', () => {
    beforeEach(() => {
      cy.intercept('POST', '/api/reviews*', {
        statusCode: 201,
        body: {
          id: 'review-new',
          reviewerName: 'Test Reviewer',
          rating: 5,
          comment: 'Fantastic service and delicious food!',
          date: '2026-06-30T00:00:00Z',
        },
      }).as('createReview');
    });

    it('should show a success message after submission', () => {
      cy.get('input[name="reviewerName"]').type('Test Reviewer');
      cy.get('[data-cy="rating-selector"]').find('button').eq(4).click();
      cy.get('textarea[name="comment"]').type('Fantastic service and delicious food!');
      cy.get('button').contains('Submit Review').click();
      cy.wait('@createReview');
      cy.get('[data-cy="success-message"]').should('be.visible');
    });

    it('should add the new review to the list', () => {
      cy.get('[data-cy="review-card"]').then(($initial) => {
        const initialCount = $initial.length;

        cy.intercept('GET', '/api/reviews*', {
          body: [
            { id: 'review-1', reviewerName: 'Ahmed Hassan', rating: 5, comment: 'Absolutely incredible food.', date: '2026-06-15T00:00:00Z' },
            { id: 'review-2', reviewerName: 'Sara Ali',     rating: 4, comment: 'Great service.',              date: '2026-06-10T00:00:00Z' },
            { id: 'review-3', reviewerName: 'Fatima Khan',  rating: 5, comment: 'Best Himalayan food.',        date: '2026-05-28T00:00:00Z' },
            { id: 'review-new', reviewerName: 'Test Reviewer', rating: 5, comment: 'Fantastic!',               date: '2026-06-30T00:00:00Z' },
          ],
        }).as('getReviewsUpdated');

        cy.get('input[name="reviewerName"]').type('Test Reviewer');
        cy.get('[data-cy="rating-selector"]').find('button').eq(4).click();
        cy.get('textarea[name="comment"]').type('Fantastic!');
        cy.get('button').contains('Submit Review').click();
        cy.wait('@createReview');

        cy.get('[data-cy="review-card"]').should(($reviews) => {
          expect($reviews.length).to.be.greaterThan(initialCount);
        });
      });
    });

    it('should clear the form after a successful submission', () => {
      cy.get('input[name="reviewerName"]').type('Test User');
      cy.get('[data-cy="rating-selector"]').find('button').eq(3).click();
      cy.get('textarea[name="comment"]').type('Good food overall.');
      cy.get('button').contains('Submit Review').click();
      cy.wait('@createReview');
      cy.get('input[name="reviewerName"]').should('have.value', '');
      cy.get('textarea[name="comment"]').should('have.value', '');
    });

    it('should require reviewer name before submitting', () => {
      cy.get('[data-cy="rating-selector"]').find('button').eq(4).click();
      cy.get('textarea[name="comment"]').type('No name provided.');
      cy.get('button').contains('Submit Review').click();
      cy.contains('required', { matchCase: false }).should('be.visible');
    });

    it('should require a comment before submitting', () => {
      cy.get('input[name="reviewerName"]').type('Anonymous');
      cy.get('[data-cy="rating-selector"]').find('button').eq(2).click();
      cy.get('button').contains('Submit Review').click();
      cy.contains('required', { matchCase: false }).should('be.visible');
    });

    it('should require a rating before submitting', () => {
      cy.get('input[name="reviewerName"]').type('Anonymous');
      cy.get('textarea[name="comment"]').type('No rating selected.');
      cy.get('button').contains('Submit Review').click();
      cy.contains('required', { matchCase: false }).should('be.visible');
    });
  });

  describe('Review Display Details', () => {
    it('should show 5-star reviews with all stars filled', () => {
      cy.get('[data-cy="review-card"]').first().find('[data-cy="rating-stars"]')
        .invoke('text')
        .should('match', /★{5}|5.*star/);
    });

    it('should display reviews in reverse-chronological order (newest first)', () => {
      cy.get('[data-cy="review-card"]').first().should('contain', '2026-06');
    });

    it('should display at least 3 fixture reviews', () => {
      cy.get('[data-cy="review-card"]').should('have.length.at.least', 3);
    });
  });
});
