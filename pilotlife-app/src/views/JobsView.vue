<template>
  <div class="jobs-view">
    <!-- Map Section (Compact) -->
    <div class="map-section" :style="{ height: `${jobsConfig.config.value.mapHeight}vh` }">
      <l-map
        ref="mapRef"
        :zoom="zoom"
        :center="mapCenter"
        :use-global-leaflet="false"
        class="leaflet-map"
        @ready="onMapReady"
        @moveend="onMapMoveEnd"
        @zoomend="onZoomEnd"
      >
        <l-tile-layer
          url="https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png"
          attribution="&copy; OpenStreetMap contributors &copy; CARTO"
        />

        <!-- Vicinity circle -->
        <l-circle
          v-if="mapReady && !isUnmounting && selectedAirport && jobsConfig.config.value.showVicinityCircle"
          :lat-lng="[selectedAirport.latitude, selectedAirport.longitude]"
          :radius="jobsConfig.config.value.vicinityRadiusNm * 1852"
          :color="'#3b82f6'"
          :fill-color="'#3b82f6'"
          :fill-opacity="0.1"
          :weight="1"
          :dash-array="'5, 5'"
        />

        <!-- Airport markers -->
        <template v-if="mapReady && !isUnmounting && visibleAirports.length > 0">
          <l-marker
            v-for="airport in visibleAirports"
            :key="`airport-${airport.id}`"
            :lat-lng="[airport.latitude, airport.longitude]"
            @click="selectAirport(airport)"
          >
            <l-icon
              :icon-url="getAirportIcon(airport)"
              :icon-size="getAirportIconSize(airport)"
              :icon-anchor="getAirportIconAnchor(airport)"
            />
            <l-popup>
              <div class="airport-popup">
                <strong>{{ airport.ident }}</strong>
                <span class="airport-name">{{ airport.name }}</span>
                <span class="airport-type">{{ formatAirportType(airport.type) }}</span>
              </div>
            </l-popup>
          </l-marker>
        </template>

        <!-- Current location marker -->
        <l-marker
          v-if="mapReady && !isUnmounting && currentAirport"
          :key="`current-${currentAirport.id}`"
          :lat-lng="[currentAirport.latitude, currentAirport.longitude]"
        >
          <l-icon :icon-url="currentLocationIcon" :icon-size="[28, 28]" :icon-anchor="[14, 14]" />
          <l-popup>
            <strong>{{ currentAirport.name }}</strong><br>
            <span>Your current location</span>
          </l-popup>
        </l-marker>

        <!-- Selected job route -->
        <template v-if="mapReady && !isUnmounting && selectedJob">
          <l-polyline
            :lat-lngs="routeLine"
            :color="'#22c55e'"
            :weight="3"
            :opacity="0.8"
            :dash-array="'10, 10'"
          />
        </template>
      </l-map>

      <!-- Map Controls Overlay -->
      <div class="map-controls">
        <div class="selected-info" v-if="selectedAirport">
          <span class="airport-code">{{ selectedAirport.ident }}</span>
          <span class="airport-label">{{ selectedAirport.name }}</span>
          <button class="clear-btn" @click="clearSelection">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <line x1="18" y1="6" x2="6" y2="18"/>
              <line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>
        <div class="map-stats">
          <span>{{ visibleAirports.length }} airports</span>
          <span class="divider">|</span>
          <span>{{ searchResult?.totalCount || 0 }} jobs</span>
        </div>
      </div>
    </div>

    <!-- Jobs Table Section -->
    <div class="jobs-section">
      <!-- Filters Bar -->
      <div class="filters-bar">
        <div class="filters-row">
          <div class="filter-group">
            <label>Type</label>
            <select v-model="filters.jobType" @change="loadJobs">
              <option :value="undefined">All</option>
              <option value="Cargo">Cargo</option>
              <option value="Passenger">Passenger</option>
            </select>
          </div>

          <div class="filter-group">
            <label>Urgency</label>
            <select v-model="filters.urgency" @change="loadJobs">
              <option :value="undefined">All</option>
              <option value="Standard">Standard</option>
              <option value="Priority">Priority</option>
              <option value="Express">Express</option>
              <option value="Urgent">Urgent</option>
              <option value="Critical">Critical</option>
            </select>
          </div>

          <div class="filter-group">
            <label>Min Payout</label>
            <input
              type="number"
              v-model.number="filters.minPayout"
              placeholder="$0"
              @change="loadJobs"
            />
          </div>

          <div class="filter-group">
            <label>Max Distance</label>
            <input
              type="number"
              v-model.number="filters.maxDistanceNm"
              placeholder="nm"
              @change="loadJobs"
            />
          </div>

          <div class="filter-group">
            <label>Departure</label>
            <input
              type="text"
              v-model="filters.departureIcao"
              placeholder="ICAO"
              maxlength="4"
              @change="loadJobs"
            />
          </div>

          <div class="filter-group">
            <label>Arrival</label>
            <input
              type="text"
              v-model="filters.arrivalIcao"
              placeholder="ICAO"
              maxlength="4"
              @change="loadJobs"
            />
          </div>

          <div class="filter-group vicinity-group">
            <label>Vicinity (nm)</label>
            <input
              type="number"
              v-model.number="vicinityRadius"
              min="25"
              max="500"
              step="25"
              @change="onVicinityChange"
            />
          </div>

          <button class="reset-btn" @click="resetFilters">Reset</button>
        </div>
      </div>

      <!-- Jobs Table -->
      <div class="jobs-table-container">
        <table class="jobs-table">
          <thead>
            <tr>
              <th class="col-route">Route</th>
              <th class="col-type sortable" @click="sortBy('type')">
                Type
                <span v-if="currentSort === 'type'" class="sort-icon">{{ sortDesc ? '↓' : '↑' }}</span>
              </th>
              <th class="col-urgency sortable" @click="sortBy('urgency')">
                Urgency
                <span v-if="currentSort === 'urgency'" class="sort-icon">{{ sortDesc ? '↓' : '↑' }}</span>
              </th>
              <th class="col-cargo">Cargo/Pax</th>
              <th class="col-weight sortable" @click="sortBy('weight')">
                Weight
                <span v-if="currentSort === 'weight'" class="sort-icon">{{ sortDesc ? '↓' : '↑' }}</span>
              </th>
              <th class="col-distance sortable" @click="sortBy('distance')">
                Distance
                <span v-if="currentSort === 'distance'" class="sort-icon">{{ sortDesc ? '↓' : '↑' }}</span>
              </th>
              <th class="col-payout sortable" @click="sortBy('payout')">
                Payout
                <span v-if="currentSort === 'payout'" class="sort-icon">{{ sortDesc ? '↓' : '↑' }}</span>
              </th>
              <th class="col-expiry sortable" @click="sortBy('expiry')">
                Expires
                <span v-if="currentSort === 'expiry'" class="sort-icon">{{ sortDesc ? '↓' : '↑' }}</span>
              </th>
              <th class="col-actions"></th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="9" class="loading-cell">
                <v-progress-circular indeterminate color="primary" size="24" />
                <span>Loading jobs...</span>
              </td>
            </tr>
            <tr v-else-if="jobs.length === 0">
              <td colspan="9" class="empty-cell">
                <span>No jobs found. Try adjusting your filters or selecting an airport.</span>
              </td>
            </tr>
            <tr
              v-else
              v-for="job in jobs"
              :key="job.id"
              :class="{ selected: selectedJob?.id === job.id, [getUrgencyRowClass(job.urgency)]: true }"
              @click="selectJob(job)"
            >
              <td class="col-route">
                <div class="route-cell">
                  <span class="icao from">{{ job.departureAirport.ident }}</span>
                  <span class="arrow">→</span>
                  <span class="icao to">{{ job.arrivalAirport.ident }}</span>
                </div>
              </td>
              <td class="col-type">
                <span class="type-badge" :class="job.type ? String(job.type).toLowerCase() : ''">{{ job.type || '-' }}</span>
              </td>
              <td class="col-urgency">
                <span class="urgency-badge" :class="job.urgency ? String(job.urgency).toLowerCase() : 'standard'">{{ job.urgency || 'Standard' }}</span>
              </td>
              <td class="col-cargo">
                <span v-if="job.type === 'Passenger'">{{ job.passengerCount }} pax</span>
                <span v-else class="cargo-name">{{ job.cargoType }}</span>
              </td>
              <td class="col-weight">{{ formatWeight(job.weightLbs || job.weight) }}</td>
              <td class="col-distance">{{ Math.round(job.distanceNm) }} nm</td>
              <td class="col-payout">
                <span class="payout" :class="{ high: isHighPayout(job) }">
                  ${{ job.payout.toLocaleString() }}
                </span>
              </td>
              <td class="col-expiry">{{ formatExpiry(job.expiresAt) }}</td>
              <td class="col-actions">
                <button
                  class="accept-btn"
                  @click.stop="acceptJob(job)"
                  :disabled="acceptingJobId === job.id"
                >
                  {{ acceptingJobId === job.id ? '...' : 'Accept' }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div class="pagination-bar" v-if="searchResult && searchResult.totalPages > 1">
        <div class="pagination-info">
          Showing {{ (currentPage - 1) * pageSize + 1 }}-{{ Math.min(currentPage * pageSize, searchResult.totalCount) }}
          of {{ searchResult.totalCount }} jobs
        </div>
        <div class="pagination-controls">
          <button :disabled="currentPage <= 1" @click="goToPage(1)">First</button>
          <button :disabled="currentPage <= 1" @click="goToPage(currentPage - 1)">Prev</button>
          <span class="page-info">Page {{ currentPage }} of {{ searchResult.totalPages }}</span>
          <button :disabled="currentPage >= searchResult.totalPages" @click="goToPage(currentPage + 1)">Next</button>
          <button :disabled="currentPage >= searchResult.totalPages" @click="goToPage(searchResult.totalPages)">Last</button>
          <select v-model="pageSize" @change="onPageSizeChange" class="page-size-select">
            <option :value="20">20</option>
            <option :value="25">25</option>
            <option :value="50">50</option>
            <option :value="100">100</option>
          </select>
        </div>
      </div>
    </div>

    <!-- Settings Panel (collapsible) -->
    <div class="settings-panel" :class="{ expanded: showSettings }">
      <button class="settings-toggle" @click="showSettings = !showSettings">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <circle cx="12" cy="12" r="3"/>
          <path d="M12 1v6m0 6v6M4.22 4.22l4.24 4.24m7.08 7.08l4.24 4.24M1 12h6m6 0h6M4.22 19.78l4.24-4.24m7.08-7.08l4.24-4.24"/>
        </svg>
        Settings
      </button>
      <div class="settings-content" v-if="showSettings">
        <div class="setting-row">
          <label>Map Height (%)</label>
          <input
            type="range"
            min="20"
            max="60"
            v-model.number="jobsConfig.config.value.mapHeight"
            @change="saveConfig"
          />
          <span>{{ jobsConfig.config.value.mapHeight }}%</span>
        </div>
        <div class="setting-row">
          <label>Default Vicinity (nm)</label>
          <input
            type="number"
            min="25"
            max="500"
            step="25"
            v-model.number="jobsConfig.config.value.vicinityRadiusNm"
            @change="saveConfig"
          />
        </div>
        <div class="setting-row">
          <label>Max Airports on Map</label>
          <input
            type="number"
            min="100"
            max="1000"
            step="100"
            v-model.number="jobsConfig.config.value.maxAirportsOnMap"
            @change="saveConfig"
          />
        </div>
        <div class="setting-row">
          <label>Show Vicinity Circle</label>
          <input
            type="checkbox"
            v-model="jobsConfig.config.value.showVicinityCircle"
            @change="saveConfig"
          />
        </div>
        <button class="reset-settings-btn" @click="resetSettings">Reset to Defaults</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount, watch } from 'vue'
