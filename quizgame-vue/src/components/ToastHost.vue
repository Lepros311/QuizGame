<script setup lang="ts">
import { storeToRefs } from 'pinia'
import { useToastStore } from '../stores/toast'

const toastStore = useToastStore()
const { toasts } = storeToRefs(toastStore)

function variantClass(variant: string) {
  if (variant === 'success') return 'text-bg-success'
  if (variant === 'danger') return 'text-bg-danger'
  if (variant === 'warning') return 'text-bg-warning'
  return 'text-bg-info'
}
</script>

<template>
  <div
    class="position-fixed bottom-0 start-50 translate-middle-x p-3"
    style="z-index: 1080; width: min(100% - 1.5rem, 28rem)"
    aria-live="polite"
    aria-atomic="true"
  >
    <div class="d-flex flex-column-reverse align-items-stretch gap-2">
      <div
        v-for="t in toasts"
        :key="t.id"
        class="toast show w-100"
        style="max-width: min(100%, 28rem)"
        role="alert"
      >
        <div class="toast-header" :class="variantClass(t.variant)">
          <strong class="me-auto">Notice</strong>
          <button
            type="button"
            class="btn-close btn-close-white"
            aria-label="Close"
            @click="toastStore.remove(t.id)"
          />
        </div>
        <div class="toast-body">
          {{ t.message }}
        </div>
      </div>
    </div>
  </div>
</template>