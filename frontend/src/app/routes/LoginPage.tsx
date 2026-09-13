import { Link } from 'react-router-dom'
import { LoginForm } from '../../features/auth/components/LoginForm'

export function LoginPage() {
  return (
    <>
      <h1 className="text-2xl font-semibold">Zaloguj się</h1>
      <LoginForm />
      <p className="text-sm text-neutral-400">
        Nie masz konta?{' '}
        <Link to="/register" className="text-red-400 hover:underline">
          Zarejestruj się
        </Link>
      </p>
    </>
  )
}
