import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import type { AnalyzeDemoResult } from '../../../services/resultsApi'
import { mockFetch, renderWithProviders } from '../../../test/renderWithProviders'
import { AddResultForm } from './AddResultForm'

const analysisState = vi.hoisted(() => ({ result: undefined as AnalyzeDemoResult | undefined, reset: vi.fn() }))

vi.mock('../hooks/useResults', () => ({
  useAddResult: () => ({ mutate: vi.fn(), isPending: false, isError: false, error: null }),
  useAnalyzeDemo: () => ({
    start: vi.fn(),
    reset: analysisState.reset,
    job: undefined,
    result: analysisState.result,
    isStarting: false,
    isRunning: false,
    isBusy: false,
    isSucceeded: analysisState.result !== undefined,
    error: null,
  }),
}))
vi.mock('../hooks/useTournaments', async (importOriginal) => ({ ...(await importOriginal<object>()), useTournaments: () => ({ data: [] }) }))
vi.mock('../hooks/useLeagues', async (importOriginal) => ({ ...(await importOriginal<object>()), useLeagues: () => ({ data: [] }) }))
vi.mock('../../opponentReport/hooks/useOpponentReport', () => ({ useLinkOpponentFaceit: () => ({ start: vi.fn() }) }))

const notesPlaceholder = 'Notatki pomeczowe (opcjonalnie)'

function analysis(): AnalyzeDemoResult {
  return {
    roundsPlayed: 24,
    mapName: 'Mirage',
    teamA: { playerNames: ['a'], steamIds: ['1'], ourScore: 13, opponentScore: 11 },
    teamB: { playerNames: ['b'], steamIds: ['2'], ourScore: 11, opponentScore: 13 },
    suggestedTeam: 'A',
    players: [],
    pendingTimelineKey: null,
    faceitMatch: {
      matchId: 'm1',
      competitionName: 'Cup',
      category: 'Tournament',
      playedAtUtc: '2026-10-01T18:00:00Z',
      mapName: 'Mirage',
      ourFactionIndex: 0,
      factions: [
        { name: 'Us', demoTeam: 'A', playerIds: [], nicknames: ['u'], linkedOpponentName: null },
        { name: 'Them', demoTeam: 'B', playerIds: [], nicknames: ['t'], linkedOpponentName: null },
      ],
    },
  } as unknown as AnalyzeDemoResult
}

describe('AddResultForm demo analysis prefill', () => {
  beforeEach(() => {
    analysisState.result = undefined
    mockFetch([])
  })

  it('keeps fields the coach already edited when the analysis finishes, and fills the untouched ones', async () => {
    renderWithProviders(<AddResultForm />)
    await userEvent.selectOptions(screen.getByDisplayValue('Sparing'), 'League')
    await userEvent.type(screen.getByPlaceholderText('Nasz wynik'), '7')
    await userEvent.type(screen.getByPlaceholderText(notesPlaceholder), 'ważne notatki')

    analysisState.result = analysis()
    // Any state change re-renders the form; the slotted job result is then picked up by its effect.
    await userEvent.type(screen.getByPlaceholderText(notesPlaceholder), '!')

    expect(await screen.findByText(/Rund w demce: 24/)).toBeInTheDocument()
    expect(screen.getByDisplayValue('Liga')).toBeInTheDocument()
    expect(screen.getByPlaceholderText('Nasz wynik')).toHaveValue(7)
    expect(screen.getByPlaceholderText(notesPlaceholder)).toHaveValue('ważne notatki!')
    expect(screen.getByDisplayValue('Mirage')).toBeInTheDocument()
    expect(screen.getByPlaceholderText('Przeciwnik')).toHaveValue('Them')
  })

  it('applies category and score from the analysis when nothing was touched', async () => {
    analysisState.result = analysis()
    renderWithProviders(<AddResultForm />)

    expect(await screen.findByText(/Rund w demce: 24/)).toBeInTheDocument()
    expect(screen.getByDisplayValue('Turniej')).toBeInTheDocument()
    expect(screen.getByPlaceholderText('Nasz wynik')).toHaveValue(13)
  })
})
