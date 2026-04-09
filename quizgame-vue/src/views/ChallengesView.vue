<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { storeToRefs } from 'pinia'
import {
  QUIZ_MAX_QUESTION_COUNT,
  QUIZ_MIN_QUESTION_COUNT,
} from '../gameConstants'
import { searchUsers } from '../api/user'
import type { ApplicationUserSummaryDto, ChallengeDto } from '../types/api'
import { useAuthStore } from '../stores/auth'
import { useCategoryStore } from '../stores/category'
import { useChallengeStore } from '../stores/challenge'

const auth = useAuthStore()
const categories = useCategoryStore()
const challenges = useChallengeStore()
const { userId } = storeToRefs(auth)

const CHALLENGE_LABELS = ['Pending', 'Active', 'Completed', 'Expired']
const PARTICIPANT_LABELS = ['Pending', 'Accepted', 'Declined', 'Completed']

const searchQuery = ref('')
const searchHits = ref<ApplicationUserSummaryDto[]>([])
const searching = ref(false)
const opponents = ref<{ id: string; username: string }[]>([])

const categoryId = ref<number | null>(null)
const difficulty = ref(0)
const questionCount = ref(10)
const questionTypesMc = ref(true)
const questionTypesTf = ref(false)
const questionTypesShort = ref(false)
const creating = ref(false)

onMounted(async () => {
  await categories.loadCategories()
  if (categories.categories[0]) categoryId.value = categories.categories[0].id
  await challenges.loadAll()
})

const myParticipant = (c: ChallengeDto) =>
  c.participants.find((p) => p.userId === userId.value)

const canRespond = (c: ChallengeDto) => {
  const p = myParticipant(c)
  return p != null && p.status === 0
}

const canPlay = (c: ChallengeDto) => {
  if (c.status !== 1 || !c.quiz?.questions?.length) return false
  const p = myParticipant(c)
  return p != null && p.status !== 3
}

function statusLabel(c: ChallengeDto) {
  return CHALLENGE_LABELS[c.status] ?? String(c.status)
}

function participantLabel(c: ChallengeDto) {
  const p = myParticipant(c)
  if (!p) return '—'
  return PARTICIPANT_LABELS[p.status] ?? String(p.status)
}

async function runSearch() {
  const q = searchQuery.value.trim()
  if (q.length < 2) {
    searchHits.value = []
    return
  }
  searching.value = true
  try {
    searchHits.value = await searchUsers(q)
  } catch {
    searchHits.value = []
  } finally {
    searching.value = false
  }
}

function toggleOpponent(u: ApplicationUserSummaryDto) {
  const i = opponents.value.findIndex((x) => x.id === u.id)
  if (i >= 0) opponents.value = opponents.value.filter((x) => x.id !== u.id)
  else opponents.value = [...opponents.value, { id: u.id, username: u.username }]
}

function removeOpponent(id: string) {
  opponents.value = opponents.value.filter((x) => x.id !== id)
}

const selectedCategories = computed(() => categories.categories)

const questionTypesPayload = computed(() => {
  const t: number[] = []
  if (questionTypesMc.value) t.push(0)
  if (questionTypesTf.value) t.push(1)
  if (questionTypesShort.value) t.push(2)
  return t.length ? t : [0]
})

async function onCreate() {
  const cid = categoryId.value
  if (cid == null || !opponents.value.length) return
  creating.value = true
  try {
    const c = await challenges.create({
      categoryId: cid,
      difficulty: difficulty.value,
      questionCount: questionCount.value,
      questionTypes: questionTypesPayload.value,
      opponentIds: opponents.value.map((o) => o.id),
    })
    if (c) {
      opponents.value = []
      searchQuery.value = ''
      searchHits.value = []
    }
  } finally {
    creating.value = false
  }
}
</script>

