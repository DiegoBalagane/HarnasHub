import { memo } from 'react'
import type { DiscordChannelsStatus } from '../../../services/adminApi'

interface ChannelInfo {
  key: keyof DiscordChannelsStatus
  title: string
  enables: string
  envVar: string
}

/** The four Discord notification channels, what each receives and the env var configuring its webhook. */
export const discordChannelInfos: ChannelInfo[] = [
  {
    key: 'announcements',
    title: 'Ogłoszenia',
    enables: 'Nowe wydarzenia (poza meczami), zadania i zwykłe przypomnienia.',
    envVar: 'Discord__Webhooks__Announcements',
  },
  {
    key: 'matchSchedule',
    title: 'Terminarz meczów',
    enables: 'Mecze (dodanie, zmiana, usunięcie), przypomnienia o meczach i wyniki.',
    envVar: 'Discord__Webhooks__MatchSchedule',
  },
  {
    key: 'demoReview',
    title: 'Analiza demek',
    enables: 'Podsumowanie po przeanalizowaniu demki dołączonej do wyniku meczu.',
    envVar: 'Discord__Webhooks__DemoReview',
  },
  {
    key: 'opponentScouting',
    title: 'Scouting rywali',
    enables: 'Odprawa przed meczem i podsumowanie tendencji po analizie demek rywala.',
    envVar: 'Discord__Webhooks__OpponentScouting',
  },
]

interface DiscordChannelsCardProps {
  channels: DiscordChannelsStatus
}

/** "Webhook Discorda" card: status of each of the four channels (own webhook or the Discord__WebhookUrl fallback). */
export const DiscordChannelsCard = memo(function DiscordChannelsCard({ channels }: DiscordChannelsCardProps) {
  return (
    <li className="flex flex-col gap-2 rounded-md border border-neutral-800 p-4 md:col-span-2">
      <h2 className="text-sm font-medium text-neutral-200">Webhook Discorda</h2>
      <p className="text-xs text-neutral-400">
        Każdy typ powiadomień trafia na własny kanał. Pusty webhook kanału korzysta z Discord__WebhookUrl (jeśli ustawiony).
      </p>
      <ul className="grid gap-2 md:grid-cols-2">
        {discordChannelInfos.map((info) => {
          const configured = channels[info.key]
          return (
            <li key={info.key} className="flex flex-col gap-1 rounded border border-neutral-800 p-3">
              <div className="flex items-center justify-between gap-2">
                <h3 className="text-sm text-neutral-200">{info.title}</h3>
                <span
                  className={`rounded-full border px-2 py-0.5 text-xs ${
                    configured ? 'border-success-500/50 text-success-400' : 'border-danger-500/50 text-danger-400'
                  }`}
                >
                  {configured ? 'Skonfigurowano' : 'Brak konfiguracji'}
                </span>
              </div>
              <p className="text-xs text-neutral-400">{info.enables}</p>
              <p className="text-xs text-neutral-500">{info.envVar}</p>
            </li>
          )
        })}
      </ul>
    </li>
  )
})
