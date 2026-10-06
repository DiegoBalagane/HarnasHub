import { useState, type FormEvent } from 'react'
import { FACEIT_LINK_SOURCE_MAX_LENGTH } from '../../../constants'
import type { OpponentFaceitLink } from '../../../services/opponentReportApi'
import { JobProgress } from '../../jobs/components/JobProgress'
import { useLinkOpponentFaceit } from '../hooks/useOpponentReport'

/** Props of {@link FaceitLinkForm}. */
interface FaceitLinkFormProps {
  opponentName: string
  link: OpponentFaceitLink | null
}

/** Paste box linking the opponent to FACEIT (a team URL, a match room URL or a list of nicknames); linking and the first
 * data sync run as one background job whose progress is shown inline. */
export function FaceitLinkForm({ opponentName, link }: FaceitLinkFormProps) {
  const [source, setSource] = useState('')
  const [isOpen, setIsOpen] = useState(link === null)
  const linkJob = useLinkOpponentFaceit(opponentName)

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault()
    linkJob.start(source.trim())
  }

  // A running link (also one started before the page was left and re-opened) keeps the form open to show its progress.
  if ((!isOpen && !linkJob.isBusy) || linkJob.isSucceeded) {
    return (
      <button
        type="button"
        onClick={() => {
          linkJob.reset()
          setSource('')
          setIsOpen(true)
        }}
        className="self-start text-sm text-neutral-400 hover:text-white"
      >
        Zmień powiązanie z FACEIT
      </button>
    )
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-2 rounded-md border border-neutral-800 p-4">
      <label htmlFor="faceit-source" className="text-sm font-medium">
        Powiąż przeciwnika z FACEIT
      </label>
      <p className="text-xs text-neutral-500">
        Wklej link do drużyny (faceit.com/…/teams/…), link do pokoju meczu, w którym grali (faceit.com/…/room/…), albo
        nicki graczy oddzielone przecinkami.
      </p>
      <textarea
        id="faceit-source"
        value={source}
        onChange={(event) => setSource(event.target.value)}
        rows={2}
        maxLength={FACEIT_LINK_SOURCE_MAX_LENGTH}
        disabled={linkJob.isBusy}
        className="rounded-md border border-neutral-700 bg-neutral-900 px-3 py-2 text-sm"
        placeholder="https://www.faceit.com/pl/teams/… lub nick1, nick2, nick3"
      />
      <JobProgress job={linkJob.job} isStarting={linkJob.isStarting} error={linkJob.error} />
      <div className="flex gap-2">
        <button
          type="submit"
          disabled={source.trim() === '' || linkJob.isBusy}
          className="rounded-md bg-primary-500 px-3 py-1.5 text-sm font-medium text-primary-950 hover:bg-primary-400 disabled:opacity-50"
        >
          {linkJob.isBusy ? 'Powiązywanie…' : 'Powiąż'}
        </button>
        {link && !linkJob.isBusy && (
          <button type="button" onClick={() => setIsOpen(false)} className="text-sm text-neutral-400 hover:text-white">
            Anuluj
          </button>
        )}
      </div>
    </form>
  )
}
