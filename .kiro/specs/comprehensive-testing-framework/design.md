# Design Document: Comprehensive Testing Framework for Naar & Noor

## Overview

The Naar & Noor testing framework establishes a multi-layer testing strategy spanning backend (.NET/C#) and frontend (Angular/TypeScript) applications. The framework implements a testing pyramid that emphasizes unit tests as the foundation, supplements with strategic integration tests, and validates critical user workflows through E2E tests.

### Key Objectives

1. **Quality Assurance**: Achieve >80% code coverage on core business logic layers (Domain, Application, Infrastructure)
2. **Maintainability**: Establish consistent testing patterns that reduce friction for developers
3. **Reliability**: Implement automated validation that catches regressions early and ensures system stability
4. **Confidence**: Provide confidence through both unit and integration testing across all layers
5. **Performance**: Optimize test execution speed while maintaining comprehensive coverage

### Target Coverage Metrics

- **Domain Layer**: 85% minimum coverage
- **Application Layer**: 82% minimum coverage
- **Infrastructure Layer**: 78% minimum coverage
- **API Layer**: 80% minimum coverage
- **Angular Services**: 80% minimum coverage
- **Angular Components**: 75% minimum coverage (UI/template logic excluded from coverage gates)

---

## Architecture

### Testing Pyramid

```
                    ╔═════════════════════╗
                    ║   E2E Tests         ║  End-to-end user workflows
                    ║   (Cypress/Playwright)
                    ║   ~5-10% of tests   ║
                    ╚═════════════════════╝
                         △ △ △ △
                    ╔═════════════════════╗
                    ║ Integration Tests   ║  Multi-component interaction
                    ║ (xUnit, Jasmine)    ║  API endpoints, DB operations
                    ║ ~20-30% of tests    ║
                    ╚═════════════════════╝
                       △ △ △ △ △ △ △
              ╔═════════════════════════════════╗
              ║     Unit Tests                  ║  Isolated components
              ║   (xUnit, Jasmine, Moq)         ║  Fast, focused validation
              ║   ~60-70% of tests              ║
              ╚═════════════════════════════════╝
```

### Testing Layers

#### 1. Backend Test Layers (.NET)

```
┌─────────────────────────────────────────────────────────┐
│                    NaarNoor.API Tests                   │
│  - WebApplicationFactory integration tests              │
│  - Controller endpoint validation                       │
│  - Middleware pipeline verification                     │
│  - HTTP status code and header validation               │
└────────────┬────────────────────────────────┬───────────┘
             │                                │
┌────────────▼──────────┐    ┌────────────────▼──────────┐
│  Command/Query Tests  │    │  Integration Tests         │
│  Application Layer    │    │  Database Operations       │
│  ────────────────     │    │  ────────────────          │
│  - Handler logic      │    │  - Repository CRUD         │
│  - Validation         │    │  - DbContext queries       │
│  - Service calls      │    │  - Transaction mgmt        │
│  - MediatR behavior   │    │  - Relationships           │
└─────────┬─────────────┘    └────────────┬────────────────┘
          │                               │
          └───────────────┬───────────────┘
                          │
              ┌───────────▼──────────┐
              │  Domain Layer Tests  │
              │  ──────────────────  │
              │  - Entity validation │
              │  - Value objects     │
              │  - Business rules    │
              │  - State transitions │
              └──────────────────────┘
```

#### 2. Frontend Test Layers (Angular)

```
┌──────────────────────────────────────────┐
│           E2E Tests (Cypress)            │
│  - User workflows end-to-end             │
│  - Real browser automation               │
│  - Navigation and transitions            │
└────────────┬─────────────────────────────┘
             │
┌────────────▼──────────┐    ┌───────────────┐
│  Component Tests      │    │  Service Tests│
│  ──────────────      │    │  ──────────────│
│  - Template rendering│    │  - API calls   │
│  - Input binding     │    │  - Data xform  │
│  - Event handling    │    │  - Observable  │
│  - Change detection  │    │  - Error hdlng │
│  - @Input/@Output    │    │  - Caching    │
└──────────────────────┘    └───────────────┘
```

### Test Project Organization

```
api-server/
├── src/
│   ├── NaarNoor.API/
│   ├── NaarNoor.Application/
│   ├── NaarNoor.Domain/
│   └── NaarNoor.Infrastructure/
└── tests/
    ├── NaarNoor.Domain.Tests/
    │   ├── Entities/
    │   │   ├── ChefTests.cs
    │   │   ├── ReservationTests.cs
    │   │   ├── OrderTests.cs
    │   │   └── ...
    │   ├── ValueObjects/
    │   │   ├── MoneyTests.cs
    │   │   ├── TimeSlotTests.cs
    │   │   └── ...
    │   └── Fixtures/
    │       └── DomainFixtures.cs
    │
    ├── NaarNoor.Application.Tests/
    │   ├── Reservations/
    │   │   ├── Commands/
    │   │   │   └── CreateReservationCommandTests.cs
    │   │   └── Queries/
    │   │       └── GetReservationsQueryTests.cs
    │   ├── Orders/
    │   ├── Menus/
    │   ├── Chefs/
    │   ├── Contacts/
    │   ├── Reviews/
    │   └── Common/
    │       ├── Fixtures/
    │       └── Mocks/
    │
    ├── NaarNoor.Infrastructure.Tests/
    │   ├── Persistence/
    │   │   ├── Repositories/
    │   │   ├── UnitOfWork/
    │   │   └── Migrations/
    │   ├── Fixtures/
    │   │   └── DatabaseFixtures.cs
    │   └── Common/
    │
    └── NaarNoor.API.Tests/
        ├── Integration/
        │   ├── Endpoints/
        │   │   ├── ReservationsEndpointTests.cs
        │   │   ├── OrdersEndpointTests.cs
        │   │   ├── MenuEndpointTests.cs
        │   │   └── ...
        │   ├── Middleware/
        │   │   ├── AuthorizationMiddlewareTests.cs
        │   │   ├── CorsMiddlewareTests.cs
        │   │   ├── ExceptionHandlingMiddlewareTests.cs
        │   │   └── ...
        │   └── Fixtures/
        │       └── WebApplicationFactoryFixture.cs
        └── Database/
            ├── RepositoryTests.cs
            ├── QueryTests.cs
            └── TransactionTests.cs

frontend/
└── tests/
    ├── unit/
    │   ├── services/
    │   │   ├── reservation.service.spec.ts
    │   │   ├── order.service.spec.ts
    │   │   ├── menu.service.spec.ts
    │   │   └── ...
    │   ├── components/
    │   │   ├── reservation-form.component.spec.ts
    │   │   ├── order-list.component.spec.ts
    │   │   ├── menu-display.component.spec.ts
    │   │   └── ...
    │   └── fixtures/
    │       ├── mock-data.ts
    │       ├── test-factories.ts
    │       └── http-testing.module.ts
    └── e2e/
        ├── reservation.cy.ts
        ├── order.cy.ts
        ├── contact.cy.ts
        └── navigation.cy.ts
```

---

## Components and Interfaces

### Backend Testing Components

#### 1. Domain Layer Testing

**Purpose**: Validate core business entities and value objects in isolation

**Key Components**:
- **Entity Test Base Class**: Common setup for entity testing
- **Value Object Test Base Class**: Immutability and equality verification
- **Domain Event Tests**: Event publishing and handling
- **Specification Pattern Tests**: Business rule validation

**Example Structure**:
```csharp
public class ChefTests
{
    [Fact]
    public void Create_WithValidData_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var name = "Gordon Ramsay";
        var specialization = "French Cuisine";
        
        // Act
        var chef = new Chef(name, specialization);
        
        // Assert
        chef.Name.Should().Be(name);
        chef.Specialization.Should().Be(specialization);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldThrowDomainException(string invalidName)
    {
        // Act & Assert
        Action act = () => new Chef(invalidName, "French");
        act.Should().Throw<DomainException>();
    }
}
```

#### 2. Application Layer Testing

**Purpose**: Validate command/query handlers and business logic orchestration

**Key Components**:
- **Command Handler Mock Setup**: Repository and service mocking
- **Query Handler Verification**: Result transformation and filtering
- **Validator Testing**: Input validation rules
- **Handler Pipeline Testing**: MediatR behavior

**Example Structure**:
```csharp
public class CreateReservationCommandTests
{
    private readonly Mock<IReservationRepository> _reservationRepositoryMock;
    private readonly Mock<IChefRepository> _chefRepositoryMock;
    private readonly CreateReservationCommandHandler _handler;
    
    public CreateReservationCommandTests()
    {
        _reservationRepositoryMock = new Mock<IReservationRepository>();
        _chefRepositoryMock = new Mock<IChefRepository>();
        _handler = new CreateReservationCommandHandler(
            _reservationRepositoryMock.Object,
            _chefRepositoryMock.Object
        );
    }
    
    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateReservationAndReturn()
    {
        // Arrange
        var command = new CreateReservationCommand(
            chefId: Guid.NewGuid(),
            reservationDate: DateTime.Now.AddDays(1),
            partySize: 4
        );
        
        _chefRepositoryMock
            .Setup(x => x.GetByIdAsync(command.ChefId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ChefBuilder.Default().Build());
        
        _reservationRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.Should().NotBe(Guid.Empty);
        _reservationRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
```

#### 3. Infrastructure Layer Testing

**Purpose**: Validate data access, repository patterns, and persistence

**Key Components**:
- **Repository Mock Verification**: Query construction and execution
- **DbContext Testing**: Entity configuration and mapping
- **Unit of Work Testing**: Transaction management
- **Query Materialization**: LINQ to SQL translation

**Example Structure**:
```csharp
public class ReservationRepositoryTests : IAsyncLifetime
{
    private readonly ApplicationDbContextFactory _dbContextFactory;
    private ApplicationDbContext _dbContext;
    
    public async Task InitializeAsync()
    {
        _dbContextFactory = new ApplicationDbContextFactory();
        _dbContext = await _dbContextFactory.CreateDbContextAsync();
    }
    
    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }
    
    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnReservation()
    {
        // Arrange
        var reservation = ReservationBuilder.Default().Build();
        _dbContext.Reservations.Add(reservation);
        await _dbContext.SaveChangesAsync();
        
        var repository = new ReservationRepository(_dbContext);
        
        // Act
        var result = await repository.GetByIdAsync(reservation.Id, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(reservation.Id);
    }
}
```

#### 4. API Layer Integration Testing

**Purpose**: Validate HTTP request/response handling and middleware pipeline

**Key Components**:
- **WebApplicationFactory**: In-memory test server
- **HttpClient**: Simulated HTTP requests
- **Response Validation**: Status codes, headers, body
- **Middleware Verification**: Pipeline behavior

**Example Structure**:
```csharp
public class ReservationsEndpointTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    
    public ReservationsEndpointTests()
    {
        _factory = new WebApplicationFactory<Program>();
    }
    
    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
    }
    
    public async Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
    }
    
    [Fact]
    public async Task Post_WithValidReservation_ShouldReturn201()
    {
        // Arrange
        var createDto = new CreateReservationDto
        {
            ChefId = Guid.NewGuid(),
            ReservationDate = DateTime.Now.AddDays(1),
            PartySize = 4
        };
        
        var json = JsonConvert.SerializeObject(createDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/reservations", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("id");
    }
}
```

### Frontend Testing Components

#### 1. Service Testing

**Purpose**: Validate Angular services and HTTP communication in isolation

**Key Components**:
- **HttpClientTestingModule**: Mock HTTP requests
- **HttpTestingController**: Assert HTTP calls
- **Observable Testing**: Async validation
- **Error Handling**: Observable error scenarios

**Example Structure**:
```typescript
describe('ReservationService', () => {
  let service: ReservationService;
  let httpMock: HttpTestingController;
  
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ReservationService]
    });
    
    service = TestBed.inject(ReservationService);
    httpMock = TestBed.inject(HttpTestingController);
  });
  
  afterEach(() => {
    httpMock.verify();
  });
  
  it('should fetch reservations from API', () => {
    // Arrange
    const mockData: Reservation[] = [
      { id: '1', chefId: '1', reservationDate: new Date(), partySize: 4 }
    ];
    
    // Act
    service.getReservations().subscribe(data => {
      // Assert
      expect(data).toEqual(mockData);
    });
    
    const req = httpMock.expectOne('/api/reservations');
    expect(req.request.method).toBe('GET');
    req.flush(mockData);
  });
});
```

#### 2. Component Testing

**Purpose**: Validate Angular component logic, bindings, and interactions

**Key Components**:
- **TestBed**: Component configuration and dependency injection
- **ComponentFixture**: Component instance and element access
- **DebugElement**: DOM query and event triggering
- **Input/Output Binding**: Property and event verification

**Example Structure**:
```typescript
describe('ReservationFormComponent', () => {
  let component: ReservationFormComponent;
  let fixture: ComponentFixture<ReservationFormComponent>;
  let mockReservationService: jasmine.SpyObj<ReservationService>;
  
  beforeEach(async () => {
    mockReservationService = jasmine.createSpyObj('ReservationService', ['createReservation']);
    
    await TestBed.configureTestingModule({
      declarations: [ReservationFormComponent],
      providers: [
        { provide: ReservationService, useValue: mockReservationService }
      ]
    }).compileComponents();
    
    fixture = TestBed.createComponent(ReservationFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });
  
  it('should initialize form fields', () => {
    expect(component.reservationForm.get('partySize')).toBeDefined();
    expect(component.reservationForm.get('reservationDate')).toBeDefined();
  });
  
  it('should call service on form submit', () => {
    // Arrange
    component.reservationForm.patchValue({
      chefId: '1',
      reservationDate: new Date(),
      partySize: 4
    });
    
    mockReservationService.createReservation.and.returnValue(
      of({ id: '1' })
    );
    
    // Act
    component.onSubmit();
    
    // Assert
    expect(mockReservationService.createReservation).toHaveBeenCalled();
  });
});
```

#### 3. E2E Testing

**Purpose**: Validate complete user workflows through browser automation

**Key Components**:
- **Cypress Commands**: Page interactions and assertions
- **Page Objects**: Test data organization
- **Wait Strategies**: Element visibility and stability
- **Screenshot Capture**: Visual debugging

**Example Structure**:
```typescript
// cypress/e2e/reservation-flow.cy.ts
describe('Reservation Flow', () => {
  beforeEach(() => {
    cy.visit('/');
  });
  
  it('should complete reservation creation end-to-end', () => {
    // Navigate to reservation page
    cy.contains('Make a Reservation').click();
    cy.location('pathname').should('include', '/reservations');
    
    // Fill form
    cy.get('input[name="chefName"]').type('Gordon Ramsay');
    cy.get('input[name="partySize"]').type('4');
    cy.get('input[type="date"]').type('2024-12-25');
    
    // Submit
    cy.get('button[type="submit"]').click();
    
    // Verify success
    cy.contains('Reservation confirmed').should('be.visible');
    cy.location('pathname').should('include', '/confirmation');
  });
});
```

---

## Data Models

### Backend Data Models

#### Domain Layer Entities

```csharp
public abstract class Entity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public List<DomainEvent> DomainEvents { get; } = new();
}

public class Chef : Entity
{
    public string Name { get; private set; }
    public string Specialization { get; private set; }
    public int Experience { get; private set; }
    public decimal Rating { get; private set; }
    public IReadOnlyList<Reservation> Reservations { get; private set; }
}

public class Reservation : Entity
{
    public Guid ChefId { get; private set; }
    public Chef Chef { get; private set; }
    public DateTime ReservationDate { get; private set; }
    public int PartySize { get; private set; }
    public ReservationStatus Status { get; private set; }
    public IReadOnlyList<OrderItem> Items { get; private set; }
}

public class Order : Entity
{
    public Guid ReservationId { get; private set; }
    public Reservation Reservation { get; private set; }
    public decimal TotalPrice { get; private set; }
    public OrderStatus Status { get; private set; }
    public IReadOnlyList<OrderItem> Items { get; private set; }
}

public class MenuItem : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Money Price { get; private set; }
    public MenuItemCategory Category { get; private set; }
    public bool IsAvailable { get; private set; }
}
```

#### Value Objects

```csharp
public class Money : IEquatable<Money>
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "USD";
    
    public Money(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new DomainException("Amount cannot be negative");
        
        Amount = amount;
        Currency = currency;
    }
    
    public bool Equals(Money other) => 
        other != null && Amount == other.Amount && Currency == other.Currency;
    
    public override bool Equals(object obj) => Equals(obj as Money);
    public override int GetHashCode() => HashCode.Combine(Amount, Currency);
}

public class TimeSlot : IEquatable<TimeSlot>
{
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    
    public TimeSlot(TimeSpan startTime, TimeSpan endTime)
    {
        if (startTime >= endTime)
            throw new DomainException("Start time must be before end time");
        
        StartTime = startTime;
        EndTime = endTime;
    }
    
    public bool Overlaps(TimeSlot other) => 
        StartTime < other.EndTime && EndTime > other.StartTime;
}
```

### Frontend Data Models

```typescript
// Core Domain Models
export interface Reservation {
  id: string;
  chefId: string;
  reservationDate: Date;
  partySize: number;
  status: ReservationStatus;
  createdAt: Date;
}

export interface Order {
  id: string;
  reservationId: string;
  items: OrderItem[];
  totalPrice: number;
  status: OrderStatus;
}

export interface MenuItem {
  id: string;
  name: string;
  description: string;
  price: number;
  category: MenuItemCategory;
  isAvailable: boolean;
}

export interface Chef {
  id: string;
  name: string;
  specialization: string;
  experience: number;
  rating: number;
}

// API DTOs (simplified from domain models for transfer)
export interface CreateReservationDto {
  chefId: string;
  reservationDate: Date;
  partySize: number;
}

export interface CreateOrderDto {
  reservationId: string;
  items: OrderItemDto[];
}
```

### Test Data Structures

#### Backend Builders

```csharp
public class ChefBuilder
{
    private string _name = "Gordon Ramsay";
    private string _specialization = "French";
    private int _experience = 20;
    
    public static ChefBuilder Default() => new();
    
    public ChefBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public Chef Build()
    {
        var chef = new Chef(_name, _specialization, _experience);
        return chef;
    }
}

public class ReservationBuilder
{
    private Guid _chefId = Guid.NewGuid();
    private DateTime _reservationDate = DateTime.Now.AddDays(1);
    private int _partySize = 4;
    
    public static ReservationBuilder Default() => new();
    
    public ReservationBuilder WithChefId(Guid chefId)
    {
        _chefId = chefId;
        return this;
    }
    
    public Reservation Build()
    {
        var reservation = new Reservation(_chefId, _reservationDate, _partySize);
        return reservation;
    }
}
```

#### Frontend Mock Data

```typescript
// Mock factories for testing
export class ReservationMockFactory {
  static create(overrides?: Partial<Reservation>): Reservation {
    return {
      id: '1',
      chefId: '1',
      reservationDate: new Date('2024-12-25'),
      partySize: 4,
      status: 'confirmed',
      createdAt: new Date(),
      ...overrides
    };
  }
  
  static list(count: number): Reservation[] {
    return Array.from({ length: count }, (_, i) =>
      this.create({ id: `${i + 1}` })
    );
  }
}

// HttpClient mock setup
export const createHttpClientMock = (): HttpTestingController => {
  const testingModule = TestBed.configureTestingModule({
    imports: [HttpClientTestingModule]
  });
  return TestBed.inject(HttpTestingController);
};
```

---

## Correctness Properties

The following acceptance criteria from the requirements document have been analyzed for property-based testing applicability.


### Property Reflection and Consolidation

After analyzing the acceptance criteria, I have identified properties suitable for property-based testing. Many of the acceptance criteria relate to testing infrastructure, CI/CD configuration, documentation, and tooling selection rather than code functionality that can be verified through properties. Below are the key property-based testable requirements consolidated to eliminate redundancy:

**Consolidation Decisions**:
- Domain entity and value object properties are combined into unified entity validation properties
- Repository CRUD operations are consolidated into data persistence round-trip properties
- Service HTTP call patterns are combined into a unified HTTP communication property
- Component interaction properties are combined into behavioral verification
- Performance properties are consolidated (query performance, endpoint performance, bulk operations)
- Security properties are combined into unified input validation, authorization, and data protection properties

### Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Entity State Validation

For any domain entity (Chef, Reservation, Order, MenuItem), when created with valid data, all properties SHALL be correctly initialized and when created with invalid data (negative prices, empty names, invalid status codes), the entity constructor SHALL throw a DomainException.

**Validates: Requirements 1.1, 1.2, 1.4**

### Property 2: Value Object Immutability and Equality

For any value object (Money, TimeSlot), attempting to modify properties after construction SHALL fail (no public setters), and two value objects with identical values SHALL compare as equal regardless of creation order, while objects with different values SHALL not be equal.

**Validates: Requirements 1.5**

### Property 3: Entity State Transitions

For any Order entity, status transitions SHALL only be allowed according to the defined state machine (e.g., Pending→Confirmed→InPreparation→Ready→Completed), and transitions to invalid states (e.g., Completed→Pending) SHALL throw a DomainException.

**Validates: Requirements 1.3, 1.6**

### Property 4: Domain Invariants Preservation

For any entity state change (status update, property modification), domain invariants relevant to that entity SHALL remain satisfied after the change (e.g., Reservation.PartySize > 0, Order.TotalPrice ≥ 0).

**Validates: Requirements 1.6**

### Property 5: Command Handler Processing

For any valid command (CreateReservationCommand, UpdateOrderCommand, etc.), when passed to the corresponding command handler with all mocked dependencies configured, the handler SHALL complete without throwing, SHALL call expected repository methods with correct parameters, and SHALL return expected result types.

**Validates: Requirements 2.1, 2.3**

### Property 6: Query Result Filtering and Transformation

For any query (GetAvailableMenuItemsQuery, GetReservationsByChefQuery, etc.) executed with random filter parameters against seeded test data, the query handler SHALL return only results matching the filter criteria, with correct ordering applied, and without database access.

**Validates: Requirements 2.2**

### Property 7: Repository CRUD Round-Trip

For any entity instance created with valid data and added to a repository, when that same entity is retrieved by ID, all properties SHALL match the original entity, and when the entity is updated with new values, subsequent retrieval SHALL return the updated values.

**Validates: Requirements 3.1, 5.1, 5.3**

### Property 8: Query Pagination Correctness

For any paginated query executed with various page numbers and page sizes against test data, the returned results SHALL be limited to the specified page size, SHALL contain the correct subset of data for the requested page, and SHALL have consistent ordering across multiple executions.

**Validates: Requirements 5.2**

### Property 9: Transaction Atomicity

For any transaction containing multiple repository operations, when the transaction is committed, all operations SHALL be persisted atomically, and when the transaction is rolled back, none of the changes SHALL be persisted.

**Validates: Requirements 5.5**

### Property 10: Referential Integrity Enforcement

For any entity with foreign key relationships, when a related entity is deleted, the database SHALL enforce referential integrity constraints and prevent deletion if dependent records exist, or cascade delete if configured.

**Validates: Requirements 5.4**

### Property 11: HTTP Exception Mapping

For any exception thrown during API endpoint processing (DomainException, ValidationException, NotFoundException, etc.), the exception handling middleware SHALL map the exception to an appropriate HTTP status code and return a properly formatted error response.

**Validates: Requirements 4.6**

### Property 12: Service HTTP Communication

For any service method that calls an HTTP endpoint, when HttpClient receives a mocked response, the service SHALL construct the correct request (method, URL, headers, body), transform the response to the expected type, and return the transformed result.

**Validates: Requirements 6.1, 6.2**

### Property 13: Service Error Handling

For any service method encountering HTTP errors (4xx, 5xx status codes, network failures), the service SHALL handle the error appropriately (throw HttpRequestException, map to domain exception, or return fallback value) consistently across all error scenarios.

**Validates: Requirements 6.4**

### Property 14: Service Caching Behavior

For any cached service method called multiple times with identical parameters, the first call SHALL execute the HTTP request, subsequent calls SHALL return cached value without executing new requests, and cache invalidation SHALL work correctly.

**Validates: Requirements 6.5**

### Property 15: HTTP Interceptor Processing

For any HTTP interceptor registered in the HttpClient pipeline, when HTTP requests are made, the interceptor SHALL be invoked, apply its transformation logic (adding headers, logging, auth token injection, etc.), and subsequent interceptors and the actual request SHALL see the modified request.

**Validates: Requirements 6.6**

### Property 16: Input Validation Consistency

For any API endpoint accepting user input, when provided with invalid data (null values where required, strings exceeding max length, invalid enum values, negative numbers where positive required, etc.), the endpoint SHALL return HTTP 400 with validation error details consistently across all endpoints.

**Validates: Requirements 15.1**

### Property 17: Authorization Enforcement

For any API endpoint requiring authorization, when accessed without valid credentials, the endpoint SHALL return HTTP 401, when accessed with valid credentials but insufficient permissions, the endpoint SHALL return HTTP 403, and when accessed with valid credentials and permissions, the endpoint SHALL process the request normally.

**Validates: Requirements 15.3**

### Property 18: Sensitive Data Protection

For any API response, logs, or exception messages, sensitive fields (passwords, tokens, personal identification numbers) SHALL never appear in their raw form, and personally identifiable information SHALL not be logged at INFO or DEBUG levels without explicit user consent.

**Validates: Requirements 15.4**

### Property 19: Injection Attack Prevention

For any serializable entity containing string fields, when deserialized from user input containing common injection patterns (SQL injection, XSS, LDAP injection payloads), the entity properties SHALL contain the literal string values without interpretation or execution.

**Validates: Requirements 15.2**

### Property 20: API Response Data Completeness

For any API endpoint returning a list of entities, each entity in the response SHALL include all required fields defined in the API contract (excluding intentionally excluded fields like passwords), and SHALL not include internal fields not intended for client use.

**Validates: Requirements 15.6**

### Property 21: Query Performance

For any database query (repository method or application query), execution time SHALL be under the specified SLA threshold (e.g., <100ms for simple queries, <500ms for complex queries with joins), measured with cold cache to ensure consistency.

**Validates: Requirements 14.1**

### Property 22: API Endpoint Response Time

For any HTTP endpoint, response time (including all processing, database access, external service calls) SHALL be under the specified SLA threshold appropriate for the operation type (simple GET <200ms, complex POST with writes <500ms).

**Validates: Requirements 14.2**

---

## Error Handling

### Backend Error Handling Strategy

#### Domain Layer Exceptions

```csharp
public class DomainException : Exception
{
    public string Code { get; }
    
    public DomainException(string message, string code = "DOMAIN_ERROR") 
        : base(message)
    {
        Code = code;
    }
}

// Usage in entity
public class Chef : Entity
{
    public void UpdateExperience(int experience)
    {
        if (experience < 0)
            throw new DomainException("Experience cannot be negative", "INVALID_EXPERIENCE");
        
        Experience = experience;
    }
}
```

#### Application Layer Exception Handling

```csharp
public class ValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }
    
    public ValidationException(Dictionary<string, string[]> errors)
    {
        Errors = errors;
    }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
```

#### API Layer Exception Middleware

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        return exception switch
        {
            DomainException de => RespondWithError(context, 400, de.Code, de.Message),
            ValidationException ve => RespondWithValidationError(context, 400, ve.Errors),
            NotFoundException => RespondWithError(context, 404, "NOT_FOUND", "Resource not found"),
            UnauthorizedAccessException => RespondWithError(context, 401, "UNAUTHORIZED", "Unauthorized access"),
            _ => RespondWithError(context, 500, "INTERNAL_ERROR", "An internal error occurred")
        };
    }
}
```

### Frontend Error Handling Strategy

```typescript
// HTTP Error Interceptor
@Injectable()
export class HttpErrorInterceptor implements HttpInterceptor {
  constructor(private notificationService: NotificationService) {}
  
