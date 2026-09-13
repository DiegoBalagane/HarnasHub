import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ApiError } from '../../../services/apiClient'
import { useLogin } from '../hooks/useAuthMutations'

/** Email/password login form; navigates to the roster on success. */
export function LoginForm() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const login = useLogin()
  const navigate = useNavigate()

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    login.mutate({ email, password }, { onSuccess: () => navigate('/roster') })
  }

  return (
    <form onSubmit={handleSubmit} className="flex w-full max-w-sm flex-col gap-4">
      <div className="flex flex-col gap-1">
        <label htmlFor="email" className="text-sm text-neutral-400">
          E-mail
        </label>
        <input
          id="email"
          type="email"
          required
          value={email}
          onChange={(event) => setEmail(event.target.value)}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 outline-none focus:border-neutral-500"
        />
      </div>

      <div className="flex flex-col gap-1">
        <label htmlFor="password" className="text-sm text-neutral-400">
          Hasło
        </label>
        <input
          id="password"
          type="password"
          required
          value={password}
          onChange={(event) => setPassword(event.target.value)}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 outline-none focus:border-neutral-500"
        />
      </div>

      {login.isError && (
        <p className="text-sm text-red-400">
          {login.error instanceof ApiError ? login.error.message : 'Nie udało się zalogować.'}
        </p>
      )}

      <button
        type="submit"
        disabled={login.isPending}
        className="rounded-md bg-red-600 px-4 py-2 font-medium text-white transition hover:bg-red-500 disabled:opacity-50"
      >
        {login.isPending ? 'Logowanie…' : 'Zaloguj się'}
      </button>
    </form>
  )
}
