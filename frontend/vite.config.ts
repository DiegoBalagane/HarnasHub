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
      // 'prompt' (not 'autoUpdate') so a new deployed build waits for the one-click "Odśwież" banner
      // (PwaUpdatePrompt, wired through virtual:pwa-register/react) instead of only refreshing itself
      // on the *next* full navigation — without this, an already-open tab could keep running stale JS
      // until someone thinks to hard-refresh it.
      registerType: 'prompt',
      // Registration is handled by the PwaUpdatePrompt component via virtual:pwa-register/react instead.
      injectRegister: false,
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
