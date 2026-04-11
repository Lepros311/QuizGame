import axios, { type AxiosError } from 'axios'
import type { Pinia } from 'pinia'
import { useAuthStore } from '../stores/auth'
import { apiClient } from './client'

let attached = false

function appPathname(pathname: string): string {
  const base = import.meta.env.BASE_URL.replace(/\/$/, '')
  if (!base) return pathname || '/'
  if (pathname.startsWith(base)) {
    const rest = pathname.slice(base.length)
    return rest ? (rest.startsWith('/') ? rest : `/${rest}`) : '/'
  }
  return pathname
}

function isLoginOrRegisterRoute(): boolean {
  const rel = appPathname(window.location.pathname)
  return rel === '/login' || rel === '/register'
}

function loginPageUrl(redirectFullPath: string): string {
  const base = import.meta.env.BASE_URL.replace(/\/$/, '')
  const loginPath = (base ? `${base}/login` : '/login').replace(/\/+/g, '/')
  const q = new URLSearchParams({ redirect: redirectFullPath })
  return `${loginPath}?${q.toString()}`
}

/**
 * On 401 from the authenticated API client, clear the session and hard-navigate
 * to login so the app reloads with a fresh shell (expired / invalid JWT).
 */
export function attachUnauthorizedRedirect(pinia: Pinia): void {
  if (attached) return
  attached = true

  let redirectPending = false

  apiClient.interceptors.response.use(
    (response) => response,
    (error: AxiosError) => {
      if (!axios.isAxiosError(error) || error.response?.status !== 401) {
        return Promise.reject(error)
      }
      if (redirectPending) return Promise.reject(error)
      if (isLoginOrRegisterRoute()) return Promise.reject(error)

      redirectPending = true
      const auth = useAuthStore(pinia)
      auth.clearToken()

      const fullPath =
        window.location.pathname +
        window.location.search +
        window.location.hash
      window.location.assign(loginPageUrl(fullPath))
      return Promise.reject(error)
    },
  )
}
