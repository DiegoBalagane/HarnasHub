import { AddMaterialForm } from '../../features/materials/components/AddMaterialForm'
import { MaterialList } from '../../features/materials/components/MaterialList'
import { useIsCoachOrManager } from '../../features/auth/hooks/useIsCoachOrManager'

export function MaterialsPage() {
  const canManage = useIsCoachOrManager()

  return (
    <>
      <h1 className="text-2xl font-semibold">Materiały treningowe</h1>
      {canManage && <AddMaterialForm />}
      <MaterialList />
    </>
  )
}
