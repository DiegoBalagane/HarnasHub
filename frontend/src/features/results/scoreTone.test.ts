import { describe, expect, it } from 'vitest'
import { scoreToneClass } from './scoreTone'

describe('scoreToneClass', () => {
  it('colours wins, losses and draws differently', () => {
    expect(scoreToneClass(13, 7)).toBe('text-success-400')
    expect(scoreToneClass(3, 13)).toBe('text-danger-400')
    expect(scoreToneClass(12, 12)).toBe('text-neutral-300')
  })
})
