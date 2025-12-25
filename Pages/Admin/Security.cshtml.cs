using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PaymentDemo.Pages.Admin;

public class SecurityModel : PageModel
{
    public List<SecurityHeader> SecurityHeaders { get; } = new()
    {
        new("Content-Security-Policy", "See details below"),
        new("X-Frame-Options", "DENY"),
        new("X-Content-Type-Options", "nosniff"),
        new("Referrer-Policy", "strict-origin-when-cross-origin"),
        new("X-XSS-Protection", "1; mode=block"),
        new("Permissions-Policy", "camera=(), microphone=(), geolocation=()")
    };

    public string CspPolicy { get; } = @"default-src 'self';
script-src 'self' https://js.stripe.com;
frame-src https://js.stripe.com https://hooks.stripe.com;
connect-src 'self' https://api.stripe.com;
style-src 'self' 'unsafe-inline';
img-src 'self' data: https://*.stripe.com;
font-src 'self';
form-action 'self' https://checkout.stripe.com;
frame-ancestors 'none';
base-uri 'self';
upgrade-insecure-requests";

    public void OnGet()
    {
    }

    public record SecurityHeader(string Name, string Value);
}
