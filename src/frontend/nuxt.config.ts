// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  devtools: { enabled: true },
  compatibilityDate: '2024-11-01',
  modules: ['nuxt-open-fetch', '@nuxt/ui', '@pinia/nuxt'],
  openFetch: {
    clients: {
      zeitungApi: {
        schema: './openapi.json',
      },
    },
  },
  colorMode: {
    preference: 'light'
  }
})
