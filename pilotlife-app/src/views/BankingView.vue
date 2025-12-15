<template>
  <div class="banking-view">
    <!-- Header -->
    <header class="page-header">
      <div class="header-content">
        <h1>Banking & Loans</h1>
        <p class="header-subtitle">Manage your finances and credit</p>
      </div>
    </header>

    <!-- Loading State -->
    <div class="loading-state" v-if="loading">
      <v-progress-circular indeterminate color="primary" size="48" />
      <span>Loading banking data...</span>
    </div>

    <!-- Content -->
    <div class="banking-content" v-else>
      <!-- Overview Cards -->
      <div class="overview-section">
        <!-- Credit Score Card -->
        <div class="overview-card credit-card">
          <div class="card-header">
            <h3>Credit Score</h3>
            <span class="rating-badge" :class="getRatingClass(creditScore?.rating)">
              {{ creditScore?.rating || 'N/A' }}
            </span>
          </div>
          <div class="credit-score-display">
            <div class="score-circle" :class="getRatingClass(creditScore?.rating)">
              <span class="score-value">{{ creditScore?.score || 0 }}</span>
            </div>
            <div class="score-range">
              <span>{{ creditScore?.minPossible || 300 }}</span>
              <div class="range-bar">
                <div class="range-fill" :style="{ width: creditScorePercent + '%' }"></div>
              </div>
              <span>{{ creditScore?.maxPossible || 850 }}</span>
            </div>
          </div>
          <div class="credit-stats">
            <div class="stat-row">
              <span class="stat-label">On-time Payments</span>
              <span class="stat-value positive">{{ Math.round(creditScore?.onTimePaymentPercent || 0) }}%</span>
            </div>
            <div class="stat-row">
              <span class="stat-label">Recent Changes</span>
              <span class="stat-value">
                <span class="positive" v-if="creditScore?.recentPositiveChanges">+{{ creditScore.recentPositiveChanges }}</span>
                <span class="negative" v-if="creditScore?.recentNegativeChanges">-{{ creditScore.recentNegativeChanges }}</span>
              </span>
            </div>
          </div>
        </div>

        <!-- Debt Summary Card -->
        <div class="overview-card debt-card">
          <div class="card-header">
            <h3>Debt Summary</h3>
          </div>
          <div class="debt-amount">
            <span class="amount-label">Total Debt</span>
            <span class="amount-value" :class="{ 'has-debt': (loanSummary?.totalDebt || 0) > 0 }">
              ${{ (loanSummary?.totalDebt || 0).toLocaleString() }}
            </span>
          </div>
          <div class="debt-stats">
            <div class="stat-row">
              <span class="stat-label">Monthly Payment</span>
              <span class="stat-value">${{ (loanSummary?.totalMonthlyPayment || 0).toLocaleString() }}</span>
            </div>
            <div class="stat-row">
              <span class="stat-label">Active Loans</span>
              <span class="stat-value">{{ loanSummary?.activeLoans || 0 }}</span>
            </div>
            <div class="stat-row" v-if="loanSummary?.nextPaymentDue">
              <span class="stat-label">Next Payment</span>
              <span class="stat-value">{{ formatDate(loanSummary.nextPaymentDue) }}</span>
            </div>
          </div>
        </div>

        <!-- Starter Loan Card -->
        <div class="overview-card starter-card" v-if="starterLoanEligibility">
          <div class="card-header">
            <h3>Starter Loan</h3>
            <span class="eligibility-badge" :class="starterLoanEligibility.isEligible ? 'eligible' : 'ineligible'">
              {{ starterLoanEligibility.isEligible ? 'Eligible' : 'Not Eligible' }}
            </span>
          </div>
          <div class="starter-content">
            <p class="starter-description" v-if="starterLoanEligibility.isEligible">
              Special low-interest loan for new commercial pilots. Available once per world.
            </p>
            <p class="starter-description" v-else>
              {{ starterLoanEligibility.reason || 'You are not eligible for a starter loan.' }}
            </p>
            <div class="starter-details" v-if="starterLoanEligibility.isEligible">
              <div class="detail-row">
                <span class="detail-label">Max Amount</span>
                <span class="detail-value">${{ (starterLoanEligibility.maxAmount || 0).toLocaleString() }}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">Interest Rate</span>
                <span class="detail-value">{{ starterLoanEligibility.interestRatePercent?.toFixed(1) }}% /mo</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">Terms Available</span>
                <span class="detail-value">{{ starterLoanEligibility.availableTerms?.join(', ') }} months</span>
              </div>
            </div>
            <button
              class="apply-starter-btn"
              v-if="starterLoanEligibility.isEligible"
              @click="showStarterLoanModal = true"
            >
              Apply for Starter Loan
            </button>
          </div>
        </div>
      </div>

      <!-- Active Loans Section -->
      <section class="section" v-if="bankingStore.activeLoans.value.length > 0">
        <h2 class="section-title">Active Loans</h2>
        <div class="loans-grid">
          <div
            v-for="loan in bankingStore.activeLoans.value"
            :key="loan.id"
            class="loan-card active"
          >
            <div class="loan-header">
              <div class="loan-type-badge" :class="loan.loanType.toLowerCase()">
                {{ formatLoanType(loan.loanType) }}
              </div>
              <span class="loan-bank">{{ loan.bankName }}</span>
            </div>
            <div class="loan-body">
              <div class="loan-amount">
                <span class="amount-label">Remaining Balance</span>
                <span class="amount-value">${{ loan.remainingPrincipal.toLocaleString() }}</span>
              </div>
              <div class="loan-progress">
                <div class="progress-bar">
                  <div
                    class="progress-fill"
                    :style="{ width: ((loan.totalPaid / loan.totalRepaymentAmount) * 100) + '%' }"
                  ></div>
                </div>
                <div class="progress-labels">
                  <span>Paid: ${{ loan.totalPaid.toLocaleString() }}</span>
                  <span>Total: ${{ loan.totalRepaymentAmount.toLocaleString() }}</span>
                </div>
              </div>
              <div class="loan-details">
                <div class="detail-row">
                  <span class="detail-label">Monthly Payment</span>
                  <span class="detail-value">${{ loan.monthlyPayment.toLocaleString() }}</span>
                </div>
                <div class="detail-row">
                  <span class="detail-label">Interest Rate</span>
                  <span class="detail-value">{{ loan.interestRatePercent.toFixed(1) }}% /mo</span>
                </div>
                <div class="detail-row">
                  <span class="detail-label">Payments</span>
                  <span class="detail-value">{{ loan.paymentsMade }} / {{ loan.paymentsMade + loan.paymentsRemaining }}</span>
                </div>
                <div class="detail-row" v-if="loan.nextPaymentDue">
                  <span class="detail-label">Next Payment</span>
                  <span class="detail-value" :class="{ overdue: isOverdue(loan.nextPaymentDue) }">
                    {{ formatDate(loan.nextPaymentDue) }}
                  </span>
                </div>
              </div>
            </div>
            <div class="loan-actions">
              <button class="payment-btn" @click="openPaymentModal(loan)">Make Payment</button>
              <button class="payoff-btn" @click="openPayoffModal(loan)">Pay Off</button>
            </div>
          </div>
        </div>
      </section>

      <!-- Loan History Section -->
      <section class="section" v-if="bankingStore.paidOffLoans.value.length > 0 || bankingStore.defaultedLoans.value.length > 0">
        <h2 class="section-title">Loan History</h2>
        <div class="history-table-container">
          <table class="history-table">
            <thead>
              <tr>
                <th>Type</th>
                <th>Amount</th>
                <th>Status</th>
                <th>Interest Rate</th>
                <th>Total Paid</th>
                <th>Date</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="loan in [...bankingStore.paidOffLoans.value, ...bankingStore.defaultedLoans.value]"
                :key="loan.id"
                :class="loan.status.toLowerCase()"
              >
                <td>{{ formatLoanType(loan.loanType) }}</td>
                <td>${{ loan.principalAmount.toLocaleString() }}</td>
                <td>
                  <span class="status-badge" :class="loan.status.toLowerCase()">
                    {{ loan.status }}
                  </span>
                </td>
                <td>{{ loan.interestRatePercent.toFixed(1) }}%</td>
                <td>${{ loan.totalPaid.toLocaleString() }}</td>
                <td>{{ formatDate(loan.paidOffAt || loan.approvedAt || '') }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- Credit History Section -->
      <section class="section" v-if="creditHistory.length > 0">
        <h2 class="section-title">Credit History</h2>
        <div class="credit-history-list">
          <div
            v-for="event in creditHistory"
            :key="event.id"
            class="credit-event"
            :class="event.scoreChange >= 0 ? 'positive' : 'negative'"
          >
            <div class="event-icon">
              <svg v-if="event.scoreChange >= 0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M12 19V5M5 12l7-7 7 7"/>
              </svg>
              <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M12 5v14M5 12l7 7 7-7"/>
              </svg>
            </div>
            <div class="event-content">
              <span class="event-description">{{ event.description }}</span>
              <span class="event-date">{{ formatDate(event.occurredAt) }}</span>
            </div>
            <div class="event-change">
              <span class="change-value">{{ event.scoreChange >= 0 ? '+' : '' }}{{ event.scoreChange }}</span>
              <span class="score-after">{{ event.scoreAfter }}</span>
            </div>
          </div>
        </div>
      </section>

      <!-- Apply for Loan Section -->
      <section class="section">
        <h2 class="section-title">Apply for a Loan</h2>
        <div class="loan-types-grid">
          <div
            v-for="loanType in loanTypes"
            :key="loanType.type"
            class="loan-type-card"
          >
            <div class="type-icon" :class="loanType.type.toLowerCase()">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path v-if="loanType.type === 'AircraftFinancing'" d="M22 12h-4l-3 9L9 3l-3 9H2"/>
                <path v-else-if="loanType.type === 'Personal'" d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/>
                <path v-else-if="loanType.type === 'Business'" d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/>
                <path v-else d="M22 12h-4l-3 9L9 3l-3 9H2"/>
              </svg>
            </div>
            <div class="type-info">
              <h4>{{ loanType.name }}</h4>
              <p>{{ loanType.description }}</p>
            </div>
            <button class="apply-btn" @click="openLoanApplicationModal(loanType.type)">
              Apply
            </button>
          </div>
        </div>
      </section>
    </div>

    <!-- Starter Loan Modal -->
    <div class="modal-overlay" v-if="showStarterLoanModal" @click.self="showStarterLoanModal = false">
      <div class="modal">
        <div class="modal-header">
          <h3>Apply for Starter Loan</h3>
          <button class="close-btn" @click="showStarterLoanModal = false">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <line x1="18" y1="6" x2="6" y2="18"/>
              <line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>
        <div class="modal-body">
          <div class="form-group">
            <label>Loan Amount</label>
            <input
              type="number"
              v-model.number="starterLoanAmount"
              :max="starterLoanEligibility?.maxAmount || 250000"
              min="1000"
              step="1000"
              class="form-input"
            />
            <span class="input-hint">Max: ${{ (starterLoanEligibility?.maxAmount || 250000).toLocaleString() }}</span>
          </div>
          <div class="form-group">
            <label>Term (Months)</label>
            <select v-model.number="starterLoanTerm" class="form-input">
              <option v-for="term in starterLoanEligibility?.availableTerms || [6, 9, 12]" :key="term" :value="term">
                {{ term }} months
              </option>
            </select>
          </div>
          <div class="loan-preview" v-if="starterLoanAmount > 0">
            <h4>Loan Preview</h4>
            <div class="preview-row">
              <span>Monthly Payment</span>
              <span>${{ calculateMonthlyPayment(starterLoanAmount, starterLoanEligibility?.interestRate || 0.015, starterLoanTerm).toLocaleString() }}</span>
            </div>
            <div class="preview-row">
              <span>Total Repayment</span>
              <span>${{ (calculateMonthlyPayment(starterLoanAmount, starterLoanEligibility?.interestRate || 0.015, starterLoanTerm) * starterLoanTerm).toLocaleString() }}</span>
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <button class="cancel-btn" @click="showStarterLoanModal = false">Cancel</button>
          <button class="confirm-btn" @click="submitStarterLoan" :disabled="isSubmitting || starterLoanAmount <= 0">
            {{ isSubmitting ? 'Submitting...' : 'Apply' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Loan Application Modal -->
    <div class="modal-overlay" v-if="showLoanModal" @click.self="showLoanModal = false">
      <div class="modal">
        <div class="modal-header">
          <h3>Apply for {{ formatLoanType(selectedLoanType) }}</h3>
          <button class="close-btn" @click="showLoanModal = false">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <line x1="18" y1="6" x2="6" y2="18"/>
              <line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>
        <div class="modal-body">
          <div class="form-group">
            <label>Loan Amount</label>
            <input
              type="number"
              v-model.number="loanAmount"
              min="1000"
              step="1000"
              class="form-input"
            />
          </div>
          <div class="form-group">
            <label>Term (Months)</label>
            <select v-model.number="loanTerm" class="form-input">
              <option v-for="term in [6, 12, 18, 24]" :key="term" :value="term">
                {{ term }} months
              </option>
            </select>
          </div>
          <div class="form-group">
            <label>Purpose (Optional)</label>
            <input
              type="text"
              v-model="loanPurpose"
              placeholder="e.g., Aircraft purchase"
              class="form-input"
            />
          </div>
          <div class="loan-preview" v-if="loanAmount > 0 && currentBank">
            <h4>Estimated Terms</h4>
            <div class="preview-row">
              <span>Interest Rate</span>
              <span>{{ getEstimatedRate().toFixed(1) }}% /mo</span>
            </div>
            <div class="preview-row">
              <span>Monthly Payment</span>
              <span>${{ calculateMonthlyPayment(loanAmount, getEstimatedRate() / 100, loanTerm).toLocaleString() }}</span>
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <button class="cancel-btn" @click="showLoanModal = false">Cancel</button>
          <button class="confirm-btn" @click="submitLoanApplication" :disabled="isSubmitting || loanAmount <= 0">
            {{ isSubmitting ? 'Submitting...' : 'Apply' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Payment Modal -->
    <div class="modal-overlay" v-if="showPaymentModal" @click.self="showPaymentModal = false">
      <div class="modal">
        <div class="modal-header">
          <h3>Make Payment</h3>
          <button class="close-btn" @click="showPaymentModal = false">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <line x1="18" y1="6" x2="6" y2="18"/>
              <line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>
        <div class="modal-body">
          <div class="payment-info">
            <p><strong>Loan:</strong> {{ formatLoanType(selectedPaymentLoan?.loanType || '') }}</p>
            <p><strong>Monthly Payment:</strong> ${{ (selectedPaymentLoan?.monthlyPayment || 0).toLocaleString() }}</p>
            <p><strong>Remaining Balance:</strong> ${{ (selectedPaymentLoan?.remainingPrincipal || 0).toLocaleString() }}</p>
          </div>
          <div class="form-group">
            <label>Payment Amount</label>
            <input
              type="number"
              v-model.number="paymentAmount"
              :min="1"
              step="100"
              class="form-input"
            />
          </div>
          <div class="quick-amounts">
            <button @click="paymentAmount = selectedPaymentLoan?.monthlyPayment || 0">Monthly</button>
            <button @click="paymentAmount = (selectedPaymentLoan?.monthlyPayment || 0) * 2">Double</button>
            <button @click="paymentAmount = selectedPaymentLoan?.remainingPrincipal || 0">Full Balance</button>
          </div>
        </div>
        <div class="modal-footer">
          <button class="cancel-btn" @click="showPaymentModal = false">Cancel</button>
          <button class="confirm-btn" @click="submitPayment" :disabled="isSubmitting || paymentAmount <= 0">
            {{ isSubmitting ? 'Processing...' : 'Pay $' + paymentAmount.toLocaleString() }}
          </button>
        </div>
      </div>
    </div>

    <!-- Payoff Confirmation Modal -->
    <div class="modal-overlay" v-if="showPayoffModal" @click.self="showPayoffModal = false">
      <div class="modal">
        <div class="modal-header">
          <h3>Pay Off Loan</h3>
          <button class="close-btn" @click="showPayoffModal = false">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <line x1="18" y1="6" x2="6" y2="18"/>
              <line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>
        <div class="modal-body">
          <p class="payoff-message">
            Are you sure you want to pay off this loan in full?
          </p>
          <div class="payoff-details">
            <p><strong>Loan Type:</strong> {{ formatLoanType(selectedPaymentLoan?.loanType || '') }}</p>
            <p><strong>Remaining Balance:</strong> ${{ (selectedPaymentLoan?.remainingPrincipal || 0).toLocaleString() }}</p>
          </div>
        </div>
        <div class="modal-footer">
          <button class="cancel-btn" @click="showPayoffModal = false">Cancel</button>
          <button class="confirm-btn payoff" @click="submitPayoff" :disabled="isSubmitting">
            {{ isSubmitting ? 'Processing...' : 'Pay Off Loan' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useBankingStore } from '../stores/banking'
import { useWorldStore } from '../stores/world'
import type { LoanResponse } from '../services/api'

const bankingStore = useBankingStore()
const worldStore = useWorldStore()

const loading = ref(true)
const isSubmitting = ref(false)

// Modal state
const showStarterLoanModal = ref(false)
const showLoanModal = ref(false)
const showPaymentModal = ref(false)
const showPayoffModal = ref(false)

// Starter loan form
const starterLoanAmount = ref(50000)
const starterLoanTerm = ref(12)

// Loan application form
const selectedLoanType = ref('AircraftFinancing')
const loanAmount = ref(50000)
const loanTerm = ref(12)
const loanPurpose = ref('')

// Payment form
const selectedPaymentLoan = ref<LoanResponse | null>(null)
const paymentAmount = ref(0)

// Data
const creditScore = computed(() => bankingStore.creditScore.value)
const loanSummary = computed(() => bankingStore.loanSummary.value)
const starterLoanEligibility = computed(() => bankingStore.starterLoanEligibility.value)
const creditHistory = computed(() => bankingStore.creditHistory.value)
const currentBank = computed(() => bankingStore.banks.value[0])

const creditScorePercent = computed(() => {
  if (!creditScore.value) return 0
  const range = creditScore.value.maxPossible - creditScore.value.minPossible
  return ((creditScore.value.score - creditScore.value.minPossible) / range) * 100
})

const loanTypes = [
  { type: 'AircraftFinancing', name: 'Aircraft Financing', description: 'Finance your next aircraft purchase' },
  { type: 'Personal', name: 'Personal Loan', description: 'General purpose loan for any need' },
  { type: 'Business', name: 'Business Loan', description: 'Expand your aviation business' },
  { type: 'Emergency', name: 'Emergency Loan', description: 'Quick approval for urgent needs' },
]

// Methods
function getRatingClass(rating?: string): string {
  if (!rating) return ''
  const lower = rating.toLowerCase()
  if (lower.includes('excellent')) return 'excellent'
  if (lower.includes('good')) return 'good'
  if (lower.includes('fair')) return 'fair'
  if (lower.includes('poor')) return 'poor'
  return ''
}

function formatLoanType(type: string): string {
  switch (type) {
    case 'StarterLoan': return 'Starter Loan'
    case 'AircraftFinancing': return 'Aircraft Financing'
    case 'Personal': return 'Personal Loan'
    case 'Business': return 'Business Loan'
    case 'Emergency': return 'Emergency Loan'
    default: return type
  }
}

function formatDate(dateString: string): string {
  if (!dateString) return '-'
  const date = new Date(dateString)
  return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
}

function isOverdue(dateString: string): boolean {
  return new Date(dateString) < new Date()
}

function calculateMonthlyPayment(principal: number, monthlyRate: number, termMonths: number): number {
  if (monthlyRate === 0) return principal / termMonths
  const rate = monthlyRate
  const payment = principal * (rate * Math.pow(1 + rate, termMonths)) / (Math.pow(1 + rate, termMonths) - 1)
  return Math.round(payment * 100) / 100
}

function getEstimatedRate(): number {
  if (!currentBank.value || !creditScore.value) return 5
  const score = creditScore.value.score
  const minScore = 500
  const maxScore = 800
  const clampedScore = Math.max(minScore, Math.min(maxScore, score))
  const ratio = (clampedScore - minScore) / (maxScore - minScore)
  return currentBank.value.maxInterestRatePercent - (ratio * (currentBank.value.maxInterestRatePercent - currentBank.value.baseInterestRatePercent))
}

function openLoanApplicationModal(type: string) {
  selectedLoanType.value = type
  loanAmount.value = 50000
  loanTerm.value = 12
  loanPurpose.value = ''
  showLoanModal.value = true
}

function openPaymentModal(loan: LoanResponse) {
  selectedPaymentLoan.value = loan
  paymentAmount.value = loan.monthlyPayment
  showPaymentModal.value = true
}

function openPayoffModal(loan: LoanResponse) {
  selectedPaymentLoan.value = loan
  showPayoffModal.value = true
}

async function submitStarterLoan() {
  isSubmitting.value = true
  const result = await bankingStore.applyForStarterLoan({
    amount: starterLoanAmount.value,
    termMonths: starterLoanTerm.value,
  })
  isSubmitting.value = false

  if (result.success) {
    showStarterLoanModal.value = false
    if (result.newBalance !== undefined) {
      worldStore.updateCurrentPlayerWorld({ balance: result.newBalance })
    }
  }
}

async function submitLoanApplication() {
  isSubmitting.value = true
  const result = await bankingStore.applyForLoan({
    loanType: selectedLoanType.value,
    amount: loanAmount.value,
    termMonths: loanTerm.value,
    purpose: loanPurpose.value || undefined,
  })
  isSubmitting.value = false

  if (result.success) {
    showLoanModal.value = false
    if (result.newBalance !== undefined) {
      worldStore.updateCurrentPlayerWorld({ balance: result.newBalance })
    }
  }
}

async function submitPayment() {
  if (!selectedPaymentLoan.value) return

  isSubmitting.value = true
  const result = await bankingStore.makePayment(selectedPaymentLoan.value.id, paymentAmount.value)
  isSubmitting.value = false

  if (result.success) {
    showPaymentModal.value = false
    if (result.newBalance !== undefined) {
      worldStore.updateCurrentPlayerWorld({ balance: result.newBalance })
    }
  }
}

async function submitPayoff() {
  if (!selectedPaymentLoan.value) return

  isSubmitting.value = true
  const result = await bankingStore.payOffLoan(selectedPaymentLoan.value.id)
  isSubmitting.value = false

  if (result.success) {
    showPayoffModal.value = false
    if (result.newBalance !== undefined) {
      worldStore.updateCurrentPlayerWorld({ balance: result.newBalance })
    }
  }
}

async function loadData() {
  loading.value = true

  await Promise.all([
    bankingStore.loadLoans(),
    bankingStore.loadSummary(),
    bankingStore.loadCreditScore(),
    bankingStore.loadCreditHistory(20),
    bankingStore.loadStarterLoanEligibility(),
    bankingStore.loadBanks(),
  ])

  loading.value = false
}

onMounted(loadData)

watch(() => worldStore.currentPlayerWorld.value?.id, loadData)
</script>

<style scoped>
.banking-view {
  min-height: 100%;
  background: var(--bg-primary);
  padding: 24px;
}

/* Header */
.page-header {
  margin-bottom: 32px;
}

.page-header h1 {
  font-size: 28px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 4px;
}

.header-subtitle {
  color: var(--text-secondary);
  font-size: 14px;
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

/* Overview Section */
.overview-section {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 20px;
  margin-bottom: 40px;
}

.overview-card {
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 16px;
  padding: 24px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.card-header h3 {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-primary);
}

.rating-badge, .eligibility-badge {
  padding: 4px 12px;
  border-radius: 100px;
  font-size: 12px;
  font-weight: 600;
}

.rating-badge.excellent, .eligibility-badge.eligible {
  background: rgba(34, 197, 94, 0.2);
  color: #22c55e;
}

.rating-badge.good {
  background: rgba(59, 130, 246, 0.2);
  color: #3b82f6;
}

.rating-badge.fair {
  background: rgba(245, 158, 11, 0.2);
  color: #f59e0b;
}

.rating-badge.poor, .eligibility-badge.ineligible {
  background: rgba(239, 68, 68, 0.2);
  color: #ef4444;
}

/* Credit Score Card */
.credit-score-display {
  text-align: center;
  margin-bottom: 20px;
}

.score-circle {
  width: 120px;
  height: 120px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 16px;
  background: var(--bg-elevated);
  border: 4px solid var(--border-subtle);
}

.score-circle.excellent { border-color: #22c55e; }
.score-circle.good { border-color: #3b82f6; }
.score-circle.fair { border-color: #f59e0b; }
.score-circle.poor { border-color: #ef4444; }

.score-value {
  font-size: 32px;
  font-weight: 700;
  color: var(--text-primary);
}

.score-range {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 12px;
  color: var(--text-muted);
}

.range-bar {
  flex: 1;
  height: 6px;
  background: var(--bg-elevated);
  border-radius: 3px;
  overflow: hidden;
}

.range-fill {
  height: 100%;
  background: linear-gradient(90deg, #ef4444, #f59e0b, #22c55e);
  border-radius: 3px;
}

.credit-stats, .debt-stats, .starter-details {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.stat-row, .detail-row {
  display: flex;
  justify-content: space-between;
  font-size: 14px;
}

.stat-label, .detail-label {
  color: var(--text-secondary);
}

.stat-value, .detail-value {
  color: var(--text-primary);
  font-weight: 500;
}

.positive { color: #22c55e; }
.negative { color: #ef4444; }

/* Debt Card */
.debt-amount {
  text-align: center;
  margin-bottom: 20px;
}

.amount-label {
  display: block;
  font-size: 12px;
  color: var(--text-muted);
  margin-bottom: 4px;
}

.amount-value {
  font-size: 32px;
  font-weight: 700;
  color: #22c55e;
}

.amount-value.has-debt {
  color: #f59e0b;
}

/* Starter Card */
.starter-description {
  font-size: 14px;
  color: var(--text-secondary);
  margin-bottom: 16px;
  line-height: 1.5;
}

.apply-starter-btn {
  width: 100%;
  padding: 12px;
  background: linear-gradient(135deg, #22c55e, #16a34a);
  border: none;
  border-radius: 8px;
  color: white;
  font-weight: 600;
  font-size: 14px;
  cursor: pointer;
  margin-top: 16px;
}

.apply-starter-btn:hover {
  opacity: 0.9;
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

/* Loans Grid */
.loans-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 20px;
}

.loan-card {
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 12px;
  overflow: hidden;
}

.loan-card.active {
  border-color: #3b82f6;
}

.loan-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  background: var(--bg-elevated);
  border-bottom: 1px solid var(--border-subtle);
}

.loan-type-badge {
  padding: 4px 10px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 600;
}

.loan-type-badge.starterloan { background: rgba(34, 197, 94, 0.2); color: #22c55e; }
.loan-type-badge.aircraftfinancing { background: rgba(59, 130, 246, 0.2); color: #3b82f6; }
.loan-type-badge.personal { background: rgba(168, 85, 247, 0.2); color: #a855f7; }
.loan-type-badge.business { background: rgba(245, 158, 11, 0.2); color: #f59e0b; }
.loan-type-badge.emergency { background: rgba(239, 68, 68, 0.2); color: #ef4444; }

.loan-bank {
  font-size: 12px;
  color: var(--text-secondary);
}

.loan-body {
  padding: 16px;
}

.loan-amount {
  text-align: center;
  margin-bottom: 16px;
}

.loan-progress {
  margin-bottom: 16px;
}

.progress-bar {
  height: 8px;
  background: var(--bg-elevated);
  border-radius: 4px;
  overflow: hidden;
  margin-bottom: 8px;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(90deg, #3b82f6, #22c55e);
  border-radius: 4px;
  transition: width 0.3s ease;
}

.progress-labels {
  display: flex;
  justify-content: space-between;
  font-size: 12px;
  color: var(--text-muted);
}

.loan-details {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.detail-value.overdue {
  color: #ef4444;
}

.loan-actions {
  display: flex;
  gap: 12px;
  padding: 16px;
  border-top: 1px solid var(--border-subtle);
}

.payment-btn, .payoff-btn {
  flex: 1;
  padding: 10px;
  border-radius: 8px;
  font-weight: 600;
  font-size: 13px;
  cursor: pointer;
  transition: opacity 0.15s;
}

.payment-btn {
  background: var(--accent-primary);
  border: none;
  color: white;
}

.payoff-btn {
  background: transparent;
  border: 1px solid var(--border-subtle);
  color: var(--text-primary);
}

.payment-btn:hover, .payoff-btn:hover {
  opacity: 0.9;
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

.status-badge {
  padding: 4px 10px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 600;
}

.status-badge.paidoff { background: rgba(34, 197, 94, 0.2); color: #22c55e; }
.status-badge.defaulted { background: rgba(239, 68, 68, 0.2); color: #ef4444; }

/* Credit History */
.credit-history-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.credit-event {
  display: flex;
  align-items: center;
  gap: 16px;
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 8px;
  padding: 16px;
}

.event-icon {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.credit-event.positive .event-icon {
  background: rgba(34, 197, 94, 0.2);
  color: #22c55e;
}

.credit-event.negative .event-icon {
  background: rgba(239, 68, 68, 0.2);
  color: #ef4444;
}

.event-icon svg {
  width: 20px;
  height: 20px;
}

.event-content {
  flex: 1;
  min-width: 0;
}

.event-description {
  display: block;
  font-size: 14px;
  color: var(--text-primary);
  margin-bottom: 2px;
}

.event-date {
  font-size: 12px;
  color: var(--text-muted);
}

.event-change {
  text-align: right;
}

.change-value {
  display: block;
  font-size: 16px;
  font-weight: 700;
}

.credit-event.positive .change-value { color: #22c55e; }
.credit-event.negative .change-value { color: #ef4444; }

.score-after {
  font-size: 12px;
  color: var(--text-muted);
}

/* Loan Types Grid */
.loan-types-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
  gap: 16px;
}

.loan-type-card {
  background: var(--bg-secondary);
  border: 1px solid var(--border-subtle);
  border-radius: 12px;
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.type-icon {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.type-icon svg {
  width: 24px;
  height: 24px;
}

.type-icon.aircraftfinancing { background: rgba(59, 130, 246, 0.2); color: #3b82f6; }
.type-icon.personal { background: rgba(168, 85, 247, 0.2); color: #a855f7; }
.type-icon.business { background: rgba(245, 158, 11, 0.2); color: #f59e0b; }
.type-icon.emergency { background: rgba(239, 68, 68, 0.2); color: #ef4444; }

.type-info h4 {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 4px;
}

.type-info p {
  font-size: 13px;
  color: var(--text-secondary);
}

.apply-btn {
  padding: 10px;
  background: var(--bg-elevated);
  border: 1px solid var(--border-subtle);
  border-radius: 8px;
  color: var(--text-primary);
  font-weight: 600;
  font-size: 13px;
  cursor: pointer;
  transition: all 0.15s;
}

.apply-btn:hover {
  background: var(--accent-primary);
  border-color: var(--accent-primary);
  color: white;
}

/* Modals */
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
}

.form-input:focus {
  outline: none;
  border-color: var(--accent-primary);
}

.input-hint {
  font-size: 12px;
  color: var(--text-muted);
  margin-top: 4px;
}

.loan-preview {
  background: var(--bg-elevated);
  border-radius: 8px;
  padding: 16px;
  margin-top: 16px;
}

.loan-preview h4 {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 12px;
}

.preview-row {
  display: flex;
  justify-content: space-between;
  font-size: 14px;
  margin-bottom: 8px;
}

.preview-row:last-child {
  margin-bottom: 0;
}

.preview-row span:first-child {
  color: var(--text-secondary);
}

.preview-row span:last-child {
  color: var(--text-primary);
  font-weight: 500;
}

.payment-info {
  background: var(--bg-elevated);
  border-radius: 8px;
  padding: 16px;
  margin-bottom: 16px;
}

.payment-info p {
  font-size: 14px;
  color: var(--text-secondary);
  margin-bottom: 8px;
}

.payment-info p:last-child {
  margin-bottom: 0;
}

.payment-info strong {
  color: var(--text-primary);
}

.quick-amounts {
  display: flex;
  gap: 8px;
  margin-top: 12px;
}

.quick-amounts button {
  flex: 1;
  padding: 8px;
  background: var(--bg-elevated);
  border: 1px solid var(--border-subtle);
  border-radius: 6px;
  font-size: 12px;
  color: var(--text-secondary);
  cursor: pointer;
}

.quick-amounts button:hover {
  background: var(--bg-primary);
  color: var(--text-primary);
}

.payoff-message {
  font-size: 14px;
  color: var(--text-secondary);
  margin-bottom: 16px;
}

.payoff-details {
  background: var(--bg-elevated);
  border-radius: 8px;
  padding: 16px;
}

.payoff-details p {
  font-size: 14px;
  color: var(--text-secondary);
  margin-bottom: 8px;
}

.payoff-details p:last-child {
  margin-bottom: 0;
}

.payoff-details strong {
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

.confirm-btn.payoff {
  background: #22c55e;
}

.confirm-btn:hover:not(:disabled) {
  opacity: 0.9;
}

.confirm-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
