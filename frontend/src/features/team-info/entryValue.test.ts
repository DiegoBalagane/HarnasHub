import { describe, expect, it } from 'vitest'
import { classifyValue, sortCategories } from './entryValue'

describe('classifyValue', () => {
  it('detects http(s) links', () => {
    expect(classifyValue('https://discord.gg/abc')).toEqual({ type: 'link', href: 'https://discord.gg/abc' })
  })

  it('keeps steam connect URLs as connect actions', () => {
    expect(classifyValue('steam://connect/1.2.3.4:27015')).toEqual({ type: 'connect', href: 'steam://connect/1.2.3.4:27015' })
  })

  it('turns a connect command into a steam URL, including the password', () => {
    expect(classifyValue('connect 1.2.3.4:27015')).toEqual({ type: 'connect', href: 'steam://connect/1.2.3.4:27015' })
    expect(classifyValue('connect 1.2.3.4:27015; password abc')).toEqual({
      type: 'connect',
      href: 'steam://connect/1.2.3.4:27015/abc',
    })
  })

  it('treats multi-line values as code and the rest as text', () => {
    expect(classifyValue('rate 1\ncl_interp 0').type).toBe('code')
    expect(classifyValue('1.2.3.4:27015')).toEqual({ type: 'text', text: '1.2.3.4:27015' })
  })
})

describe('sortCategories', () => {
  it('puts default categories first in their fixed order, then custom ones alphabetically', () => {
    expect(sortCategories(['Zeta', 'Inne', 'Alfa', 'Discord', 'Serwery'])).toEqual(['Discord', 'Serwery', 'Inne', 'Alfa', 'Zeta'])
  })
})
