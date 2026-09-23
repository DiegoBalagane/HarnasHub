import { useEffect, useState } from 'react'
import type { AnalysisBoard } from '../../../services/analysisBoardsApi'
import { analysisBoardsApi } from '../../../services/analysisBoardsApi'
import type { MapName } from '../../../services/nadesApi'
import { BoardCanvas } from '../canvas/BoardCanvas'
import type { Stroke } from '../canvas/types'
import { useCreateBoard, useUpdateBoard } from '../hooks/useAnalysisBoards'

const brushColors = ['#ff2d2d', '#ffd42d', '#2dff6b', '#2dcfff', '#ffffff', '#000000']

interface BoardEditorProps {
  /** Omitted when creating a brand new board. */
  board?: AnalysisBoard
  /** Only used (and only editable) when creating a new board — an existing one keeps the map it was created for. */
  defaultMapName: MapName
  onClose: () => void
}

/** Full-featured drawing editor: pen color/thickness, undo, clear, and a background that's either the built-in
 * radar for the board's map or a coach-uploaded screenshot, pasteable with Ctrl+V or picked from a file. */
export function BoardEditor({ board, defaultMapName, onClose }: BoardEditorProps) {
  const [mapName] = useState<MapName>(board?.mapName ?? defaultMapName)
  const [title, setTitle] = useState(board?.title ?? '')
  const [strokes, setStrokes] = useState<Stroke[]>(() => (board ? (JSON.parse(board.strokesJson) as Stroke[]) : []))
  const [color, setColor] = useState(brushColors[0])
  const [width, setWidth] = useState(6)
  // Local object URL for an image just pasted/picked but not yet uploaded — shown immediately, uploaded on save.
  const [pendingImageFile, setPendingImageFile] = useState<Blob | null>(null)
  const [pendingImagePreview, setPendingImagePreview] = useState<string | null>(null)
  const [backgroundCleared, setBackgroundCleared] = useState(false)
  const [uploadError, setUploadError] = useState(false)

  const createBoard = useCreateBoard()
  const updateBoard = useUpdateBoard()
  const isSaving = createBoard.isPending || updateBoard.isPending

  // Ctrl+V anywhere while the editor is open sets a pasted screenshot as the background.
  useEffect(() => {
    function handlePaste(event: ClipboardEvent) {
      const item = [...(event.clipboardData?.items ?? [])].find((candidate) => candidate.type.startsWith('image/'))
      const file = item?.getAsFile()
      if (!file) return
      event.preventDefault()
      applyPendingImage(file)
    }

    document.addEventListener('paste', handlePaste)
    return () => document.removeEventListener('paste', handlePaste)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  useEffect(() => {
    return () => {
      if (pendingImagePreview) URL.revokeObjectURL(pendingImagePreview)
    }
  }, [pendingImagePreview])

  function applyPendingImage(file: Blob) {
    setPendingImageFile(file)
    setPendingImagePreview(URL.createObjectURL(file))
    setBackgroundCleared(false)
  }

  function handleFileInput(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0]
    if (file) applyPendingImage(file)
  }

  function useMapRadarInstead() {
    setPendingImageFile(null)
    setPendingImagePreview(null)
    setBackgroundCleared(true)
  }

  const backgroundSrc =
    pendingImagePreview ?? (backgroundCleared ? null : board?.backgroundImageUrl) ?? `/maps/${mapName.toLowerCase()}.webp`

  function handleUndo() {
    setStrokes((current) => current.slice(0, -1))
  }

  function handleClear() {
    if (strokes.length > 0 && !window.confirm('Wyczyścić cały rysunek?')) return
    setStrokes([])
  }

  async function handleSave() {
    setUploadError(false)

    try {
      // Defaults to the board's current key unchanged; only a clear or a fresh upload actually changes it.
      let backgroundImageObjectKey: string | null = board?.backgroundImageObjectKey ?? null
      if (backgroundCleared) backgroundImageObjectKey = null
      if (pendingImageFile) backgroundImageObjectKey = await analysisBoardsApi.uploadBackgroundImage(pendingImageFile)

      const strokesJson = JSON.stringify(strokes)

      if (board) {
        await updateBoard.mutateAsync({
          boardId: board.id,
          payload: { title, backgroundImageObjectKey, strokesJson },
        })
      } else {
        await createBoard.mutateAsync({ mapName, title, backgroundImageObjectKey, strokesJson })
      }

      onClose()
    } catch {
      setUploadError(true)
    }
  }

  return (
    <div className="flex flex-col gap-3">
      <div className="flex flex-wrap items-center gap-2">
        <input
          required
          placeholder="Tytuł tablicy"
          value={title}
          onChange={(event) => setTitle(event.target.value)}
          className="flex-1 rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 text-sm outline-none focus:border-neutral-500"
        />
        {!board && (
          <span className="rounded-md border border-neutral-800 px-3 py-2 text-sm text-neutral-400">{mapName}</span>
        )}
      </div>

      <div className="flex flex-wrap items-center gap-3 rounded-md border border-neutral-800 p-2">
        <div className="flex items-center gap-1">
          {brushColors.map((option) => (
            <button
              key={option}
              type="button"
              title={option}
              onClick={() => setColor(option)}
              style={{ backgroundColor: option }}
              className={`h-6 w-6 rounded-full border-2 ${color === option ? 'border-white' : 'border-neutral-700'}`}
            />
          ))}
          <input
            type="color"
            value={color}
            onChange={(event) => setColor(event.target.value)}
            title="Własny kolor"
            className="h-6 w-6 cursor-pointer rounded border border-neutral-700 bg-transparent"
          />
        </div>

        <label className="flex items-center gap-2 text-xs text-neutral-400">
          Grubość
          <input
            type="range"
            min={2}
            max={30}
            value={width}
            onChange={(event) => setWidth(Number(event.target.value))}
            className="w-24"
          />
        </label>

        <button
          type="button"
          onClick={handleUndo}
          disabled={strokes.length === 0}
          className="rounded-md border border-neutral-700 px-2 py-1 text-xs hover:border-neutral-500 disabled:opacity-40"
        >
          ↶ Cofnij
        </button>
        <button
          type="button"
          onClick={handleClear}
          disabled={strokes.length === 0}
          className="rounded-md border border-neutral-700 px-2 py-1 text-xs hover:border-neutral-500 disabled:opacity-40"
        >
          Wyczyść
        </button>

        <label className="cursor-pointer rounded-md border border-neutral-700 px-2 py-1 text-xs hover:border-neutral-500">
          + Wgraj obraz
          <input type="file" accept="image/*" onChange={handleFileInput} className="hidden" />
        </label>
        {(pendingImagePreview || (board?.backgroundImageUrl && !backgroundCleared)) && (
          <button
            type="button"
            onClick={useMapRadarInstead}
            className="rounded-md border border-neutral-700 px-2 py-1 text-xs hover:border-neutral-500"
          >
            Użyj mapy
          </button>
        )}
        <span className="text-xs text-neutral-600">albo wklej zrzut ekranu (Ctrl+V)</span>
      </div>

      <BoardCanvas
        backgroundSrc={backgroundSrc}
        strokes={strokes}
        onStrokeComplete={(stroke) => setStrokes((current) => [...current, stroke])}
        currentColor={color}
        currentWidth={width}
      />

      {uploadError && <p className="text-sm text-red-400">Nie udało się zapisać tablicy.</p>}

      <div className="flex gap-2">
        <button
          type="button"
          onClick={handleSave}
          disabled={isSaving || title.trim() === ''}
          className="self-start rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-red-500 disabled:opacity-50"
        >
          {isSaving ? 'Zapisywanie…' : 'Zapisz'}
        </button>
        <button
          type="button"
          onClick={onClose}
          className="self-start rounded-md border border-neutral-700 px-4 py-2 text-sm text-neutral-300 transition hover:border-neutral-500"
        >
          Anuluj
        </button>
      </div>
    </div>
  )
}
