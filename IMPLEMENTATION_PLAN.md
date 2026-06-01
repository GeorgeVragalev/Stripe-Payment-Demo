# Stripe Payment Demo - Implementation Plan

A production-grade Stripe Checkout demo built with .NET 8, Razor Pages, and Tailwind CSS.

## Overview

This application demonstrates a complete Stripe Checkout payment flow with:
- Clean e-commerce UI for the shopping experience
- Separate admin panel for configuration and monitoring
- Layered credential system (environment defaults + session overrides)
- Secure webhook handling with idempotency
- Comprehensive security headers including CSP

---

## Architecture

### Credential Resolution (Priority Order)

```
1. Session Credentials (user override) → Takes precedence if set
         ↓ fallback
2. Environment Variables → Production/deployment config
         ↓ fallback
3. appsettings.json → Development defaults
         ↓ fallback
4. None → Redirect to /admin/credentials
```

### Site Structure

```
┌─────────────────────────────────────┐   ┌─────────────────────────────────┐
│      🛒 MAIN SITE                   │   │      ⚙️ ADMIN PANEL             │
│      (E-commerce Flow)              │   │      (Configuration)            │
├─────────────────────────────────────┤   ├─────────────────────────────────┤
│  /                → Home            │   │  /admin            → Dashboard  │
│  /shop            → Products        │   │  /admin/credentials→ Stripe Keys│
│  /cart            → Cart            │   │  /admin/webhooks   → Logs       │
│  /checkout        → Checkout        │   │  /admin/security   → Headers    │
│  /checkout/success→ Thank You       │   │  /admin/docs       → How It Works│
│  /orders          → My Orders       │   │                                 │
│                                     │   │                                 │
│  Clean, simple e-commerce UI        │   │  Technical dashboard UI         │
└─────────────────────────────────────┘   └─────────────────────────────────┘
```

---

## Phase 1: Foundation + Credential System

### Tasks
- [x] Create .NET 8 Razor Pages project
- [x] Configure security headers middleware (CSP, X-Frame-Options, etc.)
- [x] Set up Data Protection API for session encryption
- [x] Implement layered credential provider
- [x] Create session-based credential store
- [x] Build admin panel for credential management
- [x] Set up Tailwind CSS build pipeline
- [x] Create base layouts (main site + admin)

### Files to Create
```
PaymentDemo/
├── PaymentDemo.csproj
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── Configuration/
│   └── StripeSettings.cs
├── Middleware/
│   └── SecurityHeadersMiddleware.cs
├── Services/
│   └── Credentials/
│       ├── StripeCredentials.cs
│       ├── IStripeCredentialProvider.cs
│       ├── StripeCredentialProvider.cs
│       ├── ISessionCredentialStore.cs
│       └── SessionCredentialStore.cs
├── Pages/
│   ├── _ViewStart.cshtml
│   ├── _ViewImports.cshtml
│   ├── Index.cshtml
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   └── _AdminLayout.cshtml
│   └── Admin/
│       ├── Index.cshtml
│       ├── Credentials.cshtml
│       └── Security.cshtml
├── wwwroot/
│   └── css/
│       └── site.css
├── package.json
├── tailwind.config.js
└── postcss.config.js
```

### Acceptance Criteria
- [x] Project builds and runs
- [x] Security headers present on all responses
- [x] CSP properly configured for Stripe domains
- [x] Credentials can be loaded from appsettings
- [x] Credentials can be overridden via session
- [x] Admin panel shows current credential source
- [x] Tailwind CSS compiles correctly

---

## Phase 2: Checkout Flow + E-commerce UI

### Tasks
- [x] Create Product model and service
- [x] Implement in-memory order store
- [x] Build shopping cart (session-based)
- [x] Create Stripe service wrapper
- [x] Implement checkout session creation
- [x] Handle success/cancel callbacks
- [x] Build all e-commerce pages with Tailwind

### Pages
- `/shop` - Product listing with add to cart
- `/cart` - Shopping cart with quantities
- `/checkout` - Creates Stripe session, redirects
- `/checkout/success` - Order confirmation
- `/checkout/cancel` - Cancelled payment
- `/orders` - Order history

