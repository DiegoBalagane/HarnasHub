import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import type { TeamMember } from '../../../services/rosterApi'
import { renderWithProviders } from '../../../test/renderWithProviders'
import { PendingAccounts } from './PendingAccounts'

const guest: TeamMember = {
  id: '11111111-1111-1111-1111-111111111111',
  displayName: 'Nowy',
  role: 'Guest',
  isCoach: false,
  avatarUrl: null,
  teamRole: null,
  rosterSlot: null,
  pinColor: null,
  pinMark: null,
  inGameNickname: null,
  steamId64: null,
  showInStats: true,
  showInCalendar: true,
  faceitNickname: null,
  secondaryTeamRoles: [],
}

function stubFetch() {
  const fetchMock = vi.fn(async (_input: string | URL | Request, init?: RequestInit) =>
    new Response(JSON.stringify(init?.method === 'PATCH' ? { ...guest, role: 'Player' } : [guest]), { status: 200 }),
  )
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

describe('PendingAccounts', () => {
  it('grants the Player level through the role endpoint', async () => {
    const fetchMock = stubFetch()
    renderWithProviders(<PendingAccounts />)

    await userEvent.click(await screen.findByRole('button', { name: 'Nadaj: Zawodnik' }))

    await waitFor(() => {
      const patch = fetchMock.mock.calls.find(([, init]) => init?.method === 'PATCH')
      expect(String(patch?.[0])).toContain(`/api/roster/${guest.id}/role`)
      expect(patch?.[1]?.body).toBe(JSON.stringify({ role: 'Player' }))
    })
  })

  it('grants the Manager level', async () => {
    const fetchMock = stubFetch()
    renderWithProviders(<PendingAccounts />)

    await userEvent.click(await screen.findByRole('button', { name: 'Nadaj: Zarządca' }))

    await waitFor(() => {
      const patch = fetchMock.mock.calls.find(([, init]) => init?.method === 'PATCH')
      expect(patch?.[1]?.body).toBe(JSON.stringify({ role: 'Manager' }))
    })
  })
})
