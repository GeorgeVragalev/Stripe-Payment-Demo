using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PaymentDemo.Services.Credentials;
using Stripe;

namespace PaymentDemo.Pages.Admin;

public class CredentialsModel : PageModel
{
    private readonly IStripeCredentialProvider _credentialProvider;
    private readonly ISessionCredentialStore _sessionStore;
    private readonly ILogger<CredentialsModel> _logger;

    public CredentialsModel(
        IStripeCredentialProvider credentialProvider,
        ISessionCredentialStore sessionStore,
        ILogger<CredentialsModel> logger)
    {
        _credentialProvider = credentialProvider;
        _sessionStore = sessionStore;
        _logger = logger;
    }

    // Current credential info
    public bool HasCredentials { get; private set; }
    public CredentialSource CredentialSource { get; private set; }
    public bool IsTestMode { get; private set; }
    public string MaskedPublicKey { get; private set; } = "(not set)";
    public string MaskedSecretKey { get; private set; } = "(not set)";
    public string MaskedWebhookSecret { get; private set; } = "(not set)";
    public DateTime? ConfiguredAt { get; private set; }

    // Messages
    public string? SuccessMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    // Input model for form
    [BindProperty]
    public CredentialInputModel Input { get; set; } = new();

    public class CredentialInputModel
    {
        [Required(ErrorMessage = "Secret key is required")]
        [RegularExpression(@"^sk_test_[a-zA-Z0-9]+$", ErrorMessage = "Must be a valid test secret key (sk_test_...)")]
        public string SecretKey { get; set; } = string.Empty;

        [Required(ErrorMessage = "Publishable key is required")]
        [RegularExpression(@"^pk_test_[a-zA-Z0-9]+$", ErrorMessage = "Must be a valid test publishable key (pk_test_...)")]
        public string PublicKey { get; set; } = string.Empty;

        [RegularExpression(@"^whsec_[a-zA-Z0-9]+$", ErrorMessage = "Must be a valid webhook secret (whsec_...)")]
        public string? WebhookSecret { get; set; }
    }

    public void OnGet()
    {
        LoadCurrentCredentials();
    }

    public async Task<IActionResult> OnPostSetCredentialsAsync()
    {
        if (!ModelState.IsValid)
        {
            LoadCurrentCredentials();
            return Page();
        }

        // Block live keys
        if (Input.SecretKey.StartsWith("sk_live_") || Input.PublicKey.StartsWith("pk_live_"))
        {
            ErrorMessage = "Live keys are not allowed in this demo. Please use test keys only.";
            LoadCurrentCredentials();
            return Page();
        }

        // Validate the credentials by making a test API call
        try
        {
            var client = new StripeClient(Input.SecretKey);
            var service = new BalanceService(client);

            // This will throw if the key is invalid
            await service.GetAsync();

            // Keys are valid, store them
            var credentials = new StripeCredentials
            {
                SecretKey = Input.SecretKey,
                PublicKey = Input.PublicKey,
                WebhookSecret = Input.WebhookSecret ?? string.Empty
            };

            _sessionStore.SetCredentials(credentials);

            _logger.LogInformation("User configured session credentials (test mode)");

            SuccessMessage = "Credentials validated and saved to your session!";
            Input = new CredentialInputModel(); // Clear the form
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Invalid Stripe credentials provided");
            ErrorMessage = $"Invalid credentials: {ex.Message}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Stripe credentials");
            ErrorMessage = "An error occurred while validating credentials. Please try again.";
        }

        LoadCurrentCredentials();
        return Page();
    }

    public IActionResult OnPostReset()
    {
        _sessionStore.ClearCredentials();
        _logger.LogInformation("User cleared session credentials");

        SuccessMessage = "Credentials reset to demo/environment configuration.";
        LoadCurrentCredentials();
        return Page();
    }

    private void LoadCurrentCredentials()
    {
        var credentials = _credentialProvider.GetCredentials();

        HasCredentials = _credentialProvider.HasValidCredentials();
        CredentialSource = _credentialProvider.GetCurrentSource();
        IsTestMode = _credentialProvider.IsTestMode();

        if (credentials != null)
        {
            MaskedPublicKey = credentials.MaskedPublicKey;
            MaskedSecretKey = credentials.MaskedSecretKey;
            MaskedWebhookSecret = credentials.MaskedWebhookSecret;
            ConfiguredAt = credentials.ConfiguredAt;
        }
    }
}
