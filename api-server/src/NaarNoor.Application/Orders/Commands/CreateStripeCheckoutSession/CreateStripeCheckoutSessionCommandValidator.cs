using FluentValidation;

namespace NaarNoor.Application.Orders.Commands.CreateStripeCheckoutSession;

public class CreateStripeCheckoutSessionCommandValidator
    : AbstractValidator<CreateStripeCheckoutSessionCommand>
{
    private static readonly HashSet<string> AllowedTypes =
        new(StringComparer.OrdinalIgnoreCase) { "collection", "delivery", "dine-in" };

    public CreateStripeCheckoutSessionCommandValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Name is required.")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .MinimumLength(7).WithMessage("Phone number must be at least 7 digits.");

        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => AllowedTypes.Contains(t))
            .WithMessage("Order type must be 'collection', 'delivery', or 'dine-in'.");

        RuleFor(x => x.DeliveryAddress)
            .NotEmpty().MinimumLength(5)
            .When(x => x.Type.Equals("delivery", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Delivery address is required for delivery orders.");

        RuleFor(x => x.TableReservationName)
            .NotEmpty().MinimumLength(2)
            .When(x => x.Type.Equals("dine-in", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Table/reservation name is required for dine-in orders.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.MenuItemId)
                .NotEmpty().WithMessage("Menu item ID is required.");

            item.RuleFor(i => i.MenuItemName)
                .NotEmpty().WithMessage("Menu item name is required.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
                .LessThanOrEqualTo(20).WithMessage("Quantity cannot exceed 20 per item.");
        });

        RuleFor(x => x.SuccessUrl)
            .NotEmpty().WithMessage("Success URL is required.")
            .Must(IsValidHttpUrl).WithMessage("Success URL must be a valid HTTP/HTTPS URL.");

        RuleFor(x => x.CancelUrl)
            .NotEmpty().WithMessage("Cancel URL is required.")
            .Must(IsValidHttpUrl).WithMessage("Cancel URL must be a valid HTTP/HTTPS URL.");
    }

    private static bool IsValidHttpUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
