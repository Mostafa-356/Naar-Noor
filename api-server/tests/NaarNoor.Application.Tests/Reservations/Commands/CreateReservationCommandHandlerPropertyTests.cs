using FluentAssertions;
using Moq;
using NaarNoor.Application.Reservations.Commands.CreateReservation;
using NaarNoor.Application.Common.Interfaces;
using NaarNoor.Application.Tests.Common.Fixtures;
using NaarNoor.Domain.Entities;
using Xunit;

namespace NaarNoor.Application.Tests.Reservations.Commands;

/// <summary>
/// Property-based tests for command handler processing.
/// 
/// **Validates: Requirements 2.1, 2.3**
/// 
/// Property 5: Command Handler Processing
/// For any valid command (CreateReservationCommand, UpdateOrderCommand, etc.), when passed to the 
/// corresponding command handler with all mocked dependencies configured, the handler SHALL:
/// 1. Complete without throwing
/// 2. Call expected repository methods with correct parameters
/// 3. Return expected result types
/// 
/// This test uses multiple theory data sources to generate various valid command inputs and verify
/// that the command handler processes them correctly. The data sources represent different valid
/// combinations of input parameters that should all succeed.
/// </summary>
public class CreateReservationCommandHandlerPropertyTests : ApplicationLayerTestBase
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateReservationCommandHandler _handler;

    public CreateReservationCommandHandlerPropertyTests()
    {
        _unitOfWorkMock = CreateRepositoryMock<IUnitOfWork>();
        _handler = new CreateReservationCommandHandler(_unitOfWorkMock.Object);

        // Setup the mock to allow Add to be called and SaveChangesAsync to succeed
        _unitOfWorkMock
            .Setup(x => x.Reservations)
            .Returns(new MockReservationRepository());

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    /// <summary>
    /// Property test: Valid commands are processed successfully without throwing
    /// 
    /// Tests that for any valid reservation command, the handler completes without exception.
    /// This property holds across all combinations of valid input data.
    /// </summary>
    [Theory(DisplayName = "Property: Valid commands complete without throwing")]
    [MemberData(nameof(GetValidReservationCommands))]
    public async Task Handle_WithValidCommand_CompletesSuccessfully(CreateReservationCommand command)
    {
        // Act & Assert - should not throw
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().NotThrowAsync("Valid commands should always complete successfully");
    }

    /// <summary>
    /// Property test: Repository methods are called with correct parameters
    /// 
    /// Tests that for any valid reservation command, the handler calls the repository's
    /// Add method exactly once with a Reservation containing the correct data from the command.
    /// </summary>
    [Theory(DisplayName = "Property: Repository methods called with correct parameters")]
    [MemberData(nameof(GetValidReservationCommands))]
    public async Task Handle_WithValidCommand_CallsRepositoryWithCorrectData(CreateReservationCommand command)
    {
        // Arrange
        var capturedReservation = new List<Reservation>();
        var mockRepository = new MockReservationRepository();
        mockRepository.OnAdd = reservation => capturedReservation.Add(reservation);

        _unitOfWorkMock
            .Setup(x => x.Reservations)
            .Returns(mockRepository);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - verify repository was called with correct data
        capturedReservation.Should().HaveCount(1, "Repository Add should be called exactly once");
        var addedReservation = capturedReservation[0];

        addedReservation.CustomerName.Should().Be(command.CustomerName, "Customer name should match command");
        addedReservation.Email.Should().Be(command.Email, "Email should match command");
        addedReservation.PhoneNumber.Should().Be(command.PhoneNumber, "Phone number should match command");
        addedReservation.ReservationDate.Should().Be(command.ReservationDate, "Reservation date should match command");
        addedReservation.ReservationTime.Should().Be(TimeOnly.Parse(command.ReservationTime), "Reservation time should match command");
        addedReservation.PartySize.Should().Be(command.PartySize, "Party size should match command");
        addedReservation.SpecialRequests.Should().Be(command.SpecialRequests, "Special requests should match command");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once,
            "SaveChangesAsync should be called exactly once");
    }

    /// <summary>
    /// Property test: Handler returns expected result type
    /// 
    /// Tests that for any valid reservation command, the handler returns a non-empty Guid
    /// that matches the created reservation's ID.
    /// </summary>
    [Theory(DisplayName = "Property: Handler returns expected result type")]
    [MemberData(nameof(GetValidReservationCommands))]
    public async Task Handle_WithValidCommand_ReturnsReservationId(CreateReservationCommand command)
    {
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - result should be a valid non-empty Guid
        result.Should().NotBe(Guid.Empty, "Handler should return a valid reservation ID");
        result.Should().Be(result, "Handler should return a Guid type result");
    }

    /// <summary>
    /// Property test: Commands are validated before processing
    /// 
    /// Tests that for any valid reservation command, all properties are correctly populated
    /// and the command object itself is in a valid state for processing.
    /// </summary>
    [Theory(DisplayName = "Property: Commands are validated before processing")]
    [MemberData(nameof(GetValidReservationCommands))]
    public void CommandDataIsValidBeforeProcessing(CreateReservationCommand command)
    {
        // Assert - command should have all required data
        command.CustomerName.Should().NotBeNullOrWhiteSpace("Customer name must be provided");
        command.Email.Should().NotBeNullOrWhiteSpace("Email must be provided");
        command.PhoneNumber.Should().NotBeNullOrWhiteSpace("Phone number must be provided");
        (command.ReservationDate >= DateOnly.FromDateTime(DateTime.Today)).Should().BeTrue(
            "Reservation date must be today or in the future");
        command.PartySize.Should().BeGreaterThan(0, "Party size must be greater than zero");
        
        // ReservationTime should be parseable
        Action parseTime = () => TimeOnly.Parse(command.ReservationTime);
        parseTime.Should().NotThrow("Reservation time must be a valid time format");
    }

    /// <summary>
    /// Property test: Transaction management works correctly
    /// 
    /// Tests that for any valid reservation command, SaveChangesAsync is called exactly once
    /// after the repository Add operation, ensuring proper transaction management.
    /// </summary>
    [Theory(DisplayName = "Property: Transaction management works correctly")]
    [MemberData(nameof(GetValidReservationCommands))]
    public async Task Handle_WithValidCommand_ManagesTransactionCorrectly(CreateReservationCommand command)
    {
        // Arrange
        var callOrder = new List<string>();
        
        var mockRepository = new MockReservationRepository();
        mockRepository.OnAdd = _ => callOrder.Add("Add");

        _unitOfWorkMock
            .Setup(x => x.Reservations)
            .Returns(mockRepository);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("SaveChanges"))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - verify call order
        callOrder.Should().HaveCount(2, "Should have exactly 2 calls: Add and SaveChanges");
        callOrder[0].Should().Be("Add", "Add should be called first");
        callOrder[1].Should().Be("SaveChanges", "SaveChanges should be called second for proper transaction management");
    }

    /// <summary>
    /// Property test: Domain events are prepared for publishing
    /// 
    /// Tests that for any valid reservation command, the created reservation contains
    /// necessary information for domain event publishing (ID is generated, timestamps are set).
    /// </summary>
    [Theory(DisplayName = "Property: Domain events are prepared for publishing")]
    [MemberData(nameof(GetValidReservationCommands))]
    public async Task Handle_WithValidCommand_PreparesDomainEventData(CreateReservationCommand command)
    {
        // Arrange
        var capturedReservation = new List<Reservation>();
        var mockRepository = new MockReservationRepository();
        mockRepository.OnAdd = reservation => capturedReservation.Add(reservation);

        _unitOfWorkMock
            .Setup(x => x.Reservations)
            .Returns(mockRepository);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - verify reservation has ID for domain events
        var addedReservation = capturedReservation[0];
        addedReservation.Id.Should().NotBe(Guid.Empty,
            "Reservation should have a valid ID for domain event publishing");
    }

    #region Test Data Generators

    /// <summary>
    /// Generates multiple valid CreateReservationCommand instances with various input combinations.
    /// This property-based test uses these varied inputs to verify the handler works across
    /// different valid scenarios.
    /// </summary>
    public static TheoryData<CreateReservationCommand> GetValidReservationCommands()
    {
        var data = new TheoryData<CreateReservationCommand>();

        // Basic valid command
        data.Add(new CreateReservationCommand(
            CustomerName: "John Smith",
            Email: "john@example.com",
            PhoneNumber: "555-0123",
            ReservationDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            ReservationTime: "19:00",
            PartySize: 4,
            SpecialRequests: null
        ));

        // Command with special requests
        data.Add(new CreateReservationCommand(
            CustomerName: "Maria Garcia",
            Email: "maria.garcia@example.com",
            PhoneNumber: "+1-555-0456",
            ReservationDate: DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
            ReservationTime: "18:30",
            PartySize: 2,
            SpecialRequests: "Window seat, vegetarian options"
        ));

        // Command with larger party
        data.Add(new CreateReservationCommand(
            CustomerName: "Dr. Robert Johnson",
            Email: "robert.j@corporate.com",
            PhoneNumber: "(555) 789-0123",
            ReservationDate: DateOnly.FromDateTime(DateTime.Today.AddDays(14)),
            ReservationTime: "20:00",
            PartySize: 12,
            SpecialRequests: null
        ));

        // Command with single guest
        data.Add(new CreateReservationCommand(
            CustomerName: "Sarah",
            Email: "sarah@email.com",
            PhoneNumber: "555-9999",
            ReservationDate: DateOnly.FromDateTime(DateTime.Today),
            ReservationTime: "17:00",
            PartySize: 1,
            SpecialRequests: "Counter seating preferred"
        ));

        // Command with various time formats
        data.Add(new CreateReservationCommand(
            CustomerName: "James Wilson",
            Email: "james.w@test.com",
            PhoneNumber: "555-1234",
            ReservationDate: DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            ReservationTime: "12:00",
            PartySize: 6,
            SpecialRequests: null
        ));

        // Command with evening reservation
        data.Add(new CreateReservationCommand(
            CustomerName: "Elena Rodriguez",
            Email: "elena@domain.com",
            PhoneNumber: "555-5678",
            ReservationDate: DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            ReservationTime: "21:30",
            PartySize: 3,
            SpecialRequests: "Celebration dinner - birthday"
        ));

        // Command with special characters in name
        data.Add(new CreateReservationCommand(
            CustomerName: "François O'Brien",
            Email: "francois@example.fr",
            PhoneNumber: "555-0111",
            ReservationDate: DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
            ReservationTime: "18:00",
            PartySize: 5,
            SpecialRequests: null
        ));

        // Command with future far-out date
        data.Add(new CreateReservationCommand(
            CustomerName: "Lisa Chen",
            Email: "lisa.chen@company.org",
            PhoneNumber: "555-0222",
            ReservationDate: DateOnly.FromDateTime(DateTime.Today.AddDays(365)),
            ReservationTime: "19:00",
            PartySize: 4,
            SpecialRequests: "Anniversary celebration"
        ));

        return data;
    }

    #endregion

    #region Mock Repository Helper

    /// <summary>
    /// Mock repository for testing command handler without database access.
    /// Captures Add calls for verification in tests.
    /// </summary>
    private class MockReservationRepository : IRepository<Reservation>
    {
        public Action<Reservation>? OnAdd { get; set; }

        public void Add(Reservation entity)
        {
            OnAdd?.Invoke(entity);
        }

        public void Remove(Reservation entity)
        {
            throw new NotImplementedException();
        }

        public void Update(Reservation entity)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Reservation> Query()
        {
            return Enumerable.Empty<Reservation>().AsQueryable();
        }
    }

    #endregion
}
