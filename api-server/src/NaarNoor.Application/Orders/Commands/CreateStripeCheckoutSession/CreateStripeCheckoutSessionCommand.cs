using MediatR;

namespace NaarNoor.Application.Orders.Commands.CreateStripeCheckoutSession;

public record CreateStripeCheckoutSessionCommand(
    string CustomerName,
    string Email,
    string PhoneNumber,
    string? Notes,
    string Type,
    string? DeliveryAddress,
    string? TableReservationName,
    List<CheckoutOrderItemRequest> Items,
    string SuccessUrl,
    string CancelUrl
) : IRequest<CreateStripeCheckoutSessionResponse>;

public record CheckoutOrderItemRequest(
    Guid MenuItemId,
    string MenuItemName,
    decimal UnitPrice,
    int Quantity
);

public record CreateStripeCheckoutSessionResponse(
    Guid OrderId,
    string SessionUrl
);
