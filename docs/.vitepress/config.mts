import { defineConfig } from 'vitepress'

// https://vitepress.dev/reference/site-config
export default defineConfig({
  title: "Zeitung",
  description: "Smart RSS Feed Reader Documentation",
  base: '/zeitung/',
  ignoreDeadLinks: true,
  
  themeConfig: {
    // https://vitepress.dev/reference/default-theme-config
    nav: [
      { text: 'Home', link: '/' },
      { text: 'Guide', link: '/guide/getting-started' },
      { text: 'API', link: '/api/overview' }
    ],

    sidebar: [
      {
        text: 'Introduction',
        items: [
          { text: 'What is Zeitung?', link: '/guide/what-is-zeitung' },
          { text: 'Getting Started', link: '/guide/getting-started' },
          { text: 'Architecture', link: '/guide/architecture' }
        ]
      },
      {
        text: 'API Reference',
        items: [
          { text: 'Overview', link: '/api/overview' },
          { text: 'Authentication', link: '/api/authentication' },
          { text: 'API Reference', link: '/api/reference' }
        ]
      },
      {
        text: 'Development',
        items: [
          { text: 'Contributing', link: '/development/contributing' },
          { text: 'Testing', link: '/development/testing' }
        ]
      }
    ],

    socialLinks: [
      { icon: 'github', link: 'https://github.com/m-s-work/zeitung' }
    ],

    footer: {
      // message: 'Released under the MIT License.',
      copyright: 'Copyright © 2025-present'
    }
  }
})
