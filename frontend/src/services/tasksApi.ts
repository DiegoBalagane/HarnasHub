import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'

export type TaskStatus = 'Todo' | 'PendingReview' | 'Done' | 'NeedsRework'

export interface TeamTask {
  id: string
  title: string
  description: string | null
  status: TaskStatus
  dueAtUtc: string | null
  createdAtUtc: string
  trainingMaterialId: string | null
  trainingMaterialTitle: string | null
  trainingMaterialUrl: string | null
}

/** A task as seen by a coach/manager, with the assignee's resolved nickname attached. */
export interface TeamTaskWithAssignee extends TeamTask {
  assignedToUserId: string
  assignedToDisplayName: string
}

export interface AssignTaskPayload {
  title: string
  description?: string
  assignedToUserId: string
  dueAtUtc?: string
  trainingMaterialId?: string
}

export const tasksApi = {
  getMyTasks: () => apiClient.get<TeamTask[]>(API_ENDPOINTS.tasks.mine),
  getAllTasks: () => apiClient.get<TeamTaskWithAssignee[]>(API_ENDPOINTS.tasks.all),
  assignTask: (payload: AssignTaskPayload) => apiClient.post<TeamTask>(API_ENDPOINTS.tasks.assign, payload),
  /** Player marks a task (Todo or NeedsRework) as ready for the coach's review. */
  submitTask: (taskId: string) => apiClient.post<void>(API_ENDPOINTS.tasks.submit(taskId), {}),
  /** Coach/Manager confirms a PendingReview task as done. */
  approveTask: (taskId: string) => apiClient.post<void>(API_ENDPOINTS.tasks.approve(taskId), {}),
  /** Coach/Manager sends a PendingReview task back for rework. */
  rejectTask: (taskId: string) => apiClient.post<void>(API_ENDPOINTS.tasks.reject(taskId), {}),
  deleteTask: (taskId: string) => apiClient.delete<void>(API_ENDPOINTS.tasks.byId(taskId)),
}
