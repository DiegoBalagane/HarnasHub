import { useState } from 'react'
import { AddFormModal } from '../../../components/AddFormModal'
import type { MapName } from '../../../services/nadesApi'
import { AddNadeForm } from './AddNadeForm'
import { NadeLibrary } from './NadeLibrary'
import { NadeMapView } from './NadeMapView'

const tabButtonClass = (isActive: boolean) =>
  `rounded px-3 py-1 text-sm transition ${
    isActive ? 'bg-neutral-800 text-neutral-100' : 'text-neutral-500 hover:text-neutral-300'
  }`

interface NadesTabProps {
  /** Map picked on the Playbook page; undefined shows every map with the built-in selectors. */
  mapName?: MapName
}

/** Playbook "Granaty" tab: list/map toggle with the add form behind a button. */
export function NadesTab({ mapName }: NadesTabProps) {
  const [view, setView] = useState<'list' | 'map'>('list')

  return (
    <>
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex gap-1 rounded-md border border-neutral-800 p-0.5">
          <button type="button" onClick={() => setView('list')} className={tabButtonClass(view === 'list')}>
            Lista
          </button>
          <button type="button" onClick={() => setView('map')} className={tabButtonClass(view === 'map')}>
            Mapa
          </button>
        </div>
        <AddFormModal buttonLabel="+ Dodaj granat" title="Dodaj granat">
          {(close) => <AddNadeForm onDone={close} defaultMapName={mapName} />}
        </AddFormModal>
      </div>

      {view === 'list' ? <NadeLibrary mapName={mapName} /> : <NadeMapView mapName={mapName} />}
    </>
  )
}
