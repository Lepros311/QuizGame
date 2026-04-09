<script setup lang="ts">
import { storeToRefs } from 'pinia'
import { useQuizStore } from '../stores/quiz'
import { useAuthStore } from '../stores/auth'
import { useCategoryStore } from '../stores/category'

const quiz = useQuizStore()
const auth = useAuthStore()
const categories = useCategoryStore()

const { isLoggedIn } = storeToRefs(auth)
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

            <div v-if="!isLoggedIn" class="alert alert-info mb-4">
              Sign in to load categories and play a quiz.
              <div class="mt-2">
                <RouterLink class="btn btn-primary" :to="{ name: 'login' }">
                  Sign in
                </RouterLink>
              </div>
            </div>

            <div v-else class="mb-4">
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
                v-if="isLoggedIn && categories.categories.length"
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
                v-else-if="isLoggedIn"
                type="button"
                class="btn btn-primary btn-lg"
                disabled
              >
                Start Quiz (load categories first)
              </button>
              <button
                v-else
                type="button"
                class="btn btn-primary btn-lg"
                disabled
              >
                Start Quiz (sign in first)
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>