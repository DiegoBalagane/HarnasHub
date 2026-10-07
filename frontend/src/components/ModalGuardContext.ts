import { createContext, useContext, useEffect, useId } from 'react'

/** State a form reports to the enclosing {@link Modal} so it can refuse a silent close. */
export interface ModalGuardState {
  /** An upload / analysis / save is in flight — closing must be confirmed. */
  isBusy: boolean
  /** The form holds unsaved input — closing must be confirmed. */
  isDirty: boolean
  /** Called when the user confirms closing a dirty (not busy) form, e.g. to drop a finished background job. */
  onDiscard?: () => void
}

/** Lets a form inside a Modal report its busy/dirty state (null outside a modal). */
export const ModalGuardContext = createContext<((key: string, state: ModalGuardState | null) => void) | null>(null)

/** Reports busy/dirty state of a form to the enclosing Modal (several reporters combine: any busy/dirty counts); a no-op when rendered outside one. */
export function useModalGuard({ isBusy, isDirty, onDiscard }: ModalGuardState) {
  const report = useContext(ModalGuardContext)
  const key = useId()

  useEffect(() => {
    report?.(key, { isBusy, isDirty, onDiscard })
  }, [report, key, isBusy, isDirty, onDiscard])

  useEffect(() => () => report?.(key, null), [report, key])
}