import { LMap, LTileLayer, LMarker, LIcon, LPopup, LPolyline, LCircle } from '@vue-leaflet/vue-leaflet'
import 'leaflet/dist/leaflet.css'
import { api, type Job, type Airport, type JobSearchResult } from '../services/api'
import { useUserStore } from '../stores/user'
import { useWorldStore } from '../stores/world'
import { useJobsConfigStore } from '../stores/jobsConfig'

const userStore = useUserStore()
const worldStore = useWorldStore()
const jobsConfig = useJobsConfigStore()

// Map state
const mapRef = ref()
const mapReady = ref(false)
const isUnmounting = ref(false)
const zoom = ref(jobsConfig.config.value.defaultZoom)
const mapCenter = ref<[number, number]>([40, -95])
const visibleAirports = ref<Airport[]>([])

// Selection state
const selectedAirport = ref<Airport | null>(null)
const selectedJob = ref<Job | null>(null)

// Jobs state
const jobs = ref<Job[]>([])
const searchResult = ref<JobSearchResult | null>(null)
const loading = ref(false)
const acceptingJobId = ref<string | null>(null)

// Pagination
const currentPage = ref(1)
const pageSize = ref(jobsConfig.config.value.pageSize)

// Sorting
const currentSort = ref<string>(jobsConfig.config.value.defaultSortBy)
const sortDesc = ref(jobsConfig.config.value.defaultSortDescending)

