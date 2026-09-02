import type { AuthResponse, UserResponse } from '../models/api'
import { client } from './client'

export interface RegisterPayload {
  userName: string
  password: string
}

export interface LoginPayload {
  userName: string
  password: string
}

export const authApi = {
  register: (payload: RegisterPayload): Promise<AuthResponse> =>
    client.post<AuthResponse>('/auth/register', payload),

  login: (payload: LoginPayload): Promise<AuthResponse> =>
    client.post<AuthResponse>('/auth/login', payload),

  refresh: (refreshToken: string): Promise<AuthResponse> =>
    client.post<AuthResponse>('/auth/refresh', { refreshToken }),

  logout: (refreshToken: string): Promise<void> =>
    client.post<void>('/auth/logout', { refreshToken }),

  me: (): Promise<UserResponse> => client.get<UserResponse>('/auth/me'),
}
