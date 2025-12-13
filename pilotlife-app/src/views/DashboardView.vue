<template>
  <div class="dashboard">
    <div class="dashboard-header">
      <div class="welcome-section">
        <h1 class="welcome-title">Welcome back, {{ userStore.user.value?.firstName || 'Pilot' }}!</h1>
        <p class="welcome-subtitle">Ready for your next flight?</p>
      </div>
      <div class="user-stats">
        <div class="stat-card">
          <div class="stat-icon balance-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="12" cy="12" r="10"/>
              <path d="M12 6v12M8 10h8M8 14h8"/>
            </svg>
          </div>
          <div class="stat-content">
            <span class="stat-label">Balance</span>
            <span class="stat-value">${{ formatBalance(userStore.user.value?.balance || 0) }}</span>
          </div>
        </div>
        <div class="stat-card">
          <div class="stat-icon time-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="12" cy="12" r="10"/>
              <polyline points="12,6 12,12 16,14"/>
            </svg>
          </div>
          <div class="stat-content">
            <span class="stat-label">Flight Time</span>
            <span class="stat-value">{{ formatFlightTime(userStore.user.value?.totalFlightMinutes || 0) }}</span>
          </div>
        </div>
        <div class="stat-card">
          <div class="stat-icon location-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/>
              <circle cx="12" cy="10" r="3"/>
            </svg>
          </div>
          <div class="stat-content">
            <span class="stat-label">Location</span>
            <span class="stat-value location-value">{{ currentLocation }}</span>
          </div>
        </div>
      </div>
    </div>

    <div class="dashboard-grid">
      <button class="action-card primary" @click="navigateTo('jobs')">
        <div class="card-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>
            <polyline points="14,2 14,8 20,8"/>
            <line x1="16" y1="13" x2="8" y2="13"/>
            <line x1="16" y1="17" x2="8" y2="17"/>
            <polyline points="10,9 9,9 8,9"/>
          </svg>
        </div>
        <div class="card-content">
          <h3 class="card-title">Find a Job</h3>
          <p class="card-description">Browse available cargo and passenger flights</p>
        </div>
        <div class="card-arrow">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <line x1="5" y1="12" x2="19" y2="12"/>
            <polyline points="12,5 19,12 12,19"/>
          </svg>
        </div>
      </button>

      <button class="action-card" @click="navigateTo('hangar')">
        <div class="card-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M22 12h-4l-3 9L9 3l-3 9H2"/>
          </svg>
        </div>
        <div class="card-content">
          <h3 class="card-title">View Hangar</h3>
          <p class="card-description">Manage your aircraft collection</p>
        </div>
        <div class="card-arrow">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <line x1="5" y1="12" x2="19" y2="12"/>
            <polyline points="12,5 19,12 12,19"/>
          </svg>
        </div>
      </button>

      <button class="action-card" @click="navigateTo('skills')">
        <div class="card-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <polygon points="12,2 15.09,8.26 22,9.27 17,14.14 18.18,21.02 12,17.77 5.82,21.02 7,14.14 2,9.27 8.91,8.26 12,2"/>
          </svg>
        </div>
        <div class="card-content">
          <h3 class="card-title">Skill Points</h3>
          <p class="card-description">Upgrade your pilot abilities</p>
        </div>
        <div class="card-arrow">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <line x1="5" y1="12" x2="19" y2="12"/>
            <polyline points="12,5 19,12 12,19"/>
          </svg>
        </div>
      </button>

      <button class="action-card" @click="navigateTo('profile')">
        <div class="card-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/>
            <circle cx="12" cy="7" r="4"/>
          </svg>
        </div>
        <div class="card-content">
          <h3 class="card-title">Profile</h3>
          <p class="card-description">View and edit your pilot profile</p>
        </div>
        <div class="card-arrow">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <line x1="5" y1="12" x2="19" y2="12"/>
            <polyline points="12,5 19,12 12,19"/>
          </svg>
        </div>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '../stores/user'

const router = useRouter()
const userStore = useUserStore()

const currentLocation = computed(() => {
  if (userStore.user.value?.currentAirport) {
    const airport = userStore.user.value.currentAirport
    return airport.iataCode || airport.ident
  }
  return 'Not Set'
})

function formatBalance(balance: number): string {
  return balance.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 })
}

function formatFlightTime(minutes: number): string {
  const hours = Math.floor(minutes / 60)
  const mins = minutes % 60
  if (hours === 0) return `${mins}m`
  return `${hours}h ${mins}m`
}

function navigateTo(route: string) {
  router.push(`/${route}`)
}
</script>

<style scoped>
.dashboard {
  min-height: 100%;
  background: var(--bg-primary);
  padding: 32px 48px;
}

.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 40px;
}

.welcome-section {
  flex: 1;
}

.welcome-title {
  font-size: 32px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 8px;
}

.welcome-subtitle {
  font-size: 16px;
  color: var(--text-secondary);
}

.user-stats {
  display: flex;
  gap: 16px;
}

.stat-card {
  display: flex;
  align-items: center;
  gap: 12px;
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 12px;
  padding: 16px 20px;
  min-width: 160px;
}

.stat-icon {
  width: 40px;
  height: 40px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.stat-icon svg {
  width: 22px;
  height: 22px;
}

.balance-icon {
  background: rgba(34, 197, 94, 0.15);
  color: #22c55e;
}

.time-icon {
  background: rgba(59, 130, 246, 0.15);
  color: #3b82f6;
}

.location-icon {
  background: rgba(168, 85, 247, 0.15);
  color: #a855f7;
}

.stat-content {
  display: flex;
  flex-direction: column;
}

.stat-label {
  font-size: 12px;
  color: var(--text-muted);
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.stat-value {
  font-size: 18px;
  font-weight: 700;
  color: var(--text-primary);
}

.location-value {
  font-family: var(--font-mono);
  font-size: 16px;
}

.dashboard-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 20px;
}

.action-card {
  display: flex;
  align-items: center;
  gap: 20px;
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 16px;
  padding: 28px 32px;
  cursor: pointer;
  transition: all 0.3s ease;
  text-align: left;
  width: 100%;
}

.action-card:hover {
  border-color: var(--accent-primary);
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
}

.action-card.primary {
  background: linear-gradient(135deg, var(--accent-primary), var(--accent-secondary));
  border: none;
}

.action-card.primary .card-title,
.action-card.primary .card-description,
.action-card.primary .card-icon,
.action-card.primary .card-arrow {
  color: white;
}

.action-card.primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 32px rgba(59, 130, 246, 0.3);
}

.card-icon {
  width: 56px;
  height: 56px;
  background: var(--bg-elevated);
  border-radius: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.action-card.primary .card-icon {
  background: rgba(255, 255, 255, 0.2);
}

.card-icon svg {
  width: 28px;
  height: 28px;
  color: var(--accent-primary);
}

.action-card.primary .card-icon svg {
  color: white;
}

.card-content {
  flex: 1;
}

.card-title {
  font-size: 18px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 4px;
}

.card-description {
  font-size: 14px;
  color: var(--text-secondary);
}

.card-arrow {
  color: var(--text-muted);
  transition: transform 0.2s ease;
}

.card-arrow svg {
  width: 20px;
  height: 20px;
}

.action-card:hover .card-arrow {
  transform: translateX(4px);
}
</style>
