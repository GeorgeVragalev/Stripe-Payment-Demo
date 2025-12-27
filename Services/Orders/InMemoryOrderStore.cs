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
        SeedDemoOrders();
    }

    private void SeedDemoOrders()
    {
        var demoOrders = new[]
        {
            new Order
            {
                Id = "ORD-20251220-0001",
                Items = new List<OrderItem>
                {
                    new() { ProductId = "pro", ProductName = "Pro Plan", UnitPrice = 49.00m, Quantity = 1 }
                },
                Subtotal = 49.00m,
                Tax = 3.92m,
                Total = 52.92m,
                Status = OrderStatus.Paid,
                CustomerEmail = "john.doe@example.com",
                StripeSessionId = "cs_demo_abc123",
                StripePaymentIntentId = "pi_demo_xyz789",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                PaidAt = DateTime.UtcNow.AddDays(-5).AddMinutes(2),
                UpdatedAt = DateTime.UtcNow.AddDays(-5).AddMinutes(2)
            },
            new Order
            {
                Id = "ORD-20251222-0002",
                Items = new List<OrderItem>
                {
                    new() { ProductId = "starter", ProductName = "Starter Plan", UnitPrice = 19.00m, Quantity = 2 },
                    new() { ProductId = "pro", ProductName = "Pro Plan", UnitPrice = 49.00m, Quantity = 1 }
                },
                Subtotal = 87.00m,
                Tax = 6.96m,
                Total = 93.96m,
                Status = OrderStatus.Paid,
                CustomerEmail = "jane.smith@example.com",
                StripeSessionId = "cs_demo_def456",
                StripePaymentIntentId = "pi_demo_uvw321",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                PaidAt = DateTime.UtcNow.AddDays(-3).AddMinutes(1),
                UpdatedAt = DateTime.UtcNow.AddDays(-3).AddMinutes(1)
            },
            new Order
            {
                Id = "ORD-20251224-0003",
                Items = new List<OrderItem>
                {
                    new() { ProductId = "enterprise", ProductName = "Enterprise Plan", UnitPrice = 99.00m, Quantity = 1 }
                },
                Subtotal = 99.00m,
                Tax = 7.92m,
                Total = 106.92m,
                Status = OrderStatus.Pending,
                CustomerEmail = "pending@example.com",
                StripeSessionId = "cs_demo_pending",
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                UpdatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new Order
            {
                Id = "ORD-20251223-0004",
                Items = new List<OrderItem>
                {
                    new() { ProductId = "pro", ProductName = "Pro Plan", UnitPrice = 49.00m, Quantity = 3 }
                },
                Subtotal = 147.00m,
                Tax = 11.76m,
                Total = 158.76m,
                Status = OrderStatus.Failed,
                CustomerEmail = "failed@example.com",
                StripeSessionId = "cs_demo_failed",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2).AddMinutes(5)
            }
        };

        foreach (var order in demoOrders)
        {
            _orders[order.Id] = order;
        }

        _orderCounter = 4;
        _logger.LogInformation("Seeded {Count} demo orders", demoOrders.Length);
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
