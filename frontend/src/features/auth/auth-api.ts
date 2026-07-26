import { apiClient } from '@/lib/api-client'
import type { AuthenticatedUser, AuthenticationResponse } from '@/types/api'

export interface RegisterPayload {
  email: string
  password: string
  displayName: string
}

export interface LoginPayload {
  email: string
  password: string
}

export interface ChangePasswordPayload {
  currentPassword: string
  newPassword: string
}

export const authApi = {
  async register(payload: RegisterPayload): Promise<AuthenticationResponse> {
    const { data } = await apiClient.post<AuthenticationResponse>(
      '/auth/register',
      payload,
    )
    return data
  },

  async login(payload: LoginPayload): Promise<AuthenticationResponse> {
    const { data } = await apiClient.post<AuthenticationResponse>(
      '/auth/login',
      payload,
    )
    return data
  },

  /**
   * Sends no body: the refresh token is in an httpOnly cookie the browser attaches itself.
   */
  async refresh(): Promise<AuthenticationResponse> {
    const { data } = await apiClient.post<AuthenticationResponse>(
      '/auth/refresh',
    )
    return data
  },

  async logout(): Promise<void> {
    await apiClient.post('/auth/logout')
  },

  async changePassword(payload: ChangePasswordPayload): Promise<void> {
    await apiClient.post('/auth/change-password', payload)
  },

  async currentUser(): Promise<AuthenticatedUser> {
    const { data } = await apiClient.get<AuthenticatedUser>('/users/me')
    return data
  },

  async updateProfile(displayName: string): Promise<AuthenticatedUser> {
    const { data } = await apiClient.put<AuthenticatedUser>('/users/me', {
      displayName,
    })
    return data
  },
}
