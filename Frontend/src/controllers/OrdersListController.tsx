import { useCallback, useEffect, useState } from 'react'
import { useTranslation } from '../i18n/context'
import type { OrderResponse } from '../models/api'
import { ordersApi } from '../services/orders'
import { ApiError } from '../services/client'
import { OrdersTable } from '../views/OrdersTable'

export function OrdersListController(): React.JSX.Element {
  const { t } = useTranslation()

  const [orders, setOrders] = useState<OrderResponse[]>([])
  const [nextCursor, setNextCursor] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [loadingMore, setLoadingMore] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const loadFirstPage = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const page = await ordersApi.list({ pageSize: 20 })
      setOrders(page.items)
      setNextCursor(page.nextCursor)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t('common.error'))
    } finally {
      setLoading(false)
    }
  }, [t])

  useEffect(() => {
    void loadFirstPage()
  }, [loadFirstPage])

  async function loadMore(): Promise<void> {
    if (!nextCursor || loadingMore) return
    setLoadingMore(true)
    try {
      const page = await ordersApi.list({ cursor: nextCursor, pageSize: 20 })
      setOrders((current) => [...current, ...page.items])
      setNextCursor(page.nextCursor)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t('common.error'))
    } finally {
      setLoadingMore(false)
    }
  }

  return (
    <OrdersTable
      orders={orders}
      loading={loading}
      loadingMore={loadingMore}
      hasMore={nextCursor !== null}
      error={error}
      onLoadMore={() => void loadMore()}
    />
  )
}
