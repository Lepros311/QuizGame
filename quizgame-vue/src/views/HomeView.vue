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

const { isLoggedIn, username } = storeToRefs(auth)

const welcomeName = computed(() => {
  const u = username.value?.trim()
  if (!u) return 'there'
  const parts = u.split(/\s+/).filter(Boolean)
  return parts.length > 1 ? parts[0]! : u
})

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
  <div class="home-page">
    <!-- Landing experience for visitors -->
    <template v-if="!isLoggedIn">
      <section class="home-hero position-relative" aria-labelledby="home-hero-heading">
        <div class="home-hero-decor" aria-hidden="true" />
        <div class="container position-relative py-5 py-lg-6">
          <div class="row align-items-center g-4 g-lg-5">
            <div class="col-lg-7">
              <p class="home-eyebrow mb-2">The C# Academy · Quiz Game</p>
              <h1 id="home-hero-heading" class="home-hero-title display-4 fw-bold mb-3">
                Learn fast. Play smarter.
              </h1>
              <p class="lead home-hero-lead mb-4">
                Bite-sized quizzes, friendly challenges, and stats that show how
                you are improving—built for curious learners who like a little
                competition.
              </p>
              <div class="d-flex flex-wrap gap-3 mb-2">
                <RouterLink
                  class="btn btn-primary btn-lg app-cta-primary fw-semibold px-4"
                  :to="{ name: 'register' }"
                >
                  Create free account
                </RouterLink>
                <RouterLink
                  class="btn btn-lg home-btn-ghost px-4"
                  :to="{ name: 'login' }"
                >
                  Sign in
                </RouterLink>
              </div>
              <p class="home-hero-note small mb-0">
                No clutter—just categories, questions, and your next streak.
              </p>
            </div>
            <div class="col-lg-5">
              <div class="home-preview card border-0 shadow-lg">
                <div class="home-preview-header d-flex align-items-center justify-content-between">
                  <span class="fw-semibold text-secondary small">Sample round</span>
                  <span class="badge rounded-pill home-preview-badge">Live</span>
                </div>
                <div class="home-preview-body">
                  <p class="home-preview-q mb-3">
                    Which keyword declares an asynchronous method in C#?
                  </p>
                  <div class="d-grid gap-2">
                    <span class="home-preview-opt">async</span>
                    <span class="home-preview-opt home-preview-opt--pick">await</span>
                    <span class="home-preview-opt">task</span>
                  </div>
                  <div class="home-preview-footer mt-3 pt-3 d-flex justify-content-between small text-secondary">
                    <span>Question 3 of 10</span>
                    <span>Medium · Multiple choice</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section class="home-features py-5" aria-labelledby="home-features-heading">
        <div class="container">
          <div class="text-center mb-5 mx-auto" style="max-width: 40rem">
            <h2 id="home-features-heading" class="h3 fw-bold home-features-title mb-2">
              Everything you need for a sharper study session
            </h2>
            <p class="text-secondary mb-0">
              Solo practice when you want focus, challenges when you want energy,
              and notifications so you never miss a rematch.
            </p>
          </div>
          <div class="row g-4">
            <div class="col-md-4">
              <article class="home-feature-card h-100">
                <div class="home-feature-icon mb-3" aria-hidden="true">
                  <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" fill="currentColor" viewBox="0 0 16 16">
                    <path d="M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14zm0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16z"/>
                    <path d="M10.97 4.97a.75.75 0 0 1 1.07 1.05l-3.99 4.99a.75.75 0 0 1-1.08.02L4.324 8.384a.75.75 0 1 1 1.06-1.06l2.094 2.093 3.473-4.425a.267.267 0 0 1 .02-.022z"/>
                  </svg>
                </div>
                <h3 class="h5 fw-bold mb-2">Tailored rounds</h3>
                <p class="text-secondary small mb-0">
                  Pick categories, difficulty, question count, and formats—multiple
                  choice, true/false, or short answer.
                </p>
              </article>
            </div>
            <div class="col-md-4">
              <article class="home-feature-card h-100">
                <div class="home-feature-icon mb-3" aria-hidden="true">
                  <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" fill="currentColor" viewBox="0 0 16 16">
                    <path d="M7 14s-1 0-1-1 1-4 5-4 5 3 5 4-1 1-1 1H7zm4-6a3 3 0 1 0 0-6 3 3 0 0 0 0 6z"/>
                    <path fill-rule="evenodd" d="M5.216 14A2.238 2.238 0 0 1 5 13c0-1.355.68-2.75 1.936-3.72A6.325 6.325 0 0 0 5 9c-4 0-5 3-5 4s1 1 1 1h4.216z"/>
                    <path d="M4.5 8a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5z"/>
                  </svg>
                </div>
                <h3 class="h5 fw-bold mb-2">Challenges &amp; rivalry</h3>
                <p class="text-secondary small mb-0">
                  Invite friends, compare scores, and keep the conversation going
                  with alerts when it is your turn.
                </p>
              </article>
            </div>
            <div class="col-md-4">
              <article class="home-feature-card h-100">
                <div class="home-feature-icon mb-3" aria-hidden="true">
                  <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" fill="currentColor" viewBox="0 0 16 16">
                    <path d="M4 11a1 1 0 1 1 2 0v4a1 1 0 0 1-2 0v-4zm6-4a1 1 0 1 1 2 0v8a1 1 0 0 1-2 0V7zM9 9a1 1 0 0 1 2 0v2a1 1 0 1 1-2 0V9zM1 4a1 1 0 0 1 2 0v10a1 1 0 1 1-2 0V4z"/>
                  </svg>
                </div>
                <h3 class="h5 fw-bold mb-2">Stats that motivate</h3>
                <p class="text-secondary small mb-0">
                  Watch category mastery grow over time and celebrate milestones
                  as your confidence builds.
                </p>
              </article>
            </div>
          </div>
        </div>
      </section>

      <section class="home-bottom-cta py-5 mb-2" aria-label="Get started">
        <div class="container">
          <div class="home-bottom-cta-inner text-center px-4 py-5 rounded-4">
            <h2 class="h4 fw-bold mb-2">Ready for your first round?</h2>
            <p class="text-secondary mb-4 mx-auto" style="max-width: 28rem">
              Create an account to load categories and jump straight into a quiz.
            </p>
            <RouterLink
              class="btn btn-primary btn-lg app-cta-primary fw-semibold"
              :to="{ name: 'register' }"
            >
              Get started
            </RouterLink>
          </div>
        </div>
      </section>
    </template>

    <!-- Signed-in: quick welcome + existing quiz setup -->
    <template v-else>
      <div class="container py-4">
        <header class="home-logged-head mb-4">
          <p class="home-eyebrow home-eyebrow--inline mb-1">Welcome back</p>
          <h1 class="display-6 fw-bold home-logged-title mb-2">
            Hi, {{ welcomeName }} — let us play.
          </h1>
          <p class="lead app-lead-tint mb-0">
            Choose a category, tune your options, and hit start when you are ready.
          </p>
        </header>

        <div class="row justify-content-center">
          <div class="col-12 col-lg-10 col-xl-9">
            <div class="card shadow-sm">
              <div class="card-body p-4">
                <h2 class="h5 mb-3 app-page-title">Start a quiz</h2>

                <div class="mb-4">
                  <div class="mb-3" style="max-width: 24rem">
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

                  <div class="app-quiz-options-panel p-3">
                    <h3 class="h6 mb-3 app-section-heading">Quiz options</h3>
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

                <div class="d-grid gap-2 d-sm-flex">
                  <RouterLink
                    v-if="canStartQuiz"
                    :to="quizLink"
                    class="btn btn-primary btn-lg app-cta-primary fw-semibold"
                  >
                    Start Quiz
                  </RouterLink>
                  <button
                    v-else-if="selectedCategoryId != null"
                    type="button"
                    class="btn btn-primary btn-lg"
                    disabled
                  >
                    Start Quiz (pick question types)
                  </button>
                  <button
                    v-else
                    type="button"
                    class="btn btn-primary btn-lg"
                    disabled
                  >
                    Start Quiz (loading categories…)
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<style scoped>
.home-page {
  flex: 1;
}

