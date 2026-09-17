import { useEffect, useState } from 'react'
import { useAuthStore } from '../../auth/stores/useAuthStore'
import { useRoster, useUpdateOwnPinMark } from '../hooks/useRoster'

/** Lets any roster member pick a single character shown on their map-radar pin instead of auto-generated initials. */
export function PinMarkSettings() {
  const userId = useAuthStore((state) => state.userId)
  const { data: roster } = useRoster()
  const updatePinMark = useUpdateOwnPinMark()
  const me = roster?.find((member) => member.id === userId)
  const [mark, setMark] = useState(me?.pinMark ?? '')

  // `me` only resolves once the roster query lands, which is after this component's first
  // render — without this, the field stays stuck on the empty initial state forever.
  useEffect(() => {
    setMark(me?.pinMark ?? '')
  }, [me?.pinMark])

  if (!me) {
    return null
  }

  // Counts grapheme clusters rather than raw characters, so a single emoji (which can span a
  // surrogate pair plus modifiers) still counts as "one", matching the backend's own check.
  function firstGrapheme(value: string): string {
    const [first] = new Intl.Segmenter(undefined, { granularity: 'grapheme' }).segment(value)
    return first?.segment ?? ''
  }

  function save() {
    updatePinMark.mutate(mark.trim() === '' ? null : mark.trim())
  }

  return (
    <div className="flex w-full max-w-md flex-col gap-3 rounded-md border border-neutral-800 p-4">
      <div>
        <h2 className="font-medium">Znak na pinezce</h2>
        <p className="text-xs text-neutral-500">
          Pojedynczy znak (cyfra, litera lub emoji) pokazywany na Twojej pinezce zamiast inicjałów.
        </p>
      </div>

      <div className="flex items-center gap-2">
        <input
          maxLength={8}
          placeholder="np. 7"
          value={mark}
          onChange={(event) => setMark(firstGrapheme(event.target.value))}
          className="w-20 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-center text-sm outline-none focus:border-neutral-500"
        />
        <button
          type="button"
          disabled={updatePinMark.isPending}
          onClick={save}
          className="rounded-md bg-red-600 px-3 py-1.5 text-xs font-medium text-white transition hover:bg-red-500 disabled:opacity-50"
        >
          Zapisz
        </button>
        {me.pinMark && (
          <button
            type="button"
            disabled={updatePinMark.isPending}
            onClick={() => {
              setMark('')
              updatePinMark.mutate(null)
            }}
            className="rounded-md border border-neutral-700 px-3 py-1.5 text-xs text-neutral-300 transition hover:border-neutral-500 disabled:opacity-50"
          >
            Usuń
          </button>
        )}
      </div>

      {updatePinMark.isError && <p className="text-sm text-red-400">{updatePinMark.error.message}</p>}
    </div>
  )
}
