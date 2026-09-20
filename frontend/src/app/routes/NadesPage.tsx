import { useState } from 'react'
import { AddNadeForm } from '../../features/nades/components/AddNadeForm'
import { NadeLibrary } from '../../features/nades/components/NadeLibrary'
import { NadeMapView } from '../../features/nades/components/NadeMapView'

const tabButtonClass = (isActive: boolean) =>
  `rounded px-3 py-1 text-sm transition ${
    isActive ? 'bg-neutral-800 text-neutral-100' : 'text-neutral-500 hover:text-neutral-300'
  }`

export function NadesPage() {
  const [view, setView] = useState<'list' | 'map'>('list')

  return (
    <>
      <h1 className="text-2xl font-semibold">Granaty</h1>
      <AddNadeForm />

      <div className="flex gap-1 self-start rounded-md border border-neutral-800 p-0.5">
        <button type="button" onClick={() => setView('list')} className={tabButtonClass(view === 'list')}>
          Lista
        </button>
        <button type="button" onClick={() => setView('map')} className={tabButtonClass(view === 'map')}>
          Mapa
        </button>
      </div>

      {view === 'list' ? <NadeLibrary /> : <NadeMapView />}
    </>
  )
}
