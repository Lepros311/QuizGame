<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { storeToRefs } from 'pinia'
import ToastHost from './components/ToastHost.vue'
import { useAuthStore } from './stores/auth'

const route = useRoute()
const auth = useAuthStore()
const { isLoggedIn, username } = storeToRefs(auth)

const showSignInInNav = computed(
  () => !isLoggedIn.value && route.name !== 'login',
)

const avatarInitials = computed(() => {
  const u = username.value?.trim()
  if (!u) return '?'
  const parts = u.split(/\s+/).filter(Boolean)
  if (parts.length >= 2) {
    return (parts[0]![0]! + parts[1]![0]!).toUpperCase()
  }
  return u.slice(0, 2).toUpperCase()
})

function logout() {
  auth.logout()
}
</script>

<template>
  <header class="border-bottom bg-light">
    <nav class="navbar navbar-expand container">
      <RouterLink to="/" class="navbar-brand fw-bold">
        QuizGame Vue
      </RouterLink>
      <div class="ms-auto d-flex gap-2 align-items-center">
        <template v-if="isLoggedIn">
          <div class="d-flex align-items-center gap-2">
            <span
              class="rounded-circle bg-primary text-white d-inline-flex align-items-center justify-content-center user-select-none"
              style="width: 2rem; height: 2rem; font-size: 0.75rem"
              :title="username ?? 'Profile'"
              aria-hidden="true"
            >
              {{ avatarInitials }}
            </span>
            <span
              class="text-muted small d-none d-sm-inline text-truncate"
              style="max-width: 10rem"
            >
              {{ username ?? '…' }}
            </span>
            <button
              type="button"
              class="btn btn-sm btn-outline-secondary"
              @click="logout"
            >
              Sign out
            </button>
          </div>
        </template>
        <RouterLink
          v-else-if="showSignInInNav"
          class="btn btn-sm btn-primary"
          :to="{ name: 'login' }"
        >
          Sign in
        </RouterLink>
      </div>
    </nav>
  </header>

  <main>
    <RouterView />
  </main>

  <ToastHost />
</template>
