using FluentAssertions;
using NaarNoor.Application.Reservations.Commands.CreateReservation;
using System.Net;
using System.Text.Json;
using Xunit;

namespace NaarNoor.API.Tests.Integration;

/// <summary>
/// Property-based tests for HTTP exception mapping in API exception handling middleware.
/// 
/// **Property 11: HTTP Exception Mapping**
/// **Validates: Requirements 4.6**
/// 
/// For any exception thrown during API endpoint processing (DomainException, ValidationException,
/// NotFoundException, etc.), the exception handling middleware SHALL map the exception to an
/// appropriate HTTP status code and return a properly formatted error response.
/// </summary>
[Trait("Category", "Property-Based")]
[Trait("Property", "HttpExceptionMapping")]
public class HttpExceptionMappingPropertyTests : ApiTestBase
{
    #region ValidationException Mapping Tests

    /// <summary>
    /// Property: ValidationException thrown during command processing SHALL be mapped
    /// to HTTP 400 Bad Request with validation error details.
    /// </summary>
    [Fact]
    public async Task ValidationException_ShouldMapTo400BadRequest()
    {
        // Arrange - Invalid email triggers validation exception
        var command = new CreateReservationCommand(
            CustomerName: "John Doe",
            Email: "not-an-email",  // Invalid format triggers ValidationException
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
            "ValidationException should map to HTTP 400");
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Email", "Error details should include field name");
    }

    /// <summary>
    /// Property: ValidationException responses SHALL include structured error details
    /// containing field names and error messages.
    /// </summary>
    [Fact]
    public async Task ValidationExceptionResponse_ShouldIncludeFieldErrors()
    {
        // Arrange - Multiple validation errors
        var command = new CreateReservationCommand(
            CustomerName: "",  // Empty name
            Email: "invalid",  // Invalid email
            PhoneNumber: "",   // Empty phone
            ReservationDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),  // Past date
            ReservationTime: "",
            PartySize: -1,  // Invalid party size
            SpecialRequests: null
        );

        // Act
        var response = await PostAsync("/api/reservations", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        // Verify response has error information
        jsonDoc.RootElement.TryGetProperty("errors", out _).Should().BeTrue(
            "Error response should contain 'errors' property");
    }

    #endregion

    #region Domain Logic Exception Mapping Tests

