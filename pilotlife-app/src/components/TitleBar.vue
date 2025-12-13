<template>
  <div class="titlebar" data-tauri-drag-region>
    <div class="titlebar-left" data-tauri-drag-region></div>

    <div class="sim-status" @mouseenter="showStatusTooltip = true" @mouseleave="showStatusTooltip = false">
      <span class="sim-status-label">Sim Connected:</span>
      <span class="sim-status-indicator" :class="{ connected: simConnected }"></span>

      <div v-if="showStatusTooltip" class="status-tooltip">
        <div class="status-row">
          <span class="status-label">Connector Live:</span>
          <span class="status-indicator" :class="{ connected: connectorLive }"></span>
        </div>
        <div class="status-row">
          <span class="status-label">Sim Connected:</span>
          <span class="status-indicator" :class="{ connected: simConnected }"></span>
        </div>
      </div>
    </div>

    <div class="titlebar-controls">
      <div class="settings-wrapper">
        <button
          class="titlebar-btn settings"
          @click="toggleSettings"
          title="Settings"
          :class="{ active: showSettings }"
        >
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="12" cy="12" r="3" />
            <path d="M12 1v2M12 21v2M4.22 4.22l1.42 1.42M18.36 18.36l1.42 1.42M1 12h2M21 12h2M4.22 19.78l1.42-1.42M18.36 5.64l1.42-1.42" />
          </svg>
        </button>

        <div v-if="showSettings" class="settings-dropdown">
          <div class="settings-header">Resolution</div>
          <button
            v-for="res in resolutions"
            :key="res.label"
            class="resolution-btn"
            :class="{ active: currentResolution === res.label }"
            @click="setResolution(res.width, res.height, res.label)"
          >
            <span class="res-label">{{ res.label }}</span>
            <span class="res-size">{{ res.width }} x {{ res.height }}</span>
          </button>
        </div>
      </div>

      <button class="titlebar-btn minimize" @click="minimize" title="Minimize">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M5 12h14" />
        </svg>
      </button>

      <button class="titlebar-btn close" @click="close" title="Close">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M6 6l12 12M6 18L18 6" />
        </svg>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { getCurrentWindow, LogicalSize } from '@tauri-apps/api/window'
import { readTextFile, writeTextFile, exists, mkdir, BaseDirectory } from '@tauri-apps/plugin-fs'
import { connector, type SimulatorStatus } from '@/services/connector'

interface AppSettings {
  window: {
    width: number
    height: number
    resolutionLabel: string
  }
}

const defaultSettings: AppSettings = {
  window: {
    width: 800,
    height: 600,
    resolutionLabel: 'Medium'
  }
}

const appWindow = getCurrentWindow()
const showSettings = ref(false)
const currentResolution = ref('800 x 600')
const simConnected = ref(false)
const connectorLive = ref(false)
const showStatusTooltip = ref(false)

// Connector status unsubscribe function
let unsubscribeStatus: (() => void) | null = null

const resolutions = [
  { label: 'Small', width: 600, height: 400 },
  { label: 'Medium', width: 800, height: 600 },
  { label: 'Large', width: 1024, height: 768 },
  { label: 'HD', width: 1280, height: 720 },
  { label: 'Full HD', width: 1920, height: 1080 }
]

const SETTINGS_DIR = 'settings'
const SETTINGS_FILE = 'settings/settings.json'
const fsOptions = { baseDir: BaseDirectory.Resource }

async function loadSettings(): Promise<AppSettings> {
  try {
    const fileExists = await exists(SETTINGS_FILE, fsOptions)
    console.log('Settings file exists:', fileExists)
    if (!fileExists) {
      console.log('Using default settings')
      return defaultSettings
    }
    const content = await readTextFile(SETTINGS_FILE, fsOptions)
    console.log('Loaded settings:', content)
    return JSON.parse(content) as AppSettings
  } catch (error) {
    console.error('Failed to load settings:', error)
    return defaultSettings
  }
}

async function saveSettings(settings: AppSettings): Promise<void> {
  try {
    // Ensure settings directory exists
    const dirExists = await exists(SETTINGS_DIR, fsOptions)
    if (!dirExists) {
      await mkdir(SETTINGS_DIR, { ...fsOptions, recursive: true })
    }
    await writeTextFile(SETTINGS_FILE, JSON.stringify(settings, null, 2), fsOptions)
    console.log('Settings saved successfully:', settings)
  } catch (error) {
    console.error('Failed to save settings:', error)
  }
}

