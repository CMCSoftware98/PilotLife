<template>
  <div class="licenses-view">
    <!-- Header -->
    <header class="page-header">
      <div class="header-content">
        <h1>Licenses & Certifications</h1>
        <p class="header-subtitle">Earn licenses to unlock new aircraft and job opportunities</p>
      </div>
      <div class="header-stats" v-if="!loading">
        <div class="stat-card">
          <span class="stat-value">{{ validLicenses.length }}</span>
          <span class="stat-label">Active Licenses</span>
        </div>
        <div class="stat-card warning" v-if="expiringLicenses.length > 0">
          <span class="stat-value">{{ expiringLicenses.length }}</span>
          <span class="stat-label">Expiring Soon</span>
        </div>
      </div>
    </header>

    <!-- Loading State -->
    <div class="loading-state" v-if="loading">
      <v-progress-circular indeterminate color="primary" size="48" />
      <span>Loading licenses...</span>
    </div>

    <!-- Content -->
    <div class="licenses-content" v-else>
      <!-- My Licenses Section -->
      <section class="section" v-if="myLicenses.length > 0">
        <h2 class="section-title">My Licenses</h2>
        <div class="license-grid">
          <div
            v-for="license in myLicenses"
            :key="license.id"
            class="license-card"
            :class="{
              valid: license.isValid && !license.isRevoked,
              expired: license.isExpired,
              revoked: license.isRevoked,
              expiring: license.daysUntilExpiry !== undefined && license.daysUntilExpiry <= 30 && license.isValid
            }"
          >
            <div class="license-header">
              <div class="license-icon" :class="getCategoryClass(license.licenseType?.category)">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path v-if="license.licenseType?.category === 'Core'" d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>
                  <path v-else-if="license.licenseType?.category === 'Endorsement'" d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/>
                  <path v-else-if="license.licenseType?.category === 'TypeRating'" d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/>
                  <path v-else d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
                </svg>
              </div>
              <div class="license-title">
                <span class="license-code">{{ license.licenseType?.code }}</span>
                <span class="license-name">{{ license.licenseType?.name }}</span>
              </div>
              <span class="status-badge" :class="getStatusClass(license)">
                {{ getStatusText(license) }}
              </span>
            </div>
            <div class="license-body">
              <p class="license-description">{{ license.licenseType?.description }}</p>
              <div class="license-details">
                <div class="detail-row">
                  <span class="detail-label">Earned</span>
                  <span class="detail-value">{{ formatDate(license.earnedAt) }}</span>
                </div>
                <div class="detail-row" v-if="license.expiresAt">
                  <span class="detail-label">Expires</span>
                  <span class="detail-value" :class="{ warning: license.daysUntilExpiry !== undefined && license.daysUntilExpiry <= 30 }">
                    {{ formatDate(license.expiresAt) }}
                    <span v-if="license.daysUntilExpiry !== undefined && license.isValid" class="days-left">
                      ({{ license.daysUntilExpiry }} days)
                    </span>
                  </span>
                </div>
                <div class="detail-row">
                  <span class="detail-label">Exam Score</span>
                  <span class="detail-value">{{ license.examScore }}%</span>
                </div>
              </div>
            </div>
            <div class="license-actions" v-if="license.isExpired && !license.isRevoked">
              <button class="renew-btn" @click="handleRenew(license)" :disabled="renewingId === license.id">
                {{ renewingId === license.id ? 'Renewing...' : 'Renew License' }}
              </button>
            </div>
          </div>
        </div>
      </section>

      <!-- License Shop Section -->
      <section class="section">
        <h2 class="section-title">License Shop</h2>

        <!-- Category Tabs -->
        <div class="category-tabs">
          <button
            v-for="category in categories"
            :key="category.id"
            class="category-tab"
            :class="{ active: activeCategory === category.id }"
            @click="activeCategory = category.id"
          >
            {{ category.label }}
            <span class="count" v-if="getCategoryCount(category.id) > 0">
              {{ getCategoryCount(category.id) }}
            </span>
          </button>
        </div>

        <!-- Shop Items -->
        <div class="shop-grid">
          <div
            v-for="item in filteredShopItems"
            :key="item.licenseType.id"
            class="shop-card"
            :class="{
              owned: item.isOwned,
              available: item.canPurchase && !item.isOwned,
              locked: !item.canPurchase && !item.isOwned
            }"
          >
            <div class="shop-header">
              <div class="shop-icon" :class="getCategoryClass(item.licenseType.category)">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path v-if="item.licenseType.category === 'Core'" d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>
                  <path v-else-if="item.licenseType.category === 'Endorsement'" d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/>
                  <path v-else-if="item.licenseType.category === 'TypeRating'" d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/>
                  <path v-else d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
                </svg>
              </div>
              <div class="shop-title">
                <span class="shop-code">{{ item.licenseType.code }}</span>
                <span class="shop-name">{{ item.licenseType.name }}</span>
              </div>
              <span class="owned-badge" v-if="item.isOwned">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/>
                  <polyline points="22 4 12 14.01 9 11.01"/>
                </svg>
                Owned
              </span>
            </div>
            <div class="shop-body">
              <p class="shop-description">{{ item.licenseType.description }}</p>
              <div class="shop-details">
                <div class="detail-row">
                  <span class="detail-label">Duration</span>
                  <span class="detail-value">{{ item.licenseType.examDurationMinutes }} min exam</span>
                </div>
                <div class="detail-row">
                  <span class="detail-label">Pass Score</span>
                  <span class="detail-value">{{ item.licenseType.passingScore }}%</span>
                </div>
                <div class="detail-row" v-if="item.licenseType.validityGameDays > 0">
                  <span class="detail-label">Validity</span>
                  <span class="detail-value">{{ item.licenseType.validityGameDays }} days</span>
                </div>
              </div>
              <div class="prerequisites" v-if="item.missingPrerequisites.length > 0">
                <span class="prereq-label">Missing Prerequisites:</span>
                <div class="prereq-list">
                  <span v-for="prereq in item.missingPrerequisites" :key="prereq" class="prereq-badge">
                    {{ prereq }}
                  </span>
                </div>
              </div>
            </div>
            <div class="shop-footer">
              <div class="price-info">
                <span class="price-label">Exam Cost</span>
                <span class="price-value">${{ item.adjustedExamCost.toLocaleString() }}</span>
              </div>
              <button
                class="schedule-btn"
                :disabled="!item.canPurchase || item.isOwned || schedulingCode === item.licenseType.code"
                @click="handleScheduleExam(item)"
              >
                <template v-if="item.isOwned">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <polyline points="20 6 9 17 4 12"/>
                  </svg>
                  Earned
                </template>
                <template v-else-if="!item.canPurchase">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/>
                    <path d="M7 11V7a5 5 0 0 1 10 0v4"/>
                  </svg>
                  Locked
                </template>
                <template v-else>
                  {{ schedulingCode === item.licenseType.code ? 'Scheduling...' : 'Schedule Exam' }}
                </template>
              </button>
            </div>
          </div>
        </div>
      </section>

      <!-- Exam History Section -->
      <section class="section" v-if="examHistory.length > 0">
        <h2 class="section-title">Exam History</h2>
        <div class="history-table-container">
          <table class="history-table">
            <thead>
              <tr>
                <th>License</th>
                <th>Date</th>
                <th>Status</th>
                <th>Score</th>
                <th>Attempt</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="exam in examHistory" :key="exam.id" :class="exam.status.toLowerCase()">
                <td>
                  <span class="exam-license">{{ exam.licenseType?.code }}</span>
                  <span class="exam-license-name">{{ exam.licenseType?.name }}</span>
                </td>
                <td>{{ formatDate(exam.scheduledAt) }}</td>
                <td>
                  <span class="exam-status" :class="exam.status.toLowerCase()">
                    {{ exam.status }}
                  </span>
                </td>
                <td>
                  <span v-if="exam.finalScore !== undefined">{{ exam.finalScore }}%</span>
                  <span v-else>-</span>
                </td>
                <td>#{{ exam.attemptNumber }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </div>

    <!-- Schedule Exam Modal -->
    <div class="modal-overlay" v-if="showScheduleModal" @click.self="showScheduleModal = false">
      <div class="modal">
        <div class="modal-header">
          <h3>Schedule Exam: {{ selectedLicense?.licenseType.name }}</h3>
          <button class="close-btn" @click="showScheduleModal = false">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <line x1="18" y1="6" x2="6" y2="18"/>
              <line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>
        <div class="modal-body">
          <div class="form-group">
            <label>Departure Airport (ICAO)</label>
            <input
              type="text"
              v-model="examDeparture"
              placeholder="e.g., KJFK"
              maxlength="4"
              class="form-input"
            />
          </div>
          <div class="form-group" v-if="selectedLicense?.licenseType.category !== 'Core'">
            <label>Arrival Airport (ICAO) - Optional</label>
            <input
              type="text"
              v-model="examArrival"
              placeholder="e.g., KLAX"
              maxlength="4"
              class="form-input"
            />
          </div>
          <div class="exam-info">
            <p><strong>Duration:</strong> {{ selectedLicense?.licenseType.examDurationMinutes }} minutes</p>
            <p><strong>Passing Score:</strong> {{ selectedLicense?.licenseType.passingScore }}%</p>
            <p><strong>Cost:</strong> ${{ selectedLicense?.adjustedExamCost.toLocaleString() }}</p>
          </div>
        </div>
        <div class="modal-footer">
          <button class="cancel-btn" @click="showScheduleModal = false">Cancel</button>
          <button class="confirm-btn" @click="confirmScheduleExam" :disabled="!examDeparture || isScheduling">
            {{ isScheduling ? 'Scheduling...' : 'Schedule Exam' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useLicenseStore } from '../stores/licenses'
import { useWorldStore } from '../stores/world'
import type { LicenseShopItemResponse, UserLicenseResponse } from '../services/api'

const licenseStore = useLicenseStore()
const worldStore = useWorldStore()

const loading = ref(true)
const renewingId = ref<string | null>(null)
const schedulingCode = ref<string | null>(null)
const activeCategory = ref<string>('all')

// Modal state
const showScheduleModal = ref(false)
const selectedLicense = ref<LicenseShopItemResponse | null>(null)
const examDeparture = ref('')
const examArrival = ref('')
const isScheduling = ref(false)

const categories = [
  { id: 'all', label: 'All' },
  { id: 'Core', label: 'Core Licenses' },
  { id: 'Endorsement', label: 'Endorsements' },
  { id: 'TypeRating', label: 'Type Ratings' },
  { id: 'Certification', label: 'Certifications' },
]

// Computed
const myLicenses = computed(() => licenseStore.myLicenses.value)
const validLicenses = computed(() => licenseStore.validLicenses.value)
const expiringLicenses = computed(() => licenseStore.expiringLicenses.value)
const examHistory = computed(() => licenseStore.examHistory.value)

const filteredShopItems = computed(() => {
  const items = licenseStore.shopData.value?.licenseTypes ?? []
  if (activeCategory.value === 'all') return items
  return items.filter(item => item.licenseType.category === activeCategory.value)
})

// Methods
function getCategoryCount(categoryId: string): number {
  const items = licenseStore.shopData.value?.licenseTypes ?? []
  if (categoryId === 'all') return items.length
  return items.filter(item => item.licenseType.category === categoryId).length
}

function getCategoryClass(category?: string): string {
  switch (category) {
    case 'Core': return 'category-core'
    case 'Endorsement': return 'category-endorsement'
    case 'TypeRating': return 'category-typerating'
    case 'Certification': return 'category-certification'
    default: return ''
  }
}

function getStatusClass(license: UserLicenseResponse): string {
  if (license.isRevoked) return 'revoked'
  if (license.isExpired) return 'expired'
  if (license.daysUntilExpiry !== undefined && license.daysUntilExpiry <= 30) return 'expiring'
  return 'valid'
}

function getStatusText(license: UserLicenseResponse): string {
  if (license.isRevoked) return 'Revoked'
  if (license.isExpired) return 'Expired'
  if (license.daysUntilExpiry !== undefined && license.daysUntilExpiry <= 30) return 'Expiring Soon'
  return 'Valid'
}

function formatDate(dateString: string): string {
  const date = new Date(dateString)
  return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
}

async function handleRenew(license: UserLicenseResponse) {
  if (!worldStore.currentPlayerWorld.value?.id) return
  renewingId.value = license.id
  await licenseStore.renewLicense(worldStore.currentPlayerWorld.value.id, license.id)
  renewingId.value = null
}

function handleScheduleExam(item: LicenseShopItemResponse) {
  selectedLicense.value = item
  examDeparture.value = worldStore.currentPlayerWorld.value?.currentAirportIdent ?? ''
  examArrival.value = ''
  showScheduleModal.value = true
}

async function confirmScheduleExam() {
  if (!selectedLicense.value || !worldStore.currentPlayerWorld.value?.id || !examDeparture.value) return

  isScheduling.value = true
  schedulingCode.value = selectedLicense.value.licenseType.code

  const result = await licenseStore.scheduleExam({
    playerWorldId: worldStore.currentPlayerWorld.value.id,
    licenseTypeCode: selectedLicense.value.licenseType.code,
    departureAirportIcao: examDeparture.value.toUpperCase(),
    arrivalAirportIcao: examArrival.value ? examArrival.value.toUpperCase() : undefined,
  })

  if (result) {
    showScheduleModal.value = false
    // Refresh data
    await loadData()
  }

  isScheduling.value = false
  schedulingCode.value = null
}

async function loadData() {
  if (!worldStore.currentPlayerWorld.value?.id) return

  loading.value = true
  const worldId = worldStore.currentPlayerWorld.value.id

  await Promise.all([
    licenseStore.loadShop(worldId),
    licenseStore.loadMyLicenses(worldId),
    licenseStore.loadExamHistory(worldId),
  ])

  loading.value = false
}

onMounted(loadData)

watch(() => worldStore.currentPlayerWorld.value?.id, loadData)
</script>

<style scoped>
.licenses-view {
  min-height: 100%;
  background: var(--bg-primary);
  padding: 24px;
}

/* Header */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 32px;
}

