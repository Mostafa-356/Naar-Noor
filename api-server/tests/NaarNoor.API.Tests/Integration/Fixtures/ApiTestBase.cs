using FluentAssertions;
using System.Net;
using Xunit;

namespace NaarNoor.API.Tests.Integration.Fixtures;

/// <summary>
/// Base class for API integration tests. Provides WebApplicationFactory injection,
/// common HTTP operation helpers, and assertion methods for validating API responses.
/// </summary>
/// <remarks>
/// Validates: Requirements 10.5, 10.6, 11.1, 4.1, 4.2, 4.3
/// 
/// This base class:
/// - Manages WebApplicationFactory lifecycle and HTTP client creation
/// - Provides helper methods for common HTTP operations (GET, POST, PUT, DELETE)
/// - Supplies assertion helpers for response validation
/// - Offers bearer token generation for authentication testing
/// - Enables database seeding and cleanup between tests
/// </remarks>
public abstract class ApiTestBase : IAsyncLifetime
{
    private readonly WebApplicationFactoryFixture _fixture;
    protected HttpClient Client { get; private set; } = null!;

    protected ApiTestBase()
    {
        _fixture = new WebApplicationFactoryFixture();
    }

    /// <summary>
    /// Called by xUnit before each test to initialize the factory and client.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _fixture.InitializeAsync();
        Client = _fixture.CreateClient();
        Client.BaseAddress = new Uri("https://localhost");
    }

    /// <summary>
    /// Called by xUnit after each test to clean up.
    /// </summary>
    public async Task DisposeAsync()
    {
        Client?.Dispose();
        await _fixture.DisposeAsync();
    }

    /// <summary>
    /// Performs a GET request to the specified endpoint.
    /// </summary>
    protected async Task<HttpResponseMessage> GetAsync(string endpoint)
    {
        var response = await Client.GetAsync(endpoint);
        return response;
    }

    /// <summary>
    /// Performs a GET request and deserializes the response to the specified type.
    /// </summary>
    protected async Task<T?> GetAsyncDeserialized<T>(string endpoint)
    {
        var response = await GetAsync(endpoint);
        response.StatusCode.Should().Be(HttpStatusCode.OK, $"GET {endpoint} should return 200 OK");
        return await WebApplicationFactoryFixture.ParseJsonResponseAsync<T>(response.Content);
    }

    /// <summary>
    /// Performs a POST request with JSON content.
    /// </summary>
    protected async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data)
    {
        var content = WebApplicationFactoryFixture.CreateJsonContent(data);
        var response = await Client.PostAsync(endpoint, content);
        return response;
    }

    /// <summary>
    /// Performs a POST request and deserializes the response.
    /// </summary>
    protected async Task<TResponse?> PostAsyncDeserialized<TRequest, TResponse>(
        string endpoint,
        TRequest data)
    {
        var response = await PostAsync(endpoint, data);
        return await WebApplicationFactoryFixture.ParseJsonResponseAsync<TResponse>(response.Content);
    }

    /// <summary>
    /// Performs a PUT request with JSON content.
    /// </summary>
    protected async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data)
    {
        var content = WebApplicationFactoryFixture.CreateJsonContent(data);
        var response = await Client.PutAsync(endpoint, content);
        return response;
    }

    /// <summary>
    /// Performs a PUT request and deserializes the response.
    /// </summary>
    protected async Task<TResponse?> PutAsyncDeserialized<TRequest, TResponse>(
        string endpoint,
        TRequest data)
    {
        var response = await PutAsync(endpoint, data);
        return await WebApplicationFactoryFixture.ParseJsonResponseAsync<TResponse>(response.Content);
    }

    /// <summary>
    /// Performs a PATCH request with JSON content.
    /// </summary>
    protected async Task<HttpResponseMessage> PatchAsync<T>(string endpoint, T data)
    {
        var content = WebApplicationFactoryFixture.CreateJsonContent(data);
        var request = new HttpRequestMessage(HttpMethod.Patch, endpoint)
        {
            Content = content
        };
        var response = await Client.SendAsync(request);
        return response;
    }

    /// <summary>
    /// Performs a DELETE request.
    /// </summary>
    protected async Task<HttpResponseMessage> DeleteAsync(string endpoint)
    {
        var response = await Client.DeleteAsync(endpoint);
        return response;
    }

    /// <summary>
    /// Helper to assert that a response has the expected status code and parsing doesn't fail.
    /// </summary>
    protected void AssertResponseStatus(HttpResponseMessage response, HttpStatusCode expectedStatus)
    {
        response.StatusCode.Should().Be(expectedStatus,
            $"Response should have status {expectedStatus}, got {response.StatusCode}. " +
            $"Content: {response.Content.ReadAsStringAsync().Result}");
    }

    /// <summary>
    /// Helper to assert that a response has OK status (200).
    /// </summary>
    protected void AssertResponseOk(HttpResponseMessage response)
    {
        AssertResponseStatus(response, HttpStatusCode.OK);
    }

    /// <summary>
    /// Helper to assert that a response has Created status (201).
    /// </summary>
    protected void AssertResponseCreated(HttpResponseMessage response)
    {
        AssertResponseStatus(response, HttpStatusCode.Created);
    }

    /// <summary>
    /// Helper to assert that a response has NoContent status (204).
    /// </summary>
    protected void AssertResponseNoContent(HttpResponseMessage response)
    {
        AssertResponseStatus(response, HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Helper to assert that a response has BadRequest status (400).
    /// </summary>
    protected void AssertResponseBadRequest(HttpResponseMessage response)
    {
        AssertResponseStatus(response, HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Helper to assert that a response has Unauthorized status (401).
    /// </summary>
    protected void AssertResponseUnauthorized(HttpResponseMessage response)
    {
        AssertResponseStatus(response, HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Helper to assert that a response has Forbidden status (403).
    /// </summary>
    protected void AssertResponseForbidden(HttpResponseMessage response)
    {
        AssertResponseStatus(response, HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Helper to assert that a response has NotFound status (404).
    /// </summary>
    protected void AssertResponseNotFound(HttpResponseMessage response)
    {
        AssertResponseStatus(response, HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Helper to assert that a response has InternalServerError status (500).
    /// </summary>
    protected void AssertResponseServerError(HttpResponseMessage response)
    {
        AssertResponseStatus(response, HttpStatusCode.InternalServerError);
    }

    /// <summary>
    /// Helper to assert that response content is JSON with specific content type.
    /// </summary>
    protected void AssertResponseContentTypeJson(HttpResponseMessage response)
    {
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json",
            "Response should be JSON");
    }

    /// <summary>
    /// Helper to assert that a response has a Location header (typically for Created responses).
    /// </summary>
    protected string AssertResponseHasLocation(HttpResponseMessage response)
    {
        response.Headers.Location.Should().NotBeNull("Response should have Location header");
        return response.Headers.Location!.ToString();
    }

    /// <summary>
    /// Helper to assert that response contains specific error message.
    /// </summary>
    protected async Task AssertResponseContainsErrorAsync(
        HttpResponseMessage response,
        string expectedErrorMessage)
    {
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain(expectedErrorMessage, "Response should contain error message");
    }

    /// <summary>
    /// Creates an authenticated HTTP client with a bearer token.
    /// </summary>
    protected HttpClient CreateAuthenticatedClient(string? bearerToken = null)
    {
        return _fixture.CreateAuthenticatedClient(bearerToken);
    }

    /// <summary>
    /// Seeds test data into the database via the fixture.
    /// </summary>
    protected async Task SeedDataAsync(Func<NaarNoor.Infrastructure.Data.ApplicationDbContext, Task> seedAction)
    {
        await _fixture.SeedDataAsync(seedAction);
    }

    /// <summary>
    /// Clears all database data (useful between tests).
    /// </summary>
    protected async Task ClearDatabaseAsync()
    {
        await _fixture.ClearDatabaseAsync();
    }

    /// <summary>
    /// Gets an entity from the database by ID.
    /// </summary>
    protected async Task<T?> GetEntityAsync<T>(Guid id) where T : class
    {
        return await _fixture.GetEntityAsync<T>(id);
    }

    /// <summary>
    /// Generates a mock JWT token for testing protected endpoints.
    /// </summary>
    protected string GenerateMockJwtToken(string subject = "test-user")
    {
        return _fixture.GenerateMockJwtToken(subject);
    }
}
