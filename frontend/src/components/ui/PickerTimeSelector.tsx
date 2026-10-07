import { useState } from 'react'
import { parseTimeText } from './dateTime'

const hours = Array.from({ length: 24 }, (_, hour) => hour)
const pad = (value: number) => String(value).padStart(2, '0')

interface PickerTimeSelectorProps {
  /** `HH:mm` draft value. */
  value: string
  /** Minute granularity of the minute list (typing accepts any minute). */
  step: number
  onChange: (time: string) => void
}

/** Time part of the picker popover: a typeable `HH:mm` field plus hour and minute lists. */
export function PickerTimeSelector({ value, step, onChange }: PickerTimeSelectorProps) {
  const [typed, setTyped] = useState<{ text: string; base: string } | null>(null)
  // Typed text only wins while the draft still is the value it was typed against; list picks and shortcuts replace it.
  const text = typed && typed.base === value ? typed.text : value
  const [hour, minute] = value ? value.split(':').map(Number) : [-1, -1]
  const minutes = Array.from({ length: Math.ceil(60 / step) }, (_, index) => index * step)

  function commitText() {
    const parsed = parseTimeText(text)
    setTyped(null)
    if (parsed) onChange(parsed)
  }

  function pick(nextHour: number, nextMinute: number) {
    onChange(`${pad(nextHour)}:${pad(nextMinute)}`)
  }

  const optionClass = (active: boolean) =>
    `rounded px-2 py-1 text-sm ${active ? 'bg-primary-500 font-semibold text-primary-950' : 'text-neutral-200 hover:bg-neutral-800'}`

  return (
    <div className="flex flex-col gap-2">
      <label className="flex items-center gap-2 text-xs text-neutral-400">
        Godzina
        <input
          inputMode="numeric"
          aria-label="Godzina (HH:mm)"
          value={text}
          placeholder="HH:mm"
          onChange={(event) => setTyped({ text: event.target.value, base: value })}
          onBlur={commitText}
          onKeyDown={(event) => {
            if (event.key === 'Enter') {
              event.preventDefault()
              commitText()
            }
          }}
          className="w-24 rounded-md border border-neutral-800 bg-neutral-900 px-2 py-1 text-sm text-neutral-100 outline-none focus:border-neutral-500"
        />
      </label>
      <div className="flex gap-2">
        <div role="listbox" aria-label="Godziny" className="grid max-h-28 flex-1 grid-cols-6 gap-0.5 overflow-y-auto">
          {hours.map((option) => (
            <button
              key={option}
              type="button"
              role="option"
              aria-selected={option === hour}
              onClick={() => pick(option, Math.max(minute, 0))}
              className={optionClass(option === hour)}
            >
              {pad(option)}
            </button>
          ))}
        </div>
        <div role="listbox" aria-label="Minuty" className="grid max-h-28 w-24 grid-cols-3 gap-0.5 overflow-y-auto">
          {minutes.map((option) => (
            <button
              key={option}
              type="button"
              role="option"
              aria-selected={option === minute}
              onClick={() => pick(Math.max(hour, 0), option)}
              className={optionClass(option === minute)}
            >
              {pad(option)}
            </button>
          ))}
        </div>
      </div>
    </div>
  )
}
