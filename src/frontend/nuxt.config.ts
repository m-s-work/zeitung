// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  devtools: { enabled: true },
  compatibilityDate: '2024-11-01',
  modules: ['nuxt-open-fetch', '@nuxt/ui'],
  openFetch: {
    clients: {
      zeitungApi: {
        schema: './openapi.json',
      },
    },
  },
  runtimeConfig: {
    public: {
      apiBase: process.env.API_BASE_URL || 'http://localhost:8080',
    },
  },
  colorMode: {
    preference: 'dark', // default dark mode
    fallback: 'dark',
  },
  ui: {
    primary: 'blue',
    gray: 'slate',
  },
})
