using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NaarNoor.Application.Common.Interfaces;
using Stripe;
using Stripe.Checkout;

namespace NaarNoor.Infrastructure.Services;

public class StripeService : IStripeService
{
    private readonly string _webhookSecret;
    private readonly ILogger<StripeService> _logger;

    public StripeService(IConfiguration configuration, ILogger<StripeService> logger)
    {
        _logger = logger;

        var secretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY")
            ?? configuration["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe secret key not configured. Set STRIPE_SECRET_KEY environment variable.");

        _webhookSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET")
            ?? configuration["Stripe:WebhookSecret"]
            ?? "";

        StripeConfiguration.ApiKey = secretKey;
    }

    public async Task<StripeCheckoutResult> CreateCheckoutSessionAsync(
        StripeCheckoutRequest request,
        CancellationToken cancellationToken = default)
    {
        var lineItems = request.LineItems.Select(item => new SessionLineItemOptions
        {
            PriceData = new SessionLineItemPriceDataOptions
            {
                Currency    = "gbp",
                UnitAmount  = (long)(item.UnitPrice * 100), // Stripe uses pence
                ProductData = new SessionLineItemPriceDataProductDataOptions
                {
                    Name        = item.Name,
                    Description = item.Description
                }
            },
            Quantity = item.Quantity
        }).ToList();

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            Mode               = "payment",
            LineItems          = lineItems,
            CustomerEmail      = request.CustomerEmail,
            SuccessUrl         = $"{request.SuccessUrl}?session_id={{CHECKOUT_SESSION_ID}}&order_id={request.OrderId}",
            CancelUrl          = $"{request.CancelUrl}?order_id={request.OrderId}",
            Metadata           = new Dictionary<string, string>
            {
                ["orderId"]      = request.OrderId.ToString(),
                ["customerName"] = request.CustomerName
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

        _logger.LogInformation("Stripe checkout session created: {SessionId} for order {OrderId}", session.Id, request.OrderId);

        return new StripeCheckoutResult(session.Id, session.Url);
    }

    public Task<StripeWebhookEvent> ParseWebhookEventAsync(
        string payload,
        string stripeSignatureHeader,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_webhookSecret))
        {
            // In development, allow unsigned webhooks only when explicitly opted in
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            if (!env.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "STRIPE_WEBHOOK_SECRET is required. Set the environment variable to enable webhook verification.");
            }

            _logger.LogWarning("STRIPE_WEBHOOK_SECRET not configured — skipping signature validation (development only).");
            var unsignedEvent = EventUtility.ParseEvent(payload);
            return Task.FromResult(MapEvent(unsignedEvent));
        }

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(payload, stripeSignatureHeader, _webhookSecret);
            return Task.FromResult(MapEvent(stripeEvent));
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook signature validation failed");
            throw;
        }
    }

    private static StripeWebhookEvent MapEvent(Event stripeEvent)
    {
        string? sessionId    = null;
        string? orderId      = null;
        string? paymentIntentId = null;

        if (stripeEvent.Data.Object is Session session)
        {
            sessionId = session.Id;
            session.Metadata?.TryGetValue("orderId", out orderId);
            paymentIntentId = session.PaymentIntentId;
        }

        return new StripeWebhookEvent(
            EventType:       stripeEvent.Type,
            SessionId:       sessionId,
            OrderId:         orderId,
            PaymentIntentId: paymentIntentId
        );
    }
}
