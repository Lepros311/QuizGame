import axios from 'axios'

/** Trailing slash breaks axios baseURL + '/api/...' resolution in some cases */
const raw = import.meta.env.VITE_API_BASE_URL as string | undefined
const baseURL = raw?.trim().replace(/\/$/, '') ?? ''

if (!baseURL && import.meta.env.DEV) {
  console.error(
    '[QuizGame] VITE_API_BASE_URL is missing. Add quizgame-vue/.env.development with e.g. VITE_API_BASE_URL=https://localhost:7104',
  )
}

export const apiClient = axios.create({
  baseURL,
  headers: { 'Content-Type': 'application/json' },
})

/** Same base URL, no default Authorization — use for /api/auth/login and /api/auth/register */
export const publicApi = axios.create({
  baseURL,
  headers: { 'Content-Type': 'application/json' },
})

if (import.meta.env.DEV) {
  console.log(
    'API base URL:',
    baseURL || '(empty — requests will hit the dev server origin)',
  )
}
