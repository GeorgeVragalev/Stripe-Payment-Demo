using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PaymentDemo.Models;
using PaymentDemo.Services.Cart;
using PaymentDemo.Services.Credentials;

namespace PaymentDemo.Pages.Shop;

public class IndexModel : PageModel
{
    private readonly IStripeCredentialProvider _credentialProvider;
    private readonly ICartService _cartService;

    public IndexModel(
        IStripeCredentialProvider credentialProvider,
        ICartService cartService)
    {
        _credentialProvider = credentialProvider;
        _cartService = cartService;
    }

    public bool HasCredentials { get; private set; }
    public List<Product> Products => ProductCatalog.Products;
    public string? SuccessMessage { get; private set; }

    public void OnGet()
    {
        HasCredentials = _credentialProvider.HasValidCredentials();
        SuccessMessage = TempData["SuccessMessage"]?.ToString();
    }

    public IActionResult OnPostAddToCart(string productId)
    {
        if (!_credentialProvider.HasValidCredentials())
        {
            return RedirectToPage("/Admin/Credentials");
        }

        var product = ProductCatalog.GetById(productId);
        if (product == null)
        {
            return RedirectToPage();
        }

        _cartService.AddItem(productId);

        TempData["SuccessMessage"] = $"Added {product.Name} to your cart!";
        return RedirectToPage();
    }
}
