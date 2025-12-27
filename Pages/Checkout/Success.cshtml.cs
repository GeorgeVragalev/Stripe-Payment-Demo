using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PaymentDemo.Extensions;
using PaymentDemo.Models;
using PaymentDemo.Services.Cart;
using PaymentDemo.Services.Orders;
using PaymentDemo.Services.Stripe;

namespace PaymentDemo.Pages.Checkout;

public class SuccessModel : PageModel
{
    private readonly IOrderStore _orderStore;
    private readonly IStripeService _stripeService;
    private readonly ICartService _cartService;
    private readonly ILogger<SuccessModel> _logger;

    public SuccessModel(
        IOrderStore orderStore,
        IStripeService stripeService,
        ICartService cartService,
        ILogger<SuccessModel> logger)
    {
        _orderStore = orderStore;
        _stripeService = stripeService;
        _cartService = cartService;
        _logger = logger;
    }

    public Order? Order { get; private set; }
    public string? PaymentMessage { get; private set; }
    public bool IsAsyncPayment { get; private set; }

    public async Task<IActionResult> OnGetAsync(string? session_id)
    {
        if (string.IsNullOrEmpty(session_id))
        {
            _logger.LogWarning("Success page accessed without session_id");
            return Page();
        }

        _logger.LogInformation("Processing success callback for session {SessionId}", session_id);

        // Find the order by session ID
        Order = _orderStore.GetByStripeSessionId(session_id);

        if (Order == null)
        {
            _logger.LogWarning("No order found for session {SessionId}", session_id);
            return Page();
        }

        // IDEMPOTENCY CHECK: If order is already paid, just return it
        // This prevents double processing if user refreshes or webhook already processed
        if (Order.Status == OrderStatus.Paid)
        {
            _logger.LogInformation("Order {OrderId} is already paid, skipping payment check", Order.Id);
            PaymentMessage = "Your payment has been confirmed.";
            _cartService.ClearCart(); // Ensure cart is cleared
            return Page();
        }

        // Fetch session from Stripe to check actual payment status
        try
        {
            var session = await _stripeService.GetSessionAsync(session_id);

            if (session == null)
            {
                _logger.LogWarning("Could not retrieve session {SessionId} from Stripe", session_id);
                PaymentMessage = "Unable to verify payment status. Please check your orders.";
                return Page();
            }

            _logger.LogInformation(
                "Session {SessionId} - Status: {Status}, PaymentStatus: {PaymentStatus}",
                session_id,
                session.Status,
                session.PaymentStatus);

            // Validate session is complete
            if (!session.IsComplete())
            {
                _logger.LogWarning("Session {SessionId} is not complete (Status: {Status})", session_id, session.Status);
                PaymentMessage = "Checkout session is not complete.";
                return Page();
            }

            // Check if this is an async payment method (SEPA, BACS, etc.)
            IsAsyncPayment = session.IsAsyncPaymentMethod();

            if (session.IsPaid())
            {
                // Payment is complete - mark order as paid
                Order.Status = OrderStatus.Paid;
                Order.StripePaymentIntentId = session.PaymentIntentId;
                Order.CustomerEmail = session.CustomerDetails?.Email ?? Order.CustomerEmail;
                Order.PaidAt = DateTime.UtcNow;
                Order.UpdatedAt = DateTime.UtcNow;
                _orderStore.Update(Order);

                _logger.LogInformation("Order {OrderId} marked as Paid from success page", Order.Id);
                PaymentMessage = "Your payment has been confirmed.";
            }
            else if (session.IsProcessing())
            {
                // Async payment method - payment is being processed
                // Webhook will confirm payment later
                Order.Status = OrderStatus.PendingPayment;
                Order.StripePaymentIntentId = session.PaymentIntentId;
                Order.CustomerEmail = session.CustomerDetails?.Email ?? Order.CustomerEmail;
                Order.UpdatedAt = DateTime.UtcNow;
                _orderStore.Update(Order);

                _logger.LogInformation("Order {OrderId} marked as PendingPayment (async payment)", Order.Id);
                PaymentMessage = "Your payment is being processed. You will receive confirmation once the payment is complete.";
            }
            else
            {
                // Unpaid or other status
                _logger.LogWarning("Session {SessionId} has unexpected payment status: {Status}", session_id, session.PaymentStatus);
                PaymentMessage = $"Payment status: {session.PaymentStatus}";
            }

            // Clear the cart after successful checkout
            _cartService.ClearCart();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve/process session {SessionId}", session_id);
            PaymentMessage = "An error occurred while verifying your payment. Please check your orders.";
        }

        return Page();
    }
}
