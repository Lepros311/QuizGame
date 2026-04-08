import axios from 'axios'
import { defineStore } from 'pinia'
import { createQuiz, submitQuizAnswers } from '../api/quiz'
import type { QuestionDto, QuizDto } from '../types/api'
import { useToastStore } from './toast'

type Phase = 'idle' | 'loading' | 'in_progress' | 'submitting' | 'finished'

export const useQuizStore = defineStore('quiz', {
  state: () => ({
    phase: 'idle' as Phase,
    quiz: null as QuizDto | null,
    currentIndex: 0,
    /** Text sent for the current question (option label or short-answer text) */
    answerDraft: '',
    /** Built as the user clicks Next */
    answers: {} as Record<number, string>,
  }),
  getters: {
    currentQuestion(state): QuestionDto | null {
      const qs = state.quiz?.questions ?? []
      return qs[state.currentIndex] ?? null
    },
    isLastQuestion(state): boolean {
      const n = state.quiz?.questions.length ?? 0
      return n > 0 && state.currentIndex >= n - 1
    },
  },
  actions: {
    demoToastFromAction() {
      const toast = useToastStore()
      toast.success('Toast from a Pinia action — pattern check.')
    },

    privateMessageFromAxios(e: unknown): string {
      if (axios.isAxiosError(e)) {
        const d = e.response?.data
        if (typeof d === 'string' && d.trim()) return d
        if (Array.isArray(d)) return d.map(String).join(' ')
        return e.message
      }
      return e instanceof Error ? e.message : 'Something went wrong'
    },

    async startNewQuiz(categoryId: number) {
      const toast = useToastStore()
      this.phase = 'loading'
      this.quiz = null
      this.currentIndex = 0
      this.answerDraft = ''
      this.answers = {}
      try {
        this.quiz = await createQuiz({
          categoryId,
          difficulty: 1,
          questionCount: 10,
          questionTypes: [0],
          isMultiplayer: false,
        })
        if (!this.quiz.questions.length) {
          toast.warning('Quiz created but has no questions yet.')
        }
        this.phase = 'in_progress'
        toast.success('Quiz ready.')
      } catch (e) {
        toast.error(this.privateMessageFromAxios(e))
        this.phase = 'idle'
        this.quiz = null
      }
    },

    selectChoice(optionText: string) {
      this.answerDraft = optionText
    },

    setAnswerDraft(text: string) {
      this.answerDraft = text
    },

    goNext() {
      const toast = useToastStore()
      const q = this.currentQuestion
      if (!q) return

      const trimmed = this.answerDraft.trim()
      if (!trimmed) {
        toast.warning('Enter or select an answer first.')
        return
      }

      this.answers[q.id] = trimmed

      if (this.isLastQuestion) {
        void this.submitToApi()
        return
      }

      this.currentIndex += 1
      this.answerDraft = ''
    },

    async submitToApi() {
      const toast = useToastStore()
      if (!this.quiz) return

      this.phase = 'submitting'
      try {
        this.quiz = await submitQuizAnswers(this.quiz.id, {
          answers: { ...this.answers },
        })
        this.phase = 'finished'
        toast.success(`Submitted. Score: ${this.quiz.score} / ${this.quiz.questions.length}`)
      } catch (e) {
        toast.error(this.privateMessageFromAxios(e))
        this.phase = 'in_progress'
      }
    },

    resetQuiz() {
      const toast = useToastStore()
      this.phase = 'idle'
      this.quiz = null
      this.currentIndex = 0
      this.answerDraft = ''
      this.answers = {}
      toast.info('Quiz cleared.')
    },
  },
})