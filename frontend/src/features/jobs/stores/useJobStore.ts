import { create } from 'zustand'

interface JobStoreState {
  /** Active job id per named slot (e.g. "faceit-refresh:rivals"), so progress survives leaving and re-opening a page. */
  jobsBySlot: Record<string, string>
  /** Job ids whose success callback already ran, so it runs once per job however many components follow it. */
  handledJobIds: Record<string, true>
  setSlotJob: (slot: string, jobId: string | null) => void
  markHandled: (jobId: string) => void
}

/** Session-wide registry of background jobs started from slotted {@link useBackgroundJob} calls. */
export const useJobStore = create<JobStoreState>((set) => ({
  jobsBySlot: {},
  handledJobIds: {},
  setSlotJob: (slot, jobId) =>
    set((state) => {
      const jobsBySlot = { ...state.jobsBySlot }
      if (jobId === null) delete jobsBySlot[slot]
      else jobsBySlot[slot] = jobId
      return { jobsBySlot }
    }),
  markHandled: (jobId) => set((state) => ({ handledJobIds: { ...state.handledJobIds, [jobId]: true } })),
}))
