using Microsoft.AspNetCore.Mvc.RazorPages;
using PaymentDemo.Services.Credentials;

namespace PaymentDemo.Pages;

public class IndexModel : PageModel
{
    private readonly IStripeCredentialProvider _credentialProvider;

    public IndexModel(IStripeCredentialProvider credentialProvider)
    {
        _credentialProvider = credentialProvider;
    }

    public bool HasCredentials { get; private set; }
    public CredentialSource CredentialSource { get; private set; }

    public void OnGet()
    {
        HasCredentials = _credentialProvider.HasValidCredentials();
        CredentialSource = _credentialProvider.GetCurrentSource();
    }
}
