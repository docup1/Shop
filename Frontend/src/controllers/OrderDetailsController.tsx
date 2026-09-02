import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { useTranslation } from '../i18n/context'
import type { OrderResponse } from '../models/api'
import { ordersApi } from '../services/orders'
import { ApiError } from '../services/client'
import { OrderDetails } from '../views/OrderDetails'

export function OrderDetailsController(): React.JSX.Element {
  const { t } = useTranslation()
  const { id } = useParams<{ id: string }>()

  const [order, setOrder] = useState<OrderResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    setError(null)

    if (!id) {
      setLoading(false)
      setError(t('orderDetails.notFound'))
      return
    }

    ordersApi
      .getById(id)
      .then((data) => {
        if (!cancelled) setOrder(data)
      })
      .catch((err: unknown) => {
        if (!cancelled) setError(err instanceof ApiError ? err.message : t('common.error'))
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })

    return () => {
      cancelled = true
    }
  }, [id, t])

  return <OrderDetails order={order} loading={loading} error={error} />
}