.header-content h1 {
  font-size: 28px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 4px;
}

.header-subtitle {
  color: var(--text-secondary);
  font-size: 14px;
}

.header-stats {
  display: flex;
  gap: 16px;
}

.stat-card {
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 12px;
  padding: 16px 24px;
  text-align: center;
}

.stat-card.warning {
  border-color: #f59e0b;
  background: rgba(245, 158, 11, 0.1);
}

.stat-value {
  display: block;
  font-size: 24px;
  font-weight: 700;
  color: var(--text-primary);
}

.stat-card.warning .stat-value {
  color: #f59e0b;
}

.stat-label {
  font-size: 12px;
  color: var(--text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

/* Loading */
.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 16px;
  padding: 64px;
  color: var(--text-secondary);
}

/* Sections */
.section {
  margin-bottom: 40px;
}

.section-title {
  font-size: 18px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 20px;
}

/* License Cards */
.license-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 20px;
}

.license-card {
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 12px;
  overflow: hidden;
}

.license-card.valid {
  border-color: #22c55e;
}

.license-card.expired {
  border-color: #ef4444;
  opacity: 0.8;
}

.license-card.revoked {
  border-color: #6b7280;
  opacity: 0.6;
}

.license-card.expiring {
  border-color: #f59e0b;
}

