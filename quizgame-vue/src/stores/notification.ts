import axios from 'axios'
import { defineStore } from 'pinia'
import { fetchNotifications, markNotificationRead } from '../api/notification'
import type { NotificationDto } from '../types/api'
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

export const useNotificationStore = defineStore('notification', {
  state: () => ({
    items: [] as NotificationDto[],
    loading: false,
  }),
  getters: {
    unreadCount: (s) => s.items.filter((n) => !n.isRead).length,
  },
  actions: {
    async load() {
      const toast = useToastStore()
      this.loading = true
      try {
        this.items = await fetchNotifications()
      } catch (e) {
        toast.error(messageFromAxios(e))
        this.items = []
      } finally {
        this.loading = false
      }
    },

    async markRead(id: number) {
      const toast = useToastStore()
      try {
        await markNotificationRead(id)
        const n = this.items.find((x) => x.id === id)
        if (n) n.isRead = true
      } catch (e) {
        toast.error(messageFromAxios(e))
      }
    },
  },
})