  intercept(
    request: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        this.handleError(error);
        return throwError(() => error);
      })
    );
  }
  
  private handleError(error: HttpErrorResponse): void {
    if (error.status === 400) {
      this.notificationService.showError('Validation failed', error.error.errors);
    } else if (error.status === 401) {
      this.notificationService.showError('Unauthorized');
    } else if (error.status === 403) {
      this.notificationService.showError('Access denied');
    } else if (error.status >= 500) {
      this.notificationService.showError('Server error', 'Please try again later');
    }
  }
}

// Service error handling
export class ReservationService {
  createReservation(data: CreateReservationDto): Observable<Reservation> {
    return this.http.post<Reservation>('/api/reservations', data).pipe(
      catchError(error => {
        console.error('Failed to create reservation', error);
        return throwError(() => new Error('Reservation creation failed'));
      })
    );
  }
}
```

---

## Testing Strategy

### Testing Approach Overview

The testing strategy implements a three-tier approach:

1. **Unit Tests** (60-70% of tests): Fast, focused tests of individual components with all dependencies mocked
2. **Integration Tests** (20-30% of tests): Tests of multiple components working together, using real or realistic databases
3. **E2E Tests** (5-10% of tests): Tests of critical user workflows through the browser

### Backend Testing Strategy

#### Unit Testing

**Framework**: xUnit + FluentAssertions + Moq

**Coverage Targets**:
- Domain Layer: 85%
- Application Layer: 82%
- Infrastructure Layer: 78%
- API Layer: 80%

**Testing Patterns**:

```csharp
// 1. Arrange-Act-Assert pattern
[Fact]
public void Method_WithValidInput_ShouldReturnExpectedResult()
{
    // Arrange - setup test data and mocks
    var input = CreateValidInput();
    
    // Act - execute the code under test
    var result = SystemUnderTest.Method(input);
    
    // Assert - verify expected outcome
    result.Should().BeOfType<ExpectedType>();
    result.Value.Should().Be(expected);
}

