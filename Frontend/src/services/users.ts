import type { Page, UserResponse } from '../models/api'
import { client } from './client'

export interface UserListParams {
  cursor?: string | null
  pageSize?: number
}

export const usersApi = {
  list: (params: UserListParams = {}): Promise<Page<UserResponse>> => {
    const query = new URLSearchParams()
    if (params.cursor) query.set('cursor', params.cursor)
    if (params.pageSize !== undefined) query.set('pageSize', String(params.pageSize))
    const qs = query.toString()
    return client.get<Page<UserResponse>>(`/users${qs ? `?${qs}` : ''}`)
  },

  grantAdmin: (id: string): Promise<UserResponse> =>
    client.post<UserResponse>(`/users/${id}/admin`),

  revokeAdmin: (id: string): Promise<UserResponse> =>
    client.delete<UserResponse>(`/users/${id}/admin`),
}
