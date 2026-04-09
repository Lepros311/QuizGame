import { apiClient } from './client'
import type { ChallengeDto, CreateChallengeBody, SubmitAnswersBody } from '../types/api'

export async function fetchChallenges(): Promise<ChallengeDto[]> {
  const { data } = await apiClient.get<ChallengeDto[]>('/api/challenge')
  return data
}

export async function fetchChallenge(id: number): Promise<ChallengeDto> {
  const { data } = await apiClient.get<ChallengeDto>(`/api/challenge/${id}`)
  return data
}

export async function createChallenge(
  body: CreateChallengeBody,
): Promise<ChallengeDto> {
  const { data } = await apiClient.post<ChallengeDto>('/api/challenge', body)
  return data
}

export async function acceptChallenge(id: number): Promise<ChallengeDto> {
  const { data } = await apiClient.patch<ChallengeDto>(
    `/api/challenge/${id}/accept`,
  )
  return data
}

export async function declineChallenge(id: number): Promise<ChallengeDto> {
  const { data } = await apiClient.patch<ChallengeDto>(
    `/api/challenge/${id}/decline`,
  )
  return data
}

export async function submitChallengeAnswers(
  challengeId: number,
  body: SubmitAnswersBody,
): Promise<ChallengeDto> {
  const { data } = await apiClient.post<ChallengeDto>(
    `/api/challenge/${challengeId}/submit`,
    body,
  )
  return data
}
