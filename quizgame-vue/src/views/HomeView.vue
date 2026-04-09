<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { storeToRefs } from 'pinia'
import {
  QUIZ_MAX_QUESTION_COUNT,
  QUIZ_MIN_QUESTION_COUNT,
} from '../gameConstants'
import { useAuthStore } from '../stores/auth'
import { useCategoryStore } from '../stores/category'

const auth = useAuthStore()
const categories = useCategoryStore()

const { isLoggedIn } = storeToRefs(auth)

const selectedCategoryId = ref<number | null>(null)

const difficulty = ref(1)
const questionCount = ref(10)
const typeMultipleChoice = ref(true)
const typeTrueFalse = ref(false)
const typeShortAnswer = ref(false)
/** Radio group uses strings so values match native input behavior */
const playMode = ref<'solo' | 'multi'>('solo')

/** Loads when login state is known (immediate) and whenever auth flips */
watch(
  isLoggedIn,
  async (loggedIn) => {
    if (!loggedIn) {
      selectedCategoryId.value = null
      return
    }
    await categories.loadCategories()
    syncSelectedCategory()
  },
  { immediate: true },
)

function syncSelectedCategory() {
  const list = categories.categories
  if (!list.length) {
    selectedCategoryId.value = null
    return
  }
  if (
    selectedCategoryId.value == null ||
    !list.some((c) => c.id === selectedCategoryId.value)
  ) {
    selectedCategoryId.value = list[0]!.id
  }
}

const selectedCategory = computed(() =>
  categories.categories.find((c) => c.id === selectedCategoryId.value),
)

const selectedQuestionTypes = computed(() => {
  const t: number[] = []
  if (typeMultipleChoice.value) t.push(0)
  if (typeTrueFalse.value) t.push(1)
  if (typeShortAnswer.value) t.push(2)
  return t
})

const canStartQuiz = computed(
  () =>
    selectedCategoryId.value != null && selectedQuestionTypes.value.length > 0,
)

const quizLink = computed(() => ({
  name: 'quiz' as const,
  query: {
    categoryId: selectedCategoryId.value!,
    difficulty: difficulty.value,
    questionCount: questionCount.value,
    questionTypes: selectedQuestionTypes.value.join(','),
    multiplayer: playMode.value === 'multi' ? '1' : '0',
  },
}))
</script>

