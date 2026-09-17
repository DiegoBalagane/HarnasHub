import { BrowserRouter } from 'react-router-dom'
import { AppRouter } from './app/AppRouter'
import { PwaUpdatePrompt } from './app/PwaUpdatePrompt'
import { QueryProvider } from './app/QueryProvider'
import { RealtimeSync } from './app/RealtimeSync'
import { SessionRefresh } from './app/SessionRefresh'

export function App() {
  return (
    <QueryProvider>
      <SessionRefresh />
      <RealtimeSync />
      <PwaUpdatePrompt />
      <BrowserRouter>
        <AppRouter />
      </BrowserRouter>
    </QueryProvider>
  )
}
