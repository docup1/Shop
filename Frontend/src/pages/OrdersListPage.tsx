import { Link } from 'react-router-dom'
import { useTranslation } from '../i18n/context'
import { OrdersListController } from '../controllers/OrdersListController'

export function OrdersListPage(): React.JSX.Element {
  const { t } = useTranslation()
  return (
    <div className="page">
      <div className="page__title-row">
        <h2 className="page-title">{t('orders.title')}</h2>
        <Link className="btn btn--primary" to="/orders/new">
          {t('nav.newOrder')}
        </Link>
      </div>
      <OrdersListController />
    </div>
  )
}
