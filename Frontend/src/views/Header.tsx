import { NavLink } from 'react-router-dom'
import { useTranslation } from '../i18n/context'
import { useAuth } from '../hooks/authContext'

export function Header(): React.JSX.Element {
  const { t } = useTranslation()
  const { user, isAuthenticated, logout } = useAuth()

  if (!isAuthenticated && !user) {
    return (
      <header className="header">
        <span className="header__brand">{t('common.appName')}</span>
      </header>
    )
  }

  return (
    <header className="header">
      <span className="header__brand">{t('common.appName')}</span>
      <nav className="header__nav">
        <NavLink to="/orders" className="header__link">
          {t('nav.myOrders')}
        </NavLink>
        <NavLink to="/orders/new" className="header__link">
          {t('nav.newOrder')}
        </NavLink>
        {user?.isAdmin && (
          <NavLink to="/admin" className="header__link">
            {t('nav.adminPanel')}
          </NavLink>
        )}
      </nav>
      <div className="header__right">
        {user && <span className="header__user">{user.userName}</span>}
        <button className="btn btn--ghost" type="button" onClick={() => void logout()}>
          {t('common.logout')}
        </button>
      </div>
    </header>
  )
}
