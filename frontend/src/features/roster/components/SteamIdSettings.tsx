import { useEffect, useState } from 'react'
import { useAuthStore } from '../../auth/stores/useAuthStore'
import { useRoster, useUpdateOwnSteamId64 } from '../hooks/useRoster'

/** Lets any roster member record their own SteamID64, used later to match them in imported demo stats. */
export function SteamIdSettings() {
  const userId = useAuthStore((state) => state.userId)
  const { data: roster } = useRoster()
  const updateSteamId64 = useUpdateOwnSteamId64()
  const me = roster?.find((member) => member.id === userId)
  const [draft, setDraft] = useState(me?.steamId64 ?? '')

  // `me` only resolves once the roster query lands, which is after this component's first
  // render — without this, the field stays stuck on the empty initial state forever.
  useEffect(() => {
    setDraft(me?.steamId64 ?? '')
  }, [me?.steamId64])

  if (!me) {
    return null
  }

  function save() {
    updateSteamId64.mutate(draft.trim() === '' ? null : draft.trim())
  }

  return (
    <div className="flex w-full max-w-md flex-col gap-3 rounded-md border border-neutral-800 p-4">
      <div>
        <h2 className="font-medium">SteamID64</h2>
        <p className="text-xs text-neutral-500">
          17-cyfrowy numer Twojego konta Steam — po ustawieniu pozwala automatycznie dopasować Cię do statystyk
          zaimportowanych z demki meczu.
        </p>
      </div>

      <div className="flex items-center gap-2">
        <input
          inputMode="numeric"
          maxLength={17}
          placeholder="76561198012345678"
          value={draft}
          onChange={(event) => setDraft(event.target.value.replace(/[^0-9]/g, ''))}
          className="flex-1 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        />
        <button
          type="button"
          disabled={updateSteamId64.isPending}
          onClick={save}
          className="shrink-0 rounded-md bg-red-600 px-3 py-1.5 text-xs font-medium text-white transition hover:bg-red-500 disabled:opacity-50"
        >
          Zapisz
        </button>
        {me.steamId64 && (
          <button
            type="button"
            disabled={updateSteamId64.isPending}
            onClick={() => {
              setDraft('')
              updateSteamId64.mutate(null)
            }}
            className="shrink-0 rounded-md border border-neutral-700 px-3 py-1.5 text-xs text-neutral-300 transition hover:border-neutral-500 disabled:opacity-50"
          >
            Usuń
          </button>
        )}
      </div>

      {updateSteamId64.isError && <p className="text-sm text-red-400">{updateSteamId64.error.message}</p>}
    </div>
  )
}
