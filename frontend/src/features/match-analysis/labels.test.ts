import { describe, expect, it } from 'vitest'
import { buyTypeLabels, buyTypeShort, endReasonLabels, formatRoundTime } from './labels'

describe('match analysis labels', () => {
  it('formats round time as m:ss', () => {
    expect(formatRoundTime(0)).toBe('0:00')
    expect(formatRoundTime(115.9)).toBe('1:55')
    expect(formatRoundTime(-5)).toBe('0:00')
  })

  it('has a badge for every buy type and a label for every end reason', () => {
    for (const type of Object.keys(buyTypeLabels)) {
      expect(buyTypeShort[type as keyof typeof buyTypeShort]).toBeTruthy()
    }
    for (const label of Object.values(endReasonLabels)) {
      expect(label.length).toBeGreaterThan(0)
    }
  })
})
