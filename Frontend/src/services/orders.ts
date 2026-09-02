import type {
  ChangeStatusRequest,
  CreateOrderRequest,
  OrderResponse,
  OrderStatus,
  Page,
} from '../models/api'
import { client } from './client'

export interface OrderListParams {
  cursor?: string | null
  pageSize?: number
  status?: OrderStatus | null
}

export const ordersApi = {
  create: (payload: CreateOrderRequest): Promise<OrderResponse> =>
    client.post<OrderResponse>('/orders', payload),

  list: (params: OrderListParams = {}): Promise<Page<OrderResponse>> => {
    const query = new URLSearchParams()
    if (params.cursor) query.set('cursor', params.cursor)
    if (params.pageSize !== undefined) query.set('pageSize', String(params.pageSize))
    if (params.status) query.set('status', params.status)
    const qs = query.toString()
    return client.get<Page<OrderResponse>>(`/orders${qs ? `?${qs}` : ''}`)
  },

  getById: (id: string): Promise<OrderResponse> =>
    client.get<OrderResponse>(`/orders/${id}`),

  changeStatus: (id: string, payload: ChangeStatusRequest): Promise<OrderResponse> =>
    client.patch<OrderResponse>(`/orders/${id}/status`, payload),
}
