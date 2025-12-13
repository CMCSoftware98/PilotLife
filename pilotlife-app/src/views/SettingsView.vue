<template>
  <div class="settings-view">
    <div class="settings-header">
      <h1 class="settings-title">Settings</h1>
      <p class="settings-subtitle">Manage your application preferences</p>
    </div>

    <div class="settings-content">
      <div class="settings-section">
        <h2 class="section-title">Account</h2>
        <div class="settings-card">
          <div class="setting-item">
            <div class="setting-info">
              <span class="setting-label">Email</span>
              <span class="setting-value">{{ userStore.user.value?.email }}</span>
            </div>
          </div>
          <div class="setting-item">
            <div class="setting-info">
              <span class="setting-label">Experience Level</span>
              <span class="setting-value">{{ userStore.user.value?.experienceLevel || 'Not set' }}</span>
            </div>
          </div>
        </div>
      </div>

      <div class="settings-section">
        <h2 class="section-title">Appearance</h2>
        <div class="settings-card">
          <div class="setting-item">
            <div class="setting-info">
              <span class="setting-label">Theme</span>
              <span class="setting-description">Choose your preferred color scheme</span>
            </div>
            <div class="setting-control">
              <div class="theme-badge">Dark</div>
            </div>
          </div>
        </div>
      </div>

      <div class="settings-section">
        <h2 class="section-title">Danger Zone</h2>
        <div class="settings-card danger">
          <div class="setting-item">
            <div class="setting-info">
              <span class="setting-label">Sign Out</span>
              <span class="setting-description">Sign out of your account on this device</span>
            </div>
            <v-btn
              color="error"
              variant="outlined"
              size="small"
              @click="handleLogout"
            >
              Sign Out
            </v-btn>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useUserStore } from '../stores/user'
import { api } from '../services/api'

const router = useRouter()
const userStore = useUserStore()

async function handleLogout() {
  await api.auth.logout()
  userStore.clearUser()
  router.push('/')
}
</script>

<style scoped>
.settings-view {
  min-height: 100%;
  padding: 32px 48px;
}

.settings-header {
  margin-bottom: 32px;
}

.settings-title {
  font-size: 28px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 8px;
}

.settings-subtitle {
  font-size: 15px;
  color: var(--text-secondary);
}

.settings-content {
  max-width: 600px;
}

.settings-section {
  margin-bottom: 32px;
}

.section-title {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 12px;
}

.settings-card {
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 12px;
  overflow: hidden;
}

.settings-card.danger {
  border-color: rgba(239, 68, 68, 0.3);
}

.setting-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  border-bottom: 1px solid var(--border-subtle);
}

.setting-item:last-child {
  border-bottom: none;
}

.setting-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.setting-label {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary);
}

.setting-value {
  font-size: 14px;
  color: var(--text-secondary);
}

.setting-description {
  font-size: 13px;
  color: var(--text-muted);
}

.theme-badge {
  background: var(--bg-elevated);
  color: var(--text-secondary);
  padding: 6px 12px;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 500;
}
</style>
