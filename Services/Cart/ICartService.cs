using PaymentDemo.Models;

namespace PaymentDemo.Services.Cart;

/// <summary>
/// Service for managing the shopping cart stored in session
/// </summary>
public interface ICartService
{
    /// <summary>
    /// Get the current cart
    /// </summary>
    Models.Cart GetCart();

    /// <summary>
    /// Add a product to the cart
    /// </summary>
    void AddItem(string productId, int quantity = 1);

    /// <summary>
    /// Update the quantity of an item
    /// </summary>
    void UpdateQuantity(string productId, int quantity);

    /// <summary>
    /// Remove an item from the cart
    /// </summary>
    void RemoveItem(string productId);

    /// <summary>
    /// Clear all items from the cart
    /// </summary>
    void ClearCart();

    /// <summary>
    /// Get the number of items in the cart
    /// </summary>
    int GetItemCount();
}
