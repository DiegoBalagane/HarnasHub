import { AddFormModal } from '../../../components/AddFormModal'
import { useIsCoachOrManager } from '../../auth/hooks/useIsCoachOrManager'
import { AddMaterialForm } from './AddMaterialForm'
import { MaterialList } from './MaterialList'

/** Training materials list with the Coach/Manager add form behind a button. */
export function MaterialsTab() {
  const canManage = useIsCoachOrManager()

  return (
    <>
      {canManage && (
        <AddFormModal buttonLabel="+ Dodaj materiał" title="Dodaj materiał">
          {(close) => <AddMaterialForm onDone={close} />}
        </AddFormModal>
      )}
      <MaterialList />
    </>
  )
}