// Filters
const filters = ref({
  jobType: undefined as 'Cargo' | 'Passenger' | undefined,
  urgency: undefined as string | undefined,
  minPayout: undefined as number | undefined,
  maxDistanceNm: undefined as number | undefined,
  departureIcao: '',
  arrivalIcao: '',
})

const vicinityRadius = ref(jobsConfig.config.value.vicinityRadiusNm)
const showSettings = ref(false)

// Computed
const currentAirport = computed((): Airport | null => {
  return worldStore.currentPlayerWorld.value?.currentAirportId
    ? visibleAirports.value.find(a => a.id === worldStore.currentPlayerWorld.value?.currentAirportId) || null
    : null
})

const routeLine = computed((): [number, number][] => {
  if (!selectedJob.value) return []
  return [
    [selectedJob.value.departureAirport.latitude, selectedJob.value.departureAirport.longitude],
    [selectedJob.value.arrivalAirport.latitude, selectedJob.value.arrivalAirport.longitude]
  ]
})

const currentLocationIcon = computed(() => {
  const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="#22c55e"><circle cx="12" cy="12" r="10" fill="#22c55e"/><circle cx="12" cy="12" r="4" fill="white"/></svg>`
  return `data:image/svg+xml;base64,${btoa(svg)}`
})

// Airport icon helpers
function getAirportIcon(airport: Airport): string {
  const isSelected = selectedAirport.value?.id === airport.id
  const color = isSelected ? '#f59e0b' : airport.type === 'large_airport' ? '#3b82f6' : airport.type === 'medium_airport' ? '#8b5cf6' : '#6b7280'
  const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="${color}"><circle cx="12" cy="12" r="8" fill="${color}"/><circle cx="12" cy="12" r="3" fill="white"/></svg>`
  return `data:image/svg+xml;base64,${btoa(svg)}`
}

function getAirportIconSize(airport: Airport): [number, number] {
  if (selectedAirport.value?.id === airport.id) return [24, 24]
  if (airport.type === 'large_airport') return [20, 20]
  if (airport.type === 'medium_airport') return [16, 16]
  return [12, 12]
}

function getAirportIconAnchor(airport: Airport): [number, number] {
  const size = getAirportIconSize(airport)
  return [size[0] / 2, size[1] / 2]
}

function formatAirportType(type: string): string {
  return type.replace('_airport', '').replace('_', ' ')
}

// Map event handlers
function onMapReady() {
  mapReady.value = true
  loadAirportsInView()
}

async function onMapMoveEnd() {
  if (!mapReady.value) return
  await loadAirportsInView()
}

async function onZoomEnd() {
  if (!mapReady.value) return
  zoom.value = mapRef.value?.leafletObject?.getZoom() || zoom.value
  await loadAirportsInView()
}

async function loadAirportsInView() {
  if (!mapReady.value || !mapRef.value?.leafletObject) return

  const bounds = mapRef.value.leafletObject.getBounds()
  const response = await api.airports.getInBounds({
    north: bounds.getNorth(),
    south: bounds.getSouth(),
    east: bounds.getEast(),
    west: bounds.getWest(),
    zoomLevel: zoom.value,
    limit: jobsConfig.config.value.maxAirportsOnMap,
  })

  if (response.data) {
    visibleAirports.value = response.data
  }
}

// Selection handlers
function selectAirport(airport: Airport) {
  selectedAirport.value = airport
  selectedJob.value = null
  currentPage.value = 1
  loadJobs()
}

function clearSelection() {
  selectedAirport.value = null
  selectedJob.value = null
  loadJobs()
}

function selectJob(job: Job) {
  selectedJob.value = job

  // Fit map to show both airports
  if (mapRef.value?.leafletObject) {
    const bounds = [
      [job.departureAirport.latitude, job.departureAirport.longitude],
      [job.arrivalAirport.latitude, job.arrivalAirport.longitude]
    ]
    mapRef.value.leafletObject.fitBounds(bounds, { padding: [30, 30], maxZoom: 8 })
  }
}

// Job loading
async function loadJobs() {
  if (!worldStore.currentPlayerWorld.value?.worldId) return

  loading.value = true

  const params: any = {
    worldId: worldStore.currentPlayerWorld.value.worldId,
    sortBy: currentSort.value,
    sortDescending: sortDesc.value,
    page: currentPage.value,
    pageSize: pageSize.value,
  }

  // Apply filters
  if (filters.value.jobType) params.jobType = filters.value.jobType
  if (filters.value.urgency) params.urgency = filters.value.urgency
  if (filters.value.minPayout) params.minPayout = filters.value.minPayout
  if (filters.value.maxDistanceNm) params.maxDistanceNm = filters.value.maxDistanceNm
  if (filters.value.departureIcao) params.departureIcao = filters.value.departureIcao.toUpperCase()
  if (filters.value.arrivalIcao) params.arrivalIcao = filters.value.arrivalIcao.toUpperCase()

  // Apply vicinity search if airport selected
  if (selectedAirport.value) {
    params.centerLatitude = selectedAirport.value.latitude
    params.centerLongitude = selectedAirport.value.longitude
    params.vicinityRadiusNm = vicinityRadius.value
  }

  const response = await api.jobs.search(params)

  if (response.data) {
    jobs.value = response.data.jobs
    searchResult.value = response.data
  }

  loading.value = false
}

// Sorting
function sortBy(column: string) {
  if (currentSort.value === column) {
    sortDesc.value = !sortDesc.value
  } else {
    currentSort.value = column
    sortDesc.value = true
  }
  loadJobs()
}

// Pagination
function goToPage(page: number) {
  currentPage.value = page
  loadJobs()
}

function onPageSizeChange() {
  currentPage.value = 1
  jobsConfig.setPageSize(pageSize.value)
  loadJobs()
}

function onVicinityChange() {
  jobsConfig.setVicinityRadius(vicinityRadius.value)
  loadJobs()
}

// Filters
function resetFilters() {
  filters.value = {
    jobType: undefined,
    urgency: undefined,
    minPayout: undefined,
    maxDistanceNm: undefined,
    departureIcao: '',
    arrivalIcao: '',
  }
  currentPage.value = 1
  loadJobs()
}

// Accept job
async function acceptJob(job: Job) {
  if (!userStore.user.value?.id) return

  acceptingJobId.value = job.id
  const response = await api.jobs.accept(job.id, userStore.user.value.id)

  if (response.data) {
    jobs.value = jobs.value.filter(j => j.id !== job.id)
    if (selectedJob.value?.id === job.id) {
      selectedJob.value = null
    }
    if (searchResult.value) {
      searchResult.value.totalCount--
    }
  }
  acceptingJobId.value = null
}

// Formatting helpers
function formatWeight(weight: number): string {
  if (weight >= 1000) {
    return `${(weight / 1000).toFixed(1)}k lbs`
  }
  return `${weight} lbs`
}

function formatExpiry(expiresAt: string): string {
  const now = new Date()
  const expiry = new Date(expiresAt)
  const diff = expiry.getTime() - now.getTime()

  if (diff <= 0) return 'Expired'

  const hours = Math.floor(diff / (1000 * 60 * 60))
  const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60))

  if (hours > 24) {
    const days = Math.floor(hours / 24)
    return `${days}d ${hours % 24}h`
  }
  if (hours > 0) {
    return `${hours}h ${minutes}m`
  }
  return `${minutes}m`
}

