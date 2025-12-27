/**
 * Stripe Payment Demo - Interactive Tutorial System
 * A guided walkthrough that helps users understand the payment flow
 */

class DemoTutorial {
    constructor() {
        this.currentStep = 0;
        this.isActive = false;
        this.overlay = null;
        this.tooltip = null;
        this.steps = [];
        this.onComplete = null;

        this.init();
    }

    init() {
        // Create overlay elements
        this.createOverlay();
        this.createTooltip();

        // Load saved progress
        const savedStep = sessionStorage.getItem('tutorial_step');
        const tutorialActive = sessionStorage.getItem('tutorial_active');
        const wasAtStripe = sessionStorage.getItem('tutorial_at_stripe');

        if (tutorialActive === 'true' && savedStep) {
            this.currentStep = parseInt(savedStep, 10);
            this.isActive = true;
        }

        // Define tutorial steps based on current page
        this.defineSteps();

        // Check if returning from Stripe checkout - auto-resume on success/cancel
        const path = window.location.pathname.toLowerCase();
        if (wasAtStripe && (path.includes('/checkout/success') || path.includes('/checkout/cancel'))) {
            // Returning from Stripe - restart tutorial from beginning of this page
            this.isActive = true;
            this.currentStep = 0;
            sessionStorage.setItem('tutorial_active', 'true');
            sessionStorage.setItem('tutorial_step', '0');
        }

        // Auto-resume if tutorial was active (delay to ensure DOM is fully ready)
        if (this.isActive && this.steps.length > 0) {
            // Use requestAnimationFrame for better timing
            requestAnimationFrame(() => {
                setTimeout(() => this.showStep(this.currentStep), 300);
            });
        }
    }

    createOverlay() {
        this.overlay = document.createElement('div');
        this.overlay.id = 'tutorial-overlay';
        this.overlay.innerHTML = `
            <div class="tutorial-spotlight"></div>
        `;
        this.overlay.style.cssText = `
            position: fixed;
            inset: 0;
            background: rgba(0, 0, 0, 0.5);
            z-index: 9998;
            display: none;
            transition: opacity 0.3s ease;
        `;
        document.body.appendChild(this.overlay);

        // Click overlay to close (optional)
        this.overlay.addEventListener('click', (e) => {
            if (e.target === this.overlay) {
                // Don't close on overlay click - require button
            }
        });
    }

    createTooltip() {
        this.tooltip = document.createElement('div');
        this.tooltip.id = 'tutorial-tooltip';
        this.tooltip.style.cssText = `
            position: fixed;
            z-index: 9999;
            background: white;
            border-radius: 12px;
            box-shadow: 0 20px 50px rgba(0, 0, 0, 0.3);
            max-width: 380px;
            display: none;
            animation: tooltipFadeIn 0.3s ease;
        `;
        document.body.appendChild(this.tooltip);

        // Add animation styles
        const style = document.createElement('style');
        style.textContent = `
            @keyframes tooltipFadeIn {
                from { opacity: 0; transform: translateY(10px); }
                to { opacity: 1; transform: translateY(0); }
            }
            @keyframes pulse {
                0%, 100% { box-shadow: 0 0 0 0 rgba(99, 102, 241, 0.7); }
                50% { box-shadow: 0 0 0 10px rgba(99, 102, 241, 0); }
            }
            .tutorial-highlight {
                position: relative;
                z-index: 9999 !important;
                animation: pulse 2s infinite;
                border-radius: 8px;
            }
            .tutorial-arrow {
                position: absolute;
                width: 12px;
                height: 12px;
                background: white;
                transform: rotate(45deg);
            }
            .tutorial-progress-dot {
                width: 8px;
                height: 8px;
                border-radius: 50%;
                background: #e5e7eb;
                transition: background 0.2s;
            }
            .tutorial-progress-dot.active {
                background: #6366f1;
            }
            .tutorial-progress-dot.completed {
                background: #22c55e;
            }
        `;
        document.head.appendChild(style);
    }

