import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { tasksApi, type AssignTaskPayload } from '../../../services/tasksApi'

/** Fetches the current user's tasks, open ones first. */
export function useMyTasks() {
  return useQuery({
    queryKey: ['tasks', 'mine'],
    queryFn: tasksApi.getMyTasks,
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

/** Marks a task done and refreshes task lists and the dashboard. */
export function useCompleteTask() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (taskId: string) => tasksApi.completeTask(taskId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] })
      queryClient.invalidateQueries({ queryKey: ['dashboard'] })
    },
  })
}
