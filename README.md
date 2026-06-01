# Stripe Payment Demo

A production-grade Stripe Checkout demo built with .NET 8, Razor Pages, and Tailwind CSS. This application demonstrates a complete e-commerce payment flow with webhook handling, security headers, and an admin panel.

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)
![Stripe](https://img.shields.io/badge/Stripe-Checkout-635BFF)
![Tailwind CSS](https://img.shields.io/badge/Tailwind-CSS-38B2AC)

## Features

- **E-commerce Flow**: Product catalog, shopping cart, checkout
- **Stripe Checkout**: Secure hosted payment page
- **Webhook Handling**: Signature verification, idempotency, event logging
- **Admin Panel**: Credential management, webhook logs, security dashboard
- **Layered Credentials**: Session overrides > Environment variables > appsettings
- **Security Headers**: CSP, X-Frame-Options, and more

## Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (for Tailwind CSS)
- [Stripe CLI](https://stripe.com/docs/stripe-cli) (for webhook testing)

### Installation

```bash
# Clone the repository
git clone https://github.com/GeorgeVragalev/Stripe-Payment-Demo.git
cd Stripe-Payment-Demo

# Install .NET dependencies
dotnet restore

# Install Node dependencies
npm install

# Build Tailwind CSS
npm run build:css

# Run the application
dotnet run
```

The app will be available at `http://localhost:5000`.

### Configure Stripe Credentials

You have three options to configure Stripe credentials:

#### Option 1: Admin Panel (Recommended for Testing)
1. Navigate to `/admin/credentials`
2. Enter your Stripe test keys
3. Keys are stored in the session (not persisted)

#### Option 2: Environment Variables
```bash
export Stripe__SecretKey="sk_test_xxx"
export Stripe__PublicKey="pk_test_xxx"
export Stripe__WebhookSecret="whsec_xxx"
```

#### Option 3: User Secrets (Development)
```bash
dotnet user-secrets set "Stripe:SecretKey" "sk_test_xxx"
dotnet user-secrets set "Stripe:PublicKey" "pk_test_xxx"
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_xxx"
```

### Testing Webhooks Locally

1. Install and login to [Stripe CLI](https://stripe.com/docs/stripe-cli):
   ```bash
   stripe login
   ```

2. Forward webhooks to your local server:
   ```bash
   stripe listen --forward-to http://localhost:5000/api/webhooks/stripe
   ```

3. Copy the webhook signing secret (`whsec_xxx`) displayed by the CLI

4. Add the secret to your credentials (via admin panel or environment variable)

5. Make a test purchase and watch the webhook events!

## Test Cards

| Card Number | Scenario |
|-------------|----------|
| `4242 4242 4242 4242` | Successful payment |
| `4000 0000 0000 3220` | 3D Secure required |
| `4000 0000 0000 9995` | Declined (insufficient funds) |
| `4000 0000 0000 0069` | Expired card |

Use any future expiry date and any 3-digit CVC.

## Project Structure

```
PaymentDemo/
├── Controllers/
│   └── StripeWebhookController.cs    # Webhook endpoint
├── Models/
│   ├── Order.cs                      # Order model
│   ├── Product.cs                    # Product catalog
│   └── WebhookEvent.cs               # Webhook event model
├── Pages/
│   ├── Admin/                        # Admin panel pages
│   │   ├── Credentials.cshtml        # Stripe key configuration
│   │   ├── Docs.cshtml               # How It Works guide
│   │   ├── Security.cshtml           # Security headers info
│   │   └── Webhooks.cshtml           # Webhook event logs
│   ├── Cart/                         # Shopping cart
│   ├── Checkout/                     # Checkout flow
│   ├── Orders/                       # Order history
│   └── Shop/                         # Product catalog
├── Services/
│   ├── Cart/                         # Cart management
│   ├── Credentials/                  # Credential provider
│   ├── Orders/                       # Order store
│   ├── Stripe/                       # Stripe API wrapper
│   └── Webhooks/                     # Webhook event store
├── Middleware/
│   └── SecurityHeadersMiddleware.cs  # Security headers
└── wwwroot/css/                      # Tailwind CSS
```

## Security

This demo includes production-ready security headers:

| Header | Value |
|--------|-------|
| Content-Security-Policy | Restricts scripts, frames, connections |
| X-Frame-Options | DENY |
| X-Content-Type-Options | nosniff |
| Referrer-Policy | strict-origin-when-cross-origin |
| Permissions-Policy | Disables camera, microphone, geolocation |

## Architecture

### Credential Resolution Order

```
1. Session Credentials (admin panel) → Highest priority
         ↓ fallback
2. Environment Variables → Production config
         ↓ fallback
3. appsettings.json → Development defaults
         ↓ fallback
4. None → Redirect to /admin/credentials
```

### Webhook Flow

```
Stripe → POST /api/webhooks/stripe
           ↓
     Verify Signature (using webhook secret)
           ↓
     Check Idempotency (skip if already processed)
           ↓
     Handle Event (checkout.session.completed, etc.)
           ↓
     Update Order Status
           ↓
     Log to Webhook Event Store
```

## Production Considerations

This is a demo application. For production deployment:

1. **Replace in-memory stores** with Redis/PostgreSQL
2. **Add rate limiting** on webhook and admin endpoints
3. **Enable HSTS** with proper max-age
4. **Implement authentication** for admin panel
5. **Add application monitoring** (Application Insights, etc.)
6. **Configure proper logging** (exclude sensitive data)
7. **Set up health check endpoints**

## License

MIT

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
