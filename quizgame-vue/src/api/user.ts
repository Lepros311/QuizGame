import { apiClient } from './client'
import type { ApplicationUserSummaryDto } from '../types/api'
import type { UserProfileDto } from '../types/userProfile'

export async function fetchCurrentUser(): Promise<UserProfileDto> {
  const { data } = await apiClient.get<UserProfileDto>('/api/user/me')
  return data
}

export async function fetchPublicProfile(userId: string): Promise<UserProfileDto> {
  const { data } = await apiClient.get<UserProfileDto>(`/api/user/${userId}`)
  return data
}

export async function searchUsers(
  q: string,
): Promise<ApplicationUserSummaryDto[]> {
  const { data } = await apiClient.get<ApplicationUserSummaryDto[]>(
    '/api/user/search',
    { params: { q } },
  )
  return data
}

export async function fetchSuggestedMatches(): Promise<ApplicationUserSummaryDto[]> {
  const { data } = await apiClient.get<ApplicationUserSummaryDto[]>(
    '/api/user/suggested-matches',
  )
  return data
}

export async function followUser(userId: string): Promise<void> {
  await apiClient.post(`/api/user/${userId}/follow`, {})
}

export async function unfollowUser(userId: string): Promise<void> {
  await apiClient.delete(`/api/user/${userId}/follow`)
}