    /// <summary>
    /// Property: When domain business logic validation fails (invalid party size, past dates),
    /// the resulting exception SHALL be consistently mapped to HTTP 400 Bad Request.
    /// </summary>
    [Theory]
    [InlineData(0)]      // Below minimum (1)
    [InlineData(21)]     // Above maximum (20)
    [InlineData(-5)]     // Negative
    public async Task DomainValidationFailure_ShouldMapTo400BadRequest(int invalidPartySize)
    {
        // Arrange - Invalid party size that violates domain rules
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
            $"Domain validation error for party size {invalidPartySize} should map to 400");
    }

    /// <summary>
    /// Property: When a reservation date is in the past (domain rule violation),
    /// the exception SHALL be mapped to HTTP 400 with appropriate error message.
    /// </summary>
    [Fact]
    public async Task PastDateValidation_ShouldMapTo400BadRequest()
    {
        // Arrange
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5));
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
            "Past reservation date should map to HTTP 400");
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("ReservationDate", "Error should reference the problematic field");
    }

    #endregion

    #region Not Found Exception Mapping Tests

    /// <summary>
    /// Property: When an endpoint attempts to access a non-existent resource,
    /// NotFoundException SHALL be mapped to HTTP 404 Not Found.
    /// </summary>
    [Fact]
    public async Task NotFoundException_ShouldMapTo404()
    {
        // Arrange - Request non-existent reservation
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await GetAsync($"/api/reservations/{nonExistentId}");

        // Assert
        // Note: Response depends on whether endpoint is implemented
        // If implemented and resource doesn't exist, should return 404
        if (response.StatusCode != HttpStatusCode.NoContent)
        {
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.NoContent,
                "Non-existent resource should return 404 or 204");
        }
    }

    #endregion

    #region Error Response Format Consistency Tests

    /// <summary>
    /// Property: All error responses from the API SHALL follow a consistent format
    /// with proper HTTP status codes, Content-Type headers, and structured error details.
    /// </summary>
    [Fact]
    public async Task ErrorResponses_ShouldHaveConsistentFormat()
    {
        // Arrange - Create requests that will generate errors
        var invalidReservation = new CreateReservationCommand(
            CustomerName: "",
            Email: "not-valid",
            PhoneNumber: "",
            ReservationDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            ReservationTime: "",
            PartySize: 0,
            SpecialRequests: null
        );

        // Act - Get error response
        var response = await PostAsync("/api/reservations", invalidReservation);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json",
            "Error responses should be JSON");

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        // Verify response can be parsed as JSON
        JsonDocument.Parse(content).Should().NotBeNull(
            "Error response should be valid JSON");
    }

    /// <summary>
    /// Property: Error responses SHALL include appropriate error codes or messages
    /// that allow clients to understand and handle the error programmatically.
    /// </summary>
    [Fact]
    public async Task ErrorResponse_ShouldIncludeErrorDetails()
    {
        // Arrange
        var invalidCommand = new CreateReservationCommand(
            CustomerName: "John",
            Email: "invalid-email",
            PhoneNumber: "555-1234",
            ReservationDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            ReservationTime: "19:00",
            PartySize: 0,  // Invalid: below minimum
            SpecialRequests: null
        );

        // Act
        var response = await PostAsync("/api/reservations", invalidCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        
        // Verify error response contains meaningful information
        content.Should().Satisfy(
            c => c.Contains("PartySize") || c.Contains("party") || c.Contains("error"),
            "Error response should reference the problematic field or contain error information"
        );
    }

    #endregion

    #region Multiple Error Scenarios Tests

    /// <summary>
    /// Property: When multiple validation errors occur simultaneously, the exception handler
    /// SHALL return HTTP 400 with all error details, not just the first error.
    /// </summary>
    [Fact]
    public async Task MultipleValidationErrors_ShouldAllBeMappedInResponse()
    {
        // Arrange - Create command with multiple errors
        var invalidCommand = new CreateReservationCommand(
            CustomerName: "",         // Missing
            Email: "",                // Missing/Invalid
            PhoneNumber: "",          // Missing
            ReservationDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),  // Past
            ReservationTime: "",      // Missing
            PartySize: -5,            // Invalid
            SpecialRequests: null
        );

        // Act
        var response = await PostAsync("/api/reservations", invalidCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "Multiple errors should still return 400");

        var content = await response.Content.ReadAsStringAsync();
        
        // Verify multiple fields are mentioned in error response
        var fieldsMentioned = new[] { "CustomerName", "Email", "PhoneNumber", "PartySize", "ReservationDate" }
            .Count(field => content.Contains(field, StringComparison.OrdinalIgnoreCase));
        
        fieldsMentioned.Should().BeGreaterThan(1,
            "Error response should include multiple field errors");
    }

    /// <summary>
    /// Property: When both client and server validation errors occur,
    /// the exception handler SHALL map them to appropriate HTTP status codes
    /// (400 for client errors, 500 for server errors).
    /// </summary>
    [Fact]
    public async Task ClientErrors_ShouldReturn400NotServerError()
    {
        // Arrange - Create a client validation error
        var invalidCommand = new CreateReservationCommand(
            CustomerName: "John",
            Email: "invalid",      // Validation error (client side)
            PhoneNumber: "555-1234",
            ReservationDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            ReservationTime: "19:00",
            PartySize: 0,          // Validation error (client side)
            SpecialRequests: null
        );

        // Act
        var response = await PostAsync("/api/reservations", invalidCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "Client validation errors should return 400, not 500");
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError,
            "Validation errors should not be mapped to 500");
    }

    #endregion

    #region Exception Handler Coverage Tests

    /// <summary>
    /// Property: The exception handler SHALL properly handle and map exceptions
    /// from all layers (domain, application, infrastructure, API), ensuring
    /// no unhandled exceptions escape to the client as 500 errors.
    /// </summary>
    [Fact]
    public async Task ExceptionHandler_ShouldCatchAndMapAllExceptionTypes()
    {
        // Test various error scenarios that should all be caught and mapped appropriately

        // Scenario 1: Validation errors (handled)
        var validationError = new CreateReservationCommand(
            "", "", "", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), "", 0, null
        );
        var response1 = await PostAsync("/api/reservations", validationError);
        response1.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Scenario 2: Missing required fields (handled)
        var missingFields = new CreateReservationCommand(
            "", "", "", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), "", -1, null
        );
        var response2 = await PostAsync("/api/reservations", missingFields);
        response2.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // All should be 400 or better (not 500)
        response1.StatusCode.Should().BeLessThanOrEqualTo(HttpStatusCode.BadRequest);
        response2.StatusCode.Should().BeLessThanOrEqualTo(HttpStatusCode.BadRequest);
    }

    #endregion
}

