using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NaarNoor.Application.Common.Interfaces;
using NaarNoor.Domain.Enums;

namespace NaarNoor.Application.Orders.Commands.HandleStripeWebhook;

public class HandleStripeWebhookCommandHandler : IRequestHandler<HandleStripeWebhookCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStripeService _stripe;
    private readonly ILogger<HandleStripeWebhookCommandHandler> _logger;

    public HandleStripeWebhookCommandHandler(
        IUnitOfWork unitOfWork,
        IStripeService stripe,
        ILogger<HandleStripeWebhookCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _stripe = stripe;
        _logger = logger;
    }

    public async Task<Unit> Handle(HandleStripeWebhookCommand request, CancellationToken cancellationToken)
    {
        StripeWebhookEvent webhookEvent;
        try
        {
            webhookEvent = await _stripe.ParseWebhookEventAsync(
                request.Payload,
                request.StripeSignatureHeader,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Stripe webhook signature validation failed");
            throw;
        }

        _logger.LogInformation("Received Stripe event: {EventType}", webhookEvent.EventType);

        switch (webhookEvent.EventType)
        {
            case "checkout.session.completed":
                await HandleCheckoutCompletedAsync(webhookEvent, cancellationToken);
                break;

            case "checkout.session.expired":
                await HandleCheckoutExpiredAsync(webhookEvent, cancellationToken);
                break;

            default:
                _logger.LogDebug("Unhandled Stripe event type: {EventType}", webhookEvent.EventType);
                break;
        }

        return Unit.Value;
    }

    private async Task HandleCheckoutCompletedAsync(StripeWebhookEvent webhookEvent, CancellationToken ct)
    {
        if (!Guid.TryParse(webhookEvent.OrderId, out var orderId))
        {
            _logger.LogWarning("checkout.session.completed missing orderId metadata. SessionId: {SessionId}", webhookEvent.SessionId);
            return;
        }

        var order = await _unitOfWork.Orders.Query()
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);
        if (order is null)
        {
            _logger.LogWarning("Order {OrderId} not found for Stripe session {SessionId}", orderId, webhookEvent.SessionId);
            return;
        }

        order.Status        = OrderStatus.Confirmed;
        order.PaymentStatus = PaymentStatus.Paid;
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Order {OrderId} confirmed and marked as paid via Stripe session {SessionId}", orderId, webhookEvent.SessionId);
    }

    private async Task HandleCheckoutExpiredAsync(StripeWebhookEvent webhookEvent, CancellationToken ct)
    {
        if (!Guid.TryParse(webhookEvent.OrderId, out var orderId))
            return;

        var order = await _unitOfWork.Orders.Query()
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);
        if (order is null) return;

        if (order.PaymentStatus == PaymentStatus.Pending)
        {
            order.Status        = OrderStatus.Cancelled;
            order.PaymentStatus = PaymentStatus.Cancelled;
            await _unitOfWork.SaveChangesAsync(ct);
            _logger.LogInformation("Order {OrderId} cancelled due to expired Stripe session", orderId);
        }
    }
}
