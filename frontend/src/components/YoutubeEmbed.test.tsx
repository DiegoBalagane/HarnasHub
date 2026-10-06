import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'
import { YoutubeEmbed } from './YoutubeEmbed'

const ID = 'dQw4w9WgXcQ'

describe('YoutubeEmbed', () => {
  it.each([
    `https://www.youtube.com/watch?v=${ID}`,
    `https://youtu.be/${ID}`,
    `https://www.youtube.com/shorts/${ID}`,
    `https://www.youtube.com/embed/${ID}`,
  ])('shows a thumbnail facade for %s', (url) => {
    render(<YoutubeEmbed url={url} title="Lineup" />)
    expect(screen.getByAltText('Lineup')).toHaveAttribute('src', `https://img.youtube.com/vi/${ID}/hqdefault.jpg`)
    expect(document.querySelector('iframe')).toBeNull()
  })

  it('mounts the nocookie iframe only after play and honours the start time', async () => {
    render(<YoutubeEmbed url={`https://youtu.be/${ID}?t=42`} title="Lineup" />)
    await userEvent.click(screen.getByTitle('Odtwórz wideo'))
    expect(screen.getByTitle('Lineup')).toHaveAttribute(
      'src',
      `https://www.youtube-nocookie.com/embed/${ID}?autoplay=1&start=42`,
    )
  })

  it('falls back to a plain external link for unrecognised URLs', () => {
    render(<YoutubeEmbed url="https://example.com/video" title="x" />)
    const link = screen.getByRole('link', { name: /Otwórz wideo/ })
    expect(link).toHaveAttribute('href', 'https://example.com/video')
    expect(link).toHaveAttribute('rel', 'noopener noreferrer')
  })
})
