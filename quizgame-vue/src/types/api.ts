export type CategoryDto = {
  id: number
  name: string
  description: string
}

/** Matches backend QuestionDto; questionType is numeric enum in JSON */
export type QuestionDto = {
  id: number
  text: string
  questionType: number
  options: string[]
  userAnswer: string | null
  isCorrect: boolean | null
}

export type QuizDto = {
  id: number
  userId: string
  category: CategoryDto | null
  difficulty: number
  questionCount: number
  questionTypes: number[]
  isMultiplayer: boolean
  createdAt: string
  startedAt: string | null
  completedAt: string | null
  score: number
  questions: QuestionDto[]
}

/** Body for POST /api/quiz */
export type CreateQuizBody = {
  categoryId: number
  difficulty: number
  questionCount: number
  questionTypes: number[]
  isMultiplayer: boolean
}

/** Body for POST /api/quiz/{id}/submit */
export type SubmitAnswersBody = {
  answers: Record<number, string>
}