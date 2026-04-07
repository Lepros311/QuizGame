import axios from 'axios'

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL, // Vite injects this at build time
  headers: { 'Content-Type': 'application/json' },
})

// Optional: log base URL once in dev so you know config loaded
if (import.meta.env.DEV) {
  console.log('API base URL:', import.meta.env.VITE_API_BASE_URL) // quick sanity check in browser console
}