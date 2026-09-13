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

  const response = await fetch(`${API_SETTINGS.baseUrl}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...init?.headers,
    },
  })

  if (!response.ok) {
    // A 401 on a request that carried a token means the token expired/was revoked — the credentials-check
    // path (login) never sends a token, so this never fires for "wrong password" there.
    if (response.status === 401 && token) {
      localStorage.removeItem(STORAGE_KEYS.accessToken)
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
  patch: <TResponse>(path: string, body: unknown) =>
    request<TResponse>(path, { method: 'PATCH', body: JSON.stringify(body) }),
  delete: <TResponse>(path: string) => request<TResponse>(path, { method: 'DELETE' }),
}
