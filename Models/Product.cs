namespace PaymentDemo.Models;

/// <summary>
/// Represents a product available for purchase
/// </summary>
public class Product
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = "usd";
    public string ImageUrl { get; init; } = string.Empty;
    public string GradientFrom { get; init; } = "from-gray-400";
    public string GradientTo { get; init; } = "to-gray-600";
    public string Icon { get; init; } = string.Empty;
    public bool IsPopular { get; init; }

    /// <summary>
    /// Price in cents for Stripe (multiply by 100)
    /// </summary>
    public long PriceInCents => (long)(Price * 100);
}

/// <summary>
/// Static product catalog for the demo
/// </summary>
public static class ProductCatalog
{
    public static readonly List<Product> Products = new()
    {
        new Product
        {
            Id = "starter",
            Name = "Starter Plan",
            Description = "Perfect for individuals getting started. Includes basic features and email support.",
            Price = 19.00m,
            Currency = "usd",
            GradientFrom = "from-green-400",
            GradientTo = "to-green-600",
            Icon = "M13 10V3L4 14h7v7l9-11h-7z"
        },
        new Product
        {
            Id = "pro",
            Name = "Pro Plan",
            Description = "For growing teams. Includes advanced features, priority support, and analytics.",
            Price = 49.00m,
            Currency = "usd",
            GradientFrom = "from-indigo-400",
            GradientTo = "to-indigo-600",
            Icon = "M9.75 17L9 20l-1 1h8l-1-1-.75-3M3 13h18M5 17h14a2 2 0 002-2V5a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z",
            IsPopular = true
        },
        new Product
        {
            Id = "enterprise",
            Name = "Enterprise Plan",
            Description = "For large organizations. Unlimited features, dedicated support, and custom integrations.",
            Price = 99.00m,
            Currency = "usd",
            GradientFrom = "from-purple-400",
            GradientTo = "to-purple-600",
            Icon = "M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4"
        }
    };

    public static Product? GetById(string id) => Products.FirstOrDefault(p => p.Id == id);
}
