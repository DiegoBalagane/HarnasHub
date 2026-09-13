import { Link } from 'react-router-dom'
import { RegisterForm } from '../../features/auth/components/RegisterForm'

export function RegisterPage() {
  return (
    <>
      <h1 className="text-2xl font-semibold">Dołącz do drużyny</h1>
      <RegisterForm />
      <p className="text-sm text-neutral-400">
        Masz już konto?{' '}
        <Link to="/login" className="text-red-400 hover:underline">
          Zaloguj się
        </Link>
      </p>
    </>
  )
}
