<script setup lang="ts">
</script>

<template>
  <div class="container py-4">
    <div class="row justify-content-center">
      <div class="col-12 col-md-10 col-lg-8">
        <div class="card shadow-sm">
          <div class="card-body p-4">
            <h1 class="display-6 mb-3">Quiz Game</h1>
            <p class="lead mb-4">
              Practice your knowledge with quick quizzes.
            </p>

            <RouterLink to="/quiz" class="btn btn-primary btn-lg">
              Start Quiz
            </RouterLink>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>