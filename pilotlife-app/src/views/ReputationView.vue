<template>
  <div class="reputation-view">
    <div class="view-header">
      <div class="header-content">
        <h1 class="view-title">Reputation</h1>
        <p class="view-subtitle">Your standing as a professional pilot</p>
      </div>
    </div>

    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <p>Loading reputation...</p>
    </div>

    <div v-else-if="error" class="error-state">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <circle cx="12" cy="12" r="10"/>
        <line x1="12" y1="8" x2="12" y2="12"/>
        <line x1="12" y1="16" x2="12.01" y2="16"/>
      </svg>
      <p>{{ error }}</p>
      <button @click="loadReputation" class="retry-btn">Retry</button>
    </div>

    <div v-else-if="reputation" class="reputation-content">
      <!-- Main Reputation Card -->
      <div class="reputation-card">
        <div class="reputation-level" :class="getLevelClass(reputation.level)">
          <div class="level-badge">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>
            </svg>
            <span class="level-number">{{ reputation.level }}</span>
          </div>
          <h2 class="level-name">{{ reputation.levelName }}</h2>
          <p class="level-description">{{ getLevelDescription(reputation.level) }}</p>
        </div>

        <div class="reputation-score">
          <div class="score-display">
            <span class="score-value">{{ reputation.score.toFixed(2) }}</span>
            <span class="score-max">/ 5.0</span>
          </div>
          <div class="score-progress">
            <div class="progress-bar">
              <div
                class="progress-fill"
                :style="{ width: `${reputation.progressToNextLevel}%` }"
                :class="getLevelClass(reputation.level)"
              ></div>
            </div>
            <span class="progress-text">{{ reputation.progressToNextLevel.toFixed(0) }}% to next level</span>
          </div>
        </div>

        <div v-if="reputation.payoutBonus > 0" class="bonus-badge">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="12" cy="12" r="10"/>
            <path d="M16 8h-6a2 2 0 1 0 0 4h4a2 2 0 1 1 0 4H8"/>
            <line x1="12" y1="6" x2="12" y2="18"/>
          </svg>
          <span>+{{ reputation.payoutBonus }}% Payout Bonus</span>
        </div>
      </div>

      <!-- Stats Grid -->
      <div class="stats-grid">
        <div class="stat-card">
          <div class="stat-icon success">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <polyline points="20,6 9,17 4,12"/>
            </svg>
          </div>
          <div class="stat-content">
            <span class="stat-value">{{ reputation.onTimeDeliveries }}</span>
            <span class="stat-label">On-Time Deliveries</span>
          </div>
        </div>

        <div class="stat-card">
          <div class="stat-icon warning">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="12" cy="12" r="10"/>
              <polyline points="12,6 12,12 16,14"/>
            </svg>
          </div>
          <div class="stat-content">
            <span class="stat-value">{{ reputation.lateDeliveries }}</span>
            <span class="stat-label">Late Deliveries</span>
          </div>
        </div>

        <div class="stat-card">
          <div class="stat-icon danger">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="12" cy="12" r="10"/>
              <line x1="15" y1="9" x2="9" y2="15"/>
              <line x1="9" y1="9" x2="15" y2="15"/>
            </svg>
          </div>
          <div class="stat-content">
            <span class="stat-value">{{ reputation.failedDeliveries }}</span>
            <span class="stat-label">Failed Deliveries</span>
          </div>
        </div>

        <div class="stat-card">
          <div class="stat-icon info">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <line x1="12" y1="1" x2="12" y2="23"/>
              <path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/>
            </svg>
          </div>
          <div class="stat-content">
            <span class="stat-value">{{ (reputation.onTimeRate * 100).toFixed(0) }}%</span>
            <span class="stat-label">On-Time Rate</span>
          </div>
        </div>
      </div>

      <!-- Benefits Section -->
      <div class="benefits-section">
        <h2 class="section-title">Benefits & Unlocks</h2>
        <div class="benefits-grid">
          <div
            v-for="benefit in reputation.benefits"
            :key="benefit.name"
            class="benefit-card"
            :class="{ unlocked: benefit.isUnlocked }"
          >
            <div class="benefit-icon">
              <svg v-if="benefit.isUnlocked" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/>
                <polyline points="22,4 12,14.01 9,11.01"/>
              </svg>
              <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/>
                <path d="M7 11V7a5 5 0 0 1 10 0v4"/>
              </svg>
            </div>
            <div class="benefit-content">
              <h3 class="benefit-name">{{ benefit.name }}</h3>
              <p class="benefit-description">{{ benefit.description }}</p>
              <span v-if="!benefit.isUnlocked" class="benefit-requirement">
                Requires Level {{ benefit.requiredLevel }}
              </span>
            </div>
          </div>
        </div>
      </div>

      <!-- History Section -->
      <div v-if="history.length > 0" class="history-section">
        <h2 class="section-title">Recent Activity</h2>
        <div class="history-list">
          <div
            v-for="event in history"
            :key="event.id"
            class="history-item"
            :class="{ positive: event.pointChange > 0, negative: event.pointChange < 0 }"
          >
            <div class="history-icon">
              <svg v-if="event.pointChange > 0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <polyline points="18,15 12,9 6,15"/>
              </svg>
              <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <polyline points="6,9 12,15 18,9"/>
              </svg>
            </div>
            <div class="history-content">
              <div class="history-header">
                <span class="history-event">{{ formatEventType(event.eventType) }}</span>
                <span class="history-change" :class="{ positive: event.pointChange > 0, negative: event.pointChange < 0 }">
                  {{ event.pointChange > 0 ? '+' : '' }}{{ event.pointChange.toFixed(2) }}
                </span>
              </div>
              <p class="history-description">{{ event.description }}</p>
              <span class="history-time">{{ formatDate(event.occurredAt) }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { api, type ReputationStatusResponse, type ReputationEventResponse } from '../services/api'
import { useWorldStore } from '../stores/world'

const worldStore = useWorldStore()

const reputation = ref<ReputationStatusResponse | null>(null)
const history = ref<ReputationEventResponse[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

const currentWorldId = computed(() => worldStore.currentPlayerWorld.value?.worldId)

onMounted(async () => {
  await loadReputation()
})

async function loadReputation() {
  if (!currentWorldId.value) {
    error.value = 'Please select a world first'
    loading.value = false
    return
  }

  loading.value = true
  error.value = null

  try {
    const [statusResponse, historyResponse] = await Promise.all([
      api.reputation.getStatus(currentWorldId.value),
      api.reputation.getHistory(currentWorldId.value, 20)
    ])

    if (statusResponse.error) {
      error.value = statusResponse.error
    } else {
      reputation.value = statusResponse.data || null
    }

    if (historyResponse.data) {
      history.value = historyResponse.data
    }
  } catch (err) {
    error.value = 'Failed to load reputation'
    console.error('Error loading reputation:', err)
  } finally {
    loading.value = false
  }
}

function getLevelClass(level: number): string {
  const classes: Record<number, string> = {
    1: 'level-unreliable',
    2: 'level-novice',
    3: 'level-standard',
    4: 'level-trusted',
    5: 'level-elite'
  }
  return classes[level] || 'level-standard'
}

function getLevelDescription(level: number): string {
  const descriptions: Record<number, string> = {
    1: 'Your reliability needs improvement. Complete jobs on time to improve your standing.',
    2: 'You\'re building a reputation. Keep delivering on time to unlock more opportunities.',
    3: 'You\'re a standard pilot with good standing. Keep up the reliable work!',
    4: 'You\'re a trusted pilot with access to priority jobs and bonus payouts.',
    5: 'You\'re an elite pilot with maximum benefits and exclusive job access.'
  }
  return descriptions[level] || 'Keep working to improve your reputation.'
}

function formatEventType(eventType: string): string {
  const names: Record<string, string> = {
    'JobCompletedOnTime': 'On-Time Delivery',
    'JobCompletedEarly': 'Early Delivery',
    'JobCompletedLate': 'Late Delivery',
    'JobFailed': 'Job Failed',
    'JobCancelled': 'Job Cancelled',
    'SmoothLanding': 'Smooth Landing',
    'GoodLanding': 'Good Landing',
    'HardLanding': 'Hard Landing',
    'OverspeedViolation': 'Overspeed Violation',
    'StallWarning': 'Stall Warning',
    'Accident': 'Accident',
    'HighRiskJobCompleted': 'High-Risk Job Completed',
    'VipJobCompleted': 'VIP Job Completed',
    'InactivityDecay': 'Inactivity Decay'
  }
  return names[eventType] || eventType.replace(/([A-Z])/g, ' $1').trim()
}

function formatDate(dateStr: string): string {
  const date = new Date(dateStr)
  return date.toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}
</script>

<style scoped>
.reputation-view {
  padding: 32px;
  max-width: 1000px;
  margin: 0 auto;
}

.view-header {
  margin-bottom: 32px;
}

.view-title {
  font-size: 32px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 8px;
}

.view-subtitle {
  font-size: 16px;
  color: var(--text-secondary);
}

.loading-state,
.error-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 64px;
  text-align: center;
}

.spinner {
  width: 48px;
  height: 48px;
  border: 3px solid var(--border-subtle);
  border-top-color: var(--accent-primary);
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.error-state svg {
  width: 64px;
  height: 64px;
  color: var(--text-tertiary);
  margin-bottom: 16px;
}

.error-state p {
  color: var(--text-secondary);
  margin-bottom: 16px;
}

.retry-btn {
  background: var(--accent-primary);
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.2s ease;
}

.retry-btn:hover {
  background: var(--accent-hover);
}

.reputation-content {
  display: flex;
  flex-direction: column;
  gap: 32px;
}

.reputation-card {
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 20px;
  padding: 32px;
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
}

.reputation-level {
  margin-bottom: 24px;
}

.level-badge {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 16px;
  position: relative;
  background: var(--bg-elevated);
}

.level-badge svg {
  width: 48px;
  height: 48px;
  position: absolute;
}

.level-number {
  font-size: 24px;
  font-weight: 700;
  position: relative;
  z-index: 1;
}

.level-unreliable .level-badge { background: rgba(239, 68, 68, 0.15); color: #ef4444; }
.level-unreliable .level-badge svg { color: rgba(239, 68, 68, 0.3); }
.level-novice .level-badge { background: rgba(245, 158, 11, 0.15); color: #f59e0b; }
.level-novice .level-badge svg { color: rgba(245, 158, 11, 0.3); }
.level-standard .level-badge { background: rgba(59, 130, 246, 0.15); color: #3b82f6; }
.level-standard .level-badge svg { color: rgba(59, 130, 246, 0.3); }
.level-trusted .level-badge { background: rgba(34, 197, 94, 0.15); color: #22c55e; }
.level-trusted .level-badge svg { color: rgba(34, 197, 94, 0.3); }
.level-elite .level-badge { background: rgba(168, 85, 247, 0.15); color: #a855f7; }
.level-elite .level-badge svg { color: rgba(168, 85, 247, 0.3); }

.level-name {
  font-size: 28px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 8px;
}

.level-description {
  font-size: 14px;
  color: var(--text-secondary);
  max-width: 400px;
  line-height: 1.5;
}

.reputation-score {
  width: 100%;
  max-width: 400px;
  margin-bottom: 16px;
}

.score-display {
  display: flex;
  align-items: baseline;
  justify-content: center;
  margin-bottom: 12px;
}

.score-value {
  font-size: 48px;
  font-weight: 700;
  color: var(--text-primary);
}

.score-max {
  font-size: 20px;
  color: var(--text-tertiary);
  margin-left: 4px;
}

.score-progress {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.progress-bar {
  height: 8px;
  background: var(--bg-elevated);
  border-radius: 4px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  border-radius: 4px;
  transition: width 0.3s ease;
}

.progress-fill.level-unreliable { background: #ef4444; }
.progress-fill.level-novice { background: #f59e0b; }
.progress-fill.level-standard { background: #3b82f6; }
.progress-fill.level-trusted { background: #22c55e; }
.progress-fill.level-elite { background: #a855f7; }

.progress-text {
  font-size: 12px;
  color: var(--text-tertiary);
}

.bonus-badge {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  background: rgba(34, 197, 94, 0.1);
  border: 1px solid rgba(34, 197, 94, 0.3);
  color: #22c55e;
  padding: 10px 20px;
  border-radius: 100px;
  font-size: 14px;
  font-weight: 600;
}

.bonus-badge svg {
  width: 18px;
  height: 18px;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
}

.stat-card {
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 16px;
  padding: 20px;
  display: flex;
  align-items: center;
  gap: 16px;
}

.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.stat-icon svg {
  width: 24px;
  height: 24px;
}

.stat-icon.success { background: rgba(34, 197, 94, 0.1); color: #22c55e; }
.stat-icon.warning { background: rgba(245, 158, 11, 0.1); color: #f59e0b; }
.stat-icon.danger { background: rgba(239, 68, 68, 0.1); color: #ef4444; }
.stat-icon.info { background: rgba(59, 130, 246, 0.1); color: #3b82f6; }

.stat-content {
  display: flex;
  flex-direction: column;
}

.stat-value {
  font-size: 24px;
  font-weight: 700;
  color: var(--text-primary);
}

.stat-label {
  font-size: 13px;
  color: var(--text-secondary);
}

.section-title {
  font-size: 20px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 20px;
}

.benefits-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 16px;
}

.benefit-card {
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 16px;
  padding: 20px;
  display: flex;
  gap: 16px;
  opacity: 0.6;
  transition: all 0.2s ease;
}

.benefit-card.unlocked {
  opacity: 1;
  border-color: rgba(34, 197, 94, 0.3);
  background: linear-gradient(180deg, var(--bg-secondary) 0%, rgba(34, 197, 94, 0.05) 100%);
}

.benefit-icon {
  width: 40px;
  height: 40px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  background: var(--bg-elevated);
}

.benefit-icon svg {
  width: 20px;
  height: 20px;
  color: var(--text-tertiary);
}

.benefit-card.unlocked .benefit-icon {
  background: rgba(34, 197, 94, 0.1);
}

.benefit-card.unlocked .benefit-icon svg {
  color: #22c55e;
}

.benefit-content {
  flex: 1;
  min-width: 0;
}

.benefit-name {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 4px;
}

.benefit-description {
  font-size: 13px;
  color: var(--text-secondary);
  line-height: 1.4;
}

.benefit-requirement {
  display: inline-block;
  margin-top: 8px;
  font-size: 11px;
  color: var(--text-tertiary);
  background: var(--bg-elevated);
  padding: 2px 8px;
  border-radius: 4px;
}

.history-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.history-item {
  display: flex;
  align-items: flex-start;
  gap: 16px;
  padding: 16px;
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 12px;
}

.history-icon {
  width: 40px;
  height: 40px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  background: var(--bg-elevated);
}

.history-icon svg {
  width: 20px;
  height: 20px;
}

.history-item.positive .history-icon {
  background: rgba(34, 197, 94, 0.1);
}

.history-item.positive .history-icon svg {
  color: #22c55e;
}

.history-item.negative .history-icon {
  background: rgba(239, 68, 68, 0.1);
}

.history-item.negative .history-icon svg {
  color: #ef4444;
}

.history-content {
  flex: 1;
  min-width: 0;
}

.history-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 4px;
}

.history-event {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary);
}

.history-change {
  font-size: 14px;
  font-weight: 600;
}

.history-change.positive {
  color: #22c55e;
}

.history-change.negative {
  color: #ef4444;
}

.history-description {
  font-size: 13px;
  color: var(--text-secondary);
  margin-bottom: 4px;
}

.history-time {
  font-size: 12px;
  color: var(--text-tertiary);
}
</style>
