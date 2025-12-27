using Stripe.Checkout;

namespace PaymentDemo.Extensions;

/// <summary>
/// Extension methods for Stripe Checkout Session
/// </summary>
public static class SessionExtensions
{
    /// <summary>
    /// Check if the session payment has been completed (paid)
    /// </summary>
    public static bool IsPaid(this Session session)
    {
        return session?.PaymentStatus?.Equals("paid", StringComparison.InvariantCultureIgnoreCase) == true;
    }

    /// <summary>
    /// Check if the session payment is still unpaid (for authorize-only or before payment)
    /// </summary>
    public static bool IsUnpaid(this Session session)
    {
        return session?.PaymentStatus?.Equals("unpaid", StringComparison.InvariantCultureIgnoreCase) == true;
    }

    /// <summary>
    /// Check if the session payment is processing (for async payment methods like SEPA/BACS)
    /// </summary>
    public static bool IsProcessing(this Session session)
    {
        return session?.PaymentStatus?.Equals("processing", StringComparison.InvariantCultureIgnoreCase) == true;
    }

    /// <summary>
    /// Check if the checkout session is complete (customer finished checkout)
    /// Note: Session can be complete but payment still processing for async methods
    /// </summary>
    public static bool IsComplete(this Session session)
    {
        return session?.Status?.Equals("complete", StringComparison.InvariantCultureIgnoreCase) == true;
    }

    /// <summary>
    /// Check if the session has expired
    /// </summary>
    public static bool IsExpired(this Session session)
    {
        return session?.Status?.Equals("expired", StringComparison.InvariantCultureIgnoreCase) == true;
    }

    /// <summary>
    /// Check if this is an async payment method (SEPA, BACS, etc.)
    /// These require webhooks to confirm payment
    /// </summary>
    public static bool IsAsyncPaymentMethod(this Session session)
    {
        var paymentMethodTypes = session?.PaymentMethodTypes;
        if (paymentMethodTypes == null) return false;

        // List of async payment methods that require webhook confirmation
        var asyncMethods = new[] { "sepa_debit", "bacs_debit", "acss_debit", "us_bank_account", "sofort", "bancontact", "ideal" };

        return paymentMethodTypes.Any(pm => asyncMethods.Contains(pm, StringComparer.OrdinalIgnoreCase));
    }
}
