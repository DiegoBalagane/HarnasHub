import { useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { Modal } from '../../../components/Modal'
import type { AnalysisBoard } from '../../../services/analysisBoardsApi'
import type { MapName } from '../../../services/nadesApi'
import { useIsCoachOrManager } from '../../auth/hooks/useIsCoachOrManager'
import { mapNames } from '../../nades/labels'
import { useAnalysisBoards } from '../hooks/useAnalysisBoards'
import { BoardEditor } from './BoardEditor'
import { BoardGallery } from './BoardGallery'

type EditorState = { board: AnalysisBoard } | { mapName: MapName } | null

interface AnalysisBoardsSectionProps {
  /** Map picked on the Playbook page; undefined shows boards of every map. */
  mapName?: MapName
}

/** Coach whiteboard: per-map radar or an uploaded screenshot, drawn on and saved as reusable analysis boards. */
export function AnalysisBoardsSection({ mapName }: AnalysisBoardsSectionProps) {
  const [pickedEditorState, setEditorState] = useState<EditorState>(null)
  const canManage = useIsCoachOrManager()
  const [searchParams, setSearchParams] = useSearchParams()
  const linkedBoardId = searchParams.get('board')
  const { data: allBoards } = useAnalysisBoards()
  // A ?board= deep link (e.g. from an event's game plan) opens that board until the user closes it.
  const linkedBoard = linkedBoardId ? allBoards?.find((board) => board.id === linkedBoardId) : undefined
  const editorState: EditorState = pickedEditorState ?? (linkedBoard ? { board: linkedBoard } : null)

  function closeEditor() {
    setEditorState(null)
    if (searchParams.has('board')) {
      const nextParams = new URLSearchParams(searchParams)
      nextParams.delete('board')
      setSearchParams(nextParams, { replace: true })
    }
  }

  return (
    <>
      {canManage && (
        <button
          type="button"
          onClick={() => setEditorState({ mapName: mapName ?? mapNames[0] })}
          className="self-start rounded-md bg-primary-500 px-4 py-2 text-sm font-medium text-primary-950 transition hover:bg-primary-400"
        >
          + Nowa tablica
        </button>
      )}

      <BoardGallery mapFilter={mapName} onEdit={(board) => setEditorState({ board })} />

      {editorState && (
        <Modal title={'board' in editorState ? editorState.board.title : 'Nowa tablica'} onClose={closeEditor} wide>
          <BoardEditor
            board={'board' in editorState ? editorState.board : undefined}
            defaultMapName={'board' in editorState ? editorState.board.mapName : editorState.mapName}
            onClose={closeEditor}
          />
        </Modal>
      )}
    </>
  )
}
