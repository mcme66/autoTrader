import axios, {
  type AxiosError,
  type AxiosInstance,
  type InternalAxiosRequestConfig,
} from 'axios'

import type { AuthenticationResponse, ProblemDetails } from '@/types/api'

/**
 * Relative by default so dev (Vite proxy) and production (nginx) are both same-origin. That
 * is what lets the refresh cookie be `SameSite=Strict` in both modes without special cases.
 */
const baseURL = import.meta.env.VITE_API_BASE_URL ?? '/api'

export const apiClient: AxiosInstance = axios.create({
  baseURL,
  withCredentials: true,
  headers: { 'Content-Type': 'application/json' },
})

/**
 * The access token lives in memory only.
 *
 * Putting it in `localStorage` would make it readable by any injected script and would
 * survive a tab close; the refresh cookie is httpOnly and restores the session on reload,
 * so there is nothing to gain from persisting it.
 */
let accessToken: string | null = null
let onUnauthenticated: (() => void) | null = null

export function setAccessToken(token: string | null): void {
  accessToken = token
}

export function getAccessToken(): string | null {
  return accessToken
}

/** Registers the callback that tears down client-side session state on a hard 401. */
export function setUnauthenticatedHandler(handler: (() => void) | null): void {
  onUnauthenticated = handler
}

apiClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  if (accessToken) {
    config.headers.set('Authorization', `Bearer ${accessToken}`)
  }
  return config
})

interface RetriableRequest extends InternalAxiosRequestConfig {
  _retried?: boolean
}

/**
 * Single in-flight refresh shared by every waiting request. Without this, a page that fires
 * six queries on mount would issue six refreshes, and because refresh tokens rotate, five of
 * them would present an already-revoked token and log the user out.
 */
let refreshPromise: Promise<string> | null = null

async function refreshAccessToken(): Promise<string> {
  refreshPromise ??= apiClient
    .post<AuthenticationResponse>('/auth/refresh')
    .then((response) => {
      accessToken = response.data.accessToken
      return response.data.accessToken
    })
    .finally(() => {
      refreshPromise = null
    })

  return refreshPromise
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError<ProblemDetails>) => {
    const request = error.config as RetriableRequest | undefined
    const isAuthEndpoint = request?.url?.startsWith('/auth/') ?? false

    if (
      error.response?.status !== 401 ||
      request === undefined ||
      request._retried ||
      isAuthEndpoint
    ) {
      return Promise.reject(error)
    }

    request._retried = true

    try {
      const token = await refreshAccessToken()
      request.headers.set('Authorization', `Bearer ${token}`)
      return await apiClient.request(request)
    } catch (refreshError) {
      accessToken = null
      onUnauthenticated?.()
      throw refreshError
    }
  },
)

/** A server-reported failure, already reduced to something renderable. */
export interface ApiError {
  status: number
  title: string
  detail?: string
  fieldErrors?: Record<string, string[]>
  traceId?: string
}

/**
 * Normalizes anything thrown by axios into a shape the UI can display, so no component ever
 * has to reach into `error.response.data` and guess at its shape.
 */
export function toApiError(error: unknown): ApiError {
  if (axios.isAxiosError<ProblemDetails>(error)) {
    const problem = error.response?.data

    if (error.response === undefined) {
      return {
        status: 0,
        title: 'Cannot reach the server',
        detail:
          'The request did not complete. Check that the API is running and try again.',
      }
    }

    return {
      status: error.response.status,
      title: problem?.title ?? error.response.statusText ?? 'Request failed',
      detail: problem?.detail,
      fieldErrors: problem?.errors,
      traceId: problem?.traceId,
    }
  }

  return {
    status: 0,
    title: 'Something went wrong',
    detail: error instanceof Error ? error.message : undefined,
  }
}

/** First human-readable line of an error, suitable for a toast or inline alert. */
export function errorMessage(error: unknown): string {
  const apiError = toApiError(error)
  const firstFieldError = apiError.fieldErrors
    ? Object.values(apiError.fieldErrors).flat()[0]
    : undefined

  return apiError.detail ?? firstFieldError ?? apiError.title
}
