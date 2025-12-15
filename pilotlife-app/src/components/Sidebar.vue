<template>
  <aside class="sidebar">
    <div class="sidebar-header">
      <div class="logo-container">
        <img src="/pilotlife-icon.png" alt="PilotLife" class="sidebar-logo" />
        <span class="logo-text">PilotLife</span>
      </div>
    </div>

    <div class="world-selector-container">
      <WorldSelector v-if="worldStore.playerWorlds.value.length > 0" />
      <router-link v-else to="/select-world" class="join-world-prompt">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <circle cx="12" cy="12" r="10"/>
          <path d="M12 8v8M8 12h8"/>
        </svg>
        <span>Join a World</span>
      </router-link>
    </div>

    <nav class="sidebar-nav">
      <router-link to="/dashboard" class="nav-item" :class="{ active: isActive('/dashboard') }">
        <div class="nav-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/>
            <polyline points="9,22 9,12 15,12 15,22"/>
          </svg>
        </div>
        <span class="nav-label">Dashboard</span>
      </router-link>

      <router-link to="/jobs" class="nav-item" :class="{ active: isActive('/jobs') }">
        <div class="nav-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>
            <polyline points="14,2 14,8 20,8"/>
            <line x1="16" y1="13" x2="8" y2="13"/>
            <line x1="16" y1="17" x2="8" y2="17"/>
          </svg>
        </div>
        <span class="nav-label">Jobs</span>
      </router-link>

      <router-link to="/hangar" class="nav-item" :class="{ active: isActive('/hangar') }">
        <div class="nav-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M22 12h-4l-3 9L9 3l-3 9H2"/>
          </svg>
        </div>
        <span class="nav-label">Hangar</span>
      </router-link>

      <router-link to="/marketplace" class="nav-item" :class="{ active: isActive('/marketplace') }">
        <div class="nav-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M6 2L3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4z"/>
            <line x1="3" y1="6" x2="21" y2="6"/>
            <path d="M16 10a4 4 0 0 1-8 0"/>
          </svg>
        </div>
        <span class="nav-label">Marketplace</span>
      </router-link>

      <router-link to="/licenses" class="nav-item" :class="{ active: isActive('/licenses') }">
        <div class="nav-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <rect x="3" y="4" width="18" height="16" rx="2"/>
            <path d="M7 8h10"/>
            <path d="M7 12h6"/>
            <circle cx="16" cy="14" r="2"/>
          </svg>
        </div>
        <span class="nav-label">Licenses</span>
      </router-link>

      <router-link to="/banking" class="nav-item" :class="{ active: isActive('/banking') }">
        <div class="nav-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M3 21h18"/>
            <path d="M3 10h18"/>
            <path d="M5 6l7-3 7 3"/>
            <path d="M4 10v11"/>
            <path d="M20 10v11"/>
            <path d="M8 14v3"/>
            <path d="M12 14v3"/>
            <path d="M16 14v3"/>
          </svg>
        </div>
        <span class="nav-label">Banking</span>
      </router-link>

      <router-link to="/skills" class="nav-item" :class="{ active: isActive('/skills') }">
        <div class="nav-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <polygon points="12,2 15.09,8.26 22,9.27 17,14.14 18.18,21.02 12,17.77 5.82,21.02 7,14.14 2,9.27 8.91,8.26 12,2"/>
          </svg>
        </div>
        <span class="nav-label">Skills</span>
      </router-link>

      <router-link to="/reputation" class="nav-item" :class="{ active: isActive('/reputation') }">
        <div class="nav-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>
          </svg>
        </div>
        <span class="nav-label">Reputation</span>
      </router-link>

      <router-link to="/concept" class="nav-item concept" :class="{ active: isActive('/concept') }">
        <div class="nav-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/>
            <polyline points="3.27,6.96 12,12.01 20.73,6.96"/>
            <line x1="12" y1="22.08" x2="12" y2="12"/>
          </svg>
        </div>
        <span class="nav-label">3D Concept</span>
        <span class="beta-badge">BETA</span>
      </router-link>

      <div class="nav-divider"></div>

      <router-link to="/settings" class="nav-item" :class="{ active: isActive('/settings') }">
        <div class="nav-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="12" cy="12" r="3"/>
            <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1 0 2.83 2 2 0 0 1-2.83 0l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-2 2 2 2 0 0 1-2-2v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83 0 2 2 0 0 1 0-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1-2-2 2 2 0 0 1 2-2h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 0-2.83 2 2 0 0 1 2.83 0l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 2-2 2 2 0 0 1 2 2v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 0 2 2 0 0 1 0 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 2 2 2 2 0 0 1-2 2h-.09a1.65 1.65 0 0 0-1.51 1z"/>
          </svg>
        </div>
        <span class="nav-label">Settings</span>
      </router-link>

      <router-link v-if="settingsStore.settings.value.developerMode" to="/dev" class="nav-item dev" :class="{ active: isActive('/dev') }">
        <div class="nav-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <polyline points="16,18 22,12 16,6"/>
            <polyline points="8,6 2,12 8,18"/>
          </svg>
        </div>
        <span class="nav-label">Dev Console</span>
        <span class="dev-badge">DEV</span>
      </router-link>

      <router-link v-if="settingsStore.settings.value.adminMode" to="/admin" class="nav-item admin" :class="{ active: isActive('/admin') }">
        <div class="nav-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>
          </svg>
        </div>
        <span class="nav-label">Admin Panel</span>
        <span class="admin-badge">ADMIN</span>
      </router-link>
    </nav>

    <div class="sidebar-footer">
      <router-link to="/profile" class="user-card" :class="{ active: isActive('/profile') }">
        <div class="user-avatar">
          <span>{{ initials }}</span>
        </div>
        <div class="user-info">
          <span class="user-name">{{ fullName }}</span>
          <span class="user-balance">${{ formattedBalance }}</span>
        </div>
      </router-link>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useUserStore } from '../stores/user'
