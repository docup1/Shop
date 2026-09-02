import { useTranslation } from '../i18n/context'

export type AuthMode = 'login' | 'register'

interface AuthFormProps {
  mode: AuthMode
  userName: string
  password: string
  onUserNameChange: (value: string) => void
  onPasswordChange: (value: string) => void
  onSubmit: () => void
  onSwitchMode: () => void
  error: string | null
  fieldErrors: { userName?: string; password?: string }
  loading: boolean
}

export function AuthForm({
  mode,
  userName,
  password,
  onUserNameChange,
  onPasswordChange,
  onSubmit,
  onSwitchMode,
  error,
  fieldErrors,
  loading,
}: AuthFormProps): React.JSX.Element {
  const { t } = useTranslation()
  const isLogin = mode === 'login'

  return (
    <form
      className="card auth-form"
      onSubmit={(event) => {
        event.preventDefault()
        if (!loading) onSubmit()
      }}
      noValidate
    >
      <h2 className="auth-form__title">{isLogin ? t('auth.loginTitle') : t('auth.registerTitle')}</h2>

      <label className="field">
        <span className="field__label">{t('auth.userName')}</span>
        <input
          className="field__input"
          value={userName}
          onChange={(event) => onUserNameChange(event.target.value)}
          autoComplete="username"
        />
        {fieldErrors.userName && <span className="field__error">{fieldErrors.userName}</span>}
      </label>

      <label className="field">
        <span className="field__label">{t('auth.password')}</span>
        <input
          className="field__input"
          type="password"
          value={password}
          onChange={(event) => onPasswordChange(event.target.value)}
          autoComplete={isLogin ? 'current-password' : 'new-password'}
        />
        {fieldErrors.password && <span className="field__error">{fieldErrors.password}</span>}
      </label>

      {!isLogin && <p className="auth-form__hint">{t('auth.passwordHint')}</p>}

      {error && <p className="form-error" role="alert">{error}</p>}

      <button className="btn btn--primary" type="submit" disabled={loading}>
        {isLogin ? t('auth.login') : t('auth.register')}
      </button>

      <button className="btn btn--ghost" type="button" onClick={onSwitchMode} disabled={loading}>
        {isLogin ? t('auth.switchToRegister') : t('auth.switchToLogin')}
      </button>
    </form>
  )
}
