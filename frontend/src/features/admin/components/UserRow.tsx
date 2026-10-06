import { memo, useState } from 'react'
import { Button } from '../../../components/ui/Button'
import type { AccessLevel, RosterSlot, TeamMember } from '../../../services/rosterApi'
import {
  useSetIsCoach,
  useUpdateRole,
  useUpdateRosterSlot,
  useUpdateSteamId64,
} from '../../roster/hooks/useRoster'
import {
  accessLevelDescriptions,
  accessLevelLabels,
  accessLevels,
  isCoachDescription,
  isCoachLabel,
  rosterSlotLabels,
  rosterSlots,
} from '../../roster/labels'
import { isValidSteamId64 } from '../steamId'

/** Grid template shared by the table header and every row so columns stay aligned. */
export const userGridColumns = 'grid-cols-[minmax(160px,1fr)_130px_90px_140px_minmax(240px,300px)_90px]'

const selectClass =
  'w-full rounded-md border border-neutral-800 bg-neutral-900 px-2 py-1 text-sm outline-none focus:border-neutral-500'

interface UserRowProps {
  member: TeamMember
  isSelf: boolean
  onDelete: (member: TeamMember) => void
}

/** One account in the admin table: access level, Coach tag, roster slot, SteamID64 (explicit save) and delete. */
export const UserRow = memo(function UserRow({ member, isSelf, onDelete }: UserRowProps) {
  const updateRole = useUpdateRole()
  const setIsCoach = useSetIsCoach()
  const updateSlot = useUpdateRosterSlot()
  const errors = [updateRole, setIsCoach, updateSlot].filter((m) => m.isError).map((m) => m.error?.message)
  const primaryName = member.inGameNickname ?? member.displayName
  const showsDiscordName = member.inGameNickname !== null && member.inGameNickname !== member.displayName

  return (
    <li className="px-4 py-3">
      <div className={`grid items-center ${userGridColumns} gap-x-3 gap-y-1`}>
        <span className="flex min-w-0 items-center gap-2">
          {member.avatarUrl && <img src={member.avatarUrl} alt="" className="h-6 w-6 shrink-0 rounded-full" />}
          <span className="flex min-w-0 flex-col">
            <span className="truncate font-medium" title={primaryName}>
              {primaryName}
            </span>
            {showsDiscordName && (
              <span className="truncate text-xs text-neutral-500" title="Nazwa z Discorda">
                {member.displayName}
              </span>
            )}
          </span>
        </span>

        {isSelf ? (
          <span title={accessLevelDescriptions[member.role]} className="text-sm text-neutral-400">
            {accessLevelLabels[member.role]}
          </span>
        ) : (
          <select
            value={member.role}
            disabled={updateRole.isPending}
            aria-label={`Uprawnienia: ${primaryName}`}
            title={accessLevelDescriptions[member.role]}
            onChange={(event) => updateRole.mutate({ userId: member.id, role: event.target.value as AccessLevel })}
            className={selectClass}
          >
            {accessLevels.map((level) => (
              <option key={level} value={level}>
                {accessLevelLabels[level]}
              </option>
            ))}
          </select>
        )}

        <label
          title={member.role === 'Guest' ? 'Najpierw nadaj poziom uprawnień' : isCoachDescription}
          className="flex items-center gap-1 text-xs text-neutral-300"
        >
          <input
            type="checkbox"
            checked={member.isCoach}
            disabled={setIsCoach.isPending || member.role === 'Guest'}
            aria-label={`${isCoachLabel}: ${primaryName}`}
            onChange={(event) => setIsCoach.mutate({ userId: member.id, isCoach: event.target.checked })}
          />
          {isCoachLabel}
        </label>

        <select
          value={member.rosterSlot ?? ''}
          disabled={updateSlot.isPending}
          aria-label={`Status w składzie: ${primaryName}`}
          onChange={(event) =>
            updateSlot.mutate({
              userId: member.id,
              rosterSlot: event.target.value === '' ? null : (event.target.value as RosterSlot),
            })
          }
          className={selectClass}
        >
          <option value="">Nieprzypisany</option>
          {rosterSlots.map((slot) => (
            <option key={slot} value={slot}>
              {rosterSlotLabels[slot]}
            </option>
          ))}
        </select>

        <SteamIdField member={member} />

        <div className="flex justify-end">
          {!isSelf && (
            <button
              type="button"
              onClick={() => onDelete(member)}
              className="shrink-0 rounded-md border border-neutral-700 px-2 py-1 text-xs text-neutral-400 transition hover:border-danger-500 hover:text-danger-400"
            >
              Usuń konto
            </button>
          )}
        </div>
      </div>
      {errors.map((message) => (
        <p key={message} className="pt-1 text-xs text-danger-400">
          {message}
        </p>
      ))}
    </li>
  )
})

/** SteamID64 input with live validation and an explicit per-row save button; shows saved/error state beside it. */
function SteamIdField({ member }: { member: TeamMember }) {
  const updateSteamId64 = useUpdateSteamId64()
  const [draft, setDraft] = useState<string | null>(null)
  const value = draft ?? member.steamId64 ?? ''
  const isValid = isValidSteamId64(value)
  const isDirty = value.trim() !== (member.steamId64 ?? '')

  function save() {
    const trimmed = value.trim()
    updateSteamId64.mutate(
      { userId: member.id, steamId64: trimmed === '' ? null : trimmed },
      { onSuccess: () => setDraft(null) },
    )
  }

  return (
    <div className="flex min-w-0 flex-col gap-0.5">
      <div className="flex items-center gap-2">
        <input
          inputMode="numeric"
          maxLength={17}
          placeholder="76561198012345678"
          value={value}
          aria-label={`SteamID64: ${member.inGameNickname ?? member.displayName}`}
          aria-invalid={!isValid}
          onChange={(event) => {
            updateSteamId64.reset()
            setDraft(event.target.value.replace(/[^0-9]/g, ''))
          }}
          className={`min-w-0 flex-1 rounded-md border bg-neutral-900 px-2 py-1 text-xs outline-none focus:border-neutral-500 ${
            isValid ? 'border-neutral-800' : 'border-danger-500'
          }`}
        />
        <Button size="sm" disabled={!isDirty || !isValid || updateSteamId64.isPending} onClick={save}>
          Zapisz
        </Button>
      </div>
      {!isValid && <span className="text-xs text-danger-400">17 cyfr, zaczyna się od 7656119</span>}
      {updateSteamId64.isError && <span className="text-xs text-danger-400">{updateSteamId64.error.message}</span>}
      {updateSteamId64.isSuccess && <span className="text-xs text-success-400">Zapisano</span>}
    </div>
  )
}
