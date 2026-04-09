<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { fetchCurrentUser } from '../api/user'
import type { UserProfileDto } from '../types/userProfile'
import { useToastStore } from '../stores/toast'

const toast = useToastStore()
const profile = ref<UserProfileDto | null>(null)
const loading = ref(true)

onMounted(async () => {
  loading.value = true
  try {
    profile.value = await fetchCurrentUser()
  } catch {
    toast.error('Could not load your profile.')
    profile.value = null
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="container py-4">
    <div class="row justify-content-center">
      <div class="col-12 col-md-10 col-lg-8">
        <div class="card shadow-sm">
          <div class="card-body p-4">
            <h1 class="h3 mb-4">Your profile</h1>

            <div v-if="loading" class="d-flex align-items-center gap-2 text-muted">
              <div class="spinner-border spinner-border-sm" role="status" />
              <span>Loading…</span>
            </div>

            <dl v-else-if="profile" class="row mb-0">
              <dt class="col-sm-4">Username</dt>
              <dd class="col-sm-8">{{ profile.username }}</dd>
              <dt class="col-sm-4">Email</dt>
              <dd class="col-sm-8">{{ profile.email }}</dd>
              <dt class="col-sm-4">Member since</dt>
              <dd class="col-sm-8">{{ profile.memberSince }}</dd>
              <dt class="col-sm-4">Skill score</dt>
              <dd class="col-sm-8">{{ profile.skillScore.toFixed(1) }}</dd>
              <dt class="col-sm-4">Followers</dt>
              <dd class="col-sm-8">{{ profile.followersCount }}</dd>
              <dt class="col-sm-4">Following</dt>
              <dd class="col-sm-8">{{ profile.followingCount }}</dd>
            </dl>

            <p v-else class="text-muted mb-0">No profile data.</p>

            <div class="mt-4">
              <RouterLink to="/" class="btn btn-outline-secondary">Home</RouterLink>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
