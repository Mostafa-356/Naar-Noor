# Implementation Plan: Comprehensive Testing Framework for Naar & Noor

## Overview

Implementing a multi-layer testing framework across backend (.NET/C#) and frontend (Angular/TypeScript) systems, establishing consistent testing patterns, automated validation, and CI/CD integration to achieve >80% code coverage on core business logic layers.

## Tasks

- [x] 1. Backend Test Infrastructure Setup
  - [x] 1.1 Create test project structure and dependencies
    - Create NaarNoor.Domain.Tests, NaarNoor.Application.Tests, NaarNoor.Infrastructure.Tests, and NaarNoor.API.Tests projects
    - Add xUnit, FluentAssertions, and Moq NuGet packages to all test projects
    - Add Coverlet.collector for coverage measurement
    - Add Entity Framework testing packages (Microsoft.EntityFrameworkCore.InMemory)
    - Create directory structure mirroring source projects
    - _Requirements: 1.1, 1.2, 1.3, 9.1_

  - [x] 1.2 Write property test for test project structure validation
    - **Property 1: Entity State Validation**
    - **Validates: Requirements 1.1, 1.2, 1.4**

  - [x] 1.3 Create base test classes and common fixtures
    - Create ApplicationLayerTestBase with mock factory injection
    - Create DomainLayerTestBase for entity testing
    - Create RepositoryTestBase with database fixture management
    - Create WebApplicationFactoryFixture for API testing
    - Implement IAsyncLifetime for proper async setup/teardown
    - _Requirements: 10.5, 10.6, 11.1_

  - [x] 1.4 Write integration test for base class functionality
    - Test that base classes properly initialize mock dependencies
    - Test that fixtures handle async setup/teardown correctly
    - _Requirements: 10.5_

- [x] 2. Domain Layer Unit Tests
  - [x] 2.1 Create Chef entity tests
    - Write tests for Chef constructor validation (name, specialization required)
    - Write tests for property initialization and immutability
    - Write tests for business rule validation (experience >= 0, rating 0-5)
    - Write tests for domain event publishing
    - _Requirements: 1.1, 1.4, 1.6_

  - [x] 2.2 Write property test for Chef entity validation
    - **Property 1: Entity State Validation**
    - **Validates: Requirements 1.1, 1.4_**

  - [x] 2.3 Create Reservation entity tests
    - Write tests for Reservation constructor validation (chefId, date, partySize required)
    - Write tests for status transition state machine (Pending→Confirmed→Ready→Completed)
    - Write tests for invalid status transitions throwing DomainException
    - Write tests for party size constraints (> 0)
    - Write tests for date validation (future dates only)
    - _Requirements: 1.2, 1.3, 1.6_

  - [x] 2.4 Write property test for Reservation state transitions
    - **Property 3: Entity State Transitions**
    - **Validates: Requirements 1.3, 1.6_**

  - [x] 2.5 Create Order entity tests
    - Write tests for Order constructor with required fields
    - Write tests for order item collection management
    - Write tests for total price calculation
    - Write tests for order status transitions
    - Write tests for constraint validation (total price >= 0)
    - _Requirements: 1.2, 1.3, 1.4_

  - [x] 2.6 Write property test for Order domain invariants
    - **Property 4: Domain Invariants Preservation**
    - **Validates: Requirements 1.6_**

  - [x] 2.7 Create MenuItem entity tests
    - Write tests for MenuItem constructor validation
    - Write tests for price constraints (>= 0)
    - Write tests for category validation
    - Write tests for availability flag
    - Write tests for immutability of price after creation
    - _Requirements: 1.4, 1.5_

  - [x] 2.8 Create value object tests (Money, TimeSlot)
    - Write Money value object tests: immutability, equality, currency validation, amount constraints
    - Write TimeSlot value object tests: immutability, equality, time overlap detection, validation
    - Write tests verifying value objects implement IEquatable correctly
    - _Requirements: 1.5_

  - [x] 2.9 Write property test for value object immutability
    - **Property 2: Value Object Immutability and Equality**
    - **Validates: Requirements 1.5_**


- [x] 3. Checkpoint - Domain Layer Tests Pass
  - Ensure all domain layer tests pass, all properties validated, ask the user if questions arise.

- [x] 4. Application Layer Unit Tests
  - [x] 4.1 Create command handler test infrastructure
    - Create ChefCommandHandlerTestBase with mocked repositories
    - Create ReservationCommandHandlerTestBase with mocked dependencies
    - Create OrderCommandHandlerTestBase with mock setup
    - Create mock factories for all command types
    - Create builder classes for command objects
    - _Requirements: 2.1, 11.1, 11.2_

  - [x] 4.2 Write property test for command handler processing
    - **Property 5: Command Handler Processing**
    - **Validates: Requirements 2.1, 2.3_**

  - [x] 4.3 Implement Reservation command handler tests
    - Test CreateReservationCommand: valid data creates reservation and repository is called
    - Test UpdateReservationCommand: valid update modifies reservation correctly
    - Test CancelReservationCommand: cancellation updates status and calls repository
    - Test invalid command data throws ValidationException
    - Test missing chef reference throws NotFoundException
    - _Requirements: 2.1, 2.3_

  - [x] 4.4 Implement Order command handler tests
    - Test CreateOrderCommand: valid order is created with items
    - Test UpdateOrderCommand: order updates are persisted
    - Test UpdateOrderStatusCommand: status transitions are validated
    - Test invalid item pricing throws DomainException
    - Test quantity validation (> 0) throws exception
    - _Requirements: 2.1, 2.3_

  - [x] 4.5 Implement Menu command handler tests
    - Test CreateMenuItemCommand: new menu item is persisted
    - Test UpdateMenuItemCommand: item price and availability changes
    - Test DeleteMenuItemCommand: item is removed from repository
    - Test duplicate item name validation
    - Test category validation
    - _Requirements: 2.1, 2.3_

  - [x] 4.6 Create query handler test infrastructure
    - Create ChefQueryHandlerTestBase with seeded test data
    - Create ReservationQueryHandlerTestBase with filtering setup
    - Create OrderQueryHandlerTestBase with ordering configuration
    - Create query builder classes for filter parameters
    - _Requirements: 2.2, 10.6, 11.1_

  - [x] 4.7 Write property test for query result filtering
    - **Property 6: Query Result Filtering and Transformation**
    - **Validates: Requirements 2.2_**

  - [x] 4.8 Implement Reservation query handler tests
    - Test GetReservationByIdQuery: returns correct reservation
    - Test GetReservationsByChefsQuery: filters by chef correctly
    - Test GetReservationsByDateQuery: filters by date range
    - Test GetReservationsWithPaginationQuery: pagination works correctly
    - Test no database access occurs during query execution
    - _Requirements: 2.2_

  - [x] 4.9 Implement Order query handler tests
    - Test GetOrderByIdQuery: returns correct order with items
    - Test GetOrdersByReservationQuery: returns all orders for reservation
    - Test GetOrdersByStatusQuery: filters by status correctly
    - Test GetOrdersByDateRangeQuery: date range filtering
    - _Requirements: 2.2_

  - [x] 4.10 Write property test for query pagination correctness
    - **Property 8: Query Pagination Correctness**
    - **Validates: Requirements 5.2_**

  - [x] 4.11 Create validator tests
    - Test CreateReservationValidator: all validation rules enforced
    - Test CreateOrderValidator: item and quantity validation
    - Test UpdateMenuItemValidator: price and category constraints
    - Test error message consistency across validators
    - _Requirements: 2.1_


- [x] 5. Checkpoint - Application Layer Tests Pass
  - Ensure all application layer tests pass, handler and query logic validated, ask the user if questions arise.

- [x] 6. Infrastructure Layer Unit Tests
  - [x] 6.1 Create repository test infrastructure
    - Create RepositoryTestBase with in-memory DbContext factory
    - Create IAsyncLifetime fixture for database isolation
    - Create ApplicationDbContextFactory for test database creation
    - Create data seeding helpers for test data population
    - _Requirements: 3.1, 10.5, 10.6_

  - [x] 6.2 Write property test for CRUD round-trip operations
    - **Property 7: Repository CRUD Round-Trip**
    - **Validates: Requirements 3.1, 5.1, 5.3_**

  - [x] 6.3 Implement Chef repository tests
    - Test AddAsync: new chef is persisted to database
    - Test GetByIdAsync: returns correct chef by id
    - Test GetAllAsync: returns all chefs with correct count
    - Test UpdateAsync: chef updates are persisted
    - Test DeleteAsync: chef is removed from database
    - Test query methods: GetBySpecializationAsync, GetByRatingAsync
    - _Requirements: 3.1, 5.1, 5.3_

  - [x] 6.4 Implement Reservation repository tests
    - Test CRUD operations with relationship to Chef
    - Test GetByChefIdAsync: returns reservations for specific chef
    - Test GetByStatusAsync: filters by reservation status
    - Test GetByDateRangeAsync: date range queries work correctly
    - Test foreign key constraints enforced
    - Test cascading deletes handled properly
    - _Requirements: 3.1, 5.1, 5.3, 5.4_

  - [x] 6.5 Implement Order repository tests
    - Test CRUD operations with OrderItems collection
    - Test GetByReservationIdAsync: returns all orders for reservation
    - Test GetByStatusAsync: status filtering works
    - Test complex queries with joins and filters
    - Test collection initialization and lazy loading behavior
    - _Requirements: 3.1, 5.1, 5.3_

  - [x] 6.6 Implement Unit of Work tests
    - Test BeginTransactionAsync: transaction starts properly
    - Test CommitAsync: all repository changes are persisted atomically
    - Test RollbackAsync: changes are not persisted on rollback
    - Test nested transactions handled correctly
    - _Requirements: 5.5_

  - [x] 6.7 Write property test for transaction atomicity
    - **Property 9: Transaction Atomicity**
    - **Validates: Requirements 5.5_**

  - [x] 6.8 Create DbContext configuration tests
    - Test entity configurations: all entities properly configured
    - Test column mappings: properties map to correct database columns
    - Test constraints: unique constraints, required fields configured
    - Test relationships: one-to-many, many-to-one relationships configured
    - Test shadow properties and value conversions
    - _Requirements: 3.1, 5.4_

  - [x] 6.9 Write property test for referential integrity
    - **Property 10: Referential Integrity Enforcement**
    - **Validates: Requirements 5.4_**


- [x] 7. Checkpoint - Infrastructure Layer Tests Pass
  - Ensure all infrastructure tests pass, data persistence validated, ask the user if questions arise.

- [x] 8. API Layer Integration Tests
  - [x] 8.1 Create WebApplicationFactory test fixture
    - Create CustomWebApplicationFactory extending WebApplicationFactory
    - Configure in-memory database for tests
    - Override CreateClient to set base address
    - Create helper methods for common test operations
    - _Requirements: 4.7, 7.0_

  - [x] 8.2 Create API endpoint test base class
    - Create ApiTestBase with WebApplicationFactory injection
    - Create helper methods for common HTTP operations
    - Create assertion helpers for response validation
    - Create bearer token generation for auth tests
    - _Requirements: 4.1, 4.2, 4.3_

  - [x] 8.3 Implement Reservations endpoint integration tests
    - Test POST /api/reservations with valid data returns 201
    - Test GET /api/reservations returns list of reservations
    - Test GET /api/reservations/{id} returns specific reservation
    - Test PUT /api/reservations/{id} updates reservation
    - Test DELETE /api/reservations/{id} removes reservation
    - Test invalid data returns 400 with validation errors
    - Test missing required fields returns 400
    - Test unauthorized requests return 401
    - _Requirements: 4.1, 4.2, 4.3, 15.1, 15.3_

  - [x] 8.4 Write property test for input validation consistency
    - **Property 16: Input Validation Consistency**
    - **Validates: Requirements 15.1_**

  - [x] 8.5 Implement Orders endpoint integration tests
    - Test POST /api/orders creates order with items
    - Test GET /api/orders returns user's orders
    - Test GET /api/orders/{id} returns specific order
    - Test PUT /api/orders/{id} updates order
    - Test PATCH /api/orders/{id}/status updates status
    - Test invalid pricing data returns 400
    - Test negative quantities rejected
    - _Requirements: 4.1, 4.2, 4.3_

  - [x] 8.6 Implement Menu endpoint integration tests
    - Test GET /api/menu returns all menu items
    - Test GET /api/menu/{id} returns specific item
    - Test POST /api/menu creates menu item (admin only)
    - Test PUT /api/menu/{id} updates menu item
    - Test DELETE /api/menu/{id} removes menu item
    - Test category filtering
    - Test availability filtering
    - _Requirements: 4.1, 4.2, 4.3_

  - [x] 8.7 Implement Chefs endpoint integration tests
    - Test GET /api/chefs returns all chefs
    - Test GET /api/chefs/{id} returns chef details
    - Test specialization filtering
    - Test rating filtering and sorting
    - Test error responses for invalid chef ids
    - _Requirements: 4.1, 4.2_

  - [x] 8.8 Implement Contact endpoint integration tests
    - Test POST /api/contact submits inquiry
    - Test required fields validation
    - Test email format validation
    - Test message length constraints
    - _Requirements: 4.1, 4.2, 4.3_

  - [x] 8.9 Write property test for authorization enforcement
    - **Property 17: Authorization Enforcement**
    - **Validates: Requirements 15.3_**

  - [x] 8.10 Implement exception handling middleware tests
    - Test DomainException mapped to 400 with error code
    - Test ValidationException mapped to 400 with field errors
    - Test NotFoundException mapped to 404
    - Test UnauthorizedAccessException mapped to 401
    - Test generic exceptions mapped to 500
    - Test error response format consistency
    - _Requirements: 4.6_

  - [x] 8.11 Write property test for exception mapping
    - **Property 11: HTTP Exception Mapping**
    - **Validates: Requirements 4.6_**


- [x] 9. Checkpoint - API Integration Tests Pass
  - Ensure all API endpoint tests pass, HTTP handling validated, ask the user if questions arise.

- [x] 10. Middleware Testing
  - [x] 10.1 Create middleware test infrastructure
    - Create MiddlewareTestBase with HttpContext builder
    - Create request/response builders for middleware testing
    - Create assertion helpers for middleware behavior
    - _Requirements: 4.4, 4.5_

  - [x] 10.2 Implement Authorization middleware tests
    - Test requests without Authorization header return 401
    - Test requests with invalid tokens return 401
    - Test valid tokens allow request to proceed
    - Test token validation logic
    - _Requirements: 4.4, 15.3_

  - [x] 10.3 Implement CORS middleware tests
    - Test requests without Origin header processed normally
    - Test valid origins receive CORS headers
    - Test invalid origins don't receive CORS headers
    - Test preflight requests handled correctly
    - _Requirements: 4.5_

  - [x] 10.4 Implement SecurityHeaders middleware tests
    - Test response includes security headers (X-Frame-Options, X-Content-Type-Options, etc.)
    - Test header values are correct
    - Test HTTPS enforcement headers present
    - _Requirements: 4.5, 15.5_

  - [x] 10.5 Implement request/response logging middleware tests
    - Test sensitive data is not logged (passwords, tokens)
    - Test PII fields are masked or excluded
    - Test logging doesn't affect request processing
    - _Requirements: 15.4_

  - [x] 10.6 Write property test for sensitive data protection
    - **Property 18: Sensitive Data Protection**
    - **Validates: Requirements 15.4_**

- [x] 11. Frontend Test Infrastructure Setup
  - [x] 11.1 Configure Angular testing environment
    - Configure Karma test runner with ChromeHeadless
    - Configure Jasmine for BDD-style testing
    - Set up HttpClientTestingModule for HTTP mocking
    - Configure code coverage reporting
    - Create karma.conf.js with coverage thresholds
    - Create test setup files (test.ts, setup.spec.ts)
    - _Requirements: 6.7, 9.2, 9.4_

  - [x] 11.2 Create frontend test base classes and fixtures
    - Create base test class for services with common setup
    - Create base test class for components with TestBed configuration
    - Create mock factories for common services
    - Create test data builders for TypeScript models
    - _Requirements: 10.5, 10.6_

  - [x] 11.3 Configure code coverage for frontend
    - Set coverage thresholds: services 80%, components 75%
    - Configure coverage reporters (html, lcovonly, cobertura)
    - Create coverage exclusion patterns for generated code
    - Set up coverage gating in karma.conf.js
    - _Requirements: 9.3, 9.4_

  - [x] 11.4 Create HTTP testing utilities
    - Create HttpTestingHelper for common HTTP mock patterns
    - Create response builders for standard API responses
    - Create error response builders for error scenarios
    - Create helper methods for assertion patterns
    - _Requirements: 6.1, 6.2, 6.4_


- [x] 12. Frontend Service Unit Tests
  - [x] 12.1 Create ReservationService tests
    - Test getReservations() returns list from API
    - Test getReservationById(id) returns specific reservation
    - Test createReservation(data) posts to API and returns result
    - Test updateReservation(id, data) updates via API
    - Test deleteReservation(id) deletes via API
    - Test error handling for failed requests
    - Test caching behavior if implemented
    - _Requirements: 6.1, 6.2, 6.4, 6.5_

  - [x] 12.2 Write property test for HTTP communication
    - **Property 12: Service HTTP Communication**
    - **Validates: Requirements 6.1, 6.2_**

  - [x] 12.3 Create OrderService tests
    - Test getOrders() returns user's orders
    - Test createOrder(data) creates order with items
    - Test updateOrderStatus(id, status) updates status
    - Test calculateTotal(items) calculates price correctly
    - Test error handling for validation errors
    - Test HTTP error handling (4xx, 5xx)
    - _Requirements: 6.1, 6.2, 6.4_

  - [x] 12.4 Write property test for service error handling
    - **Property 13: Service Error Handling**
    - **Validates: Requirements 6.4_**

  - [x] 12.5 Create MenuService tests
    - Test getMenuItems() returns all items
    - Test getMenuItemById(id) returns specific item
    - Test filterMenuItems(criteria) filters correctly
    - Test searchMenuItems(query) searches by name/description
    - Test error handling for failed requests
    - Test caching of menu items
    - _Requirements: 6.1, 6.2, 6.5_

  - [x] 12.6 Write property test for caching behavior
    - **Property 14: Service Caching Behavior**
    - **Validates: Requirements 6.5_**

  - [x] 12.7 Create ChefService tests
    - Test getChefs() returns all chefs
    - Test getChefById(id) returns chef details
    - Test searchChefsBySpecialization(spec) filters correctly
    - Test getChefReservations(chefId) returns chef's reservations
    - Test error handling for not found cases
    - _Requirements: 6.1, 6.2_

  - [x] 12.8 Create authentication-related service tests
    - Test login service makes POST request with credentials
    - Test token is stored after successful login
    - Test logout clears stored token
    - Test getCurrentUser returns authenticated user
    - Test error handling for authentication failures
    - _Requirements: 6.1, 6.2, 6.4_

  - [x] 12.9 Create HTTP interceptor tests
    - Test AuthInterceptor adds Authorization header to requests
    - Test ErrorInterceptor catches and handles HTTP errors
    - Test LoggingInterceptor logs requests (without sensitive data)
    - Test interceptor chain preserves request integrity
    - _Requirements: 6.6, 15.4_

  - [x] 12.10 Write property test for HTTP interceptor processing
    - **Property 15: HTTP Interceptor Processing**
    - **Validates: Requirements 6.6_**


- [x] 13. Checkpoint - Frontend Service Tests Pass
  - Ensure all frontend service tests pass, HTTP communication validated, ask the user if questions arise.

- [x] 14. Frontend Component Unit Tests
  - [x] 14.1 Create ReservationFormComponent tests
    - Test form initialization with empty fields
    - Test form fields validation (required, format)
    - Test form binding to component property
    - Test submit button disabled until valid
    - Test onSubmit calls service with form data
    - Test error messages display on validation failure
    - Test success message displays on submission
    - _Requirements: 7.1, 7.2, 7.3_

  - [x] 14.2 Create OrderListComponent tests
    - Test component displays order list from service
    - Test order items display correctly
    - Test pagination works (if implemented)
    - Test sorting/filtering options work
    - Test loading state displays while fetching
    - Test error state displays on load failure
    - Test empty state displays when no orders
    - _Requirements: 7.1, 7.4, 7.5_

  - [x] 14.3 Create MenuDisplayComponent tests
    - Test menu items display from service
    - Test category filtering works
    - Test search/filter functionality
    - Test price display with formatting
    - Test availability indicators
    - Test add-to-cart button click handling
    - Test error handling for load failures
    - _Requirements: 7.1, 7.4, 7.5_

  - [x] 14.4 Create ChefListComponent tests
    - Test chef list displays from service
    - Test chef cards show name, specialization, rating
    - Test specialization filtering
    - Test rating sorting
    - Test click on chef navigates to detail
    - Test loading and error states
    - _Requirements: 7.1, 7.4, 7.5_

  - [x] 14.5 Create HeaderComponent tests
    - Test navigation links display
    - Test user profile menu displays when logged in
    - Test login/logout buttons display appropriately
    - Test search input functionality
    - Test responsive menu for mobile
    - _Requirements: 7.1, 7.5, 7.6_

  - [x] 14.6 Create error and loading components tests
    - Test LoadingComponent displays spinner
    - Test ErrorComponent displays error message
    - Test ErrorComponent retry button works
    - Test NotFoundComponent displays 404 message
    - Test UnauthorizedComponent displays access denied
    - _Requirements: 7.1, 8.6_

  - [x] 14.7 Create directive and pipe tests
    - Test custom directives (if any) work correctly
    - Test custom pipes (if any) transform data correctly
    - Test decimal pipe for price formatting
    - Test date pipe for date formatting
    - _Requirements: 7.1_

  - [x] 14.8 Create reactive form component tests
    - Test complex form with nested form groups
    - Test dynamic form field addition/removal
    - Test form array handling
    - Test cross-field validation
    - Test async validators
    - _Requirements: 7.1, 7.2, 7.3_


- [x] 15. Checkpoint - Frontend Component Tests Pass
  - Ensure all component tests pass, user interactions validated, ask the user if questions arise.

- [x] 16. Frontend E2E Tests (Cypress)
  - [x] 16.1 Configure Cypress test infrastructure
    - Install and configure Cypress
    - Create cypress.config.ts with base URL and viewport settings
    - Create support files for custom commands and utilities
    - Create page objects directory structure
    - Configure screenshot and video capture
    - Create cypress/fixtures for test data
    - _Requirements: 8.7, 8.1_

  - [x] 16.2 Create page object classes for main workflows
    - Create ReservationPage page object with form methods
    - Create OrderPage page object with order methods
    - Create MenuPage page object with menu interaction methods
    - Create ChefPage page object with chef browsing methods
    - Create CommonPage page object with navigation/header methods
    - Create AuthPage page object for login/logout
    - _Requirements: 8.1, 8.7_

  - [x] 16.3 Implement reservation workflow E2E tests
    - Test user can navigate to reservations page
    - Test user can search for available chefs
    - Test user can fill reservation form with valid data
    - Test user can submit reservation
    - Test confirmation message appears
    - Test reservation appears in user's reservation list
    - _Requirements: 8.1, 8.2_

  - [x] 16.4 Write property test for reservation workflow
    - Test complete workflow with various valid input combinations
    - _Requirements: 8.1_

  - [x] 16.5 Implement order workflow E2E tests
    - Test user can view menu items
    - Test user can search/filter menu items
    - Test user can add items to order
    - Test user can modify order quantities
    - Test user can view order total
    - Test user can submit order
    - Test order confirmation appears
    - _Requirements: 8.2_

  - [x] 16.6 Implement contact workflow E2E tests
    - Test user can navigate to contact page
    - Test user can fill contact form
    - Test form validation shows errors for invalid input
    - Test user can submit contact inquiry
    - Test success message appears
    - _Requirements: 8.3_

  - [x] 16.7 Implement search and filter E2E tests
    - Test user can search by chef name
    - Test user can filter by specialization
    - Test user can filter by rating
    - Test user can sort results
    - Test search results update on filter change
    - _Requirements: 8.4_

  - [x] 16.8 Implement navigation E2E tests
    - Test main navigation links work
    - Test breadcrumbs show correct path
    - Test back button works
    - Test URL changes on navigation
    - Test all main pages accessible
    - _Requirements: 8.5_

  - [x] 16.9 Implement error handling E2E tests
    - Test 404 page displays for invalid URL
    - Test error page displays on server error
    - Test error message displays on failed submission
    - Test retry button works on error pages
    - _Requirements: 8.6_

  - [x] 16.10 Implement authentication E2E tests
    - Test user can log in with valid credentials
    - Test user can log out
    - Test protected routes redirect to login
    - Test user can stay logged in across pages
    - Test session timeout works
    - _Requirements: 8.1, 8.5_


- [x] 17. Checkpoint - Frontend E2E Tests Pass
  - Ensure all E2E tests pass, user workflows validated end-to-end, ask the user if questions arise.

- [x] 18. Code Coverage Configuration and Gating
  - [x] 18.1 Configure backend coverage measurement
    - Create coverlet.runsettings configuration file
    - Configure Cobertura format output for reports
    - Set coverage thresholds: Domain 85%, Application 82%, Infrastructure 78%, API 80%
    - Configure ReportGenerator for HTML reports
    - Test coverage measurement with sample tests
    - _Requirements: 9.1, 9.2, 9.6_

  - [x] 18.2 Configure frontend coverage measurement
    - Configure karma.conf.js coverage reporter
    - Set coverage thresholds: Services 80%, Components 75%
    - Configure Istanbul/nyc reporting
    - Exclude template files from coverage
    - Test coverage collection with sample tests
    - _Requirements: 9.3, 9.4, 9.6_

  - [x] 18.3 Create coverage reporting scripts
    - Create script to generate backend coverage reports
    - Create script to generate frontend coverage reports
    - Create combined report viewer
    - Create coverage trend tracking
    - _Requirements: 9.2, 9.5_

  - [x] 18.4 Write property test for performance SLAs
    - **Property 21: Query Performance**
    - **Property 22: API Endpoint Response Time**
    - **Validates: Requirements 14.1, 14.2_**

  - [x] 18.5 Implement coverage gating in CI/CD
    - Create script to check if coverage meets minimum threshold
    - Fail CI/CD if core layers below threshold
    - Create exception allowlist for excluded files
    - Generate detailed coverage reports on failure
    - _Requirements: 9.7_

- [x] 19. Security Testing
  - [x] 19.1 Create input validation security tests
    - Test SQL injection patterns are escaped
    - Test XSS payloads are encoded
    - Test LDAP injection patterns are handled
    - Test command injection prevention
    - Test path traversal attempts blocked
    - _Requirements: 15.1, 15.2_

  - [x] 19.2 Write property test for injection attack prevention
    - **Property 19: Injection Attack Prevention**
    - **Validates: Requirements 15.2_**

  - [x] 19.3 Create sensitive data protection tests
    - Test passwords are never logged
    - Test tokens are never logged
    - Test PII fields are masked in logs
    - Test error messages don't expose internals
    - Test database queries don't leak credentials
    - _Requirements: 15.4_

  - [x] 19.4 Create API response data validation tests
    - Test response includes all required fields
    - Test response excludes sensitive fields (passwords, tokens)
    - Test response excludes internal fields
    - Test nested objects properly formatted
    - Test field ordering consistent
    - _Requirements: 15.6_

  - [x] 19.5 Write property test for response data completeness
    - **Property 20: API Response Data Completeness**
    - **Validates: Requirements 15.6_**


- [x] 20. Checkpoint - Security Tests Pass
  - Ensure all security tests pass, input validation and data protection validated, ask the user if questions arise.

- [x] 21. CI/CD Integration
  - [x] 21.1 Create GitHub Actions workflow
    - Create .github/workflows/tests.yml
    - Configure backend test job: restore, build, run tests with coverage
    - Configure frontend test job: install, build, run tests with coverage
    - Configure coverage upload to Codecov
    - Configure coverage gating to prevent merge if below threshold
    - Add notifications for test failures
    - _Requirements: 12.1, 12.2, 12.3, 12.4, 12.6_

  - [x] 21.2 Configure test parallelization
    - Configure backend tests to run in parallel by project
    - Configure frontend tests to run in parallel by suite
    - Configure E2E tests to run with multiple browsers
    - Optimize CI/CD execution time
    - _Requirements: 12.1, 12.2_

  - [x] 21.3 Create test result aggregation
    - Create script to aggregate test results from all layers
    - Create dashboard to display test results
    - Create failure analysis report
    - Create trend tracking for test metrics
    - _Requirements: 12.6_

  - [x] 21.4 Implement test artifact collection
    - Configure upload of test reports
    - Configure upload of coverage reports
    - Configure upload of E2E screenshots/videos on failure
    - Configure retention policies for artifacts
    - _Requirements: 12.6_

  - [x] 21.5 Create deployment gate configuration
    - Require all tests to pass before merge
    - Require coverage thresholds to pass
    - Block merge if flaky tests detected
    - Allow manual override with explanation
    - _Requirements: 12.4, 12.7_

- [x] 22. Testing Documentation
  - [x] 22.1 Create testing guidelines document
    - Document naming conventions for tests
    - Document Arrange-Act-Assert pattern
    - Document when to use unit vs integration vs E2E
    - Document mock/stub patterns
    - Document common assertions and helpers
    - Create README in each test project
    - _Requirements: 13.1, 13.2, 13.3_

  - [x] 22.2 Create test development templates
    - Create unit test template with examples
    - Create integration test template with examples
    - Create E2E test template with page object pattern
    - Create property-based test template
    - Document template usage
    - _Requirements: 13.2_

  - [x] 22.3 Create mocking and fixture guide
    - Document builder pattern for test data
    - Document mock factory patterns
    - Document database fixture setup/teardown
    - Document HttpClientTestingModule patterns
    - Create example builders and factories
    - _Requirements: 13.5, 10.5_

  - [x] 22.4 Create troubleshooting guide
    - Document common test failures and solutions
    - Document flaky test diagnosis
    - Document test performance optimization
    - Document debugging techniques
    - Document coverage analysis
    - _Requirements: 13.6, 13.7_

  - [x] 22.5 Create best practices documentation
    - Document assertion best practices
    - Document test isolation strategies
    - Document test data management
    - Document performance testing
    - Document security testing practices
    - _Requirements: 13.4, 13.7_


- [x] 23. Final Checkpoint - Complete Test Suite
  - Ensure all test layers have complete coverage, all properties validated, entire test framework operational, ask the user if questions arise.

## Task Dependency Graph

```json
{
  "waves": [
    {
      "waveId": 1,
      "tasks": ["1.1 Create test project structure and dependencies"]
    },
    {
      "waveId": 2,
      "tasks": ["1.2 Write property test for test project structure validation", "1.3 Create base test classes and common fixtures"]
    },
    {
      "waveId": 3,
      "tasks": ["1.4 Write integration test for base class functionality", "2.1 Create Chef entity tests", "2.3 Create Reservation entity tests", "2.5 Create Order entity tests", "2.7 Create MenuItem entity tests", "2.8 Create value object tests (Money, TimeSlot)"]
    },
    {
      "waveId": 4,
      "tasks": ["2.2 Write property test for Chef entity validation", "2.4 Write property test for Reservation state transitions", "2.6 Write property test for Order domain invariants", "2.9 Write property test for value object immutability"]
    },
    {
      "waveId": 5,
      "tasks": ["3. Checkpoint - Domain Layer Tests Pass"]
    },
    {
      "waveId": 6,
      "tasks": ["4.1 Create command handler test infrastructure", "4.6 Create query handler test infrastructure"]
    },
    {
      "waveId": 7,
      "tasks": ["4.2 Write property test for command handler processing", "4.3 Implement Reservation command handler tests", "4.4 Implement Order command handler tests", "4.5 Implement Menu command handler tests", "4.7 Write property test for query result filtering", "4.8 Implement Reservation query handler tests", "4.9 Implement Order query handler tests"]
    },
    {
      "waveId": 8,
      "tasks": ["4.10 Write property test for query pagination correctness", "4.11 Create validator tests"]
    },
    {
      "waveId": 9,
      "tasks": ["5. Checkpoint - Application Layer Tests Pass"]
    },
    {
      "waveId": 10,
      "tasks": ["6.1 Create repository test infrastructure"]
    },
    {
      "waveId": 11,
      "tasks": ["6.2 Write property test for CRUD round-trip operations", "6.3 Implement Chef repository tests", "6.4 Implement Reservation repository tests", "6.5 Implement Order repository tests", "6.6 Implement Unit of Work tests", "6.8 Create DbContext configuration tests"]
    },
    {
      "waveId": 12,
      "tasks": ["6.7 Write property test for transaction atomicity", "6.9 Write property test for referential integrity"]
    },
    {
      "waveId": 13,
      "tasks": ["7. Checkpoint - Infrastructure Layer Tests Pass"]
    },
    {
      "waveId": 14,
      "tasks": ["8.1 Create WebApplicationFactory test fixture", "8.2 Create API endpoint test base class"]
    },
    {
      "waveId": 15,
      "tasks": ["8.3 Implement Reservations endpoint integration tests", "8.5 Implement Orders endpoint integration tests", "8.6 Implement Menu endpoint integration tests", "8.7 Implement Chefs endpoint integration tests", "8.8 Implement Contact endpoint integration tests", "8.10 Implement exception handling middleware tests"]
    },
    {
      "waveId": 16,
      "tasks": ["8.4 Write property test for input validation consistency", "8.9 Write property test for authorization enforcement", "8.11 Write property test for exception mapping"]
    },
    {
      "waveId": 17,
      "tasks": ["9. Checkpoint - API Integration Tests Pass"]
    },
    {
      "waveId": 18,
      "tasks": ["10.1 Create middleware test infrastructure", "10.2 Implement Authorization middleware tests", "10.3 Implement CORS middleware tests", "10.4 Implement SecurityHeaders middleware tests", "10.5 Implement request/response logging middleware tests"]
    },
    {
      "waveId": 19,
      "tasks": ["10.6 Write property test for sensitive data protection", "11.1 Configure Angular testing environment"]
    },
    {
      "waveId": 20,
      "tasks": ["11.2 Create frontend test base classes and fixtures", "11.3 Configure code coverage for frontend", "11.4 Create HTTP testing utilities"]
    },
    {
      "waveId": 21,
      "tasks": ["12.1 Create ReservationService tests", "12.3 Create OrderService tests", "12.5 Create MenuService tests", "12.7 Create ChefService tests", "12.8 Create authentication-related service tests", "12.9 Create HTTP interceptor tests"]
    },
    {
      "waveId": 22,
      "tasks": ["12.2 Write property test for HTTP communication", "12.4 Write property test for service error handling", "12.6 Write property test for caching behavior", "12.10 Write property test for HTTP interceptor processing"]
    },
    {
      "waveId": 23,
      "tasks": ["13. Checkpoint - Frontend Service Tests Pass"]
    },
    {
      "waveId": 24,
      "tasks": ["14.1 Create ReservationFormComponent tests", "14.2 Create OrderListComponent tests", "14.3 Create MenuDisplayComponent tests", "14.4 Create ChefListComponent tests", "14.5 Create HeaderComponent tests", "14.6 Create error and loading components tests", "14.7 Create directive and pipe tests", "14.8 Create reactive form component tests"]
    },
    {
      "waveId": 25,
      "tasks": ["15. Checkpoint - Frontend Component Tests Pass"]
    },
    {
      "waveId": 26,
      "tasks": ["16.1 Configure Cypress test infrastructure"]
    },
    {
      "waveId": 27,
      "tasks": ["16.2 Create page object classes for main workflows"]
    },
    {
      "waveId": 28,
      "tasks": ["16.3 Implement reservation workflow E2E tests", "16.5 Implement order workflow E2E tests", "16.6 Implement contact workflow E2E tests", "16.7 Implement search and filter E2E tests", "16.8 Implement navigation E2E tests", "16.9 Implement error handling E2E tests", "16.10 Implement authentication E2E tests"]
    },
    {
      "waveId": 29,
      "tasks": ["16.4 Write property test for reservation workflow"]
    },
    {
      "waveId": 30,
      "tasks": ["17. Checkpoint - Frontend E2E Tests Pass"]
    },
    {
      "waveId": 31,
      "tasks": ["18.1 Configure backend coverage measurement", "18.2 Configure frontend coverage measurement"]
    },
    {
      "waveId": 32,
      "tasks": ["18.3 Create coverage reporting scripts", "18.5 Implement coverage gating in CI/CD"]
    },
    {
      "waveId": 33,
      "tasks": ["18.4 Write property test for performance SLAs"]
    },
    {
      "waveId": 34,
      "tasks": ["19.1 Create input validation security tests"]
    },
    {
      "waveId": 35,
      "tasks": ["19.2 Write property test for injection attack prevention", "19.3 Create sensitive data protection tests", "19.4 Create API response data validation tests"]
    },
    {
      "waveId": 36,
      "tasks": ["19.5 Write property test for response data completeness"]
    },
    {
      "waveId": 37,
      "tasks": ["20. Checkpoint - Security Tests Pass"]
    },
    {
      "waveId": 38,
      "tasks": ["21.1 Create GitHub Actions workflow"]
    },
    {
      "waveId": 39,
      "tasks": ["21.2 Configure test parallelization", "21.3 Create test result aggregation", "21.4 Implement test artifact collection", "21.5 Create deployment gate configuration"]
    },
    {
      "waveId": 40,
      "tasks": ["22.1 Create testing guidelines document"]
    },
    {
      "waveId": 41,
      "tasks": ["22.2 Create test development templates", "22.3 Create mocking and fixture guide", "22.4 Create troubleshooting guide", "22.5 Create best practices documentation"]
    },
    {
      "waveId": 42,
      "tasks": ["23. Final Checkpoint - Complete Test Suite"]
    }
  ]
}
```

## Notes

- All tasks are organized by testing layer (Domain, Application, Infrastructure, API, Frontend Services, Frontend Components, E2E)
- Property-based test tasks (marked with *) are optional but strongly recommended for validating correctness properties
- Each testing layer has checkpoint tasks to verify progress before moving forward
- Backend uses C# with xUnit, FluentAssertions, Moq, and Coverlet
- Frontend uses TypeScript with Jasmine/Karma for unit tests and Cypress for E2E tests
- Tasks reference specific requirements for full traceability
- Coverage targets: Domain 85%, Application 82%, Infrastructure 78%, API 80%, Services 80%, Components 75%
- Test names follow [Method]_[Condition]_[ExpectedResult] convention
- Builder pattern used for complex test data creation
- Mock factories reduce boilerplate and ensure consistency
- Base test classes provide common setup and fixtures
- All external dependencies are mocked to ensure test isolation
- Integration tests use in-memory databases or test containers for isolation
- E2E tests use page objects for maintainability
- CI/CD pipeline enforces coverage gates before merge
