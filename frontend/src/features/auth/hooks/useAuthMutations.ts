import { useMutation } from '@tanstack/react-query'
import { authApi, type LoginPayload, type RegisterPayload } from '../../../services/authApi'
import { useAuthStore } from '../stores/useAuthStore'

/** Logs a user in and stores the resulting session on success. */
export function useLogin() {
  const setSession = useAuthStore((state) => state.setSession)

  return useMutation({
    mutationFn: (payload: LoginPayload) => authApi.login(payload),
    onSuccess: setSession,
  })
}

/** Registers a new player account and stores the resulting session on success. */
export function useRegister() {
  const setSession = useAuthStore((state) => state.setSession)

  return useMutation({
    mutationFn: (payload: RegisterPayload) => authApi.register(payload),
    onSuccess: setSession,
  })
}
