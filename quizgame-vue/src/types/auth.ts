/** Mirrors QuizGame.Core.Models.AuthResult (JSON camelCase) */
export type AuthResultDto = {
  succeeded: boolean
  token: string | null
  errors: string[]
}