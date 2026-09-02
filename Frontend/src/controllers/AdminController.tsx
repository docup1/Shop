import { useCallback, useEffect, useState } from 'react'
import { useTranslation } from '../i18n/context'
import type { OrderResponse, OrderStatus, UserResponse } from '../models/api'
import { ordersApi } from '../services/orders'
import { usersApi } from '../services/users'
import { ApiError } from '../services/client'
import { AdminOrderTable } from '../views/AdminOrderTable'
import { AdminUserTable } from '../views/AdminUserTable'

type Tab = 'users' | 'orders'

export function AdminController(): React.JSX.Element {
  const { t } = useTranslation()
  const [tab, setTab] = useState<Tab>('users')

  return (
    <div>
      <h2 className="page-title">{t('admin.title')}</h2>
      <div className="tabs" role="tablist">
        <button
          className={`tab ${tab === 'users' ? 'tab--active' : ''}`}
          type="button"
          onClick={() => setTab('users')}
        >
          {t('admin.usersTab')}
        </button>
        <button
          className={`tab ${tab === 'orders' ? 'tab--active' : ''}`}
          type="button"
          onClick={() => setTab('orders')}
        >
          {t('admin.ordersTab')}
        </button>
      </div>

      {tab === 'users' ? <UsersTab /> : <OrdersTab />}
    </div>
  )
}

function UsersTab(): React.JSX.Element {
  const { t } = useTranslation()
  const [users, setUsers] = useState<UserResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [busyId, setBusyId] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  const loadUsers = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const page = await usersApi.list({ pageSize: 100 })
      setUsers(page.items)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t('common.error'))
    } finally {
      setLoading(false)
    }
  }, [t])

  useEffect(() => {
    void loadUsers()
  }, [loadUsers])

  async function toggleAdmin(user: UserResponse): Promise<void> {
    setBusyId(user.id)
    setError(null)
    try {
      const updated = user.isAdmin
        ? await usersApi.revokeAdmin(user.id)
        : await usersApi.grantAdmin(user.id)
      setUsers((current) => current.map((u) => (u.id === updated.id ? updated : u)))
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t('common.error'))
    } finally {
      setBusyId(null)
    }
  }

  return (
    <AdminUserTable
      users={users}
      loading={loading}
      busyId={busyId}
      error={error}
      onToggleAdmin={(user) => void toggleAdmin(user)}
    />
  )
}

function OrdersTab(): React.JSX.Element {
  const [orders, setOrders] = useState<OrderResponse[]>([])
  const [status, setStatus] = useState<OrderStatus>('New')
  const [nextCursor, setNextCursor] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [loadingMore, setLoadingMore] = useState(false)
  const [busyId, setBusyId] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  const loadFirstPage = useCallback(async (targetStatus: OrderStatus) => {
    setLoading(true)
    setError(null)
    try {
      const page = await ordersApi.list({ status: targetStatus, pageSize: 20 })
      setOrders(page.items)
      setNextCursor(page.nextCursor)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : '')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadFirstPage(status)
  }, [status, loadFirstPage])

  async function loadMore(): Promise<void> {
    if (!nextCursor || loadingMore) return
    setLoadingMore(true)
    try {
      const page = await ordersApi.list({ status, cursor: nextCursor, pageSize: 20 })
      setOrders((current) => [...current, ...page.items])
      setNextCursor(page.nextCursor)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : '')
    } finally {
      setLoadingMore(false)
    }
  }

  async function changeStatus(order: OrderResponse, targetStatus: OrderStatus): Promise<void> {
    if (targetStatus === order.status) return
    setBusyId(order.id)
    setError(null)
    try {
      await ordersApi.changeStatus(order.id, { status: targetStatus })
      const page = await ordersApi.list({ status, pageSize: 100 })
      setOrders(page.items)
      setNextCursor(page.nextCursor)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : '')
    } finally {
      setBusyId(null)
    }
  }

  return (
    <AdminOrderTable
      orders={orders}
      loading={loading}
      loadingMore={loadingMore}
      hasMore={nextCursor !== null}
      busyId={busyId}
      currentStatus={status}
      error={error}
      onStatusFilterChange={(s) => setStatus(s)}
      onLoadMore={() => void loadMore()}
      onChangeStatus={(order, s) => void changeStatus(order, s)}
    />
  )
}
