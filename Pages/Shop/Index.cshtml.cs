using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PaymentDemo.Configuration;
using PaymentDemo.Constants;
using PaymentDemo.Models;
using PaymentDemo.Services.Account;
using PaymentDemo.Services.Cart;
using PaymentDemo.Services.Credentials;

namespace PaymentDemo.Pages.Shop;

public class IndexModel : PageModel
{
    private readonly IStripeCredentialProvider _credentialProvider;
    private readonly ICartService _cartService;
    private readonly IUserContextService _userContextService;

    public IndexModel(
        IStripeCredentialProvider credentialProvider,
        ICartService cartService,
        IUserContextService userContextService)
    {
        _credentialProvider = credentialProvider;
        _cartService = cartService;
        _userContextService = userContextService;
    }

    public bool HasCredentials { get; private set; }
    public List<Product> Products => ProductCatalog.Products;
    public string? SuccessMessage { get; private set; }
    public CurrencyType Currency { get; private set; }
    public string CurrencySymbol => Currency.GetSymbol();

    public void OnGet()
    {
        HasCredentials = _credentialProvider.HasValidCredentials();
        SuccessMessage = TempData["SuccessMessage"]?.ToString();
        Currency = _userContextService.GetCurrency();
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
