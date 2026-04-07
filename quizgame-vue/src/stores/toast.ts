import { defineStore } from 'pinia'

export type ToastVariant = 'success' | 'danger' | 'info' | 'warning'

type ToastItem = {
  id: number
  message: string
  variant: ToastVariant
}

export const useToastStore = defineStore('toast', {
  state: () => ({
    toasts: [] as ToastItem[],
    nextId: 1,
  }),
  actions: {
    /** Internal: show a toast and auto-remove after a few seconds */
    push(message: string, variant: ToastVariant = 'info') {
      const id = this.nextId++
      this.toasts.push({ id, message, variant })
      window.setTimeout(() => this.remove(id), 4000)
    },
    remove(id: number) {
      this.toasts = this.toasts.filter((t) => t.id !== id)
    },
    success(message: string) {
      this.push(message, 'success')
    },
    error(message: string) {
      this.push(message, 'danger')
    },
    info(message: string) {
      this.push(message, 'info')
    },
    warning(message: string) {
      this.push(message, 'warning')
    },
  },
})