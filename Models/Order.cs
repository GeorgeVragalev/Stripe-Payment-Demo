namespace PaymentDemo.Models;

/// <summary>
/// Represents an order in the system
/// </summary>
public class Order
{
    public string Id { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = "usd";
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? StripeSessionId { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public string? CustomerEmail { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string StatusDisplay => Status switch
    {
        OrderStatus.Pending => "Pending",
        OrderStatus.Processing => "Processing",
        OrderStatus.Paid => "Paid",
        OrderStatus.Failed => "Failed",
        OrderStatus.Cancelled => "Cancelled",
        OrderStatus.Refunded => "Refunded",
        _ => "Unknown"
    };

    public string StatusBadgeClass => Status switch
    {
        OrderStatus.Pending => "badge-warning",
        OrderStatus.Processing => "badge-info",
        OrderStatus.Paid => "badge-success",
        OrderStatus.Failed => "badge-error",
        OrderStatus.Cancelled => "badge-error",
        OrderStatus.Refunded => "badge-purple",
        _ => "badge-info"
    };
}

public class OrderItem
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Total => UnitPrice * Quantity;
}

public enum OrderStatus
{
    Pending,
    Processing,
    Paid,
    Failed,
    Cancelled,
    Refunded
}
