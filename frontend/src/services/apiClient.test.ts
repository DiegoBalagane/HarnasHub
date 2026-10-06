import { describe, expect, it } from 'vitest'
import { STORAGE_KEYS } from '../constants'
import { mockFetch } from '../test/renderWithProviders'
import { ApiError, apiClient } from './apiClient'

describe('apiClient', () => {
  it('parses JSON and sends the bearer token when present', async () => {
    localStorage.setItem(STORAGE_KEYS.accessToken, 'tok')
    const fetchMock = mockFetch({ ok: true })

    await expect(apiClient.get('/api/x')).resolves.toEqual({ ok: true })

    const init = fetchMock.mock.calls[0][1] as RequestInit
    expect((init.headers as Record<string, string>).Authorization).toBe('Bearer tok')
  })

  it('returns undefined on 204', async () => {
    mockFetch(null, 204)
    await expect(apiClient.delete('/api/x')).resolves.toBeUndefined()
  })

  it('prefers the first validation error message', async () => {
    mockFetch({ errors: { Name: ['Nazwa jest wymagana.'] }, title: 'Bad' }, 400)
    await expect(apiClient.post('/api/x', {})).rejects.toMatchObject({ status: 400, message: 'Nazwa jest wymagana.' })
  })

  it('falls back to problem detail, then to a generic message', async () => {
    mockFetch({ detail: 'Szczegol' }, 409)
    const error = await apiClient.get('/api/x').catch((e: unknown) => e)
    expect(error).toBeInstanceOf(ApiError)
    expect((error as ApiError).message).toBe('Szczegol')

    mockFetch({}, 500)
    await expect(apiClient.get('/api/x')).rejects.toMatchObject({ message: 'Wystąpił błąd zapytania.' })
  })
})
