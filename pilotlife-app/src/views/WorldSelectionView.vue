<template>
  <div class="world-selection">
    <div class="selection-container">
      <div class="header">
        <h1 class="title">Choose Your Starting World</h1>
        <p class="subtitle">Each world has different economy settings. We recommend starting with <strong>Medium</strong> for a balanced experience.</p>
        <p class="hint">You can join other worlds anytime from the sidebar — you're not locked into this choice!</p>
      </div>

      <div v-if="isLoading" class="loading-state">
        <v-progress-circular indeterminate size="48" color="primary" />
        <p>Loading worlds...</p>
      </div>

      <div v-else-if="error" class="error-state">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <circle cx="12" cy="12" r="10"/>
          <path d="M12 8v4M12 16h.01"/>
        </svg>
        <p>{{ error }}</p>
        <v-btn color="primary" variant="outlined" @click="loadData">Try Again</v-btn>
      </div>

      <div v-else class="worlds-grid">
        <div
          v-for="world in sortedWorlds"
          :key="world.id"
          class="world-card"
          :class="{
            selected: selectedWorld?.id === world.id,
            joined: isWorldJoined(world.id),
            recommended: world.isDefault
          }"
          @click="selectWorld(world)"
        >
          <div v-if="world.isDefault" class="recommended-banner">
            <svg viewBox="0 0 24 24" fill="currentColor">
              <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
            </svg>
            Recommended
          </div>

          <div class="world-badge" :class="world.difficulty.toLowerCase()">
            {{ world.difficulty }}
          </div>

          <h2 class="world-name">{{ world.name }}</h2>
          <p class="world-description">{{ world.description }}</p>

          <div class="world-stats">
            <div class="stat">
              <span class="stat-label">Starting Capital</span>
              <span class="stat-value">${{ formatNumber(world.startingCapital) }}</span>
            </div>
            <div class="stat">
              <span class="stat-label">Job Payouts</span>
              <span class="stat-value" :class="getMultiplierClass(world.jobPayoutMultiplier)">{{ formatMultiplier(world.jobPayoutMultiplier) }}</span>
            </div>
            <div class="stat">
              <span class="stat-label">Aircraft Prices</span>
              <span class="stat-value" :class="getMultiplierClass(world.aircraftPriceMultiplier, true)">{{ formatMultiplier(world.aircraftPriceMultiplier) }}</span>
            </div>
            <div class="stat">
              <span class="stat-label">Maintenance Costs</span>
              <span class="stat-value" :class="getMultiplierClass(world.maintenanceCostMultiplier, true)">{{ formatMultiplier(world.maintenanceCostMultiplier) }}</span>
            </div>
          </div>

          <div class="world-footer">
            <div class="player-count">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/>
                <circle cx="9" cy="7" r="4"/>
                <path d="M23 21v-2a4 4 0 0 0-3-3.87"/>
                <path d="M16 3.13a4 4 0 0 1 0 7.75"/>
              </svg>
              <span>{{ world.currentPlayers }} players</span>
            </div>
          </div>

          <div v-if="isWorldJoined(world.id)" class="joined-indicator">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/>
              <polyline points="22 4 12 14.01 9 11.01"/>
            </svg>
            <span>Already Joined</span>
          </div>
        </div>
      </div>

      <div class="actions">
        <v-btn
          color="primary"
          size="large"
          :disabled="!selectedWorld"
          :loading="joining"
          @click="handleJoinWorld"
        >
          {{ getButtonText() }}
        </v-btn>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useWorldStore } from '../stores/world'
import type { WorldResponse } from '../services/api'

const router = useRouter()
const worldStore = useWorldStore()

const selectedWorld = ref<WorldResponse | null>(null)
const joining = ref(false)

const worlds = worldStore.worlds
const playerWorlds = worldStore.playerWorlds
const isLoading = worldStore.isLoading
const error = worldStore.error

// Sort worlds: Easy, Medium, Hard
const sortedWorlds = computed(() => {
  const difficultyOrder: Record<string, number> = { 'Easy': 0, 'Medium': 1, 'Hard': 2 }
  return [...worlds.value].sort((a, b) => {
    const orderA = difficultyOrder[a.difficulty] ?? 99
    const orderB = difficultyOrder[b.difficulty] ?? 99
    return orderA - orderB
  })
})

onMounted(async () => {
  await loadData()
})

async function loadData() {
  await Promise.all([
    worldStore.loadWorlds(),
    worldStore.loadMyWorlds()
  ])

  // Pre-select Medium (recommended) world
  if (!selectedWorld.value && worlds.value.length > 0) {
    const mediumWorld = worlds.value.find(w => w.difficulty === 'Medium' || w.isDefault)
    selectedWorld.value = mediumWorld || worlds.value[0]
  }
}

function selectWorld(world: WorldResponse) {
  selectedWorld.value = world
}

function isWorldJoined(worldId: string): boolean {
  return playerWorlds.value.some(pw => pw.worldId === worldId)
}

