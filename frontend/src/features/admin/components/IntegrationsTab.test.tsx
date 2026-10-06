import { screen, within } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { mockFetch, renderWithProviders } from '../../../test/renderWithProviders'
import { IntegrationsTab } from './IntegrationsTab'

const status = {
  faceitApiKeyConfigured: true,
  faceitDownloadsTokenConfigured: false,
  s3Configured: true,
  discordWebhookConfigured: true,
  frontendBaseUrlConfigured: true,
  discordChannels: { announcements: true, matchSchedule: false, demoReview: true, opponentScouting: false },
}

describe('IntegrationsTab', () => {
  it('lists the four Discord channels with their status and env var names', async () => {
    mockFetch(status)
    renderWithProviders(<IntegrationsTab />)

    const card = (await screen.findByRole('heading', { name: 'Webhook Discorda' })).closest('li') as HTMLElement
    const channel = (name: string) => within(card).getByRole('heading', { name }).closest('li') as HTMLElement

    expect(within(channel('Ogłoszenia')).getByText('Skonfigurowano')).toBeInTheDocument()
    expect(within(channel('Terminarz meczów')).getByText('Brak konfiguracji')).toBeInTheDocument()
    expect(within(channel('Analiza demek')).getByText('Skonfigurowano')).toBeInTheDocument()
    expect(within(channel('Scouting rywali')).getByText('Discord__Webhooks__OpponentScouting')).toBeInTheDocument()
    expect(within(channel('Terminarz meczów')).getByText('Discord__Webhooks__MatchSchedule')).toBeInTheDocument()
  })
})
