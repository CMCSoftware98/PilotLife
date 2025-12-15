<template>
  <div class="skills-view">
    <div class="view-header">
      <div class="header-content">
        <h1 class="view-title">Skills</h1>
        <p class="view-subtitle">Track your pilot abilities and progress</p>
      </div>
      <div class="total-level" v-if="totalLevel > 0">
        <span class="total-label">Total Level</span>
        <span class="total-value">{{ totalLevel }}</span>
      </div>
    </div>

    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <p>Loading skills...</p>
    </div>

    <div v-else-if="error" class="error-state">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <circle cx="12" cy="12" r="10"/>
        <line x1="12" y1="8" x2="12" y2="12"/>
        <line x1="12" y1="16" x2="12.01" y2="16"/>
      </svg>
      <p>{{ error }}</p>
      <button @click="loadSkills" class="retry-btn">Retry</button>
    </div>

    <div v-else-if="skills.length === 0" class="empty-state">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
        <polygon points="12,2 15.09,8.26 22,9.27 17,14.14 18.18,21.02 12,17.77 5.82,21.02 7,14.14 2,9.27 8.91,8.26 12,2"/>
      </svg>
      <h2>No Skills Yet</h2>
      <p>Complete flights and jobs to start gaining skill XP!</p>
    </div>

    <div v-else class="skills-grid">
      <div v-for="skill in skills" :key="skill.skillId" class="skill-card" :class="{ 'max-level': skill.isMaxLevel }">
        <div class="skill-header">
          <div class="skill-icon" :style="{ background: getSkillColor(skill.skillType) }">
            <component :is="getSkillIcon(skill.skillType)" />
          </div>
          <div class="skill-info">
            <h3 class="skill-name">{{ skill.skillName }}</h3>
            <span class="skill-level-badge">{{ skill.levelName }}</span>
          </div>
          <div class="skill-level">
            <span class="level-number">{{ skill.level }}</span>
            <span class="level-label">Level</span>
          </div>
        </div>

        <p class="skill-description">{{ skill.description }}</p>

        <div class="skill-progress">
          <div class="progress-bar">
            <div
              class="progress-fill"
              :style="{ width: `${skill.progressToNextLevel}%`, background: getSkillColor(skill.skillType) }"
            ></div>
          </div>
          <div class="progress-info">
            <span class="xp-current">{{ formatNumber(skill.currentXp) }} XP</span>
            <span class="xp-next" v-if="!skill.isMaxLevel">/ {{ formatNumber(skill.xpForNextLevel) }} XP</span>
            <span class="xp-next" v-else>MAX</span>
          </div>
        </div>
      </div>
    </div>

    <!-- XP History Section -->
    <div v-if="xpHistory.length > 0" class="history-section">
      <h2 class="section-title">Recent XP Gains</h2>
      <div class="history-list">
        <div v-for="event in xpHistory" :key="event.id" class="history-item">
          <div class="history-icon" :class="{ 'level-up': event.causedLevelUp }">
            <svg v-if="event.causedLevelUp" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <polyline points="18,15 12,9 6,15"/>
            </svg>
            <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <polygon points="12,2 15.09,8.26 22,9.27 17,14.14 18.18,21.02 12,17.77 5.82,21.02 7,14.14 2,9.27 8.91,8.26 12,2"/>
            </svg>
          </div>
          <div class="history-content">
            <div class="history-header">
              <span class="history-skill">{{ formatSkillType(event.skillType) }}</span>
              <span class="history-xp">+{{ event.xpGained }} XP</span>
            </div>
            <p class="history-description">{{ event.source }} - {{ event.description }}</p>
            <span class="history-time">{{ formatDate(event.occurredAt) }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, h } from 'vue'
import { api, type PlayerSkillResponse, type SkillXpEventResponse } from '../services/api'
import { useWorldStore } from '../stores/world'

const worldStore = useWorldStore()

const skills = ref<PlayerSkillResponse[]>([])
const xpHistory = ref<SkillXpEventResponse[]>([])
const totalLevel = ref(0)
const loading = ref(true)
const error = ref<string | null>(null)

const currentWorldId = computed(() => worldStore.currentPlayerWorld.value?.worldId)

onMounted(async () => {
  await loadSkills()
})

async function loadSkills() {
  if (!currentWorldId.value) {
    error.value = 'Please select a world first'
    loading.value = false
    return
  }

  loading.value = true
  error.value = null

  try {
    const [skillsResponse, historyResponse, totalResponse] = await Promise.all([
      api.skills.getAll(currentWorldId.value),
      api.skills.getHistory(currentWorldId.value, { limit: 10 }),
      api.skills.getTotal(currentWorldId.value)
    ])

    if (skillsResponse.error) {
      error.value = skillsResponse.error
    } else {
      skills.value = skillsResponse.data || []
    }

    if (historyResponse.data) {
      xpHistory.value = historyResponse.data
    }

    if (totalResponse.data) {
      totalLevel.value = totalResponse.data.totalLevel
    }
  } catch (err) {
    error.value = 'Failed to load skills'
    console.error('Error loading skills:', err)
  } finally {
    loading.value = false
  }
}

