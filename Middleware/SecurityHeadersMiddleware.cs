namespace PaymentDemo.Middleware;

/// <summary>
/// Middleware that adds comprehensive security headers to all responses.
/// Includes CSP configured specifically for Stripe integration.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    // CSP directives - configured for Stripe Checkout
    private static readonly string ContentSecurityPolicy = string.Join("; ", new[]
    {
        "default-src 'self'",
        "script-src 'self' https://js.stripe.com",
        "frame-src https://js.stripe.com https://hooks.stripe.com",
        "connect-src 'self' https://api.stripe.com",
        "style-src 'self' 'unsafe-inline'",  // unsafe-inline needed for Tailwind
        "img-src 'self' data: https://*.stripe.com",
        "font-src 'self'",
        "form-action 'self' https://checkout.stripe.com",
        "frame-ancestors 'none'",
        "base-uri 'self'",
        "upgrade-insecure-requests"
    });

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers before processing request
        var headers = context.Response.Headers;

        // Content Security Policy - prevents XSS and injection attacks
        headers["Content-Security-Policy"] = ContentSecurityPolicy;

        // Prevent clickjacking
        headers["X-Frame-Options"] = "DENY";

        // Prevent MIME type sniffing
        headers["X-Content-Type-Options"] = "nosniff";

        // Control referrer information
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // XSS Protection (legacy browsers)
        headers["X-XSS-Protection"] = "1; mode=block";

        // Permissions Policy - disable unnecessary features
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=(self \"https://js.stripe.com\")";

        // Prevent caching of sensitive pages
        if (context.Request.Path.StartsWithSegments("/admin"))
        {
            headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            headers["Pragma"] = "no-cache";
        }

        await _next(context);
    }
}

/// <summary>
/// Extension method to register the security headers middleware
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
