using Microsoft.AspNetCore.Mvc.RazorPages;
using PaymentDemo.Services.Credentials;

namespace PaymentDemo.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly IStripeCredentialProvider _credentialProvider;

    public IndexModel(IStripeCredentialProvider credentialProvider)
    {
        _credentialProvider = credentialProvider;
    }

    public bool HasCredentials { get; private set; }
    public CredentialSource CredentialSource { get; private set; }
    public bool IsTestMode { get; private set; }
    public string MaskedPublicKey { get; private set; } = string.Empty;
    public bool HasWebhookSecret { get; private set; }
    public int WebhookCount { get; private set; }
    public int FailedWebhookCount { get; private set; }

    public void OnGet()
    {
        var credentials = _credentialProvider.GetCredentials();

        HasCredentials = _credentialProvider.HasValidCredentials();
        CredentialSource = _credentialProvider.GetCurrentSource();
        IsTestMode = _credentialProvider.IsTestMode();
        MaskedPublicKey = credentials?.MaskedPublicKey ?? "(not set)";
        HasWebhookSecret = credentials?.HasWebhookSecret ?? false;

        // TODO: Get actual webhook counts from store
        WebhookCount = 0;
        FailedWebhookCount = 0;
    }
}
