import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import type { UserResponse } from '../models/api'
import { authApi } from '../services/auth'
import { setAccessToken, setOnUnauthorized } from '../services/client'

interface AuthState {
  token: string | null
  refreshToken: string | null
  user: UserResponse | null
  loading: boolean
  isAuthenticated: boolean
  login: (userName: string, password: string) => Promise<void>
  register: (userName: string, password: string) => Promise<void>
  logout: () => Promise<void>
}

const TOKEN_KEY = 'shop.accessToken'
const REFRESH_KEY = 'shop.refreshToken'
const USER_KEY = 'shop.user'

const AuthContext = createContext<AuthState | null>(null)

function readStoredUser(): UserResponse | null {
  try {
    const raw = localStorage.getItem(USER_KEY)
    return raw ? (JSON.parse(raw) as UserResponse) : null
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: ReactNode }): React.JSX.Element {
  const [token, setToken] = useState<string | null>(() => localStorage.getItem(TOKEN_KEY))
  const [refreshToken, setRefreshToken] = useState<string | null>(() =>
    localStorage.getItem(REFRESH_KEY),
  )
  const [user, setUser] = useState<UserResponse | null>(readStoredUser)
  const [loading, setLoading] = useState<boolean>(() => Boolean(localStorage.getItem(TOKEN_KEY)))

  useEffect(() => {
    setAccessToken(token)
  }, [token])

  // При 401 (истёк access) пробуем обновить токен один раз; если не вышло — разлогиниваем.
  useEffect(() => {
    setOnUnauthorized(() => {
      // делегируем активную сессию: обновление ниже
      void attemptRefresh()
    })
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [refreshToken])

  async function attemptRefresh(): Promise<void> {
    const storedRefresh = refreshToken ?? localStorage.getItem(REFRESH_KEY)
    if (!storedRefresh) return
    try {
      const result = await authApi.refresh(storedRefresh)
      applySession(result.accessToken, result.refreshToken)
    } catch {
      clearSession()
    }
  }

  async function bootstrap(): Promise<void> {
    const storedToken = localStorage.getItem(TOKEN_KEY)
    if (!storedToken) {
      setLoading(false)
      return
    }
    try {
      const me = await authApi.me()
      setUser(me)
    } catch {
      await attemptRefresh()
    } finally {
      setLoading(false)
    }
  }

  function applySession(newToken: string, newRefresh: string): void {
    localStorage.setItem(TOKEN_KEY, newToken)
    localStorage.setItem(REFRESH_KEY, newRefresh)
    setToken(newToken)
    setRefreshToken(newRefresh)
  }

  function clearSession(): void {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(REFRESH_KEY)
    localStorage.removeItem(USER_KEY)
    setToken(null)
    setRefreshToken(null)
    setUser(null)
    setAccessToken(null)
  }

  useEffect(() => {
    void bootstrap()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  async function finishAuth(userName: string, password: string, mode: 'login' | 'register'): Promise<void> {
    const result = mode === 'login'
      ? await authApi.login({ userName, password })
      : await authApi.register({ userName, password })
    applySession(result.accessToken, result.refreshToken)
    const me = await authApi.me()
    localStorage.setItem(USER_KEY, JSON.stringify(me))
    setUser(me)
  }

  const login = useCallback(
    (userName: string, password: string) => finishAuth(userName, password, 'login'),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [],
  )

  const register = useCallback(
    (userName: string, password: string) => finishAuth(userName, password, 'register'),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [],
  )

  const logout = useCallback(async () => {
    const storedRefresh = refreshToken ?? localStorage.getItem(REFRESH_KEY)
    if (storedRefresh) {
      try {
        await authApi.logout(storedRefresh)
      } catch {
        // игнорируем — сессия всё равно локально очищается
      }
    }
    clearSession()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [refreshToken])

  const value = useMemo<AuthState>(
    () => ({
      token,
      refreshToken,
      user,
      loading,
      isAuthenticated: Boolean(token),
      login,
      register,
      logout,
    }),
    [token, refreshToken, user, loading, login, register, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthState {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
