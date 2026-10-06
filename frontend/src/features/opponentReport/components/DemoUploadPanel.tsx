import { memo, useRef, useState } from 'react'
import { OPPONENT_DEMO_DOWNLOAD } from '../../../constants'
import type { MapName } from '../../../services/nadesApi'
import { JobProgress } from '../../jobs/components/JobProgress'
import { mapNames } from '../../nades/labels'
import { useFaceitDemoDownload, useUploadOpponentDemos } from '../hooks/useOpponentDemos'
import { DemoUploadRow } from './DemoUploadRow'

interface DemoUploadPanelProps {
  opponentName: string
  /** Whether the FACEIT Downloads API path is available (token, API key and link all present). */
  autoDownloadAvailable: boolean
}

/** Multi-file .dem upload with per-file upload and analysis progress, plus the automatic FACEIT download (a background
 * job with its own progress) when it is configured. */
export const DemoUploadPanel = memo(function DemoUploadPanel({
  opponentName,
  autoDownloadAvailable,
}: DemoUploadPanelProps) {
  const inputRef = useRef<HTMLInputElement>(null)
  const { items, busy, upload, complete, fail } = useUploadOpponentDemos(opponentName)
  const download = useFaceitDemoDownload(opponentName)
  const [maps, setMaps] = useState<MapName[]>([])
  const [count, setCount] = useState<number>(OPPONENT_DEMO_DOWNLOAD.defaultCount)

  function handleFiles(event: React.ChangeEvent<HTMLInputElement>) {
    const files = Array.from(event.target.files ?? [])
    event.target.value = ''
    if (files.length > 0) void upload(files)
  }

  function toggleMap(map: MapName) {
    setMaps((current) => (current.includes(map) ? current.filter((m) => m !== map) : [...current, map]))
  }

  return (
    <div className="flex flex-col gap-3">
      <div className="flex flex-wrap items-center gap-3">
        <input ref={inputRef} type="file" accept=".dem" multiple className="hidden" onChange={handleFiles} />
        <button
          type="button"
          disabled={busy}
          onClick={() => inputRef.current?.click()}
          className="rounded-md border border-neutral-800 px-3 py-1.5 text-sm text-primary-400 transition hover:border-neutral-600 disabled:opacity-50"
        >
          {busy ? 'Trwa wgrywanie…' : 'Wgraj demki ręcznie'}
        </button>
        <span className="text-xs text-neutral-500">
          Ich mecze z FACEIT albo nasze mecze przeciw nim — można wybrać kilka plików. Oryginalna nazwa pliku z FACEIT
          pozwala rozpoznać mecz i stronę rywala.
        </span>
      </div>

      {items.length > 0 && (
        <ul className="flex flex-col gap-1 text-sm">
          {items.map((item) => (
            <DemoUploadRow key={item.id} item={item} onDone={complete} onFailed={fail} />
          ))}
        </ul>
      )}

      {autoDownloadAvailable && (
        <div className="flex flex-col gap-2 rounded-md border border-neutral-800 p-3">
          <span className="text-sm font-medium">Pobierz z FACEIT (ostatnie mecze drużynowe)</span>
          <div className="flex flex-wrap gap-2">
            {mapNames.map((map) => (
              <label key={map} className="flex items-center gap-1 text-xs text-neutral-300">
                <input type="checkbox" checked={maps.includes(map)} onChange={() => toggleMap(map)} />
                {map}
              </label>
            ))}
          </div>
          <div className="flex items-center gap-2 text-sm">
            <label className="text-neutral-400" htmlFor="faceit-demo-count">
              Liczba demek
            </label>
            <input
              id="faceit-demo-count"
              type="number"
              min={1}
              max={OPPONENT_DEMO_DOWNLOAD.maxCount}
              value={count}
              onChange={(event) => setCount(Number(event.target.value))}
              className="w-16 rounded-md border border-neutral-800 bg-neutral-950 px-2 py-1"
            />
            <button
              type="button"
              disabled={download.isBusy}
              onClick={() => download.start({ maps, count })}
              className="rounded-md border border-neutral-800 px-3 py-1 text-primary-400 hover:border-neutral-600 disabled:opacity-50"
            >
              {download.isBusy ? 'Pobieranie i analiza…' : 'Pobierz automatycznie'}
            </button>
          </div>
          <JobProgress job={download.job} isStarting={download.isStarting} error={download.error} />
          {download.isSucceeded && download.result && (
            <p className="text-sm text-neutral-400">
              Przeanalizowano {download.result.analysed} z {download.result.candidates} meczów
              {download.result.failed > 0 && ` (${download.result.failed} bez demki lub z błędem)`}.
            </p>
          )}
        </div>
      )}
    </div>
  )
})