<template>
  <div class="container py-4">
    <h1 class="h3 mb-4">Challenges</h1>

    <div class="card shadow-sm mb-4">
      <div class="card-body">
        <h2 class="h5 mb-3">Create a challenge</h2>
        <div class="row g-3">
          <div class="col-md-4">
            <label class="form-label">Category</label>
            <select v-model.number="categoryId" class="form-select">
              <option
                v-for="c in selectedCategories"
                :key="c.id"
                :value="c.id"
              >
                {{ c.name }}
              </option>
            </select>
          </div>
          <div class="col-md-4">
            <label class="form-label">Difficulty</label>
            <select v-model.number="difficulty" class="form-select">
              <option :value="0">Easy</option>
              <option :value="1">Medium</option>
              <option :value="2">Hard</option>
            </select>
          </div>
          <div class="col-md-4">
            <label class="form-label">Question count</label>
            <input
              v-model.number="questionCount"
              type="number"
              :min="QUIZ_MIN_QUESTION_COUNT"
              :max="QUIZ_MAX_QUESTION_COUNT"
              class="form-control"
            />
          </div>
          <div class="col-12">
            <span class="form-label d-block">Question types</span>
            <div class="form-check form-check-inline">
              <input
                id="qt-mc"
                v-model="questionTypesMc"
                class="form-check-input"
                type="checkbox"
              />
              <label class="form-check-label" for="qt-mc">Multiple choice</label>
            </div>
            <div class="form-check form-check-inline">
              <input
                id="qt-tf"
                v-model="questionTypesTf"
                class="form-check-input"
                type="checkbox"
              />
              <label class="form-check-label" for="qt-tf">True / false</label>
            </div>
            <div class="form-check form-check-inline">
              <input
                id="qt-sa"
                v-model="questionTypesShort"
                class="form-check-input"
                type="checkbox"
              />
              <label class="form-check-label" for="qt-sa">Short answer</label>
            </div>
          </div>
          <div class="col-12">
            <label class="form-label">Find opponents</label>
            <div class="input-group">
              <input
                v-model="searchQuery"
                type="search"
                class="form-control"
                placeholder="Search by username (min 2 chars)"
                @keyup.enter="runSearch"
              />
              <button
                type="button"
                class="btn btn-outline-secondary"
                :disabled="searching"
                @click="runSearch"
              >
                {{ searching ? '…' : 'Search' }}
              </button>
            </div>
            <ul
              v-if="searchHits.length"
              class="list-group list-group-flush border rounded mt-2 small"
            >
              <li
                v-for="u in searchHits"
                :key="u.id"
                class="list-group-item d-flex justify-content-between align-items-center"
              >
                <span>{{ u.username }}</span>
                <button
                  type="button"
                  class="btn btn-sm"
                  :class="
                    opponents.some((o) => o.id === u.id)
                      ? 'btn-secondary'
                      : 'btn-outline-primary'
                  "
                  @click="toggleOpponent(u)"
                >
                  {{ opponents.some((o) => o.id === u.id) ? 'Remove' : 'Add' }}
                </button>
              </li>
            </ul>
            <div v-if="opponents.length" class="mt-2">
              <span class="text-muted small">Opponents: </span>
              <span
                v-for="o in opponents"
                :key="o.id"
                class="badge bg-secondary me-1"
              >
                {{ o.username }}
                <button
                  type="button"
                  class="btn btn-link btn-sm text-white text-decoration-none p-0 ms-1"
                  aria-label="Remove"
                  @click="removeOpponent(o.id)"
                >
                  ×
                </button>
              </span>
            </div>
          </div>
          <div class="col-12">
            <button
              type="button"
              class="btn btn-primary"
              :disabled="
                creating ||
                categoryId == null ||
                !opponents.length
              "
              @click="onCreate"
            >
              {{ creating ? 'Sending…' : 'Send challenge' }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <div class="card shadow-sm">
      <div class="card-body">
        <h2 class="h5 mb-3">Your challenges</h2>
        <div
          v-if="challenges.loading"
          class="d-flex align-items-center gap-2 text-muted"
        >
          <div class="spinner-border spinner-border-sm" role="status" />
          <span>Loading…</span>
        </div>
        <ul v-else-if="challenges.list.length" class="list-group list-group-flush">
          <li
            v-for="c in challenges.list"
            :key="c.id"
            class="list-group-item px-0"
          >
            <div class="d-flex flex-wrap justify-content-between gap-2">
              <div>
                <strong>#{{ c.id }}</strong>
                <span class="text-muted small ms-2">
                  from {{ c.challengerUsername }}
                </span>
                <div class="small text-muted">
                  Status: {{ statusLabel(c) }} · You:
                  {{ participantLabel(c) }}
                </div>
              </div>
              <div class="d-flex flex-wrap gap-2">
                <template v-if="canRespond(c)">
                  <button
                    type="button"
                    class="btn btn-sm btn-success"
                    @click="challenges.accept(c.id)"
                  >
                    Accept
                  </button>
                  <button
                    type="button"
                    class="btn btn-sm btn-outline-secondary"
                    @click="challenges.decline(c.id)"
                  >
                    Decline
                  </button>
                </template>
                <RouterLink
                  v-if="canPlay(c)"
                  :to="{ name: 'challenge-play', params: { id: c.id } }"
                  class="btn btn-sm btn-primary"
                >
                  Play
                </RouterLink>
              </div>
            </div>
          </li>
        </ul>
        <p v-else class="text-muted mb-0">No challenges yet.</p>
      </div>
    </div>

    <div class="mt-3">
      <RouterLink to="/" class="btn btn-outline-secondary">Home</RouterLink>
    </div>
  </div>
</template>