### Acceptance Criteria
- [x] Products display correctly
- [x] Cart persists in session
- [x] Checkout redirects to Stripe
- [x] Success page shows order details
- [x] Orders list shows all orders

---

## Phase 3: Webhooks + Logging + Polish

### Tasks
- [x] Create webhook endpoint
- [x] Implement signature verification
- [x] Handle checkout.session.completed event
- [x] Implement idempotency (track event IDs)
- [x] Create webhook event store
- [x] Build webhook logs admin page
- [x] Create "How It Works" documentation page
- [x] Write README with setup instructions

### Endpoints
- `POST /api/webhooks/stripe` - Webhook receiver

### Acceptance Criteria
- [x] Webhooks verify signature correctly
- [x] Invalid signatures return 400
- [x] Duplicate events are skipped
- [x] Order status updates from webhook
- [x] Webhook logs show all events
- [x] Documentation is complete

---

## Security Configuration

### Headers Applied
| Header | Value |
|--------|-------|
| Content-Security-Policy | See below |
| X-Frame-Options | DENY |
| X-Content-Type-Options | nosniff |
| Referrer-Policy | strict-origin-when-cross-origin |
| X-XSS-Protection | 1; mode=block |
| Permissions-Policy | camera=(), microphone=(), geolocation=() |

### Content Security Policy
```
default-src 'self';
script-src 'self' https://js.stripe.com;
frame-src https://js.stripe.com https://hooks.stripe.com;
connect-src 'self' https://api.stripe.com;
style-src 'self' 'unsafe-inline';
img-src 'self' data: https://*.stripe.com;
font-src 'self';
form-action 'self' https://checkout.stripe.com;
frame-ancestors 'none';
base-uri 'self';
```

---

## Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `Stripe__SecretKey` | Stripe secret key | `sk_test_xxx` |
| `Stripe__PublicKey` | Stripe publishable key | `pk_test_xxx` |
| `Stripe__WebhookSecret` | Webhook signing secret | `whsec_xxx` |

---

## Local Development

```bash
# Install dependencies
dotnet restore
npm install

# Set up user secrets (optional - can use admin panel instead)
dotnet user-secrets set "Stripe:SecretKey" "sk_test_xxx"
dotnet user-secrets set "Stripe:PublicKey" "pk_test_xxx"
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_xxx"

# Build Tailwind CSS (watch mode)
npm run watch

# Run the app
dotnet run

# In another terminal, start Stripe CLI for webhooks
stripe listen --forward-to https://localhost:5001/api/webhooks/stripe
```

---

## Test Cards

| Card Number | Scenario |
|-------------|----------|
| 4242 4242 4242 4242 | Successful payment |
| 4000 0000 0000 3220 | 3D Secure authentication |
| 4000 0000 0000 9995 | Declined (insufficient funds) |

Use any future expiry date and any 3-digit CVC.

---

## Phase 4: Interactive Tutorial

### Tasks
- [x] Create tutorial JavaScript system
- [x] Add tutorial steps for home page
- [x] Add tutorial steps for shopping flow
- [x] Add tutorial steps for admin panel
- [x] Add tutorial UI overlay with progress tracking
- [x] Add welcome banner for first-time visitors
- [x] Add tutorial button in navigation

### Features
- Step-by-step guided tour
- Highlight elements as you progress
- Cross-page navigation support
- Session-based progress tracking
- Skip/resume functionality

### Acceptance Criteria
- [x] Tutorial can be started from navigation
- [x] Tutorial guides through shopping flow
- [x] Tutorial guides through admin panel
- [x] Progress persists across page navigation
- [x] Tutorial can be skipped or resumed

---

## Production Hardening

For production deployment:

1. **Replace in-memory stores** with Redis/PostgreSQL
2. **Add rate limiting** on webhook and admin endpoints
3. **Enable HSTS** with proper max-age
4. **Configure proper session timeout**
5. **Add application monitoring** (Application Insights, etc.)
6. **Implement proper authentication** for admin panel
7. **Set up health check endpoints**
8. **Configure proper logging** (exclude sensitive data)
