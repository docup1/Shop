import { useTranslation } from '../i18n/context'
import type { OrderResponse } from '../models/api'
import { OrderStatusBadge } from './OrderStatusBadge'
import { Spinner } from './Spinner'

interface OrderDetailsProps {
  order: OrderResponse | null
  loading: boolean
  error: string | null
}

function formatDate(value: string): string {
  return new Date(value).toLocaleString()
}

export function OrderDetails({ order, loading, error }: OrderDetailsProps): React.JSX.Element {
  const { t } = useTranslation()

  if (loading) return <Spinner />
  if (error) return <p className="form-error" role="alert">{error}</p>
  if (!order) return <p>{t('orderDetails.notFound')}</p>

  const rows: Array<[string, string]> = [
    [t('orderForm.senderCity'), order.senderCity],
    [t('orderForm.recipientCity'), order.recipientCity],
    [t('orderForm.senderAddress'), order.senderAddress],
    [t('orderForm.recipientAddress'), order.recipientAddress],
    [t('orderForm.weight'), `${order.weight} кг`],
    [t('orderDetails.createdAt'), formatDate(order.createdAt)],
  ]

  return (
    <div className="card order-details">
      <h2 className="order-details__title">{t('orderDetails.title')} #{order.id.slice(0, 8)}</h2>
      <div className="order-details__status">
        <OrderStatusBadge status={order.status} />
      </div>
      <dl className="order-details__list">
        {rows.map(([label, value]) => (
          <div className="order-details__row" key={label}>
            <dt>{label}</dt>
            <dd>{value}</dd>
          </div>
        ))}
      </dl>
    </div>
  )
}
