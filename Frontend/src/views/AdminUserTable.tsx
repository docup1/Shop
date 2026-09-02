import { useTranslation } from '../i18n/context'
import type { UserResponse } from '../models/api'
import { Spinner } from './Spinner'

interface AdminUserTableProps {
  users: UserResponse[]
  loading: boolean
  busyId: string | null
  error: string | null
  onToggleAdmin: (user: UserResponse) => void
}

export function AdminUserTable({
  users,
  loading,
  busyId,
  error,
  onToggleAdmin,
}: AdminUserTableProps): React.JSX.Element {
  const { t } = useTranslation()

  if (loading) return <Spinner />
  if (error) return <p className="form-error" role="alert">{error}</p>

  return (
    <table className="table">
      <thead>
        <tr>
          <th>{t('admin.users.userName')}</th>
          <th>{t('admin.users.isAdmin')}</th>
          <th>{t('admin.users.id')}</th>
          <th />
        </tr>
      </thead>
      <tbody>
        {users.map((user) => {
          const busy = busyId === user.id
          return (
            <tr key={user.id}>
              <td>{user.userName}</td>
              <td>{user.isAdmin ? '✓' : '—'}</td>
              <td className="table__mono">{user.id.slice(0, 8)}</td>
              <td>
                {user.isAdmin ? (
                  <button className="btn btn--danger" type="button" disabled={busy} onClick={() => onToggleAdmin(user)}>
                    {busy ? t('common.loading') : t('admin.users.demote')}
                  </button>
                ) : (
                  <button className="btn btn--secondary" type="button" disabled={busy} onClick={() => onToggleAdmin(user)}>
                    {busy ? t('common.loading') : t('admin.users.promote')}
                  </button>
                )}
              </td>
            </tr>
          )
        })}
      </tbody>
    </table>
  )
}
