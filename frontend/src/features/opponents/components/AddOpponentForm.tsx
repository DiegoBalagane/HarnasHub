import { useEffect, useRef, useState, type ChangeEvent, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { FACEIT_LINK_SOURCE_MAX_LENGTH } from '../../../constants'
import { useModalGuard } from '../../../components/ModalGuardContext'
import { JobProgress } from '../../jobs/components/JobProgress'
import { DemoUploadRow } from '../../opponentReport/components/DemoUploadRow'
import { useUploadOpponentDemos } from '../../opponentReport/hooks/useOpponentDemos'
import { useLinkOpponentFaceit } from '../../opponentReport/hooks/useOpponentReport'
import { opponentReportPath } from '../paths'
import { useOpponents } from '../hooks/useOpponents'
import { OpponentNameInput } from './OpponentNameInput'

const inputClass =
  'rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500'

/** Props of {@link AddOpponentForm}. */
interface AddOpponentFormProps {
  /** Called when the user follows the "already exists" link, so the hosting modal can close. */
  onDone?: () => void
}

/** Coach/Manager-only form creating an opponent straight from its FACEIT source (team/room URL or nicknames) and/or
 * demo files: the link job and the demo uploads start with the typed name, then the user lands on that opponent's
 * report where the (slotted) link job progress keeps running. An opponent exists once it has data, so at least one of
 * the two is required. */
export function AddOpponentForm({ onDone }: AddOpponentFormProps) {
  const navigate = useNavigate()
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [name, setName] = useState('')
  const [source, setSource] = useState('')
  const [files, setFiles] = useState<File[]>([])
  const [submitted, setSubmitted] = useState(false)
  const { data: opponents } = useOpponents()
  const linkJob = useLinkOpponentFaceit(name)
  const uploads = useUploadOpponentDemos(name)

  const trimmedName = name.trim()
  const trimmedSource = source.trim()
  const existing = opponents?.find((o) => o.name.trim().toLowerCase() === trimmedName.toLowerCase())
  const wantsLink = trimmedSource !== ''
  const wantsDemos = files.length > 0

  // Link: accepted once its job exists (it then keeps running in the background slot); demos: uploaded and analysed.
  const linkSettled = !wantsLink || linkJob.job !== undefined || Boolean(linkJob.error)
  const demosSettled =
    !wantsDemos ||
    (!uploads.busy &&
      uploads.items.length > 0 &&
      uploads.items.every((item) => item.status === 'done' || item.status === 'error'))
  const hasProblem = Boolean(linkJob.error) || uploads.items.some((item) => item.status === 'error')
  const isFinished = submitted && linkSettled && demosSettled

  useModalGuard({
    isBusy: submitted && !isFinished,
    isDirty: !submitted && (trimmedName !== '' || trimmedSource !== '' || wantsDemos),
  })

  useEffect(() => {
    if (isFinished && !hasProblem) navigate(opponentReportPath(trimmedName))
  }, [isFinished, hasProblem, navigate, trimmedName])

  function handleFiles(event: ChangeEvent<HTMLInputElement>) {
    const picked = Array.from(event.target.files ?? [])
    event.target.value = ''
    setFiles((current) => [
      ...current,
      ...picked.filter((file) => !current.some((c) => c.name === file.name && c.size === file.size)),
    ])
  }

  function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSubmitted(true)
    if (wantsLink) linkJob.start(trimmedSource)
    if (wantsDemos) void uploads.upload(files)
  }

  const canSubmit = trimmedName !== '' && (wantsLink || wantsDemos) && !submitted

  return (
    <form onSubmit={handleSubmit} className="flex w-full flex-col gap-4">
      <div className="flex flex-col gap-1">
        <label htmlFor="new-opponent-name" className="text-sm font-medium">
          Nazwa przeciwnika
        </label>
        <OpponentNameInput
          id="new-opponent-name"
          required
          value={name}
          onChange={setName}
          className={inputClass}
          disabled={submitted}
        />
        {existing && !submitted && (
          <p className="text-xs text-warning-300">
            Przeciwnik „{existing.name}" już istnieje — dane zostaną do niego dodane.{' '}
            <Link to={opponentReportPath(existing.name)} onClick={onDone} className="underline hover:text-white">
              Przejdź do raportu
            </Link>
          </p>
        )}
      </div>

      <div className="flex flex-col gap-1">
        <label htmlFor="new-opponent-faceit" className="text-sm font-medium">
          FACEIT (opcjonalnie)
        </label>
        <textarea
          id="new-opponent-faceit"
          value={source}
          onChange={(event) => setSource(event.target.value)}
          rows={2}
          maxLength={FACEIT_LINK_SOURCE_MAX_LENGTH}
          disabled={submitted}
          className={inputClass}
          placeholder="Link do drużyny, link do pokoju meczu albo nicki: nick1, nick2, nick3"
        />
      </div>

      <div className="flex flex-col gap-2">
        <span className="text-sm font-medium">Demki (opcjonalnie)</span>
        <input
          ref={fileInputRef}
          aria-label="Pliki demek"
          type="file"
          accept=".dem"
          multiple
          className="hidden"
          onChange={handleFiles}
        />
        {!submitted && (
          <button
            type="button"
            onClick={() => fileInputRef.current?.click()}
            className="self-start rounded-md border border-neutral-800 px-3 py-1.5 text-sm text-primary-400 hover:border-neutral-600"
          >
            Wybierz pliki .dem
          </button>
        )}
        {!submitted && files.length > 0 && (
          <ul className="flex flex-col gap-1 text-sm">
            {files.map((file) => (
              <li
                key={`${file.name}:${file.size}`}
                className="flex justify-between gap-3 rounded-md bg-neutral-900 px-3 py-1.5"
              >
                <span className="truncate">{file.name}</span>
                <button
                  type="button"
                  aria-label={`Usuń ${file.name}`}
                  onClick={() => setFiles((current) => current.filter((f) => f !== file))}
                  className="text-neutral-400 hover:text-white"
                >
                  ×
                </button>
              </li>
            ))}
          </ul>
        )}
        {submitted && uploads.items.length > 0 && (
          <ul className="flex flex-col gap-1 text-sm">
            {uploads.items.map((item) => (
              <DemoUploadRow key={item.id} item={item} onDone={uploads.complete} onFailed={uploads.fail} />
            ))}
          </ul>
        )}
      </div>

      {submitted && wantsLink && (
        <JobProgress job={linkJob.job} isStarting={linkJob.isStarting} error={linkJob.error} />
      )}

      {!submitted && !wantsLink && !wantsDemos && (
        <p className="text-xs text-neutral-500">
          Podaj FACEIT lub dodaj demkę — przeciwnik pojawia się na liście, gdy ma jakieś dane. Samą notatkę dodasz z
          profilu przeciwnika.
        </p>
      )}

      <div className="flex gap-3">
        <button
          type="submit"
          disabled={!canSubmit}
          className="rounded-md bg-primary-500 px-4 py-2 text-sm font-medium text-primary-950 transition hover:bg-primary-400 disabled:opacity-50"
        >
          {submitted && !isFinished ? 'Dodawanie…' : 'Dodaj'}
        </button>
        {isFinished && hasProblem && (
          <button
            type="button"
            onClick={() => navigate(opponentReportPath(trimmedName))}
            className="rounded-md border border-neutral-700 px-4 py-2 text-sm hover:border-neutral-500"
          >
            Przejdź do raportu
          </button>
        )}
      </div>
    </form>
  )
}
