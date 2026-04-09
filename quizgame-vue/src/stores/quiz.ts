import axios from 'axios'
import { defineStore } from 'pinia'
import { fetchChallenge, submitChallengeAnswers } from '../api/challenge'
import { createQuiz, submitQuizAnswers } from '../api/quiz'
import {
  QUIZ_MAX_QUESTION_COUNT,
  QUIZ_MIN_QUESTION_COUNT,
} from '../gameConstants'
import type { QuestionDto, QuizDto } from '../types/api'
import { useToastStore } from './toast'

/** ChallengeStatus from API (numeric enum) */
const CHALLENGE_ACTIVE = 1

type Phase = 'idle' | 'loading' | 'in_progress' | 'submitting' | 'finished'

export const useQuizStore = defineStore('quiz', {
  state: () => ({
    phase: 'idle' as Phase,
    /** When set, submit posts to challenge submit instead of quiz submit */
    challengeId: null as number | null,
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
    privateMessageFromAxios(e: unknown): string {
      if (axios.isAxiosError(e)) {
        const d = e.response?.data
        if (typeof d === 'string' && d.trim()) return d
        if (Array.isArray(d)) return d.map(String).join(' ')
        return e.message
      }
      return e instanceof Error ? e.message : 'Something went wrong'
    },

    async startChallengeQuiz(challengeId: number) {
      const toast = useToastStore()
      this.phase = 'loading'
      this.challengeId = null
      this.quiz = null
      this.currentIndex = 0
      this.answerDraft = ''
      this.answers = {}
      try {
        const c = await fetchChallenge(challengeId)
        if (c.status !== CHALLENGE_ACTIVE) {
          toast.warning(
            'Challenge must be active (accepted) before you can play.',
          )
          this.phase = 'idle'
          return
        }
        if (!c.quiz?.questions?.length) {
          toast.error('This challenge has no quiz loaded yet.')
          this.phase = 'idle'
          return
        }
        this.challengeId = challengeId
        this.quiz = c.quiz
        this.phase = 'in_progress'
        toast.success('Challenge quiz ready.')
      } catch (e) {
        toast.error(this.privateMessageFromAxios(e))
        this.phase = 'idle'
        this.quiz = null
      }
    },

    async startNewQuiz(options: {
      categoryId: number
      difficulty: number
      questionCount: number
      questionTypes: number[]
      isMultiplayer: boolean
    }) {
      const toast = useToastStore()
      this.phase = 'loading'
      this.challengeId = null
      this.quiz = null
      this.currentIndex = 0
      this.answerDraft = ''
      this.answers = {}
      const types = [...new Set(options.questionTypes)].filter(
        (t) => t >= 0 && t <= 2,
      )
      if (!types.length) {
        toast.warning('Pick at least one question type.')
        this.phase = 'idle'
        return
      }
      const rawCount = Number(options.questionCount)
      const questionCount = Math.min(
        QUIZ_MAX_QUESTION_COUNT,
        Math.max(
          QUIZ_MIN_QUESTION_COUNT,
          Number.isFinite(rawCount) ? Math.round(rawCount) : QUIZ_MIN_QUESTION_COUNT,
        ),
      )
      try {
        this.quiz = await createQuiz({
          categoryId: options.categoryId,
          difficulty: options.difficulty,
          questionCount,
          questionTypes: types,
          isMultiplayer: options.isMultiplayer,
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
        const payload = { answers: { ...this.answers } }
        if (this.challengeId != null) {
          const updated = await submitChallengeAnswers(this.challengeId, payload)
          this.quiz = updated.quiz ?? this.quiz
        } else {
          this.quiz = await submitQuizAnswers(this.quiz.id, payload)
        }
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
      this.challengeId = null
      this.quiz = null
      this.currentIndex = 0
      this.answerDraft = ''
      this.answers = {}
      toast.info('Quiz cleared.')
    },
  },
})