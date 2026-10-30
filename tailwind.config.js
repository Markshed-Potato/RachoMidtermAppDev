/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./**/*.razor", "./**/*.html", "./**/*.cshtml"],
  theme: {
    extend: {
      colors: {
        ink: { 950: "#070b17", 900: "#0f172a", 800: "#162036" },
        neon: { cyan: "#22d3ee", violet: "#a78bfa", pink: "#f472b6", lime: "#a3e635" },
      },
      fontFamily: {
        sans: ["Inter", "ui-sans-serif", "system-ui", "sans-serif"],
        mono: ["JetBrains Mono", "ui-monospace", "monospace"],
      },
      boxShadow: {
        glow: "0 0 24px -4px rgba(34,211,238,.45)",
        "glow-violet": "0 0 28px -4px rgba(167,139,250,.5)",
      },
      keyframes: {
        "toast-in": { from: { opacity: 0, transform: "translateY(12px) scale(.96)" }, to: { opacity: 1, transform: "none" } },
        float: { "0%,100%": { transform: "translateY(0)" }, "50%": { transform: "translateY(-10px)" } },
        eq: { "0%,100%": { height: "20%" }, "50%": { height: "100%" } },
        "fade-up": { from: { opacity: 0, transform: "translateY(16px)" }, to: { opacity: 1, transform: "none" } },
      },
      animation: {
        "toast-in": "toast-in .25s ease-out both",
        float: "float 6s ease-in-out infinite",
        eq: "eq .9s ease-in-out infinite",
        "fade-up": "fade-up .5s ease-out both",
      },
    },
  },
  plugins: [],
};
