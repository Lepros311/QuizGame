import axios from 'axios'
import { defineStore } from 'pinia'
import { apiClient } from '../api/client'
import { loginRequest, registerRequest } from '../api/auth'
import { fetchCurrentUser } from '../api/user'
import { useToastStore } from './toast'

const STORAGE_KEY = 'quizgame.jwt'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: null as string | null,
    /** Display name from GET /api/user/me */
    username: null as string | null,
    userId: null as string | null,
  }),
  getters: {
    isLoggedIn: (s) => Boolean(s.token?.trim()),
  },
  actions: {
    loadTokenFromStorage() {
      this.token = localStorage.getItem(STORAGE_KEY)
      this.applyTokenToAxios()
    },

    applyTokenToAxios() {
      const t = this.token?.trim()
      if (t) {
        apiClient.defaults.headers.common.Authorization = `Bearer ${t}`
      } else {
        delete apiClient.defaults.headers.common.Authorization
      }
    },

    /** Still useful if you ever paste a token in dev tools */
    setToken(raw: string) {
      const trimmed = raw.trim()
      this.token = trimmed
      localStorage.setItem(STORAGE_KEY, trimmed)
      this.applyTokenToAxios()
    },

    clearToken() {
      this.token = null
      this.username = null
      this.userId = null
      localStorage.removeItem(STORAGE_KEY)
      delete apiClient.defaults.headers.common.Authorization
    },

    async fetchMe() {
      if (!this.token?.trim()) {
        this.username = null
        this.userId = null
        return
      }
      try {
        const me = await fetchCurrentUser()
        this.username = me.username
        this.userId = me.userId
      } catch {
        this.username = null
        this.userId = null
      }
    },

    /** Restore token from storage and load profile (call once at app startup). */
    async hydrateFromStorage() {
      this.loadTokenFromStorage()
      if (this.isLoggedIn) await this.fetchMe()
    },

    async login(email: string, password: string) {
      const toast = useToastStore()
      const result = await loginRequest(email.trim(), password)

      if (!result.succeeded || !result.token) {
        const msg =
          result.errors?.length > 0
            ? result.errors.join(' ')
            : 'Login failed.'
        toast.error(msg)
        throw new Error(msg)
      }

      this.setToken(result.token)
      await this.fetchMe()
    },

    async register(username: string, email: string, password: string) {
      const toast = useToastStore()
      try {
        const result = await registerRequest(
          username.trim(),
          email.trim(),
          password,
        )
        if (!result.succeeded || !result.token) {
          const msg =
            result.errors?.filter(Boolean).join(' ') || 'Registration failed.'
          toast.error(msg)
          throw new Error(msg)
        }
        this.setToken(result.token)
        await this.fetchMe()
        toast.success('Welcome! Your account is ready.')
      } catch (e) {
        if (axios.isAxiosError(e) && e.response?.status === 400) {
          const d = e.response.data
          const msg = Array.isArray(d)
            ? d.map(String).join(' ')
            : typeof d === 'string'
              ? d
              : 'Could not register.'
          toast.error(msg)
          throw new Error(msg)
        }
        throw e
      }
    },

    logout() {
      const toast = useToastStore()
      this.clearToken()
      toast.info('Signed out.')
    },
  },
})
