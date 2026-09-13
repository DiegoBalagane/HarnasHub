import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ApiError } from '../../../services/apiClient'
import { useRegister } from '../hooks/useAuthMutations'

/** Account creation form for new team members; navigates to the roster on success. */
export function RegisterForm() {
  const [email, setEmail] = useState('')
  const [displayName, setDisplayName] = useState('')
  const [password, setPassword] = useState('')
  const register = useRegister()
  const navigate = useNavigate()

  function handleSubmit(event: React.FormEvent) {
    event.preventDefault()
    register.mutate({ email, displayName, password }, { onSuccess: () => navigate('/roster') })
  }

  return (
    <form onSubmit={handleSubmit} className="flex w-full max-w-sm flex-col gap-4">
      <div className="flex flex-col gap-1">
        <label htmlFor="displayName" className="text-sm text-neutral-400">
          Nazwa wyświetlana
        </label>
        <input
          id="displayName"
          required
          value={displayName}
          onChange={(event) => setDisplayName(event.target.value)}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 outline-none focus:border-neutral-500"
        />
      </div>

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
          minLength={8}
          value={password}
          onChange={(event) => setPassword(event.target.value)}
          className="rounded-md border border-neutral-800 bg-neutral-900 px-3 py-2 outline-none focus:border-neutral-500"
        />
      </div>

      {register.isError && (
        <p className="text-sm text-red-400">
          {register.error instanceof ApiError ? register.error.message : 'Nie udało się utworzyć konta.'}
        </p>
      )}

      <button
        type="submit"
        disabled={register.isPending}
        className="rounded-md bg-red-600 px-4 py-2 font-medium text-white transition hover:bg-red-500 disabled:opacity-50"
      >
        {register.isPending ? 'Tworzenie konta…' : 'Utwórz konto'}
      </button>
    </form>
  )
}