<template>
  <div class="container py-4">
    <div class="row justify-content-center">
      <div class="col-12 col-md-10 col-lg-8">
        <div class="card shadow-sm">
          <div class="card-body p-4">
            <h1 class="display-6 mb-3 app-page-title">Quiz Game</h1>
            <p class="lead mb-4 app-lead-tint">
              Practice your knowledge with quick quizzes, challenges, and
              leaderboards.
            </p>

            <div
              v-if="!isLoggedIn"
              class="alert alert-info mb-4 app-welcome-alert"
            >
              Sign in to load categories and play. New here?
              <RouterLink :to="{ name: 'register' }">Create an account</RouterLink>
              or
              <RouterLink :to="{ name: 'login' }">sign in</RouterLink>.
            </div>

            <div v-else class="mb-4">
              <div class="d-flex flex-wrap gap-2 align-items-end mb-3">
                <div class="flex-grow-1" style="min-width: 12rem">
                  <label class="form-label mb-0">Category</label>
                  <select
                    v-model.number="selectedCategoryId"
                    class="form-select"
                    :disabled="categories.loading || !categories.categories.length"
                  >
                    <option
                      v-for="c in categories.categories"
                      :key="c.id"
                      :value="c.id"
                    >
                      {{ c.name }}
                    </option>
                  </select>
                </div>
                <button
                  type="button"
                  class="btn btn-outline-secondary"
                  :disabled="categories.loading"
                  @click="categories.loadCategories()"
                >
                  {{ categories.loading ? 'Loading…' : 'Refresh' }}
                </button>
              </div>
              <p v-if="selectedCategory" class="text-muted small mb-3">
                {{ selectedCategory.description }}
              </p>

              <div class="app-quiz-options-panel p-3">
                <h2 class="h6 mb-3 app-section-heading">Quiz options</h2>
                <div class="row g-3">
                  <div class="col-sm-6">
                    <label class="form-label mb-0" for="home-difficulty"
                      >Difficulty</label
                    >
                    <select
                      id="home-difficulty"
                      v-model.number="difficulty"
                      class="form-select form-select-sm"
                    >
                      <option :value="0">Easy</option>
                      <option :value="1">Medium</option>
                      <option :value="2">Hard</option>
                    </select>
                  </div>
                  <div class="col-sm-6">
                    <label class="form-label mb-0" for="home-qcount"
                      >Number of questions</label
                    >
                    <input
                      id="home-qcount"
                      v-model.number="questionCount"
                      type="number"
                      :min="QUIZ_MIN_QUESTION_COUNT"
                      :max="QUIZ_MAX_QUESTION_COUNT"
                      class="form-control form-control-sm"
                    />
                    <span class="form-text small"
                      >API allows {{ QUIZ_MIN_QUESTION_COUNT }}–{{
                        QUIZ_MAX_QUESTION_COUNT
                      }}
                      questions.</span
                    >
                  </div>
                  <div class="col-12">
                    <span class="form-label d-block mb-1">Question types</span>
                    <div class="d-flex flex-wrap gap-3">
                      <div class="form-check">
                        <input
                          id="home-qt-mc"
                          v-model="typeMultipleChoice"
                          class="form-check-input"
                          type="checkbox"
                        />
                        <label class="form-check-label" for="home-qt-mc"
                          >Multiple choice</label
                        >
                      </div>
                      <div class="form-check">
                        <input
                          id="home-qt-tf"
                          v-model="typeTrueFalse"
                          class="form-check-input"
                          type="checkbox"
                        />
                        <label class="form-check-label" for="home-qt-tf"
                          >True / false</label
                        >
                      </div>
                      <div class="form-check">
                        <input
                          id="home-qt-sa"
                          v-model="typeShortAnswer"
                          class="form-check-input"
                          type="checkbox"
                        />
                        <label class="form-check-label" for="home-qt-sa"
                          >Short answer</label
                        >
                      </div>
                    </div>
                    <p
                      v-if="!selectedQuestionTypes.length"
                      class="text-warning small mb-0 mt-2"
                    >
                      Select at least one question type to start a quiz.
                    </p>
                  </div>
                  <div class="col-12">
                    <span class="form-label d-block mb-1">Mode</span>
                    <div class="d-flex flex-wrap gap-3">
                      <div class="form-check">
                        <input
                          id="home-mode-solo"
                          v-model="playMode"
                          class="form-check-input"
                          type="radio"
                          value="solo"
                        />
                        <label class="form-check-label" for="home-mode-solo"
                          >Solo</label
                        >
                      </div>
                      <div class="form-check">
                        <input
                          id="home-mode-multi"
                          v-model="playMode"
                          class="form-check-input"
                          type="radio"
                          value="multi"
                        />
                        <label class="form-check-label" for="home-mode-multi"
                          >Multiplayer</label
                        >
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div
              v-if="isLoggedIn"
              class="row row-cols-2 row-cols-sm-4 g-2 mb-4"
            >
              <div class="col">
                <RouterLink
                  class="btn btn-outline-primary w-100 app-quick-tile"
                  :to="{ name: 'stats' }"
                >
                  Stats
                </RouterLink>
              </div>
              <div class="col">
                <RouterLink
                  class="btn btn-outline-primary w-100 app-quick-tile"
                  :to="{ name: 'challenges' }"
                >
                  Challenges
                </RouterLink>
              </div>
              <div class="col">
                <RouterLink
                  class="btn btn-outline-primary w-100 app-quick-tile"
                  :to="{ name: 'notifications' }"
                >
                  Alerts
                </RouterLink>
              </div>
              <div class="col">
                <RouterLink
                  class="btn btn-outline-primary w-100 app-quick-tile"
                  :to="{ name: 'profile' }"
                >
                  Profile
                </RouterLink>
              </div>
            </div>

            <div class="d-grid gap-2 d-sm-flex">
              <RouterLink
                v-if="isLoggedIn && canStartQuiz"
                :to="quizLink"
                class="btn btn-primary btn-lg app-cta-primary fw-semibold"
              >
                Start Quiz
              </RouterLink>
              <button
                v-else-if="isLoggedIn && selectedCategoryId != null"
                type="button"
                class="btn btn-primary btn-lg"
                disabled
              >
                Start Quiz (pick question types)
              </button>
              <button
                v-else-if="isLoggedIn"
                type="button"
                class="btn btn-primary btn-lg"
                disabled
              >
                Start Quiz (loading categories…)
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
