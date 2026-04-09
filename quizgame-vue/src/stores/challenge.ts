import axios from 'axios'
import { defineStore } from 'pinia'
import {
  acceptChallenge,
  createChallenge,
  declineChallenge,
  fetchChallenges,
} from '../api/challenge'
import type { ChallengeDto, CreateChallengeBody } from '../types/api'
import { useToastStore } from './toast'

function messageFromAxios(e: unknown): string {
  if (axios.isAxiosError(e)) {
    const d = e.response?.data
    if (typeof d === 'string' && d.trim()) return d
    if (Array.isArray(d)) return d.map(String).join(' ')
    return e.message
  }
  return e instanceof Error ? e.message : 'Something went wrong'
}

export const useChallengeStore = defineStore('challenge', {
  state: () => ({
    list: [] as ChallengeDto[],
    loading: false,
  }),
  actions: {
    async loadAll() {
      const toast = useToastStore()
      this.loading = true
      try {
        this.list = await fetchChallenges()
      } catch (e) {
        toast.error(messageFromAxios(e))
        this.list = []
      } finally {
        this.loading = false
      }
    },

    async accept(id: number) {
      const toast = useToastStore()
      try {
        const updated = await acceptChallenge(id)
        const i = this.list.findIndex((c) => c.id === id)
        if (i >= 0) this.list[i] = updated
        else this.list.unshift(updated)
        toast.success('Challenge accepted.')
      } catch (e) {
        toast.error(messageFromAxios(e))
      }
    },

    async decline(id: number) {
      const toast = useToastStore()
      try {
        const updated = await declineChallenge(id)
        const i = this.list.findIndex((c) => c.id === id)
        if (i >= 0) this.list[i] = updated
        toast.info('Challenge declined.')
      } catch (e) {
        toast.error(messageFromAxios(e))
      }
    },

    async create(body: CreateChallengeBody) {
      const toast = useToastStore()
      try {
        const c = await createChallenge(body)
        this.list.unshift(c)
        toast.success('Challenge sent.')
        return c
      } catch (e) {
        toast.error(messageFromAxios(e))
        return null
      }
    },
  },
})
