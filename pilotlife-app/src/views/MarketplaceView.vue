<template>
  <div class="marketplace-layout">
    <div class="marketplace-list-panel">
      <div class="panel-header">
        <h2 class="panel-title">Aircraft Marketplace</h2>
        <p class="panel-subtitle">{{ searchMode === 'local' ? 'Aircraft at your location' : 'Search results' }}</p>
      </div>

      <!-- Search Section -->
      <div class="search-section">
        <v-autocomplete
          v-model="selectedAircraftType"
          :items="aircraftTypes"
          item-title="title"
          item-value="id"
          label="Search Aircraft Type"
          variant="outlined"
          density="compact"
          clearable
          hide-details
          placeholder="e.g., Cessna 172, Boeing 737..."
          class="aircraft-search"
          @update:model-value="onAircraftTypeChange"
        />

        <!-- Advanced Filters (shown when searching) -->
        <div v-if="showAdvancedFilters" class="advanced-filters">
          <v-text-field
            v-model.number="maxDistance"
            label="Max Distance (nm)"
            variant="outlined"
            density="compact"
            type="number"
            hide-details
            placeholder="e.g., 500"
            class="filter-input"
          />
          <v-text-field
            v-model.number="maxPrice"
            label="Max Price ($)"
            variant="outlined"
            density="compact"
            type="number"
            hide-details
            placeholder="e.g., 500000"
            class="filter-input"
          />
          <v-text-field
            v-model.number="minCondition"
            label="Min Condition (%)"
            variant="outlined"
            density="compact"
            type="number"
            hide-details
            placeholder="e.g., 80"
            class="filter-input"
          />
          <v-btn
            color="primary"
            variant="flat"
            class="search-btn"
            :loading="isSearching"
            @click="performSearch"
          >
            Search
          </v-btn>
        </div>

        <!-- Mode Toggle -->
        <div class="mode-toggle">
          <v-btn-toggle v-model="searchMode" mandatory density="compact" color="primary">
            <v-btn value="local" size="small">
              <v-icon start size="small">mdi-map-marker</v-icon>
              Local
            </v-btn>
            <v-btn value="search" size="small">
              <v-icon start size="small">mdi-magnify</v-icon>
              Search
            </v-btn>
          </v-btn-toggle>
        </div>
      </div>

      <!-- Results Info -->
      <div v-if="searchMode === 'search' && searchResult" class="results-info">
        <span>Found {{ searchResult.totalCount }} aircraft across {{ searchResult.searchedAirports }} airports</span>
        <span v-if="searchResult.totalCount > 20" class="limit-warning">
          (showing first 20 - add filters to narrow results)
        </span>
      </div>

      <div v-if="isLoading" class="loading-state">
        <v-progress-circular indeterminate color="primary" />
        <span>Loading inventory...</span>
      </div>

      <div v-else-if="displayedInventory.length === 0" class="empty-state">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="empty-icon">
          <path d="M22 12h-4l-3 9L9 3l-3 9H2"/>
        </svg>
        <p v-if="searchMode === 'local'">No aircraft available at your location</p>
        <p v-else>No aircraft found matching your criteria</p>
        <p class="empty-hint">{{ searchMode === 'local' ? 'Try searching for a specific aircraft type' : 'Try adjusting your filters' }}</p>
      </div>

      <div v-else class="inventory-scroll">
        <div
          v-for="item in displayedInventory"
          :key="item.id"
          class="aircraft-card"
          :class="{ selected: selectedItem?.id === item.id }"
          @click="selectItem(item)"
        >
          <div class="aircraft-header">
            <div class="aircraft-info">
              <span class="aircraft-name">{{ item.aircraft?.title || 'Unknown Aircraft' }}</span>
              <span class="aircraft-location">
                <v-icon size="x-small">mdi-map-marker</v-icon>
                {{ item.dealer?.airportIcao || 'Unknown' }}
                <span v-if="getDistanceToAircraft(item)" class="distance-badge">
                  {{ getDistanceToAircraft(item) }} nm
                </span>
              </span>
            </div>
            <div class="condition-badge" :class="getConditionClass(item.condition)">
              {{ item.condition }}%
            </div>
          </div>

          <div class="aircraft-details">
            <div class="detail-row">
              <span class="detail-label">Dealer</span>
              <span class="detail-value">{{ item.dealer?.name || 'Unknown' }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">Hours</span>
              <span class="detail-value">{{ item.totalFlightHours.toLocaleString() }} hrs</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">Status</span>
              <span class="detail-value" :class="{ 'text-success': item.isNew }">
                {{ item.isNew ? 'New' : 'Used' }}
              </span>
            </div>
            <div v-if="item.hasWarranty" class="detail-row">
              <span class="detail-label">Warranty</span>
              <span class="detail-value text-success">{{ item.warrantyMonths }}mo</span>
            </div>
          </div>

          <div class="aircraft-footer">
            <div class="pricing">
              <div v-if="item.discountPercent > 0" class="base-price">${{ item.basePrice.toLocaleString() }}</div>
              <div class="list-price">${{ item.listPrice.toLocaleString() }}</div>
            </div>
            <v-btn
              size="small"
              color="primary"
              class="buy-btn"
              :disabled="!canAfford(item.listPrice)"
              @click.stop="purchaseItem(item)"
              :loading="purchasingId === item.id"
            >
              {{ canAfford(item.listPrice) ? 'Buy' : 'Insufficient' }}
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

        <!-- Current Location Marker -->
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

        <!-- Aircraft Location Markers -->
        <template v-for="location in aircraftLocations" :key="location.icao">
          <l-marker
            :lat-lng="[location.latitude, location.longitude]"
            @click="onMarkerClick(location)"
          >
            <l-icon
              :icon-url="location.icao === selectedItem?.dealer?.airportIcao ? selectedAircraftIcon : aircraftIcon"
              :icon-size="[24, 24]"
              :icon-anchor="[12, 12]"
            />
            <l-popup>
              <strong>{{ location.name || location.icao }}</strong><br>
              <span>{{ location.count }} aircraft available</span><br>
              <span v-if="location.distance">{{ location.distance }} nm away</span>
            </l-popup>
          </l-marker>
        </template>

        <!-- Line from current location to selected aircraft -->
        <l-polyline
          v-if="selectedItem && selectedItemLocation"
          :lat-lngs="[[currentAirport?.latitude || 0, currentAirport?.longitude || 0], [selectedItemLocation.latitude, selectedItemLocation.longitude]]"
          :color="'#3b82f6'"
          :weight="2"
          :opacity="0.6"
          :dash-array="'8, 8'"
        />
      </l-map>

      <!-- Selected Aircraft Info Panel -->
      <div v-if="selectedItem" class="selected-info-panel">
        <div class="selected-header">
          <h3>{{ selectedItem.aircraft?.title }}</h3>
          <v-btn icon size="small" variant="text" @click="selectedItem = null">
            <v-icon>mdi-close</v-icon>
          </v-btn>
        </div>
        <div class="selected-details">
          <div class="detail-grid">
            <div class="detail-item">
              <span class="label">Location</span>
              <span class="value">{{ selectedItem.dealer?.airportIcao }}</span>
            </div>
            <div class="detail-item">
              <span class="label">Condition</span>
              <span class="value" :class="getConditionClass(selectedItem.condition)">{{ selectedItem.condition }}%</span>
            </div>
            <div class="detail-item">
              <span class="label">Flight Hours</span>
              <span class="value">{{ selectedItem.totalFlightHours.toLocaleString() }}</span>
            </div>
            <div class="detail-item">
              <span class="label">Price</span>
              <span class="value price">${{ selectedItem.listPrice.toLocaleString() }}</span>
            </div>
          </div>
          <v-btn
            block
            color="primary"
            class="mt-3"
            :disabled="!canAfford(selectedItem.listPrice)"
            @click="purchaseItem(selectedItem)"
            :loading="purchasingId === selectedItem.id"
          >
            {{ canAfford(selectedItem.listPrice) ? 'Purchase Aircraft' : 'Insufficient Funds' }}
          </v-btn>
        </div>
      </div>

      <!-- Balance Display -->
      <div class="balance-display">
        <span class="balance-label">Your Balance</span>
        <span class="balance-value">${{ currentBalance.toLocaleString() }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { LMap, LTileLayer, LMarker, LIcon, LPopup, LPolyline } from '@vue-leaflet/vue-leaflet'
import 'leaflet/dist/leaflet.css'
import type { DealerInventoryResponse, AircraftResponse, MarketplaceSearchResult } from '../services/api'
import { api } from '../services/api'
import { useMarketplaceStore } from '../stores/marketplace'
import { useWorldStore } from '../stores/world'

const marketplaceStore = useMarketplaceStore()
const worldStore = useWorldStore()

const mapRef = ref()
const zoom = ref(6)
const mapCenter = ref<[number, number]>([40, -95])

const selectedItem = ref<DealerInventoryResponse | null>(null)
const purchasingId = ref<string | null>(null)
const isSearching = ref(false)

// Search state
const searchMode = ref<'local' | 'search'>('local')
const selectedAircraftType = ref<string | null>(null)
const maxDistance = ref<number | null>(null)
const maxPrice = ref<number | null>(null)
const minCondition = ref<number | null>(null)
const searchResult = ref<MarketplaceSearchResult | null>(null)
const searchInventory = ref<DealerInventoryResponse[]>([])

// Aircraft types for autocomplete
const aircraftTypes = ref<AircraftResponse[]>([])

const isLoading = computed(() => marketplaceStore.isLoading.value || isSearching.value)

const showAdvancedFilters = computed(() => searchMode.value === 'search')

const currentBalance = computed(() => {
  return worldStore.currentPlayerWorld.value?.balance ?? 0
})

const currentAirport = computed(() => {
  const icao = worldStore.currentPlayerWorld.value?.currentAirportIdent
  if (!icao) return null
  // We'll need to get airport data - for now use a simple lookup
  return {
    icao,
    name: icao,
    latitude: mapCenter.value[0],
    longitude: mapCenter.value[1]
  }
})

const displayedInventory = computed(() => {
  if (searchMode.value === 'search') {
    return searchInventory.value
  }
  return marketplaceStore.inventory.value
})

interface AircraftLocation {
  icao: string
  name?: string
  latitude: number
  longitude: number
  count: number
  distance?: number
}

const aircraftLocations = computed((): AircraftLocation[] => {
  const locations = new Map<string, AircraftLocation>()

  for (const item of displayedInventory.value) {
    const icao = item.dealer?.airportIcao
    if (!icao) continue

    const airport = item.dealer?.airport
    if (!airport) continue

    if (locations.has(icao)) {
      locations.get(icao)!.count++
    } else {
      locations.set(icao, {
        icao,
        name: airport.name,
        latitude: airport.latitude,
        longitude: airport.longitude,
        count: 1,
        distance: undefined // TODO: Calculate distance
      })
    }
  }

  return Array.from(locations.values())
})

const selectedItemLocation = computed(() => {
  if (!selectedItem.value?.dealer?.airport) return null
  return selectedItem.value.dealer.airport
})

// Map icons
const currentLocationIcon = computed(() => {
  const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="#22c55e"><circle cx="12" cy="12" r="10" fill="#22c55e"/><circle cx="12" cy="12" r="4" fill="white"/></svg>`
  return `data:image/svg+xml;base64,${btoa(svg)}`
})

const aircraftIcon = computed(() => {
  const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="#3b82f6"><circle cx="12" cy="12" r="10" fill="#3b82f6"/><path d="M12 6l-3 6h6l-3-6z" fill="white"/><rect x="10" y="12" width="4" height="6" fill="white"/></svg>`
  return `data:image/svg+xml;base64,${btoa(svg)}`
})

const selectedAircraftIcon = computed(() => {
  const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="#f59e0b"><circle cx="12" cy="12" r="10" fill="#f59e0b"/><path d="M12 6l-3 6h6l-3-6z" fill="white"/><rect x="10" y="12" width="4" height="6" fill="white"/></svg>`
  return `data:image/svg+xml;base64,${btoa(svg)}`
})

function canAfford(price: number): boolean {
  return currentBalance.value >= price
}

function getConditionClass(condition: number): string {
  if (condition >= 90) return 'condition-excellent'
  if (condition >= 75) return 'condition-good'
  if (condition >= 50) return 'condition-fair'
  return 'condition-poor'
}

function getDistanceToAircraft(item: DealerInventoryResponse): string | null {
  // TODO: Calculate actual distance from current location
  return null
}

function selectItem(item: DealerInventoryResponse) {
  selectedItem.value = item

  // Pan map to selected aircraft location
  if (item.dealer?.airport && mapRef.value?.leafletObject) {
    mapRef.value.leafletObject.panTo([item.dealer.airport.latitude, item.dealer.airport.longitude])
  }
}

function onMarkerClick(location: AircraftLocation) {
  // Select first aircraft at this location
  const item = displayedInventory.value.find(i => i.dealer?.airportIcao === location.icao)
  if (item) {
    selectItem(item)
  }
}

function onAircraftTypeChange() {
  if (selectedAircraftType.value) {
    searchMode.value = 'search'
  }
}

async function performSearch() {
  const currentIcao = worldStore.currentPlayerWorld.value?.currentAirportIdent
  if (!currentIcao) return

  isSearching.value = true

  try {
    const response = await api.marketplace.search({
      fromAirportIcao: currentIcao,
      aircraftType: selectedAircraftType.value || undefined,
      maxDistance: maxDistance.value || undefined,
      maxPrice: maxPrice.value || undefined,
      minCondition: minCondition.value || undefined,
      limit: 20
    })

    if (response.data) {
      searchResult.value = response.data
      searchInventory.value = response.data.inventory
    }
  } catch (error) {
    console.error('Search failed:', error)
  } finally {
    isSearching.value = false
  }
}

async function purchaseItem(item: DealerInventoryResponse) {
  purchasingId.value = item.id

  const result = await marketplaceStore.purchaseAircraft({
    inventoryId: item.id,
    useFinancing: false,
  })

  if (result) {
    selectedItem.value = null
    // Refresh player world to update balance
    await worldStore.loadMyWorlds()
    // Reload local inventory
    await loadLocalInventory()
  }

  purchasingId.value = null
}

async function loadLocalInventory() {
  const currentIcao = worldStore.currentPlayerWorld.value?.currentAirportIdent
  if (currentIcao) {
    await marketplaceStore.loadDealers(currentIcao)
    await marketplaceStore.loadInventory()
  }
}

async function loadAircraftTypes() {
  const response = await api.aircraft.list(true)
  if (response.data) {
    aircraftTypes.value = response.data
  }
}

watch(searchMode, (newMode) => {
  if (newMode === 'local') {
    searchResult.value = null
    searchInventory.value = []
    loadLocalInventory()
  }
})

watch(
  () => worldStore.currentPlayerWorld.value?.currentAirportIdent,
  async (newIcao) => {
    if (newIcao) {
      // Try to get airport coordinates for map centering
      const response = await api.airports.getByIdent(newIcao)
      if (response.data) {
        mapCenter.value = [response.data.latitude, response.data.longitude]
      }
      if (searchMode.value === 'local') {
        await loadLocalInventory()
      }
    }
  },
  { immediate: true }
)

onMounted(async () => {
  await loadAircraftTypes()
  await loadLocalInventory()
})
</script>

<style scoped>
.marketplace-layout {
  display: flex;
  height: 100%;
  background: var(--bg-primary);
}

.marketplace-list-panel {
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

.search-section {
  padding: 16px;
  border-bottom: 1px solid var(--border-subtle);
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.aircraft-search {
  width: 100%;
}

.advanced-filters {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 8px;
}

.filter-input {
  font-size: 13px;
}

.search-btn {
  grid-column: span 2;
  text-transform: none;
}

.mode-toggle {
  display: flex;
  justify-content: center;
}

.results-info {
  padding: 8px 16px;
  background: rgba(59, 130, 246, 0.1);
  border-bottom: 1px solid var(--border-subtle);
  font-size: 12px;
  color: var(--text-secondary);
}

.limit-warning {
  color: #f59e0b;
  display: block;
  margin-top: 2px;
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

.empty-hint {
  font-size: 13px;
  color: var(--text-muted);
}

.inventory-scroll {
  flex: 1;
  overflow-y: auto;
  padding: 16px;
}

.aircraft-card {
  background: var(--bg-elevated);
  border: 2px solid transparent;
  border-radius: 12px;
  padding: 14px;
  margin-bottom: 10px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.aircraft-card:hover {
  border-color: var(--border-subtle);
}

.aircraft-card.selected {
  border-color: var(--accent-primary);
  box-shadow: 0 0 0 4px var(--accent-glow);
}

.aircraft-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 10px;
}

.aircraft-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.aircraft-name {
  font-weight: 600;
  font-size: 14px;
  color: var(--text-primary);
}

.aircraft-location {
  font-size: 12px;
  color: var(--text-muted);
  display: flex;
  align-items: center;
  gap: 4px;
}

.distance-badge {
  background: rgba(59, 130, 246, 0.2);
  color: #3b82f6;
  padding: 1px 6px;
  border-radius: 4px;
  font-size: 10px;
  margin-left: 4px;
}

.condition-badge {
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 11px;
  font-weight: 600;
}

.condition-excellent {
  background: rgba(34, 197, 94, 0.2);
  color: #22c55e;
}

.condition-good {
  background: rgba(59, 130, 246, 0.2);
  color: #3b82f6;
}

.condition-fair {
  background: rgba(234, 179, 8, 0.2);
  color: #eab308;
}

.condition-poor {
  background: rgba(239, 68, 68, 0.2);
  color: #ef4444;
}

.aircraft-details {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 6px 12px;
  margin-bottom: 10px;
}

.detail-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.detail-label {
  font-size: 11px;
  color: var(--text-muted);
}

.detail-value {
  font-size: 12px;
  font-weight: 500;
  color: var(--text-primary);
}

.text-success {
  color: #22c55e;
}

.aircraft-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding-top: 10px;
  border-top: 1px solid var(--border-subtle);
}

.pricing {
  display: flex;
  flex-direction: column;
}

.base-price {
  font-size: 11px;
  color: var(--text-muted);
  text-decoration: line-through;
}

.list-price {
  font-size: 16px;
  font-weight: 700;
  color: #22c55e;
}

.buy-btn {
  text-transform: none;
  font-size: 12px;
}

/* Map Panel */
.map-panel {
  flex: 1;
  position: relative;
}

.leaflet-map {
  height: 100%;
  width: 100%;
}

.selected-info-panel {
  position: absolute;
  bottom: 80px;
  left: 50%;
  transform: translateX(-50%);
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 12px;
  padding: 16px;
  min-width: 320px;
  max-width: 400px;
  z-index: 1000;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.3);
}

.selected-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.selected-header h3 {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0;
}

.detail-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 8px;
}

.detail-item {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.detail-item .label {
  font-size: 11px;
  color: var(--text-muted);
}

.detail-item .value {
  font-size: 14px;
  font-weight: 500;
  color: var(--text-primary);
}

.detail-item .value.price {
  color: #22c55e;
  font-weight: 700;
}

.balance-display {
  position: absolute;
  top: 16px;
  right: 16px;
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 8px;
  padding: 10px 16px;
  z-index: 1000;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
}

.balance-label {
  font-size: 11px;
  color: var(--text-muted);
}

.balance-value {
  font-size: 18px;
  font-weight: 700;
  color: #22c55e;
}
</style>
