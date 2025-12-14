import { ref } from 'vue'

const SETTINGS_KEY = 'pilotlife_settings'

interface AppSettings {
  developerMode: boolean
  adminMode: boolean
}

const defaultSettings: AppSettings = {
  developerMode: false,
  adminMode: false
}

const settings = ref<AppSettings>({ ...defaultSettings })

function loadSettings(): void {
  try {
    const stored = localStorage.getItem(SETTINGS_KEY)
    if (stored) {
      const parsed = JSON.parse(stored) as Partial<AppSettings>
      settings.value = { ...defaultSettings, ...parsed }
    }
  } catch (error) {
    console.error('Failed to load settings:', error)
  }
}

function saveSettings(): void {
  try {
    localStorage.setItem(SETTINGS_KEY, JSON.stringify(settings.value))
  } catch (error) {
    console.error('Failed to save settings:', error)
  }
}

export function useSettingsStore() {
  // Load settings on first use
  loadSettings()

  function setDeveloperMode(enabled: boolean): void {
    settings.value.developerMode = enabled
    saveSettings()
  }

  function setAdminMode(enabled: boolean): void {
    settings.value.adminMode = enabled
    saveSettings()
  }

  return {
    settings,
    setDeveloperMode,
    setAdminMode
  }
}
