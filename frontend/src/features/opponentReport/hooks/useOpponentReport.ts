import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useCallback } from 'react'
import { gamePlanApi } from '../../../services/gamePlanApi'
import {
  opponentReportApi,
  type OpponentFaceitLink,
  type OpponentReport,
  type VetoFormat,
} from '../../../services/opponentReportApi'
import { useBackgroundJob } from '../../jobs/hooks/useBackgroundJob'
import { buildPlanNotes } from '../planNotes'

/** Query key of one opponent's report; lower-cased so differently-typed spellings share one cache entry. */
export function reportKey(name: string) {
  return ['opponent-report', name.trim().toLowerCase()]
}

/** Fetches the opponent's FACEIT report (the stored snapshot, or a live build from the cache). */
export function useOpponentReport(name: string) {
  return useQuery({
    queryKey: reportKey(name),
    queryFn: () => opponentReportApi.getReport(name),
    enabled: name.trim() !== '',
  })
}

/** Links the opponent to a FACEIT roster and pulls its data as one background job (slotted per opponent, so progress
 * survives leaving the page); the report, veto suggestion and opponent lists are refetched afterwards. */
export function useLinkOpponentFaceit(name: string) {
  const queryClient = useQueryClient()
  const key = name.trim().toLowerCase()

  const start = useCallback((source: string) => opponentReportApi.link(name, source), [name])
  const onSucceeded = useCallback(() => {
    queryClient.invalidateQueries({ queryKey: reportKey(key) })
    queryClient.invalidateQueries({ queryKey: ['veto'] })
    queryClient.invalidateQueries({ queryKey: ['opponents'] })
  }, [queryClient, key])

  return useBackgroundJob<string, OpponentFaceitLink>(start, { onSucceeded, slot: `faceit-link:${key}` })
}

/** Pulls fresh FACEIT data as a background job shared by every component of the same opponent; the resulting report
 * replaces the cached one right away. */
export function useRefreshOpponentReport(name: string) {
  const queryClient = useQueryClient()
  const key = name.trim().toLowerCase()

  const start = useCallback(() => opponentReportApi.refresh(name), [name])
  const onSucceeded = useCallback(
    (report: OpponentReport) => {
      queryClient.setQueryData(reportKey(key), report)
      queryClient.invalidateQueries({ queryKey: ['veto'] })
    },
    [queryClient, key],
  )

  return useBackgroundJob<void, OpponentReport>(start, { onSucceeded, slot: `faceit-refresh:${key}` })
}

/** Writes the report's TL;DR and recommended veto into the next event's game plan notes, keeping attached tactics and boards. */
export function useCreateMatchPlan() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ report, format }: { report: OpponentReport; format: VetoFormat }) => {
      if (!report.nextEventId) {
        throw new Error('Brak zaplanowanego meczu z tym przeciwnikiem.')
      }
      const plan = await gamePlanApi.getPlan(report.nextEventId)
      return gamePlanApi.setPlan(report.nextEventId, {
        notes: buildPlanNotes(plan.notes, report, format),
        tacticIds: plan.tactics.map((tactic) => tactic.id),
        boardIds: plan.boards.map((board) => board.id),
      })
    },
    onSuccess: (plan) => {
      queryClient.invalidateQueries({ queryKey: ['game-plan', plan.eventId] })
    },
  })
}
