import { apiClient } from './client'
import type { CreateQuizBody, QuizDto, SubmitAnswersBody } from '../types/api'

export async function createQuiz(body: CreateQuizBody): Promise<QuizDto> {
  const { data } = await apiClient.post<QuizDto>('/api/quiz', body)
  return data
}

export async function getQuiz(quizId: number): Promise<QuizDto> {
  const { data } = await apiClient.get<QuizDto>(`/api/quiz/${quizId}`)
  return data
}

export async function submitQuizAnswers(
  quizId: number,
  body: SubmitAnswersBody,
): Promise<QuizDto> {
  const { data } = await apiClient.post<QuizDto>(
    `/api/quiz/${quizId}/submit`,
    body,
  )
  return data
}