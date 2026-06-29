using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NaarNoor.Infrastructure.Services;
using NaarNoor.Infrastructure;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace NaarNoor.Infrastructure.Tests.Services;

public class SupabaseAuthServiceTests
{
    private static SupabaseAuthService CreateService(HttpMessageHandler handler, SupabaseConfig? config = null)
    {
        var cfg = config ?? new SupabaseConfig { Url = "https://test.supabase.co", AnonKey = "test-anon-key", ServiceRoleKey = "test-service-key" };
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(cfg.Url) };
        var logger = Mock.Of<ILogger<SupabaseAuthService>>();
        return new SupabaseAuthService(httpClient, cfg, logger);
    }

    private static HttpMessageHandler SuccessHandler(object responseBody)
    {
        var mock = new Mock<HttpMessageHandler>();
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(responseBody), Encoding.UTF8, "application/json")
            });
        return mock.Object;
    }

    private static HttpMessageHandler ErrorHandler(HttpStatusCode status = HttpStatusCode.BadRequest)
    {
        var mock = new Mock<HttpMessageHandler>();
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(status)
            {
                Content = new StringContent("{\"error\":\"invalid_credentials\"}", Encoding.UTF8, "application/json")
            });
        return mock.Object;
    }

    [Fact]
    public async Task RegisterUserAsync_OnSuccess_ReturnsTrueWithUserId()
    {
        var response = new { user = new { id = "user-123" } };
        var service = CreateService(SuccessHandler(response));

        var (success, userId, error) = await service.RegisterUserAsync("test@test.com", "password123");

        success.Should().BeTrue();
        userId.Should().Be("user-123");
        error.Should().BeNull();
    }

    [Fact]
    public async Task RegisterUserAsync_OnHttpError_ReturnsFalse()
    {
        var service = CreateService(ErrorHandler(HttpStatusCode.BadRequest));

        var (success, userId, error) = await service.RegisterUserAsync("test@test.com", "pass");

        success.Should().BeFalse();
        userId.Should().BeNull();
        error.Should().NotBeNull();
    }

    [Fact]
    public async Task RegisterUserAsync_WhenResponseMissingUserId_ReturnsFalse()
    {
        var response = new { some_other_field = "value" };
        var service = CreateService(SuccessHandler(response));

        var (success, userId, error) = await service.RegisterUserAsync("test@test.com", "pass");

        success.Should().BeFalse();
        error.Should().NotBeNull();
    }

    [Fact]
    public async Task LoginUserAsync_OnSuccess_ReturnsToken()
    {
        var response = new { access_token = "jwt-token-abc" };
        var service = CreateService(SuccessHandler(response));

        var (success, token, error) = await service.LoginUserAsync("user@test.com", "password");

        success.Should().BeTrue();
        token.Should().Be("jwt-token-abc");
        error.Should().BeNull();
    }

    [Fact]
    public async Task LoginUserAsync_OnHttpError_ReturnsFalse()
    {
        var service = CreateService(ErrorHandler(HttpStatusCode.Unauthorized));

        var (success, token, error) = await service.LoginUserAsync("user@test.com", "wrong");

        success.Should().BeFalse();
        token.Should().BeNull();
    }

    [Fact]
    public async Task LoginUserAsync_WhenNoTokenInResponse_ReturnsFalse()
    {
        var response = new { refresh_token = "rt" };
        var service = CreateService(SuccessHandler(response));

        var (success, token, error) = await service.LoginUserAsync("user@test.com", "pass");

        success.Should().BeFalse();
        error.Should().NotBeNull();
    }

    [Fact]
    public async Task LogoutUserAsync_OnSuccess_ReturnsTrue()
    {
        var service = CreateService(SuccessHandler(new { }));

        var (success, error) = await service.LogoutUserAsync("user-123");

        success.Should().BeTrue();
        error.Should().BeNull();
    }

    [Fact]
    public async Task LogoutUserAsync_OnHttpError_ReturnsFalse()
    {
        var service = CreateService(ErrorHandler(HttpStatusCode.Unauthorized));

        var (success, error) = await service.LogoutUserAsync("user-123");

        success.Should().BeFalse();
        error.Should().NotBeNull();
    }

    [Fact]
    public async Task VerifyTokenAsync_OnSuccess_ReturnsUserId()
    {
        var response = new { id = "verified-user-id" };
        var service = CreateService(SuccessHandler(response));

        var (valid, userId, error) = await service.VerifyTokenAsync("valid-token");

        valid.Should().BeTrue();
        userId.Should().Be("verified-user-id");
        error.Should().BeNull();
    }

    [Fact]
    public async Task VerifyTokenAsync_OnHttpError_ReturnsFalse()
    {
        var service = CreateService(ErrorHandler(HttpStatusCode.Unauthorized));

        var (valid, userId, error) = await service.VerifyTokenAsync("bad-token");

        valid.Should().BeFalse();
        userId.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentUserAsync_OnSuccess_ReturnsUserData()
    {
        var response = new { id = "curr-user-id", email = "curr@test.com" };
        var service = CreateService(SuccessHandler(response));

        var (success, userId, email, error) = await service.GetCurrentUserAsync("valid-token");

        success.Should().BeTrue();
        userId.Should().Be("curr-user-id");
        email.Should().Be("curr@test.com");
        error.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentUserAsync_OnHttpError_ReturnsFalse()
    {
        var service = CreateService(ErrorHandler(HttpStatusCode.Unauthorized));

        var (success, userId, email, error) = await service.GetCurrentUserAsync("bad-token");

        success.Should().BeFalse();
        userId.Should().BeNull();
        email.Should().BeNull();
    }

    [Fact]
    public async Task ResetPasswordAsync_OnSuccess_ReturnsTrue()
    {
        var service = CreateService(SuccessHandler(new { }));

        var (success, error) = await service.ResetPasswordAsync("user@test.com");

        success.Should().BeTrue();
        error.Should().BeNull();
    }

    [Fact]
    public async Task ResetPasswordAsync_OnHttpError_ReturnsFalse()
    {
        var service = CreateService(ErrorHandler(HttpStatusCode.BadRequest));

        var (success, error) = await service.ResetPasswordAsync("user@test.com");

        success.Should().BeFalse();
        error.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateUserEmailAsync_OnSuccess_ReturnsTrue()
    {
        var service = CreateService(SuccessHandler(new { }));

        var (success, error) = await service.UpdateUserEmailAsync("user-id", "new@test.com");

        success.Should().BeTrue();
        error.Should().BeNull();
    }

    [Fact]
    public async Task UpdateUserEmailAsync_OnHttpError_ReturnsFalse()
    {
        var service = CreateService(ErrorHandler(HttpStatusCode.Forbidden));

        var (success, error) = await service.UpdateUserEmailAsync("user-id", "new@test.com");

        success.Should().BeFalse();
        error.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateUserPasswordAsync_OnSuccess_ReturnsTrue()
    {
        var service = CreateService(SuccessHandler(new { }));

        var (success, error) = await service.UpdateUserPasswordAsync("user-id", "new-password");

        success.Should().BeTrue();
        error.Should().BeNull();
    }

    [Fact]
    public async Task UpdateUserPasswordAsync_OnHttpError_ReturnsFalse()
    {
        var service = CreateService(ErrorHandler(HttpStatusCode.Forbidden));

        var (success, error) = await service.UpdateUserPasswordAsync("user-id", "new-password");

        success.Should().BeFalse();
        error.Should().NotBeNull();
    }

    [Fact]
    public async Task VerifyTokenAsync_WhenResponseHasNoIdProperty_ReturnsFalse()
    {
        // Response is 200 OK but missing the "id" field
        var response = new { email = "user@test.com", role = "anon" };
        var service = CreateService(SuccessHandler(response));

        var (valid, userId, error) = await service.VerifyTokenAsync("some-token");

        valid.Should().BeFalse();
        userId.Should().BeNull();
        error.Should().Be("Invalid token: No user found");
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenResponseMissingEmail_ReturnsFalse()
    {
        // Response has "id" but missing "email"
        var response = new { id = "user-abc" };
        var service = CreateService(SuccessHandler(response));

        var (success, userId, email, error) = await service.GetCurrentUserAsync("some-token");

        success.Should().BeFalse();
        userId.Should().BeNull();
        email.Should().BeNull();
        error.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCurrentUserAsync_WhenResponseMissingIdAndEmail_ReturnsFalse()
    {
        // Response is 200 OK but missing both "id" and "email"
        var response = new { role = "anon" };
        var service = CreateService(SuccessHandler(response));

        var (success, userId, email, error) = await service.GetCurrentUserAsync("some-token");

        success.Should().BeFalse();
        error.Should().Be("Failed to get current user");
    }

    [Fact]
    public async Task LogoutUserAsync_WithDifferentUserId_OnSuccess_ReturnsTrue()
    {
        var service = CreateService(SuccessHandler(new { }));

        var (success, error) = await service.LogoutUserAsync("different-user-789");

        success.Should().BeTrue();
        error.Should().BeNull();
    }

    [Fact]
    public async Task ResetPasswordAsync_WithDifferentEmail_OnSuccess_ReturnsTrue()
    {
        var service = CreateService(SuccessHandler(new { }));

        var (success, error) = await service.ResetPasswordAsync("other@domain.com");

        success.Should().BeTrue();
        error.Should().BeNull();
    }
}
