<template>
  <div class="airport-selection">
    <div class="selection-panel">
      <div class="panel-header">
        <h1 class="panel-title">Select Your Home Airport</h1>
        <p class="panel-subtitle">This will be your starting location and base of operations</p>
      </div>

      <div class="search-container">
        <div class="search-input-wrapper">
          <svg class="search-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="11" cy="11" r="8"/>
            <path d="M21 21l-4.35-4.35"/>
          </svg>
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Search by ICAO code or airport name..."
            class="search-input"
            @input="handleSearch"
          />
          <div v-if="searching" class="search-spinner">
            <v-progress-circular indeterminate size="20" width="2" color="primary" />
          </div>
        </div>
      </div>

      <div class="search-results" v-if="searchResults.length > 0">
        <div
          v-for="airport in searchResults"
          :key="airport.id"
          class="airport-item"
          :class="{ selected: selectedAirport?.id === airport.id }"
          @click="selectAirport(airport)"
        >
          <div class="airport-code">{{ airport.ident }}</div>
          <div class="airport-details">
            <span class="airport-name">{{ airport.name }}</span>
            <span class="airport-location">{{ airport.municipality || '' }}{{ airport.municipality && airport.country ? ', ' : '' }}{{ airport.country || '' }}</span>
          </div>
          <div v-if="airport.iataCode" class="airport-iata">{{ airport.iataCode }}</div>
        </div>
      </div>

      <div v-else-if="searchQuery.length >= 2 && !searching" class="no-results">
        <p>No airports found matching "{{ searchQuery }}"</p>
      </div>

      <div v-if="selectedAirport" class="selected-airport-card">
        <div class="selected-header">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/>
            <polyline points="9,22 9,12 15,12 15,22"/>
          </svg>
          <span>Selected Home Airport</span>
        </div>
        <div class="selected-details">
          <span class="selected-code">{{ selectedAirport.ident }}</span>
          <span class="selected-name">{{ selectedAirport.name }}</span>
        </div>
      </div>

      <v-btn
        color="primary"
        size="large"
        block
        class="confirm-btn"
        :disabled="!selectedAirport"
        :loading="saving"
        @click="confirmSelection"
      >
        Confirm Home Airport
      </v-btn>
    </div>

    <div class="map-panel">
      <l-map
        ref="mapRef"
        :zoom="zoom"
        :center="mapCenter"
        :use-global-leaflet="false"
        class="selection-map"
      >
        <l-tile-layer
          url="https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png"
          attribution="&copy; OpenStreetMap contributors &copy; CARTO"
        />

        <l-marker
          v-if="selectedAirport"
          :lat-lng="[selectedAirport.latitude, selectedAirport.longitude]"
        >
          <l-icon :icon-url="homeIcon" :icon-size="[48, 48]" :icon-anchor="[24, 48]" />
          <l-popup>
            <strong>{{ selectedAirport.name }}</strong><br>
            <span>{{ selectedAirport.ident }}</span>
          </l-popup>
        </l-marker>
      </l-map>

      <div v-if="!selectedAirport" class="map-overlay">
        <div class="overlay-content">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
            <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/>
            <circle cx="12" cy="10" r="3"/>
          </svg>
          <p>Search and select an airport to see it on the map</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { LMap, LTileLayer, LMarker, LIcon, LPopup } from '@vue-leaflet/vue-leaflet'
import 'leaflet/dist/leaflet.css'
import { api, type Airport } from '../services/api'
import { useUserStore } from '../stores/user'

const router = useRouter()
const userStore = useUserStore()

const searchQuery = ref('')
const searchResults = ref<Airport[]>([])
const selectedAirport = ref<Airport | null>(null)
const searching = ref(false)
const saving = ref(false)

const mapRef = ref()
const zoom = ref(3)
const mapCenter = ref<[number, number]>([30, 0])

let searchTimeout: ReturnType<typeof setTimeout> | null = null

