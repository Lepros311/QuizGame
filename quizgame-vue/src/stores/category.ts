import { defineStore } from 'pinia'
import { fetchCategories } from '../api/categories'
import type { CategoryDto } from '../types/api'
import { useToastStore } from './toast'

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
        const msg =
          e && typeof e === 'object' && 'message' in e
            ? String((e as { message?: string }).message)
            : 'Failed to load categories'
        toast.error(msg)
        this.categories = []
      } finally {
        this.loading = false
      }
    },
  },
})