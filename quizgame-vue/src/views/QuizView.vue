<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { storeToRefs } from 'pinia'
import { useQuizStore } from '../stores/quiz'
import { useCategoryStore } from '../stores/category'
import { useToastStore } from '../stores/toast'

const route = useRoute()
const quiz = useQuizStore()
const categories = useCategoryStore()
const toast = useToastStore()

const { phase, currentQuestion, answerDraft } = storeToRefs(quiz)

const categoryId = computed(() => {
  const raw = route.query.categoryId
  if (raw === undefined || raw === null) return null
  const n = Number(Array.isArray(raw) ? raw[0] : raw)
  return Number.isFinite(n) ? n : null
})

const resolvedCategoryId = computed(() => {
  return categoryId.value ?? categories.categories[0]?.id ?? null
})

const isShortAnswer = computed(() => {
  return currentQuestion.value?.questionType === 2
})

const isTrueFalse = computed(() => {
  return currentQuestion.value?.questionType === 1
})

/** True while answers are being posted — used to dim/disable the question UI */
const isSubmitting = computed(() => phase.value === 'submitting')

onMounted(() => {
  const id = resolvedCategoryId.value
  if (id == null) {
    toast.error(
      'No category. Go home, click “Load categories from API”, then Start Quiz.',
    )
    return
  }
  void quiz.startNewQuiz(id)
})
</script>

<template>
  <div class="container py-4">
    <div class="row justify-content-center">
      <div class="col-12 col-md-10 col-lg-8">
        <div class="card shadow-sm">
          <div class="card-body p-4">
            <!-- Creating quiz: spinner + message -->
            <div v-if="phase === 'loading'">
              <div class="d-flex align-items-center gap-3 mb-3">
                <div class="spinner-border text-primary" role="status">
                  <span class="visually-hidden">Loading</span>
                </div>
                <span>Creating quiz (may take a bit)…</span>
              </div>
              <RouterLink to="/" class="btn btn-outline-secondary">Home</RouterLink>
            </div>

            <!-- Questions: same card while submitting (no full-screen swap) -->
            <div
              v-else-if="
                (phase === 'in_progress' || phase === 'submitting') &&
                currentQuestion
              "
            >
              <p class="text-muted small mb-1">
                Question {{ quiz.currentIndex + 1 }} /
                {{ quiz.quiz?.questions.length ?? 0 }}
              </p>
              <h2 class="h4 mb-3">{{ currentQuestion.text }}</h2>

              <!-- fieldset disabled grays out controls while submitting -->
              <fieldset
                class="border-0 p-0 m-0 mb-3"
                :disabled="isSubmitting"
              >
                <div v-if="isShortAnswer" class="mb-4">
                  <label class="form-label">Your answer</label>
                  <textarea
                    class="form-control"
                    rows="3"
                    :value="answerDraft"
                    @input="
                      quiz.setAnswerDraft(
                        ($event.target as HTMLTextAreaElement).value,
                      )
                    "
                  />
                </div>

                <div v-else-if="isTrueFalse" class="list-group mb-4">
                  <button
                    type="button"
                    class="list-group-item list-group-item-action"
                    :class="{ active: answerDraft === 'true' }"
                    @click="quiz.selectChoice('true')"
                  >
                    True
                  </button>
                  <button
                    type="button"
                    class="list-group-item list-group-item-action"
                    :class="{ active: answerDraft === 'false' }"
                    @click="quiz.selectChoice('false')"
                  >
                    False
                  </button>
                </div>

                <div v-else class="list-group mb-4">
                  <button
                    v-for="(choice, idx) in currentQuestion.options"
                    :key="idx"
                    type="button"
                    class="list-group-item list-group-item-action"
                    :class="{ active: answerDraft === choice }"
                    @click="quiz.selectChoice(choice)"
                  >
                    {{ choice }}
                  </button>
                </div>
              </fieldset>

              <div
                v-if="isSubmitting"
                class="d-flex align-items-center gap-2 text-muted small mb-3"
              >
                <div
                  class="spinner-border spinner-border-sm text-primary"
                  role="status"
                >
                  <span class="visually-hidden">Submitting</span>
                </div>
                <span>Submitting answers…</span>
              </div>

              <div class="d-grid gap-2 d-md-flex">
                <button
                  type="button"
                  class="btn btn-primary d-inline-flex align-items-center justify-content-center"
                  :disabled="isSubmitting"
                  @click="quiz.goNext()"
                >
                  <span
                    v-if="isSubmitting"
                    class="spinner-border spinner-border-sm me-2"
                    role="status"
                    aria-hidden="true"
                  />
                  {{ quiz.isLastQuestion ? 'Finish' : 'Next' }}
                </button>
                <RouterLink to="/" class="btn btn-outline-secondary">Home</RouterLink>
              </div>
            </div>

            <div v-else-if="phase === 'finished' && quiz.quiz">
              <h2 class="h4 mb-3">Results</h2>
              <p class="lead mb-4">
                Score: {{ quiz.quiz.score }} /
                {{ quiz.quiz.questions.length }}
              </p>
              <div class="d-grid gap-2 d-md-flex">
                <RouterLink
                  v-if="resolvedCategoryId != null"
                  :to="{
                    name: 'quiz',
                    query: { categoryId: resolvedCategoryId },
                  }"
                  class="btn btn-primary"
                >
                  Play again
                </RouterLink>
                <button
                  type="button"
                  class="btn btn-outline-secondary"
                  @click="quiz.resetQuiz()"
                >
                  Reset
                </button>
                <RouterLink to="/" class="btn btn-outline-secondary">Home</RouterLink>
              </div>
            </div>

            <div v-else-if="phase === 'idle'">
              <p class="mb-3">No quiz loaded. Start from home.</p>
              <RouterLink to="/" class="btn btn-outline-secondary">Home</RouterLink>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>