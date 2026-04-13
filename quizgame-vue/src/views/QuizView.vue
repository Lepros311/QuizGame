<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import type { LocationQueryValue } from 'vue-router'
import { storeToRefs } from 'pinia'
import {
  QUIZ_MAX_QUESTION_COUNT,
  QUIZ_MIN_QUESTION_COUNT,
} from '../gameConstants'
import { useQuizStore } from '../stores/quiz'
import { useCategoryStore } from '../stores/category'
import { useToastStore } from '../stores/toast'
import type { QuestionDto } from '../types/api'

const route = useRoute()
const quiz = useQuizStore()
const categories = useCategoryStore()
const toast = useToastStore()

const { phase, currentQuestion, answerDraft, challengeId } = storeToRefs(quiz)

const isChallengePlay = computed(() => route.name === 'challenge-play')

const challengeRouteId = computed(() => {
  if (!isChallengePlay.value) return null
  const raw = route.params.id
  const n = Number(Array.isArray(raw) ? raw[0] : raw)
  return Number.isFinite(n) ? n : null
})

const categoryId = computed(() => {
  const raw = route.query.categoryId
  if (raw === undefined || raw === null) return null
  const n = Number(Array.isArray(raw) ? raw[0] : raw)
  return Number.isFinite(n) ? n : null
})

const resolvedCategoryId = computed(() => {
  return categoryId.value ?? categories.categories[0]?.id ?? null
})

function firstQueryValue(
  v: LocationQueryValue | LocationQueryValue[] | undefined,
): string | undefined {
  if (v == null) return undefined
  if (Array.isArray(v)) {
    const x = v[0]
    return x == null ? undefined : String(x)
  }
  return v === null ? undefined : String(v)
}

function parseIntQuery(
  v: LocationQueryValue | LocationQueryValue[] | undefined,
  fallback: number,
  min: number,
  max: number,
): number {
  const n = Number(firstQueryValue(v))
  if (!Number.isFinite(n)) return fallback
  return Math.min(max, Math.max(min, n))
}

function parseQuestionTypesQuery(
  v: LocationQueryValue | LocationQueryValue[] | undefined,
): number[] {
  const s = firstQueryValue(v)
  if (s == null || !String(s).trim()) return [0]
  const nums = String(s)
    .split(',')
    .map((x) => Number(x.trim()))
    .filter((n) => Number.isFinite(n) && n >= 0 && n <= 2)
  const uniq = [...new Set(nums)]
  return uniq.length ? uniq : [0]
}

function parseMultiplayerQuery(
  v: LocationQueryValue | LocationQueryValue[] | undefined,
): boolean {
  const s = firstQueryValue(v)
  return s === '1' || s === 'true'
}

const soloQuizOptions = computed(() => {
  const cid = resolvedCategoryId.value
  if (cid == null) return null
  const q = route.query
  return {
    categoryId: cid,
    difficulty: parseIntQuery(q.difficulty, 1, 0, 2),
    questionCount: parseIntQuery(
      q.questionCount,
      QUIZ_MIN_QUESTION_COUNT,
      QUIZ_MIN_QUESTION_COUNT,
      QUIZ_MAX_QUESTION_COUNT,
    ),
    questionTypes: parseQuestionTypesQuery(q.questionTypes),
    isMultiplayer: parseMultiplayerQuery(q.multiplayer),
  }
})

const isShortAnswer = computed(() => {
  return currentQuestion.value?.questionType === 2
})

const isTrueFalse = computed(() => {
  return currentQuestion.value?.questionType === 1
})

/** True while answers are being posted — used to dim/disable the question UI */
const isSubmitting = computed(() => phase.value === 'submitting')

function questionTypeLabel(type: number): string {
  if (type === 0) return 'Multiple choice'
  if (type === 1) return 'True / false'
  if (type === 2) return 'Short answer'
  return 'Question'
}

