import { useState } from 'react'
import { useAuthStore } from '../../auth/stores/useAuthStore'
import { useUpdateOwnNickname } from '../hooks/useRoster'

const inputClass =
  'rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500'

/** Settings form for the caller's in-game nickname: set/change it, or clear it back to the Discord name. */
export function NicknameSettings() {
  const displayName = useAuthStore((state) => state.displayName)
  const inGameNickname = useAuthStore((state) => state.inGameNickname)
  const updateNickname = useUpdateOwnNickname()
  const [draft, setDraft] = useState(inGameNickname ?? '')

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    updateNickname.mutate(draft.trim() === '' ? null : draft.trim())
  }

  function handleClear() {
    setDraft('')
    updateNickname.mutate(null)
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="flex w-full max-w-md flex-col gap-3 rounded-md border border-neutral-800 p-4"
    >
      <div>
        <h2 className="font-medium">Nick w grze</h2>
        <p className="text-xs text-neutral-500">
          Ten nick wyświetla się w kalendarzu, składzie i na mapach zamiast Twojej nazwy z Discorda (
          {displayName}).
        </p>
      </div>

      <input
        maxLength={32}
        placeholder="np. s1mple"
        value={draft}
        onChange={(event) => setDraft(event.target.value)}
        className={inputClass}
      />

      {updateNickname.isError && <p className="text-sm text-red-400">{updateNickname.error.message}</p>}

      <div className="flex gap-2">
        <button
          type="submit"
          disabled={updateNickname.isPending}
          className="rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-red-500 disabled:opacity-50"
        >
          Zapisz
        </button>
        {inGameNickname && (
          <button
            type="button"
            onClick={handleClear}
            disabled={updateNickname.isPending}
            className="rounded-md border border-neutral-700 px-4 py-2 text-sm text-neutral-300 transition hover:border-neutral-500 disabled:opacity-50"
          >
            Usuń nick
          </button>
        )}
      </div>
    </form>
  )
}