.license-header {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px;
  border-bottom: 1px solid var(--border-subtle);
}

.license-icon, .shop-icon {
  width: 40px;
  height: 40px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.license-icon svg, .shop-icon svg {
  width: 20px;
  height: 20px;
}

.category-core {
  background: rgba(59, 130, 246, 0.2);
  color: #3b82f6;
}

.category-endorsement {
  background: rgba(168, 85, 247, 0.2);
  color: #a855f7;
}

.category-typerating {
  background: rgba(34, 197, 94, 0.2);
  color: #22c55e;
}

.category-certification {
  background: rgba(245, 158, 11, 0.2);
  color: #f59e0b;
}

.license-title, .shop-title {
  flex: 1;
  min-width: 0;
}

.license-code, .shop-code {
  display: block;
  font-size: 14px;
  font-weight: 700;
  color: var(--text-primary);
  font-family: var(--font-mono);
}

.license-name, .shop-name {
  display: block;
  font-size: 12px;
  color: var(--text-secondary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.status-badge {
  padding: 4px 10px;
  border-radius: 100px;
  font-size: 11px;
  font-weight: 600;
}

.status-badge.valid {
  background: rgba(34, 197, 94, 0.2);
  color: #22c55e;
}

.status-badge.expired {
  background: rgba(239, 68, 68, 0.2);
  color: #ef4444;
}

.status-badge.revoked {
  background: rgba(107, 114, 128, 0.2);
  color: #6b7280;
}

.status-badge.expiring {
  background: rgba(245, 158, 11, 0.2);
  color: #f59e0b;
}

.license-body, .shop-body {
  padding: 16px;
}

.license-description, .shop-description {
  font-size: 13px;
  color: var(--text-secondary);
  margin-bottom: 16px;
  line-height: 1.5;
}

.license-details, .shop-details {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.detail-row {
  display: flex;
  justify-content: space-between;
  font-size: 13px;
}

.detail-label {
  color: var(--text-muted);
}

.detail-value {
  color: var(--text-primary);
  font-weight: 500;
}

.detail-value.warning {
  color: #f59e0b;
}

.days-left {
  font-size: 11px;
  color: var(--text-muted);
}

.license-actions {
  padding: 12px 16px;
  border-top: 1px solid var(--border-subtle);
}

.renew-btn {
  width: 100%;
  padding: 10px;
  background: #f59e0b;
  border: none;
  border-radius: 8px;
  color: white;
  font-weight: 600;
  font-size: 14px;
  cursor: pointer;
  transition: opacity 0.15s;
}

.renew-btn:hover:not(:disabled) {
  opacity: 0.9;
}

.renew-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* Category Tabs */
.category-tabs {
  display: flex;
  gap: 8px;
  margin-bottom: 20px;
  flex-wrap: wrap;
}

.category-tab {
  padding: 8px 16px;
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 8px;
  color: var(--text-secondary);
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.15s;
  display: flex;
  align-items: center;
  gap: 8px;
}

.category-tab:hover {
  background: var(--bg-elevated);
  color: var(--text-primary);
}

.category-tab.active {
  background: rgba(59, 130, 246, 0.1);
  border-color: var(--accent-primary);
  color: var(--accent-primary);
}

.category-tab .count {
  background: var(--bg-elevated);
  padding: 2px 6px;
  border-radius: 4px;
  font-size: 11px;
}

.category-tab.active .count {
  background: rgba(59, 130, 246, 0.2);
}

/* Shop Cards */
.shop-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
}

.shop-card {
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 12px;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.shop-card.owned {
  border-color: #22c55e;
}

.shop-card.locked {
  opacity: 0.7;
}

.shop-header {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px;
  border-bottom: 1px solid var(--border-subtle);
}

.owned-badge {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 4px 10px;
  background: rgba(34, 197, 94, 0.2);
  color: #22c55e;
  border-radius: 100px;
  font-size: 11px;
  font-weight: 600;
}

.owned-badge svg {
  width: 14px;
  height: 14px;
}

.shop-body {
  flex: 1;
}

.prerequisites {
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px solid var(--border-subtle);
}

.prereq-label {
  display: block;
  font-size: 11px;
  color: var(--text-muted);
  margin-bottom: 8px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.prereq-list {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.prereq-badge {
  padding: 4px 8px;
  background: rgba(239, 68, 68, 0.2);
  color: #ef4444;
  border-radius: 4px;
  font-size: 11px;
  font-weight: 600;
  font-family: var(--font-mono);
}

.shop-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  border-top: 1px solid var(--border-subtle);
  background: var(--bg-elevated);
}

.price-info {
  display: flex;
  flex-direction: column;
}

.price-label {
  font-size: 11px;
  color: var(--text-muted);
}

.price-value {
  font-size: 16px;
  font-weight: 700;
  color: #22c55e;
}

.schedule-btn {
  padding: 8px 16px;
  background: var(--accent-primary);
  border: none;
  border-radius: 8px;
  color: white;
  font-weight: 600;
  font-size: 13px;
  cursor: pointer;
  transition: opacity 0.15s;
  display: flex;
  align-items: center;
  gap: 6px;
}

.schedule-btn:hover:not(:disabled) {
  opacity: 0.9;
}

.schedule-btn:disabled {
  background: var(--bg-secondary);
  color: var(--text-muted);
  cursor: not-allowed;
}

.schedule-btn svg {
  width: 16px;
  height: 16px;
}

/* History Table */
.history-table-container {
  overflow-x: auto;
}

.history-table {
  width: 100%;
  border-collapse: collapse;
}

.history-table th {
  background: var(--bg-secondary);
  padding: 12px 16px;
  text-align: left;
  font-size: 11px;
  font-weight: 600;
  color: var(--text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  border-bottom: 1px solid var(--border-subtle);
}

.history-table td {
  padding: 12px 16px;
  font-size: 14px;
  color: var(--text-primary);
  border-bottom: 1px solid var(--border-subtle);
}

.exam-license {
  display: block;
  font-weight: 600;
  font-family: var(--font-mono);
}

.exam-license-name {
  display: block;
  font-size: 12px;
  color: var(--text-secondary);
}

.exam-status {
  padding: 4px 10px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 600;
}

.exam-status.passed {
  background: rgba(34, 197, 94, 0.2);
  color: #22c55e;
}

.exam-status.failed {
  background: rgba(239, 68, 68, 0.2);
  color: #ef4444;
}

.exam-status.inprogress {
  background: rgba(59, 130, 246, 0.2);
  color: #3b82f6;
}

.exam-status.scheduled {
  background: rgba(168, 85, 247, 0.2);
  color: #a855f7;
}

.exam-status.abandoned {
  background: rgba(107, 114, 128, 0.2);
  color: #6b7280;
}

/* Modal */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal {
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 16px;
  width: 100%;
  max-width: 480px;
  max-height: 90vh;
  overflow: hidden;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  border-bottom: 1px solid var(--border-subtle);
}

.modal-header h3 {
  font-size: 18px;
  font-weight: 600;
  color: var(--text-primary);
}

.close-btn {
  background: none;
  border: none;
  padding: 8px;
  color: var(--text-secondary);
  cursor: pointer;
}

.close-btn:hover {
  color: var(--text-primary);
}

.close-btn svg {
  width: 20px;
  height: 20px;
}

.modal-body {
  padding: 20px;
}

.form-group {
  margin-bottom: 16px;
}

.form-group label {
  display: block;
  font-size: 13px;
  font-weight: 500;
  color: var(--text-secondary);
  margin-bottom: 6px;
}

.form-input {
  width: 100%;
  padding: 10px 14px;
  background: var(--bg-primary);
  border: 1px solid var(--border-subtle);
  border-radius: 8px;
  font-size: 14px;
  color: var(--text-primary);
  text-transform: uppercase;
}

.form-input:focus {
  outline: none;
  border-color: var(--accent-primary);
}

.exam-info {
  background: var(--bg-elevated);
  border-radius: 8px;
  padding: 16px;
}

.exam-info p {
  font-size: 14px;
  color: var(--text-secondary);
  margin-bottom: 8px;
}

.exam-info p:last-child {
  margin-bottom: 0;
}

.exam-info strong {
  color: var(--text-primary);
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding: 16px 20px;
  border-top: 1px solid var(--border-subtle);
}

.cancel-btn {
  padding: 10px 20px;
  background: var(--bg-elevated);
  border: 1px solid var(--border-subtle);
  border-radius: 8px;
  color: var(--text-secondary);
  font-weight: 500;
  cursor: pointer;
}

.cancel-btn:hover {
  background: var(--bg-primary);
}

.confirm-btn {
  padding: 10px 20px;
  background: var(--accent-primary);
  border: none;
  border-radius: 8px;
  color: white;
  font-weight: 600;
  cursor: pointer;
}

.confirm-btn:hover:not(:disabled) {
  opacity: 0.9;
}

.confirm-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