function isHighPayout(job: Job): boolean {
  return job.payout >= 10000 || (job.urgency !== undefined && job.urgency !== 'Standard')
}

function getUrgencyRowClass(urgency?: string): string {
  if (!urgency || urgency === 'Standard') return 'urgency-row-standard'
  if (typeof urgency !== 'string') return 'urgency-row-standard'
  return `urgency-row-${urgency.toLowerCase()}`
}

// Settings
function saveConfig() {
  jobsConfig.updateConfig(jobsConfig.config.value)
}

function resetSettings() {
  jobsConfig.resetConfig()
  vicinityRadius.value = jobsConfig.config.value.vicinityRadiusNm
  pageSize.value = jobsConfig.config.value.pageSize
}

// Init
onMounted(async () => {
  // Center on current airport if available
  if (worldStore.currentPlayerWorld.value?.currentAirportId) {
    const airport = await api.airports.get(worldStore.currentPlayerWorld.value.currentAirportId)
    if (airport.data) {
      mapCenter.value = [airport.data.latitude, airport.data.longitude]
      zoom.value = 7
    }
  }

  // Load jobs (airports will be loaded when map is ready via @ready event)
  loadJobs()
})

// Watch for world changes
watch(() => worldStore.currentPlayerWorld.value?.worldId, () => {
  loadJobs()
})

