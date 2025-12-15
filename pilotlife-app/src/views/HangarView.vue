<template>
  <div class="hangar-layout">
    <div class="hangar-list-panel">
      <div class="panel-header">
        <h2 class="panel-title">My Hangar</h2>
        <p class="panel-subtitle">Your aircraft fleet</p>
      </div>

      <div class="filters">
        <v-select
          v-model="selectedLocation"
          :items="locationOptions"
          label="Location"
          variant="outlined"
          density="compact"
          clearable
          hide-details
          class="filter-select"
        />
        <v-select
          v-model="selectedStatus"
          :items="statusOptions"
          label="Status"
          variant="outlined"
          density="compact"
          clearable
          hide-details
          class="filter-select"
        />
      </div>

      <div v-if="isLoading" class="loading-state">
        <v-progress-circular indeterminate color="primary" />
        <span>Loading aircraft...</span>
      </div>

      <div v-else-if="filteredAircraft.length === 0" class="empty-state">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="empty-icon">
          <path d="M22 12h-4l-3 9L9 3l-3 9H2"/>
        </svg>
        <p>No aircraft in your hangar</p>
        <p class="empty-hint">Visit the marketplace to purchase aircraft</p>
        <v-btn
          color="primary"
          variant="outlined"
          class="mt-4"
          @click="goToMarketplace"
        >
          Browse Marketplace
        </v-btn>
      </div>

      <div v-else class="aircraft-scroll">
        <div
          v-for="aircraft in filteredAircraft"
          :key="aircraft.id"
          class="aircraft-card"
          :class="{ selected: selectedAircraft?.id === aircraft.id }"
          @click="selectAircraft(aircraft)"
        >
          <div class="aircraft-header">
            <div class="aircraft-info">
              <span class="aircraft-name">{{ aircraft.nickname || aircraft.aircraft?.title || 'Unknown' }}</span>
              <span class="aircraft-reg">{{ aircraft.registration }}</span>
            </div>
            <div class="status-badges">
              <div v-if="!aircraft.isAirworthy" class="status-badge status-grounded">
                Grounded
              </div>
              <div v-else-if="aircraft.isInMaintenance" class="status-badge status-maintenance">
                In Maintenance
              </div>
              <div v-else-if="aircraft.isInUse" class="status-badge status-in-use">
                In Use
              </div>
              <div v-else class="status-badge status-available">
                Available
              </div>
            </div>
          </div>

          <div class="aircraft-details">
            <div class="detail-row">
              <span class="detail-label">Condition</span>
              <span class="detail-value" :class="getConditionClass(aircraft.condition)">
                {{ aircraft.condition }}%
              </span>
            </div>
            <div class="detail-row">
              <span class="detail-label">Location</span>
              <span class="detail-value">{{ aircraft.currentLocationIcao }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">Flight Hours</span>
              <span class="detail-value">{{ aircraft.totalFlightHours.toLocaleString() }} hrs</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">Cycles</span>
              <span class="detail-value">{{ aircraft.totalCycles.toLocaleString() }}</span>
            </div>
          </div>

          <div class="aircraft-footer">
            <div class="value-info">
              <span class="value-label">Est. Value</span>
              <span class="value-amount">${{ aircraft.estimatedValue.toLocaleString() }}</span>
            </div>
            <div v-if="aircraft.isListedForSale" class="for-sale-badge">
              For Sale
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="details-panel">
      <template v-if="selectedAircraft">
        <div class="details-header">
          <div class="header-top">
            <div>
              <h2>{{ selectedAircraft.nickname || selectedAircraft.aircraft?.title }}</h2>
              <div class="reg-display">{{ selectedAircraft.registration }}</div>
            </div>
            <v-btn
              size="small"
              variant="outlined"
              @click="showNicknameDialog = true"
            >
              Edit Nickname
            </v-btn>
          </div>
          <div class="details-badges">
            <span class="badge" :class="getConditionClass(selectedAircraft.condition)">
              {{ selectedAircraft.condition }}% Condition
            </span>
            <span v-if="selectedAircraft.hasWarranty" class="badge badge-warranty">
              Warranty until {{ formatDate(selectedAircraft.warrantyExpiresAt) }}
            </span>
            <span v-if="selectedAircraft.hasInsurance" class="badge badge-insurance">
              Insured until {{ formatDate(selectedAircraft.insuranceExpiresAt) }}
            </span>
          </div>
        </div>

        <div class="details-section">
          <h3>Aircraft Status</h3>
          <div class="status-grid">
            <div class="status-item" :class="{ active: selectedAircraft.isAirworthy }">
              <div class="status-icon">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M22 12h-4l-3 9L9 3l-3 9H2"/>
                </svg>
              </div>
              <div class="status-text">
                <span class="status-label">Airworthy</span>
                <span class="status-value">{{ selectedAircraft.isAirworthy ? 'Yes' : 'No' }}</span>
              </div>
            </div>
            <div class="status-item" :class="{ active: !selectedAircraft.isInMaintenance }">
              <div class="status-icon">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <circle cx="12" cy="12" r="3"/>
                  <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1 0 2.83 2 2 0 0 1-2.83 0l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-2 2 2 2 0 0 1-2-2v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83 0 2 2 0 0 1 0-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1-2-2 2 2 0 0 1 2-2h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 0-2.83 2 2 0 0 1 2.83 0l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 2-2 2 2 0 0 1 2 2v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 0 2 2 0 0 1 0 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 2 2 2 2 0 0 1-2 2h-.09a1.65 1.65 0 0 0-1.51 1z"/>
                </svg>
              </div>
              <div class="status-text">
                <span class="status-label">Maintenance</span>
                <span class="status-value">{{ selectedAircraft.isInMaintenance ? 'In Progress' : 'None Required' }}</span>
              </div>
            </div>
            <div class="status-item">
              <div class="status-icon">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <circle cx="12" cy="12" r="10"/>
                  <polyline points="12 6 12 12 16 14"/>
                </svg>
              </div>
              <div class="status-text">
                <span class="status-label">Hours Since Inspection</span>
                <span class="status-value">{{ selectedAircraft.hoursSinceLastInspection }} hrs</span>
              </div>
            </div>
          </div>
        </div>

        <div class="details-section">
          <h3>Aircraft Specifications</h3>
          <div class="specs-grid">
            <div class="spec-item">
              <span class="spec-label">Category</span>
              <span class="spec-value">{{ selectedAircraft.aircraft?.category || 'N/A' }}</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">Engine Type</span>
              <span class="spec-value">{{ selectedAircraft.aircraft?.engineTypeStr || 'N/A' }}</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">Engines</span>
              <span class="spec-value">{{ selectedAircraft.aircraft?.numberOfEngines || 'N/A' }}</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">Cruise Speed</span>
              <span class="spec-value">{{ selectedAircraft.aircraft?.cruiseSpeedKts || 'N/A' }} kts</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">Max Gross Weight</span>
              <span class="spec-value">{{ selectedAircraft.aircraft?.maxGrossWeightLbs?.toLocaleString() || 'N/A' }} lbs</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">Empty Weight</span>
              <span class="spec-value">{{ selectedAircraft.aircraft?.emptyWeightLbs?.toLocaleString() || 'N/A' }} lbs</span>
            </div>
          </div>
        </div>

        <div class="details-section">
          <h3>Flight History</h3>
          <div class="specs-grid">
            <div class="spec-item">
              <span class="spec-label">Total Flight Hours</span>
              <span class="spec-value">{{ selectedAircraft.totalFlightHours.toLocaleString() }} hrs</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">Total Flight Minutes</span>
              <span class="spec-value">{{ selectedAircraft.totalFlightMinutes.toLocaleString() }} min</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">Total Cycles</span>
              <span class="spec-value">{{ selectedAircraft.totalCycles.toLocaleString() }}</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">Current Location</span>
              <span class="spec-value">{{ selectedAircraft.currentLocationIcao }}</span>
            </div>
          </div>
        </div>

        <div class="details-section">
          <h3>Financial Information</h3>
          <div class="specs-grid">
            <div class="spec-item">
              <span class="spec-label">Purchase Price</span>
              <span class="spec-value">${{ selectedAircraft.purchasePrice.toLocaleString() }}</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">Purchase Date</span>
              <span class="spec-value">{{ formatDate(selectedAircraft.purchasedAt) }}</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">Estimated Value</span>
              <span class="spec-value" :class="getValueChangeClass(selectedAircraft)">${{ selectedAircraft.estimatedValue.toLocaleString() }}</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">Value Change</span>
              <span class="spec-value" :class="getValueChangeClass(selectedAircraft)">
                {{ getValueChange(selectedAircraft) }}
              </span>
            </div>
          </div>
        </div>

        <div class="action-section">
          <template v-if="!selectedAircraft.isListedForSale">
            <v-btn
              color="warning"
              variant="outlined"
              class="action-btn"
              @click="showSaleDialog = true"
            >
              List for Sale
            </v-btn>
          </template>
          <template v-else>
            <div class="sale-info">
              <span>Listed for sale</span>
            </div>
            <v-btn
              color="error"
              variant="outlined"
              class="action-btn"
              @click="cancelSale"
              :loading="cancellingSale"
            >
              Cancel Sale
            </v-btn>
          </template>
        </div>
      </template>

      <template v-else>
        <div class="no-selection">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" class="no-selection-icon">
            <path d="M22 12h-4l-3 9L9 3l-3 9H2"/>
          </svg>
          <h3>Select an Aircraft</h3>
          <p>Click on an aircraft from your hangar to view details</p>
        </div>
      </template>
    </div>

    <!-- Nickname Dialog -->
    <v-dialog v-model="showNicknameDialog" max-width="400">
      <v-card>
        <v-card-title>Edit Aircraft Nickname</v-card-title>
        <v-card-text>
          <v-text-field
            v-model="newNickname"
            label="Nickname"
            variant="outlined"
            density="compact"
            :placeholder="selectedAircraft?.aircraft?.title"
          />
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="showNicknameDialog = false">Cancel</v-btn>
          <v-btn color="primary" @click="saveNickname" :loading="savingNickname">Save</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Sale Dialog -->
    <v-dialog v-model="showSaleDialog" max-width="400">
      <v-card>
        <v-card-title>List Aircraft for Sale</v-card-title>
        <v-card-text>
          <p class="mb-4">Set your asking price for {{ selectedAircraft?.nickname || selectedAircraft?.aircraft?.title }}</p>
          <v-text-field
            v-model.number="askingPrice"
            label="Asking Price"
            variant="outlined"
            density="compact"
            type="number"
            prefix="$"
            :hint="`Estimated value: $${selectedAircraft?.estimatedValue?.toLocaleString()}`"
            persistent-hint
          />
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="showSaleDialog = false">Cancel</v-btn>
          <v-btn color="warning" @click="listForSale" :loading="listingSale">List for Sale</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import type { OwnedAircraftResponse } from '../services/api'
import { useMarketplaceStore } from '../stores/marketplace'
import { useWorldStore } from '../stores/world'

const router = useRouter()
const marketplaceStore = useMarketplaceStore()
const worldStore = useWorldStore()

const selectedAircraft = ref<OwnedAircraftResponse | null>(null)
const selectedLocation = ref<string | null>(null)
const selectedStatus = ref<string | null>(null)

const showNicknameDialog = ref(false)
const newNickname = ref('')
const savingNickname = ref(false)

const showSaleDialog = ref(false)
const askingPrice = ref(0)
const listingSale = ref(false)
const cancellingSale = ref(false)

const statusOptions = [
  { title: 'Available', value: 'available' },
  { title: 'In Use', value: 'in_use' },
  { title: 'In Maintenance', value: 'maintenance' },
  { title: 'Grounded', value: 'grounded' },
]

const isLoading = computed(() => marketplaceStore.isLoading.value)

const locationOptions = computed(() => {
  const locations = new Set(marketplaceStore.ownedAircraft.value.map(a => a.currentLocationIcao))
  return Array.from(locations).map(loc => ({ title: loc, value: loc }))
})

const filteredAircraft = computed(() => {
  let aircraft = [...marketplaceStore.ownedAircraft.value]

  if (selectedLocation.value) {
    aircraft = aircraft.filter(a => a.currentLocationIcao === selectedLocation.value)
  }

  if (selectedStatus.value) {
    aircraft = aircraft.filter(a => {
      switch (selectedStatus.value) {
        case 'available':
          return a.isAirworthy && !a.isInMaintenance && !a.isInUse
        case 'in_use':
          return a.isInUse
        case 'maintenance':
          return a.isInMaintenance
        case 'grounded':
          return !a.isAirworthy
        default:
          return true
      }
    })
  }

  return aircraft
})

function getConditionClass(condition: number): string {
  if (condition >= 90) return 'condition-excellent'
  if (condition >= 75) return 'condition-good'
  if (condition >= 50) return 'condition-fair'
  return 'condition-poor'
}

function getValueChangeClass(aircraft: OwnedAircraftResponse): string {
  const change = aircraft.estimatedValue - aircraft.purchasePrice
  if (change > 0) return 'value-up'
  if (change < 0) return 'value-down'
  return ''
}

function getValueChange(aircraft: OwnedAircraftResponse): string {
  const change = aircraft.estimatedValue - aircraft.purchasePrice
  const percent = ((change / aircraft.purchasePrice) * 100).toFixed(1)
  if (change > 0) return `+$${change.toLocaleString()} (+${percent}%)`
  if (change < 0) return `-$${Math.abs(change).toLocaleString()} (${percent}%)`
  return 'No change'
}

function formatDate(dateStr?: string): string {
  if (!dateStr) return 'N/A'
  return new Date(dateStr).toLocaleDateString()
}

function selectAircraft(aircraft: OwnedAircraftResponse) {
  selectedAircraft.value = aircraft
  newNickname.value = aircraft.nickname || ''
  askingPrice.value = aircraft.estimatedValue
}

async function saveNickname() {
  if (!selectedAircraft.value) return
  savingNickname.value = true

  const result = await marketplaceStore.updateAircraftNickname(
    selectedAircraft.value.id,
    newNickname.value
  )

  if (result) {
    selectedAircraft.value = result
  }

  savingNickname.value = false
  showNicknameDialog.value = false
}

async function listForSale() {
  if (!selectedAircraft.value || askingPrice.value <= 0) return
  listingSale.value = true

  const result = await marketplaceStore.listAircraftForSale(
    selectedAircraft.value.id,
    askingPrice.value
  )

  if (result) {
    selectedAircraft.value = result
  }

  listingSale.value = false
  showSaleDialog.value = false
}

async function cancelSale() {
  if (!selectedAircraft.value) return
  cancellingSale.value = true

  const result = await marketplaceStore.cancelAircraftSale(selectedAircraft.value.id)

  if (result) {
    selectedAircraft.value = result
  }

  cancellingSale.value = false
}

function goToMarketplace() {
  router.push('/marketplace')
}

async function loadHangar() {
  const worldId = worldStore.currentPlayerWorld.value?.worldId
  if (worldId) {
    await marketplaceStore.loadMyAircraft(worldId)
  }
}

watch(
  () => worldStore.currentPlayerWorld.value?.worldId,
  () => {
    loadHangar()
  }
)

onMounted(() => {
  loadHangar()
})
</script>

<style scoped>
.hangar-layout {
  display: flex;
  height: 100%;
  background: var(--bg-primary);
}

.hangar-list-panel {
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

.empty-hint {
  font-size: 13px;
  color: var(--text-muted);
}

.aircraft-scroll {
  flex: 1;
  overflow-y: auto;
  padding: 16px;
}

.aircraft-card {
  background: var(--bg-elevated);
  border: 2px solid transparent;
  border-radius: 12px;
  padding: 16px;
  margin-bottom: 12px;
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
  margin-bottom: 12px;
}

.aircraft-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.aircraft-name {
  font-weight: 600;
  font-size: 15px;
  color: var(--text-primary);
}

.aircraft-reg {
  font-family: var(--font-mono);
  font-size: 12px;
  color: var(--text-muted);
}

.status-badges {
  display: flex;
  gap: 4px;
}

.status-badge {
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 11px;
  font-weight: 600;
}

.status-available {
  background: rgba(34, 197, 94, 0.2);
  color: #22c55e;
}

.status-in-use {
  background: rgba(59, 130, 246, 0.2);
  color: #3b82f6;
}

.status-maintenance {
  background: rgba(234, 179, 8, 0.2);
  color: #eab308;
}

.status-grounded {
  background: rgba(239, 68, 68, 0.2);
  color: #ef4444;
}

.aircraft-details {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 8px 16px;
  margin-bottom: 12px;
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

.condition-excellent {
  color: #22c55e;
}

.condition-good {
  color: #3b82f6;
}

.condition-fair {
  color: #eab308;
}

.condition-poor {
  color: #ef4444;
}

.aircraft-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding-top: 12px;
  border-top: 1px solid var(--border-subtle);
}

.value-info {
  display: flex;
  flex-direction: column;
}

.value-label {
  font-size: 11px;
  color: var(--text-muted);
}

.value-amount {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-primary);
}

.for-sale-badge {
  background: rgba(234, 179, 8, 0.2);
  color: #eab308;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 11px;
  font-weight: 600;
}

/* Details Panel */
.details-panel {
  flex: 1;
  overflow-y: auto;
  padding: 24px;
}

.details-header {
  margin-bottom: 24px;
}

.header-top {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 12px;
}

.details-header h2 {
  font-size: 24px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 4px;
}

.reg-display {
  font-family: var(--font-mono);
  font-size: 14px;
  color: var(--text-muted);
}

.details-badges {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.badge {
  padding: 6px 12px;
  border-radius: 6px;
  font-size: 12px;
  font-weight: 600;
}

.badge-warranty {
  background: rgba(59, 130, 246, 0.2);
  color: #3b82f6;
}

.badge-insurance {
  background: rgba(34, 197, 94, 0.2);
  color: #22c55e;
}

.details-section {
  background: var(--bg-secondary);
  border-radius: 12px;
  padding: 20px;
  margin-bottom: 16px;
}

.details-section h3 {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 16px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.status-grid {
  display: flex;
  gap: 16px;
}

.status-item {
  flex: 1;
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  background: var(--bg-elevated);
  border-radius: 8px;
  opacity: 0.6;
}

.status-item.active {
  opacity: 1;
  border: 1px solid rgba(34, 197, 94, 0.3);
}

.status-icon {
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.status-icon svg {
  width: 20px;
  height: 20px;
  color: var(--text-muted);
}

.status-item.active .status-icon svg {
  color: #22c55e;
}

.status-text {
  display: flex;
  flex-direction: column;
}

.status-label {
  font-size: 11px;
  color: var(--text-muted);
}

.status-value {
  font-size: 13px;
  font-weight: 500;
  color: var(--text-primary);
}

.specs-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.spec-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.spec-label {
  font-size: 12px;
  color: var(--text-muted);
}

.spec-value {
  font-size: 14px;
  font-weight: 500;
  color: var(--text-primary);
}

.value-up {
  color: #22c55e;
}

.value-down {
  color: #ef4444;
}

.action-section {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 20px;
  background: var(--bg-secondary);
  border-radius: 12px;
}

.action-btn {
  text-transform: none;
}

.sale-info {
  flex: 1;
  font-size: 14px;
  color: #eab308;
  font-weight: 500;
}

/* No Selection State */
.no-selection {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  text-align: center;
  color: var(--text-secondary);
}

.no-selection-icon {
  width: 80px;
  height: 80px;
  color: var(--text-muted);
  margin-bottom: 24px;
}

.no-selection h3 {
  font-size: 18px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 8px;
}

.no-selection p {
  font-size: 14px;
  color: var(--text-muted);
}
</style>
