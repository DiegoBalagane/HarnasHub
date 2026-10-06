import type { AdminStatus } from '../../../services/adminApi'
import { DiscordChannelsCard } from './DiscordChannelsCard'
import { useAdminStatus } from '../hooks/useAdminStatus'

interface IntegrationCard {
  key: Exclude<keyof AdminStatus, 'discordChannels'>
  title: string
  enables: string
  setup: string
}

/** Static description of each integration; `setup` names the env var / user-secrets key from docs/DEPLOYMENT.md. */
const cards: IntegrationCard[] = [
  {
    key: 'faceitApiKeyConfigured',
    title: 'FACEIT Data API',
    enables: 'Raport rywala pobiera dane z FACEIT (profile, historia, statystyki map).',
    setup: 'Ustaw Faceit__ApiKey (user-secrets: Faceit:ApiKey).',
  },
  {
    key: 'faceitDownloadsTokenConfigured',
    title: 'FACEIT Downloads token',
    enables: 'Przycisk „Pobierz automatycznie” demek rywala w raporcie (wymaga też klucza Data API i S3).',
    setup: 'Ustaw Faceit__DownloadsApiToken (user-secrets: Faceit:DownloadsApiToken).',
  },
  {
    key: 's3Configured',
    title: 'Storage S3',
    enables: 'Wgrywanie dużych plików: demek i teł tablic analizy.',
    setup: 'Ustaw S3__Endpoint, S3__AccessKey, S3__SecretKey i S3__BucketName.',
  },
  {
    key: 'frontendBaseUrlConfigured',
    title: 'Adres aplikacji (Frontend:BaseUrl)',
    enables: 'Linki do aplikacji w wiadomościach, m.in. link do raportu w odprawie.',
    setup: 'Ustaw Frontend__BaseUrl na publiczny adres aplikacji (user-secrets: Frontend:BaseUrl).',
  },
]

/** "Integracje" tab: read-only cards showing which optional integrations are configured (flags only, never secrets). */
export function IntegrationsTab() {
  const { data: status, isLoading, isError } = useAdminStatus()

  if (isLoading) {
    return <p className="text-neutral-400">Ładowanie statusu…</p>
  }

  if (isError || !status) {
    return <p className="text-danger-400">Nie udało się pobrać statusu integracji.</p>
  }

  return (
    <ul className="grid gap-3 md:grid-cols-2">
      {cards.map((card) => {
        const configured = status[card.key]
        return (
          <li key={card.key} className="flex flex-col gap-1 rounded-md border border-neutral-800 p-4">
            <div className="flex items-center justify-between gap-2">
              <h2 className="text-sm font-medium text-neutral-200">{card.title}</h2>
              <span
                className={`rounded-full border px-2 py-0.5 text-xs ${
                  configured ? 'border-success-500/50 text-success-400' : 'border-danger-500/50 text-danger-400'
                }`}
              >
                {configured ? 'Skonfigurowano' : 'Brak konfiguracji'}
              </span>
            </div>
            <p className="text-xs text-neutral-400">{card.enables}</p>
            {!configured && <p className="text-xs text-neutral-500">{card.setup}</p>}
          </li>
        )
      })}
      <DiscordChannelsCard channels={status.discordChannels} />
    </ul>
  )
}
