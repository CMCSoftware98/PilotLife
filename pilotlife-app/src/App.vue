<template>
  <v-app>
    <TitleBar />
    <div class="app-content">
      <router-view />
    </div>
  </v-app>
</template>

<script setup lang="ts">
import TitleBar from './components/TitleBar.vue'
</script>

<style>
@import url('https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;500;600;700;800&family=Space+Mono:wght@400;700&display=swap');

html, body, #app {
  height: 100%;
  margin: 0;
  padding: 0;
  overflow: hidden;
}

/* Hide scrollbar on html/body (Tauri window level) */
html {
  scrollbar-width: none; /* Firefox */
  -ms-overflow-style: none; /* IE and Edge */
}

html::-webkit-scrollbar,
body::-webkit-scrollbar {
  display: none; /* Chrome, Safari, Opera */
}

.v-application {
  background: var(--bg-primary) !important;
}

.app-content {
  margin-top: 36px;
  height: calc(100% - 36px);
  overflow: auto;
}
</style>
