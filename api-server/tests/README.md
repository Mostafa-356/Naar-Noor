# Test Projects for Naar & Noor

This directory contains the comprehensive test suite for the Naar & Noor restaurant management application.

## Project Structure

### Backend Test Projects

#### NaarNoor.Domain.Tests
Unit tests for domain entities and value objects in the core business logic layer.

**Directory Structure:**
- `Entities/` - Tests for domain entities (Chef, Reservation, Order, MenuItem, etc.)
- `ValueObjects/` - Tests for value objects (Money, TimeSlot, etc.)
- `Fixtures/` - Reusable test fixtures and builders

**Key Patterns:**
- Entity constructor validation and immutability
- Value object equality and immutability verification
- Domain invariants and business rule enforcement
- State machine transitions for entities with status

#### NaarNoor.Application.Tests
Unit tests for command handlers, query handlers, and application services.

**Directory Structure:**
- `Reservations/Commands/` - Tests for reservation command handlers
- `Reservations/Queries/` - Tests for reservation query handlers
- `Orders/` - Tests for order-related handlers
- `Menus/` - Tests for menu-related handlers
- `Chefs/` - Tests for chef-related handlers
- `Contacts/` - Tests for contact inquiry handlers
- `Reviews/` - Tests for review-related handlers
- `Common/Fixtures/` - Shared test fixtures and base classes
- `Common/Mocks/` - Mock implementations and factories

**Key Patterns:**
- Command handler testing with mocked dependencies
- Query handler testing with seeded test data
- FluentAssertions for readable assertions
- Moq for dependency mocking

#### NaarNoor.Infrastructure.Tests
Unit and integration tests for data access, repositories, and persistence logic.

**Directory Structure:**
- `Persistence/Repositories/` - Tests for repository implementations
- `Persistence/UnitOfWork/` - Tests for transaction management
- `Persistence/Migrations/` - Tests for database schema changes
- `Fixtures/` - Database fixtures and factory methods
- `Common/` - Shared utilities and base classes

**Key Patterns:**
- In-memory database testing with Entity Framework
- CRUD operation verification
- Query construction and execution testing
- Transaction and rollback behavior testing

#### NaarNoor.API.Tests
Integration tests for HTTP endpoints, middleware, and the full API stack.

**Directory Structure:**
- `Integration/Endpoints/` - Tests for REST API endpoints
- `Integration/Middleware/` - Tests for middleware components
- `Integration/Fixtures/` - WebApplicationFactory and test helpers
- `Database/` - Database integration tests

**Key Patterns:**
- WebApplicationFactory for in-memory test server
- HTTP status code and response validation
- Middleware behavior verification
- Exception mapping to HTTP responses

## Testing Guidelines

### Running Tests

Run all tests:
```bash
dotnet test NaarNoor.sln
```

Run tests for a specific project:
```bash
dotnet test tests/NaarNoor.Domain.Tests
dotnet test tests/NaarNoor.Application.Tests
dotnet test tests/NaarNoor.Infrastructure.Tests
dotnet test tests/NaarNoor.API.Tests
```

Run tests with coverage:
```bash
dotnet test NaarNoor.sln --collect:"XPlat Code Coverage"
```

### Coverage Goals

- **Domain Layer**: 85% minimum coverage
- **Application Layer**: 82% minimum coverage
- **Infrastructure Layer**: 78% minimum coverage
- **API Layer**: 80% minimum coverage

### Naming Conventions

- Test classes: `[ClassUnderTest]Tests` (e.g., `ChefTests`, `CreateReservationCommandHandlerTests`)
- Test methods: `[MethodName]_[Scenario]_[ExpectedBehavior]` (e.g., `Create_WithValidData_ShouldInitializeProperties`)
- Theory test data: `[Fact]` or `[Theory]` with `[InlineData]` or `[MemberData]`

### Test Organization

1. **Arrange** - Set up test data and dependencies
2. **Act** - Execute the code under test
3. **Assert** - Verify the results

Use FluentAssertions for readable, chainable assertions:
```csharp
result.Should().NotBeNull();
result.Id.Should().Be(expectedId);
result.Status.Should().Be(OrderStatus.Confirmed);
```

### Mocking Patterns

Use Moq for dependency isolation:
```csharp
var mockRepository = new Mock<IReservationRepository>();
mockRepository
    .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
    .ReturnsAsync(expectedReservation);
```

Verify mock calls:
```csharp
mockRepository.Verify(
    x => x.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()),
    Times.Once
);
```

### Test Data Builders

Use builder pattern for complex entity construction:
```csharp
var reservation = ReservationBuilder
    .Default()
    .WithChefId(chefId)
    .WithPartySize(4)
    .Build();
```

## CI/CD Integration

Tests are automatically run on:
- Every pull request
- Before merge to main/master
- On scheduled daily builds

Coverage reports are generated and tracked over time.