    defineSteps() {
        const path = window.location.pathname.toLowerCase();

        if (path === '/' || path === '/index') {
            this.steps = this.getHomeSteps();
        } else if (path === '/shop' || path === '/shop/index') {
            this.steps = this.getShopSteps();
        } else if (path === '/cart' || path === '/cart/index') {
            this.steps = this.getCartSteps();
        } else if (path === '/orders' || path === '/orders/index') {
            this.steps = this.getOrdersSteps();
        } else if (path.startsWith('/admin')) {
            this.steps = this.getAdminSteps();
        } else if (path.startsWith('/checkout/success')) {
            this.steps = this.getSuccessSteps();
        } else if (path.startsWith('/checkout/cancel')) {
            this.steps = this.getCancelSteps();
        }
    }

    getHomeSteps() {
        return [
            {
                title: "Welcome to the Stripe Demo! 👋",
                content: "This interactive demo shows you how Stripe Checkout works. Let me guide you through the payment flow.",
                target: null, // No specific target - centered
                position: 'center'
            },
            {
                title: "Start Shopping",
                content: "Click here to browse our demo products and start the checkout experience.",
                target: 'a[href="/Shop"]',
                position: 'bottom',
                nextPage: '/Shop'
            }
        ];
    }

    getShopSteps() {
        return [
            {
                title: "Product Catalog 🛍️",
                content: "These are our demo products. Each represents a subscription plan with different features.",
                target: '.grid',
                position: 'top'
            },
            {
                title: "Popular Choice",
                content: "The Pro Plan is marked as 'Popular' - let's add it to your cart automatically!",
                target: '.border-brand-500',
                position: 'left'
            },
            {
                title: "Adding to Cart...",
                content: "I'll add the Pro Plan to your cart now. Watch the cart icon update!",
                target: '.border-brand-500 button[type="submit"]',
                position: 'top',
                autoAction: () => {
                    // Find the Pro Plan form and submit it
                    const proForm = document.querySelector('.border-brand-500 form');
                    if (proForm) {
                        proForm.submit();
                        return true; // Action will navigate away
                    }
                    return false;
                },
                autoActionDelay: 1500
            }
        ];
    }

    getCartSteps() {
        return [
            {
                title: "Your Shopping Cart 🛒",
                content: "The Pro Plan has been added! You can see the item, quantity, and pricing here.",
                target: '.card',
                position: 'top'
            },
            {
                title: "Order Summary",
                content: "The summary shows subtotal, tax (8%), and total. This exact amount will be charged via Stripe.",
                target: '.bg-gray-50',
                position: 'left'
            },
            {
                title: "Ready for Checkout! 🚀",
                content: "Click 'Next' and I'll redirect you to Stripe's secure payment page. Use test card <strong>4242 4242 4242 4242</strong> with any future date and CVC.",
                target: 'form[action*="Checkout"] button[type="submit"], a.btn-primary',
                position: 'top',
                autoAction: () => {
                    // Submit the checkout form
                    const checkoutForm = document.querySelector('form[action*="Checkout"]');
                    if (checkoutForm) {
                        // Mark that we're going to Stripe
                        sessionStorage.setItem('tutorial_at_stripe', 'true');
                        checkoutForm.submit();
                        return true;
                    }
                    return false;
                },
                autoActionDelay: 2000,
                isCheckout: true
            }
        ];
    }

    getOrdersSteps() {
        return [
            {
                title: "Order History 📋",
                content: "All orders are tracked here. Notice the different statuses: Paid, Pending, Failed.",
                target: '.divide-y',
                position: 'top'
            },
            {
                title: "Order Details",
                content: "Each order shows the items purchased, total amount, and payment status.",
                target: '.card:first-child',
                position: 'right'
            },
            {
                title: "Explore the Admin Panel",
                content: "Want to see how webhooks work? Check out the Admin panel!",
                target: 'a[href="/Admin"]',
                position: 'bottom',
                nextPage: '/Admin'
            }
        ];
    }

