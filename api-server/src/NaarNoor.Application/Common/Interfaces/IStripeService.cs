namespace NaarNoor.Application.Common.Interfaces;

public interface IStripeService
{
    /// <summary>
    /// Creates a Stripe Checkout Session for the given order.
    /// Returns the hosted Checkout URL and the Stripe session ID.
    /// </summary>
    Task<StripeCheckoutResult> CreateCheckoutSessionAsync(
        StripeCheckoutRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the Stripe webhook signature and returns the event payload.
    /// Throws on invalid signature.
    /// </summary>
    Task<StripeWebhookEvent> ParseWebhookEventAsync(
        string payload,
        string stripeSignatureHeader,
        CancellationToken cancellationToken = default);
}

public record StripeCheckoutRequest(
    Guid OrderId,
    string CustomerEmail,
    string CustomerName,
    List<StripeLineItem> LineItems,
    string SuccessUrl,
    string CancelUrl
);

public record StripeLineItem(
    string Name,
    string? Description,
    decimal UnitPrice,
    int Quantity
);

public record StripeCheckoutResult(
    string SessionId,
    string SessionUrl
);

public record StripeWebhookEvent(
    string EventType,
    string? SessionId,
    string? OrderId,
    string? PaymentIntentId
);
