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
        <h2 class="section-title">MSFS Integration</h2>
        <div class="settings-card">
          <div class="setting-item">
            <div class="setting-info">
              <span class="setting-label">UserCfg.opt Location</span>
              <span class="setting-value path-value" :class="{ 'not-found': !msfsPaths?.userCfgOptPath }">
                {{ msfsPaths?.userCfgOptPath || 'Not found' }}
              </span>
            </div>
            <div class="status-indicator" :class="msfsPaths?.userCfgOptPath ? 'found' : 'not-found'">
              {{ msfsPaths?.userCfgOptPath ? 'Found' : 'Not Found' }}
            </div>
          </div>
          <div class="setting-item">
            <div class="setting-info">
              <span class="setting-label">Community Folder</span>
              <span class="setting-value path-value" :class="{ 'not-found': !communityFolder }">
                {{ communityFolder || 'Not found' }}
              </span>
            </div>
            <div class="status-indicator" :class="communityFolder ? 'found' : 'not-found'">
              {{ communityFolder ? 'Found' : 'Not Found' }}
            </div>
          </div>
          <div class="setting-item">
            <div class="setting-info">
              <span class="setting-label">Indexed Aircraft</span>
              <span class="setting-value">{{ msfsPaths?.indexedAircraftCount ?? 0 }} variants</span>
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
        <h2 class="section-title">Developer</h2>
        <div class="settings-card">
          <div class="setting-item">
            <div class="setting-info">
              <span class="setting-label">Developer Mode</span>
              <span class="setting-description">Enable developer console to view connector and flight data</span>
            </div>
            <div class="setting-control">
              <label class="toggle-switch">
                <input
                  type="checkbox"
                  :checked="settingsStore.settings.value.developerMode"
                  @change="toggleDeveloperMode"
                >
                <span class="toggle-slider"></span>
              </label>
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
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '../stores/user'
import { useSettingsStore } from '../stores/settings'
import { api } from '../services/api'
import { connector, type MSFSPathsInfo } from '../services/connector'

const router = useRouter()
const userStore = useUserStore()
const settingsStore = useSettingsStore()

const msfsPaths = ref<MSFSPathsInfo | null>(null)

// Get the community folder from search paths (the one that ends with \Community)
const communityFolder = computed(() => {
  if (!msfsPaths.value?.searchPaths) return null
  return msfsPaths.value.searchPaths.find(path => path.toLowerCase().endsWith('\\community')) || null
})

let unsubscribePaths: (() => void) | null = null

onMounted(() => {
  // Get initial paths info
  msfsPaths.value = connector.getMSFSPaths()

  // Subscribe to paths updates
  unsubscribePaths = connector.onMSFSPaths((paths) => {
    msfsPaths.value = paths
  })
})

onUnmounted(() => {
  if (unsubscribePaths) {
    unsubscribePaths()
  }
})

function toggleDeveloperMode(event: Event) {
  const target = event.target as HTMLInputElement
  settingsStore.setDeveloperMode(target.checked)
}

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

.setting-value.path-value {
  font-family: monospace;
  font-size: 12px;
  word-break: break-all;
}

.setting-value.not-found {
  color: var(--text-muted);
  font-style: italic;
}

.status-indicator {
  font-size: 12px;
  font-weight: 600;
  padding: 4px 10px;
  border-radius: 12px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  flex-shrink: 0;
}

.status-indicator.found {
  background: rgba(34, 197, 94, 0.15);
  color: #22c55e;
}

.status-indicator.not-found {
  background: rgba(239, 68, 68, 0.15);
  color: #ef4444;
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

.toggle-switch {
  position: relative;
  display: inline-block;
  width: 44px;
  height: 24px;
}

.toggle-switch input {
  opacity: 0;
  width: 0;
  height: 0;
}

.toggle-slider {
  position: absolute;
  cursor: pointer;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: var(--bg-elevated);
  border: 1px solid var(--border-subtle);
  transition: 0.3s;
  border-radius: 24px;
}

.toggle-slider:before {
  position: absolute;
  content: "";
  height: 18px;
  width: 18px;
  left: 2px;
  bottom: 2px;
  background-color: var(--text-secondary);
  transition: 0.3s;
  border-radius: 50%;
}

.toggle-switch input:checked + .toggle-slider {
  background: linear-gradient(135deg, var(--accent-primary), var(--accent-secondary));
  border-color: transparent;
}

.toggle-switch input:checked + .toggle-slider:before {
  transform: translateX(20px);
  background-color: white;
}
</style>
