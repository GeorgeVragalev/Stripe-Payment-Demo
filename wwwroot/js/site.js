/**
 * Stripe Payment Demo - Site JavaScript
 * General site functionality
 */

(function() {
    'use strict';

    // Currency Selector
    function initCurrencySelector() {
        const currencySelector = document.getElementById('currency-selector');
        if (!currencySelector) return;

        currencySelector.addEventListener('change', async function() {
            const currency = this.value;
            try {
                const response = await fetch('/api/user/currency', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ currency: currency })
                });

                if (response.ok) {
                    // Reload the page to reflect the new currency
                    window.location.reload();
                } else {
                    console.error('Failed to update currency');
                }
            } catch (error) {
                console.error('Error updating currency:', error);
            }
        });
    }

    // Payment Mode Toggle (Two-Step vs Automatic Capture)
    function initPaymentModeToggle() {
        const toggle = document.getElementById('payment-mode-toggle');
        if (!toggle) return;

        toggle.addEventListener('change', async function() {
            const isTwoStep = this.checked;
            try {
                const response = await fetch('/api/user/payment-mode', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ separateAuthorizeAndCapture: isTwoStep })
                });

                if (response.ok) {
                    // Update the tooltip
                    const container = toggle.closest('[title]');
                    if (container) {
                        container.title = isTwoStep
                            ? 'Two-step: Authorize first, then capture on confirmation'
                            : 'One-step: Automatic capture during checkout';
                    }
                } else {
                    console.error('Failed to update payment mode');
                    // Revert the toggle
                    toggle.checked = !isTwoStep;
                }
            } catch (error) {
                console.error('Error updating payment mode:', error);
                // Revert the toggle
                toggle.checked = !isTwoStep;
            }
        });
    }

    // Initialize all components
    function init() {
        initCurrencySelector();
        initPaymentModeToggle();
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