    getAdminSteps() {
        const path = window.location.pathname.toLowerCase();

        if (path === '/admin' || path === '/admin/index') {
            return [
                {
                    title: "Admin Dashboard ⚙️",
                    content: "This is the admin panel where you can configure Stripe credentials and monitor webhooks.",
                    target: null,
                    position: 'center'
                },
                {
                    title: "Credential Status",
                    content: "This shows whether your Stripe API keys are configured. Green means ready!",
                    target: '.status-dot',
                    position: 'right'
                },
                {
                    title: "Check Webhooks",
                    content: "Let's see how Stripe webhooks are logged.",
                    target: 'a[href="/Admin/Webhooks"]',
                    position: 'right',
                    nextPage: '/Admin/Webhooks'
                }
            ];
        } else if (path === '/admin/webhooks') {
            return [
                {
                    title: "Webhook Events 📡",
                    content: "Every time Stripe sends an event (like payment completed), it's logged here.",
                    target: null,
                    position: 'center'
                },
                {
                    title: "Event Statistics",
                    content: "See how many events were processed, failed, or skipped at a glance.",
                    target: '.grid-cols-2',
                    position: 'bottom'
                },
                {
                    title: "Event Details",
                    content: "Each event shows the type (checkout.session.completed), related IDs, and processing status.",
                    target: 'table',
                    position: 'top'
                },
                {
                    title: "Learn More",
                    content: "Check out the documentation to understand how the payment flow works!",
                    target: 'a[href="/Admin/Docs"]',
                    position: 'right',
                    nextPage: '/Admin/Docs'
                }
            ];
        } else if (path === '/admin/docs') {
            return [
                {
                    title: "How It Works 📚",
                    content: "This page explains the complete payment flow from cart to webhook.",
                    target: null,
                    position: 'center'
                },
                {
                    title: "Payment Flow",
                    content: "Follow these steps to understand how Stripe Checkout integrates with your app.",
                    target: '.space-y-6',
                    position: 'right'
                },
                {
                    title: "Test Cards",
                    content: "Use these test card numbers when making payments. 4242... always succeeds!",
                    target: '.grid.sm\\:grid-cols-2',
                    position: 'top'
                },
                {
                    title: "Tutorial Complete! 🎉",
                    content: "You've learned the basics! Try making a test purchase to see it all in action.",
                    target: null,
                    position: 'center',
                    isLast: true
                }
            ];
        }

        return [];
    }

    getSuccessSteps() {
        // Clear the stripe flag
        sessionStorage.removeItem('tutorial_at_stripe');

        return [
            {
                title: "Payment Successful! 🎉",
                content: "Congratulations! The payment was processed by Stripe. A webhook notified our server to update the order.",
                target: '.bg-green-50',
                position: 'bottom'
            },
            {
                title: "Order Confirmed",
                content: "Your order details are shown here. <strong>Important:</strong> The order status was updated by the webhook, not this redirect URL.",
                target: '.card',
                position: 'top'
            },
            {
                title: "View All Orders",
                content: "Let's go to the Orders page to see your new order alongside the demo orders!",
                target: 'a[href="/Orders"]',
                position: 'bottom',
                nextPage: '/Orders'
            }
        ];
    }

    getCancelSteps() {
        // Clear the stripe flag
        sessionStorage.removeItem('tutorial_at_stripe');

        return [
            {
                title: "Payment Cancelled",
                content: "No worries! The payment was cancelled and no charge was made. This happens when users close the Stripe Checkout page.",
                target: '.bg-yellow-50, .card',
                position: 'bottom'
            },
            {
                title: "Order Status",
                content: "Your order is still in 'Pending' status. In a real app, you could retry the payment or the order would expire.",
                target: '.card',
                position: 'top'
            },
            {
                title: "View Your Orders",
                content: "Let's check the Orders page to see all orders including the pending one!",
                target: 'a[href="/Orders"]',
                position: 'bottom',
                nextPage: '/Orders'
            }
        ];
    }

    start() {
        if (this.steps.length === 0) {
            console.log('No tutorial steps for this page');
            return;
        }

        this.isActive = true;
        this.currentStep = 0;
        sessionStorage.setItem('tutorial_active', 'true');
        sessionStorage.setItem('tutorial_step', '0');
        this.showStep(0);
    }