function getButtonText(): string {
  if (!selectedWorld.value) return 'Select a World'
  if (isWorldJoined(selectedWorld.value.id)) return 'Continue Playing'
  return 'Start in This World'
}

async function handleJoinWorld() {
  if (!selectedWorld.value) return

  joining.value = true

  try {
    // Check if already joined
    const existingPlayerWorld = playerWorlds.value.find(
      pw => pw.worldId === selectedWorld.value!.id
    )

    if (existingPlayerWorld) {
      // Already joined, set as current and continue
      worldStore.setCurrentWorld(existingPlayerWorld)
      if (existingPlayerWorld.homeAirportId) {
        router.push('/dashboard')
      } else {
        router.push('/select-world-airport')
      }
    } else {
      // Join the world
      const playerWorld = await worldStore.joinWorld(selectedWorld.value.id)
      if (playerWorld) {
        router.push('/select-world-airport')
      }
    }
  } finally {
    joining.value = false
  }
}

function formatNumber(num: number): string {
  return new Intl.NumberFormat('en-US').format(num)
}

function formatMultiplier(value: number): string {
  const percentage = Math.round((value - 1) * 100)
  if (percentage === 0) return 'Standard'
  return percentage > 0 ? `+${percentage}%` : `${percentage}%`
}

function getMultiplierClass(value: number, inverted = false): string {
  const percentage = Math.round((value - 1) * 100)
  if (percentage === 0) return ''
  if (inverted) {
    return percentage > 0 ? 'negative' : 'positive'
  }
  return percentage > 0 ? 'positive' : 'negative'
}
</script>

<style scoped>
.world-selection {
  min-height: 100vh;
  background: var(--bg-primary);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 48px 24px;
}

.selection-container {
  max-width: 1200px;
  width: 100%;
}

.header {
  text-align: center;
  margin-bottom: 48px;
}

.title {
  font-size: 36px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 12px;
}

.subtitle {
  font-size: 16px;
  color: var(--text-secondary);
  max-width: 600px;
  margin: 0 auto 12px;
}

.subtitle strong {
  color: var(--accent-primary);
}

.hint {
  font-size: 14px;
  color: var(--text-muted);
  max-width: 500px;
  margin: 0 auto;
  padding: 12px 20px;
  background: var(--bg-secondary);
  border-radius: 8px;
  border: 1px solid var(--border-subtle);
}

.loading-state,
.error-state {
  text-align: center;
  padding: 64px;
  color: var(--text-secondary);
}

.loading-state p,
.error-state p {
  margin-top: 16px;
  font-size: 16px;
}

.error-state svg {
  width: 48px;
  height: 48px;
  color: var(--color-error);
}

.error-state .v-btn {
  margin-top: 16px;
}

.worlds-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 24px;
  margin-bottom: 48px;
}

@media (max-width: 1024px) {
  .worlds-grid {
    grid-template-columns: 1fr;
  }
}

.world-card {
  background: var(--bg-secondary);
  border: 2px solid var(--border-subtle);
  border-radius: 16px;
  padding: 24px;
  cursor: pointer;
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
}

.world-card:hover {
  border-color: var(--accent-primary);
  transform: translateY(-4px);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
}

.world-card.selected {
  border-color: var(--accent-primary);
  background: rgba(59, 130, 246, 0.05);
}

.world-card.recommended {
  border-color: #f59e0b;
}

.world-card.recommended.selected {
  border-color: var(--accent-primary);
}

.world-card.joined {
  border-color: var(--color-success);
}

.recommended-banner {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  background: linear-gradient(135deg, #f59e0b, #f97316);
  color: white;
  padding: 8px 16px;
  font-size: 12px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
}

.recommended-banner svg {
  width: 14px;
  height: 14px;
}

.world-card.recommended {
  padding-top: 56px;
}

.world-badge {
  display: inline-block;
  padding: 6px 14px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 16px;
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
  font-size: 24px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 8px;
}

.world-description {
  font-size: 14px;
  color: var(--text-secondary);
  margin-bottom: 20px;
  line-height: 1.5;
}

.world-stats {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px;
  margin-bottom: 20px;
}

.stat {
  background: var(--bg-elevated);
  border-radius: 8px;
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.stat-label {
  font-size: 11px;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.stat-value {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary);
}

.stat-value.positive {
  color: #22c55e;
}

.stat-value.negative {
  color: #ef4444;
}

.world-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding-top: 16px;
  border-top: 1px solid var(--border-subtle);
}

.player-count {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: var(--text-muted);
}

.player-count svg {
  width: 16px;
  height: 16px;
}

.joined-indicator {
  position: absolute;
  top: 16px;
  right: 16px;
  display: flex;
  align-items: center;
  gap: 6px;
  background: rgba(34, 197, 94, 0.15);
  color: #22c55e;
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
}

.world-card.recommended .joined-indicator {
  top: 48px;
}

.joined-indicator svg {
  width: 14px;
  height: 14px;
}

.actions {
  display: flex;
  justify-content: center;
}

.actions .v-btn {
  min-width: 200px;
  text-transform: none;
  font-weight: 600;
}
</style>