function formatSubmittedAnswer(q: QuestionDto, raw: string | null | undefined): string {
  if (raw == null || !String(raw).trim()) return '—'
  const s = String(raw).trim()
  if (q.questionType === 1) {
    const low = s.toLowerCase()
    if (low === 'true') return 'True'
    if (low === 'false') return 'False'
  }
  return s
}

onMounted(() => {
  if (isChallengePlay.value) {
    const cid = challengeRouteId.value
    if (cid == null) {
      toast.error('Invalid challenge link.')
      return
    }
    void quiz.startChallengeQuiz(cid)
    return
  }
  const opts = soloQuizOptions.value
  if (opts == null) {
    toast.error(
      'No category. Go home, load categories, pick one, then Start Quiz.',
    )
    return
  }
  void quiz.startNewQuiz(opts)
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
                class="d-flex flex-wrap gap-2 justify-content-between align-items-center"
              >
                <RouterLink to="/" class="btn btn-outline-secondary">Home</RouterLink>
                <template v-if="isSubmitting">
                  <div
                    class="d-inline-flex align-items-center gap-2 rounded-2 border border-primary-subtle bg-primary-subtle px-3 py-2 min-w-0"
                    role="status"
                  >
                    <span
                      class="spinner-border spinner-border-sm text-primary flex-shrink-0"
                      aria-hidden="true"
                    />
                    <span class="fw-semibold text-primary-emphasis"
                      >Submitting answers…</span
                    >
                  </div>
                </template>
                <button
                  v-else
                  type="button"
                  class="btn btn-primary app-cta-primary d-inline-flex align-items-center justify-content-center fw-semibold"
                  @click="quiz.goNext()"
                >
                  {{ quiz.isLastQuestion ? 'Finish' : 'Next' }}
                </button>
              </div>
            </div>

            <div v-else-if="phase === 'finished' && quiz.quiz">
              <h2 class="h4 mb-3">Results</h2>
              <p class="lead mb-4">
                Score: {{ quiz.quiz.score }} /
                {{ quiz.quiz.questions.length }}
              </p>

              <h3 class="h6 text-secondary mb-3">Review</h3>
              <ul class="list-unstyled quiz-results-list mb-4">
                <li
                  v-for="(q, idx) in quiz.quiz.questions"
                  :key="q.id"
                  class="quiz-results-item rounded border p-3 mb-3"
                >
                  <div class="d-flex flex-wrap align-items-baseline justify-content-between gap-2 mb-2">
                    <span class="small text-muted"
                      >Question {{ idx + 1 }} ·
                      {{ questionTypeLabel(q.questionType) }}</span
                    >
                    <span
                      v-if="q.isCorrect === true"
                      class="badge rounded-pill text-bg-success"
                      >Correct</span
                    >
                    <span
                      v-else-if="q.isCorrect === false"
                      class="badge rounded-pill text-bg-danger"
                      >Incorrect</span
                    >
                  </div>
                  <p class="fw-semibold mb-2 mb-md-3">{{ q.text }}</p>
                  <p class="mb-0 small">
                    <span class="text-secondary">Your answer:</span>
                    <span class="ms-1 text-break">{{
                      formatSubmittedAnswer(q, q.userAnswer)
                    }}</span>
                  </p>
                  <p
                    v-if="
                      q.isCorrect === false &&
                      q.correctAnswer != null &&
                      String(q.correctAnswer).trim() !== ''
                    "
                    class="mb-0 small mt-2"
                  >
                    <span class="text-secondary">Correct answer:</span>
                    <span class="ms-1 text-break fw-medium">{{
                      formatSubmittedAnswer(q, q.correctAnswer)
                    }}</span>
                  </p>
                </li>
              </ul>

              <div class="d-grid gap-2 d-md-flex">
                <RouterLink
                  v-if="challengeId != null"
                  :to="{ name: 'challenges' }"
                  class="btn btn-primary app-cta-primary fw-semibold"
                >
                  Back to challenges
                </RouterLink>
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