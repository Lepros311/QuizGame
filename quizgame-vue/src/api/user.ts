import { apiClient } from './client'
import type { UserProfileDto } from '../types/userProfile'

export async function fetchCurrentUser(): Promise<UserProfileDto> {
  const { data } = await apiClient.get<UserProfileDto>('/api/user/me')
  return data
}
