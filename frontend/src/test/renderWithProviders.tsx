import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, type RenderResult } from '@testing-library/react'
import type { ReactElement } from 'react'
import { MemoryRouter } from 'react-router-dom'
import { vi } from 'vitest'

interface RenderOptions {
  /** Initial history entries of the MemoryRouter (default: "/"). */
  route?: string | string[]
}

/** Renders a component inside a retry-free QueryClient and a MemoryRouter at the given route(s). */
export function renderWithProviders(ui: ReactElement, { route = '/' }: RenderOptions = {}): RenderResult {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false, gcTime: 0 }, mutations: { retry: false } },
  })
  const entries = Array.isArray(route) ? route : [route]

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={entries}>{ui}</MemoryRouter>
    </QueryClientProvider>,
  )
}

/** Replaces global fetch with a mock returning the given JSON body/status (restored automatically after each test). */
export function mockFetch(body: unknown, status = 200) {
  const fetchMock = vi.fn(async (_input: string | URL | Request, _init?: RequestInit) => new Response(status === 204 ? null : JSON.stringify(body), { status }))
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}
