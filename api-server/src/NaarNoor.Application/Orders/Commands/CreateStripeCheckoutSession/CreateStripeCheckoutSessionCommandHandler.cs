using MediatR;
using Microsoft.EntityFrameworkCore;
using NaarNoor.Application.Common.Interfaces;
using NaarNoor.Domain.Entities;
using NaarNoor.Domain.Enums;

namespace NaarNoor.Application.Orders.Commands.CreateStripeCheckoutSession;

public class CreateStripeCheckoutSessionCommandHandler
    : IRequestHandler<CreateStripeCheckoutSessionCommand, CreateStripeCheckoutSessionResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApplicationDbContext _db;
    private readonly IStripeService _stripe;

    public CreateStripeCheckoutSessionCommandHandler(
        IUnitOfWork unitOfWork,
        IApplicationDbContext db,
        IStripeService stripe)
    {
        _unitOfWork = unitOfWork;
        _db = db;
        _stripe = stripe;
    }

    public async Task<CreateStripeCheckoutSessionResponse> Handle(
        CreateStripeCheckoutSessionCommand request,
        CancellationToken cancellationToken)
    {
        // --- Resolve authoritative prices from the database ---
        var requestedIds = request.Items.Select(i => i.MenuItemId).Distinct().ToList();

        var menuItems = await _db.MenuItems
            .Where(m => requestedIds.Contains(m.Id) && m.IsAvailable)
            .ToDictionaryAsync(m => m.Id, cancellationToken);

        // Validate all requested items exist and are available
        var missingIds = requestedIds.Where(id => !menuItems.ContainsKey(id)).ToList();
        if (missingIds.Count > 0)
        {
            throw new InvalidOperationException(
                $"The following menu items are not available: {string.Join(", ", missingIds)}");
        }

        var orderType = request.Type.ToLowerInvariant() switch
        {
            "delivery" => OrderType.Delivery,
            "dine-in"  => OrderType.DineIn,
            _          => OrderType.Collection
        };

        // Build order items using server-side prices only
        var items = request.Items.Select(i =>
        {
            var menuItem = menuItems[i.MenuItemId];
            return new OrderItem
            {
                MenuItemId   = menuItem.Id,
                MenuItemName = menuItem.Name,
                UnitPrice    = menuItem.Price, // authoritative price from DB
                Quantity     = i.Quantity
            };
        }).ToList();

        var order = new Order
        {
            CustomerName         = request.CustomerName,
            Email                = request.Email,
            PhoneNumber          = request.PhoneNumber,
            Notes                = request.Notes,
            Type                 = orderType,
            DeliveryAddress      = request.DeliveryAddress,
            TableReservationName = request.TableReservationName,
            TotalAmount          = items.Sum(i => i.UnitPrice * i.Quantity),
            Items                = items,
            Status               = OrderStatus.Pending,
            PaymentStatus        = PaymentStatus.Pending
        };

        _unitOfWork.Orders.Add(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Build Stripe line items using authoritative server-side prices
        var stripeLineItems = items.Select(i => new StripeLineItem(
            Name:        i.MenuItemName,
            Description: null,
            UnitPrice:   i.UnitPrice,
            Quantity:    i.Quantity
        )).ToList();

        var checkoutResult = await _stripe.CreateCheckoutSessionAsync(
            new StripeCheckoutRequest(
                OrderId:       order.Id,
                CustomerEmail: request.Email,
                CustomerName:  request.CustomerName,
                LineItems:     stripeLineItems,
                SuccessUrl:    request.SuccessUrl,
                CancelUrl:     request.CancelUrl
            ),
            cancellationToken
        );

        // Persist the session ID against the order
        order.StripeSessionId = checkoutResult.SessionId;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateStripeCheckoutSessionResponse(order.Id, checkoutResult.SessionUrl);
    }
}
