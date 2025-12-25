using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PaymentDemo.Services.Credentials;

namespace PaymentDemo.Pages.Admin;

public class WebhooksModel : PageModel
{
    private readonly IStripeCredentialProvider _credentialProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WebhooksModel(
        IStripeCredentialProvider credentialProvider,
        IHttpContextAccessor httpContextAccessor)
    {
        _credentialProvider = credentialProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public bool HasWebhookSecret { get; private set; }
    public string WebhookUrl { get; private set; } = string.Empty;
    public List<WebhookEventDisplay> Events { get; private set; } = new();

    public void OnGet()
    {
        LoadData();
    }

    public IActionResult OnPostClear()
    {
        // TODO: Clear webhook events from store
        LoadData();
        return Page();
    }

    private void LoadData()
    {
        var credentials = _credentialProvider.GetCredentials();
        HasWebhookSecret = credentials?.HasWebhookSecret ?? false;

        // Build webhook URL
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request != null)
        {
            WebhookUrl = $"{request.Scheme}://{request.Host}/api/webhooks/stripe";
        }

        // TODO: Load events from webhook event store
        // For now, return empty list
        Events = new List<WebhookEventDisplay>();
    }

    public record WebhookEventDisplay(
        DateTime Timestamp,
        string EventType,
        string? OrderId,
        bool Success,
        bool IsDuplicate);
}
