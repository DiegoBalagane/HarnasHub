import { useState } from 'react'
import type { MapName } from '../../../services/nadesApi'
import { useNades } from '../../nades/hooks/useNades'
import { grenadeTypeLabels } from '../../nades/labels'

interface TacticPointEditorProps {
  displayNumber: number
  mapName: MapName
  description: string
  nadeEntryId: string | null
  canEdit: boolean
  onChange: (changes: { description: string | null; nadeEntryId: string | null }) => void
  onRemove: () => void
  onClose: () => void
}

/** Inline editor for one numbered radar point — a short instruction plus an optional link to a saved nade lineup. */
export function TacticPointEditor({
  displayNumber,
  mapName,
  description,
  nadeEntryId,
  canEdit,
  onChange,
  onRemove,
  onClose,
}: TacticPointEditorProps) {
  const [text, setText] = useState(description)
  const [selectedNadeId, setSelectedNadeId] = useState(nadeEntryId ?? '')
  const { data: nades } = useNades({ mapName })

  function handleSave() {
    onChange({ description: text.trim() === '' ? null : text.trim(), nadeEntryId: selectedNadeId || null })
    onClose()
  }

  return (
    <div className="flex flex-col gap-2 rounded-md border border-neutral-700 bg-neutral-950 p-3">
      <div className="flex items-center justify-between">
        <p className="text-xs text-neutral-400">Punkt {displayNumber}</p>
        <button type="button" onClick={onClose} className="text-xs text-neutral-500 hover:text-neutral-300">
          Zamknij
        </button>
      </div>

      <textarea
        maxLength={300}
        rows={2}
        readOnly={!canEdit}
        placeholder="Co robić w tym miejscu…"
        value={text}
        onChange={(event) => setText(event.target.value)}
        className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
      />

      {canEdit ? (
        <select
          value={selectedNadeId}
          onChange={(event) => setSelectedNadeId(event.target.value)}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        >
          <option value="">Bez podpiętego granatu</option>
          {nades?.map((nade) => (
            <option key={nade.id} value={nade.id}>
              {grenadeTypeLabels[nade.type]} — {nade.title}
            </option>
          ))}
        </select>
      ) : (
        nadeEntryId &&
        nades?.find((nade) => nade.id === nadeEntryId) && (
          <p className="text-xs text-neutral-500">
            Granat: {nades.find((nade) => nade.id === nadeEntryId)!.title}
          </p>
        )
      )}

      {canEdit && (
        <div className="flex gap-2">
          <button
            type="button"
            onClick={handleSave}
            className="self-start rounded-md bg-red-600 px-3 py-1.5 text-xs font-medium text-white transition hover:bg-red-500"
          >
            Zapisz punkt
          </button>
          <button
            type="button"
            onClick={onRemove}
            className="rounded-md border border-neutral-700 px-3 py-1.5 text-xs text-neutral-300 transition hover:border-red-500 hover:text-red-400"
          >
            Usuń punkt
          </button>
        </div>
      )}
    </div>
  )
}
