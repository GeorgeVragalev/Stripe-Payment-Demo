namespace PaymentDemo.Models;

/// <summary>
/// Represents an item in the shopping cart
/// </summary>
public class CartItem
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;

    public decimal Total => UnitPrice * Quantity;
}

/// <summary>
/// Shopping cart containing multiple items
/// </summary>
public class Cart
{
    public List<CartItem> Items { get; set; } = new();

    public decimal Subtotal => Items.Sum(i => i.Total);
    public decimal Tax => Math.Round(Subtotal * 0.08m, 2); // 8% tax for demo
    public decimal Total => Subtotal + Tax;
    public int ItemCount => Items.Sum(i => i.Quantity);
    public bool IsEmpty => Items.Count == 0;
}
