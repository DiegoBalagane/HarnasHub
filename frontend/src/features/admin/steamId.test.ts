import { describe, expect, it } from 'vitest'
import { isValidSteamId64 } from './steamId'

describe('isValidSteamId64', () => {
  it('accepts an empty value and a well-formed id', () => {
    expect(isValidSteamId64('')).toBe(true)
    expect(isValidSteamId64('  ')).toBe(true)
    expect(isValidSteamId64('76561198012345678')).toBe(true)
  })

  it('rejects wrong length, prefix and non-digits', () => {
    expect(isValidSteamId64('7656119801234567')).toBe(false)
    expect(isValidSteamId64('765611980123456789')).toBe(false)
    expect(isValidSteamId64('12345678901234567')).toBe(false)
    expect(isValidSteamId64('7656119801234567a')).toBe(false)
  })
})
