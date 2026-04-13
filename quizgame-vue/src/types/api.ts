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
  /** Present after grading when the API includes it for review */
  correctAnswer?: string | null
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

export type ChallengeParticipantDto = {
  id: number
  userId: string
  username: string
  status: number
  score: number | null
  completedAt: string | null
}

export type ChallengeDto = {
  id: number
  challengerId: string
  challengerUsername: string
  quiz: QuizDto | null
  status: number
  createdAt: string
  completedAt: string | null
  participants: ChallengeParticipantDto[]
}

export type NotificationDto = {
  id: number
  message: string
  isRead: boolean
  createdAt: string
  challengeId: number | null
}

export type StatBoardDto = {
  id: number
  name: string
  description: string
}

export type UserStatBoardDto = {
  userId: string
  username: string
  totalQuizzesCompleted: number
  totalCorrectAnswers: number
  totalWrongAnswers: number
  averageScorePercentage: number
  highestScore: number
  totalChallengesSent: number
  totalChallengesReceived: number
  totalChallengesWon: number
  totalChallengesLost: number
  fastestCompletionSeconds: number
  averageCompletionSeconds: number
  currentWinStreak: number
  longestWinStreak: number
  bestCategory: string
  mostPlayedCategory: string
  skillScore: number
  skillScoreConfidence: number
  lastUpdated: string
}

export type CreateChallengeBody = {
  categoryId: number
  difficulty: number
  questionCount: number
  questionTypes: number[]
  opponentIds: string[]
}

/** Matches ApplicationUserDto from search/suggested endpoints */
export type ApplicationUserSummaryDto = {
  id: string
  username: string
  createdAt: string
  skillScore: number
  followersCount: number
  followingCount: number
}