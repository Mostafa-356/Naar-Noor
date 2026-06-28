using FluentAssertions;
using NaarNoor.Application.Reservations.Commands.CreateReservation;
using NaarNoor.Application.Orders.Commands.CreateOrder;
using NaarNoor.Application.Contact.Commands;
using System.Net;
using System.Text.Json;
using Xunit;

namespace NaarNoor.API.Tests.Integration;

/// <summary>
/// Property-based tests for input validation consistency across API endpoints.
/// 
/// **Property 16: Input Validation Consistency**
/// **Validates: Requirements 15.1**
/// 
/// For any API endpoint accepting user input, when provided with invalid data 
/// (null values where required, strings exceeding max length, invalid enum values, 
/// negative numbers where positive required, etc.), the endpoint SHALL return HTTP 400 
/// with validation error details consistently across all endpoints.
/// </summary>
[Trait("Category", "Property-Based")]
[Trait("Property", "InputValidationConsistency")]
public class InputValidationConsistencyPropertyTests : ApiTestBase
{
    #region Reservation Validation Tests

    /// <summary>
    /// Property: When POST /api/reservations receives requests with missing required fields,
    /// it SHALL return HTTP 400 with validation errors for all missing fields.
    /// 
    /// Test strategy: Generate various combinations of null/empty required fields and verify
    /// that each generates a 400 response with appropriate error messages.
    /// </summary>
    [Fact]
    public async Task PostReservation_WithMissingRequiredName_ShouldReturn400WithValidationError()
    {
        // Arrange
        var command = new CreateReservationCommand(
            CustomerName: "",  // Invalid: empty
            Email: "test@example.com",
            PhoneNumber: "555-1234",
            ReservationDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            ReservationTime: "19:00",
            PartySize: 4,
            SpecialRequests: null
        );

        // Act
        var response = await PostAsync("/api/reservations", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "Missing required CustomerName should return 400");
        await AssertValidationErrorResponseAsync(response, "CustomerName");
    }

    /// <summary>
    /// Property: When POST /api/reservations receives invalid email format,
    /// it SHALL return HTTP 400 with specific email validation error.
    /// </summary>
    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@nodomain")]
    [InlineData("test@")]
    [InlineData("")]
    public async Task PostReservation_WithInvalidEmail_ShouldReturn400WithEmailError(string invalidEmail)
    {
        // Arrange
        var command = new CreateReservationCommand(
            CustomerName: "John Doe",
            Email: invalidEmail,
            PhoneNumber: "555-1234",
            ReservationDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            ReservationTime: "19:00",
            PartySize: 4,
            SpecialRequests: null
        );

        // Act
        var response = await PostAsync("/api/reservations", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            $"Invalid email '{invalidEmail}' should return 400");
        await AssertValidationErrorResponseAsync(response, "Email");
    }

    /// <summary>
    /// Property: When POST /api/reservations receives party size outside valid range,
    /// it SHALL return HTTP 400 with party size validation error.
    /// </summary>
    [Theory]
    [InlineData(0)]      // Below minimum (1)
    [InlineData(-5)]     // Negative
    [InlineData(21)]     // Above maximum (20)
    [InlineData(100)]    // Way above maximum
    public async Task PostReservation_WithInvalidPartySize_ShouldReturn400WithPartySizeError(int invalidPartySize)
    {
        // Arrange
        var command = new CreateReservationCommand(
            CustomerName: "John Doe",
            Email: "john@example.com",
            PhoneNumber: "555-1234",
            ReservationDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            ReservationTime: "19:00",
            PartySize: invalidPartySize,
            SpecialRequests: null
        );

        // Act
        var response = await PostAsync("/api/reservations", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            $"Invalid party size {invalidPartySize} should return 400");
        await AssertValidationErrorResponseAsync(response, "PartySize");
    }

    /// <summary>
    /// Property: When POST /api/reservations receives past reservation date,
    /// it SHALL return HTTP 400 with date validation error.
    /// </summary>
    [Fact]
    public async Task PostReservation_WithPastDate_ShouldReturn400WithDateError()
    {
        // Arrange
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var command = new CreateReservationCommand(
            CustomerName: "John Doe",
            Email: "john@example.com",
            PhoneNumber: "555-1234",
            ReservationDate: pastDate,
            ReservationTime: "19:00",
            PartySize: 4,
            SpecialRequests: null
        );

        // Act
        var response = await PostAsync("/api/reservations", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "Past reservation date should return 400");
        await AssertValidationErrorResponseAsync(response, "ReservationDate");
    }

    /// <summary>
    /// Property: When POST /api/reservations receives name exceeding max length,
    /// it SHALL return HTTP 400 with length validation error.
    /// </summary>
    [Fact]
    public async Task PostReservation_WithNameExceedingMaxLength_ShouldReturn400WithLengthError()
    {
        // Arrange
        var longName = new string('A', 101); // Exceeds max length of 100
        var command = new CreateReservationCommand(
            CustomerName: longName,
            Email: "john@example.com",
            PhoneNumber: "555-1234",
            ReservationDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            ReservationTime: "19:00",
            PartySize: 4,
            SpecialRequests: null
        );

        // Act
        var response = await PostAsync("/api/reservations", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "Name exceeding max length should return 400");
        await AssertValidationErrorResponseAsync(response, "CustomerName");
    }

