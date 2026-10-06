import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import type { TeamMember } from '../../../services/rosterApi'
import { renderWithProviders } from '../../../test/renderWithProviders'
import { UserRow } from './UserRow'

const member: TeamMember = {
  id: '11111111-1111-1111-1111-111111111111',
  displayName: 'Zenek',
  role: 'Player',
  isCoach: false,
  avatarUrl: null,
  teamRole: null,
  rosterSlot: 'Main',
  pinColor: null,
  pinMark: null,
  inGameNickname: null,
  steamId64: null,
  secondaryTeamRoles: [],
  showInStats: true,
  showInCalendar: true,
  faceitNickname: null,
}

function stubFetch() {
  const fetchMock = vi.fn(async (_input: string | URL | Request, _init?: RequestInit) =>
    new Response(JSON.stringify(member), { status: 200 }),
  )
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

function lastPatch(fetchMock: ReturnType<typeof stubFetch>) {
  return fetchMock.mock.calls.find(([, init]) => init?.method === 'PATCH')
}

describe('UserRow visibility and FACEIT nickname', () => {
  it('saves the stats toggle at once, keeping the calendar flag', async () => {
    const fetchMock = stubFetch()
    renderWithProviders(<UserRow member={member} isSelf={false} onDelete={() => undefined} />)

    await userEvent.click(screen.getByRole('checkbox', { name: 'Statystyki: Zenek' }))

    await waitFor(() => {
      const patch = lastPatch(fetchMock)
      expect(String(patch?.[0])).toContain(`/api/roster/${member.id}/visibility`)
      expect(patch?.[1]?.body).toBe(JSON.stringify({ showInStats: false, showInCalendar: true }))
    })
  })

  it('saves the calendar toggle keeping the stats flag and shows hidden state', async () => {
    const fetchMock = stubFetch()
    renderWithProviders(
      <UserRow member={{ ...member, showInStats: false }} isSelf={false} onDelete={() => undefined} />,
    )

    expect(screen.getByRole('checkbox', { name: 'Statystyki: Zenek' })).not.toBeChecked()
    await userEvent.click(screen.getByRole('checkbox', { name: 'Kalendarz: Zenek' }))

    await waitFor(() => {
      expect(lastPatch(fetchMock)?.[1]?.body).toBe(JSON.stringify({ showInStats: false, showInCalendar: false }))
    })
  })

  it('saves a trimmed FACEIT nickname through the dedicated endpoint', async () => {
    const fetchMock = stubFetch()
    renderWithProviders(<UserRow member={member} isSelf={false} onDelete={() => undefined} />)
    const save = screen.getAllByRole('button', { name: 'Zapisz' })[1]

    expect(save).toBeDisabled()
    await userEvent.type(screen.getByLabelText('Nick FACEIT: Zenek'), '  s1mple ')
    await userEvent.click(save)

    await waitFor(() => {
      const patch = lastPatch(fetchMock)
      expect(String(patch?.[0])).toContain(`/api/roster/${member.id}/faceit-nickname`)
      expect(patch?.[1]?.body).toBe(JSON.stringify({ nickname: 's1mple' }))
    })
  })

  it('rejects a blank nickname and clears a saved one with null', async () => {
    const fetchMock = stubFetch()
    renderWithProviders(
      <UserRow member={{ ...member, faceitNickname: 'old' }} isSelf={false} onDelete={() => undefined} />,
    )
    const input = screen.getByLabelText('Nick FACEIT: Zenek')

    await userEvent.type(input, '   ')
    await userEvent.clear(input)
    await userEvent.click(screen.getAllByRole('button', { name: 'Zapisz' })[1])

    await waitFor(() => {
      expect(lastPatch(fetchMock)?.[1]?.body).toBe(JSON.stringify({ nickname: null }))
    })
  })

  it('flags a whitespace-only draft as invalid and disables saving', async () => {
    stubFetch()
    renderWithProviders(<UserRow member={member} isSelf={false} onDelete={() => undefined} />)

    await userEvent.type(screen.getByLabelText('Nick FACEIT: Zenek'), '   ')

    expect(screen.getByText('Nick nie może być pusty')).toBeInTheDocument()
    expect(screen.getAllByRole('button', { name: 'Zapisz' })[1]).toBeDisabled()
  })
})
