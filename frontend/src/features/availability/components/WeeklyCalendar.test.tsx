import { screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { renderWithProviders } from '../../../test/renderWithProviders'
import { useAuthStore } from '../../auth/stores/useAuthStore'
import { makeMember, testToday } from '../availabilityTestData'
import { WeeklyCalendar } from './WeeklyCalendar'

const state = vi.hoisted(() => ({ members: [] as unknown[], isDesktop: true }))
const mutateAsync = vi.hoisted(() => vi.fn())

vi.mock('../hooks/useAvailability', () => ({
  useWeekAvailability: () => ({ data: { members: state.members }, isLoading: false, isError: false }),
  useSetDayAvailability: () => ({ mutate: vi.fn(), mutateAsync, isPending: false, isError: false }),
}))
vi.mock('../../calendar/hooks/useCalendar', () => ({
  useUpcomingEvents: () => ({
    data: [
      { id: 'e1', title: 'Mecz z Rywalami', type: 'Match', startsAtUtc: `${testToday}T17:00:00Z`, endsAtUtc: null, location: null, url: null, notes: null, opponent: null },
    ],
  }),
}))
vi.mock('../../calendar/hooks/useIsDesktop', () => ({ useIsDesktop: () => state.isDesktop }))

function setMembers() {
  state.members = [
    makeMember('me', 'Ja', { base: { status: 'Available' }, overrides: { '2026-10-08': { status: 'PartiallyAvailable', from: '16:00:00', to: '21:00:00' } } }),
    makeMember('p2', 'Anna', { base: { status: 'Available' }, overrides: { '2026-10-09': { isVacation: true, status: 'Off' } } }),
    makeMember('p3', 'Bartek', { rosterSlot: 'Bench' }),
    makeMember('coach', 'Capybar', { rosterSlot: null, isCoach: true, base: { status: 'Off' } }),
  ]
}

describe('WeeklyCalendar', () => {
  beforeEach(() => {
    vi.useFakeTimers({ toFake: ['Date'] })
    vi.setSystemTime(new Date(`${testToday}T12:00:00`))
    useAuthStore.setState({ userId: 'me' })
    state.isDesktop = true
    mutateAsync.mockReset()
    setMembers()
  })
  afterEach(() => vi.useRealTimers())

  it('renders the day header fill summary and common window', () => {
    renderWithProviders(<WeeklyCalendar />)

    const bars = screen.getAllByRole('progressbar', { name: 'Wypełnienie głównego składu' })
    expect(bars).toHaveLength(7)
    expect(bars[0]).toHaveAttribute('aria-valuenow', '2')
    expect(bars[0]).toHaveAttribute('aria-valuemax', '2')
    expect(screen.getByRole('link', { name: 'Wydarzenie: Mecz z Rywalami' })).toBeInTheDocument()
  })

  it('renders pills per status', () => {
    renderWithProviders(<WeeklyCalendar />)

    expect(screen.getByRole('button', { name: `Ja 2026-10-08: Częściowo dostępny` })).toHaveTextContent('16–21')
    expect(screen.getByRole('img', { name: 'Anna 2026-10-09: Urlop' })).toHaveTextContent('Urlop')
    expect(screen.getByRole('img', { name: `Bartek ${testToday}: Brak odpowiedzi` })).toHaveTextContent('—')
  })

  it('opens the editor popover from an own cell but not from someone else', async () => {
    const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime })
    renderWithProviders(<WeeklyCalendar />)

    expect(screen.queryByRole('button', { name: /Anna 2026-10-08/ })).not.toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: `Ja ${testToday}: Dostępny` }))

    const dialog = screen.getByRole('dialog', { name: 'Edycja dostępności' })
    expect(within(dialog).getByRole('button', { name: 'Nie gram tego dnia' })).toBeInTheDocument()
  })

  it('keeps the coach on one collapsed line and expands on click', async () => {
    const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime })
    renderWithProviders(<WeeklyCalendar />)

    const toggle = screen.getByRole('button', { name: /Trener: Capybar — off cały tydzień/ })
    expect(toggle).toHaveAttribute('aria-expanded', 'false')
    expect(screen.queryByRole('img', { name: new RegExp(`Capybar ${testToday}`) })).not.toBeInTheDocument()

    await user.click(toggle)
    expect(screen.getByRole('img', { name: `Capybar ${testToday}: Off` })).toBeInTheDocument()
  })

  it('shows a single day with arrows on phones', async () => {
    state.isDesktop = false
    const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime })
    renderWithProviders(<WeeklyCalendar />)

    expect(screen.getAllByRole('progressbar')).toHaveLength(1)
    expect(screen.getByRole('button', { name: `Ja ${testToday}: Dostępny` })).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'Następny dzień' }))
    expect(screen.getByRole('button', { name: 'Ja 2026-10-08: Częściowo dostępny' })).toHaveTextContent('16–21')
    expect(screen.queryByRole('button', { name: `Ja ${testToday}: Dostępny` })).not.toBeInTheDocument()
  })

  it('bulk dialog preselects eligible days, honours chips and saves one call per selected day', async () => {
    const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime })
    mutateAsync.mockResolvedValue(undefined)
    state.members = [makeMember('me', 'Ja', { overrides: { '2026-10-09': { isVacation: true, status: 'Off' } } })]
    renderWithProviders(<WeeklyCalendar />)

    await user.click(screen.getByRole('button', { name: 'Ustaw dla tygodnia…' }))
    const boxes = () => screen.getAllByRole('checkbox').filter((box) => !box.closest('label')?.textContent?.includes('Cały dzień'))
    // 7 days, the vacation day is disabled and not selected.
    expect(boxes().filter((box) => (box as HTMLInputElement).checked)).toHaveLength(6)
    expect(screen.getByLabelText(/Pt 09.10/)).toBeDisabled()

    await user.click(screen.getByRole('button', { name: 'Weekend' }))
    expect(boxes().filter((box) => (box as HTMLInputElement).checked)).toHaveLength(2)

    await user.click(screen.getByRole('button', { name: 'Dni robocze' }))
    await user.click(screen.getByRole('button', { name: 'Zastosuj' }))

    // Workdays among eligible days: Wed 7, Thu 8, Mon 12, Tue 13 (Fri 9 is on vacation).
    expect(mutateAsync).toHaveBeenCalledTimes(4)
    expect(mutateAsync.mock.calls.map(([payload]) => payload.date)).toEqual(['2026-10-07', '2026-10-08', '2026-10-12', '2026-10-13'])
    expect(mutateAsync.mock.calls[0][0]).toMatchObject({ status: 'Available', availableFromLocal: null })
  })

  it('reports days that failed to save and keeps the dialog open', async () => {
    const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime })
    mutateAsync.mockImplementation(async (payload: { date: string }) => {
      if (payload.date === '2026-10-08') throw new Error('boom')
    })
    state.members = [makeMember('me', 'Ja')]
    renderWithProviders(<WeeklyCalendar />)

    await user.click(screen.getByRole('button', { name: 'Ustaw dla tygodnia…' }))
    await user.click(screen.getByRole('button', { name: 'Zastosuj' }))

    expect(mutateAsync).toHaveBeenCalledTimes(7)
    expect(await screen.findByText(/Nie udało się zapisać części dni/)).toBeInTheDocument()
  })
})
