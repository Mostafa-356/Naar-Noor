using NaarNoor.Domain.Common;
using NaarNoor.Domain.Enums;

namespace NaarNoor.Domain.Entities;

public class Order : BaseEntity
{
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public OrderType Type { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? TableReservationName { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public string? StripeSessionId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}
