import { useTranslation } from '../i18n/context'
import type { OrderResponse, OrderStatus } from '../models/api'
import { OrderStatusBadge } from './OrderStatusBadge'
import { Spinner } from './Spinner'

const ORDER_STATUSES: OrderStatus[] = [
  'New',
  'InProgress',
  'PickedUp',
  'InTransit',
  'OutForDelivery',
  'Delivered',
  'Cancelled',
]

interface AdminOrderTableProps {
  orders: OrderResponse[]
  loading: boolean
  loadingMore: boolean
  hasMore: boolean
  busyId: string | null
  currentStatus: OrderStatus
  error: string | null
  onStatusFilterChange: (status: OrderStatus) => void
  onLoadMore: () => void
  onChangeStatus: (order: OrderResponse, status: OrderStatus) => void
}

export function AdminOrderTable({
  orders,
  loading,
  loadingMore,
  hasMore,
  busyId,
  currentStatus,
  error,
  onStatusFilterChange,
  onLoadMore,
  onChangeStatus,
}: AdminOrderTableProps): React.JSX.Element {
  const { t } = useTranslation()

  return (
    <div>
      <div className="filter-row">
        <label className="field field--inline">
          <span className="field__label">{t('admin.orders.filterStatus')}</span>
          <select
            className="field__input"
            value={currentStatus}
            onChange={(event) => onStatusFilterChange(event.target.value as OrderStatus)}
          >
            {ORDER_STATUSES.map((status) => (
              <option key={status} value={status}>
                {t(`status.${status}`)}
              </option>
            ))}
          </select>
        </label>
      </div>

      {loading ? (
        <Spinner />
      ) : error ? (
        <p className="form-error" role="alert">{error}</p>
      ) : (
        <>
          <table className="table">
            <thead>
              <tr>
                <th>{t('orders.table.senderCity')}</th>
                <th>{t('orders.table.recipientCity')}</th>
                <th>{t('orders.table.status')}</th>
                <th>{t('orders.table.createdAt')}</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {orders.map((order) => {
                const busy = busyId === order.id
                return (
                  <tr key={order.id}>
                    <td>{order.senderCity}</td>
                    <td>{order.recipientCity}</td>
                    <td><OrderStatusBadge status={order.status} /></td>
                    <td>{new Date(order.createdAt).toLocaleString()}</td>
                    <td>
                      <select
                        className="field__input"
                        value={order.status}
                        disabled={busy || order.status === 'Cancelled' || order.status === 'Delivered'}
                        onChange={(event) => onChangeStatus(order, event.target.value as OrderStatus)}
                      >
                        {ORDER_STATUSES.map((status) => (
                          <option key={status} value={status}>
                            {t(`status.${status}`)}
                          </option>
                        ))}
                      </select>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>

          {hasMore && (
            <button className="btn btn--secondary load-more" type="button" onClick={onLoadMore} disabled={loadingMore}>
              {loadingMore ? t('common.loading') : t('orders.loadMore')}
            </button>
          )}
        </>
      )}
    </div>
  )
}
