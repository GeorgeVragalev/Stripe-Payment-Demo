using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PaymentDemo.Models;
using PaymentDemo.Services.Credentials;
using PaymentDemo.Services.Webhooks;

namespace PaymentDemo.Pages.Admin;

public class WebhooksModel : PageModel
{
    private readonly IStripeCredentialProvider _credentialProvider;
    private readonly IWebhookEventStore _webhookEventStore;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WebhooksModel(
        IStripeCredentialProvider credentialProvider,
        IWebhookEventStore webhookEventStore,
        IHttpContextAccessor httpContextAccessor)
    {
        _credentialProvider = credentialProvider;
        _webhookEventStore = webhookEventStore;
        _httpContextAccessor = httpContextAccessor;
    }

    public bool HasWebhookSecret { get; private set; }
    public string WebhookUrl { get; private set; } = string.Empty;
    public List<WebhookEvent> Events { get; private set; } = new();
    public WebhookStats Stats { get; private set; } = new();

    public void OnGet()
    {
        LoadData();
    }

    public IActionResult OnPostClear()
    {
        // Note: In-memory store doesn't support clearing in this demo
        // In production, you'd implement a Clear() method
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

        // Load events from webhook event store
        Events = _webhookEventStore.GetRecent(50).ToList();
        Stats = _webhookEventStore.GetStats();
    }
}
