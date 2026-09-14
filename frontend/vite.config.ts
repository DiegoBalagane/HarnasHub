import tailwindcss from '@tailwindcss/vite'
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'
import { VitePWA } from 'vite-plugin-pwa'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    tailwindcss(),
    VitePWA({
      registerType: 'autoUpdate',
      includeAssets: ['favicon.svg', 'apple-touch-icon.png'],
      workbox: {
        // The PWA's offline navigation fallback must never intercept API/hub calls — a plain
        // `<a href>` navigation to e.g. /api/auth/discord/login is still "mode: navigate", so
        // without this the service worker served the cached app shell instead of hitting the
        // network, breaking Discord OAuth (redirect never happened).
        navigateFallbackDenylist: [/^\/api\//, /^\/hubs\//],
      },
      manifest: {
        name: 'HarnasHub',
        short_name: 'HarnasHub',
        description: 'Team management platform for HArnasiESport',
        theme_color: '#0a0a0a',
        background_color: '#0a0a0a',
        display: 'standalone',
        start_url: '/',
        icons: [
          { src: 'pwa-192x192.png', sizes: '192x192', type: 'image/png' },
          { src: 'pwa-512x512.png', sizes: '512x512', type: 'image/png' },
          { src: 'pwa-512x512.png', sizes: '512x512', type: 'image/png', purpose: 'maskable' },
        ],
      },
    }),
  ],
  server: {
    port: 5173,
  },
})
