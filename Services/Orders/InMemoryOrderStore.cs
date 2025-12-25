using System.Collections.Concurrent;
using PaymentDemo.Models;

namespace PaymentDemo.Services.Orders;

/// <summary>
/// In-memory order store for demo purposes.
/// In production, replace with a proper database (Redis, PostgreSQL, etc.)
/// </summary>
public class InMemoryOrderStore : IOrderStore
{
    private readonly ConcurrentDictionary<string, Order> _orders = new();
    private readonly ILogger<InMemoryOrderStore> _logger;
    private int _orderCounter = 0;

    public InMemoryOrderStore(ILogger<InMemoryOrderStore> logger)
    {
        _logger = logger;
    }

    public Order CreateOrder(Models.Cart cart, string? customerEmail = null)
    {
        var orderNumber = Interlocked.Increment(ref _orderCounter);
        var orderId = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{orderNumber:D4}";

        var order = new Order
        {
            Id = orderId,
            Items = cart.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity
            }).ToList(),
            Subtotal = cart.Subtotal,
            Tax = cart.Tax,
            Total = cart.Total,
            CustomerEmail = customerEmail,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _orders[orderId] = order;
        _logger.LogInformation("Created order {OrderId} with total {Total}", orderId, order.Total);

        return order;
    }

    public Order? GetById(string orderId)
    {
        _orders.TryGetValue(orderId, out var order);
        return order;
    }

    public Order? GetByStripeSessionId(string sessionId)
    {
        return _orders.Values.FirstOrDefault(o => o.StripeSessionId == sessionId);
    }

    public IEnumerable<Order> GetAll()
    {
        return _orders.Values.OrderByDescending(o => o.CreatedAt);
    }

    public void Update(Order order)
    {
        order.UpdatedAt = DateTime.UtcNow;
        _orders[order.Id] = order;
        _logger.LogInformation("Updated order {OrderId}", order.Id);
    }

    public void UpdateStatus(string orderId, OrderStatus status, string? paymentIntentId = null)
    {
        if (_orders.TryGetValue(orderId, out var order))
        {
            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                order.StripePaymentIntentId = paymentIntentId;
            }

            if (status == OrderStatus.Paid)
            {
                order.PaidAt = DateTime.UtcNow;
            }

            _logger.LogInformation("Updated order {OrderId} status to {Status}", orderId, status);
        }
    }

    public void MarkAsPaid(string orderId, string paymentIntentId)
    {
        UpdateStatus(orderId, OrderStatus.Paid, paymentIntentId);
    }
}