// Cleanup before unmount to prevent Leaflet errors
onBeforeUnmount(() => {
  isUnmounting.value = true
  mapReady.value = false
  visibleAirports.value = []
  selectedAirport.value = null
  selectedJob.value = null
})
</script>

<style scoped>
.jobs-view {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: var(--bg-primary);
  position: relative;
}

/* Map Section */
.map-section {
  position: relative;
  min-height: 200px;
  border-bottom: 1px solid var(--border-subtle);
}

.leaflet-map {
  height: 100%;
  width: 100%;
}

.map-controls {
  position: absolute;
  top: 12px;
  left: 12px;
  right: 12px;
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  z-index: 1000;
  pointer-events: none;
}

.map-controls > * {
  pointer-events: auto;
}

.selected-info {
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 8px;
  padding: 8px 12px;
  display: flex;
  align-items: center;
  gap: 8px;
}

.selected-info .airport-code {
  font-family: var(--font-mono);
  font-weight: 700;
  color: var(--accent-primary);
}

.selected-info .airport-label {
  color: var(--text-secondary);
  font-size: 13px;
  max-width: 200px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.clear-btn {
  background: none;
  border: none;
  padding: 4px;
  cursor: pointer;
  color: var(--text-muted);
  display: flex;
}

.clear-btn:hover {
  color: var(--text-primary);
}

.clear-btn svg {
  width: 16px;
  height: 16px;
}

.map-stats {
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 8px;
  padding: 6px 12px;
  font-size: 12px;
  color: var(--text-secondary);
}

.map-stats .divider {
  margin: 0 8px;
  color: var(--border-subtle);
}

.airport-popup {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.airport-popup .airport-name {
  font-size: 12px;
  color: var(--text-secondary);
}

.airport-popup .airport-type {
  font-size: 11px;
  color: var(--text-muted);
  text-transform: capitalize;
}

/* Filters Bar */
.filters-bar {
  background: var(--bg-secondary);
  border-bottom: 1px solid var(--border-subtle);
  padding: 12px 16px;
}

.filters-row {
  display: flex;
  gap: 12px;
  align-items: flex-end;
  flex-wrap: wrap;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.filter-group label {
  font-size: 11px;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.filter-group select,
.filter-group input {
  background: var(--bg-primary);
  border: 1px solid var(--border-subtle);
  border-radius: 4px;
  padding: 6px 10px;
  font-size: 13px;
  color: var(--text-primary);
  min-width: 100px;
}

.filter-group input[type="number"] {
  width: 80px;
}

.filter-group input[type="text"] {
  width: 70px;
  text-transform: uppercase;
}

.vicinity-group input {
  width: 70px;
}

.reset-btn {
  background: var(--bg-elevated);
  border: 1px solid var(--border-subtle);
  border-radius: 4px;
  padding: 6px 12px;
  font-size: 13px;
  color: var(--text-secondary);
  cursor: pointer;
}

.reset-btn:hover {
  background: var(--bg-primary);
  color: var(--text-primary);
}

/* Jobs Table */
.jobs-section {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.jobs-table-container {
  flex: 1;
  overflow: auto;
}

.jobs-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 13px;
}

.jobs-table th {
  position: sticky;
  top: 0;
  background: var(--bg-secondary);
  border-bottom: 1px solid var(--border-subtle);
  padding: 10px 12px;
  text-align: left;
  font-weight: 600;
  color: var(--text-secondary);
  font-size: 11px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  white-space: nowrap;
  z-index: 10;
}

.jobs-table th.sortable {
  cursor: pointer;
}

.jobs-table th.sortable:hover {
  color: var(--text-primary);
}

.sort-icon {
  margin-left: 4px;
  color: var(--accent-primary);
}

.jobs-table td {
  padding: 10px 12px;
  border-bottom: 1px solid var(--border-subtle);
  color: var(--text-primary);
}

.jobs-table tr {
  cursor: pointer;
  transition: background 0.15s;
}

.jobs-table tbody tr:hover {
  background: var(--bg-elevated);
}

.jobs-table tr.selected {
  background: rgba(59, 130, 246, 0.1);
}

.jobs-table tr.urgency-row-priority {
  border-left: 3px solid #3b82f6;
}

.jobs-table tr.urgency-row-express {
  border-left: 3px solid #eab308;
}

.jobs-table tr.urgency-row-urgent {
  border-left: 3px solid #f97316;
}

.jobs-table tr.urgency-row-critical {
  border-left: 3px solid #ef4444;
  background: rgba(239, 68, 68, 0.05);
}

.loading-cell,
.empty-cell {
  text-align: center;
  padding: 40px !important;
  color: var(--text-secondary);
}

.loading-cell {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
}

/* Route cell */
.route-cell {
  display: flex;
  align-items: center;
  gap: 6px;
  font-family: var(--font-mono);
}

.route-cell .icao {
  font-weight: 600;
}

.route-cell .arrow {
  color: var(--text-muted);
}

/* Badges */
.type-badge {
  padding: 2px 6px;
  border-radius: 4px;
  font-size: 11px;
  font-weight: 600;
}

.type-badge.cargo {
  background: rgba(59, 130, 246, 0.2);
  color: #3b82f6;
}

.type-badge.passenger {
  background: rgba(168, 85, 247, 0.2);
  color: #a855f7;
}

.urgency-badge {
  padding: 2px 6px;
  border-radius: 4px;
  font-size: 11px;
  font-weight: 600;
}

.urgency-badge.standard {
  background: rgba(107, 114, 128, 0.2);
  color: #6b7280;
}

.urgency-badge.priority {
  background: rgba(59, 130, 246, 0.2);
  color: #3b82f6;
}

.urgency-badge.express {
  background: rgba(234, 179, 8, 0.2);
  color: #eab308;
}

.urgency-badge.urgent {
  background: rgba(249, 115, 22, 0.2);
  color: #f97316;
}

.urgency-badge.critical {
  background: rgba(239, 68, 68, 0.2);
  color: #ef4444;
}

.cargo-name {
  font-size: 12px;
  max-width: 120px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.payout {
  font-weight: 600;
  color: #22c55e;
}

.payout.high {
  color: #f59e0b;
}

.accept-btn {
  background: var(--accent-primary);
  border: none;
  border-radius: 4px;
  padding: 4px 12px;
  font-size: 12px;
  font-weight: 600;
  color: white;
  cursor: pointer;
  transition: opacity 0.15s;
}

.accept-btn:hover {
  opacity: 0.9;
}

.accept-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* Pagination */
.pagination-bar {
  background: var(--bg-secondary);
  border-top: 1px solid var(--border-subtle);
  padding: 10px 16px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.pagination-info {
  font-size: 13px;
  color: var(--text-secondary);
}

.pagination-controls {
  display: flex;
  align-items: center;
  gap: 8px;
}

.pagination-controls button {
  background: var(--bg-primary);
  border: 1px solid var(--border-subtle);
  border-radius: 4px;
  padding: 4px 10px;
  font-size: 12px;
  color: var(--text-primary);
  cursor: pointer;
}

.pagination-controls button:hover:not(:disabled) {
  background: var(--bg-elevated);
}

.pagination-controls button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.page-info {
  font-size: 13px;
  color: var(--text-secondary);
  padding: 0 8px;
}

.page-size-select {
  background: var(--bg-primary);
  border: 1px solid var(--border-subtle);
  border-radius: 4px;
  padding: 4px 8px;
  font-size: 12px;
  color: var(--text-primary);
  margin-left: 8px;
}

/* Settings Panel */
.settings-panel {
  position: absolute;
  bottom: 60px;
  right: 16px;
  z-index: 100;
}

.settings-toggle {
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 8px;
  padding: 8px 12px;
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  color: var(--text-secondary);
  cursor: pointer;
}

.settings-toggle:hover {
  background: var(--bg-elevated);
}

.settings-toggle svg {
  width: 16px;
  height: 16px;
}

.settings-content {
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 8px;
  padding: 16px;
  margin-top: 8px;
  min-width: 250px;
}

.setting-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 12px;
}

.setting-row:last-child {
  margin-bottom: 0;
}

.setting-row label {
  font-size: 13px;
  color: var(--text-secondary);
}

.setting-row input[type="range"] {
  flex: 1;
  max-width: 100px;
}

.setting-row input[type="number"] {
  width: 70px;
  background: var(--bg-primary);
  border: 1px solid var(--border-subtle);
  border-radius: 4px;
  padding: 4px 8px;
  font-size: 13px;
  color: var(--text-primary);
}

.setting-row input[type="checkbox"] {
  width: 18px;
  height: 18px;
}

.reset-settings-btn {
  width: 100%;
  margin-top: 12px;
  padding: 8px;
  background: var(--bg-primary);
  border: 1px solid var(--border-subtle);
  border-radius: 4px;
  font-size: 12px;
  color: var(--text-secondary);
  cursor: pointer;
}

.reset-settings-btn:hover {
  background: var(--bg-elevated);
}

/* Column widths */
.col-route { width: 120px; }
.col-type { width: 80px; }
.col-urgency { width: 90px; }
.col-cargo { width: 130px; }
.col-weight { width: 80px; }
.col-distance { width: 80px; }
.col-payout { width: 100px; }
.col-expiry { width: 80px; }
.col-actions { width: 70px; }
</style>
