<template>
  <div class="jobs-layout">
    <div class="jobs-list-panel">
      <div class="panel-header">
        <h2 class="panel-title">Available Jobs</h2>
        <p class="panel-subtitle">Select a job to see the route on the map</p>
      </div>

      <div class="filters">
        <v-select
          v-model="selectedCargoType"
          :items="cargoTypes"
          label="Cargo Type"
          variant="outlined"
          density="compact"
          clearable
          hide-details
          class="filter-select"
        />
        <v-select
          v-model="selectedAircraftType"
          :items="aircraftTypes"
          label="Aircraft Type"
          variant="outlined"
          density="compact"
          clearable
          hide-details
          class="filter-select"
        />
      </div>

      <div v-if="loading" class="loading-state">
        <v-progress-circular indeterminate color="primary" />
        <span>Loading jobs...</span>
      </div>

      <div v-else-if="jobs.length === 0" class="empty-state">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="empty-icon">
          <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>
          <polyline points="14,2 14,8 20,8"/>
          <line x1="12" y1="18" x2="12" y2="12"/>
          <line x1="9" y1="15" x2="15" y2="15"/>
        </svg>
        <p>No jobs available at your location</p>
      </div>

      <div v-else class="jobs-scroll">
        <div
          v-for="job in jobs"
          :key="job.id"
          class="job-card"
          :class="{ selected: selectedJob?.id === job.id }"
          @click="selectJob(job)"
        >
          <div class="job-route">
            <span class="airport-code">{{ job.departureAirport.iataCode || job.departureAirport.ident }}</span>
            <div class="route-line">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M5 12h14M12 5l7 7-7 7"/>
              </svg>
            </div>
            <span class="airport-code">{{ job.arrivalAirport.iataCode || job.arrivalAirport.ident }}</span>
          </div>

          <div class="job-details">
            <div class="detail-row">
              <span class="detail-label">Distance</span>
              <span class="detail-value">{{ Math.round(job.distanceNm) }} nm</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">Flight Time</span>
              <span class="detail-value">{{ formatFlightTime(job.estimatedFlightTimeMinutes) }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">Cargo</span>
              <span class="detail-value cargo-badge">{{ job.cargoType }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">Weight</span>
              <span class="detail-value">{{ job.weight.toLocaleString() }} kg</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">Aircraft</span>
              <span class="detail-value">{{ job.requiredAircraftType }}</span>
            </div>
          </div>

          <div class="job-footer">
            <div class="payout">
              <span class="payout-label">Payout</span>
              <span class="payout-value">${{ job.payout.toLocaleString() }}</span>
            </div>
            <v-btn
              size="small"
              color="primary"
              class="accept-btn"
              @click.stop="acceptJob(job)"
              :loading="acceptingJobId === job.id"
            >
              Accept
            </v-btn>
          </div>
        </div>
      </div>
    </div>

    <div class="map-panel">
      <l-map
        ref="mapRef"
        :zoom="zoom"
        :center="mapCenter"
        :use-global-leaflet="false"
        class="leaflet-map"
      >
        <l-tile-layer
          url="https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png"
          attribution="&copy; OpenStreetMap contributors &copy; CARTO"
        />

        <l-marker
          v-if="currentAirport"
          :lat-lng="[currentAirport.latitude, currentAirport.longitude]"
        >
          <l-icon :icon-url="currentLocationIcon" :icon-size="[32, 32]" :icon-anchor="[16, 16]" />
          <l-popup>
            <strong>{{ currentAirport.name }}</strong><br>
            <span>Your current location</span>
          </l-popup>
        </l-marker>

        <template v-if="selectedJob">
          <l-marker :lat-lng="[selectedJob.departureAirport.latitude, selectedJob.departureAirport.longitude]">
            <l-icon :icon-url="departureIcon" :icon-size="[28, 28]" :icon-anchor="[14, 14]" />
            <l-popup>
              <strong>{{ selectedJob.departureAirport.name }}</strong><br>
              <span>Departure</span>
            </l-popup>
          </l-marker>

          <l-marker :lat-lng="[selectedJob.arrivalAirport.latitude, selectedJob.arrivalAirport.longitude]">
            <l-icon :icon-url="arrivalIcon" :icon-size="[28, 28]" :icon-anchor="[14, 14]" />
            <l-popup>
              <strong>{{ selectedJob.arrivalAirport.name }}</strong><br>
              <span>Arrival</span>
            </l-popup>
          </l-marker>

          <l-polyline
            :lat-lngs="routeLine"
            :color="'#3b82f6'"
            :weight="3"
            :opacity="0.8"
            :dash-array="'10, 10'"
          />
        </template>
      </l-map>

      <div v-if="selectedJob" class="map-info-panel">
        <h3>{{ selectedJob.departureAirport.name }}</h3>
        <div class="route-arrow">to</div>
        <h3>{{ selectedJob.arrivalAirport.name }}</h3>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { LMap, LTileLayer, LMarker, LIcon, LPopup, LPolyline } from '@vue-leaflet/vue-leaflet'
import 'leaflet/dist/leaflet.css'
import { api, type Job, type Airport } from '../services/api'
import { useUserStore } from '../stores/user'

const userStore = useUserStore()

const jobs = ref<Job[]>([])
const selectedJob = ref<Job | null>(null)
const loading = ref(true)
const acceptingJobId = ref<string | null>(null)

const selectedCargoType = ref<string | null>(null)
const selectedAircraftType = ref<string | null>(null)

const cargoTypes = ['Passengers', 'Cargo', 'Mail', 'Medical', 'Hazmat']
const aircraftTypes = ['Light', 'Medium', 'Heavy', 'Turboprop', 'Jet']

const mapRef = ref()
const zoom = ref(4)
const mapCenter = ref<[number, number]>([40, -95])

const currentAirport = computed((): Airport | null => {
  return userStore.user.value?.currentAirport || null
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

const departureIcon = computed(() => {
  const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="#3b82f6"><circle cx="12" cy="12" r="10" fill="#3b82f6"/><path d="M8 12l4-4 4 4M12 16V8" stroke="white" stroke-width="2" fill="none"/></svg>`
  return `data:image/svg+xml;base64,${btoa(svg)}`
})

const arrivalIcon = computed(() => {
  const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="#ef4444"><circle cx="12" cy="12" r="10" fill="#ef4444"/><path d="M8 12l4 4 4-4M12 8v8" stroke="white" stroke-width="2" fill="none"/></svg>`
  return `data:image/svg+xml;base64,${btoa(svg)}`
})

function formatFlightTime(minutes: number): string {
  const hours = Math.floor(minutes / 60)
  const mins = minutes % 60
  if (hours === 0) return `${mins}m`
  return `${hours}h ${mins}m`
}

async function loadJobs() {
  if (!userStore.user.value?.currentAirportId) {
    loading.value = false
    return
  }

  loading.value = true
  const response = await api.jobs.getAvailable({
    airportId: userStore.user.value.currentAirportId,
    cargoType: selectedCargoType.value || undefined,
    aircraftType: selectedAircraftType.value || undefined
  })

  if (response.data) {
    jobs.value = response.data
  }
  loading.value = false
}

function selectJob(job: Job) {
  selectedJob.value = job

  if (mapRef.value?.leafletObject) {
    const bounds = [
      [job.departureAirport.latitude, job.departureAirport.longitude],
      [job.arrivalAirport.latitude, job.arrivalAirport.longitude]
    ]
    mapRef.value.leafletObject.fitBounds(bounds, { padding: [50, 50] })
  }
}

async function acceptJob(job: Job) {
  if (!userStore.user.value?.id) return

  acceptingJobId.value = job.id
  const response = await api.jobs.accept(job.id, userStore.user.value.id)

  if (response.data) {
    jobs.value = jobs.value.filter(j => j.id !== job.id)
    if (selectedJob.value?.id === job.id) {
      selectedJob.value = null
    }
  }
  acceptingJobId.value = null
}

watch([selectedCargoType, selectedAircraftType], () => {
  loadJobs()
})

onMounted(() => {
  if (currentAirport.value) {
    mapCenter.value = [currentAirport.value.latitude, currentAirport.value.longitude]
    zoom.value = 6
  }
  loadJobs()
})
</script>

<style scoped>
.jobs-layout {
  display: flex;
  height: 100%;
  background: var(--bg-primary);
}

.jobs-list-panel {
  width: 420px;
  background: var(--bg-secondary);
  border-right: 1px solid var(--border-subtle);
  display: flex;
  flex-direction: column;
}

.panel-header {
  padding: 24px;
  border-bottom: 1px solid var(--border-subtle);
}

.panel-title {
  font-size: 20px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 4px;
}

.panel-subtitle {
  font-size: 14px;
  color: var(--text-secondary);
}

.filters {
  display: flex;
  gap: 12px;
  padding: 16px 24px;
  border-bottom: 1px solid var(--border-subtle);
}

.filter-select {
  flex: 1;
}

.loading-state,
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 16px;
  padding: 48px 24px;
  color: var(--text-secondary);
}

.empty-icon {
  width: 48px;
  height: 48px;
  color: var(--text-muted);
}

.jobs-scroll {
  flex: 1;
  overflow-y: auto;
  padding: 16px;
}

.job-card {
  background: var(--bg-elevated);
  border: 2px solid transparent;
  border-radius: 12px;
  padding: 16px;
  margin-bottom: 12px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.job-card:hover {
  border-color: var(--border-subtle);
}

.job-card.selected {
  border-color: var(--accent-primary);
  box-shadow: 0 0 0 4px var(--accent-glow);
}

.job-route {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  margin-bottom: 16px;
}

.airport-code {
  font-family: var(--font-mono);
  font-size: 18px;
  font-weight: 700;
  color: var(--text-primary);
}

.route-line {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  max-width: 80px;
}

.route-line svg {
  width: 24px;
  height: 24px;
  color: var(--accent-primary);
}

.job-details {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 8px 16px;
  margin-bottom: 16px;
}

.detail-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.detail-label {
  font-size: 12px;
  color: var(--text-muted);
}

.detail-value {
  font-size: 13px;
  font-weight: 500;
  color: var(--text-primary);
}

.cargo-badge {
  background: var(--accent-primary);
  color: white;
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 11px;
}

.job-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding-top: 12px;
  border-top: 1px solid var(--border-subtle);
}

.payout-label {
  display: block;
  font-size: 11px;
  color: var(--text-muted);
  text-transform: uppercase;
}

.payout-value {
  font-size: 20px;
  font-weight: 700;
  color: #22c55e;
}

.accept-btn {
  text-transform: none;
}

.map-panel {
  flex: 1;
  position: relative;
}

.leaflet-map {
  height: 100%;
  width: 100%;
}

.map-info-panel {
  position: absolute;
  bottom: 24px;
  left: 50%;
  transform: translateX(-50%);
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 12px;
  padding: 16px 24px;
  display: flex;
  align-items: center;
  gap: 16px;
  z-index: 1000;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.2);
}

.map-info-panel h3 {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary);
  max-width: 200px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.route-arrow {
  font-size: 12px;
  color: var(--text-muted);
  text-transform: uppercase;
}
</style>
