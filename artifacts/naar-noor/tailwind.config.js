/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      fontFamily: {
        forum: ['Forum', 'serif'],
        sans: ['Open Sans', 'sans-serif'],
      },
      colors: {
        primary: '#C65A1E',
      },
    },
  },
  plugins: [],
}

