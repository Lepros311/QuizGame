import { defineStore } from 'pinia'
import { apiClient } from '../api/client'

const STORAGE_KEY = 'quizgame.jwt' // localStorage key for pasted token

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: null as string | null,
  }),
  actions: {
    loadTokenFromStorage() {
      this.token = localStorage.getItem(STORAGE_KEY) // restore after refresh
    },
    setToken(raw: string) {
      const trimmed = raw.trim()
      this.token = trimmed
      localStorage.setItem(STORAGE_KEY, trimmed) // persist for dev convenience
      apiClient.defaults.headers.common.Authorization = trimmed
        ? `Bearer ${trimmed}`
        : '' // every axios request gets the header
    },
    clearToken() {
      this.token = null
      localStorage.removeItem(STORAGE_KEY)
      delete apiClient.defaults.headers.common.Authorization
    },
  },
})