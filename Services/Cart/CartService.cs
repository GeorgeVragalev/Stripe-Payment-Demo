using System.Text.Json;
using PaymentDemo.Models;

namespace PaymentDemo.Services.Cart;

/// <summary>
/// Shopping cart service that stores cart data in session
/// </summary>
public class CartService : ICartService
{
    private const string CartSessionKey = "ShoppingCart";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Models.Cart GetCart()
    {
        var session = GetSession();
        if (session == null)
            return new Models.Cart();

        var cartJson = session.GetString(CartSessionKey);
        if (string.IsNullOrEmpty(cartJson))
            return new Models.Cart();

        try
        {
            return JsonSerializer.Deserialize<Models.Cart>(cartJson) ?? new Models.Cart();
        }
        catch
        {
            return new Models.Cart();
        }
    }

    public void AddItem(string productId, int quantity = 1)
    {
        var product = ProductCatalog.GetById(productId);
        if (product == null)
            return;

        var cart = GetCart();
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = quantity
            });
        }

        SaveCart(cart);
    }

    public void UpdateQuantity(string productId, int quantity)
    {
        var cart = GetCart();
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

        if (item == null)
            return;

        if (quantity <= 0)
        {
            cart.Items.Remove(item);
        }
        else
        {
            item.Quantity = quantity;
        }

        SaveCart(cart);
    }

    public void RemoveItem(string productId)
    {
        var cart = GetCart();
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

        if (item != null)
        {
            cart.Items.Remove(item);
            SaveCart(cart);
        }
    }

    public void ClearCart()
    {
        var session = GetSession();
        session?.Remove(CartSessionKey);
    }

    public int GetItemCount()
    {
        return GetCart().ItemCount;
    }

    private void SaveCart(Models.Cart cart)
    {
        var session = GetSession();
        if (session == null)
            return;

        var cartJson = JsonSerializer.Serialize(cart);
        session.SetString(CartSessionKey, cartJson);
    }

    private ISession? GetSession()
    {
        return _httpContextAccessor.HttpContext?.Session;
    }
}