// 2. Theory pattern for multiple inputs
[Theory]
[InlineData(1)]
[InlineData(10)]
[InlineData(100)]
public void Method_WithVariousInputs_ShouldHandleCorrectly(int input)
{
    var result = SystemUnderTest.Method(input);
    result.Should().BeGreaterThan(0);
}

// 3. Moq setup patterns
mockRepository
    .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(testEntity);

mockRepository
    .Verify(x => x.AddAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()), Times.Once);
```

#### Integration Testing

**Framework**: xUnit + WebApplicationFactory + Testcontainers

**Database Strategy**:
- Use Testcontainers for integration tests requiring real database behavior
- Use in-memory SQLite for faster tests with simpler requirements
- Clean up test data after each test using transactions and rollback

**API Integration Testing**:

```csharp
public class ReservationsEndpointIntegrationTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient _client;
    private IDbContext _dbContext;
    
    public async Task InitializeAsync()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
        _dbContext = _factory.Services.GetRequiredService<IDbContext>();
    }
    
    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }
    
    [Fact]
    public async Task CreateReservation_WithValidData_Should Return201()
    {
        // Arrange
        var request = new CreateReservationDto { /* ... */ };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/reservations", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

#### Database Integration Testing

```csharp
public class RepositoryIntegrationTests : IAsyncLifetime
{
    private ApplicationDbContext _dbContext;
    private readonly MsTestDbContextFactory _factory;
    
    public async Task InitializeAsync()
    {
        _factory = new MsTestDbContextFactory();
        _dbContext = await _factory.CreateDbContextAsync();
    }
    
    public async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.DisposeAsync();
    }
    
    [Fact]
    public async Task AddReservation_ThenQueryById_ShouldReturnEntity()
    {
        // Arrange
        var reservation = ReservationBuilder.Default().Build();
        _dbContext.Reservations.Add(reservation);
        await _dbContext.SaveChangesAsync();
        
        // Act
        var result = await _dbContext.Reservations.FirstOrDefaultAsync(x => x.Id == reservation.Id);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(reservation.Id);
    }
}
```

### Frontend Testing Strategy

#### Unit Testing

**Framework**: Jasmine + Karma + TestBed

**Coverage Targets**:
- Services: 80%
- Components (logic only): 75%
- Template/UI logic excluded from coverage gates

**Service Testing Pattern**:

```typescript
describe('ReservationService', () => {
  let service: ReservationService;
  let httpMock: HttpTestingController;
  
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ReservationService]
    });
    service = TestBed.inject(ReservationService);
    httpMock = TestBed.inject(HttpTestingController);
  });
  
  afterEach(() => httpMock.verify());
  
  it('should fetch reservations', () => {
    const mockData: Reservation[] = [
      { id: '1', chefId: '1', reservationDate: new Date(), partySize: 4 }
    ];
    
    service.getReservations().subscribe(data => {
      expect(data.length).toBe(1);
      expect(data[0].id).toBe('1');
    });
    
    const req = httpMock.expectOne('/api/reservations');
    expect(req.request.method).toBe('GET');
    req.flush(mockData);
  });
});
```

**Component Testing Pattern**:

```typescript
describe('ReservationFormComponent', () => {
  let component: ReservationFormComponent;
  let fixture: ComponentFixture<ReservationFormComponent>;
  let mockService: jasmine.SpyObj<ReservationService>;
  
  beforeEach(async () => {
    mockService = jasmine.createSpyObj('ReservationService', [
      'createReservation',
      'getChefs'
    ]);
    
    await TestBed.configureTestingModule({
      declarations: [ReservationFormComponent],
      providers: [{ provide: ReservationService, useValue: mockService }]
    }).compileComponents();
    
    fixture = TestBed.createComponent(ReservationFormComponent);
    component = fixture.componentInstance;
  });
  
  it('should initialize form on load', () => {
    fixture.detectChanges();
    expect(component.form.get('chefId')).toBeDefined();
  });
  
  it('should call service on submit', () => {
    mockService.createReservation.and.returnValue(of({ id: '1' }));
    
    component.form.patchValue({ chefId: '1', partySize: 4 });
    component.onSubmit();
    
    expect(mockService.createReservation).toHaveBeenCalled();
  });
});
```

#### E2E Testing

**Framework**: Cypress + Page Objects

**Test Organization**:

```typescript
// cypress/support/page-objects/reservation.po.ts
export class ReservationPage {
  visit() {
    cy.visit('/reservations');
  }
  
  fillForm(data: ReservationFormData) {
    cy.get('input[name="chefName"]').type(data.chefName);
    cy.get('input[name="partySize"]').type(data.partySize.toString());
    cy.get('input[type="date"]').type(data.date);
  }
  
  submit() {
    cy.get('button[type="submit"]').click();
  }
  
  getSuccessMessage() {
    return cy.contains('Reservation confirmed');
  }
}

// cypress/e2e/reservation.cy.ts
import { ReservationPage } from '../support/page-objects/reservation.po';

describe('Reservation Flow', () => {
  let page: ReservationPage;
  
  beforeEach(() => {
    page = new ReservationPage();
    page.visit();
  });
  
  it('should complete reservation creation', () => {
    page.fillForm({
      chefName: 'Gordon Ramsay',
      partySize: 4,
      date: '2024-12-25'
    });
    
    page.submit();
    page.getSuccessMessage().should('be.visible');
  });
});
```

### Code Coverage Configuration

#### Backend Coverage (.NET)

**Coverlet Configuration** (`coverlet.runsettings`):

```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat Code Coverage" assemblyQualifiedName="Coverlet.Collector.DataCollection.CoverletInstrumentationCollector, coverlet.collector">
        <Configuration>
          <Format>cobertura</Format>
          <IncludeTestAssembly>false</IncludeTestAssembly>
          <Exclude>[NaarNoor.*.Tests]*,[*.Tests]*</Exclude>
          <ExcludeByAttribute>System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute</ExcludeByAttribute>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
```

**Coverage Reporting**:

```bash
# Run tests with coverage
dotnet test --no-build --settings coverlet.runsettings /p:CollectCoverage=true

# Generate HTML report
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
```

#### Frontend Coverage (Angular)

**Coverage Configuration** (`karma.conf.js`):

```javascript
coverageReporter: {
  dir: require('path').join(__dirname, './coverage'),
  reports: ['html', 'lcovonly', 'cobertura', 'json'],
  fixWebpackSourcePaths: true,
  thresholds: {
    emitWarning: false,
    global: {
      statements: 80,
      branches: 75,
      functions: 80,
      lines: 80
    }
  }
},

// Run tests with coverage
// ng test --code-coverage --watch=false
```

### Continuous Integration Integration

#### GitHub Actions Workflow

```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '7.x'
      
      - name: Run backend tests
        run: |
          dotnet test api-server/tests --settings coverlet.runsettings \
            /p:CollectCoverage=true \
            /p:CoverageFormat=cobertura \
            /p:Exclude="[*.Tests]*"
      
      - name: Upload backend coverage
        uses: codecov/codecov-action@v3
        with:
          files: '**/coverage.cobertura.xml'
          flags: backend
      
      - name: Setup Node
        uses: actions/setup-node@v3
        with:
          node-version: '18'
      
      - name: Install frontend dependencies
        run: cd frontend && npm ci
      
      - name: Run frontend tests
        run: cd frontend && npm run test:ci
      
      - name: Upload frontend coverage
        uses: codecov/codecov-action@v3
        with:
          files: 'frontend/coverage/lcov.info'
          flags: frontend
      
      - name: Check coverage gates
        run: |
          # Fail if coverage below threshold
          if grep -q "line-rate=\"[0-6]" backend-coverage.xml; then
            echo "Backend coverage below 80%"
            exit 1
          fi
```

---

## Implementation Patterns

### Test Data Builders

**Builder Pattern for Complex Entities**:

```csharp
public class ReservationBuilder
{
    private Guid _chefId = Guid.NewGuid();
    private DateTime _reservationDate = DateTime.Now.AddDays(7);
    private int _partySize = 4;
    private ReservationStatus _status = ReservationStatus.Pending;
    
    public static ReservationBuilder Default() => new();
    
    public ReservationBuilder WithChefId(Guid chefId)
    {
        _chefId = chefId;
        return this;
    }
    
    public ReservationBuilder WithPartySize(int size)
    {
        _partySize = size;
        return this;
    }
    
    public ReservationBuilder WithStatus(ReservationStatus status)
    {
        _status = status;
        return this;
    }
    
    public Reservation Build()
    {
        return new Reservation(_chefId, _reservationDate, _partySize)
        {
            Status = _status
        };
    }
}

// Usage
var reservation = ReservationBuilder.Default()
    .WithPartySize(8)
    .WithStatus(ReservationStatus.Confirmed)
    .Build();
```

### Mock Configuration Factories

```csharp
public static class MockFactory
{
    public static Mock<IReservationRepository> CreateReservationRepositoryMock(
        Reservation defaultEntity = null)
    {
        var mock = new Mock<IReservationRepository>();
        
        mock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(defaultEntity ?? ReservationBuilder.Default().Build());
        
        return mock;
    }
    
    public static Mock<IChefRepository> CreateChefRepositoryMock(
        Chef defaultEntity = null)
    {
        var mock = new Mock<IChefRepository>();
        
        mock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(defaultEntity ?? ChefBuilder.Default().Build());
        
        return mock;
    }
}

// Usage
var mockReservationRepository = MockFactory.CreateReservationRepositoryMock();
```

### Test Base Classes

```csharp
public abstract class ApplicationLayerTestBase
{
    protected Mock<IReservationRepository> ReservationRepositoryMock { get; }
    protected Mock<IChefRepository> ChefRepositoryMock { get; }
    protected Mock<IUnitOfWork> UnitOfWorkMock { get; }
    
    public ApplicationLayerTestBase()
    {
        ReservationRepositoryMock = MockFactory.CreateReservationRepositoryMock();
        ChefRepositoryMock = MockFactory.CreateChefRepositoryMock();
        UnitOfWorkMock = new Mock<IUnitOfWork>();
    }
}

// Usage
public class CreateReservationCommandTests : ApplicationLayerTestBase
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldSucceed()
    {
        var handler = new CreateReservationCommandHandler(
            ReservationRepositoryMock.Object,
            ChefRepositoryMock.Object
        );
        
        // Test body
    }
}
```

---

## Documentation & Guidelines

### Test File Organization

**Naming Convention**:
- Test classes: `[ComponentUnderTest]Tests.cs` or `[Feature][Layer]Tests.cs`
- Test methods: `[MethodName]_[Condition]_[ExpectedResult]`
- Example: `CreateReservation_WithValidData_ShouldReturnId`

**File Placement**:
- Mirror source project structure in test projects
- Example: `src/NaarNoor.Application/Reservations/Handlers/CreateReservationCommandHandler.cs` → `tests/NaarNoor.Application.Tests/Reservations/Handlers/CreateReservationCommandHandlerTests.cs`

### Common Testing Patterns

#### Pattern 1: Happy Path Testing

```csharp
[Fact]
public async Task ValidOperation_ShouldComplete()
{
    // Arrange - setup with valid data
    var validEntity = EntityBuilder.Default().Build();
    
    // Act - perform the operation
    await SystemUnderTest.PerformOperation(validEntity);
    
    // Assert - verify success
    mockRepository.Verify(x => x.AddAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()), Times.Once);
}
```

#### Pattern 2: Error Scenario Testing

```csharp
[Theory]
[InlineData("")]
[InlineData(null)]
[InlineData("   ")]
public void InvalidInput_ShouldThrow(string invalidInput)
{
    // Act & Assert
    Action act = () => SystemUnderTest.ProcessInput(invalidInput);
    act.Should().Throw<DomainException>();
}
```

#### Pattern 3: Behavior Verification

```csharp
[Fact]
public void OperationWithSideEffects_ShouldCallDependencies()
{
    // Arrange
    var mock = new Mock<IDependency>();
    var sut = new ClassUnderTest(mock.Object);
    
    // Act
    sut.PerformOperation();
    
    // Assert - verify the dependency was called
    mock.Verify(x => x.DoSomething(It.IsAny<Param>()), Times.Once);
}
```

### Troubleshooting Common Issues

#### Issue: Flaky Tests

**Cause**: Timing-dependent tests, shared state, non-deterministic behavior

**Solution**:
- Use `IAsyncLifetime` for proper async setup/teardown
- Clear database state between tests
- Avoid hardcoded delays; use proper async/await patterns
- Use testcontainers for database isolation

#### Issue: Slow Tests

**Cause**: Database access, external service calls, large test data

**Solution**:
- Use in-memory substitutes for integration tests
- Mock external service calls
- Build smaller test data sets
- Run unit tests separately from integration tests
- Run integration tests in parallel

#### Issue: Mock Configuration Errors

**Cause**: Incorrect mock setup, missing returns configuration

**Solution**:
- Use strict mocks to catch unexpected calls
- Review Moq setup patterns
- Verify mock.Verify() calls match actual calls
- Use InlineData for multiple mock configurations

---

## Summary

The comprehensive testing framework for Naar & Noor establishes a multi-layer testing strategy that ensures code quality, maintainability, and confidence across both backend and frontend systems. By implementing unit tests, integration tests, and E2E tests with consistent patterns, builders, and mocking strategies, the framework enables developers to catch regressions early while maintaining fast test execution and high code coverage on critical business logic.

Key success factors:
1. Clear separation of testing layers with appropriate tools and patterns
2. Reusable test data builders and mock factories
3. Strong base classes and test organization
4. Automated coverage measurement and gating
5. Integration with CI/CD pipelines for continuous validation
6. Comprehensive documentation and guidelines for consistency

The framework supports >80% code coverage on core business logic while maintaining an efficient testing pyramid that balances coverage with execution speed and maintainability.
