<template>
  <div class="world-selector">
    <v-menu v-model="menuOpen" location="bottom" offset="8">
      <template v-slot:activator="{ props }">
        <button class="selector-button" v-bind="props">
          <div class="world-info">
            <span class="world-badge" :class="currentDifficulty">
              {{ currentDifficulty }}
            </span>
            <span class="world-name">{{ currentWorldName }}</span>
          </div>
          <svg
            class="chevron"
            :class="{ rotated: menuOpen }"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="2"
          >
            <polyline points="6 9 12 15 18 9"/>
          </svg>
        </button>
      </template>

      <v-list class="world-menu" density="compact" bg-color="var(--bg-elevated)">
        <v-list-subheader class="menu-header">Your Worlds</v-list-subheader>

        <v-list-item
          v-for="pw in worldStore.playerWorlds.value"
          :key="pw.id"
          :active="pw.id === currentPlayerWorldId"
          @click="selectWorld(pw)"
          class="world-item"
        >
          <template v-slot:prepend>
            <span class="item-badge" :class="pw.worldDifficulty.toLowerCase()">
              {{ pw.worldDifficulty.charAt(0) }}
            </span>
          </template>

          <v-list-item-title>{{ pw.worldName }}</v-list-item-title>

          <template v-slot:subtitle>
            <span class="balance">${{ formatBalance(pw.balance) }}</span>
          </template>
        </v-list-item>

        <v-divider class="my-2" />

        <v-list-item
          prepend-icon="mdi-plus"
          @click="goToWorldSelection"
          class="join-world-item"
        >
          <v-list-item-title>Join Another World</v-list-item-title>
        </v-list-item>
      </v-list>
    </v-menu>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useWorldStore } from '../stores/world'
import type { PlayerWorldResponse } from '../services/api'

const router = useRouter()
const worldStore = useWorldStore()

const menuOpen = ref(false)

const currentWorldName = computed(() => {
  return worldStore.currentPlayerWorld.value?.worldName ?? 'Select World'
})

const currentDifficulty = computed(() => {
  return worldStore.currentPlayerWorld.value?.worldDifficulty?.toLowerCase() ?? 'medium'
})

const currentPlayerWorldId = computed(() => {
  return worldStore.currentPlayerWorld.value?.id ?? ''
})

function selectWorld(pw: PlayerWorldResponse) {
  worldStore.setCurrentWorld(pw)
  menuOpen.value = false

  // Reload the current page to refresh data for the new world
  router.go(0)
}

function goToWorldSelection() {
  menuOpen.value = false
  router.push('/select-world')
}

function formatBalance(balance: number): string {
  return new Intl.NumberFormat('en-US').format(Math.round(balance))
}
</script>

<style scoped>
.world-selector {
  width: 100%;
}

.selector-button {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  background: var(--bg-elevated);
  border: 1px solid var(--border-subtle);
  border-radius: 10px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.selector-button:hover {
  border-color: var(--accent-primary);
  background: var(--bg-secondary);
}

.world-info {
  display: flex;
  align-items: center;
  gap: 10px;
}

.world-badge {
  padding: 4px 8px;
  border-radius: 6px;
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
}

.world-badge.easy {
  background: rgba(34, 197, 94, 0.15);
  color: #22c55e;
}

.world-badge.medium {
  background: rgba(59, 130, 246, 0.15);
  color: #3b82f6;
}

.world-badge.hard {
  background: rgba(239, 68, 68, 0.15);
  color: #ef4444;
}

.world-name {
  font-size: 14px;
  font-weight: 500;
  color: var(--text-primary);
}

.chevron {
  width: 18px;
  height: 18px;
  color: var(--text-muted);
  transition: transform 0.2s ease;
}

.chevron.rotated {
  transform: rotate(180deg);
}

.world-menu {
  min-width: 220px;
  border: 1px solid var(--border-subtle);
  border-radius: 12px;
}

.menu-header {
  font-size: 11px;
  font-weight: 600;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.world-item {
  border-radius: 8px;
  margin: 0 8px;
}

.item-badge {
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 6px;
  font-size: 11px;
  font-weight: 700;
}

.item-badge.easy {
  background: rgba(34, 197, 94, 0.15);
  color: #22c55e;
}

.item-badge.medium {
  background: rgba(59, 130, 246, 0.15);
  color: #3b82f6;
}

.item-badge.hard {
  background: rgba(239, 68, 68, 0.15);
  color: #ef4444;
}

.balance {
  font-family: var(--font-mono);
  font-size: 12px;
  color: var(--text-secondary);
}

.join-world-item {
  color: var(--accent-primary);
  margin: 0 8px;
  border-radius: 8px;
}
</style>