    showStep(index) {
        if (index >= this.steps.length) {
            this.complete();
            return;
        }

        const step = this.steps[index];
        this.currentStep = index;
        sessionStorage.setItem('tutorial_step', index.toString());

        // Show overlay
        this.overlay.style.display = 'block';

        // Find target element
        let targetEl = null;
        if (step.target) {
            targetEl = document.querySelector(step.target);
        }

        // Position tooltip
        this.renderTooltip(step, targetEl, index);

        // Highlight target
        if (targetEl) {
            // Remove previous highlights
            document.querySelectorAll('.tutorial-highlight').forEach(el => {
                el.classList.remove('tutorial-highlight');
            });
            targetEl.classList.add('tutorial-highlight');

            // Scroll into view
            targetEl.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    }

    renderTooltip(step, targetEl, index) {
        const progressDots = this.steps.map((_, i) => {
            let className = 'tutorial-progress-dot';
            if (i < index) className += ' completed';
            else if (i === index) className += ' active';
            return `<div class="${className}"></div>`;
        }).join('');

        this.tooltip.innerHTML = `
            <div style="padding: 20px;">
                <div style="display: flex; align-items: center; gap: 8px; margin-bottom: 12px;">
                    <span style="background: #6366f1; color: white; font-size: 12px; font-weight: 600; padding: 2px 8px; border-radius: 9999px;">
                        Step ${index + 1} of ${this.steps.length}
                    </span>
                </div>
                <h3 style="font-size: 18px; font-weight: 700; color: #111827; margin-bottom: 8px;">
                    ${step.title}
                </h3>
                <p style="color: #6b7280; font-size: 14px; line-height: 1.5; margin-bottom: 16px;">
                    ${step.content}
                </p>
                <div style="display: flex; justify-content: space-between; align-items: center;">
                    <div style="display: flex; gap: 4px;">
                        ${progressDots}
                    </div>
                    <div style="display: flex; gap: 8px;">
                        <button id="tutorial-skip" style="padding: 8px 16px; font-size: 14px; color: #6b7280; background: transparent; border: none; cursor: pointer;">
                            Skip Tour
                        </button>
                        ${index > 0 ? `
                            <button id="tutorial-prev" style="padding: 8px 16px; font-size: 14px; color: #6366f1; background: #f0f4ff; border: none; border-radius: 6px; cursor: pointer; font-weight: 500;">
                                Back
                            </button>
                        ` : ''}
                        <button id="tutorial-next" style="padding: 8px 16px; font-size: 14px; color: white; background: #6366f1; border: none; border-radius: 6px; cursor: pointer; font-weight: 500;">
                            ${step.isLast ? 'Finish' : step.autoAction ? 'Do it! →' : step.nextPage ? 'Continue →' : 'Next'}
                        </button>
                    </div>
                </div>
            </div>
        `;

        // Position the tooltip
        this.positionTooltip(step, targetEl);

        // Add event listeners
        this.tooltip.querySelector('#tutorial-skip').addEventListener('click', () => this.end());
        this.tooltip.querySelector('#tutorial-next').addEventListener('click', () => {
            if (step.autoAction) {
                // Execute auto action with delay for visual effect
                const nextBtn = this.tooltip.querySelector('#tutorial-next');
                nextBtn.textContent = 'Loading...';
                nextBtn.disabled = true;

                setTimeout(() => {
                    const navigates = step.autoAction();
                    if (!navigates) {
                        // If action doesn't navigate, move to next step
                        this.next();
                    }
                    // If it navigates, the page will reload and tutorial continues
                }, step.autoActionDelay || 500);
            } else if (step.nextPage) {
                sessionStorage.setItem('tutorial_step', (index + 1).toString());
                window.location.href = step.nextPage;
            } else if (step.isLast) {
                this.complete();
            } else {
                this.next();
            }
        });

        const prevBtn = this.tooltip.querySelector('#tutorial-prev');
        if (prevBtn) {
            prevBtn.addEventListener('click', () => this.prev());
        }

        this.tooltip.style.display = 'block';
    }

    positionTooltip(step, targetEl) {
        const tooltip = this.tooltip;
        const padding = 16;

        if (step.position === 'center' || !targetEl) {
            // Center on screen
            tooltip.style.top = '50%';
            tooltip.style.left = '50%';
            tooltip.style.transform = 'translate(-50%, -50%)';
            return;
        }

        const rect = targetEl.getBoundingClientRect();
        const tooltipRect = tooltip.getBoundingClientRect();

        tooltip.style.transform = 'none';

        switch (step.position) {
            case 'top':
                tooltip.style.top = `${rect.top - tooltipRect.height - padding}px`;
                tooltip.style.left = `${rect.left + rect.width / 2 - tooltipRect.width / 2}px`;
                break;
            case 'bottom':
                tooltip.style.top = `${rect.bottom + padding}px`;
                tooltip.style.left = `${rect.left + rect.width / 2 - tooltipRect.width / 2}px`;
                break;
            case 'left':
                tooltip.style.top = `${rect.top + rect.height / 2 - tooltipRect.height / 2}px`;
                tooltip.style.left = `${rect.left - tooltipRect.width - padding}px`;
                break;
            case 'right':
                tooltip.style.top = `${rect.top + rect.height / 2 - tooltipRect.height / 2}px`;
                tooltip.style.left = `${rect.right + padding}px`;
                break;
        }

        // Keep tooltip in viewport
        const finalRect = tooltip.getBoundingClientRect();
        if (finalRect.left < padding) {
            tooltip.style.left = `${padding}px`;
        }
        if (finalRect.right > window.innerWidth - padding) {
            tooltip.style.left = `${window.innerWidth - finalRect.width - padding}px`;
        }
        if (finalRect.top < padding) {
            tooltip.style.top = `${padding}px`;
        }
        if (finalRect.bottom > window.innerHeight - padding) {
            tooltip.style.top = `${window.innerHeight - finalRect.height - padding}px`;
        }
    }

    next() {
        this.showStep(this.currentStep + 1);
    }

    prev() {
        if (this.currentStep > 0) {
            this.showStep(this.currentStep - 1);
        }
    }

    end() {
        this.isActive = false;
        sessionStorage.removeItem('tutorial_active');
        sessionStorage.removeItem('tutorial_step');
        this.hide();
    }

    complete() {
        this.isActive = false;
        sessionStorage.removeItem('tutorial_active');
        sessionStorage.removeItem('tutorial_step');
        sessionStorage.setItem('tutorial_completed', 'true');
        this.hide();

        if (this.onComplete) {
            this.onComplete();
        }
    }

    hide() {
        this.overlay.style.display = 'none';
        this.tooltip.style.display = 'none';
        document.querySelectorAll('.tutorial-highlight').forEach(el => {
            el.classList.remove('tutorial-highlight');
        });
    }

    isCompleted() {
        return sessionStorage.getItem('tutorial_completed') === 'true';
    }

    reset() {
        sessionStorage.removeItem('tutorial_completed');
        sessionStorage.removeItem('tutorial_active');
        sessionStorage.removeItem('tutorial_step');
    }
}

// Initialize tutorial when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    window.demoTutorial = new DemoTutorial();

