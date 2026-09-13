import { AddNadeForm } from '../../features/nades/components/AddNadeForm'
import { NadeLibrary } from '../../features/nades/components/NadeLibrary'

export function NadesPage() {
  return (
    <>
      <h1 className="text-2xl font-semibold">Granaty</h1>
      <AddNadeForm />
      <NadeLibrary />
    </>
  )
}
