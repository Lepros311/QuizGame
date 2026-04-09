import { apiClient } from './client'
import type { StatBoardDto, UserStatBoardDto } from '../types/api'

export async function fetchStatBoards(): Promise<StatBoardDto[]> {
  const { data } = await apiClient.get<StatBoardDto[]>('/api/statboard')
  return data
}

export async function fetchStatBoard(id: number): Promise<StatBoardDto> {
  const { data } = await apiClient.get<StatBoardDto>(`/api/statboard/${id}`)
  return data
}

export async function fetchMyStats(): Promise<UserStatBoardDto> {
  const { data } = await apiClient.get<UserStatBoardDto>('/api/statboard/me')
  return data
}

export async function fetchGlobalRankings(
  statBoardId: number,
): Promise<UserStatBoardDto[]> {
  const { data } = await apiClient.get<UserStatBoardDto[]>(
    `/api/statboard/${statBoardId}/rankings`,
  )
  return data
}

export async function fetchFollowingRankings(
  statBoardId: number,
): Promise<UserStatBoardDto[]> {
  const { data } = await apiClient.get<UserStatBoardDto[]>(
    `/api/statboard/${statBoardId}/rankings/following`,
  )
  return data
}
