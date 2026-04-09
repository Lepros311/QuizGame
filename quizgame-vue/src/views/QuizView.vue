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

function serializeQuizRouteQuery(o: {
  categoryId: number
  difficulty: number
  questionCount: number
  questionTypes: number[]
  isMultiplayer: boolean
}): Record<string, string> {
  return {
    categoryId: String(o.categoryId),
    difficulty: String(o.difficulty),
    questionCount: String(o.questionCount),
    questionTypes: o.questionTypes.join(','),
    multiplayer: o.isMultiplayer ? '1' : '0',
  }
}

const playAgainQuery = computed((): Record<string, string> => {
  if (challengeId.value != null) return {}
  const dto = quiz.quiz
  if (!dto) return {}
  const cid = dto.category?.id ?? resolvedCategoryId.value
  if (cid == null) return {}
  const types = dto.questionTypes?.length ? dto.questionTypes : [0]
  return serializeQuizRouteQuery({
    categoryId: cid,
    difficulty: dto.difficulty,
    questionCount: dto.questionCount,
    questionTypes: types,
    isMultiplayer: dto.isMultiplayer,
  })
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
                  class="btn btn-primary app-cta-primary d-inline-flex align-items-center justify-content-center fw-semibold"
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
                  v-if="challengeId != null"
                  :to="{ name: 'challenges' }"
                  class="btn btn-primary app-cta-primary fw-semibold"
                >
                  Back to challenges
                </RouterLink>
                <RouterLink
                  v-else-if="resolvedCategoryId != null"
                  :to="{ name: 'quiz', query: playAgainQuery }"
                  class="btn btn-primary app-cta-primary fw-semibold"
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