    // Bind event listeners for tutorial buttons (CSP-compliant)

    // Navigation tutorial button
    const navBtn = document.getElementById('nav-start-tutorial');
    if (navBtn) {
        navBtn.addEventListener('click', function() {
            window.demoTutorial.start();
        });
    }

    // Hero "Take the Tour" button
    const heroBtn = document.getElementById('hero-start-tutorial');
    if (heroBtn) {
        heroBtn.addEventListener('click', function() {
            window.demoTutorial.start();
        });
    }

    // Welcome banner start button
    const welcomeStartBtn = document.getElementById('welcome-start-tutorial');
    if (welcomeStartBtn) {
        welcomeStartBtn.addEventListener('click', function() {
            window.demoTutorial.start();
            const welcomeBanner = document.getElementById('tutorial-welcome');
            if (welcomeBanner) {
                welcomeBanner.classList.add('hidden');
            }
        });
    }

    // Welcome banner dismiss button
    const welcomeDismissBtn = document.getElementById('welcome-dismiss');
    if (welcomeDismissBtn) {
        welcomeDismissBtn.addEventListener('click', function() {
            sessionStorage.setItem('tutorial_welcome_dismissed', 'true');
            const welcomeBanner = document.getElementById('tutorial-welcome');
            if (welcomeBanner) {
                welcomeBanner.classList.add('hidden');
            }
        });
    }

    // Show welcome banner for first-time visitors
    const welcomeBanner = document.getElementById('tutorial-welcome');
    if (welcomeBanner) {
        const dismissed = sessionStorage.getItem('tutorial_welcome_dismissed');
        const completed = sessionStorage.getItem('tutorial_completed');
        if (!dismissed && !completed) {
            welcomeBanner.classList.remove('hidden');
        }
    }
});

// Helper function to start tutorial (can be called programmatically)
function startTutorial() {
    if (window.demoTutorial) {
        window.demoTutorial.start();
    }
}

// Helper function to reset tutorial
function resetTutorial() {
    if (window.demoTutorial) {
        window.demoTutorial.reset();
        window.demoTutorial.start();
    }
}
