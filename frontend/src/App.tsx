import { BrowserRouter } from 'react-router-dom'
import { AppRouter } from './app/AppRouter'
import { QueryProvider } from './app/QueryProvider'

export function App() {
  return (
    <QueryProvider>
      <BrowserRouter>
        <AppRouter />
      </BrowserRouter>
    </QueryProvider>
  )
}
