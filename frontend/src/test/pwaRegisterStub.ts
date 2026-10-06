/** Test-only stand-in for `virtual:pwa-register/react`, which exists only when the PWA plugin runs; tests vi.mock it. */
export function useRegisterSW() {
  return { needRefresh: [false, () => {}] as const, offlineReady: [false, () => {}] as const, updateServiceWorker: async () => {} }
}
