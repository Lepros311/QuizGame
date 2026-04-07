<script setup lang="ts">
import { onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { useQuizStore } from '../stores/quiz'

const quiz = useQuizStore()
const { phase, currentQuestion, selectedChoiceIndex, score } = storeToRefs(quiz) // reactive bindings for template

onMounted(() => {
  if (quiz.phase === 'idle') {
    quiz.startMockQuiz() // auto-start mock quiz when opening this page (change later if you prefer a button)
  }
})
</script>

<template>
  <div class="container py-4">
    <div class="row justify-content-center">
      <div class="col-12 col-md-10 col-lg-8">
        <div class="card shadow-sm">
          <div class="card-body p-4">
            <div v-if="phase === 'in_progress' && currentQuestion">
              <p class="text-muted small mb-1">
                Question {{ quiz.currentIndex + 1 }} / {{ quiz.questions.length }}
              </p>
              <h2 class="h4 mb-3">{{ currentQuestion.prompt }}</h2>

              <div class="list-group mb-4">
                <button
                  v-for="(choice, idx) in currentQuestion.choices"
                  :key="idx"
                  type="button"
                  class="list-group-item list-group-item-action"
                  :class="{ active: selectedChoiceIndex === idx }"
                  @click="quiz.selectChoice(idx)"
                >
                  {{ choice }}
                </button>
              </div>

              <div class="d-grid gap-2 d-md-flex">
                <button
                  type="button"
                  class="btn btn-primary"
                  @click="quiz.goNext()"
                >
                  {{ quiz.isLastQuestion ? 'Finish' : 'Next' }}
                </button>
                <RouterLink to="/" class="btn btn-outline-secondary">Home</RouterLink>
              </div>
            </div>

            <div v-else-if="phase === 'finished'">
              <h2 class="h4 mb-3">Results</h2>
              <p class="lead mb-4">You scored {{ score }} / {{ quiz.questions.length }}.</p>
              <div class="d-grid gap-2 d-md-flex">
                <button type="button" class="btn btn-primary" @click="quiz.startMockQuiz()">
                  Play again
                </button>
                <button type="button" class="btn btn-outline-secondary" @click="quiz.resetQuiz()">
                  Reset
                </button>
                <RouterLink to="/" class="btn btn-outline-secondary">Home</RouterLink>
              </div>
            </div>

            <div v-else>
              <p class="mb-3">Loading quiz…</p>
              <RouterLink to="/" class="btn btn-outline-secondary">Home</RouterLink>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>