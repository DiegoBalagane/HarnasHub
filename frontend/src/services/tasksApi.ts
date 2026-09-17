import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'

export type TaskStatus = 'Todo' | 'Done'

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

export interface AssignTaskPayload {
  title: string
  description?: string
  assignedToUserId: string
  dueAtUtc?: string
  trainingMaterialId?: string
}

export const tasksApi = {
  getMyTasks: () => apiClient.get<TeamTask[]>(API_ENDPOINTS.tasks.mine),
  assignTask: (payload: AssignTaskPayload) => apiClient.post<TeamTask>(API_ENDPOINTS.tasks.assign, payload),
  completeTask: (taskId: string) => apiClient.post<void>(API_ENDPOINTS.tasks.complete(taskId), {}),
}
