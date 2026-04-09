import axios from 'axios'

const baseURL = import.meta.env.VITE_API_BASE_URL

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
  console.log('API base URL:', import.meta.env.VITE_API_BASE_URL)
}