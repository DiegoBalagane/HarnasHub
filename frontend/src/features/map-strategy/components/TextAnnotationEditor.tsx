import { useState } from 'react'
import type { MapTextAnnotation } from '../../../services/mapStrategyApi'

interface TextAnnotationEditorProps {
  annotation: MapTextAnnotation
  onSave: (text: string, color: string, fontSizePx: number) => void
  onDelete: () => void
  onClose: () => void
  isSaving: boolean
}

/** Inline form for one map text annotation's content, color, and font size. Saving closes the editor right away. */
export function TextAnnotationEditor({ annotation, onSave, onDelete, onClose, isSaving }: TextAnnotationEditorProps) {
  const [text, setText] = useState(annotation.text)
  const [color, setColor] = useState(annotation.color)
  const [fontSizePx, setFontSizePx] = useState(annotation.fontSizePx)

  return (
    <div className="flex flex-col gap-2 rounded-md border border-neutral-700 bg-neutral-950 p-3">
      <div className="flex items-center justify-between">
        <p className="text-xs text-neutral-400">Notatka na mapie</p>
        <button type="button" onClick={onClose} className="text-xs text-neutral-500 hover:text-neutral-300">
          Zamknij
        </button>
      </div>

      <textarea
        maxLength={200}
        rows={2}
        placeholder="Treść notatki…"
        value={text}
        onChange={(event) => setText(event.target.value)}
        className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
      />

      <div className="flex items-center gap-4">
        <label className="flex items-center gap-2 text-xs text-neutral-400">
          Kolor
          <input
            type="color"
            value={color}
            onChange={(event) => setColor(event.target.value)}
            className="h-7 w-10 cursor-pointer rounded border border-neutral-800 bg-neutral-900"
          />
        </label>

        <label className="flex flex-1 items-center gap-2 text-xs text-neutral-400">
          Rozmiar
          <input
            type="range"
            min={10}
            max={40}
            value={fontSizePx}
            onChange={(event) => setFontSizePx(Number(event.target.value))}
            className="flex-1"
          />
          <span className="w-8 text-right text-neutral-300">{fontSizePx}</span>
        </label>
      </div>

      <p style={{ color, fontSize: `${fontSizePx}px` }} className="truncate font-semibold">
        {text || 'Podgląd…'}
      </p>

      <div className="flex gap-2">
        <button
          type="button"
          disabled={isSaving || text.trim() === ''}
          onClick={() => {
            onSave(text.trim(), color, fontSizePx)
            onClose()
          }}
          className="self-start rounded-md bg-red-600 px-3 py-1.5 text-xs font-medium text-white transition hover:bg-red-500 disabled:opacity-50"
        >
          Zapisz
        </button>
        <button
          type="button"
          disabled={isSaving}
          onClick={onDelete}
          className="rounded-md border border-neutral-700 px-3 py-1.5 text-xs text-neutral-300 transition hover:border-neutral-500 disabled:opacity-50"
        >
          Usuń notatkę
        </button>
      </div>
    </div>
  )
}
