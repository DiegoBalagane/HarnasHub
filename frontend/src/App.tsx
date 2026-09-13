import { BrowserRouter } from 'react-router-dom'
import { AppRouter } from './app/AppRouter'
import { QueryProvider } from './app/QueryProvider'
import { RealtimeSync } from './app/RealtimeSync'

export function App() {
  return (
    <QueryProvider>
      <RealtimeSync />
      <BrowserRouter>
        <AppRouter />
      </BrowserRouter>
    </QueryProvider>
  )
}
