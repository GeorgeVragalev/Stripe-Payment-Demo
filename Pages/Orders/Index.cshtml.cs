using Microsoft.AspNetCore.Mvc.RazorPages;
using PaymentDemo.Models;
using PaymentDemo.Services.Orders;

namespace PaymentDemo.Pages.Orders;

public class IndexModel : PageModel
{
    private readonly IOrderStore _orderStore;

    public IndexModel(IOrderStore orderStore)
    {
        _orderStore = orderStore;
    }

    public IEnumerable<Order> Orders { get; private set; } = Enumerable.Empty<Order>();

    public void OnGet()
    {
        Orders = _orderStore.GetAll();
    }
}
