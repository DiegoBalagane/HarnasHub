import { memo } from 'react'
import { Link } from 'react-router-dom'
import type { CalendarEvent } from '../../../services/calendarApi'
import type { OpponentMapRecord, OpponentMatch } from '../../../services/opponentsApi'
import { eventTypeLabels } from '../../calendar/labels'
import { matchCategoryLabels } from '../../results/labels'
import { RecordBadge } from './RecordBadge'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium' })
const dateTimeFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'full', timeStyle: 'short' })

/** Scheduled games against the opponent, each linking to its event (availability, details). */
export const UpcomingGames = memo(function UpcomingGames({ events }: { events: CalendarEvent[] }) {
  if (events.length === 0) return null

  return (
    <section className="flex flex-col gap-2 rounded-md border border-primary-700/60 bg-primary-950/20 p-4">
      <h2 className="text-sm font-medium text-primary-300">Nadchodzące mecze</h2>
      <ul className="flex flex-col gap-1">
        {events.map((event) => (
          <li key={event.id}>
            <Link to={`/calendar?event=${event.id}`} className="text-sm hover:underline">
              {event.title} · {eventTypeLabels[event.type]} ·{' '}
              {dateTimeFormatter.format(new Date(event.startsAtUtc))}
            </Link>
          </li>
        ))}
      </ul>
    </section>
  )
})

/** Head-to-head record per map, most-played first — the input for picks and bans against this team. */
export const MapRecords = memo(function MapRecords({ maps }: { maps: OpponentMapRecord[] }) {
  return (
    <section className="flex flex-col gap-2">
      <h2 className="text-lg font-medium">Mapy</h2>
      {maps.length === 0 ? (
        <p className="text-sm text-neutral-500">Brak meczów z wpisaną mapą.</p>
      ) : (
        <ul className="grid gap-2 sm:grid-cols-2">
          {maps.map((map) => (
            <li
              key={map.mapName}
              className="flex items-center justify-between rounded-md border border-neutral-800 px-3 py-2"
            >
              <span className="text-sm">{map.mapName}</span>
              <RecordBadge wins={map.wins} losses={map.losses} draws={map.draws} />
            </li>
          ))}
        </ul>
      )}
    </section>
  )
})

/** Every logged game against the opponent, newest first, with demo link and post-match notes. */
export const MatchHistory = memo(function MatchHistory({ matches }: { matches: OpponentMatch[] }) {
  return (
    <section className="flex flex-col gap-2">
      <h2 className="text-lg font-medium">Historia meczów</h2>
      {matches.length === 0 && <p className="text-sm text-neutral-500">Jeszcze z nimi nie graliście.</p>}
      <ul className="flex flex-col gap-2">
        {matches.map((match) => {
          const outcomeClass =
            match.ourScore > match.opponentScore
              ? 'text-success-400'
              : match.ourScore < match.opponentScore
                ? 'text-danger-400'
                : 'text-neutral-400'

          return (
            <li key={match.matchResultId} className="rounded-md border border-neutral-800 px-3 py-2">
              <div className="flex flex-wrap items-center justify-between gap-2 text-sm">
                <span>
                  <span className={`font-medium tabular-nums ${outcomeClass}`}>
                    {match.ourScore}:{match.opponentScore}
                  </span>
                  <span className="text-neutral-400"> · {match.mapName ?? 'bez mapy'}</span>
                  <span className="text-neutral-500"> · {matchCategoryLabels[match.category]}</span>
                </span>
                <span className="flex items-center gap-3 text-neutral-500">
                  {match.demoUrl && (
                    <a
                      href={match.demoUrl}
                      target="_blank"
                      rel="noreferrer"
                      className="text-primary-400 hover:underline"
                    >
                      Demka
                    </a>
                  )}
                  {dateFormatter.format(new Date(match.playedAtUtc))}
                </span>
              </div>
              {match.notes && (
                <p className="mt-1 whitespace-pre-wrap text-sm text-neutral-400">{match.notes}</p>
              )}
            </li>
          )
        })}
      </ul>
    </section>
  )
})
