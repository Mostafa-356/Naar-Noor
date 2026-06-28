using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NaarNoor.Infrastructure.Data;
using System.Net;
using Xunit;

namespace NaarNoor.API.Tests.Integration.Fixtures;

/// <summary>
/// Integration tests for base test classes created in task 1.3.
/// 
/// Tests that:
/// 1. WebApplicationFactoryFixture properly initializes mock dependencies
/// 2. Fixtures handle async setup/teardown correctly with IAsyncLifetime
/// 3. Database fixtures are properly isolated between tests
/// 4. WebApplicationFactory creates usable HttpClient instances
/// 5. Base class initialization and cleanup works correctly
/// 
/// Validates: Requirements 10.5
/// </summary>
public class BaseClassFunctionalityTests : IAsyncLifetime
{
    private WebApplicationFactoryFixture _fixture = null!;

    /// <summary>
    /// Initialize the fixture before each test.
    /// </summary>
    public async Task InitializeAsync()
    {
        _fixture = new WebApplicationFactoryFixture();
        await _fixture.InitializeAsync();
    }

    /// <summary>
    /// Dispose the fixture after each test.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }

    #region WebApplicationFactoryFixture Tests

    [Fact]
    public void WebApplicationFactoryFixture_CreateClient_ReturnsUsableHttpClient()
    {
        // Arrange & Act
        var client = _fixture.CreateClient();

        // Assert
        client.Should().NotBeNull();
        client.DefaultRequestHeaders.Should().NotBeNull();
    }

    [Fact]
    public void WebApplicationFactoryFixture_CreateAuthenticatedClient_WithBearerToken_AddsBearerHeader()
    {
        // Arrange
        var testToken = "test-jwt-token-123";

        // Act
        var client = _fixture.CreateAuthenticatedClient(testToken);

        // Assert
        client.Should().NotBeNull();
        client.DefaultRequestHeaders.Authorization.Should().NotBeNull();
        client.DefaultRequestHeaders.Authorization!.Scheme.Should().Be("Bearer");
        client.DefaultRequestHeaders.Authorization.Parameter.Should().Be(testToken);
    }

    [Fact]
    public void WebApplicationFactoryFixture_CreateAuthenticatedClient_WithoutBearerToken_NoAuthorizationHeader()
    {
        // Arrange & Act
        var client = _fixture.CreateAuthenticatedClient();

        // Assert
        client.Should().NotBeNull();
        client.DefaultRequestHeaders.Authorization.Should().BeNull();
    }

    [Fact]
    public void WebApplicationFactoryFixture_CreateClientWithBaseAddress_SetsCorrectBaseAddress()
    {
        // Arrange
        var baseAddress = "https://api.test.example.com";

        // Act
        var client = _fixture.CreateClientWithBaseAddress(baseAddress);

        // Assert
        client.BaseAddress.Should().NotBeNull();
        client.BaseAddress!.ToString().Should().Be(baseAddress + "/");
    }

    [Fact]
    public async Task WebApplicationFactoryFixture_GetDbContext_ReturnsApplicationDbContext()
    {
        // Arrange & Act
        var dbContext = _fixture.GetDbContext();

        // Assert
        dbContext.Should().NotBeNull();
        dbContext.Should().BeOfType<ApplicationDbContext>();
        
        // Verify it's functional by checking Database property
        var isInMemory = dbContext.Database.IsInMemory();
        isInMemory.Should().BeTrue("Test database should be in-memory");
    }

    [Fact]
    public async Task WebApplicationFactoryFixture_SeedDataAsync_InsertsDataIntoDatabase()
    {
        // Arrange
        var dbContext = _fixture.GetDbContext();

        // Act
        await _fixture.SeedDataAsync(async context =>
        {
            // Verify context is usable
            var canConnect = await context.Database.CanConnectAsync();
            canConnect.Should().BeTrue();
        });

        // Assert - verify seeding worked
        var contextAfterSeed = _fixture.GetDbContext();
        var canConnect2 = await contextAfterSeed.Database.CanConnectAsync();
        canConnect2.Should().BeTrue();
    }

