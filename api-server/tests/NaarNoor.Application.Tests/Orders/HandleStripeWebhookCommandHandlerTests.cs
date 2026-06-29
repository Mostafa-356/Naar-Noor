using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NaarNoor.Application.Common.Interfaces;
using NaarNoor.Application.Orders.Commands.HandleStripeWebhook;
using NaarNoor.Application.Tests.Common.Fixtures;
using NaarNoor.Domain.Entities;
using NaarNoor.Domain.Enums;
using NaarNoor.Infrastructure.Data;
using NaarNoor.Infrastructure.Repositories;
using Xunit;

namespace NaarNoor.Application.Tests.Orders;

public class HandleStripeWebhookCommandHandlerTests : ApplicationLayerTestBase, IAsyncLifetime
{
    private ApplicationDbContext _dbContext = null!;
    private UnitOfWork _unitOfWork = null!;
    private readonly Mock<IStripeService> _stripeMock;
    private readonly Mock<ILogger<HandleStripeWebhookCommandHandler>> _loggerMock;
    private HandleStripeWebhookCommandHandler _handler = null!;

    public HandleStripeWebhookCommandHandlerTests()
    {
        _stripeMock = CreateServiceMock<IStripeService>();
        _loggerMock = new Mock<ILogger<HandleStripeWebhookCommandHandler>>();
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("Webhook_" + Guid.NewGuid())
            .Options;
        _dbContext = new ApplicationDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
        _unitOfWork = new UnitOfWork(_dbContext);
        _handler = new HandleStripeWebhookCommandHandler(_unitOfWork, _stripeMock.Object, _loggerMock.Object);
    }

    public async Task DisposeAsync() => await _dbContext.DisposeAsync();

    private async Task<Order> SeedOrder(OrderStatus status = OrderStatus.Pending, PaymentStatus paymentStatus = PaymentStatus.Pending)
    {
        var order = new Order
        {
            CustomerName = "Test Customer",
            Email = "test@test.com",
            PhoneNumber = "07700000000",
            Type = OrderType.Collection,
            TotalAmount = 25.00m,
            Status = status,
            PaymentStatus = paymentStatus
        };
        _unitOfWork.Orders.Add(order);
        await _unitOfWork.SaveChangesAsync();
        return order;
    }

    [Fact]
    public async Task Handle_CheckoutCompleted_UpdatesOrderToPaidAndConfirmed()
    {
        var order = await SeedOrder();

        _stripeMock.Setup(s => s.ParseWebhookEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new StripeWebhookEvent("checkout.session.completed", "sess-123", order.Id.ToString(), null));

        await _handler.Handle(new HandleStripeWebhookCommand("payload", "sig"), CancellationToken.None);

        var updated = await _dbContext.Orders.FindAsync(order.Id);
        updated!.Status.Should().Be(OrderStatus.Confirmed);
        updated.PaymentStatus.Should().Be(PaymentStatus.Paid);
    }

    [Fact]
    public async Task Handle_CheckoutCompleted_OrderNotFound_DoesNotThrow()
    {
        _stripeMock.Setup(s => s.ParseWebhookEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new StripeWebhookEvent("checkout.session.completed", "sess-123", Guid.NewGuid().ToString(), null));

        Func<Task> act = () => _handler.Handle(new HandleStripeWebhookCommand("payload", "sig"), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_CheckoutCompleted_InvalidOrderId_DoesNotThrow()
    {
        _stripeMock.Setup(s => s.ParseWebhookEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new StripeWebhookEvent("checkout.session.completed", "sess-123", "not-a-guid", null));

        Func<Task> act = () => _handler.Handle(new HandleStripeWebhookCommand("payload", "sig"), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_CheckoutExpired_PendingOrder_CancelsIt()
    {
        var order = await SeedOrder(OrderStatus.Pending, PaymentStatus.Pending);

        _stripeMock.Setup(s => s.ParseWebhookEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new StripeWebhookEvent("checkout.session.expired", "sess-456", order.Id.ToString(), null));

        await _handler.Handle(new HandleStripeWebhookCommand("payload", "sig"), CancellationToken.None);

        var updated = await _dbContext.Orders.FindAsync(order.Id);
        updated!.Status.Should().Be(OrderStatus.Cancelled);
        updated.PaymentStatus.Should().Be(PaymentStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_CheckoutExpired_AlreadyPaidOrder_DoesNotCancel()
    {
        var order = await SeedOrder(OrderStatus.Confirmed, PaymentStatus.Paid);

        _stripeMock.Setup(s => s.ParseWebhookEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new StripeWebhookEvent("checkout.session.expired", "sess-789", order.Id.ToString(), null));

        await _handler.Handle(new HandleStripeWebhookCommand("payload", "sig"), CancellationToken.None);

        var updated = await _dbContext.Orders.FindAsync(order.Id);
        updated!.Status.Should().Be(OrderStatus.Confirmed);
        updated.PaymentStatus.Should().Be(PaymentStatus.Paid);
    }

    [Fact]
    public async Task Handle_UnknownEventType_ReturnsUnitWithoutChangingOrders()
    {
        var order = await SeedOrder();

        _stripeMock.Setup(s => s.ParseWebhookEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new StripeWebhookEvent("payment_intent.created", null, null, "pi-123"));

        var result = await _handler.Handle(new HandleStripeWebhookCommand("payload", "sig"), CancellationToken.None);

        result.Should().Be(MediatR.Unit.Value);
        var unchanged = await _dbContext.Orders.FindAsync(order.Id);
        unchanged!.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public async Task Handle_WebhookParseThrows_ExceptionPropagates()
    {
        _stripeMock.Setup(s => s.ParseWebhookEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new InvalidOperationException("Invalid signature"));

        Func<Task> act = () => _handler.Handle(new HandleStripeWebhookCommand("bad-payload", "bad-sig"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Invalid signature");
    }

    [Fact]
    public async Task Handle_CheckoutExpired_OrderNotFound_DoesNotThrow()
    {
        _stripeMock.Setup(s => s.ParseWebhookEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new StripeWebhookEvent("checkout.session.expired", "sess-x", Guid.NewGuid().ToString(), null));

        Func<Task> act = () => _handler.Handle(new HandleStripeWebhookCommand("payload", "sig"), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_CheckoutExpired_InvalidOrderId_DoesNotThrow()
    {
        _stripeMock.Setup(s => s.ParseWebhookEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new StripeWebhookEvent("checkout.session.expired", "sess-x", "bad-guid", null));

        Func<Task> act = () => _handler.Handle(new HandleStripeWebhookCommand("payload", "sig"), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
