<template>
  <div class="concept-view">
    <div class="controls-panel">
      <h2 class="panel-title">3D Flight Simulator</h2>
      <p class="panel-subtitle">Experience flights in stunning 3D with Streets.GL</p>

      <div class="input-group">
        <label>Departure Airport</label>
        <div class="airport-input-wrapper">
          <input
            v-model="departureQuery"
            type="text"
            placeholder="Search ICAO or name..."
            class="airport-input"
            @input="searchDeparture"
            @focus="showDepartureResults = true"
          />
          <div v-if="departureAirport" class="selected-badge">
            {{ departureAirport.ident }}
          </div>
        </div>
        <div v-if="showDepartureResults && departureResults.length > 0" class="search-results">
          <div
            v-for="airport in departureResults"
            :key="airport.id"
            class="result-item"
            @click="selectDeparture(airport)"
          >
            <span class="result-code">{{ airport.ident }}</span>
            <span class="result-name">{{ airport.name }}</span>
          </div>
        </div>
      </div>

      <div class="input-group">
        <label>Arrival Airport</label>
        <div class="airport-input-wrapper">
          <input
            v-model="arrivalQuery"
            type="text"
            placeholder="Search ICAO or name..."
            class="airport-input"
            @input="searchArrival"
            @focus="showArrivalResults = true"
          />
          <div v-if="arrivalAirport" class="selected-badge">
            {{ arrivalAirport.ident }}
          </div>
        </div>
        <div v-if="showArrivalResults && arrivalResults.length > 0" class="search-results">
          <div
            v-for="airport in arrivalResults"
            :key="airport.id"
            class="result-item"
            @click="selectArrival(airport)"
          >
            <span class="result-code">{{ airport.ident }}</span>
            <span class="result-name">{{ airport.name }}</span>
          </div>
        </div>
      </div>

      <div class="flight-info" v-if="departureAirport && arrivalAirport">
        <div class="info-row">
          <span class="info-label">Distance</span>
          <span class="info-value">{{ flightDistance }} nm</span>
        </div>
        <div class="info-row">
          <span class="info-label">Route</span>
          <span class="info-value">{{ departureAirport.ident }} → {{ arrivalAirport.ident }}</span>
        </div>
        <div class="info-row">
          <span class="info-label">Est. Time</span>
          <span class="info-value">{{ estimatedFlightTime }}</span>
        </div>
      </div>

      <v-btn
        color="primary"
        size="large"
        block
        class="start-btn"
        :disabled="!departureAirport || !arrivalAirport || isFlying || !streetsGlReady"
        :loading="isFlying"
        @click="startFlight"
      >
        {{ !streetsGlReady ? 'Loading 3D Map...' : isFlying ? 'Flying...' : 'Start Flight' }}
      </v-btn>

      <div v-if="isFlying" class="flight-status">
        <div class="status-header">
          <span class="status-phase">{{ flightPhase }}</span>
          <span class="status-progress">{{ Math.round(flightProgress * 100) }}%</span>
        </div>
        <div class="progress-bar">
          <div class="progress-fill" :style="{ width: `${flightProgress * 100}%` }"></div>
        </div>
        <div class="flight-data">
          <div class="data-item">
            <span class="data-label">Altitude</span>
            <span class="data-value">{{ currentAltitude.toLocaleString() }} ft</span>
          </div>
          <div class="data-item">
            <span class="data-label">Speed</span>
            <span class="data-value">{{ currentSpeed }} kts</span>
          </div>
          <div class="data-item">
            <span class="data-label">Heading</span>
            <span class="data-value">{{ currentHeading }}°</span>
          </div>
        </div>
      </div>

      <v-btn
        v-if="isFlying"
        color="error"
        variant="outlined"
        size="small"
        block
        class="stop-btn"
        @click="stopFlight"
      >
        Stop Flight
      </v-btn>

      <div class="map-mode-toggle">
        <span class="toggle-label">Map View</span>
        <div class="toggle-buttons">
          <button
            class="toggle-btn"
            :class="{ active: mapMode === '2d' }"
            @click="setMapMode('2d')"
          >
            2D
          </button>
          <button
            class="toggle-btn"
            :class="{ active: mapMode === '3d' }"
            @click="setMapMode('3d')"
          >
            3D
          </button>
        </div>
      </div>

      <div class="map-status" :class="{ ready: streetsGlReady }">
        <div class="status-dot"></div>
        <span>{{ streetsGlReady ? 'Streets.GL Ready' : 'Loading Streets.GL...' }}</span>
      </div>
    </div>

    <div class="map-container">
      <iframe
        ref="streetsGlFrame"
        src="/streets-gl/index.html"
        class="streets-gl-iframe"
        allow="accelerometer; autoplay; encrypted-media; gyroscope"
        @load="onIframeLoad"
      ></iframe>
      <div v-if="!streetsGlReady" class="loading-overlay">
        <div class="loading-content">
          <div class="loading-icon">
            <svg viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 2L2 7L12 12L22 7L12 2Z" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
              <path d="M2 17L12 22L22 17" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
              <path d="M2 12L12 17L22 12" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
            </svg>
          </div>
          <h3 class="loading-title">Loading Streets.GL</h3>
          <div class="loading-progress-container">
            <div class="loading-progress-bar">
              <div class="loading-progress-fill" :style="{ width: `${loadingProgress}%` }"></div>
            </div>
            <span class="loading-progress-text">{{ loadingProgress }}%</span>
          </div>
          <p class="loading-file">{{ loadingFileName || 'Initializing...' }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'
import * as turf from '@turf/turf'
import { api, type Airport } from '../services/api'

const streetsGlFrame = ref<HTMLIFrameElement | null>(null)
const streetsGlReady = ref(false)
const loadingProgress = ref(0)
const loadingFileName = ref('')
const mapMode = ref<'2d' | '3d'>('3d')

// Airport search state
const departureQuery = ref('')
const arrivalQuery = ref('')
const departureResults = ref<Airport[]>([])
const arrivalResults = ref<Airport[]>([])
const departureAirport = ref<Airport | null>(null)
const arrivalAirport = ref<Airport | null>(null)
const showDepartureResults = ref(false)
const showArrivalResults = ref(false)

// Flight state
const isFlying = ref(false)
const flightProgress = ref(0)
const flightPhase = ref('')
const currentAltitude = ref(0)
const currentSpeed = ref(0)
const currentHeading = ref(0)
let animationFrame: number | null = null
let flightStartTime: number | null = null

// Search timeouts
let departureTimeout: ReturnType<typeof setTimeout> | null = null
let arrivalTimeout: ReturnType<typeof setTimeout> | null = null

// Flight waypoint interface
interface FlightWaypoint {
  lng: number
  lat: number
  altitude: number
  speed: number
  heading: number
  phase: string
}

const flightDistance = computed(() => {
  if (!departureAirport.value || !arrivalAirport.value) return 0
  const from = turf.point([departureAirport.value.longitude, departureAirport.value.latitude])
  const to = turf.point([arrivalAirport.value.longitude, arrivalAirport.value.latitude])
  const distance = turf.distance(from, to, { units: 'nauticalmiles' })
  return Math.round(distance)
})

const estimatedFlightTime = computed(() => {
  if (!flightDistance.value) return '--'
  // Average speed of 450 knots
  const hours = flightDistance.value / 450
  const h = Math.floor(hours)
  const m = Math.round((hours - h) * 60)
  if (h === 0) return `${m}m`
  return `${h}h ${m}m`
})

// Cruising altitude based on distance
function getCruisingAltitude(distanceNm: number): number {
  if (distanceNm < 100) return 15000
  if (distanceNm < 300) return 25000
  if (distanceNm < 500) return 32000
  if (distanceNm < 1000) return 36000
  return 39000
}

// Generate realistic flight waypoints
function generateFlightPlan(
  departure: Airport,
  arrival: Airport,
  distanceNm: number
): FlightWaypoint[] {
  const waypoints: FlightWaypoint[] = []
  const cruiseAlt = getCruisingAltitude(distanceNm)

  // Create great circle route with many points
  const start = [departure.longitude, departure.latitude]
  const end = [arrival.longitude, arrival.latitude]
  const route = turf.greatCircle(turf.point(start), turf.point(end), { npoints: 200 })
  const coords = route.geometry.coordinates as [number, number][]

  const totalPoints = coords.length

  // Define phase boundaries
  const phases = {
    taxi: 0.01,
    takeoff: 0.03,
    climb: 0.15,
    cruise: 0.75,
    descent: 0.92,
    approach: 0.97,
    landing: 1.0
  }

  for (let i = 0; i < totalPoints; i++) {
    const progress = i / (totalPoints - 1)
    const coord = coords[i]
    const nextCoord = coords[Math.min(i + 1, totalPoints - 1)]

    // Calculate heading
    const heading = turf.bearing(turf.point(coord), turf.point(nextCoord))

    let altitude: number
    let speed: number
    let phase: string

    if (progress <= phases.taxi) {
      phase = 'Taxiing'
      altitude = departure.elevationFt || 0
      speed = 20
    } else if (progress <= phases.takeoff) {
      phase = 'Taking Off'
      const takeoffProgress = (progress - phases.taxi) / (phases.takeoff - phases.taxi)
      altitude = (departure.elevationFt || 0) + takeoffProgress * 3000
      speed = 150 + takeoffProgress * 50
    } else if (progress <= phases.climb) {
      phase = 'Climbing'
      const climbProgress = (progress - phases.takeoff) / (phases.climb - phases.takeoff)
      const startAlt = (departure.elevationFt || 0) + 3000
      altitude = startAlt + climbProgress * (cruiseAlt - startAlt)
      speed = 200 + climbProgress * 280
    } else if (progress <= phases.cruise) {
      phase = 'Cruising'
      altitude = cruiseAlt
      speed = 480
    } else if (progress <= phases.descent) {
      phase = 'Descending'
      const descentProgress = (progress - phases.cruise) / (phases.descent - phases.cruise)
      const targetAlt = 3000 + (arrival.elevationFt || 0)
      altitude = cruiseAlt - descentProgress * (cruiseAlt - targetAlt)
      speed = 480 - descentProgress * 200
    } else if (progress <= phases.approach) {
      phase = 'Approach'
      const approachProgress = (progress - phases.descent) / (phases.approach - phases.descent)
      const startAlt = 3000 + (arrival.elevationFt || 0)
      altitude = startAlt - approachProgress * 2500
      speed = 280 - approachProgress * 130
    } else {
      phase = 'Landing'
      const landingProgress = (progress - phases.approach) / (phases.landing - phases.approach)
      altitude = Math.max((arrival.elevationFt || 0), 500 - landingProgress * 500 + (arrival.elevationFt || 0))
      speed = Math.max(20, 150 - landingProgress * 130)
    }

    waypoints.push({
      lng: coord[0],
      lat: coord[1],
      altitude: Math.round(altitude),
      speed: Math.round(speed),
      heading: Math.round((heading + 360) % 360),
      phase
    })
  }

  return waypoints
}

// Send message to streets-gl iframe
function sendToStreetsGl(message: object): void {
  if (streetsGlFrame.value?.contentWindow) {
    streetsGlFrame.value.contentWindow.postMessage(message, '*')
  }
}

// Handle messages from streets-gl
function handleStreetsGlMessage(event: MessageEvent): void {
  const data = event.data
  if (!data || !data.type) return

  console.log('Streets.GL message:', data.type, data)

  switch (data.type) {
    case 'ready':
      streetsGlReady.value = true
      loadingProgress.value = 100
      break
    case 'pong':
      streetsGlReady.value = data.ready
      break
    case 'loadingProgress':
      loadingProgress.value = Math.round(data.progress * 100)
      break
    case 'loadingFile':
      loadingFileName.value = data.fileName
      break
    case 'mapModeChanged':
      mapMode.value = data.mode
      break
  }
}

function onIframeLoad(): void {
  // Ping streets-gl to check if ready
  setTimeout(() => {
    sendToStreetsGl({ type: 'ping' })
  }, 1000)
}

function setMapMode(mode: '2d' | '3d'): void {
  mapMode.value = mode
  sendToStreetsGl({ type: 'setMapMode', mode })
}

async function searchDeparture() {
  if (departureTimeout) clearTimeout(departureTimeout)
  if (departureQuery.value.length < 2) {
    departureResults.value = []
    return
  }
  departureTimeout = setTimeout(async () => {
    const response = await api.airports.search(departureQuery.value, 10)
    if (response.data) {
      departureResults.value = response.data
    }
  }, 300)
}

async function searchArrival() {
  if (arrivalTimeout) clearTimeout(arrivalTimeout)
  if (arrivalQuery.value.length < 2) {
    arrivalResults.value = []
    return
  }
  arrivalTimeout = setTimeout(async () => {
    const response = await api.airports.search(arrivalQuery.value, 10)
    if (response.data) {
      arrivalResults.value = response.data
    }
  }, 300)
}

function selectDeparture(airport: Airport) {
  departureAirport.value = airport
  departureQuery.value = `${airport.ident} - ${airport.name}`
  departureResults.value = []
  showDepartureResults.value = false

  // If we already have an arrival airport, show the route
  if (arrivalAirport.value) {
    showFlightRoute()
  } else {
    // Just move camera to departure airport
    sendToStreetsGl({
      type: 'flyTo',
      lat: airport.latitude,
      lng: airport.longitude,
      distance: 2000
    })
  }
}

function selectArrival(airport: Airport) {
  arrivalAirport.value = airport
  arrivalQuery.value = `${airport.ident} - ${airport.name}`
  arrivalResults.value = []
  showArrivalResults.value = false

  // If we have both airports, draw the route
  if (departureAirport.value) {
    showFlightRoute()
  }
}

function showFlightRoute() {
  if (!departureAirport.value || !arrivalAirport.value) return

  // Send route to streets-gl to draw the curve and zoom out
  sendToStreetsGl({
    type: 'setRoute',
    departureLat: departureAirport.value.latitude,
    departureLng: departureAirport.value.longitude,
    arrivalLat: arrivalAirport.value.latitude,
    arrivalLng: arrivalAirport.value.longitude
  })
}

function interpolateWaypoint(wp1: FlightWaypoint, wp2: FlightWaypoint, t: number): FlightWaypoint {
  return {
    lng: wp1.lng + (wp2.lng - wp1.lng) * t,
    lat: wp1.lat + (wp2.lat - wp1.lat) * t,
    altitude: Math.round(wp1.altitude + (wp2.altitude - wp1.altitude) * t),
    speed: Math.round(wp1.speed + (wp2.speed - wp1.speed) * t),
    heading: Math.round(wp1.heading + (wp2.heading - wp1.heading) * t),
    phase: t < 0.5 ? wp1.phase : wp2.phase
  }
}

function startFlight() {
  if (!departureAirport.value || !arrivalAirport.value) return

  isFlying.value = true
  flightProgress.value = 0
  flightPhase.value = 'Preparing'
  currentAltitude.value = departureAirport.value.elevationFt || 0
  currentSpeed.value = 0
  currentHeading.value = 0
  flightStartTime = Date.now()

  // Generate flight plan
  const flightPlan = generateFlightPlan(
    departureAirport.value,
    arrivalAirport.value,
    flightDistance.value
  )

  // Send flight path to streets-gl
  sendToStreetsGl({
    type: 'setFlightPath',
    flightPath: {
      waypoints: flightPlan,
      departureIcao: departureAirport.value.ident,
      arrivalIcao: arrivalAirport.value.ident
    }
  })

  // Flight duration - 200ms per nautical mile
  const flightDurationMs = Math.max(flightDistance.value * 200, 30000)

  function animate() {
    if (!isFlying.value) return

    const elapsed = Date.now() - (flightStartTime || 0)
    const progress = Math.min(elapsed / flightDurationMs, 1)
    flightProgress.value = progress

    // Calculate current waypoint
    const exactIndex = progress * (flightPlan.length - 1)
    const index = Math.floor(exactIndex)
    const indexFraction = exactIndex - index

    const currentWp = flightPlan[Math.min(index, flightPlan.length - 1)]
    const nextWp = flightPlan[Math.min(index + 1, flightPlan.length - 1)]
    const interpolated = interpolateWaypoint(currentWp, nextWp, indexFraction)

    // Update flight data
    flightPhase.value = interpolated.phase
    currentAltitude.value = interpolated.altitude
    currentSpeed.value = interpolated.speed
    currentHeading.value = interpolated.heading

    // Send position update to streets-gl
    sendToStreetsGl({
      type: 'updateAircraftPosition',
      waypoint: interpolated
    })

    if (progress < 1) {
      animationFrame = requestAnimationFrame(animate)
    } else {
      flightPhase.value = 'Arrived'
      currentSpeed.value = 0
      setTimeout(() => {
        isFlying.value = false
        sendToStreetsGl({ type: 'clearFlight' })
      }, 3000)
    }
  }

  // Start animation after a short delay
  setTimeout(() => {
    animate()
  }, 1000)
}

function stopFlight() {
  isFlying.value = false
  if (animationFrame) {
    cancelAnimationFrame(animationFrame)
    animationFrame = null
  }
  flightProgress.value = 0
  flightPhase.value = ''
  currentAltitude.value = 0
  currentSpeed.value = 0
  currentHeading.value = 0
  sendToStreetsGl({ type: 'clearFlight' })
}

function handleClickOutside(event: MouseEvent) {
  const target = event.target as HTMLElement
  if (!target.closest('.input-group')) {
    showDepartureResults.value = false
    showArrivalResults.value = false
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
  window.addEventListener('message', handleStreetsGlMessage)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
  window.removeEventListener('message', handleStreetsGlMessage)
  if (animationFrame) {
    cancelAnimationFrame(animationFrame)
  }
})
</script>

<style scoped>
.concept-view {
  display: flex;
  height: 100%;
  background: var(--bg-primary);
}

.controls-panel {
  width: 360px;
  background: var(--bg-secondary);
  border-right: 1px solid var(--border-subtle);
  padding: 24px;
  display: flex;
  flex-direction: column;
  overflow-y: auto;
}

.panel-title {
  font-size: 20px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 4px;
}

.panel-subtitle {
  font-size: 13px;
  color: var(--text-secondary);
  margin-bottom: 24px;
}

.input-group {
  margin-bottom: 16px;
  position: relative;
}

.input-group label {
  display: block;
  font-size: 12px;
  font-weight: 600;
  color: var(--text-secondary);
  margin-bottom: 8px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.airport-input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.airport-input {
  width: 100%;
  padding: 12px 16px;
  background: var(--bg-elevated);
  border: 2px solid transparent;
  border-radius: 10px;
  font-size: 14px;
  color: var(--text-primary);
  outline: none;
  transition: all 0.2s ease;
}

.airport-input::placeholder {
  color: var(--text-muted);
}

.airport-input:focus {
  border-color: var(--accent-primary);
  background: var(--bg-secondary);
}

.selected-badge {
  position: absolute;
  right: 12px;
  background: var(--accent-primary);
  color: white;
  font-size: 11px;
  font-weight: 700;
  padding: 4px 8px;
  border-radius: 4px;
  font-family: var(--font-mono);
}

.search-results {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  background: var(--bg-card);
  border: 1px solid var(--border-subtle);
  border-radius: 10px;
  margin-top: 4px;
  max-height: 200px;
  overflow-y: auto;
  z-index: 100;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
}

.result-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 10px 14px;
  cursor: pointer;
  transition: background 0.15s ease;
}

.result-item:hover {
  background: var(--bg-elevated);
}

.result-code {
  font-family: var(--font-mono);
  font-size: 13px;
  font-weight: 700;
  color: var(--accent-primary);
  min-width: 50px;
}

.result-name {
  font-size: 13px;
  color: var(--text-primary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.flight-info {
  background: var(--bg-elevated);
  border-radius: 10px;
  padding: 16px;
  margin-bottom: 16px;
}

.info-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 6px 0;
}

.info-label {
  font-size: 12px;
  color: var(--text-muted);
}

.info-value {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary);
  font-family: var(--font-mono);
}

.start-btn {
  text-transform: none;
  font-weight: 600;
  margin-bottom: 16px;
}

.flight-status {
  background: var(--bg-elevated);
  border-radius: 10px;
  padding: 16px;
  margin-bottom: 12px;
}

.status-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.status-phase {
  font-size: 14px;
  font-weight: 600;
  color: var(--accent-primary);
}

.status-progress {
  font-size: 13px;
  font-weight: 600;
  color: var(--text-secondary);
  font-family: var(--font-mono);
}

.progress-bar {
  height: 6px;
  background: var(--bg-secondary);
  border-radius: 3px;
  overflow: hidden;
  margin-bottom: 12px;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(90deg, var(--accent-primary), var(--accent-secondary));
  border-radius: 3px;
  transition: width 0.1s linear;
}

.flight-data {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 12px;
}

.data-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
}

