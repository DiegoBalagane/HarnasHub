import { useState, type FormEvent } from 'react'
import { useAuthStore } from '../../auth/stores/useAuthStore'
import { useUpdateOwnNickname } from '../hooks/useRoster'

/** Top-bar control showing the caller's in-game nickname and letting them change it inline. */
export function NicknameEditor() {
  const displayName = useAuthStore((state) => state.displayName)
  const inGameNickname = useAuthStore((state) => state.inGameNickname)
  const updateNickname = useUpdateOwnNickname()
  const [isOpen, setIsOpen] = useState(false)
  const [draft, setDraft] = useState('')

  function open() {
    setDraft(inGameNickname ?? '')
    setIsOpen(true)
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()

    try {
      await updateNickname.mutateAsync(draft.trim())
      setIsOpen(false)
    } catch {
      // The error message is rendered from the mutation state below.
    }
  }

  return (
    <div className="relative">
      <button
        onClick={() => (isOpen ? setIsOpen(false) : open())}
        className="text-neutral-400 hover:text-white"
        title={inGameNickname ? `Discord: ${displayName}` : 'Ustaw swój nick w grze'}
      >
        {inGameNickname ?? displayName}
      </button>

      {isOpen && (
        <form
          onSubmit={handleSubmit}
          className="absolute right-0 z-10 mt-2 flex w-64 flex-col gap-2 rounded-md border border-neutral-800 bg-neutral-950 p-3 shadow-lg"
        >
          <label htmlFor="in-game-nickname" className="text-xs text-neutral-400">
            Nick w grze
          </label>
          <input
            id="in-game-nickname"
            value={draft}
            onChange={(event) => setDraft(event.target.value)}
            maxLength={32}
            autoFocus
            className="rounded-md border border-neutral-800 bg-neutral-900 px-2 py-1 text-sm outline-none focus:border-neutral-500"
          />

          {updateNickname.isError && <p className="text-xs text-red-400">{updateNickname.error.message}</p>}

          <div className="flex justify-end gap-2">
            <button
              type="button"
              onClick={() => setIsOpen(false)}
              className="text-xs text-neutral-400 hover:text-white"
            >
              Anuluj
            </button>
            <button
              type="submit"
              disabled={updateNickname.isPending}
              className="rounded-md bg-red-600 px-2 py-1 text-xs font-medium text-white disabled:opacity-50"
            >
              Zapisz
            </button>
          </div>
        </form>
      )}
    </div>
  )
}
