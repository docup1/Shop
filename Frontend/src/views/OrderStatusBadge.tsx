import { useTranslation } from '../i18n/context'
import type { OrderStatus } from '../models/api'

export function OrderStatusBadge({ status }: { status: OrderStatus }): React.JSX.Element {
  const { t } = useTranslation()
  return <span className={`status-badge status-badge--${status.toLowerCase()}`}>{t(`status.${status}`)}</span>
}