const homeIcon = computed(() => {
  const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="48" height="48">
    <path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7z" fill="#3b82f6"/>
    <path d="M12 6l-4 3v5h2.5v-3h3v3H16v-5l-4-3z" fill="white"/>
  </svg>`
  return `data:image/svg+xml;base64,${btoa(svg)}`
})

async function handleSearch() {
  if (searchTimeout) {
    clearTimeout(searchTimeout)
  }

  if (searchQuery.value.length < 2) {
    searchResults.value = []
    return
  }

  searchTimeout = setTimeout(async () => {
    searching.value = true
    const response = await api.airports.search(searchQuery.value, 20)
    if (response.data) {
      searchResults.value = response.data
    }
    searching.value = false
  }, 300)
}

function selectAirport(airport: Airport) {
  selectedAirport.value = airport

  if (mapRef.value?.leafletObject) {
    mapRef.value.leafletObject.flyTo(
      [airport.latitude, airport.longitude],
      8,
      { duration: 1.5 }
    )
  }
}

async function confirmSelection() {
  if (!selectedAirport.value || !userStore.user.value) return

  saving.value = true

  const response = await api.auth.setHomeAirport(selectedAirport.value.id)

  if (response.data) {
    userStore.updateUser({
      homeAirportId: selectedAirport.value.id,
      homeAirport: selectedAirport.value,
      currentAirportId: selectedAirport.value.id,
      currentAirport: selectedAirport.value,
    })
    router.push('/dashboard')
  }

  saving.value = false
}
</script>

<style scoped>
.airport-selection {
  display: flex;
  height: 100%;
  background: var(--bg-primary);
}

.selection-panel {
  width: 480px;
  background: var(--bg-secondary);
  border-right: 1px solid var(--border-subtle);
  display: flex;
  flex-direction: column;
  padding: 32px;
}

.panel-header {
  margin-bottom: 24px;
}

.panel-title {
  font-size: 24px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 8px;
}

.panel-subtitle {
  font-size: 14px;
  color: var(--text-secondary);
}

.search-container {
  margin-bottom: 16px;
}

.search-input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.search-icon {
  position: absolute;
  left: 16px;
  width: 20px;
  height: 20px;
  color: var(--text-muted);
  pointer-events: none;
}

.search-input {
  width: 100%;
  padding: 14px 48px 14px 48px;
  background: var(--bg-elevated);
  border: 2px solid transparent;
  border-radius: 12px;
  font-size: 15px;
  color: var(--text-primary);
  outline: none;
  transition: all 0.2s ease;
}

.search-input::placeholder {
  color: var(--text-muted);
}

.search-input:focus {
  border-color: var(--accent-primary);
  background: var(--bg-secondary);
}

.search-spinner {
  position: absolute;
  right: 16px;
}

.search-results {
  flex: 1;
  overflow-y: auto;
  margin-bottom: 16px;
}

.airport-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 16px;
  border-radius: 10px;
  cursor: pointer;
  transition: all 0.2s ease;
  margin-bottom: 4px;
}

.airport-item:hover {
  background: var(--bg-elevated);
}

.airport-item.selected {
  background: rgba(59, 130, 246, 0.1);
  border: 1px solid rgba(59, 130, 246, 0.3);
}

.airport-code {
  font-family: var(--font-mono);
  font-size: 14px;
  font-weight: 700;
  color: var(--accent-primary);
  background: var(--bg-elevated);
  padding: 6px 10px;
  border-radius: 6px;
  min-width: 60px;
  text-align: center;
}

.airport-details {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.airport-name {
  font-size: 14px;
  font-weight: 500;
  color: var(--text-primary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.airport-location {
  font-size: 12px;
  color: var(--text-muted);
}

.airport-iata {
  font-family: var(--font-mono);
  font-size: 12px;
  color: var(--text-secondary);
  background: var(--bg-primary);
  padding: 4px 8px;
  border-radius: 4px;
}

.no-results {
  text-align: center;
  padding: 32px;
  color: var(--text-muted);
}

.selected-airport-card {
  background: var(--bg-elevated);
  border: 1px solid var(--border-subtle);
  border-radius: 12px;
  padding: 16px;
  margin-bottom: 16px;
}

.selected-header {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 12px;
  font-weight: 600;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 12px;
}

.selected-header svg {
  width: 16px;
  height: 16px;
  color: var(--accent-primary);
}

.selected-details {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.selected-code {
  font-family: var(--font-mono);
  font-size: 20px;
  font-weight: 700;
  color: var(--accent-primary);
}

.selected-name {
  font-size: 14px;
  color: var(--text-primary);
}

.confirm-btn {
  margin-top: auto;
  text-transform: none;
  font-weight: 600;
}

.map-panel {
  flex: 1;
  position: relative;
}

.selection-map {
  height: 100%;
  width: 100%;
}

.map-overlay {
  position: absolute;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.overlay-content {
  text-align: center;
  color: var(--text-secondary);
}

.overlay-content svg {
  width: 64px;
  height: 64px;
  margin-bottom: 16px;
  opacity: 0.5;
}

.overlay-content p {
  font-size: 16px;
}
</style>