.home-hero {
  padding-bottom: 0.5rem;
}

.home-hero-decor {
  pointer-events: none;
  position: absolute;
  inset: 0;
  background:
    radial-gradient(
      120% 80% at 15% 10%,
      rgba(37, 99, 235, 0.12),
      transparent 55%
    ),
    radial-gradient(
      90% 70% at 85% 20%,
      rgba(124, 58, 237, 0.1),
      transparent 50%
    ),
    radial-gradient(
      70% 60% at 50% 100%,
      rgba(234, 179, 8, 0.14),
      transparent 55%
    );
}

.py-lg-6 {
  padding-top: 4.5rem !important;
  padding-bottom: 4.5rem !important;
}

.home-eyebrow {
  font-size: 0.75rem;
  font-weight: 700;
  letter-spacing: 0.14em;
  text-transform: uppercase;
  color: #6366f1;
}

.home-eyebrow--inline {
  letter-spacing: 0.12em;
}

.home-hero-title {
  letter-spacing: -0.035em;
  line-height: 1.15;
  background: linear-gradient(
    115deg,
    var(--app-brand-from) 0%,
    var(--app-brand-via) 42%,
    var(--app-brand-to) 100%
  );
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
  text-shadow: none;
}

.home-hero-lead {
  color: #475569;
  max-width: 36rem;
}

