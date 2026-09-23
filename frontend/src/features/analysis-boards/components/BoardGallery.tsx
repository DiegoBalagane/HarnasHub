import type { AnalysisBoard } from '../../../services/analysisBoardsApi'
import type { MapName } from '../../../services/nadesApi'
import { useIsCoachOrManager } from '../../auth/hooks/useIsCoachOrManager'
import { BoardCanvas } from '../canvas/BoardCanvas'
import type { Stroke } from '../canvas/types'
import { useAnalysisBoards, useDeleteBoard } from '../hooks/useAnalysisBoards'

const dateFormatter = new Intl.DateTimeFormat('pl-PL', { dateStyle: 'medium', timeStyle: 'short' })

function groupByMap(boards: AnalysisBoard[]) {
  const groups = new Map<MapName, AnalysisBoard[]>()

  for (const board of boards) {
    const group = groups.get(board.mapName)
    if (group) {
      group.push(board)
    } else {
      groups.set(board.mapName, [board])
    }
  }

  return [...groups.entries()]
}

interface BoardGalleryProps {
  mapFilter?: MapName
  onEdit: (board: AnalysisBoard) => void
}

/** Saved analysis boards grouped by map, each shown as a small clickable preview that opens the full editor. */
export function BoardGallery({ mapFilter, onEdit }: BoardGalleryProps) {
  const { data: boards, isLoading, isError } = useAnalysisBoards(mapFilter)
  const deleteBoard = useDeleteBoard()
  const canManage = useIsCoachOrManager()

  if (isLoading) {
    return <p className="text-neutral-400">Ładowanie tablic…</p>
  }

  if (isError) {
    return <p className="text-red-400">Nie udało się pobrać tablic.</p>
  }

  if (boards?.length === 0) {
    return <p className="text-neutral-400">Brak zapisanych tablic dla tej mapy.</p>
  }

  return (
    <div className="flex w-full flex-col gap-6">
      {groupByMap(boards ?? []).map(([mapName, group]) => (
        <section key={mapName} className="flex flex-col gap-2">
          <h3 className="text-sm font-semibold text-neutral-300">{mapName}</h3>
          <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 md:grid-cols-4">
            {group.map((board) => (
              <div key={board.id} className="flex flex-col gap-1">
                <button
                  type="button"
                  onClick={() => onEdit(board)}
                  className="overflow-hidden rounded-md border border-neutral-800 transition hover:border-neutral-500"
                >
                  <BoardCanvas
                    backgroundSrc={board.backgroundImageUrl ?? `/maps/${board.mapName.toLowerCase()}.webp`}
                    strokes={JSON.parse(board.strokesJson) as Stroke[]}
                    className="block h-auto w-full"
                  />
                </button>
                <p className="truncate text-xs font-medium">{board.title}</p>
                <p className="text-[11px] text-neutral-500">{dateFormatter.format(new Date(board.updatedAtUtc))}</p>
                {canManage && (
                  <button
                    type="button"
                    onClick={() => {
                      if (window.confirm(`Usunąć tablicę „${board.title}"?`)) {
                        deleteBoard.mutate(board.id)
                      }
                    }}
                    disabled={deleteBoard.isPending}
                    className="self-start text-[11px] text-neutral-500 transition hover:text-red-400 disabled:opacity-50"
                  >
                    Usuń
                  </button>
                )}
              </div>
            ))}
          </div>
        </section>
      ))}
    </div>
  )
}
