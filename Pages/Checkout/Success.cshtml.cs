using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PaymentDemo.Models;
using PaymentDemo.Services.Orders;
using PaymentDemo.Services.Stripe;

namespace PaymentDemo.Pages.Checkout;

public class SuccessModel : PageModel
{
    private readonly IOrderStore _orderStore;
    private readonly IStripeService _stripeService;
    private readonly ILogger<SuccessModel> _logger;

    public SuccessModel(
        IOrderStore orderStore,
        IStripeService stripeService,
        ILogger<SuccessModel> logger)
    {
        _orderStore = orderStore;
        _stripeService = stripeService;
        _logger = logger;
    }

    public Order? Order { get; private set; }

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

        // Try to get session details from Stripe to verify payment
        try
        {
            var session = await _stripeService.GetSessionAsync(session_id);

            if (session != null)
            {
                _logger.LogInformation(
                    "Session {SessionId} status: {PaymentStatus}",
                    session_id,
                    session.PaymentStatus);

                // Update order based on session payment status
                // Note: This is a fallback. The webhook should handle this primarily.
                if (session.PaymentStatus == "paid" && Order.Status != OrderStatus.Paid)
                {
                    Order.Status = OrderStatus.Paid;
                    Order.StripePaymentIntentId = session.PaymentIntentId;
                    Order.PaidAt = DateTime.UtcNow;
                    _orderStore.Update(Order);

                    _logger.LogInformation(
                        "Updated order {OrderId} to Paid (from success page)",
                        Order.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve session {SessionId}", session_id);
            // Continue anyway - we still have the order
        }

        return Page();
    }
}
