import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { renderWithProviders } from '../../../test/renderWithProviders'
import { toIsoDate } from '../../availability/weekDates'
import { EventCalendar } from './EventCalendar'

let canManage = false
vi.mock('../../auth/hooks/useIsCoachOrManager', () => ({ useIsCoachOrManager: () => canManage }))
vi.mock('./EventDetails', () => ({ EventDetails: () => <div>details-body</div> }))

const start = new Date()
start.setHours(18, 0, 0, 0)
const events = [
  {
    id: 'e1',
    title: 'Trening taktyczny',
    type: 'Training',
    startsAtUtc: start.toISOString(),
    endsAtUtc: null,
    location: null,
    url: null,
    notes: null,
    opponent: null,
  },
]

function stubApi() {
  vi.stubGlobal(
    'fetch',
    vi.fn(async (input: string | URL | Request) => {
      const url = String(input)
      const body = url.includes('/availability') ? { weekStart: '', members: [] } : events
      return new Response(JSON.stringify(body), { status: 200 })
    }),
  )
}

describe('EventCalendar', () => {
  beforeEach(() => {
    canManage = false
    stubApi()
  })

  it('switches between month, week and agenda views', async () => {
    renderWithProviders(<EventCalendar />)
    await screen.findByText(/Trening taktyczny/)
    expect(screen.getAllByRole('gridcell').length).toBeGreaterThanOrEqual(28)

    await userEvent.click(screen.getByRole('button', { name: 'Tydzień' }))
    expect(screen.queryAllByRole('gridcell')).toHaveLength(0)
    expect(screen.getByText('08:00')).toBeInTheDocument()

    await userEvent.click(screen.getByRole('button', { name: 'Agenda' }))
    expect(screen.getByText(/Dziś ·/)).toBeInTheDocument()
  })

  it('opens the event details for the ?event= deep link', async () => {
    renderWithProviders(<EventCalendar />, { route: '/calendar?event=e1' })
    expect(await screen.findByRole('dialog', { name: /Trening taktyczny/ })).toBeInTheDocument()
    expect(screen.getByText('details-body')).toBeInTheDocument()
  })

  it('lets only Coach/Manager create on an empty day', async () => {
    const { unmount } = renderWithProviders(<EventCalendar />)
    await screen.findByText(/Trening taktyczny/)
    await userEvent.click(screen.getByRole('gridcell', { name: toIsoDate(new Date()) }))
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
    unmount()

    canManage = true
    renderWithProviders(<EventCalendar />)
    await screen.findByText(/Trening taktyczny/)
    await userEvent.click(screen.getByRole('gridcell', { name: toIsoDate(new Date()) }))
    await waitFor(() => expect(screen.getByRole('dialog', { name: /Dodaj wydarzenie/ })).toBeInTheDocument())
  })
})
