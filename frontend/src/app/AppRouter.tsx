import { Navigate, Route, Routes } from 'react-router-dom'
import { Layout } from './Layout'
import { ProtectedRoute } from './ProtectedRoute'
import { CalendarPage } from './routes/CalendarPage'
import { DashboardPage } from './routes/DashboardPage'
import { LoginPage } from './routes/LoginPage'
import { MaterialsPage } from './routes/MaterialsPage'
import { NadesPage } from './routes/NadesPage'
import { RegisterPage } from './routes/RegisterPage'
import { ResultsPage } from './routes/ResultsPage'
import { RosterPage } from './routes/RosterPage'
import { TasksPage } from './routes/TasksPage'

export function AppRouter() {
  return (
    <Layout>
      <Routes>
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
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
          path="/materials"
          element={
            <ProtectedRoute>
              <MaterialsPage />
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
      </Routes>
    </Layout>
  )
}
