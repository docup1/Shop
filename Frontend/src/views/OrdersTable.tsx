import { useTranslation } from '../i18n/context'
import type { OrderResponse } from '../models/api'
import { OrderStatusBadge } from './OrderStatusBadge'
import { Spinner } from './Spinner'
import { Link } from 'react-router-dom'

interface OrdersTableProps {
  orders: OrderResponse[]
  loading: boolean
  loadingMore: boolean
  hasMore: boolean
  error: string | null
  onLoadMore: () => void
}

export function OrdersTable({
  orders,
  loading,
  loadingMore,
  hasMore,
  error,
  onLoadMore,
}: OrdersTableProps): React.JSX.Element {
  const { t } = useTranslation()

  if (loading) return <Spinner />
  if (error) return <p className="form-error" role="alert">{error}</p>
  if (orders.length === 0) return <p className="empty">{t('orders.empty')}</p>

  return (
    <div>
      <table className="table">
        <thead>
          <tr>
            <th>{t('orders.table.senderCity')}</th>
            <th>{t('orders.table.recipientCity')}</th>
            <th>{t('orders.table.weight')}</th>
            <th>{t('orders.table.status')}</th>
            <th>{t('orders.table.createdAt')}</th>
          </tr>
        </thead>
        <tbody>
          {orders.map((order) => (
            <tr key={order.id}>
              <td>
                <Link to={`/orders/${order.id}`} className="table__link">
                  {order.senderCity} → {order.recipientCity}
                </Link>
              </td>
              <td>{order.recipientCity}</td>
              <td>{order.weight} кг</td>
              <td><OrderStatusBadge status={order.status} /></td>
              <td>{new Date(order.createdAt).toLocaleString()}</td>
            </tr>
          ))}
        </tbody>
      </table>

      {hasMore && (
        <button className="btn btn--secondary load-more" type="button" onClick={onLoadMore} disabled={loadingMore}>
          {loadingMore ? t('common.loading') : t('orders.loadMore')}
        </button>
      )}
    </div>
  )
}
