import { API_ENDPOINTS } from '../constants'
import { apiClient } from './apiClient'

export interface AuthResult {
  accessToken: string
  userId: string
  displayName: string
  role: string
}

export interface RegisterPayload {
  email: string
  displayName: string
  password: string
}

export interface LoginPayload {
  email: string
  password: string
}

export const authApi = {
  register: (payload: RegisterPayload) => apiClient.post<AuthResult>(API_ENDPOINTS.auth.register, payload),
  login: (payload: LoginPayload) => apiClient.post<AuthResult>(API_ENDPOINTS.auth.login, payload),
}