function toggleSettings() {
  showSettings.value = !showSettings.value
}

async function setResolution(width: number, height: number, label: string) {
  await appWindow.setSize(new LogicalSize(width, height))
  currentResolution.value = label
  showSettings.value = false

  // Save to settings file
  const settings = await loadSettings()
  settings.window = { width, height, resolutionLabel: label }
  await saveSettings(settings)
}

async function minimize() {
  await appWindow.minimize()
}

async function close() {
  await appWindow.close()
}

// Close dropdown when clicking outside
function handleClickOutside(event: MouseEvent) {
  const target = event.target as HTMLElement
  if (!target.closest('.settings-wrapper')) {
    showSettings.value = false
  }
}

onMounted(async () => {
  document.addEventListener('click', handleClickOutside)

  // Load settings and apply saved window size
  const settings = await loadSettings()
  await appWindow.setSize(new LogicalSize(settings.window.width, settings.window.height))
  currentResolution.value = settings.window.resolutionLabel

  // Subscribe to connector status updates
  unsubscribeStatus = connector.onStatus((status: SimulatorStatus) => {
    simConnected.value = status.isConnected && status.isSimRunning
    console.log('Sim status:', status.isConnected ? 'Connected' : 'Disconnected', status.simulatorVersion)
  })

  // Start the connector
  try {
    await connector.start()
    console.log('Connector started on port:', connector.getPort())
  } catch (error) {
    console.error('Failed to start connector:', error)
  }
})

onUnmounted(async () => {
  document.removeEventListener('click', handleClickOutside)

  // Unsubscribe from status updates
  if (unsubscribeStatus) {
    unsubscribeStatus()
    unsubscribeStatus = null
  }

  // Stop the connector
  try {
    await connector.stop()
    console.log('Connector stopped')
  } catch (error) {
    console.error('Failed to stop connector:', error)
  }
})
</script>

<style scoped>
.titlebar {
  height: 36px;
  background: var(--bg-primary);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 8px;
  user-select: none;
  -webkit-user-select: none;
  border-bottom: 1px solid var(--border-subtle);
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 9999;
}

.titlebar-left {
  flex: 1;
}

.sim-status {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-right: 16px;
}

.sim-status-label {
  font-size: 12px;
  color: var(--text-secondary);
  font-weight: 500;
}

.sim-status-indicator {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: #ef4444;
  box-shadow: 0 0 6px rgba(239, 68, 68, 0.6);
  transition: all 0.3s ease;
}

.sim-status-indicator.connected {
  background: #22c55e;
  box-shadow: 0 0 6px rgba(34, 197, 94, 0.6);
}

.titlebar-controls {
  display: flex;
  align-items: center;
  gap: 4px;
  width: 120px;
  justify-content: flex-end;
}

.settings-wrapper {
  position: relative;
}

.titlebar-btn {
  width: 32px;
  height: 28px;
  border: none;
  background: transparent;
  color: var(--text-secondary);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 6px;
  transition: all 0.15s ease;
}

.titlebar-btn svg {
  width: 14px;
  height: 14px;
}

.titlebar-btn:hover {
  background: var(--bg-elevated);
  color: var(--text-primary);
}

.titlebar-btn.settings.active {
  background: var(--bg-elevated);
  color: var(--accent-primary);
}

.titlebar-btn.close:hover {
  background: #e81123;
  color: white;
}

.settings-dropdown {
  position: absolute;
  top: 100%;
  right: 0;
  margin-top: 8px;
  background: var(--bg-card);
  border: 1px solid var(--border-subtle);
  border-radius: 12px;
  padding: 8px;
  min-width: 180px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
  z-index: 10000;
}

.settings-header {
  font-size: 11px;
  font-weight: 600;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 1px;
  padding: 4px 8px 8px;
}

.resolution-btn {
  width: 100%;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 12px;
  border: none;
  background: transparent;
  color: var(--text-primary);
  cursor: pointer;
  border-radius: 8px;
  transition: all 0.15s ease;
  font-size: 13px;
}

.resolution-btn:hover {
  background: var(--bg-elevated);
}

.resolution-btn.active {
  background: linear-gradient(135deg, rgba(14, 165, 233, 0.15), rgba(139, 92, 246, 0.15));
  color: var(--accent-primary);
}

.res-label {
  font-weight: 500;
}

.res-size {
  font-size: 11px;
  color: var(--text-muted);
  font-family: 'Space Mono', monospace;
}

.resolution-btn.active .res-size {
  color: var(--accent-secondary);
}
</style>
