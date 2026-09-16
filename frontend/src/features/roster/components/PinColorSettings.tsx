import { useAuthStore } from '../../auth/stores/useAuthStore'
import { useRoster, useUpdateOwnPinColor } from '../hooks/useRoster'
import { PinColorPicker } from './PinColorPicker'

/** Lets a Main-roster player pick their map-radar pin colour; renders nothing for anyone else. */
export function PinColorSettings() {
  const userId = useAuthStore((state) => state.userId)
  const { data: roster } = useRoster()
  const updatePinColor = useUpdateOwnPinColor()
  const me = roster?.find((member) => member.id === userId)

  if (!me || me.rosterSlot !== 'Main') {
    return null
  }

  return (
    <div className="flex w-full max-w-md flex-col gap-3 rounded-md border border-neutral-800 p-4">
      <div>
        <h2 className="font-medium">Kolor pinezki na mapie</h2>
        <p className="text-xs text-neutral-500">
          Twój kolor na radarze pozycji — widoczny tylko dla głównego składu.
        </p>
      </div>

      <PinColorPicker
        value={me.pinColor}
        onChange={(color) => updatePinColor.mutate(color)}
        disabled={updatePinColor.isPending}
      />

      {updatePinColor.isError && <p className="text-sm text-red-400">{updatePinColor.error.message}</p>}
    </div>
  )
}
