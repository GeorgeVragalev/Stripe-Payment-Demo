using Microsoft.AspNetCore.Mvc.RazorPages;
using PaymentDemo.Models;
using PaymentDemo.Services.Orders;

namespace PaymentDemo.Pages.Checkout;

public class CancelModel : PageModel
{
    private readonly IOrderStore _orderStore;
    private readonly ILogger<CancelModel> _logger;

    public CancelModel(
        IOrderStore orderStore,
        ILogger<CancelModel> logger)
    {
        _orderStore = orderStore;
        _logger = logger;
    }

    public Order? Order { get; private set; }

    public void OnGet(string? order_id)
    {
        if (!string.IsNullOrEmpty(order_id))
        {
            _logger.LogInformation("Checkout cancelled for order {OrderId}", order_id);

            Order = _orderStore.GetById(order_id);

            if (Order != null && Order.Status == OrderStatus.Processing)
            {
                Order.Status = OrderStatus.Cancelled;
                _orderStore.Update(Order);
            }
        }
    }
}
