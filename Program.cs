using Microsoft.AspNetCore.DataProtection;
using PaymentDemo.Configuration;
using PaymentDemo.Middleware;
using PaymentDemo.Services.Cart;
using PaymentDemo.Services.Credentials;
using PaymentDemo.Services.Orders;
using PaymentDemo.Services.Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// Configure session with encryption
builder.Services.AddDataProtection()
    .SetApplicationName("PaymentDemo")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys")));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// Configure Stripe settings from configuration
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

// Register credential services
builder.Services.AddScoped<ISessionCredentialStore, SessionCredentialStore>();
builder.Services.AddScoped<IStripeCredentialProvider, StripeCredentialProvider>();

// Register cart service
builder.Services.AddScoped<ICartService, CartService>();

// Register order store (singleton for in-memory demo)
builder.Services.AddSingleton<IOrderStore, InMemoryOrderStore>();

// Register Stripe service
builder.Services.AddScoped<IStripeService, StripeService>();

// Configure antiforgery
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

var app = builder.Build();

// Configure middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
else
{
    // Only redirect to HTTPS in development if HTTPS is configured
    if (builder.Configuration["ASPNETCORE_HTTPS_PORT"] != null ||
        builder.Configuration["Kestrel:Endpoints:Https:Url"] != null)
    {
        app.UseHttpsRedirection();
    }
}
app.UseStaticFiles();

// Security headers middleware (before routing)
app.UseSecurityHeaders();

app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