.data-label {
  font-size: 10px;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 4px;
}

.data-value {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary);
  font-family: var(--font-mono);
}

.stop-btn {
  text-transform: none;
}

.map-status {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: auto;
  padding-top: 16px;
  font-size: 12px;
  color: var(--text-muted);
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #ef4444;
  transition: background 0.3s ease;
}

.map-status.ready .status-dot {
  background: #22c55e;
}

.map-container {
  flex: 1;
  position: relative;
  overflow: hidden;
}

.streets-gl-iframe {
  width: 100%;
  height: 100%;
  border: none;
}

.loading-overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(135deg, var(--bg-primary) 0%, var(--bg-secondary) 100%);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  z-index: 10;
}

.loading-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 20px;
  max-width: 320px;
  width: 100%;
  padding: 0 24px;
}

.loading-icon {
  width: 64px;
  height: 64px;
  color: var(--accent-primary);
  animation: pulse 2s ease-in-out infinite;
}

.loading-icon svg {
  width: 100%;
  height: 100%;
}

@keyframes pulse {
  0%, 100% {
    opacity: 1;
    transform: scale(1);
  }
  50% {
    opacity: 0.7;
    transform: scale(1.05);
  }
}

.loading-title {
  font-size: 18px;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0;
}

.loading-progress-container {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 12px;
}

