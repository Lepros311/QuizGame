<script setup lang="ts">
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()

const username = ref('')
const email = ref('')
const password = ref('')
const busy = ref(false)

async function onSubmit() {
  busy.value = true
  try {
    await auth.register(username.value, email.value, password.value)
    const redirect =
      typeof route.query.redirect === 'string' ? route.query.redirect : '/'
    await router.replace(redirect || '/')
  } catch {
    /* toast in store */
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
            <h1 class="h3 mb-4 app-section-heading">Create account</h1>
            <form @submit.prevent="onSubmit">
              <div class="mb-3">
                <label class="form-label" for="reg-username">Username</label>
                <input
                  id="reg-username"
                  v-model="username"
                  type="text"
                  class="form-control"
                  autocomplete="username"
                  required
                />
              </div>
              <div class="mb-3">
                <label class="form-label" for="reg-email">Email</label>
                <input
                  id="reg-email"
                  v-model="email"
                  type="email"
                  class="form-control"
                  autocomplete="email"
                  required
                />
              </div>
              <div class="mb-3">
                <label class="form-label" for="reg-password">Password</label>
                <input
                  id="reg-password"
                  v-model="password"
                  type="password"
                  class="form-control"
                  autocomplete="new-password"
                  required
                />
              </div>
              <button
                type="submit"
                class="btn btn-primary w-100 app-cta-primary fw-semibold"
                :disabled="busy"
              >
                <span
                  v-if="busy"
                  class="spinner-border spinner-border-sm me-2"
                  role="status"
                  aria-hidden="true"
                />
                {{ busy ? 'Creating…' : 'Register' }}
              </button>
            </form>
            <p class="mt-3 mb-0 text-muted small">
              Already have an account?
              <RouterLink :to="{ name: 'login' }">Sign in</RouterLink>
            </p>
            <p class="mt-2 mb-0 text-muted small">
              <RouterLink to="/">Back to home</RouterLink>
            </p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
