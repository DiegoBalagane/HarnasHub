import { Navigate, Route, Routes, useLocation } from 'react-router-dom'
import type { ComponentType } from 'react'
import { Layout } from './Layout'
import { ProtectedRoute } from './ProtectedRoute'
import { AdminPage } from './routes/AdminPage'
import { AttendancePage } from './routes/AttendancePage'
import { AuthCallbackPage } from './routes/AuthCallbackPage'
import { CalendarPage } from './routes/CalendarPage'
import { DashboardPage } from './routes/DashboardPage'
import { LoginPage } from './routes/LoginPage'
import { MatchDetailPage } from './routes/MatchDetailPage'
import { OpponentProfilePage } from './routes/OpponentProfilePage'
import { OpponentReportPage } from './routes/OpponentReportPage'
import { OpponentsPage } from './routes/OpponentsPage'
import { PlaybookPage } from './routes/PlaybookPage'
import { ResultsPage } from './routes/ResultsPage'
import { RosterPage } from './routes/RosterPage'
import { SettingsPage } from './routes/SettingsPage'
import { StatsPage } from './routes/StatsPage'
import { TeamInfoPage } from './routes/TeamInfoPage'
import { TasksPage } from './routes/TasksPage'

/** Old route → Playbook tab, so bookmarks and deep links (?map=, ?tactic=, ?board=) keep working. */
const legacyPlaybookRoutes: Record<string, string> = {
  '/map-strategy': 'positions',
  '/nades': 'nades',
  '/tactics': 'tactics',
  '/analysis-boards': 'boards',
  '/materials': 'materials',
  '/maps': 'pool',
}

/** Every authenticated page, keyed by path. */
const protectedRoutes: [string, ComponentType][] = [
  ['/dashboard', DashboardPage],
  ['/calendar', CalendarPage],
  ['/tasks', TasksPage],
  ['/results', ResultsPage],
  ['/results/:id', MatchDetailPage],
  ['/playbook', PlaybookPage],
  ['/stats', StatsPage],
  ['/opponents', OpponentsPage],
  ['/opponents/profile', OpponentProfilePage],
  ['/opponents/report', OpponentReportPage],
  ['/roster', RosterPage],
  ['/attendance', AttendancePage],
  ['/info', TeamInfoPage],
  ['/settings', SettingsPage],
  ['/admin', AdminPage],
]

/** Redirects a retired route to its new home, carrying the original query string over. */
function LegacyRedirect({ to, tab }: { to: string; tab?: string }) {
  const { search } = useLocation()
  const params = new URLSearchParams(search)
  if (tab) params.set('tab', tab)
  const query = params.toString()
  return <Navigate to={query ? `${to}?${query}` : to} replace />
}


export function AppRouter() {
  return (
    <Layout>
      <Routes>
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/auth/callback" element={<AuthCallbackPage />} />
        {protectedRoutes.map(([path, Page]) => (
          <Route key={path} path={path} element={<ProtectedRoute><Page /></ProtectedRoute>} />
        ))}
        <Route path="/events" element={<LegacyRedirect to="/calendar" />} />
        {Object.entries(legacyPlaybookRoutes).map(([path, tab]) => (
          <Route key={path} path={path} element={<LegacyRedirect to="/playbook" tab={tab} />} />
        ))}
      </Routes>
    </Layout>
  )
}
