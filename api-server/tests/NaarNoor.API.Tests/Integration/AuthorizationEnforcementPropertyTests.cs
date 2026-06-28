using FluentAssertions;
using NaarNoor.Application.Reservations.Commands.CreateReservation;
using NaarNoor.Application.Orders.Commands.CreateOrder;
using System.Net;
using Xunit;

namespace NaarNoor.API.Tests.Integration;

/// <summary>
/// Property-based tests for authorization enforcement across API endpoints.
/// 
/// **Property 17: Authorization Enforcement**
/// **Validates: Requirements 15.3**
/// 
/// For any API endpoint requiring authorization, when accessed without valid credentials,
/// the endpoint SHALL return HTTP 401, when accessed with valid credentials but insufficient
/// permissions, the endpoint SHALL return HTTP 403, and when accessed with valid credentials
/// and permissions, the endpoint SHALL process the request normally.
/// </summary>
[Trait("Category", "Property-Based")]
[Trait("Property", "AuthorizationEnforcement")]
public class AuthorizationEnforcementPropertyTests : ApiTestBase
{
    #region Unauthenticated Access Tests

    /// <summary>
    /// Property: Public endpoints (like GET /api/menu, GET /api/chefs) SHALL be accessible
    /// without authentication, returning appropriate success status codes.
    /// </summary>
    [Theory]
    [InlineData("/api/menu")]
    [InlineData("/api/chefs")]
    public async Task PublicEndpoints_WithoutAuthentication_ShouldReturnSuccess(string endpoint)
    {
        // Act
        var response = await GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound,
            $"Public endpoint {endpoint} should not require authentication");
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            "Public endpoints should not return 401");
    }

    /// <summary>
    /// Property: Protected endpoints requiring POST operations SHALL return HTTP 401
    /// when accessed without any authentication credentials.
    /// 
    /// Tests that creating reservations without authentication is blocked with 401.
    /// </summary>
    [Fact]
    public async Task PostReservation_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange
        var command = new CreateReservationCommand(
            CustomerName: "John Doe",
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
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "Creating a reservation without authentication should return 401");
    }

    /// <summary>
    /// Property: Protected DELETE endpoints SHALL return HTTP 401
    /// when accessed without authentication.
    /// </summary>
    [Fact]
    public async Task DeleteReservation_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange
        var reservationId = Guid.NewGuid();

        // Act
        var response = await DeleteAsync($"/api/reservations/{reservationId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "Deleting a reservation without authentication should return 401");
    }

    /// <summary>
    /// Property: Protected PUT/PATCH endpoints requiring updates SHALL return HTTP 401
    /// when accessed without authentication.
    /// </summary>
    [Fact]
    public async Task UpdateReservation_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var command = new CreateReservationCommand(
            CustomerName: "Jane Doe",
            Email: "jane@example.com",
            PhoneNumber: "555-5678",
            ReservationDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
            ReservationTime: "20:00",
            PartySize: 2,
            SpecialRequests: null
        );

        // Act
        var response = await PutAsync($"/api/reservations/{reservationId}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "Updating a reservation without authentication should return 401");
    }

    #endregion

    #region Invalid Authentication Tests

    /// <summary>
    /// Property: Endpoints SHALL return HTTP 401 when provided with
    /// malformed or invalid authentication tokens.
    /// </summary>
    [Theory]
    [InlineData("invalid-token")]
    [InlineData("bearer")]
    [InlineData("")]
    [InlineData("malformed-jwt")]
    public async Task PostReservation_WithInvalidToken_ShouldReturn401(string invalidToken)
    {
        // Arrange
        var client = CreateAuthenticatedClient(invalidToken);
        var command = new CreateReservationCommand(
            CustomerName: "John Doe",
            Email: "john@example.com",
            PhoneNumber: "555-1234",
            ReservationDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            ReservationTime: "19:00",
            PartySize: 4,
            SpecialRequests: null
        );

        var content = WebApplicationFactoryFixture.CreateJsonContent(command);

        // Act
        var response = await client.PostAsync("/api/reservations", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            $"Invalid token '{invalidToken}' should return 401");
    }

    /// <summary>
    /// Property: Endpoints with expired tokens SHALL return HTTP 401.
    /// </summary>
    [Fact]
    public async Task PostReservation_WithExpiredToken_ShouldReturn401()
    {
        // Arrange
        var expiredToken = GenerateMockJwtToken("expired-user");
        var client = CreateAuthenticatedClient(expiredToken);

        var command = new CreateReservationCommand(
            CustomerName: "John Doe",
            Email: "john@example.com",
            PhoneNumber: "555-1234",
            ReservationDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            ReservationTime: "19:00",
            PartySize: 4,
            SpecialRequests: null
        );

        var content = WebApplicationFactoryFixture.CreateJsonContent(command);

        // Act
        var response = await client.PostAsync("/api/reservations", content);

        // Assert
        // Note: Expired token validation depends on implementation
        // This test ensures consistent 401 response for invalid tokens
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "Expired token should return 401");
    }

    #endregion

    #region Authorization Consistency Tests

    /// <summary>
    /// Property: All protected endpoints that require authentication SHALL consistently
    /// return HTTP 401 for unauthenticated requests, regardless of HTTP method or endpoint path.
    /// 
    /// This ensures that the authorization middleware is consistently applied.
    /// </summary>
    [Fact]
    public async Task ProtectedEndpoints_WithoutAuth_ShouldConsistentlyReturn401()
    {
        // Arrange
        var reservationId = Guid.NewGuid();

        // Act - Test multiple endpoints with different HTTP methods
        var postResponse = await PostAsync("/api/reservations", new CreateReservationCommand(
            "John", "john@example.com", "555-1234",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), "19:00", 4, null
        ));

        var deleteResponse = await DeleteAsync($"/api/reservations/{reservationId}");

        var getOrdersResponse = await GetAsync("/api/orders");

        // Assert - All should return 401
        postResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        getOrdersResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Property: When authentication middleware is properly configured, all protected endpoints
    /// SHALL follow the same authentication pattern, returning 401 for missing/invalid auth,
    /// demonstrating consistent security enforcement.
    /// </summary>
    [Fact]
    public async Task AllProtectedEndpoints_ShouldEnforceAuthenticationConsistently()
    {
        // Arrange - Get list of protected endpoints
        var protectedEndpoints = new[]
        {
            ("/api/reservations", HttpMethod.Post),
            ("/api/orders", HttpMethod.Post),
            ("/api/reservations/123", HttpMethod.Delete),
            ("/api/orders/123", HttpMethod.Patch),
        };

        // Act & Assert
        foreach (var (endpoint, method) in protectedEndpoints)
        {
            HttpResponseMessage response = method.Method switch
            {
                "Post" => await PostAsync(endpoint, new CreateReservationCommand(
                    "Test", "test@example.com", "123", 
                    DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), "19:00", 1, null)),
                "Delete" => await DeleteAsync(endpoint),
                "Patch" => await PatchAsync(endpoint, new { status = "completed" }),
                _ => throw new ArgumentException($"Unsupported method: {method}")
            };

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                $"{method.Method} {endpoint} without auth should return 401");
        }
    }

    #endregion

    #region Error Response Consistency Tests

    /// <summary>
    /// Property: HTTP 401 Unauthorized responses from protected endpoints SHALL include
    /// appropriate error messages indicating authentication is required.
    /// </summary>
    [Fact]
    public async Task UnauthorizedResponse_ShouldContainAuthenticationErrorMessage()
    {
        // Arrange
        var command = new CreateReservationCommand(
            "John Doe", "john@example.com", "555-1234",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), "19:00", 4, null
        );

        // Act
        var response = await PostAsync("/api/reservations", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().NotBeNullOrEmpty("401 responses should include error details");
        responseBody.Should().Match(c => 
            c.Contains("unauthorized", System.StringComparison.OrdinalIgnoreCase) ||
            c.Contains("authentication", System.StringComparison.OrdinalIgnoreCase),
            "Error message should indicate authentication issue");
    }

    #endregion
}

