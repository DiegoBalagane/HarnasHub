import { useAuthStore } from '../../auth/stores/useAuthStore'
import { useRoster, useUpdateOwnPinColor } from '../hooks/useRoster'
import { pinColorLabels, pinColorSwatch, pinColors } from '../labels'

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

      <div className="flex gap-2">
        {pinColors.map((color) => (
          <button
            key={color}
            type="button"
            title={pinColorLabels[color]}
            aria-label={pinColorLabels[color]}
            disabled={updatePinColor.isPending}
            onClick={() => updatePinColor.mutate(color)}
            className={`h-8 w-8 rounded-full ${pinColorSwatch[color]} transition disabled:opacity-50 ${
              me.pinColor === color ? 'ring-2 ring-white ring-offset-2 ring-offset-neutral-950' : ''
            }`}
          />
        ))}
      </div>

      {updatePinColor.isError && <p className="text-sm text-red-400">{updatePinColor.error.message}</p>}

      {me.pinColor && (
        <button
          type="button"
          disabled={updatePinColor.isPending}
          onClick={() => updatePinColor.mutate(null)}
          className="self-start rounded-md border border-neutral-700 px-3 py-1.5 text-xs text-neutral-300 transition hover:border-neutral-500 disabled:opacity-50"
        >
          Usuń kolor
        </button>
      )}
    </div>
  )
}
