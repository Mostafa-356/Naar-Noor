using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaarNoor.Application.Orders.Commands.CreateStripeCheckoutSession;
using NaarNoor.Application.Orders.Commands.HandleStripeWebhook;

namespace NaarNoor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a Stripe Checkout Session. Returns the hosted checkout URL.
    /// </summary>
    [Authorize]  // ✅ SECURITY: Require authentication for checkout
    [HttpPost("create-checkout-session")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateCheckoutSession(
        [FromBody] CreateStripeCheckoutSessionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(new { orderId = result.OrderId, sessionUrl = result.SessionUrl });
    }

    /// <summary>
    /// Stripe webhook endpoint. Called by Stripe on payment events.
    /// Must NOT require authentication — Stripe calls this directly.
    /// </summary>
    [AllowAnonymous]  // ✅ EXPLICIT: Webhook is intentionally public
    [HttpPost("webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(HttpContext.Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);

        var signatureHeader = Request.Headers["Stripe-Signature"].FirstOrDefault() ?? "";

        try
        {
            await _mediator.Send(new HandleStripeWebhookCommand(payload, signatureHeader), cancellationToken);
            return Ok(new { received = true });
        }
        catch (Stripe.StripeException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