.home-hero-note {
  color: #64748b;
}

.home-btn-ghost {
  --bs-btn-color: #4338ca;
  --bs-btn-border-color: rgba(67, 56, 202, 0.35);
  --bs-btn-hover-bg: rgba(99, 102, 241, 0.08);
  --bs-btn-hover-border-color: rgba(67, 56, 202, 0.55);
  --bs-btn-hover-color: #312e81;
  --bs-btn-active-bg: rgba(99, 102, 241, 0.12);
  --bs-btn-active-border-color: rgba(67, 56, 202, 0.65);
  --bs-btn-active-color: #1e1b4b;
  font-weight: 600;
}

.home-preview {
  border-radius: 1rem;
  overflow: hidden;
  background: rgba(255, 255, 255, 0.92);
  backdrop-filter: blur(10px);
  box-shadow:
    0 1.25rem 2.5rem rgba(79, 70, 229, 0.18),
    0 0 0 1px rgba(255, 255, 255, 0.6) inset !important;
}

.home-preview-header {
  padding: 1rem 1.25rem;
  border-bottom: 1px solid rgba(99, 102, 241, 0.12);
  background: linear-gradient(
    90deg,
    rgba(37, 99, 235, 0.06),
    rgba(250, 204, 21, 0.12)
  );
}

.home-preview-badge {
  background: linear-gradient(
    120deg,
    var(--app-brand-from),
    var(--app-brand-via)
  ) !important;
  color: #fff !important;
  font-weight: 600;
  font-size: 0.65rem;
  padding: 0.35em 0.75em;
}

.home-preview-body {
  padding: 1.25rem 1.25rem 1rem;
}

.home-preview-q {
  font-weight: 600;
  color: #334155;
  font-size: 1.05rem;
  line-height: 1.45;
}

.home-preview-opt {
  display: block;
  padding: 0.55rem 0.85rem;
  border-radius: 0.45rem;
  border: 2px dashed rgba(100, 116, 139, 0.35);
  color: #64748b;
  font-size: 0.9rem;
  background: rgba(248, 250, 252, 0.9);
}

.home-preview-opt--pick {
  border-style: solid;
  border-color: var(--app-yellow-rich);
  background: var(--app-yellow-soft);
  color: #422006;
  font-weight: 600;
  box-shadow: 0 0 0 1px rgba(234, 179, 8, 0.25);
}

.home-preview-footer {
  border-top: 1px dashed rgba(148, 163, 184, 0.5);
}

.home-features-title {
  color: #3730a3;
  letter-spacing: -0.02em;
}

.home-feature-card {
  border-radius: 0.85rem;
  padding: 1.5rem 1.35rem;
  background: rgba(255, 255, 255, 0.78);
  border: 1px solid rgba(99, 102, 241, 0.14);
  box-shadow:
    0 0.5rem 1.25rem rgba(99, 102, 241, 0.08),
    0 0 0 1px rgba(255, 255, 255, 0.5) inset;
  transition:
    transform 0.2s ease,
    box-shadow 0.2s ease;
}

.home-feature-card:hover {
  transform: translateY(-3px);
  box-shadow:
    0 0.85rem 1.75rem rgba(79, 70, 229, 0.14),
    0 0 0 1px rgba(255, 255, 255, 0.6) inset;
}

.home-feature-icon {
  width: 3rem;
  height: 3rem;
  border-radius: 0.75rem;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #4338ca;
  background: linear-gradient(
    145deg,
    rgba(37, 99, 235, 0.12),
    var(--app-yellow-soft)
  );
  box-shadow: 0 0 0 1px rgba(99, 102, 241, 0.15);
}

.home-bottom-cta-inner {
  background: linear-gradient(
    125deg,
    rgba(255, 255, 255, 0.92) 0%,
    rgba(224, 231, 255, 0.65) 45%,
    rgba(254, 243, 199, 0.55) 100%
  );
  border: 1px solid rgba(99, 102, 241, 0.16);
  box-shadow:
    0 0.75rem 2rem rgba(79, 70, 229, 0.12),
    0 0 0 1px rgba(255, 255, 255, 0.7) inset;
}

.home-logged-title {
  letter-spacing: -0.03em;
  color: #4338ca;
  text-shadow:
    0 2px 14px rgba(99, 102, 241, 0.18),
    0 1px 0 var(--app-yellow-soft);
}
</style>
