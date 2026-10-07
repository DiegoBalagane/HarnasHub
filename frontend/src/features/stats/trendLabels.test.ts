import { describe, expect, it } from 'vitest'
import { recordLabel } from './trendLabels'

describe('recordLabel', () => {
  it('shows draws only when there are some', () => {
    expect(recordLabel({ cumulativeWins: 1, cumulativeLosses: 1, cumulativeDraws: 1 })).toBe('1W / 1R / 1L')
    expect(recordLabel({ cumulativeWins: 2, cumulativeLosses: 1, cumulativeDraws: 0 })).toBe('2W / 1L')
  })
})
