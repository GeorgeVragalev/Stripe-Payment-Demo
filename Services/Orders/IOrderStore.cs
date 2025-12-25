using PaymentDemo.Models;

namespace PaymentDemo.Services.Orders;

/// <summary>
/// Interface for storing and retrieving orders
/// </summary>
public interface IOrderStore
{
    /// <summary>
    /// Create a new order
    /// </summary>
    Order CreateOrder(Models.Cart cart, string? customerEmail = null);

    /// <summary>
    /// Get an order by ID
    /// </summary>
    Order? GetById(string orderId);

    /// <summary>
    /// Get an order by Stripe session ID
    /// </summary>
    Order? GetByStripeSessionId(string sessionId);

    /// <summary>
    /// Get all orders (for demo purposes)
    /// </summary>
    IEnumerable<Order> GetAll();

    /// <summary>
    /// Update an order
    /// </summary>
    void Update(Order order);

    /// <summary>
    /// Update order status
    /// </summary>
    void UpdateStatus(string orderId, OrderStatus status, string? paymentIntentId = null);

    /// <summary>
    /// Mark an order as paid
    /// </summary>
    void MarkAsPaid(string orderId, string paymentIntentId);
}
