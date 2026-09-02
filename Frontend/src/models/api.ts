export type OrderStatus =
  | 'New'
  | 'InProgress'
  | 'PickedUp'
  | 'InTransit'
  | 'OutForDelivery'
  | 'Delivered'
  | 'Cancelled'

export interface AuthResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
}

export interface UserResponse {
  id: string
  userName: string
  isAdmin: boolean
}

export interface OrderResponse {
  id: string
  userId: string
  senderCity: string
  recipientCity: string
  senderAddress: string
  recipientAddress: string
  weight: number
  status: OrderStatus
  createdAt: string
}

export interface Page<T> {
  items: T[]
  nextCursor: string | null
}

export interface CreateOrderRequest {
  senderCity: string
  recipientCity: string
  senderAddress: string
  recipientAddress: string
  weight: number
}

export interface ChangeStatusRequest {
  status: OrderStatus
}

export interface ApiProblem {
  status?: number
  title?: string
  detail?: string
}
