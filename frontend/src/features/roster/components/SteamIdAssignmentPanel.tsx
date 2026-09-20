import { useState } from 'react'
import type { TeamMember } from '../../../services/rosterApi'
import { useAuthStore } from '../../auth/stores/useAuthStore'
import { useRoster, useUpdateSteamId64 } from '../hooks/useRoster'

/** Manager-only panel for setting any team member's SteamID64 on their behalf — for players who haven't set their
 * own yet in Ustawienia, so demo-imported stats can still be matched to them. */
export function SteamIdAssignmentPanel() {
  const role = useAuthStore((state) => state.role)
  const { data: roster } = useRoster()
  const updateSteamId64 = useUpdateSteamId64()
  const [drafts, setDrafts] = useState<Record<string, string>>({})

  if (role !== 'Manager' || !roster || roster.length === 0) {
    return null
  }

  function draftFor(member: TeamMember) {
    return drafts[member.id] ?? member.steamId64 ?? ''
  }

  function save(member: TeamMember) {
    const value = draftFor(member).trim()
    updateSteamId64.mutate({ userId: member.id, steamId64: value === '' ? null : value })
  }

  return (
    <div className="flex w-full max-w-xl flex-col gap-2 rounded-md border border-neutral-800 p-4">
      <div>
        <h2 className="text-sm font-medium text-neutral-200">SteamID64 graczy</h2>
        <p className="text-xs text-neutral-500">
          Możesz ustawić SteamID64 za gracza, jeśli sam jeszcze go nie wpisał — potrzebne do dopasowania statystyk z demek.
        </p>
      </div>
      {updateSteamId64.isError && <p className="text-sm text-red-400">{updateSteamId64.error.message}</p>}
      <ul className="flex flex-col gap-2">
        {roster.map((member) => (
          <li key={member.id} className="flex items-center gap-2">
            <span className="w-32 shrink-0 truncate text-sm text-neutral-300">
              {member.inGameNickname ?? member.displayName}
            </span>
            <input
              inputMode="numeric"
              maxLength={17}
              placeholder="76561198012345678"
              value={draftFor(member)}
              onChange={(event) =>
                setDrafts((current) => ({ ...current, [member.id]: event.target.value.replace(/[^0-9]/g, '') }))
              }
              className="flex-1 rounded-md border border-neutral-800 bg-neutral-900 px-2 py-1 text-xs outline-none focus:border-neutral-500"
            />
            <button
              type="button"
              disabled={updateSteamId64.isPending}
              onClick={() => save(member)}
              className="shrink-0 rounded-md bg-red-600 px-2.5 py-1 text-xs font-medium text-white transition hover:bg-red-500 disabled:opacity-50"
            >
              Zapisz
            </button>
          </li>
        ))}
      </ul>
    </div>
  )
}
