using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PaymentDemo.Constants;
using PaymentDemo.Models;
using PaymentDemo.Services.Account;
using PaymentDemo.Services.Cart;
using PaymentDemo.Services.Credentials;
using PaymentDemo.Services.Orders;
using PaymentDemo.Services.Stripe;

namespace PaymentDemo.Pages.Cart;

public class IndexModel : PageModel
{
    private readonly ICartService _cartService;
    private readonly IStripeCredentialProvider _credentialProvider;
    private readonly IStripeService _stripeService;
    private readonly IOrderStore _orderStore;
    private readonly IUserContextService _userContextService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        ICartService cartService,
        IStripeCredentialProvider credentialProvider,
        IStripeService stripeService,
        IOrderStore orderStore,
        IUserContextService userContextService,
        ILogger<IndexModel> logger)
    {
        _cartService = cartService;
        _credentialProvider = credentialProvider;
        _stripeService = stripeService;
        _orderStore = orderStore;
        _userContextService = userContextService;
        _logger = logger;
    }

    public Models.Cart Cart { get; private set; } = new();
    public string? ErrorMessage { get; private set; }
    public string CurrencySymbol { get; private set; } = "$";

    public void OnGet()
    {
        Cart = _cartService.GetCart();
        ErrorMessage = TempData["ErrorMessage"]?.ToString();
        CurrencySymbol = _userContextService.GetCurrencySymbol();
    }

    public IActionResult OnPostUpdateQuantity(string productId, int quantity)
    {
        _cartService.UpdateQuantity(productId, quantity);
        return RedirectToPage();
    }

    public IActionResult OnPostRemoveItem(string productId)
    {
        _cartService.RemoveItem(productId);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCheckoutAsync()
    {
        if (!_credentialProvider.HasValidCredentials())
        {
            TempData["ErrorMessage"] = "Please configure Stripe credentials before checkout.";
            return RedirectToPage("/Admin/Credentials");
        }

        Cart = _cartService.GetCart();

        if (Cart.IsEmpty)
        {
            TempData["ErrorMessage"] = "Your cart is empty.";
            return RedirectToPage();
        }

        try
        {
            // Create order with user's selected currency
            var order = _orderStore.CreateOrder(Cart);
            order.Currency = _userContextService.GetCurrencyCode();

            // Build URLs
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var successUrl = $"{baseUrl}/Checkout/Success?session_id={{CHECKOUT_SESSION_ID}}";
            var cancelUrl = $"{baseUrl}/Checkout/Cancel?order_id={order.Id}";

            // Create Stripe checkout session
            var session = await _stripeService.CreateCheckoutSessionAsync(order, successUrl, cancelUrl);

            // Update order with session ID
            order.StripeSessionId = session.Id;
            order.Status = OrderStatus.Processing;
            _orderStore.Update(order);

            // Clear the cart
            _cartService.ClearCart();

            _logger.LogInformation(
                "Redirecting to Stripe checkout for order {OrderId}, session {SessionId}",
                order.Id,
                session.Id);

            // Redirect to Stripe Checkout
            return Redirect(session.Url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create checkout session");
            TempData["ErrorMessage"] = $"Failed to start checkout: {ex.Message}";
            return RedirectToPage();
        }
    }
}
