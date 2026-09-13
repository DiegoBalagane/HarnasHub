import { AddMaterialForm } from '../../features/materials/components/AddMaterialForm'
import { MaterialList } from '../../features/materials/components/MaterialList'
import { useAuthStore } from '../../features/auth/stores/useAuthStore'

const coachRoles = new Set(['Coach', 'Manager'])

export function MaterialsPage() {
  const role = useAuthStore((state) => state.role)
  const canManage = role !== null && coachRoles.has(role)

  return (
    <>
      <h1 className="text-2xl font-semibold">Materiały treningowe</h1>
      {canManage && <AddMaterialForm />}
      <MaterialList />
    </>
  )
}
