using MediatR;

namespace NaarNoor.Application.Orders.Commands.HandleStripeWebhook;

public record HandleStripeWebhookCommand(
    string Payload,
    string StripeSignatureHeader
) : IRequest<Unit>;
