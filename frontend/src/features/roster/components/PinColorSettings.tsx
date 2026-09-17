import { useAuthStore } from '../../auth/stores/useAuthStore'
import { useRoster, useUpdateOwnPinColor } from '../hooks/useRoster'
import { PinColorPicker } from './PinColorPicker'

/** Lets a Main-roster player pick their map-radar pin colour; shown to everyone, but locked for non-Main members. */
export function PinColorSettings() {
  const userId = useAuthStore((state) => state.userId)
  const { data: roster } = useRoster()
  const updatePinColor = useUpdateOwnPinColor()
  const me = roster?.find((member) => member.id === userId)

  if (!me) {
    return null
  }

  const isMain = me.rosterSlot === 'Main'

  return (
    <div className="flex w-full max-w-md flex-col gap-3 rounded-md border border-neutral-800 p-4">
      <div>
        <h2 className="font-medium">Kolor pinezki na mapie</h2>
        <p className="text-xs text-neutral-500">
          {isMain
            ? 'Twój kolor na radarze pozycji — widoczny tylko dla głównego składu.'
            : 'Dostępne tylko dla głównego składu.'}
        </p>
      </div>

      <PinColorPicker
        value={me.pinColor}
        onChange={(color) => updatePinColor.mutate(color)}
        disabled={!isMain || updatePinColor.isPending}
      />

      {updatePinColor.isError && <p className="text-sm text-red-400">{updatePinColor.error.message}</p>}
    </div>
  )
}
