import { BrowserRouter, Navigate, Outlet, Route, Routes } from 'react-router-dom'
import { I18nProvider } from './i18n/context'
import { AuthProvider, useAuth } from './hooks/authContext'
import { Header } from './views/Header'
import { LoginPage } from './pages/LoginPage'
import { RegisterPage } from './pages/RegisterPage'
import { OrdersListPage } from './pages/OrdersListPage'
import { CreateOrderPage } from './pages/CreateOrderPage'
import { OrderViewPage } from './pages/OrderViewPage'
import { AdminPage } from './pages/AdminPage'
import { Spinner } from './views/Spinner'

function RequireAuth(): React.JSX.Element {
  const { isAuthenticated, loading } = useAuth()
  if (loading) return <div className="page"><Spinner /></div>
  if (!isAuthenticated) return <Navigate to="/login" replace />
  return <Outlet />
}

function RequireAdmin(): React.JSX.Element {
  const { user } = useAuth()
  if (!user?.isAdmin) return <Navigate to="/orders" replace />
  return <Outlet />
}

function GuestOnly(): React.JSX.Element {
  const { isAuthenticated, loading } = useAuth()
  if (loading) return <div className="page"><Spinner /></div>
  if (isAuthenticated) return <Navigate to="/orders" replace />
  return <Outlet />
}

function Layout(): React.JSX.Element {
  return (
    <>
      <Header />
      <main className="main">
        <Outlet />
      </main>
    </>
  )
}

function AppRoutes(): React.JSX.Element {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route element={<GuestOnly />}>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
        </Route>

        <Route element={<RequireAuth />}>
          <Route path="/orders" element={<OrdersListPage />} />
          <Route path="/orders/new" element={<CreateOrderPage />} />
          <Route path="/orders/:id" element={<OrderViewPage />} />

          <Route element={<RequireAdmin />}>
            <Route path="/admin" element={<AdminPage />} />
          </Route>
        </Route>

        <Route path="*" element={<Navigate to="/orders" replace />} />
      </Route>
    </Routes>
  )
}

export default function App(): React.JSX.Element {
  return (
    <I18nProvider>
      <AuthProvider>
        <BrowserRouter>
          <AppRoutes />
        </BrowserRouter>
      </AuthProvider>
    </I18nProvider>
  )
}
