<template>
  <div class="admin-view">
    <div class="admin-header">
      <h1 class="admin-title">Admin Panel</h1>
      <p class="admin-subtitle">Manage aircraft and review requests</p>
    </div>

    <div class="admin-tabs">
      <button
        class="tab-btn"
        :class="{ active: activeTab === 'requests' }"
        @click="activeTab = 'requests'"
      >
        Aircraft Requests
        <span v-if="pendingCount > 0" class="badge">{{ pendingCount }}</span>
      </button>
      <button
        class="tab-btn"
        :class="{ active: activeTab === 'aircraft' }"
        @click="activeTab = 'aircraft'"
      >
        Aircraft Database
      </button>
    </div>

    <div class="admin-content">
      <!-- Aircraft Requests Tab -->
      <div v-if="activeTab === 'requests'" class="tab-content">
        <div class="filter-bar">
          <select v-model="statusFilter" class="status-filter">
            <option value="">All Requests</option>
            <option value="Pending">Pending</option>
            <option value="Approved">Approved</option>
            <option value="Rejected">Rejected</option>
          </select>
          <button class="refresh-btn" @click="loadRequests" :disabled="loadingRequests">
            Refresh
          </button>
        </div>

        <div v-if="loadingRequests" class="loading-state">
          Loading requests...
        </div>

        <div v-else-if="filteredRequests.length === 0" class="empty-state">
          No aircraft requests found.
        </div>

        <div v-else class="requests-grid">
          <div
            v-for="request in filteredRequests"
            :key="request.id"
            class="request-card"
            :class="[request.status.toLowerCase(), { expanded: expandedCards[request.id] }]"
          >
            <div class="request-header" @click="toggleCard(request.id)">
              <div class="header-left">
                <span class="expand-icon">{{ expandedCards[request.id] ? '&#9660;' : '&#9654;' }}</span>
                <h3 class="request-title">{{ request.aircraftTitle }}</h3>
              </div>
              <div class="header-right">
                <span v-if="request.hasFileData" class="file-data-badge">Has File Data</span>
                <span class="status-badge" :class="request.status.toLowerCase()">
                  {{ request.status }}
                </span>
              </div>
            </div>

            <!-- Expanded Content -->
            <div v-if="expandedCards[request.id]" class="request-expanded">
              <!-- Inner Tabs -->
              <div class="inner-tabs">
                <button
                  class="inner-tab"
                  :class="{ active: cardTabs[request.id] === 'info' || !cardTabs[request.id] }"
                  @click.stop="setCardTab(request.id, 'info')"
                >
                  Submission Info
                </button>
                <button
                  class="inner-tab"
                  :class="{ active: cardTabs[request.id] === 'manifest', disabled: !request.manifestData }"
                  :disabled="!request.manifestData"
                  @click.stop="setCardTab(request.id, 'manifest')"
                >
                  Manifest.json
                </button>
                <button
                  class="inner-tab"
                  :class="{ active: cardTabs[request.id] === 'aircraft', disabled: !request.aircraftCfgData }"
                  :disabled="!request.aircraftCfgData"
                  @click.stop="setCardTab(request.id, 'aircraft')"
                >
                  Aircraft.cfg
                </button>
              </div>

              <!-- Submission Info Tab -->
              <div v-if="cardTabs[request.id] === 'info' || !cardTabs[request.id]" class="tab-panel">
                <div class="request-details">
                  <div class="detail-row">
                    <span class="detail-label">Type:</span>
                    <span class="detail-value">{{ request.atcType || 'N/A' }}</span>
                  </div>
                  <div class="detail-row">
                    <span class="detail-label">Model:</span>
                    <span class="detail-value">{{ request.atcModel || 'N/A' }}</span>
                  </div>
                  <div class="detail-row">
                    <span class="detail-label">Category:</span>
                    <span class="detail-value">{{ request.category || 'N/A' }}</span>
                  </div>
                  <div class="detail-row">
                    <span class="detail-label">Engine:</span>
                    <span class="detail-value">{{ request.engineTypeStr }} ({{ request.numberOfEngines }})</span>
                  </div>
                  <div class="detail-row">
                    <span class="detail-label">Max Weight:</span>
                    <span class="detail-value">{{ formatNumber(request.maxGrossWeightLbs) }} lbs</span>
                  </div>
                  <div class="detail-row">
                    <span class="detail-label">Empty Weight:</span>
                    <span class="detail-value">{{ formatNumber(request.emptyWeightLbs) }} lbs</span>
                  </div>
                  <div class="detail-row">
                    <span class="detail-label">Cruise Speed:</span>
                    <span class="detail-value">{{ formatNumber(request.cruiseSpeedKts) }} kts</span>
                  </div>
                  <div class="detail-row">
                    <span class="detail-label">Sim Version:</span>
                    <span class="detail-value">{{ request.simulatorVersion || 'N/A' }}</span>
                  </div>
                  <div class="detail-row">
                    <span class="detail-label">Requested By:</span>
                    <span class="detail-value">{{ request.requestedByUserName || 'Unknown' }}</span>
                  </div>
                  <div class="detail-row">
                    <span class="detail-label">Created:</span>
                    <span class="detail-value">{{ formatDate(request.createdAt) }}</span>
                  </div>
                  <div v-if="request.reviewNotes" class="detail-row notes">
                    <span class="detail-label">Notes:</span>
                    <span class="detail-value">{{ request.reviewNotes }}</span>
                  </div>
                </div>
              </div>

              <!-- Manifest.json Tab -->
              <div v-if="cardTabs[request.id] === 'manifest'" class="tab-panel">
                <div v-if="request.manifestData" class="file-data">
                  <div class="file-data-section">
                    <h4 class="section-header">Package Information</h4>
                    <div class="detail-row">
                      <span class="detail-label">Content Type:</span>
                      <span class="detail-value">{{ request.manifestData.contentType || 'N/A' }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="detail-label">Title:</span>
                      <span class="detail-value">{{ request.manifestData.title || 'N/A' }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="detail-label">Manufacturer:</span>
                      <span class="detail-value">{{ request.manifestData.manufacturer || 'N/A' }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="detail-label">Creator:</span>
                      <span class="detail-value">{{ request.manifestData.creator || 'N/A' }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="detail-label">Package Version:</span>
                      <span class="detail-value">{{ request.manifestData.packageVersion || 'N/A' }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="detail-label">Min Game Version:</span>
                      <span class="detail-value">{{ request.manifestData.minimumGameVersion || 'N/A' }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="detail-label">Package Size:</span>
                      <span class="detail-value">{{ request.manifestData.totalPackageSize || 'N/A' }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="detail-label">Content ID:</span>
                      <span class="detail-value mono">{{ request.manifestData.contentId || 'N/A' }}</span>
                    </div>
                  </div>
                </div>
                <div v-else class="no-data">
                  No manifest data available
                </div>
              </div>

              <!-- Aircraft.cfg Tab -->
              <div v-if="cardTabs[request.id] === 'aircraft'" class="tab-panel">
                <div v-if="request.aircraftCfgData" class="file-data">
                  <div class="file-data-section">
                    <h4 class="section-header">[FLTSIM.0] Section</h4>
                    <div class="detail-row">
                      <span class="detail-label">Title:</span>
                      <span class="detail-value">{{ request.aircraftCfgData.title || 'N/A' }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="detail-label">Model:</span>
                      <span class="detail-value">{{ request.aircraftCfgData.model || 'N/A' }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="detail-label">Panel:</span>
                      <span class="detail-value">{{ request.aircraftCfgData.panel || 'N/A' }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="detail-label">Sound:</span>
                      <span class="detail-value">{{ request.aircraftCfgData.sound || 'N/A' }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="detail-label">Texture:</span>
                      <span class="detail-value">{{ request.aircraftCfgData.texture || 'N/A' }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="detail-label">ATC Type:</span>
                      <span class="detail-value">{{ request.aircraftCfgData.atcType || 'N/A' }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="detail-label">ATC Model:</span>
                      <span class="detail-value">{{ request.aircraftCfgData.atcModel || 'N/A' }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="detail-label">ATC ID:</span>
                      <span class="detail-value mono">{{ request.aircraftCfgData.atcId || 'N/A' }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="detail-label">ATC Airline:</span>
                      <span class="detail-value">{{ request.aircraftCfgData.atcAirline || 'N/A' }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="detail-label">UI Manufacturer:</span>
                      <span class="detail-value">{{ request.aircraftCfgData.uiManufacturer || 'N/A' }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="detail-label">UI Type:</span>
                      <span class="detail-value">{{ request.aircraftCfgData.uiType || 'N/A' }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="detail-label">UI Variation:</span>
                      <span class="detail-value">{{ request.aircraftCfgData.uiVariation || 'N/A' }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="detail-label">ICAO Airline:</span>
                      <span class="detail-value mono">{{ request.aircraftCfgData.icaoAirline || 'N/A' }}</span>
                    </div>
                  </div>

                  <div class="file-data-section">
                    <h4 class="section-header">[GENERAL] Section</h4>
                    <div class="detail-row">
                      <span class="detail-label">ATC Type:</span>
                      <span class="detail-value">{{ request.aircraftCfgData.generalAtcType || 'N/A' }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="detail-label">ATC Model:</span>
                      <span class="detail-value">{{ request.aircraftCfgData.generalAtcModel || 'N/A' }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="detail-label">Editable:</span>
                      <span class="detail-value">{{ request.aircraftCfgData.editable || 'N/A' }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="detail-label">Performance:</span>
                      <span class="detail-value">{{ request.aircraftCfgData.performance || 'N/A' }}</span>
                    </div>
                    <div class="detail-row">
                      <span class="detail-label">Category:</span>
                      <span class="detail-value">{{ request.aircraftCfgData.category || 'N/A' }}</span>
                    </div>
                  </div>
                </div>
                <div v-else class="no-data">
                  No aircraft.cfg data available
                </div>
              </div>

              <!-- Actions -->
              <div v-if="request.status === 'Pending'" class="request-actions">
                <v-btn
                  color="success"
                  variant="flat"
                  size="small"
                  :loading="processingId === request.id"
                  @click.stop="approveRequest(request)"
                >
                  Approve
                </v-btn>
                <v-btn
                  color="error"
                  variant="outlined"
                  size="small"
                  :loading="processingId === request.id"
                  @click.stop="rejectRequest(request)"
                >
                  Reject
                </v-btn>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Aircraft Database Tab -->
      <div v-if="activeTab === 'aircraft'" class="tab-content">
        <div class="filter-bar">
          <button class="refresh-btn" @click="loadAircraft" :disabled="loadingAircraft">
            Refresh
          </button>
        </div>

        <div v-if="loadingAircraft" class="loading-state">
          Loading aircraft...
        </div>

        <div v-else-if="aircraft.length === 0" class="empty-state">
          No aircraft in database.
        </div>

        <div v-else class="aircraft-table-container">
          <table class="aircraft-table">
            <thead>
              <tr>
                <th>Title</th>
                <th>Type</th>
                <th>Model</th>
                <th>Category</th>
                <th>Engines</th>
                <th>Max Weight</th>
                <th>Cruise</th>
                <th>Approved</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="ac in aircraft" :key="ac.id">
                <td class="title-cell">{{ ac.title }}</td>
                <td>{{ ac.atcType || '-' }}</td>
                <td>{{ ac.atcModel || '-' }}</td>
                <td>{{ ac.category || '-' }}</td>
                <td>{{ ac.engineTypeStr }} ({{ ac.numberOfEngines }})</td>
                <td>{{ formatNumber(ac.maxGrossWeightLbs) }}</td>
                <td>{{ formatNumber(ac.cruiseSpeedKts) }}</td>
                <td>
                  <span class="approved-badge" :class="{ approved: ac.isApproved }">
                    {{ ac.isApproved ? 'Yes' : 'No' }}
                  </span>
                </td>
                <td>
                  <button
                    class="action-btn delete"
                    @click="deleteAircraft(ac)"
                    :disabled="deletingId === ac.id"
                  >
                    Delete
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, reactive } from 'vue'
import { api, type AircraftRequestResponse, type AircraftResponse } from '@/services/api'

const activeTab = ref<'requests' | 'aircraft'>('requests')
const statusFilter = ref('')
const loadingRequests = ref(false)
const loadingAircraft = ref(false)
const processingId = ref<string | null>(null)
const deletingId = ref<string | null>(null)

const requests = ref<AircraftRequestResponse[]>([])
const aircraft = ref<AircraftResponse[]>([])

// Expandable cards state
const expandedCards = reactive<Record<string, boolean>>({})
const cardTabs = reactive<Record<string, 'info' | 'manifest' | 'aircraft'>>({})

function toggleCard(id: string) {
  expandedCards[id] = !expandedCards[id]
  // Set default tab when expanding
  if (expandedCards[id] && !cardTabs[id]) {
    cardTabs[id] = 'info'
  }
}

function setCardTab(id: string, tab: 'info' | 'manifest' | 'aircraft') {
  cardTabs[id] = tab
}

const filteredRequests = computed(() => {
  if (!statusFilter.value) return requests.value
  return requests.value.filter(r => r.status === statusFilter.value)
})

const pendingCount = computed(() =>
  requests.value.filter(r => r.status === 'Pending').length
)

function formatNumber(value: number | undefined): string {
  if (value === undefined || value === null) return 'N/A'
  return value.toLocaleString('en-US', { maximumFractionDigits: 0 })
}

function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

async function loadRequests() {
  loadingRequests.value = true
  try {
    const response = await api.aircraftRequests.list()
    if (response.data) {
      requests.value = response.data
    }
  } catch (error) {
    console.error('Failed to load requests:', error)
  } finally {
    loadingRequests.value = false
  }
}

async function loadAircraft() {
  loadingAircraft.value = true
  try {
    const response = await api.aircraft.list(false)
    if (response.data) {
      aircraft.value = response.data
    }
  } catch (error) {
    console.error('Failed to load aircraft:', error)
  } finally {
    loadingAircraft.value = false
  }
}

async function approveRequest(request: AircraftRequestResponse) {
  processingId.value = request.id
  try {
    const response = await api.aircraftRequests.approve(request.id)
    if (response.data) {
      // Update the request in the list
      const idx = requests.value.findIndex(r => r.id === request.id)
      if (idx !== -1) {
        requests.value[idx] = response.data
      }
      // Reload aircraft list
      loadAircraft()
    }
  } catch (error) {
    console.error('Failed to approve request:', error)
  } finally {
    processingId.value = null
  }
}

async function rejectRequest(request: AircraftRequestResponse) {
  processingId.value = request.id
  try {
    const response = await api.aircraftRequests.reject(request.id, 'Rejected by admin')
    if (response.data) {
      const idx = requests.value.findIndex(r => r.id === request.id)
      if (idx !== -1) {
        requests.value[idx] = response.data
      }
    }
  } catch (error) {
    console.error('Failed to reject request:', error)
  } finally {
    processingId.value = null
  }
}

async function deleteAircraft(ac: AircraftResponse) {
  if (!confirm(`Are you sure you want to delete "${ac.title}"?`)) return

  deletingId.value = ac.id
  try {
    await api.aircraft.delete(ac.id)
    aircraft.value = aircraft.value.filter(a => a.id !== ac.id)
  } catch (error) {
    console.error('Failed to delete aircraft:', error)
  } finally {
    deletingId.value = null
  }
}

onMounted(() => {
  loadRequests()
  loadAircraft()
})
</script>

<style scoped>
.admin-view {
  min-height: 100%;
  padding: 32px 48px;
  overflow-y: auto;
}

.admin-header {
  margin-bottom: 24px;
}

.admin-title {
  font-size: 28px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 8px;
}

.admin-subtitle {
  font-size: 15px;
  color: var(--text-secondary);
}

.admin-tabs {
  display: flex;
  gap: 8px;
  margin-bottom: 24px;
  border-bottom: 1px solid var(--border-subtle);
  padding-bottom: 12px;
}

.tab-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 20px;
  background: transparent;
  border: none;
  border-radius: 8px;
  color: var(--text-secondary);
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
}

.tab-btn:hover {
  background: var(--bg-elevated);
  color: var(--text-primary);
}

.tab-btn.active {
  background: rgba(59, 130, 246, 0.1);
  color: var(--accent-primary);
}

.tab-btn .badge {
  background: #ef4444;
  color: white;
  font-size: 11px;
  font-weight: 600;
  padding: 2px 8px;
  border-radius: 10px;
}

.filter-bar {
  display: flex;
  gap: 12px;
  margin-bottom: 20px;
}

.status-filter {
  padding: 8px 12px;
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 8px;
  color: var(--text-primary);
  font-size: 14px;
}

.refresh-btn {
  padding: 8px 16px;
  background: var(--bg-elevated);
  border: 1px solid var(--border-subtle);
  border-radius: 8px;
  color: var(--text-primary);
  font-size: 14px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.refresh-btn:hover {
  background: var(--bg-secondary);
}

.refresh-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.loading-state,
.empty-state {
  padding: 60px 20px;
  text-align: center;
  color: var(--text-muted);
}

.requests-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(380px, 1fr));
  gap: 20px;
}

.request-card {
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 12px;
  padding: 20px;
}

.request-card.pending {
  border-left: 3px solid #f59e0b;
}

.request-card.approved {
  border-left: 3px solid #22c55e;
}

.request-card.rejected {
  border-left: 3px solid #ef4444;
}

.request-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  cursor: pointer;
  padding: 4px 0;
  transition: opacity 0.2s ease;
}

.request-header:hover {
  opacity: 0.8;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 12px;
  flex: 1;
}

.header-right {
  display: flex;
  align-items: center;
  gap: 10px;
}

.expand-icon {
  font-size: 10px;
  color: var(--text-muted);
  transition: transform 0.2s ease;
}

.request-card.expanded .expand-icon {
  color: var(--accent-primary);
}

.request-title {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-primary);
}

.file-data-badge {
  font-size: 10px;
  font-weight: 600;
  padding: 4px 8px;
  border-radius: 4px;
  background: rgba(59, 130, 246, 0.1);
  color: var(--accent-primary);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.status-badge {
  font-size: 11px;
  font-weight: 600;
  padding: 4px 10px;
  border-radius: 12px;
  text-transform: uppercase;
}

.status-badge.pending {
  background: rgba(245, 158, 11, 0.1);
  color: #f59e0b;
}

.status-badge.approved {
  background: rgba(34, 197, 94, 0.1);
  color: #22c55e;
}

.status-badge.rejected {
  background: rgba(239, 68, 68, 0.1);
  color: #ef4444;
}

.request-details {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 16px;
}

.detail-row {
  display: flex;
  justify-content: space-between;
  font-size: 13px;
}

.detail-row.notes {
  flex-direction: column;
  gap: 4px;
}

.detail-label {
  color: var(--text-muted);
}

.detail-value {
  color: var(--text-primary);
  font-weight: 500;
}

.request-actions {
  display: flex;
  gap: 12px;
  padding-top: 16px;
  border-top: 1px solid var(--border-subtle);
}

.aircraft-table-container {
  overflow-x: auto;
}

.aircraft-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 13px;
}

.aircraft-table th,
.aircraft-table td {
  padding: 12px 16px;
  text-align: left;
  border-bottom: 1px solid var(--border-subtle);
}

.aircraft-table th {
  background: var(--bg-elevated);
  color: var(--text-muted);
  font-weight: 600;
  text-transform: uppercase;
  font-size: 11px;
  letter-spacing: 0.5px;
}

.aircraft-table td {
  color: var(--text-primary);
}

.title-cell {
  max-width: 250px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.approved-badge {
  font-size: 11px;
  font-weight: 600;
  padding: 3px 8px;
  border-radius: 4px;
  background: rgba(239, 68, 68, 0.1);
  color: #ef4444;
}

.approved-badge.approved {
  background: rgba(34, 197, 94, 0.1);
  color: #22c55e;
}

.action-btn {
  padding: 6px 12px;
  background: transparent;
  border: 1px solid var(--border-subtle);
  border-radius: 6px;
  color: var(--text-secondary);
  font-size: 12px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.action-btn:hover {
  background: var(--bg-elevated);
}

.action-btn.delete:hover {
  background: rgba(239, 68, 68, 0.1);
  border-color: #ef4444;
  color: #ef4444;
}

.action-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* Expandable card content */
.request-expanded {
  margin-top: 16px;
  padding-top: 16px;
  border-top: 1px solid var(--border-subtle);
}

.inner-tabs {
  display: flex;
  gap: 4px;
  margin-bottom: 16px;
  padding-bottom: 12px;
  border-bottom: 1px solid var(--border-subtle);
}

.inner-tab {
  padding: 8px 16px;
  background: transparent;
  border: none;
  border-radius: 6px;
  color: var(--text-muted);
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
}

.inner-tab:hover:not(:disabled) {
  background: var(--bg-elevated);
  color: var(--text-primary);
}

.inner-tab.active {
  background: rgba(59, 130, 246, 0.1);
  color: var(--accent-primary);
}

.inner-tab.disabled,
.inner-tab:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.tab-panel {
  animation: fadeIn 0.2s ease;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

.file-data {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.file-data-section {
  background: var(--bg-elevated);
  border-radius: 8px;
  padding: 16px;
}

.section-header {
  font-size: 12px;
  font-weight: 600;
  color: var(--accent-primary);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 12px;
  padding-bottom: 8px;
  border-bottom: 1px solid var(--border-subtle);
}

.detail-value.mono {
  font-family: 'Space Mono', monospace;
  font-size: 12px;
}

.no-data {
  padding: 32px;
  text-align: center;
  color: var(--text-muted);
  font-size: 14px;
}
</style>
