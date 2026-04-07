import { defineStore } from 'pinia'
import { useToastStore } from './toast'
import { mockQuestions, type MockQuestion } from '../mocks/mockQuiz'

type Phase = 'idle' | 'in_progress' | 'finished'

export const useQuizStore = defineStore('quiz', {
  state: () => ({
    phase: 'idle' as Phase,
    questions: [] as MockQuestion[],
    currentIndex: 0,
    selectedChoiceIndex: null as number | null,
    answers: [] as number[], // chosen index per question, same order as questions
    score: null as number | null,
  }),
  getters: {
    currentQuestion(state): MockQuestion | null {
      return state.questions[state.currentIndex] ?? null
    },
    isLastQuestion(state): boolean {
      return state.currentIndex >= state.questions.length - 1
    },
  },
  actions: {
    demoToastFromAction() {
      const toast = useToastStore()
      toast.success('Toast from a Pinia action — this is the pattern we’ll use everywhere.')
    },

    startMockQuiz() {
      const toast = useToastStore()
      this.phase = 'in_progress'
      this.questions = [...mockQuestions]
      this.currentIndex = 0
      this.selectedChoiceIndex = null
      this.answers = []
      this.score = null
      toast.info('Mock quiz started (not calling the API yet).') // user-visible feedback from the action
    },

    selectChoice(index: number) {
      this.selectedChoiceIndex = index
    },

    goNext() {
      const toast = useToastStore()
      if (this.selectedChoiceIndex === null) {
        toast.warning('Pick an answer first.') // guardrail toast from the action
        return
      }
      this.answers[this.currentIndex] = this.selectedChoiceIndex

      if (this.isLastQuestion) {
        this.finishQuiz()
        return
      }

      this.currentIndex += 1
      this.selectedChoiceIndex = null
    },

    finishQuiz() {
      const toast = useToastStore()
      const last = this.selectedChoiceIndex
      if (last === null) {
        toast.warning('Pick an answer first.')
        return
      }
      this.answers[this.currentIndex] = last

      let correct = 0
      this.questions.forEach((q, i) => {
        if (this.answers[i] === q.correctIndex) correct += 1
      })
      this.score = correct
      this.phase = 'finished'
      toast.success(`Finished! Score: ${correct} / ${this.questions.length}`) // outcome toast from the action
    },

    resetQuiz() {
      const toast = useToastStore()
      this.phase = 'idle'
      this.questions = []
      this.currentIndex = 0
      this.selectedChoiceIndex = null
      this.answers = []
      this.score = null
      toast.info('Quiz reset.')
    },
  },
})