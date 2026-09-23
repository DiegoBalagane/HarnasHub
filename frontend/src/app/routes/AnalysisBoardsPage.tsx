import { useState } from 'react'
import { Modal } from '../../components/Modal'
import { useIsCoachOrManager } from '../../features/auth/hooks/useIsCoachOrManager'
import { BoardEditor } from '../../features/analysis-boards/components/BoardEditor'
import { BoardGallery } from '../../features/analysis-boards/components/BoardGallery'
import { mapNames } from '../../features/nades/labels'
import type { AnalysisBoard } from '../../services/analysisBoardsApi'
import type { MapName } from '../../services/nadesApi'

type EditorState = { board: AnalysisBoard } | { mapName: MapName } | null

/** Coach whiteboard: per-map radar or an uploaded screenshot, drawn on and saved as reusable analysis boards. */
export function AnalysisBoardsPage() {
  const [mapFilter, setMapFilter] = useState<MapName | 'all'>('all')
  const [editorState, setEditorState] = useState<EditorState>(null)
  const canManage = useIsCoachOrManager()

  return (
    <>
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-2xl font-semibold">Analizy</h1>
        {canManage && (
          <button
            type="button"
            onClick={() => setEditorState({ mapName: mapFilter === 'all' ? mapNames[0] : mapFilter })}
            className="rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-red-500"
          >
            + Nowa tablica
          </button>
        )}
      </div>

      <div className="flex flex-wrap items-center gap-2">
        <button
          type="button"
          onClick={() => setMapFilter('all')}
          className={`rounded-md border px-3 py-2 text-sm ${
            mapFilter === 'all' ? 'border-neutral-500 bg-neutral-100 text-neutral-900' : 'border-neutral-800 text-neutral-400 hover:text-white'
          }`}
        >
          Wszystkie
        </button>
        {mapNames.map((map) => (
          <button
            key={map}
            type="button"
            onClick={() => setMapFilter(map)}
            className={`rounded-md border px-3 py-2 text-sm ${
              mapFilter === map ? 'border-neutral-500 bg-neutral-100 text-neutral-900' : 'border-neutral-800 text-neutral-400 hover:text-white'
            }`}
          >
            {map}
          </button>
        ))}
      </div>

      <BoardGallery mapFilter={mapFilter === 'all' ? undefined : mapFilter} onEdit={(board) => setEditorState({ board })} />

      {editorState && (
        <Modal title={'board' in editorState ? editorState.board.title : 'Nowa tablica'} onClose={() => setEditorState(null)} wide>
          <BoardEditor
            board={'board' in editorState ? editorState.board : undefined}
            defaultMapName={'board' in editorState ? editorState.board.mapName : editorState.mapName}
            onClose={() => setEditorState(null)}
          />
        </Modal>
      )}
    </>
  )
}
