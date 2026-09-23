import type { MapSide } from '../../../services/mapStrategyApi'
import type { MapName } from '../../../services/nadesApi'
import { useAddTextAnnotation } from '../hooks/useMapStrategy'

interface AddTextAnnotationControlProps {
  mapName: MapName
  side: MapSide
}

/** Coach/Manager button that drops a new text annotation in the middle of the radar, ready to be edited and dragged. */
export function AddTextAnnotationControl({ mapName, side }: AddTextAnnotationControlProps) {
  const addTextAnnotation = useAddTextAnnotation()

  return (
    <button
      type="button"
      onClick={() =>
        addTextAnnotation.mutate({
          mapName,
          side,
          text: 'Nowa notatka',
          color: '#ffffff',
          fontSizePx: 16,
          x: 0.5,
          y: 0.5,
        })
      }
      disabled={addTextAnnotation.isPending}
      className="rounded-md border border-neutral-700 px-3 py-2 text-sm font-medium text-neutral-100 transition hover:border-neutral-500 disabled:opacity-40"
    >
      + Dodaj tekst na mapie
    </button>
  )
}
