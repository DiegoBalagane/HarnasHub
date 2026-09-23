import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { tasksApi, type AssignTaskPayload } from '../../../services/tasksApi'

/** Fetches the current user's tasks, open ones first. */
export function useMyTasks() {
  return useQuery({
    queryKey: ['tasks', 'mine'],
    queryFn: tasksApi.getMyTasks,
  })
}

/** Coach/Manager: fetches every player's tasks, for the review/delete overview. */
export function useAllTasks() {
  return useQuery({
    queryKey: ['tasks', 'all'],
    queryFn: tasksApi.getAllTasks,
  })
}

/** Assigns a new task and refreshes task lists and the dashboard. */
export function useAssignTask() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: AssignTaskPayload) => tasksApi.assignTask(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] })
      queryClient.invalidateQueries({ queryKey: ['dashboard'] })
    },
  })
}

/** Player: submits a task for the coach's review and refreshes task lists and the dashboard. */
export function useSubmitTask() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (taskId: string) => tasksApi.submitTask(taskId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] })
      queryClient.invalidateQueries({ queryKey: ['dashboard'] })
    },
  })
}

/** Coach/Manager: confirms a task as done and refreshes task lists and the dashboard. */
export function useApproveTask() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (taskId: string) => tasksApi.approveTask(taskId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] })
      queryClient.invalidateQueries({ queryKey: ['dashboard'] })
    },
  })
}

/** Coach/Manager: sends a task back for rework and refreshes task lists and the dashboard. */
export function useRejectTask() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (taskId: string) => tasksApi.rejectTask(taskId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] })
      queryClient.invalidateQueries({ queryKey: ['dashboard'] })
    },
  })
}

/** Coach/Manager: permanently removes a task, e.g. one assigned by mistake. */
export function useDeleteTask() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (taskId: string) => tasksApi.deleteTask(taskId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] })
      queryClient.invalidateQueries({ queryKey: ['dashboard'] })
    },
  })
}
