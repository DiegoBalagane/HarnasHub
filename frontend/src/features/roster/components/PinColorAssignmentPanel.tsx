import type { TeamMember } from '../../../services/rosterApi'
import { useAuthStore } from '../../auth/stores/useAuthStore'
import { useRoster, useUpdatePinColor } from '../hooks/useRoster'
import { PinColorPicker } from './PinColorPicker'

/** Manager-only panel for assigning map-radar pin colours to the 5 Main-roster players, kept out of the roster rows so it doesn't crowd out their names. */
export function PinColorAssignmentPanel() {
  const role = useAuthStore((state) => state.role)
  const { data: roster } = useRoster()
  const updatePinColor = useUpdatePinColor()
  const mainRoster = (roster ?? []).filter((member) => member.rosterSlot === 'Main')

  if (role !== 'Manager' || mainRoster.length === 0) {
    return null
  }

  return (
    <div className="flex w-full max-w-xl flex-col gap-2 rounded-md border border-neutral-800 p-4">
      <h2 className="text-sm font-medium text-neutral-200">Kolory pinezek (główny skład)</h2>
      {updatePinColor.isError && <p className="text-sm text-red-400">{updatePinColor.error.message}</p>}
      <ul className="flex flex-col gap-2">
        {mainRoster.map((member: TeamMember) => (
          <li key={member.id} className="flex items-center justify-between gap-3">
            <span className="truncate text-sm text-neutral-300">{member.inGameNickname ?? member.displayName}</span>
            <PinColorPicker
              value={member.pinColor}
              onChange={(pinColor) => updatePinColor.mutate({ userId: member.id, pinColor })}
              disabled={updatePinColor.isPending}
              size="sm"
            />
          </li>
        ))}
      </ul>
    </div>
  )
}
