/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Pages/**/*.cshtml',
    './Views/**/*.cshtml',
    './Models/**/*.cs'
  ],
  safelist: [
    // Product gradient classes (dynamically applied)
    'from-green-400', 'to-green-600',
    'from-indigo-400', 'to-indigo-600',
    'from-purple-400', 'to-purple-600',
    'from-gray-400', 'to-gray-600',
  ],
  theme: {
    extend: {
      colors: {
        // Stripe-inspired color palette
        stripe: {
          purple: '#635bff',
          'purple-dark': '#5851ea',
          'purple-light': '#7a73ff',
          blue: '#00d4ff',
          cyan: '#80e9ff',
          green: '#00d924',
          orange: '#ff9500',
          red: '#ff5567',
        },
        // Custom brand colors
        brand: {
          50: '#f0f4ff',
          100: '#e0e7ff',
          200: '#c7d2fe',
          300: '#a5b4fc',
          400: '#818cf8',
          500: '#6366f1',
          600: '#4f46e5',
          700: '#4338ca',
          800: '#3730a3',
          900: '#312e81',
          950: '#1e1b4b',
        }
      },
      fontFamily: {
        sans: [
          '-apple-system',
          'BlinkMacSystemFont',
          'Segoe UI',
          'Roboto',
          'Helvetica Neue',
          'Ubuntu',
          'sans-serif'
        ],
      },
      boxShadow: {
        'stripe': '0 6px 20px rgba(0, 0, 0, 0.06)',
        'stripe-hover': '0 8px 30px rgba(0, 0, 0, 0.12)',
      }
    },
  },
  plugins: [],
}
