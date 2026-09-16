import type { PinColor } from '../../../services/rosterApi'
import { pinColorLabels, pinColorSwatch, pinColors } from '../labels'

interface PinColorPickerProps {
  value: PinColor | null
  onChange: (color: PinColor | null) => void
  disabled?: boolean
  size?: 'sm' | 'md'
}

/** Renders the pin-colour swatches plus a clear button; shared by the self-service settings and the Manager's per-member picker. */
export function PinColorPicker({ value, onChange, disabled, size = 'md' }: PinColorPickerProps) {
  const dimension = size === 'sm' ? 'h-5 w-5' : 'h-8 w-8'

  return (
    <div className="flex items-center gap-1.5">
      {pinColors.map((color) => (
        <button
          key={color}
          type="button"
          title={pinColorLabels[color]}
          aria-label={pinColorLabels[color]}
          disabled={disabled}
          onClick={() => onChange(color)}
          className={`${dimension} rounded-full ${pinColorSwatch[color]} transition disabled:opacity-50 ${
            value === color ? 'ring-2 ring-white ring-offset-2 ring-offset-neutral-950' : ''
          }`}
        />
      ))}

      {value && (
        <button
          type="button"
          title="Usuń kolor"
          aria-label="Usuń kolor"
          disabled={disabled}
          onClick={() => onChange(null)}
          className="text-xs text-neutral-500 transition hover:text-neutral-300 disabled:opacity-50"
        >
          ✕
        </button>
      )}
    </div>
  )
}
