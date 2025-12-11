<template>
  <div class="auth-tabs">
    <button
      class="auth-tab"
      :class="{ active: modelValue === 'login' }"
      @click="$emit('update:modelValue', 'login')"
    >
      Sign In
    </button>
    <button
      class="auth-tab"
      :class="{ active: modelValue === 'register' }"
      @click="$emit('update:modelValue', 'register')"
    >
      Create Account
    </button>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  modelValue: 'login' | 'register'
}>()

defineEmits<{
  'update:modelValue': [value: 'login' | 'register']
}>()
</script>

<style scoped>
.auth-tabs {
  display: flex;
  gap: 8px;
  margin-bottom: 40px;
  background: var(--bg-elevated);
  padding: 6px;
  border-radius: 14px;
}

.auth-tab {
  flex: 1;
  padding: 14px 24px;
  border: none;
  background: transparent;
  color: var(--text-secondary);
  font-family: var(--font-primary);
  font-size: 15px;
  font-weight: 600;
  cursor: pointer;
  border-radius: 10px;
  transition: all 0.3s ease;
}

.auth-tab.active {
  background: var(--bg-secondary);
  color: var(--accent-primary);
  box-shadow: var(--shadow-md);
}

.auth-tab:hover:not(.active) {
  color: var(--text-primary);
}
</style>
