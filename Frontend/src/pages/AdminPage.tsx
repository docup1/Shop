import { Link } from 'react-router-dom'
import { useTranslation } from '../i18n/context'
import { AdminController } from '../controllers/AdminController'

export function AdminPage(): React.JSX.Element {
  const { t } = useTranslation()
  return (
    <div className="page">
      <Link className="btn btn--ghost" to="/orders">
        ← {t('common.back')}
      </Link>
      <AdminController />
    </div>
  )
}
