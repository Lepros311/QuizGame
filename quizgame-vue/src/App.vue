<script setup lang="ts">
import { Dropdown } from 'bootstrap'
import { computed, nextTick, onBeforeUnmount, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { storeToRefs } from 'pinia'
import ToastHost from './components/ToastHost.vue'
import { useAuthStore } from './stores/auth'
import { useNotificationStore } from './stores/notification'

const route = useRoute()
const auth = useAuthStore()
const notifications = useNotificationStore()
const { isLoggedIn, username, email } = storeToRefs(auth)
const { unreadCount } = storeToRefs(notifications)

const userMenuToggleRef = ref<HTMLButtonElement | null>(null)

const menuDisplayName = computed(
  () => username.value?.trim() || 'User',
)
const menuEmail = computed(() => email.value?.trim() || '')

watch(
  isLoggedIn,
  (loggedIn) => {
    if (loggedIn) void notifications.load()
    else notifications.items = []
  },
  { immediate: true },
)

/** Bootstrap Dropdown must be created after the toggle is in the DOM (Vue SPA). */
watch(
  isLoggedIn,
  async (loggedIn) => {
    const el = userMenuToggleRef.value
    if (el) Dropdown.getInstance(el)?.dispose()
    if (!loggedIn) return
    await nextTick()
    const btn = userMenuToggleRef.value
    if (btn) Dropdown.getOrCreateInstance(btn)
  },
  { immediate: true },
)

onBeforeUnmount(() => {
  const btn = userMenuToggleRef.value
  if (btn) Dropdown.getInstance(btn)?.dispose()
})

const showSignInInNav = computed(
  () =>
    !isLoggedIn.value &&
    route.name !== 'login' &&
    route.name !== 'register',
)

const avatarInitials = computed(() => {
  const u = username.value?.trim()
  if (!u) return '?'
  return u[0]!.toUpperCase()
})

function hideUserDropdown() {
  const el = userMenuToggleRef.value
  if (el) Dropdown.getInstance(el)?.hide()
}

function logout() {
  auth.logout()
}

function onSignOutClick() {
  hideUserDropdown()
  logout()
}
</script>

<template>
  <header class="app-site-header">
    <nav class="navbar container py-2 px-3">
      <div
        class="d-flex align-items-center justify-content-between w-100 gap-3"
      >
        <div
          class="d-flex align-items-center gap-2 gap-sm-3 min-w-0 flex-nowrap"
        >
          <RouterLink
            to="/"
            class="navbar-brand fw-bold mb-0 py-0 text-nowrap flex-shrink-0"
          >
            Quiz Game
          </RouterLink>
          <div
            v-if="isLoggedIn"
            class="d-flex align-items-center gap-1 flex-shrink-0"
          >
            <RouterLink
              class="btn btn-sm btn-link py-1 text-nowrap"
              :to="{ name: 'stats' }"
            >
              Stats
            </RouterLink>
            <RouterLink
              class="btn btn-sm btn-link py-1 text-nowrap"
              :to="{ name: 'challenges' }"
            >
              Challenges
            </RouterLink>
            <RouterLink
              class="btn btn-sm btn-link py-1 text-nowrap"
              :to="{ name: 'notifications' }"
            >
              Alerts
              <span
                v-if="unreadCount > 0"
                class="badge rounded-pill text-bg-danger ms-1"
              >
                {{ unreadCount }}
              </span>
            </RouterLink>
          </div>
        </div>

        <div v-if="isLoggedIn" class="dropdown flex-shrink-0">
          <button
            id="navbar-user-menu"
            ref="userMenuToggleRef"
            type="button"
            class="btn user-menu-btn rounded-circle d-inline-flex align-items-center justify-content-center fw-semibold"
            data-bs-toggle="dropdown"
            data-bs-auto-close="true"
            aria-expanded="false"
            aria-haspopup="true"
            aria-label="Account menu"
          >
            <span class="user-select-none lh-1">{{ avatarInitials }}</span>
          </button>
          <ul
            class="dropdown-menu dropdown-menu-end shadow user-menu-dropdown"
            aria-labelledby="navbar-user-menu"
          >
            <li class="px-3 pt-3 pb-2" role="presentation">
              <div class="user-menu-identity">
                <div class="fw-semibold text-truncate">{{ menuDisplayName }}</div>
                <div
                  v-if="menuEmail"
                  class="small text-secondary text-truncate user-menu-email"
                >
                  {{ menuEmail }}
                </div>
              </div>
            </li>
            <li><hr class="dropdown-divider my-0" /></li>
            <li>
              <RouterLink
                class="dropdown-item"
                :to="{ name: 'profile' }"
                @click="hideUserDropdown"
              >
                Profile
              </RouterLink>
            </li>
            <li><hr class="dropdown-divider" /></li>
            <li>
              <button
                type="button"
                class="dropdown-item"
                @click="onSignOutClick"
              >
                Sign out
              </button>
            </li>
          </ul>
        </div>
        <div
          v-else-if="showSignInInNav"
          class="d-flex align-items-center gap-2 flex-shrink-0"
        >
          <RouterLink
            class="btn btn-sm btn-outline-primary"
            :to="{ name: 'register' }"
          >
            Register
          </RouterLink>
          <RouterLink
            class="btn btn-sm btn-primary fw-semibold"
            :to="{ name: 'login' }"
          >
            Sign in
          </RouterLink>
        </div>
      </div>
    </nav>
  </header>

  <main class="app-main">
    <RouterView />
  </main>

  <ToastHost />
</template>

<style scoped>
.user-menu-btn {
  width: 2.5rem;
  height: 2.5rem;
  padding: 0;
  font-size: 0.8125rem;
  box-shadow: 0 0 0 1px rgba(0, 0, 0, 0.08);
}

.user-menu-dropdown {
  min-width: 12.5rem;
  max-width: min(20rem, 92vw);
}

.user-menu-identity {
  max-width: 100%;
}

.user-menu-email {
  margin-top: 0.2rem;
  line-height: 1.35;
}
</style>
