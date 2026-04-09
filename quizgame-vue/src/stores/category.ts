import axios from 'axios'
import { defineStore } from 'pinia'
import { fetchCategories } from '../api/categories'
import type { CategoryDto } from '../types/api'
import { useToastStore } from './toast'

function messageFromAxios(e: unknown): string {
  if (axios.isAxiosError(e)) {
    const d = e.response?.data
    if (typeof d === 'string' && d.trim()) return d
    if (Array.isArray(d)) return d.map(String).join(' ')
    if (e.response?.status === 401)
      return 'Not authorized — try signing in again.'
    if (e.response?.status === 404)
      return 'Categories API not found — check VITE_API_BASE_URL (use https://localhost:7104 for the dev API).'
    return e.message
  }
  return e instanceof Error ? e.message : 'Failed to load categories'
}

export const useCategoryStore = defineStore('category', {
  state: () => ({
    categories: [] as CategoryDto[],
    loading: false,
  }),
  actions: {
    async loadCategories() {
      const toast = useToastStore()
      this.loading = true
      try {
        this.categories = await fetchCategories()
        toast.success(`Loaded ${this.categories.length} categories`)
      } catch (e: unknown) {
        toast.error(messageFromAxios(e))
        this.categories = []
      } finally {
        this.loading = false
      }
    },
  },
})