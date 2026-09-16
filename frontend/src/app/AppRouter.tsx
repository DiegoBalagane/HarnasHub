import { Navigate, Route, Routes } from 'react-router-dom'
import { Layout } from './Layout'
import { ProtectedRoute } from './ProtectedRoute'
import { AuthCallbackPage } from './routes/AuthCallbackPage'
import { CalendarPage } from './routes/CalendarPage'
import { DashboardPage } from './routes/DashboardPage'
import { LoginPage } from './routes/LoginPage'
import { MapStrategyPage } from './routes/MapStrategyPage'
import { MaterialsPage } from './routes/MaterialsPage'
import { NadesPage } from './routes/NadesPage'
import { OpponentsPage } from './routes/OpponentsPage'
import { ResultsPage } from './routes/ResultsPage'
import { RosterPage } from './routes/RosterPage'
import { SettingsPage } from './routes/SettingsPage'
import { StatsPage } from './routes/StatsPage'
import { TasksPage } from './routes/TasksPage'

export function AppRouter() {
  return (
    <Layout>
      <Routes>
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/auth/callback" element={<AuthCallbackPage />} />
        <Route
          path="/dashboard"
          element={
            <ProtectedRoute>
              <DashboardPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/calendar"
          element={
            <ProtectedRoute>
              <CalendarPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/tasks"
          element={
            <ProtectedRoute>
              <TasksPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/results"
          element={
            <ProtectedRoute>
              <ResultsPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/nades"
          element={
            <ProtectedRoute>
              <NadesPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/map-strategy"
          element={
            <ProtectedRoute>
              <MapStrategyPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/materials"
          element={
            <ProtectedRoute>
              <MaterialsPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/stats"
          element={
            <ProtectedRoute>
              <StatsPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/opponents"
          element={
            <ProtectedRoute>
              <OpponentsPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/roster"
          element={
            <ProtectedRoute>
              <RosterPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/settings"
          element={
            <ProtectedRoute>
              <SettingsPage />
            </ProtectedRoute>
          }
        />
      </Routes>
    </Layout>
  )
}
