import { useAuthStore } from '../features/auth/stores/useAuthStore'
import { API_SETTINGS, STORAGE_KEYS } from '../constants'

export class ApiError extends Error {
  readonly status: number

  constructor(status: number, message: string) {
    super(message)
    this.status = status
  }
}

/** Thin fetch wrapper: adds the base URL, JWT header, and JSON parsing/error handling shared by every API module. */
async function request<TResponse>(path: string, init?: RequestInit): Promise<TResponse> {
  const token = localStorage.getItem(STORAGE_KEYS.accessToken)
  // A FormData body needs the browser to set its own multipart boundary — an explicit
  // Content-Type here would break that, so it's only added for JSON bodies.
  const isFormData = init?.body instanceof FormData

  const response = await fetch(`${API_SETTINGS.baseUrl}${path}`, {
    ...init,
    headers: {
      ...(isFormData ? {} : { 'Content-Type': 'application/json' }),
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...init?.headers,
    },
  })

  if (!response.ok) {
    // A 401 on a request that carried a token means the token expired/was revoked — drop the whole session
    // (not just the stored token) so the user lands on a readable login screen instead of silent failures.
    // A 403 is deliberately left alone: that's a Guest hitting team data, handled by ProtectedRoute.
    if (response.status === 401 && token) {
      useAuthStore.getState().clearSession()
      if (!window.location.pathname.startsWith('/login')) {
        window.location.assign('/login')
      }
    }

    const problem = await response.json().catch(() => null)
    throw new ApiError(response.status, problem?.detail ?? problem?.title ?? 'Wystąpił błąd zapytania.')
  }

  if (response.status === 204) {
    return undefined as TResponse
  }

  return (await response.json()) as TResponse
}

export const apiClient = {
  get: <TResponse>(path: string) => request<TResponse>(path, { method: 'GET' }),
  post: <TResponse>(path: string, body: unknown) =>
    request<TResponse>(path, { method: 'POST', body: JSON.stringify(body) }),
  postForm: <TResponse>(path: string, formData: FormData) => request<TResponse>(path, { method: 'POST', body: formData }),
  patch: <TResponse>(path: string, body: unknown) =>
    request<TResponse>(path, { method: 'PATCH', body: JSON.stringify(body) }),
  put: <TResponse>(path: string, body: unknown) =>
    request<TResponse>(path, { method: 'PUT', body: JSON.stringify(body) }),
  delete: <TResponse>(path: string) => request<TResponse>(path, { method: 'DELETE' }),
}
