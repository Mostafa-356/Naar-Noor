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

public class SupabaseStorageServiceTests
{
    private static SupabaseStorageService CreateService(HttpMessageHandler handler)
    {
        var cfg = new SupabaseConfig { Url = "https://test.supabase.co", AnonKey = "test-key", ServiceRoleKey = "service-key" };
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(cfg.Url) };
        var logger = Mock.Of<ILogger<SupabaseStorageService>>();
        return new SupabaseStorageService(httpClient, cfg, logger);
    }

    private static HttpMessageHandler OkHandler(object? body = null)
    {
        var mock = new Mock<HttpMessageHandler>();
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(body ?? new { }), Encoding.UTF8, "application/json")
            });
        return mock.Object;
    }

    private static HttpMessageHandler ErrorHandler(HttpStatusCode status)
    {
        var mock = new Mock<HttpMessageHandler>();
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(status)
            {
                Content = new StringContent("{\"error\":\"Unauthorized\"}", Encoding.UTF8, "application/json")
            });
        return mock.Object;
    }

    [Fact]
    public void GetPublicUrl_ReturnsConstructedUrl()
    {
        var service = CreateService(OkHandler());

        var url = service.GetPublicUrl("my-bucket", "images/photo.jpg");

        url.Should().NotBeNullOrEmpty();
        url.Should().Contain("my-bucket");
        url.Should().Contain("photo.jpg");
    }

    [Fact]
    public async Task UploadImageAsync_OnSuccess_ReturnsPublicUrl()
    {
        var service = CreateService(OkHandler(new { Key = "chef-images/chef.jpg" }));

        var fileBytes = Encoding.UTF8.GetBytes("fake-image-data");
        var (success, publicUrl, error) = await service.UploadImageAsync("chef-images", "chef.jpg", fileBytes, "image/jpeg");

        success.Should().BeTrue();
        error.Should().BeNull();
        publicUrl.Should().NotBeNull();
    }

    [Fact]
    public async Task UploadImageAsync_OnHttpError_ReturnsError()
    {
        var service = CreateService(ErrorHandler(HttpStatusCode.Unauthorized));

        var fileBytes = new byte[] { 1, 2, 3 };
        var (success, publicUrl, error) = await service.UploadImageAsync("chef-images", "chef.jpg", fileBytes, "image/jpeg");

        success.Should().BeFalse();
        error.Should().NotBeNull();
    }

    [Fact]
    public async Task UploadChefImageAsync_OnSuccess_ReturnsPublicUrl()
    {
        var service = CreateService(OkHandler(new { }));

        var (success, publicUrl, error) = await service.UploadChefImageAsync("chef-001", new byte[] { 1, 2, 3 }, "image/jpeg");

        success.Should().BeTrue();
    }

    [Fact]
    public async Task UploadMenuItemImageAsync_OnSuccess_ReturnsPublicUrl()
    {
        var service = CreateService(OkHandler(new { }));

        var (success, publicUrl, error) = await service.UploadMenuItemImageAsync("item-001", new byte[] { 1, 2, 3 }, "image/png");

        success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteFileAsync_OnSuccess_ReturnsTrue()
    {
        var service = CreateService(OkHandler(new { }));

        var (success, error) = await service.DeleteFileAsync("chef-images", "chef.jpg");

        success.Should().BeTrue();
        error.Should().BeNull();
    }

    [Fact]
    public async Task DeleteFileAsync_OnHttpError_ReturnsFalse()
    {
        var service = CreateService(ErrorHandler(HttpStatusCode.NotFound));

        var (success, error) = await service.DeleteFileAsync("chef-images", "nonexistent.jpg");

        success.Should().BeFalse();
        error.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteChefImageAsync_OnSuccess_ReturnsTrue()
    {
        var service = CreateService(OkHandler(new { }));

        var (success, error) = await service.DeleteChefImageAsync("chef-001");

        success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteMenuItemImageAsync_OnSuccess_ReturnsTrue()
    {
        var service = CreateService(OkHandler(new { }));

        var (success, error) = await service.DeleteMenuItemImageAsync("item-001");

        success.Should().BeTrue();
    }

    [Fact]
    public async Task ListFilesAsync_OnSuccess_ReturnsFiles()
    {
        var service = CreateService(OkHandler(new[] { new { name = "chef1.jpg" }, new { name = "chef2.jpg" } }));

        var (success, files, error) = await service.ListFilesAsync("chef-images");

        success.Should().BeTrue();
    }

    [Fact]
    public async Task ListFilesAsync_OnHttpError_ReturnsFalse()
    {
        var service = CreateService(ErrorHandler(HttpStatusCode.Forbidden));

        var (success, files, error) = await service.ListFilesAsync("chef-images");

        success.Should().BeFalse();
        error.Should().NotBeNull();
    }
}
