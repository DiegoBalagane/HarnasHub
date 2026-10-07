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
        // The app shell (index.html) is NOT precached: opening the app always asks the server for the current
        // index.html first (NetworkFirst below), which references the newest hashed bundles — so anyone who opens
        // or logs into the app gets the latest deployed version straight away, without an "Odśwież" step. The
        // cached copy is only a fallback when the network is down or slow. Already-open tabs are still switched
        // over by PwaUpdatePrompt (on navigation / when backgrounded).
        globIgnores: ['**/index.html'],
        navigateFallback: null,
        cleanupOutdatedCaches: true,
        runtimeCaching: [
          {
            // API/hub navigations (e.g. /api/auth/discord/login during OAuth) must always hit the network.
            urlPattern: ({ request, url }) =>
              request.mode === 'navigate' && !url.pathname.startsWith('/api/') && !url.pathname.startsWith('/hubs/'),
            handler: 'NetworkFirst',
            options: {
              cacheName: 'app-shell',
              networkTimeoutSeconds: 4,
              expiration: { maxEntries: 5 },
            },
          },
        ],
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
