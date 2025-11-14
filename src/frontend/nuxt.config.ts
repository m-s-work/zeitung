// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  devtools: { enabled: true },
  compatibilityDate: '2024-11-01',
  modules: ['nuxt-open-fetch', '@nuxt/ui'],
  css: ['~/assets/css/tailwind.css'],
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
  // SSG mode for Tauri compatibility
  ssr: process.env.TAURI_BUILD === 'true' ? false : true,
  colorMode: {
    preference: 'system', // Let users choose, respecting system preference
    fallback: 'light',
    classSuffix: '',
  },
  ui: {
    primary: 'blue',
    gray: 'slate',
  },
})
