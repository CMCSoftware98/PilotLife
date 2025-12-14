<template>
  <div class="dev-view">
    <div class="dev-header">
      <div class="dev-header-content">
        <h1 class="dev-title">Developer Console</h1>
        <p class="dev-subtitle">Real-time connector and flight data monitoring</p>
      </div>
      <div class="dev-header-actions">
        <label class="admin-toggle">
          <input
            type="checkbox"
            :checked="settingsStore.settings.value.adminMode"
            @change="toggleAdminMode"
          >
          <span class="toggle-slider"></span>
          <span class="toggle-label">Admin Mode</span>
        </label>
      </div>
    </div>

    <div class="dev-content">
      <!-- Connection Status Section -->
      <div class="dev-section">
        <h2 class="section-title">Connection Status</h2>
        <div class="dev-card">
          <div class="status-grid">
            <div class="status-item">
              <span class="status-label">Connector Live</span>
              <span class="status-indicator" :class="{ connected: connectorLive }"></span>
            </div>
            <div class="status-item">
              <span class="status-label">Sim Connected</span>
              <span class="status-indicator" :class="{ connected: simConnected }"></span>
            </div>
            <div class="status-item">
              <span class="status-label">WebSocket Port</span>
              <span class="status-value">{{ port || 'N/A' }}</span>
            </div>
            <div class="status-item">
              <span class="status-label">Simulator Version</span>
              <span class="status-value">{{ status?.simulatorVersion || 'N/A' }}</span>
            </div>
            <div class="status-item" v-if="status?.connectionError">
              <span class="status-label">Error</span>
              <span class="status-value error">{{ status.connectionError }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Flight Data Section -->
      <div class="dev-section">
        <h2 class="section-title">Flight Data</h2>
        <div class="dev-card" v-if="flightData">
          <div class="data-grid">
            <!-- Aircraft Metadata -->
            <div class="data-group aircraft-metadata">
              <h3 class="group-title">Aircraft Metadata</h3>
              <div class="data-item">
                <span class="data-label">Title</span>
                <span class="data-value">{{ flightData.aircraftTitle || 'N/A' }}</span>
              </div>
              <div class="data-item">
                <span class="data-label">ATC Type</span>
                <span class="data-value">{{ flightData.atcType || 'N/A' }}</span>
              </div>
              <div class="data-item">
                <span class="data-label">ATC Model</span>
                <span class="data-value mono">{{ flightData.atcModel || 'N/A' }}</span>
              </div>
              <div class="data-item">
                <span class="data-label">Tail Number</span>
                <span class="data-value mono">{{ flightData.atcId || 'N/A' }}</span>
              </div>
              <div class="data-item">
                <span class="data-label">Airline</span>
                <span class="data-value">{{ flightData.atcAirline || 'N/A' }}</span>
              </div>
              <div class="data-item">
                <span class="data-label">Flight Number</span>
                <span class="data-value mono">{{ flightData.atcFlightNumber || 'N/A' }}</span>
              </div>
              <div class="data-item">
                <span class="data-label">Category</span>
                <span class="data-value">{{ flightData.category || 'N/A' }}</span>
              </div>
              <div class="data-item">
                <span class="data-label">Engine Type</span>
                <span class="data-value">{{ flightData.engineTypeStr || 'N/A' }} ({{ flightData.numberOfEngines || 0 }})</span>
              </div>
              <div class="data-item">
                <span class="data-label">Max Gross Weight</span>
                <span class="data-value mono">{{ formatNumber(flightData.maxGrossWeightLbs, 0) }} lbs</span>
              </div>
              <div class="data-item">
                <span class="data-label">Empty Weight</span>
                <span class="data-value mono">{{ formatNumber(flightData.emptyWeightLbs, 0) }} lbs</span>
              </div>
              <div class="data-item">
                <span class="data-label">Cruise Speed</span>
                <span class="data-value mono">{{ formatNumber(flightData.cruiseSpeedKts, 0) }} kts</span>
              </div>
              <div class="data-item">
                <span class="data-label">Timestamp</span>
                <span class="data-value mono">{{ flightData.timestamp || 'N/A' }}</span>
              </div>

              <v-btn
                color="primary"
                variant="flat"
                class="request-btn"
                :disabled="!flightData || requestingAircraft"
                :loading="requestingAircraft"
                @click="requestAircraft"
              >
                Request to Add Aircraft to Sim
              </v-btn>
              <div v-if="requestMessage" class="request-message" :class="requestMessage.type">
                {{ requestMessage.text }}
              </div>
            </div>

            <!-- Position -->
            <div class="data-group">
              <h3 class="group-title">Position</h3>
              <div class="data-item">
                <span class="data-label">Latitude</span>
                <span class="data-value mono">{{ formatNumber(flightData.latitude, 6) }}</span>
              </div>
              <div class="data-item">
                <span class="data-label">Longitude</span>
                <span class="data-value mono">{{ formatNumber(flightData.longitude, 6) }}</span>
              </div>
            </div>

            <!-- Altitude -->
            <div class="data-group">
              <h3 class="group-title">Altitude</h3>
              <div class="data-item">
                <span class="data-label">Indicated</span>
                <span class="data-value mono">{{ formatNumber(flightData.altitudeIndicated, 0) }} ft</span>
              </div>
              <div class="data-item">
                <span class="data-label">True</span>
                <span class="data-value mono">{{ formatNumber(flightData.altitudeTrue, 0) }} ft</span>
              </div>
              <div class="data-item">
                <span class="data-label">AGL</span>
                <span class="data-value mono">{{ formatNumber(flightData.altitudeAGL, 0) }} ft</span>
              </div>
            </div>

            <!-- Speed -->
            <div class="data-group">
              <h3 class="group-title">Speed</h3>
              <div class="data-item">
                <span class="data-label">IAS</span>
                <span class="data-value mono">{{ formatNumber(flightData.airspeedIndicated, 0) }} kts</span>
              </div>
              <div class="data-item">
                <span class="data-label">TAS</span>
                <span class="data-value mono">{{ formatNumber(flightData.airspeedTrue, 0) }} kts</span>
              </div>
              <div class="data-item">
                <span class="data-label">Ground Speed</span>
                <span class="data-value mono">{{ formatNumber(flightData.groundSpeed, 0) }} kts</span>
              </div>
              <div class="data-item">
                <span class="data-label">Mach</span>
                <span class="data-value mono">{{ formatNumber(flightData.machNumber, 3) }}</span>
              </div>
            </div>

            <!-- Heading -->
            <div class="data-group">
              <h3 class="group-title">Heading</h3>
              <div class="data-item">
                <span class="data-label">Magnetic</span>
                <span class="data-value mono">{{ formatNumber(flightData.headingMagnetic, 0) }}°</span>
              </div>
              <div class="data-item">
                <span class="data-label">True</span>
                <span class="data-value mono">{{ formatNumber(flightData.headingTrue, 0) }}°</span>
              </div>
              <div class="data-item">
                <span class="data-label">Track</span>
                <span class="data-value mono">{{ formatNumber(flightData.track, 0) }}°</span>
              </div>
            </div>

            <!-- Weight & Fuel -->
            <div class="data-group">
              <h3 class="group-title">Weight & Fuel</h3>
              <div class="data-item">
                <span class="data-label">Fuel</span>
                <span class="data-value mono">{{ formatNumber(flightData.fuelLbs, 0) }} lbs / {{ formatNumber(flightData.fuelKgs, 0) }} kg</span>
              </div>
              <div class="data-item">
                <span class="data-label">Payload</span>
                <span class="data-value mono">{{ formatNumber(flightData.payloadLbs, 0) }} lbs / {{ formatNumber(flightData.payloadKgs, 0) }} kg</span>
              </div>
              <div class="data-item">
                <span class="data-label">Total Weight</span>
                <span class="data-value mono">{{ formatNumber(flightData.totalWeightLbs, 0) }} lbs / {{ formatNumber(flightData.totalWeightKgs, 0) }} kg</span>
              </div>
            </div>

            <!-- Radio -->
            <div class="data-group">
              <h3 class="group-title">Radio Frequencies</h3>
              <div class="data-item">
                <span class="data-label">COM1</span>
                <span class="data-value mono">{{ flightData.com1Frequency || 'N/A' }}</span>
              </div>
              <div class="data-item">
                <span class="data-label">COM2</span>
                <span class="data-value mono">{{ flightData.com2Frequency || 'N/A' }}</span>
              </div>
              <div class="data-item">
                <span class="data-label">NAV1</span>
                <span class="data-value mono">{{ flightData.nav1Frequency || 'N/A' }}</span>
              </div>
              <div class="data-item">
                <span class="data-label">NAV2</span>
                <span class="data-value mono">{{ flightData.nav2Frequency || 'N/A' }}</span>
              </div>
            </div>
          </div>
        </div>
        <div class="dev-card empty" v-else>
          <p>No flight data received yet. Connect to the simulator to see live data.</p>
        </div>
      </div>

      <!-- Raw Data Section -->
      <div class="dev-section">
        <h2 class="section-title">Raw JSON Data</h2>
        <div class="dev-card code">
          <pre>{{ rawData }}</pre>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { connector, type FlightData, type SimulatorStatus } from '@/services/connector'
import { useSettingsStore } from '@/stores/settings'
import { api } from '@/services/api'

const settingsStore = useSettingsStore()

const connectorLive = ref(false)
const simConnected = ref(false)
const port = ref<number | null>(null)
const status = ref<SimulatorStatus | null>(null)
const flightData = ref<FlightData | null>(null)
const requestingAircraft = ref(false)
const requestMessage = ref<{ type: 'success' | 'error', text: string } | null>(null)

let unsubscribeConnection: (() => void) | null = null
let unsubscribeStatus: (() => void) | null = null
let unsubscribeFlightData: (() => void) | null = null

const rawData = computed(() => {
  return JSON.stringify({
    connection: {
      connectorLive: connectorLive.value,
      port: port.value
    },
    status: status.value,
    flightData: flightData.value
  }, null, 2)
})

function formatNumber(value: number | undefined, decimals: number): string {
  if (value === undefined || value === null || isNaN(value)) return 'N/A'
  return value.toFixed(decimals)
}

function toggleAdminMode(event: Event) {
  const target = event.target as HTMLInputElement
  settingsStore.setAdminMode(target.checked)
}

async function requestAircraft() {
  if (!flightData.value) return

  requestingAircraft.value = true
  requestMessage.value = null

  try {
    await api.aircraftRequests.create({
      aircraftTitle: flightData.value.aircraftTitle,
      atcType: flightData.value.atcType,
      atcModel: flightData.value.atcModel,
      category: flightData.value.category,
      engineType: flightData.value.engineType,
      engineTypeStr: flightData.value.engineTypeStr,
      numberOfEngines: flightData.value.numberOfEngines,
      maxGrossWeightLbs: flightData.value.maxGrossWeightLbs,
      emptyWeightLbs: flightData.value.emptyWeightLbs,
      cruiseSpeedKts: flightData.value.cruiseSpeedKts,
      simulatorVersion: flightData.value.simulatorVersion
    })

    requestMessage.value = {
      type: 'success',
      text: 'Aircraft request submitted successfully!'
    }
  } catch (error: unknown) {
    const errorMessage = error instanceof Error ? error.message : 'Failed to submit aircraft request'
    requestMessage.value = {
      type: 'error',
      text: errorMessage
    }
  } finally {
    requestingAircraft.value = false

    // Clear message after 5 seconds
    setTimeout(() => {
      requestMessage.value = null
    }, 5000)
  }
}

onMounted(() => {
  port.value = connector.getPort()

  unsubscribeConnection = connector.onConnection((connected: boolean) => {
    connectorLive.value = connected
  })

  unsubscribeStatus = connector.onStatus((newStatus: SimulatorStatus) => {
    status.value = newStatus
    simConnected.value = newStatus.isConnected && newStatus.isSimRunning
  })

  unsubscribeFlightData = connector.onFlightData((data: FlightData) => {
    flightData.value = data
  })

  // Set initial state
  connectorLive.value = connector.isRunning()
})

onUnmounted(() => {
  if (unsubscribeConnection) unsubscribeConnection()
  if (unsubscribeStatus) unsubscribeStatus()
  if (unsubscribeFlightData) unsubscribeFlightData()
})
</script>

<style scoped>
.dev-view {
  min-height: 100%;
  padding: 32px 48px;
  overflow-y: auto;
}

.dev-header {
  margin-bottom: 32px;
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
}

.dev-header-content {
  flex: 1;
}

.dev-header-actions {
  display: flex;
  align-items: center;
  gap: 16px;
}

.admin-toggle {
  display: flex;
  align-items: center;
  gap: 12px;
  cursor: pointer;
}

.admin-toggle input {
  opacity: 0;
  width: 0;
  height: 0;
  position: absolute;
}

.admin-toggle .toggle-slider {
  position: relative;
  width: 44px;
  height: 24px;
  background-color: var(--bg-elevated);
  border: 1px solid var(--border-subtle);
  border-radius: 24px;
  transition: 0.3s;
}

.admin-toggle .toggle-slider:before {
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

.admin-toggle input:checked + .toggle-slider {
  background: linear-gradient(135deg, #f59e0b, #ef4444);
  border-color: transparent;
}

.admin-toggle input:checked + .toggle-slider:before {
  transform: translateX(20px);
  background-color: white;
}

.admin-toggle .toggle-label {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-secondary);
}

.dev-title {
  font-size: 28px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 8px;
}

.dev-subtitle {
  font-size: 15px;
  color: var(--text-secondary);
}

.dev-content {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.dev-section {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.section-title {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.dev-card {
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 12px;
  padding: 20px;
}

.dev-card.empty {
  color: var(--text-muted);
  text-align: center;
  padding: 40px;
}

.dev-card.code {
  background: var(--bg-primary);
  font-family: 'Space Mono', monospace;
  font-size: 12px;
  overflow-x: auto;
}

.dev-card.code pre {
  margin: 0;
  color: var(--text-secondary);
  white-space: pre-wrap;
  word-break: break-all;
}

.status-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
}

.status-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  background: var(--bg-elevated);
  border-radius: 8px;
}

.status-label {
  font-size: 13px;
  font-weight: 500;
  color: var(--text-secondary);
}

.status-indicator {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  background: #ef4444;
  box-shadow: 0 0 8px rgba(239, 68, 68, 0.6);
  transition: all 0.3s ease;
}

.status-indicator.connected {
  background: #22c55e;
  box-shadow: 0 0 8px rgba(34, 197, 94, 0.6);
}

.status-value {
  font-size: 13px;
  font-weight: 600;
  color: var(--text-primary);
}

.status-value.error {
  color: #ef4444;
}

.data-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 24px;
}

.data-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.group-title {
  font-size: 12px;
  font-weight: 600;
  color: var(--accent-primary);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 4px;
  padding-bottom: 8px;
  border-bottom: 1px solid var(--border-subtle);
}

.data-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 6px 0;
}

.data-label {
  font-size: 13px;
  color: var(--text-muted);
}

.data-value {
  font-size: 13px;
  font-weight: 500;
  color: var(--text-primary);
}

.data-value.mono {
  font-family: 'Space Mono', monospace;
}

.aircraft-metadata {
  grid-column: span 2;
}

.request-btn {
  margin-top: 16px;
  width: 100%;
}

.request-message {
  margin-top: 12px;
  padding: 12px 16px;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 500;
}

.request-message.success {
  background: rgba(34, 197, 94, 0.1);
  color: #22c55e;
  border: 1px solid rgba(34, 197, 94, 0.3);
}

.request-message.error {
  background: rgba(239, 68, 68, 0.1);
  color: #ef4444;
  border: 1px solid rgba(239, 68, 68, 0.3);
}
</style>
