<script setup lang="ts">
import { computed, onMounted, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { useStatBoardStore } from '../stores/statboard'
import type { StatBoardDto, UserStatBoardDto } from '../types/api'

const statboard = useStatBoardStore()
const {
  boards,
  myStats,
  selectedBoardId,
  globalRankings,
  followingRankings,
  loadingBoards,
  loadingMyStats,
  loadingRankings,
} = storeToRefs(statboard)

/** Must match StatBoardConstants / API StatBoard.Name */
function rankingColumnForBoard(
  board: StatBoardDto | undefined,
): { label: string; format: (row: UserStatBoardDto) => string } {
  const name = board?.name ?? ''
  switch (name) {
    case 'Top Scores':
      return {
        label: 'Highest score',
        format: (r) => String(r.highestScore),
      }
    case 'Win Streaks':
      return {
        label: 'Longest streak',
        format: (r) => String(r.longestWinStreak),
      }
    case 'Fastest Completions':
      return {
        label: 'Fastest (sec)',
        format: (r) =>
          r.fastestCompletionSeconds > 0
            ? r.fastestCompletionSeconds.toFixed(1)
            : '—',
      }
    case 'Most Challenges Won':
      return {
        label: 'Challenge wins',
        format: (r) => String(r.totalChallengesWon),
      }
    case 'Most Quizzes Completed':
      return {
        label: 'Quizzes done',
        format: (r) => String(r.totalQuizzesCompleted),
      }
    default:
      return {
        label: 'Skill',
        format: (r) => r.skillScore.toFixed(1),
      }
  }
}

const selectedBoard = computed(() =>
  boards.value.find((b) => b.id === selectedBoardId.value),
)
const rankingColumn = computed(() => rankingColumnForBoard(selectedBoard.value))

onMounted(async () => {
  await Promise.all([statboard.loadBoards(), statboard.loadMyStats()])
  await statboard.loadRankingsForSelected()
})

watch(selectedBoardId, () => {
  void statboard.loadRankingsForSelected()
})
</script>

<template>
  <div class="container py-4">
    <h1 class="h3 mb-4">Stats &amp; leaderboards</h1>

    <div class="card shadow-sm mb-4">
      <div class="card-body">
        <h2 class="h5">Your stats</h2>
        <div
          v-if="loadingMyStats"
          class="d-flex align-items-center gap-2 text-muted"
        >
          <div class="spinner-border spinner-border-sm" role="status" />
          <span>Loading…</span>
        </div>
        <dl v-else-if="myStats" class="row small mb-0">
          <dt class="col-6 col-md-3">Quizzes completed</dt>
          <dd class="col-6 col-md-3">{{ myStats.totalQuizzesCompleted }}</dd>
          <dt class="col-6 col-md-3">Avg score %</dt>
          <dd class="col-6 col-md-3">
            {{ myStats.averageScorePercentage.toFixed(1) }}
          </dd>
          <dt class="col-6 col-md-3">Challenges W / L</dt>
          <dd class="col-6 col-md-3">
            {{ myStats.totalChallengesWon }} / {{ myStats.totalChallengesLost }}
          </dd>
          <dt class="col-6 col-md-3">Skill score</dt>
          <dd class="col-6 col-md-3">{{ myStats.skillScore.toFixed(1) }}</dd>
        </dl>
        <p v-else class="text-muted mb-0">No stats yet.</p>
      </div>
    </div>

    <div class="card shadow-sm">
      <div class="card-body">
        <div class="d-flex flex-wrap align-items-end gap-3 mb-3">
          <div>
            <label class="form-label mb-0">Stat board</label>
            <select
              v-model.number="selectedBoardId"
              class="form-select"
              :disabled="loadingBoards || !boards.length"
            >
              <option v-for="b in boards" :key="b.id" :value="b.id">
                {{ b.name }}
              </option>
            </select>
          </div>
          <RouterLink to="/" class="btn btn-outline-secondary btn-sm">
            Home
          </RouterLink>
        </div>

        <div
          v-if="loadingRankings"
          class="d-flex align-items-center gap-2 text-muted mb-3"
        >
          <div class="spinner-border spinner-border-sm" role="status" />
          <span>Loading rankings…</span>
        </div>

        <div v-else class="row g-4">
          <div class="col-lg-6">
            <h3 class="h6">Global</h3>
            <div class="table-responsive">
              <table class="table table-sm table-striped mb-0">
                <thead>
                  <tr>
                    <th>#</th>
                    <th>User</th>
                    <th>{{ rankingColumn.label }}</th>
                  </tr>
                </thead>
                <tbody>
                  <tr
                    v-for="(row, idx) in globalRankings"
                    :key="row.userId"
                  >
                    <td>{{ idx + 1 }}</td>
                    <td>{{ row.username }}</td>
                    <td>{{ rankingColumn.format(row) }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
            <p
              v-if="!globalRankings.length"
              class="text-muted small mb-0"
            >
              No rankings for this board.
            </p>
          </div>
          <div class="col-lg-6">
            <h3 class="h6">People you follow</h3>
            <div class="table-responsive">
              <table class="table table-sm table-striped mb-0">
                <thead>
                  <tr>
                    <th>#</th>
                    <th>User</th>
                    <th>{{ rankingColumn.label }}</th>
                  </tr>
                </thead>
                <tbody>
                  <tr
                    v-for="(row, idx) in followingRankings"
                    :key="row.userId"
                  >
                    <td>{{ idx + 1 }}</td>
                    <td>{{ row.username }}</td>
                    <td>{{ rankingColumn.format(row) }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
            <p
              v-if="!followingRankings.length"
              class="text-muted small mb-0"
            >
              No followed users on this board yet.
            </p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
