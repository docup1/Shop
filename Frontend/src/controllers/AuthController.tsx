import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from '../i18n/context'
import { useAuth } from '../hooks/authContext'
import { ApiError } from '../services/client'
import { AuthForm, type AuthMode } from '../views/AuthForm'

interface Props {
  initialMode?: AuthMode
}

export function AuthController({ initialMode = 'login' }: Props): React.JSX.Element {
  const { t } = useTranslation()
  const { login, register } = useAuth()
  const navigate = useNavigate()

  const [mode, setMode] = useState<AuthMode>(initialMode)
  const [userName, setUserName] = useState('')
  const [password, setPassword] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [fieldErrors, setFieldErrors] = useState<Partial<Record<'userName' | 'password', string>>>({})

  function switchMode(): void {
    setMode((current) => (current === 'login' ? 'register' : 'login'))
    setError(null)
    setFieldErrors({})
  }

  async function handleSubmit(): Promise<void> {
    const errors: typeof fieldErrors = {}
    if (!userName.trim()) errors.userName = t('auth.errors.userNameRequired')
    if (password.length < 8) errors.password = t('auth.errors.passwordTooShort')
    setFieldErrors(errors)
    if (Object.keys(errors).length > 0) return

    setLoading(true)
    setError(null)
    try {
      const action = mode === 'login' ? login : register
      await action(userName.trim(), password)
      navigate('/orders')
    } catch (err) {
      if (err instanceof ApiError) {
        if (err.status === 409) {
          setError(err.message)
        } else {
          setError(t('auth.errors.default'))
        }
      } else {
        setError(t('common.error'))
      }
    } finally {
      setLoading(false)
    }
  }

  return (
    <AuthForm
      mode={mode}
      userName={userName}
      password={password}
      onUserNameChange={setUserName}
      onPasswordChange={setPassword}
      onSubmit={() => void handleSubmit()}
      onSwitchMode={switchMode}
      error={error}
      fieldErrors={fieldErrors}
      loading={loading}
    />
  )
}