.loading-progress-bar {
  flex: 1;
  height: 8px;
  background: var(--bg-elevated);
  border-radius: 4px;
  overflow: hidden;
}

.loading-progress-fill {
  height: 100%;
  background: linear-gradient(90deg, var(--accent-primary), var(--accent-secondary));
  border-radius: 4px;
  transition: width 0.3s ease;
}

.loading-progress-text {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary);
  font-family: var(--font-mono);
  min-width: 42px;
  text-align: right;
}

.loading-file {
  font-size: 12px;
  color: var(--text-muted);
  margin: 0;
  text-align: center;
  max-width: 100%;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.map-mode-toggle {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: var(--bg-elevated);
  border-radius: 10px;
  padding: 12px 16px;
  margin-bottom: 16px;
}

.toggle-label {
  font-size: 13px;
  font-weight: 600;
  color: var(--text-primary);
}

.toggle-buttons {
  display: flex;
  gap: 4px;
  background: var(--bg-secondary);
  border-radius: 8px;
  padding: 4px;
}

.toggle-btn {
  padding: 8px 16px;
  font-size: 13px;
  font-weight: 600;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s ease;
  background: transparent;
  color: var(--text-secondary);
}

.toggle-btn:hover {
  color: var(--text-primary);
}

.toggle-btn.active {
  background: var(--accent-primary);
  color: white;
  box-shadow: 0 2px 8px rgba(var(--accent-primary-rgb), 0.3);
}
</style>
