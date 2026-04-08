<script setup lang="ts">
import { ref, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { useQuizStore } from '../stores/quiz'
import { useAuthStore } from '../stores/auth'
import { useCategoryStore } from '../stores/category'

const quiz = useQuizStore()
const auth = useAuthStore()
const categories = useCategoryStore()

const { token } = storeToRefs(auth)
const tokenInput = ref(token.value ?? '')

watch(token, (v) => {
  tokenInput.value = v ?? ''
})
</script>

<template>
  <div class="container py-4">
    <div class="row justify-content-center">
      <div class="col-12 col-md-10 col-lg-8">
        <div class="card shadow-sm">
          <div class="card-body p-4">
            <h1 class="display-6 mb-3">Quiz Game</h1>
            <p class="lead mb-4">
              Practice your knowledge with quick quizzes.
            </p>

            <div class="mb-4 p-3 border rounded">
              <label class="form-label">Dev JWT (paste from Swagger)</label>
              <textarea
                v-model="tokenInput"
                class="form-control mb-2"
                rows="2"
                placeholder="eyJhbGciOi..."
              />
              <div class="d-flex gap-2 flex-wrap">
                <button
                  type="button"
                  class="btn btn-sm btn-primary"
                  @click="auth.setToken(tokenInput)"
                >
                  Save token
                </button>
                <button
                  type="button"
                  class="btn btn-sm btn-outline-secondary"
                  @click="auth.clearToken()"
                >
                  Clear
                </button>
              </div>
            </div>

            <div class="mb-4">
              <button
                type="button"
                class="btn btn-outline-primary"
                :disabled="categories.loading"
                @click="categories.loadCategories()"
              >
                {{ categories.loading ? 'Loading…' : 'Load categories from API' }}
              </button>
              <ul
                v-if="categories.categories.length"
                class="list-group mt-3"
              >
                <li
                  v-for="c in categories.categories"
                  :key="c.id"
                  class="list-group-item"
                >
                  <strong>{{ c.name }}</strong>
                  <span class="text-muted"> — {{ c.description }}</span>
                </li>
              </ul>
            </div>

            <div class="d-grid gap-2 d-sm-flex">
              <button
                type="button"
                class="btn btn-outline-secondary"
                @click="quiz.demoToastFromAction()"
              >
                Test toast (Pinia action)
              </button>

              <RouterLink
                v-if="categories.categories.length"
                :to="{
                  name: 'quiz',
                  query: {
                    categoryId: categories.categories[0].id,
                  },
                }"
                class="btn btn-primary btn-lg"
              >
                Start Quiz
              </RouterLink>
              <button
                v-else
                type="button"
                class="btn btn-primary btn-lg"
                disabled
              >
                Start Quiz (load categories first)
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>