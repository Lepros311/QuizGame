<script setup lang="ts">
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()

const email = ref('')
const password = ref('')
const busy = ref(false)

async function onSubmit() {
  busy.value = true
  try {
    await auth.login(email.value, password.value)
    const redirect =
      typeof route.query.redirect === 'string' ? route.query.redirect : '/'
    await router.replace(redirect || '/')
  } catch {
    /* toast already shown in store */
  } finally {
    busy.value = false
  }
}
</script>

<template>
  <div class="container py-5">
    <div class="row justify-content-center">
      <div class="col-12 col-sm-10 col-md-8 col-lg-5">
        <div class="card shadow-sm">
          <div class="card-body p-4">
            <h1 class="h3 mb-4">Sign in</h1>
            <form @submit.prevent="onSubmit">
              <div class="mb-3">
                <label class="form-label" for="login-email">Email</label>
                <input
                  id="login-email"
                  v-model="email"
                  type="email"
                  class="form-control"
                  autocomplete="username"
                  required
                />
              </div>
              <div class="mb-3">
                <label class="form-label" for="login-password">Password</label>
                <input
                  id="login-password"
                  v-model="password"
                  type="password"
                  class="form-control"
                  autocomplete="current-password"
                  required
                />
              </div>
              <button
                type="submit"
                class="btn btn-primary w-100"
                :disabled="busy"
              >
                <span
                  v-if="busy"
                  class="spinner-border spinner-border-sm me-2"
                  role="status"
                  aria-hidden="true"
                />
                {{ busy ? 'Signing in…' : 'Sign in' }}
              </button>
            </form>
            <p class="mt-3 mb-0 text-muted small">
              <RouterLink to="/">Back to home</RouterLink>
            </p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>