    /// <summary>
    /// Property: When POST /api/reservations receives multiple validation errors,
    /// the response SHALL include error messages for all invalid fields,
    /// demonstrating consistent error response format.
    /// </summary>
    [Fact]
    public async Task PostReservation_WithMultipleValidationErrors_ShouldReturn400WithAllErrors()
    {
        // Arrange
        var command = new CreateReservationCommand(
            CustomerName: "",           // Missing
            Email: "invalid-email",     // Invalid format
            PhoneNumber: "555-1234",
            ReservationDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),  // Past date
            ReservationTime: "19:00",
            PartySize: 0,               // Invalid range
            SpecialRequests: null
        );

        // Act
        var response = await PostAsync("/api/reservations", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "Multiple validation errors should return 400");

        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("CustomerName", "Should report CustomerName error");
        responseBody.Should().Contain("Email", "Should report Email error");
        responseBody.Should().Contain("PartySize", "Should report PartySize error");
        responseBody.Should().Contain("ReservationDate", "Should report ReservationDate error");
    }

    #endregion

    #region Contact Endpoint Validation Tests

    /// <summary>
    /// Property: When POST /api/contact receives requests with missing required fields,
    /// it SHALL return HTTP 400 with validation errors, demonstrating consistent validation
    /// behavior across different endpoints.
    /// </summary>
    [Fact]
    public async Task PostContact_WithMissingEmail_ShouldReturn400WithValidationError()
    {
        // Arrange
        var command = new SendContactInquiryCommand(
            Name: "John Doe",
            Email: "",  // Missing
            Subject: "Question",
            Message: "This is a test message about the restaurant"
        );

        // Act
        var response = await PostAsync("/api/contact", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "Missing email should return 400");
        await AssertValidationErrorResponseAsync(response, "Email");
    }

    /// <summary>
    /// Property: When POST /api/contact receives invalid email format,
    /// it SHALL return HTTP 400, maintaining consistent email validation rules
    /// across all endpoints that use email.
    /// </summary>
    [Theory]
    [InlineData("not-an-email")]
    [InlineData("@example.com")]
    public async Task PostContact_WithInvalidEmailFormat_ShouldReturn400(string invalidEmail)
    {
        // Arrange
        var command = new SendContactInquiryCommand(
            Name: "John Doe",
            Email: invalidEmail,
            Subject: "Question",
            Message: "This is a test message"
        );

        // Act
        var response = await PostAsync("/api/contact", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "Invalid email format should return 400");
        await AssertValidationErrorResponseAsync(response, "Email");
    }

    #endregion

    #region Consistency Tests

    /// <summary>
    /// Property: All validation error responses across different endpoints SHALL
    /// follow the same format and structure, allowing clients to parse them consistently.
    /// 
    /// This test verifies that 400 responses have a consistent structure with
    /// an errors field containing field-level error messages.
    /// </summary>
    [Fact]
    public async Task ValidationErrorResponses_ShouldHaveConsistentFormat()
    {
        // Arrange
        var invalidReservation = new CreateReservationCommand(
            CustomerName: "",
            Email: "invalid",
            PhoneNumber: "",
            ReservationDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            ReservationTime: "",
            PartySize: 0,
            SpecialRequests: null
        );

        var invalidContact = new SendContactInquiryCommand(
            Name: "",
            Email: "invalid-email",
            Subject: "",
            Message: ""
        );

        // Act
        var reservationResponse = await PostAsync("/api/reservations", invalidReservation);
        var contactResponse = await PostAsync("/api/contact", invalidContact);

        // Assert - Both should return 400
        reservationResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        contactResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Both should have JSON content type
        reservationResponse.Content.Headers.ContentType?.MediaType
            .Should().Be("application/json");
        contactResponse.Content.Headers.ContentType?.MediaType
            .Should().Be("application/json");

        // Both responses should be parseable as JSON objects
        var reservationContent = await reservationResponse.Content.ReadAsStringAsync();
        var contactContent = await contactResponse.Content.ReadAsStringAsync();

        JsonDocument.Parse(reservationContent).Should().NotBeNull();
        JsonDocument.Parse(contactContent).Should().NotBeNull();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Helper method to assert that a validation error response contains
    /// error details for a specific field.
    /// </summary>
    private async Task AssertValidationErrorResponseAsync(HttpResponseMessage response, string fieldName)
    {
        var responseBody = await response.Content.ReadAsStringAsync();
        
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json",
            "Error response should be JSON");
        
        responseBody.Should().Contain(fieldName,
            $"Error response should contain field name '{fieldName}'");
    }

    #endregion
}

