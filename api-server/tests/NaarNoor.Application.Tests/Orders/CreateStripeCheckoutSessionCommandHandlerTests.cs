using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NaarNoor.Application.Common.Interfaces;
using NaarNoor.Application.Orders.Commands.CreateStripeCheckoutSession;
using NaarNoor.Application.Tests.Common.Fixtures;
using NaarNoor.Domain.Entities;
using NaarNoor.Domain.Enums;
using NaarNoor.Infrastructure.Data;
using Xunit;

namespace NaarNoor.Application.Tests.Orders;

public class CreateStripeCheckoutSessionCommandHandlerTests : ApplicationLayerTestBase, IAsyncLifetime
{
    private ApplicationDbContext _dbContext = null!;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IStripeService> _stripeMock;

    public CreateStripeCheckoutSessionCommandHandlerTests()
    {
        _unitOfWorkMock = CreateRepositoryMock<IUnitOfWork>();
        _stripeMock = CreateServiceMock<IStripeService>();
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("StripeSession_" + Guid.NewGuid())
            .Options;
        _dbContext = new ApplicationDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync() => await _dbContext.DisposeAsync();

    private CreateStripeCheckoutSessionCommandHandler CreateHandler()
    {
        var capturedOrders = new List<Order>();
        var orderRepo = new MockOrderRepository(capturedOrders);
        _unitOfWorkMock.Setup(x => x.Orders).Returns(orderRepo);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return new CreateStripeCheckoutSessionCommandHandler(_unitOfWorkMock.Object, _dbContext, _stripeMock.Object);
    }

    private async Task<MenuItem> SeedMenuItem(string name, decimal price, bool isAvailable = true)
    {
        var item = new MenuItem { Name = name, Description = "Desc", Price = price, Category = MenuCategory.Mains, IsAvailable = isAvailable };
        _dbContext.MenuItems.Add(item);
        await _dbContext.SaveChangesAsync();
        return item;
    }

    [Fact]
    public async Task Handle_WithAvailableItems_CreatesOrderAndCallsStripe()
    {
        var menuItem = await SeedMenuItem("Dal Bhat", 14.95m);
        _stripeMock.Setup(s => s.CreateCheckoutSessionAsync(It.IsAny<StripeCheckoutRequest>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new StripeCheckoutResult("sess_abc", "https://checkout.stripe.com/sess_abc"));

        var handler = CreateHandler();
        var command = new CreateStripeCheckoutSessionCommand(
            CustomerName: "John Doe",
            Email: "john@test.com",
            PhoneNumber: "07700000000",
            Notes: null,
            Type: "collection",
            DeliveryAddress: null,
            TableReservationName: null,
            Items: new List<CheckoutOrderItemRequest> { new(menuItem.Id, menuItem.Name, menuItem.Price, 2) },
            SuccessUrl: "https://example.com/success",
            CancelUrl: "https://example.com/cancel"
        );

        var result = await handler.Handle(command, CancellationToken.None);

        result.SessionUrl.Should().Be("https://checkout.stripe.com/sess_abc");
        result.OrderId.Should().NotBe(Guid.Empty);
        _stripeMock.Verify(s => s.CreateCheckoutSessionAsync(It.IsAny<StripeCheckoutRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_WithUnavailableItem_ThrowsInvalidOperationException()
    {
        var unavailableItem = await SeedMenuItem("Unavailable Dish", 9.99m, isAvailable: false);
        var handler = CreateHandler();

        var command = new CreateStripeCheckoutSessionCommand(
            CustomerName: "Jane Doe",
            Email: "jane@test.com",
            PhoneNumber: "07700000001",
            Notes: null,
            Type: "collection",
            DeliveryAddress: null,
            TableReservationName: null,
            Items: new List<CheckoutOrderItemRequest> { new(unavailableItem.Id, unavailableItem.Name, 9.99m, 1) },
            SuccessUrl: "https://example.com/success",
            CancelUrl: "https://example.com/cancel"
        );

        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not available*");
    }

    [Fact]
    public async Task Handle_WithNonExistentItem_ThrowsInvalidOperationException()
    {
        var handler = CreateHandler();
        var command = new CreateStripeCheckoutSessionCommand(
            CustomerName: "Bob",
            Email: "bob@test.com",
            PhoneNumber: "07700000002",
            Notes: null,
            Type: "collection",
            DeliveryAddress: null,
            TableReservationName: null,
            Items: new List<CheckoutOrderItemRequest> { new(Guid.NewGuid(), "Ghost Item", 5.00m, 1) },
            SuccessUrl: "https://example.com/success",
            CancelUrl: "https://example.com/cancel"
        );

        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_DeliveryOrder_SetsCorrectOrderType()
    {
        var menuItem = await SeedMenuItem("Biryani", 16.95m);
        var capturedOrders = new List<Order>();
        var orderRepo = new MockOrderRepository(capturedOrders);
        _unitOfWorkMock.Setup(x => x.Orders).Returns(orderRepo);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _stripeMock.Setup(s => s.CreateCheckoutSessionAsync(It.IsAny<StripeCheckoutRequest>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new StripeCheckoutResult("sess_def", "https://checkout.stripe.com/sess_def"));

        var handler = new CreateStripeCheckoutSessionCommandHandler(_unitOfWorkMock.Object, _dbContext, _stripeMock.Object);
        var command = new CreateStripeCheckoutSessionCommand(
            CustomerName: "Ali",
            Email: "ali@test.com",
            PhoneNumber: "07700000003",
            Notes: null,
            Type: "delivery",
            DeliveryAddress: "123 Main Street",
            TableReservationName: null,
            Items: new List<CheckoutOrderItemRequest> { new(menuItem.Id, menuItem.Name, menuItem.Price, 1) },
            SuccessUrl: "https://example.com/success",
            CancelUrl: "https://example.com/cancel"
        );

        await handler.Handle(command, CancellationToken.None);

        capturedOrders.Should().ContainSingle(o => o.Type == OrderType.Delivery);
    }

    [Fact]
    public async Task Handle_DineInOrder_SetsCorrectOrderType()
    {
        var menuItem = await SeedMenuItem("Kheer", 6.95m);
        var capturedOrders = new List<Order>();
        var orderRepo = new MockOrderRepository(capturedOrders);
        _unitOfWorkMock.Setup(x => x.Orders).Returns(orderRepo);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _stripeMock.Setup(s => s.CreateCheckoutSessionAsync(It.IsAny<StripeCheckoutRequest>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new StripeCheckoutResult("sess_ghi", "https://checkout.stripe.com/sess_ghi"));

        var handler = new CreateStripeCheckoutSessionCommandHandler(_unitOfWorkMock.Object, _dbContext, _stripeMock.Object);
        var command = new CreateStripeCheckoutSessionCommand(
            CustomerName: "Sara",
            Email: "sara@test.com",
            PhoneNumber: "07700000004",
            Notes: null,
            Type: "dine-in",
            DeliveryAddress: null,
            TableReservationName: "Sara party",
            Items: new List<CheckoutOrderItemRequest> { new(menuItem.Id, menuItem.Name, menuItem.Price, 3) },
            SuccessUrl: "https://example.com/success",
            CancelUrl: "https://example.com/cancel"
        );

        await handler.Handle(command, CancellationToken.None);

        capturedOrders.Should().ContainSingle(o => o.Type == OrderType.DineIn);
    }

    [Fact]
    public async Task Handle_UsesServerSidePrices_NotClientSidePrices()
    {
        var menuItem = await SeedMenuItem("Sel Roti", 4.50m);
        StripeCheckoutRequest? capturedRequest = null;
        _stripeMock.Setup(s => s.CreateCheckoutSessionAsync(It.IsAny<StripeCheckoutRequest>(), It.IsAny<CancellationToken>()))
                   .Callback<StripeCheckoutRequest, CancellationToken>((req, _) => capturedRequest = req)
                   .ReturnsAsync(new StripeCheckoutResult("sess_jkl", "https://stripe.com/sess_jkl"));

        var handler = CreateHandler();
        var command = new CreateStripeCheckoutSessionCommand(
            CustomerName: "Priya",
            Email: "priya@test.com",
            PhoneNumber: "07700000005",
            Notes: null,
            Type: "collection",
            DeliveryAddress: null,
            TableReservationName: null,
            Items: new List<CheckoutOrderItemRequest> { new(menuItem.Id, menuItem.Name, 99.99m, 2) },
            SuccessUrl: "https://example.com/success",
            CancelUrl: "https://example.com/cancel"
        );

        await handler.Handle(command, CancellationToken.None);

        capturedRequest.Should().NotBeNull();
        capturedRequest!.LineItems.Should().ContainSingle(li => li.UnitPrice == 4.50m,
            "Should use server-side price, not client-submitted price of 99.99");
    }

    private class MockOrderRepository : IRepository<Order>
    {
        private readonly List<Order> _orders;
        public MockOrderRepository(List<Order> orders) => _orders = orders;

        public IQueryable<Order> Query() => _orders.AsQueryable();
        public void Add(Order entity) => _orders.Add(entity);
        public void Remove(Order entity) => _orders.Remove(entity);
        public void Update(Order entity) { }
    }
}