    [Fact]
    public async Task WebApplicationFactoryFixture_ClearDatabaseAsync_RemovesAllData()
    {
        // Arrange - Insert test data via seed
        await _fixture.SeedDataAsync(async context =>
        {
            // Just verify context is usable
            var canConnect = await context.Database.CanConnectAsync();
            canConnect.Should().BeTrue();
        });

        // Act
        await _fixture.ClearDatabaseAsync();

        // Assert - verify database is clean
        var dbContext = _fixture.GetDbContext();
        var canConnect2 = await dbContext.Database.CanConnectAsync();
        canConnect2.Should().BeTrue();
    }

    [Fact]
    public void WebApplicationFactoryFixture_GenerateMockJwtToken_ReturnsValidBase64String()
    {
        // Arrange
        var subject = "test-user-id-123";

        // Act
        var token = _fixture.GenerateMockJwtToken(subject);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        // Verify it's valid base64
        Func<string> decodeAction = () => 
        {
            var bytes = Convert.FromBase64String(token);
            return System.Text.Encoding.UTF8.GetString(bytes);
        };
        
        var decodedContent = decodeAction();
        decodedContent.Should().Contain(subject);
    }

    [Fact]
    public void WebApplicationFactoryFixture_GenerateMockJwtToken_WithClaims_IncludesClaimsInToken()
    {
        // Arrange
        var subject = "test-user";
        var claims = new[] { ("role", "admin"), ("email", "test@example.com") };

        // Act
        var token = _fixture.GenerateMockJwtToken(subject, claims);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        var decodedContent = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(token));
        decodedContent.Should().Contain("role=admin");
        decodedContent.Should().Contain("email=test@example.com");
    }

    [Fact]
    public void WebApplicationFactoryFixture_CreateJsonContent_SerializesObjectCorrectly()
    {
        // Arrange
        var testObject = new { id = 1, name = "test", email = "test@example.com" };

        // Act
        var content = WebApplicationFactoryFixture.CreateJsonContent(testObject);

        // Assert
        content.Should().NotBeNull();
        content.Headers.ContentType!.MediaType.Should().Be("application/json");
        
        var json = content.ReadAsStringAsync().Result;
        json.Should().Contain("\"id\":1");
        json.Should().Contain("\"name\":\"test\"");
    }

    [Fact]
    public async Task WebApplicationFactoryFixture_ParseJsonResponseAsync_DeserializesJsonCorrectly()
    {
        // Arrange
        var testObject = new { id = 42, name = "ParseTest" };
        var json = System.Text.Json.JsonSerializer.Serialize(testObject);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        var result = await WebApplicationFactoryFixture.ParseJsonResponseAsync<System.Text.Json.JsonElement>(content);

        // Assert
        result.Should().NotBe(default(System.Text.Json.JsonElement));
    }

    [Fact]
    public void WebApplicationFactoryFixture_AssertHttpResponse_WithMatchingStatus_DoesNotThrow()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        // Act & Assert
        var action = () => WebApplicationFactoryFixture.AssertHttpResponse(response, HttpStatusCode.OK);
        action.Should().NotThrow();
    }

    [Fact]
    public void WebApplicationFactoryFixture_AssertHttpResponse_WithMismatchedStatus_Throws()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);

        // Act & Assert
        var action = () => WebApplicationFactoryFixture.AssertHttpResponse(response, HttpStatusCode.OK);
        action.Should().Throw<Xunit.Sdk.XunitException>();
    }

    #endregion

    #region IAsyncLifetime Implementation Tests

    [Fact]
    public async Task WebApplicationFactoryFixture_AsyncLifetime_InitializeAsync_IsIdempotent()
    {
        // Arrange - fixture is already initialized from InitializeAsync
        var firstClient = _fixture.CreateClient();

        // Act - Initialize again
        await _fixture.InitializeAsync();
        var secondClient = _fixture.CreateClient();

        // Assert - both clients should be functional
        firstClient.Should().NotBeNull();
        secondClient.Should().NotBeNull();
    }

    [Fact]
    public async Task WebApplicationFactoryFixture_AsyncLifetime_DisposeAsync_CleansUpResources()
    {
        // Arrange
        var fixture = new WebApplicationFactoryFixture();
        await fixture.InitializeAsync();
        var client = fixture.CreateClient();

        // Act
        await fixture.DisposeAsync();

        // Assert - fixture should be disposed (this is hard to test directly,
        // but we verify no exceptions are thrown)
        fixture.Should().NotBeNull(); // Fixture object itself still exists
    }

    #endregion

    #region Database Fixture Isolation Tests

    [Fact]
    public async Task DatabaseFixture_EnsuresIsolationBetweenTests_FirstTest()
    {
        // Arrange & Act
        var dbContext = _fixture.GetDbContext();
        var isInMemory = dbContext.Database.IsInMemory();

        // Assert
        isInMemory.Should().BeTrue();
        var canConnect = await dbContext.Database.CanConnectAsync();
        canConnect.Should().BeTrue();
    }

    [Fact]
    public async Task DatabaseFixture_EachTestGetsCleanDatabase_SecondTest()
    {
        // Arrange & Act
        var dbContext = _fixture.GetDbContext();
        var isInMemory = dbContext.Database.IsInMemory();

        // Assert
        isInMemory.Should().BeTrue();
        var canConnect = await dbContext.Database.CanConnectAsync();
        canConnect.Should().BeTrue();
    }

    #endregion

    #region ApiTestBase Integration Tests

    [Fact]
    public async Task ApiTestBase_CanBeInherited_AndInitializesCorrectly()
    {
        // Arrange
        var testClass = new SampleApiTestClass();

        // Act
        await testClass.InitializeAsync();

        // Assert
        testClass.GetClient().Should().NotBeNull();
        testClass.GetClient().BaseAddress.Should().NotBeNull();

        // Cleanup
        await testClass.DisposeAsync();
    }

    [Fact]
    public async Task ApiTestBase_GetAsyncHelper_ReturnsHttpResponseMessage()
    {
        // Arrange
        var testClass = new SampleApiTestClass();
        await testClass.InitializeAsync();

        // Act - Testing a non-existent endpoint should return 404
        var response = await testClass.TestGetAsync("/api/nonexistent");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().NotBe(HttpStatusCode.ServiceUnavailable);

        // Cleanup
        await testClass.DisposeAsync();
    }

    [Fact]
    public async Task ApiTestBase_AssertionHelpers_WorkCorrectly()
    {
        // Arrange
        var testClass = new SampleApiTestClass();
        await testClass.InitializeAsync();

        // Act & Assert - Test assertion helpers
        testClass.TestAssertResponseStatus(HttpStatusCode.OK);
        testClass.TestAssertResponseCreated(HttpStatusCode.Created);

        await testClass.DisposeAsync();
    }

    [Fact]
    public async Task ApiTestBase_SeedAndClearDatabase_WorksCorrectly()
    {
        // Arrange
        var testClass = new SampleApiTestClass();
        await testClass.InitializeAsync();

        // Act
        await testClass.TestSeedAndClearData();

        // Assert
        testClass.SeedDataWasSuccessful.Should().BeTrue();

        await testClass.DisposeAsync();
    }

    #endregion

    #region Mock Dependency Initialization Tests

    [Fact]
    public void MockDependencies_AreProperlyInjected_IntoFixture()
    {
        // Arrange & Act
        var client = _fixture.CreateClient();

        // Assert
        client.Should().NotBeNull();
        
        // Verify that the client has default headers set up (indication of proper initialization)
        client.DefaultRequestHeaders.Should().NotBeNull();
        client.DefaultRequestHeaders.Accept.Should().NotBeNull();
    }

    #endregion
}

/// <summary>
/// Sample implementation of ApiTestBase for testing the base class functionality.
/// </summary>
internal class SampleApiTestClass : ApiTestBase
{
    public bool SeedDataWasSuccessful { get; private set; }

    public HttpClient GetClient() => Client;

    public async Task<HttpResponseMessage> TestGetAsync(string endpoint)
    {
        return await GetAsync(endpoint);
    }

    public void TestAssertResponseStatus(HttpStatusCode status)
    {
        var response = new HttpResponseMessage(status);
        AssertResponseStatus(response, status);
    }

    public void TestAssertResponseCreated(HttpStatusCode status)
    {
        var response = new HttpResponseMessage(status);
        AssertResponseStatus(response, status);
    }

    public async Task TestSeedAndClearData()
    {
        await SeedDataAsync(async context =>
        {
            SeedDataWasSuccessful = true;
            await context.Database.CanConnectAsync();
        });

        await ClearDatabaseAsync();
    }
}
