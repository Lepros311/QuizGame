<script setup lang="ts">
import ToastHost from './components/ToastHost.vue';
</script>

<template>
  <header class="border-bottom bg-light">
    <nav class="navbar navbar-expand container">
      <RouterLink to="/" class="navbar-brand fw-bold">
        QuizGame Vue
      </RouterLink>
    </nav>
  </header>

  <main>
    <RouterView />
  </main>

  <ToastHost />
</template>
