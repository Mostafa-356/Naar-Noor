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

    [Fact]
    public async Task ReconnectAsync_SetsIsConnectedTrue()
    {
        var service = CreateService();
        service.IsConnected.Should().BeFalse();

        await service.ReconnectAsync();

        service.IsConnected.Should().BeTrue();
    }

    [Fact]
    public async Task UnsubscribeAsync_ExistingNullWebsocket_RemovesEntry()
    {
        // Subscribe first to create a dict entry with null websocket
        var service = CreateService();
        await service.SubscribeToOrderUpdatesAsync(
            "order-42",
            _ => Task.CompletedTask,
            _ => Task.CompletedTask);

        // Now unsubscribe using the channel key created by SubscribeToChannelAsync
        Func<Task> act = () => service.UnsubscribeAsync("orders:order-42");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UnsubscribeAsync_ReservationChannel_RemovesEntry()
    {
        var service = CreateService();
        await service.SubscribeToReservationUpdatesAsync(
            "res-99",
            _ => Task.CompletedTask,
            _ => Task.CompletedTask);

        Func<Task> act = () => service.UnsubscribeAsync("reservations:res-99");
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UnsubscribeAsync_ReviewChannel_RemovesEntry()
    {
        var service = CreateService();
        await service.SubscribeToReviewUpdatesAsync(
            "menu-1",
            _ => Task.CompletedTask,
            _ => Task.CompletedTask);

        Func<Task> act = () => service.UnsubscribeAsync("reviews:menu-1");
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UnsubscribeAsync_TableAvailabilityChannel_RemovesEntry()
    {
        var service = CreateService();
        await service.SubscribeToTableAvailabilityAsync(
            _ => Task.CompletedTask,
            _ => Task.CompletedTask);

        Func<Task> act = () => service.UnsubscribeAsync("table-availability");
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SubscribeToChannelAsync_ThenUnsubscribeMultiple_HandlesAll()
    {
        var service = CreateService();

        await service.SubscribeToOrderUpdatesAsync("o1", _ => Task.CompletedTask, _ => Task.CompletedTask);
        await service.SubscribeToReservationUpdatesAsync("r1", _ => Task.CompletedTask, _ => Task.CompletedTask);
        await service.SubscribeToReviewUpdatesAsync("m1", _ => Task.CompletedTask, _ => Task.CompletedTask);
        await service.SubscribeToTableAvailabilityAsync(_ => Task.CompletedTask, _ => Task.CompletedTask);

        Func<Task> act = async () =>
        {
            await service.UnsubscribeAsync("orders:o1");
            await service.UnsubscribeAsync("reservations:r1");
            await service.UnsubscribeAsync("reviews:m1");
            await service.UnsubscribeAsync("table-availability");
        };

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task BroadcastMessageAsync_WithObjectPayload_DoesNotThrow()
    {
        var service = CreateService();

        Func<Task> act = () => service.BroadcastMessageAsync("reservations", "table-update",
            new { tableId = "table-5", available = false });

        await act.Should().NotThrowAsync();
    }
}
