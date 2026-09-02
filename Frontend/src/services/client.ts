import type { ApiProblem } from '../models/api'

export class ApiError extends Error {
  readonly status: number
  readonly problem?: ApiProblem

  constructor(status: number, problem?: ApiProblem) {
    super(problem?.detail ?? problem?.title ?? `Request failed with status ${status}`)
    this.name = 'ApiError'
    this.status = status
    this.problem = problem
  }
}

let accessToken: string | null = null
let onUnauthorized: (() => void) | null = null

export function setAccessToken(token: string | null): void {
  accessToken = token
}

/** Регистрирует обработчик, вызываемый при 401 (например, для разлогина). */
export function setOnUnauthorized(handler: (() => void) | null): void {
  onUnauthorized = handler
}

async function parseProblem(response: Response): Promise<ApiError> {
  let problem: ApiProblem | undefined
  try {
    problem = (await response.json()) as ApiProblem
  } catch {
    // тело не в JSON — используем заголовки
  }
  return new ApiError(response.status, problem)
}

async function request<T>(
  path: string,
  init: RequestInit = {},
): Promise<T> {
  const headers = new Headers(init.headers)
  headers.set('Accept', 'application/json')
  if (init.body !== undefined) {
    headers.set('Content-Type', 'application/json')
  }
  if (accessToken) {
    headers.set('Authorization', `Bearer ${accessToken}`)
  }

  const response = await fetch(`/api${path}`, { ...init, headers })

  if (!response.ok) {
    const error = await parseProblem(response)
    if (response.status === 401 && onUnauthorized) {
      onUnauthorized()
    }
    throw error
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

export const client = {
  get: <T>(path: string): Promise<T> => request<T>(path),
  post: <T>(path: string, body?: unknown): Promise<T> =>
    request<T>(path, { method: 'POST', body: body === undefined ? undefined : JSON.stringify(body) }),
  patch: <T>(path: string, body: unknown): Promise<T> =>
    request<T>(path, { method: 'PATCH', body: JSON.stringify(body) }),
  delete: <T>(path: string): Promise<T> => request<T>(path, { method: 'DELETE' }),
}
