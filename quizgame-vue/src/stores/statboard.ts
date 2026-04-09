import axios from 'axios'
import { defineStore } from 'pinia'
import {
  fetchFollowingRankings,
  fetchGlobalRankings,
  fetchMyStats,
  fetchStatBoards,
} from '../api/statboard'
import type { StatBoardDto, UserStatBoardDto } from '../types/api'
import { useToastStore } from './toast'

function messageFromAxios(e: unknown): string {
  if (axios.isAxiosError(e)) {
    const d = e.response?.data
    if (typeof d === 'string' && d.trim()) return d
    if (Array.isArray(d)) return d.map(String).join(' ')
    return e.message
  }
  return e instanceof Error ? e.message : 'Something went wrong'
}

export const useStatBoardStore = defineStore('statboard', {
  state: () => ({
    boards: [] as StatBoardDto[],
    myStats: null as UserStatBoardDto | null,
    selectedBoardId: null as number | null,
    globalRankings: [] as UserStatBoardDto[],
    followingRankings: [] as UserStatBoardDto[],
    loadingBoards: false,
    loadingMyStats: false,
    loadingRankings: false,
  }),
  actions: {
    async loadBoards() {
      const toast = useToastStore()
      this.loadingBoards = true
      try {
        this.boards = await fetchStatBoards()
        if (
          this.selectedBoardId == null &&
          this.boards.length > 0 &&
          this.boards[0]
        ) {
          this.selectedBoardId = this.boards[0].id
        }
      } catch (e) {
        toast.error(messageFromAxios(e))
        this.boards = []
      } finally {
        this.loadingBoards = false
      }
    },

    async loadMyStats() {
      const toast = useToastStore()
      this.loadingMyStats = true
      try {
        this.myStats = await fetchMyStats()
      } catch (e) {
        toast.error(messageFromAxios(e))
        this.myStats = null
      } finally {
        this.loadingMyStats = false
      }
    },

    async loadRankingsForSelected() {
      const toast = useToastStore()
      const id = this.selectedBoardId
      if (id == null) {
        this.globalRankings = []
        this.followingRankings = []
        return
      }
      this.loadingRankings = true
      try {
        const [g, f] = await Promise.all([
          fetchGlobalRankings(id),
          fetchFollowingRankings(id),
        ])
        this.globalRankings = g
        this.followingRankings = f
      } catch (e) {
        toast.error(messageFromAxios(e))
        this.globalRankings = []
        this.followingRankings = []
      } finally {
        this.loadingRankings = false
      }
    },

    setSelectedBoardId(id: number | null) {
      this.selectedBoardId = id
    },
  },
})