import { useSettingsStore } from '../stores/settings'
import { useWorldStore } from '../stores/world'
import WorldSelector from './WorldSelector.vue'

const route = useRoute()
const userStore = useUserStore()
const settingsStore = useSettingsStore()
const worldStore = useWorldStore()

onMounted(async () => {
  // Load worlds data if not already loaded
  if (worldStore.worlds.value.length === 0) {
    await worldStore.loadWorlds()
  }
  if (worldStore.playerWorlds.value.length === 0) {
    await worldStore.loadMyWorlds()
  }
})

const isActive = (path: string) => route.path === path

const fullName = computed(() => {
  if (!userStore.user.value) return ''
  return `${userStore.user.value.firstName} ${userStore.user.value.lastName}`
})

const initials = computed(() => {
  if (!userStore.user.value) return ''
  return `${userStore.user.value.firstName[0]}${userStore.user.value.lastName[0]}`.toUpperCase()
})

const formattedBalance = computed(() => {
  // Use world-specific balance from currentPlayerWorld
  const balance = worldStore.currentPlayerWorld.value?.balance ?? userStore.user.value?.balance ?? 0
  return balance.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 })
})
</script>

<style scoped>
.sidebar {
  width: 240px;
  height: 100%;
  background: var(--bg-secondary);
  border-right: 1px solid var(--border-subtle);
  display: flex;
  flex-direction: column;
  flex-shrink: 0;
}

.sidebar-header {
  padding: 20px;
  border-bottom: 1px solid var(--border-subtle);
}

.logo-container {
  display: flex;
  align-items: center;
  gap: 12px;
}

.sidebar-logo {
  width: 36px;
  height: 36px;
  border-radius: 8px;
}

.logo-text {
  font-size: 18px;
  font-weight: 700;
  color: var(--text-primary);
}

.world-selector-container {
  padding: 12px;
  border-bottom: 1px solid var(--border-subtle);
}

.join-world-prompt {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px 16px;
  background: rgba(59, 130, 246, 0.1);
  border: 1px dashed var(--accent-primary);
  border-radius: 10px;
  color: var(--accent-primary);
  text-decoration: none;
  transition: all 0.2s ease;
}

.join-world-prompt:hover {
  background: rgba(59, 130, 246, 0.2);
  border-style: solid;
}

.join-world-prompt svg {
  width: 20px;
  height: 20px;
}

.join-world-prompt span {
  font-size: 14px;
  font-weight: 500;
}

.sidebar-nav {
  flex: 1;
  padding: 16px 12px;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 16px;
  border-radius: 10px;
  color: var(--text-secondary);
  text-decoration: none;
  transition: all 0.2s ease;
}

.nav-item:hover {
  background: var(--bg-elevated);
  color: var(--text-primary);
}

.nav-item.active {
  background: rgba(59, 130, 246, 0.1);
  color: var(--accent-primary);
}

.nav-item.active .nav-icon {
  color: var(--accent-primary);
}

.nav-icon {
  width: 20px;
  height: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.nav-icon svg {
  width: 20px;
  height: 20px;
}

.nav-label {
  font-size: 14px;
  font-weight: 500;
}

.nav-divider {
  height: 1px;
  background: var(--border-subtle);
  margin: 12px 0;
}

.nav-item.concept {
  position: relative;
}

.beta-badge {
  font-size: 9px;
  font-weight: 700;
  background: linear-gradient(135deg, #f59e0b, #ef4444);
  color: white;
  padding: 2px 6px;
  border-radius: 4px;
  margin-left: auto;
  letter-spacing: 0.5px;
}

.dev-badge {
  font-size: 9px;
  font-weight: 700;
  background: linear-gradient(135deg, #22c55e, #06b6d4);
  color: white;
  padding: 2px 6px;
  border-radius: 4px;
  margin-left: auto;
  letter-spacing: 0.5px;
}

.admin-badge {
  font-size: 9px;
  font-weight: 700;
  background: linear-gradient(135deg, #ef4444, #f59e0b);
  color: white;
  padding: 2px 6px;
  border-radius: 4px;
  margin-left: auto;
  letter-spacing: 0.5px;
}

.sidebar-footer {
  padding: 12px;
  border-top: 1px solid var(--border-subtle);
}

.user-card {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  border-radius: 10px;
  text-decoration: none;
  transition: all 0.2s ease;
}

.user-card:hover {
  background: var(--bg-elevated);
}

.user-card.active {
  background: rgba(59, 130, 246, 0.1);
}

.user-avatar {
  width: 40px;
  height: 40px;
  background: linear-gradient(135deg, var(--accent-primary), var(--accent-secondary));
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 14px;
  font-weight: 600;
}

.user-info {
  display: flex;
  flex-direction: column;
}

.user-name {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary);
}

.user-balance {
  font-size: 12px;
  color: #22c55e;
  font-weight: 500;
}
</style>
