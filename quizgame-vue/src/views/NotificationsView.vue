<script setup lang="ts">
import { onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { useNotificationStore } from '../stores/notification'

const notifications = useNotificationStore()
const { items, loading } = storeToRefs(notifications)

onMounted(() => {
  void notifications.load()
})

function formatDate(iso: string) {
  try {
    return new Date(iso).toLocaleString()
  } catch {
    return iso
  }
}
</script>

<template>
  <div class="container py-4">
    <h1 class="h3 mb-4">Notifications</h1>

    <div class="card shadow-sm">
      <div class="card-body p-0">
        <div
          v-if="loading"
          class="p-4 d-flex align-items-center gap-2 text-muted"
        >
          <div class="spinner-border spinner-border-sm" role="status" />
          <span>Loading…</span>
        </div>
        <ul v-else-if="items.length" class="list-group list-group-flush">
          <li
            v-for="n in items"
            :key="n.id"
            class="list-group-item d-flex flex-wrap justify-content-between gap-2 align-items-start"
            :class="{ 'bg-light': !n.isRead }"
          >
            <div>
              <div>{{ n.message }}</div>
              <div class="small text-muted">{{ formatDate(n.createdAt) }}</div>
              <RouterLink
                v-if="n.challengeId != null"
                :to="{
                  name: 'challenge-play',
                  params: { id: n.challengeId },
                }"
                class="small"
              >
                Open challenge
              </RouterLink>
            </div>
            <button
              v-if="!n.isRead"
              type="button"
              class="btn btn-sm btn-outline-secondary"
              @click="notifications.markRead(n.id)"
            >
              Mark read
            </button>
          </li>
        </ul>
        <p v-else class="p-4 text-muted mb-0">No notifications.</p>
      </div>
    </div>

    <div class="mt-3">
      <RouterLink to="/" class="btn btn-outline-secondary">Home</RouterLink>
    </div>
  </div>
</template>
