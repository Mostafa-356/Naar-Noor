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

public class SupabaseRealtimeServiceTests
{
    private static SupabaseRealtimeService CreateService(HttpMessageHandler? handler = null)
    {
        handler ??= OkHandler();
        var cfg = new SupabaseConfig { Url = "https://test.supabase.co", AnonKey = "test-key", ServiceRoleKey = "svc-key" };
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(cfg.Url) };
        var logger = Mock.Of<ILogger<SupabaseRealtimeService>>();
        return new SupabaseRealtimeService(httpClient, cfg, logger);
    }

    private static HttpMessageHandler OkHandler()
    {
        var mock = new Mock<HttpMessageHandler>();
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });
        return mock.Object;
    }

    [Fact]
    public void IsConnected_InitialState_IsFalse()
    {
        var service = CreateService();

        service.IsConnected.Should().BeFalse("Service should not be connected on construction");
    }

    [Fact]
    public async Task BroadcastMessageAsync_DoesNotThrow()
    {
        var service = CreateService();

        Func<Task> act = () => service.BroadcastMessageAsync("orders", "new-order", new { id = Guid.NewGuid() });

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SubscribeToOrderUpdatesAsync_DoesNotThrow()
    {
        var service = CreateService();

        Func<Task> act = () => service.SubscribeToOrderUpdatesAsync(
            "order-1",
            _ => Task.CompletedTask,
            _ => Task.CompletedTask);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SubscribeToReservationUpdatesAsync_DoesNotThrow()
    {
        var service = CreateService();

        Func<Task> act = () => service.SubscribeToReservationUpdatesAsync(
            "res-1",
            _ => Task.CompletedTask,
            _ => Task.CompletedTask);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SubscribeToReviewUpdatesAsync_DoesNotThrow()
    {
        var service = CreateService();

        Func<Task> act = () => service.SubscribeToReviewUpdatesAsync(
            "item-1",
            _ => Task.CompletedTask,
            _ => Task.CompletedTask);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SubscribeToTableAvailabilityAsync_DoesNotThrow()
    {
        var service = CreateService();

        Func<Task> act = () => service.SubscribeToTableAvailabilityAsync(
            _ => Task.CompletedTask,
            _ => Task.CompletedTask);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UnsubscribeAsync_DoesNotThrow()
    {
        var service = CreateService();

        Func<Task> act = () => service.UnsubscribeAsync("some-subscription-id");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ReconnectAsync_DoesNotThrow()
    {
        var service = CreateService();

        Func<Task> act = () => service.ReconnectAsync();

        await act.Should().NotThrowAsync();
    }
}
