import { ref, computed } from 'vue'

export interface JobsDisplayConfig {
  // Map settings
  mapHeight: number // percentage of screen height
  defaultZoom: number
  minZoomForSmallAirports: number
  minZoomForMediumAirports: number
  maxAirportsOnMap: number

  // Vicinity settings
  vicinityRadiusNm: number
  showVicinityCircle: boolean

  // Job list settings
  pageSize: number
  defaultSortBy: 'payout' | 'distance' | 'weight' | 'expiry' | 'urgency'
  defaultSortDescending: boolean

  // Column visibility
  visibleColumns: string[]

  // Filter defaults
  defaultJobType: 'Cargo' | 'Passenger' | null
  defaultMaxDistance: number | null
  defaultMinPayout: number | null
}

const defaultConfig: JobsDisplayConfig = {
  // Map settings
  mapHeight: 35,
  defaultZoom: 6,
  minZoomForSmallAirports: 8,
  minZoomForMediumAirports: 5,
  maxAirportsOnMap: 500,

  // Vicinity settings
  vicinityRadiusNm: 150,
  showVicinityCircle: true,

  // Job list settings
  pageSize: 25,
  defaultSortBy: 'payout',
  defaultSortDescending: true,

  // Column visibility
  visibleColumns: [
    'route',
    'type',
    'urgency',
    'cargo',
    'weight',
    'distance',
    'payout',
    'expiry',
    'actions'
  ],

  // Filter defaults
  defaultJobType: null,
  defaultMaxDistance: null,
  defaultMinPayout: null,
}

// Singleton config
const config = ref<JobsDisplayConfig>({ ...defaultConfig })

// Load from localStorage on init
const stored = localStorage.getItem('pilotlife_jobs_config')
if (stored) {
  try {
    const parsed = JSON.parse(stored)
    config.value = { ...defaultConfig, ...parsed }
  } catch (e) {
    console.warn('Failed to parse stored jobs config', e)
  }
}

export function useJobsConfigStore() {
  function updateConfig(updates: Partial<JobsDisplayConfig>) {
    config.value = { ...config.value, ...updates }
    localStorage.setItem('pilotlife_jobs_config', JSON.stringify(config.value))
  }

  function resetConfig() {
    config.value = { ...defaultConfig }
    localStorage.removeItem('pilotlife_jobs_config')
  }

  function setPageSize(size: number) {
    updateConfig({ pageSize: Math.min(Math.max(size, 10), 100) })
  }

  function setVicinityRadius(radius: number) {
    updateConfig({ vicinityRadiusNm: Math.min(Math.max(radius, 25), 500) })
  }

  function toggleColumn(column: string) {
    const columns = [...config.value.visibleColumns]
    const index = columns.indexOf(column)
    if (index >= 0) {
      columns.splice(index, 1)
    } else {
      columns.push(column)
    }
    updateConfig({ visibleColumns: columns })
  }

  function isColumnVisible(column: string): boolean {
    return config.value.visibleColumns.includes(column)
  }

  const allColumns = computed(() => [
    { key: 'route', label: 'Route', sortable: false },
    { key: 'type', label: 'Type', sortable: true },
    { key: 'urgency', label: 'Urgency', sortable: true },
    { key: 'cargo', label: 'Cargo/Pax', sortable: false },
    { key: 'weight', label: 'Weight', sortable: true },
    { key: 'distance', label: 'Distance', sortable: true },
    { key: 'payout', label: 'Payout', sortable: true },
    { key: 'expiry', label: 'Expires', sortable: true },
    { key: 'actions', label: '', sortable: false },
  ])

  return {
    config,
    allColumns,
    updateConfig,
    resetConfig,
    setPageSize,
    setVicinityRadius,
    toggleColumn,
    isColumnVisible,
  }
}
