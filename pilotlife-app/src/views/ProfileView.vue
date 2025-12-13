<template>
  <div class="profile-view">
    <div class="profile-card">
      <div class="profile-header">
        <div class="avatar">
          <span class="avatar-initials">{{ initials }}</span>
        </div>
        <div class="profile-info">
          <h1 class="profile-name">{{ fullName }}</h1>
          <p class="profile-email">{{ userStore.user.value?.email }}</p>
          <span class="experience-badge">{{ userStore.user.value?.experienceLevel || 'Rookie' }}</span>
        </div>
      </div>

      <div class="stats-grid">
        <div class="stat-item">
          <div class="stat-icon balance">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="12" cy="12" r="10"/>
              <path d="M12 6v12M8 10h8M8 14h8"/>
            </svg>
          </div>
          <div class="stat-details">
            <span class="stat-label">Balance</span>
            <span class="stat-value">${{ formatBalance(userStore.user.value?.balance || 0) }}</span>
          </div>
        </div>

        <div class="stat-item">
          <div class="stat-icon flight-time">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="12" cy="12" r="10"/>
              <polyline points="12,6 12,12 16,14"/>
            </svg>
          </div>
          <div class="stat-details">
            <span class="stat-label">Total Flight Time</span>
            <span class="stat-value">{{ formatFlightTime(userStore.user.value?.totalFlightMinutes || 0) }}</span>
          </div>
        </div>

        <div class="stat-item">
          <div class="stat-icon home">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/>
              <polyline points="9,22 9,12 15,12 15,22"/>
            </svg>
          </div>
          <div class="stat-details">
            <span class="stat-label">Home Airport</span>
            <span class="stat-value">{{ homeAirport }}</span>
          </div>
        </div>

        <div class="stat-item">
          <div class="stat-icon location">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/>
              <circle cx="12" cy="10" r="3"/>
            </svg>
          </div>
          <div class="stat-details">
            <span class="stat-label">Current Location</span>
            <span class="stat-value">{{ currentLocation }}</span>
          </div>
        </div>
      </div>

      <div class="profile-actions">
        <v-btn
          color="error"
          variant="outlined"
          class="action-btn"
          @click="handleLogout"
        >
          Sign Out
        </v-btn>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '../stores/user'
import { api } from '../services/api'

const router = useRouter()
const userStore = useUserStore()

const fullName = computed(() => {
  if (!userStore.user.value) return ''
  return `${userStore.user.value.firstName} ${userStore.user.value.lastName}`
})

const initials = computed(() => {
  if (!userStore.user.value) return ''
  return `${userStore.user.value.firstName[0]}${userStore.user.value.lastName[0]}`.toUpperCase()
})

const homeAirport = computed(() => {
  if (userStore.user.value?.homeAirport) {
    const airport = userStore.user.value.homeAirport
    return airport.iataCode || airport.ident
  }
  return 'Not Set'
})

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
  if (hours === 0) return `${mins} minutes`
  return `${hours}h ${mins}m`
}

async function handleLogout() {
  await api.auth.logout()
  userStore.clearUser()
  router.push('/')
}
</script>

<style scoped>
.profile-view {
  min-height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--bg-primary);
  padding: 48px;
}

.profile-card {
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 20px;
  padding: 40px;
  width: 100%;
  max-width: 500px;
}

.profile-header {
  display: flex;
  align-items: center;
  gap: 20px;
  margin-bottom: 32px;
  padding-bottom: 24px;
  border-bottom: 1px solid var(--border-subtle);
}

.avatar {
  width: 80px;
  height: 80px;
  background: linear-gradient(135deg, var(--accent-primary), var(--accent-secondary));
  border-radius: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.avatar-initials {
  font-size: 28px;
  font-weight: 700;
  color: white;
}

.profile-info {
  flex: 1;
}

.profile-name {
  font-size: 24px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 4px;
}

.profile-email {
  font-size: 14px;
  color: var(--text-secondary);
  margin-bottom: 8px;
}

.experience-badge {
  display: inline-block;
  background: rgba(59, 130, 246, 0.1);
  border: 1px solid rgba(59, 130, 246, 0.3);
  color: var(--accent-primary);
  padding: 4px 12px;
  border-radius: 100px;
  font-size: 12px;
  font-weight: 600;
}

.stats-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
  margin-bottom: 32px;
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 12px;
  background: var(--bg-elevated);
  border-radius: 12px;
  padding: 16px;
}

.stat-icon {
  width: 44px;
  height: 44px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.stat-icon svg {
  width: 22px;
  height: 22px;
}

.stat-icon.balance {
  background: rgba(34, 197, 94, 0.15);
  color: #22c55e;
}

.stat-icon.flight-time {
  background: rgba(59, 130, 246, 0.15);
  color: #3b82f6;
}

.stat-icon.home {
  background: rgba(245, 158, 11, 0.15);
  color: #f59e0b;
}

.stat-icon.location {
  background: rgba(168, 85, 247, 0.15);
  color: #a855f7;
}

.stat-details {
  display: flex;
  flex-direction: column;
}

.stat-label {
  font-size: 11px;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.stat-value {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-primary);
}

.profile-actions {
  display: flex;
  justify-content: flex-end;
}

.action-btn {
  text-transform: none;
}
</style>
