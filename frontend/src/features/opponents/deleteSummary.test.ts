import { describe, expect, it } from 'vitest'
import type { OpponentDeletePreview } from '../../services/opponentsApi'
import { hasHistory, historyLabel, pluralPl, scoutingLabel } from './deleteSummary'

const empty: OpponentDeletePreview = {
  notes: 0,
  demoAnalyses: 0,
  hasFaceitLink: false,
  hasReportSnapshot: false,
  matchResults: 0,
  events: 0,
  isHidden: false,
}

describe('deleteSummary', () => {
  it('uses the correct Polish plural forms', () => {
    expect(pluralPl(1, 'wynik', 'wyniki', 'wyników')).toBe('wynik')
    expect(pluralPl(3, 'wynik', 'wyniki', 'wyników')).toBe('wyniki')
    expect(pluralPl(12, 'wynik', 'wyniki', 'wyników')).toBe('wyników')
    expect(pluralPl(22, 'wynik', 'wyniki', 'wyników')).toBe('wyniki')
    expect(pluralPl(5, 'wynik', 'wyniki', 'wyników')).toBe('wyników')
  })

  it('describes the history that a delete keeps by default', () => {
    expect(historyLabel({ matchResults: 3, events: 1 })).toBe('3 wyniki, 1 wydarzenie')
    expect(historyLabel({ matchResults: 0, events: 5 })).toBe('5 wydarzeń')
    expect(historyLabel({ matchResults: 0, events: 0 })).toBe('')
  })

  it('describes the scouting data that is always removed', () => {
    expect(
      scoutingLabel({ ...empty, notes: 2, hasFaceitLink: true, hasReportSnapshot: true, demoAnalyses: 1 }),
    ).toBe('2 notatki, powiązanie FACEIT, raport, 1 analiza demki')
    expect(scoutingLabel(empty)).toBe('')
  })

  it('detects whether results or events reference the opponent', () => {
    expect(hasHistory({ matchResults: 0, events: 0 })).toBe(false)
    expect(hasHistory({ matchResults: 0, events: 1 })).toBe(true)
  })
})