function formatNumber(num: number): string {
  return num.toLocaleString()
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

function formatSkillType(skillType: string): string {
  return skillType.replace(/([A-Z])/g, ' $1').trim()
}

function getSkillColor(skillType: string): string {
  const colors: Record<string, string> = {
    'Piloting': 'linear-gradient(135deg, #3b82f6, #1d4ed8)',
    'Navigation': 'linear-gradient(135deg, #22c55e, #16a34a)',
    'CargoHandling': 'linear-gradient(135deg, #f59e0b, #d97706)',
    'PassengerService': 'linear-gradient(135deg, #ec4899, #db2777)',
    'AircraftKnowledge': 'linear-gradient(135deg, #8b5cf6, #7c3aed)',
    'WeatherFlying': 'linear-gradient(135deg, #06b6d4, #0891b2)',
    'NightFlying': 'linear-gradient(135deg, #6366f1, #4f46e5)',
    'MountainFlying': 'linear-gradient(135deg, #78716c, #57534e)'
  }
  return colors[skillType] || 'linear-gradient(135deg, #6b7280, #4b5563)'
}

function getSkillIcon(skillType: string) {
  const icons: Record<string, () => any> = {
    'Piloting': () => h('svg', { viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2' }, [
      h('path', { d: 'M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5' })
    ]),
    'Navigation': () => h('svg', { viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2' }, [
      h('polygon', { points: '3,11 22,2 13,21 11,13 3,11' })
    ]),
    'CargoHandling': () => h('svg', { viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2' }, [
      h('path', { d: 'M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z' })
    ]),
    'PassengerService': () => h('svg', { viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2' }, [
      h('path', { d: 'M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2' }),
      h('circle', { cx: '9', cy: '7', r: '4' }),
      h('path', { d: 'M23 21v-2a4 4 0 0 0-3-3.87' }),
      h('path', { d: 'M16 3.13a4 4 0 0 1 0 7.75' })
    ]),
    'AircraftKnowledge': () => h('svg', { viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2' }, [
      h('path', { d: 'M4 19.5A2.5 2.5 0 0 1 6.5 17H20' }),
      h('path', { d: 'M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z' })
    ]),
    'WeatherFlying': () => h('svg', { viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2' }, [
      h('path', { d: 'M18 10h-1.26A8 8 0 1 0 9 20h9a5 5 0 0 0 0-10z' })
    ]),
    'NightFlying': () => h('svg', { viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2' }, [
      h('path', { d: 'M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z' })
    ]),
    'MountainFlying': () => h('svg', { viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2' }, [
      h('path', { d: 'm8 3 4 8 5-5 5 15H2L8 3z' })
    ])
  }
  return icons[skillType] || icons['Piloting']
}
</script>

<style scoped>
.skills-view {
  padding: 32px;
  max-width: 1200px;
  margin: 0 auto;
}

.view-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
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

.total-level {
  display: flex;
  flex-direction: column;
  align-items: center;
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 16px;
  padding: 16px 24px;
}

.total-label {
  font-size: 12px;
  color: var(--text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 4px;
}

.total-value {
  font-size: 32px;
  font-weight: 700;
  color: var(--accent-primary);
}

.loading-state,
.error-state,
.empty-state {
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

.error-state svg,
.empty-state svg {
  width: 64px;
  height: 64px;
  color: var(--text-tertiary);
  margin-bottom: 16px;
}

.error-state p,
.empty-state p {
  color: var(--text-secondary);
  margin-bottom: 16px;
}

.empty-state h2 {
  font-size: 24px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 8px;
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

.skills-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 20px;
  margin-bottom: 40px;
}

.skill-card {
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 16px;
  padding: 24px;
  transition: all 0.2s ease;
}

.skill-card:hover {
  border-color: var(--border-hover);
  transform: translateY(-2px);
}

.skill-card.max-level {
  border-color: rgba(245, 158, 11, 0.5);
  background: linear-gradient(180deg, var(--bg-secondary) 0%, rgba(245, 158, 11, 0.05) 100%);
}

.skill-header {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 16px;
}

.skill-icon {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.skill-icon svg {
  width: 24px;
  height: 24px;
  color: white;
}

.skill-info {
  flex: 1;
  min-width: 0;
}

.skill-name {
  font-size: 18px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 4px;
}

.skill-level-badge {
  display: inline-block;
  font-size: 12px;
  color: var(--text-secondary);
  background: var(--bg-elevated);
  padding: 2px 8px;
  border-radius: 4px;
}

.skill-level {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 8px 16px;
  background: var(--bg-elevated);
  border-radius: 12px;
}

.level-number {
  font-size: 24px;
  font-weight: 700;
  color: var(--text-primary);
}

.level-label {
  font-size: 10px;
  color: var(--text-tertiary);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.skill-description {
  font-size: 14px;
  color: var(--text-secondary);
  line-height: 1.5;
  margin-bottom: 16px;
}

.skill-progress {
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

.progress-info {
  display: flex;
  justify-content: space-between;
  font-size: 12px;
}

.xp-current {
  color: var(--text-primary);
  font-weight: 500;
}

.xp-next {
  color: var(--text-tertiary);
}

.history-section {
  margin-top: 40px;
}

.section-title {
  font-size: 20px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 20px;
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
  background: var(--bg-elevated);
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.history-icon svg {
  width: 20px;
  height: 20px;
  color: var(--text-secondary);
}

.history-icon.level-up {
  background: rgba(34, 197, 94, 0.1);
}

.history-icon.level-up svg {
  color: #22c55e;
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

.history-skill {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary);
}

.history-xp {
  font-size: 14px;
  font-weight: 600;
  color: #22c55e;
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
