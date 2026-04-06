<script setup lang="ts">
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
</template>
