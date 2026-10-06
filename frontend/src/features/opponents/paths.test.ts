import { describe, expect, it } from 'vitest'
import { opponentProfilePath, opponentReportPath } from './paths'

describe('opponent paths', () => {
  it('encodes names with reserved characters into the query string', () => {
    expect(opponentProfilePath('A/B?&team')).toBe('/opponents/profile?name=A%2FB%3F%26team')
  })

  it('trims the name before encoding', () => {
    expect(opponentReportPath('  Navi ')).toBe('/opponents/report?name=Navi')
  })

  it('round-trips through URLSearchParams', () => {
    const name = 'Team / ? # & Ü'
    const query = opponentReportPath(name).split('?')[1]
    expect(new URLSearchParams(query).get('name')).toBe(name)
  })
})
