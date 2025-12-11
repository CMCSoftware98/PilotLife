<template>
  <div class="auth-layout">
    <BackgroundDecoration />

    <BrandPanel />

    <div class="auth-panel">
      <AuthTabs v-model="activeTab" />

      <h2 class="form-title">Join PilotLife</h2>
      <p class="form-subtitle">Create your pilot account and start your virtual career</p>

      <!-- Progress Steps -->
      <div class="progress-steps">
        <div class="step" :class="{ completed: currentStep > 1, active: currentStep === 1 }">
          <div class="step-number">{{ currentStep > 1 ? '✓' : '1' }}</div>
          <span class="step-label">Account</span>
        </div>
        <div class="step-connector" :class="{ completed: currentStep > 1 }"></div>
        <div class="step" :class="{ completed: currentStep > 2, active: currentStep === 2 }">
          <div class="step-number">{{ currentStep > 2 ? '✓' : '2' }}</div>
          <span class="step-label">Profile</span>
        </div>
        <div class="step-connector" :class="{ completed: currentStep > 2 }"></div>
        <div class="step" :class="{ active: currentStep === 3 }">
          <div class="step-number">3</div>
          <span class="step-label">Confirm</span>
        </div>
      </div>

      <v-form @submit.prevent="handleRegister" class="auth-form">
        <!-- Step 1: Account -->
        <template v-if="currentStep === 1">
          <div class="form-row">
            <div class="form-group">
              <label class="form-label">First Name</label>
              <v-text-field
                v-model="firstName"
                placeholder="John"
                hide-details
                class="custom-input"
              />
            </div>
            <div class="form-group">
              <label class="form-label">Last Name</label>
              <v-text-field
                v-model="lastName"
                placeholder="Doe"
                hide-details
                class="custom-input"
              />
            </div>
          </div>

          <div class="form-group">
            <label class="form-label">Email Address</label>
            <v-text-field
              v-model="email"
              type="email"
              placeholder="pilot@example.com"
              hide-details
              class="custom-input"
            />
          </div>
        </template>

        <!-- Step 2: Security -->
        <template v-if="currentStep === 2">
          <div class="form-group">
            <label class="form-label">Password</label>
            <v-text-field
              v-model="password"
              :type="showPassword ? 'text' : 'password'"
              placeholder="Create a strong password"
              hide-details
              class="custom-input"
              :append-inner-icon="showPassword ? 'mdi-eye-off' : 'mdi-eye'"
              @click:append-inner="showPassword = !showPassword"
            />
            <div class="password-strength">
              <div class="strength-bar" :class="passwordStrengthClass(0)"></div>
              <div class="strength-bar" :class="passwordStrengthClass(1)"></div>
              <div class="strength-bar" :class="passwordStrengthClass(2)"></div>
              <div class="strength-bar" :class="passwordStrengthClass(3)"></div>
            </div>
            <p class="strength-text" :class="{ strong: passwordStrength >= 3 }">
              {{ passwordStrengthText }}
            </p>
          </div>

          <div class="form-group">
            <label class="form-label">Confirm Password</label>
            <v-text-field
              v-model="confirmPassword"
              :type="showConfirmPassword ? 'text' : 'password'"
              placeholder="Confirm your password"
              hide-details
              class="custom-input"
              :append-inner-icon="showConfirmPassword ? 'mdi-eye-off' : 'mdi-eye'"
              @click:append-inner="showConfirmPassword = !showConfirmPassword"
            />
          </div>

          <div class="form-group">
            <label class="form-label">Experience Level</label>
            <v-select
              v-model="experienceLevel"
              :items="experienceLevels"
              item-title="text"
              item-value="value"
              placeholder="Select your experience..."
              hide-details
              class="custom-input"
            />
          </div>
        </template>

        <!-- Step 3: Confirmation -->
        <template v-if="currentStep === 3">
          <div class="form-checkbox">
            <v-checkbox
              v-model="agreeTerms"
              hide-details
              color="primary"
            >
              <template #label>
                <span class="checkbox-label">
                  I agree to the <a href="#">Terms of Service</a> and <a href="#">Privacy Policy</a>.
                  I understand that my flight data will be tracked for career progression.
                </span>
              </template>
            </v-checkbox>
          </div>

          <div class="form-checkbox">
            <v-checkbox
              v-model="agreeNewsletter"
              hide-details
              color="primary"
            >
              <template #label>
                <span class="checkbox-label">
                  Send me updates about new features, community events, and special offers
                </span>
              </template>
            </v-checkbox>
          </div>

          <div class="summary-card">
            <h4>Account Summary</h4>
            <div class="summary-row">
              <span>Name:</span>
              <strong>{{ firstName }} {{ lastName }}</strong>
            </div>
            <div class="summary-row">
              <span>Email:</span>
              <strong>{{ email }}</strong>
            </div>
          </div>
        </template>

        <div class="form-actions">
          <v-btn
            v-if="currentStep > 1"
            variant="outlined"
            size="large"
            class="back-btn"
            @click="currentStep--"
          >
            Back
          </v-btn>
          <v-btn
            v-if="currentStep < 3"
            size="large"
            class="submit-btn gradient-btn"
            @click="currentStep++"
          >
            Continue
          </v-btn>
          <v-btn
            v-else
            type="submit"
            size="large"
            class="submit-btn gradient-btn"
            :loading="loading"
            :disabled="!agreeTerms"
          >
            Create Pilot Account
          </v-btn>
        </div>
      </v-form>

      <SocialLogin divider-text="or register with" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useRouter } from 'vue-router'
import BackgroundDecoration from '../components/BackgroundDecoration.vue'
import BrandPanel from '../components/BrandPanel.vue'
import AuthTabs from '../components/AuthTabs.vue'
import SocialLogin from '../components/SocialLogin.vue'

const router = useRouter()

const activeTab = ref<'login' | 'register'>('register')
const currentStep = ref(1)

// Form fields
const firstName = ref('')
const lastName = ref('')
const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const showPassword = ref(false)
const showConfirmPassword = ref(false)
const experienceLevel = ref('')
const agreeTerms = ref(false)
const agreeNewsletter = ref(true)
const loading = ref(false)

const experienceLevels = [
  { text: 'Beginner - Just starting out', value: 'beginner' },
  { text: 'Intermediate - Some flight sim experience', value: 'intermediate' },
  { text: 'Advanced - Experienced virtual pilot', value: 'advanced' },
  { text: 'Expert - Real-world pilot or instructor', value: 'expert' },
]

watch(activeTab, (newTab) => {
  if (newTab === 'login') {
    router.push('/')
  }
})

const passwordStrength = computed(() => {
  let strength = 0
  if (password.value.length >= 8) strength++
  if (/[A-Z]/.test(password.value)) strength++
  if (/[0-9]/.test(password.value)) strength++
  if (/[^A-Za-z0-9]/.test(password.value)) strength++
  return strength
})

const passwordStrengthText = computed(() => {
  if (!password.value) return 'Enter a password'
  switch (passwordStrength.value) {
    case 0:
    case 1:
      return 'Weak password'
    case 2:
      return 'Fair password'
    case 3:
      return 'Good password'
    case 4:
      return 'Strong password'
    default:
      return ''
  }
})

const passwordStrengthClass = (index: number) => {
  if (!password.value) return ''
  if (passwordStrength.value > index) {
    if (passwordStrength.value <= 1) return 'weak'
    if (passwordStrength.value === 2) return 'medium'
    return 'strong'
  }
  return ''
}

const handleRegister = async () => {
  loading.value = true
  await new Promise(resolve => setTimeout(resolve, 1500))
  loading.value = false
  console.log('Register:', {
    firstName: firstName.value,
    lastName: lastName.value,
    email: email.value,
    experienceLevel: experienceLevel.value,
  })
}
</script>

<style scoped>
.auth-layout {
  display: flex;
  min-height: 100vh;
  overflow: hidden;
}

.auth-panel {
  width: 560px;
  background: var(--bg-secondary);
  border-left: 1px solid var(--border-subtle);
  display: flex;
  flex-direction: column;
  justify-content: center;
  padding: 48px 60px;
  position: relative;
  z-index: 2;
  box-shadow: -10px 0 40px rgba(0, 0, 0, 0.05);
  overflow-y: auto;
}

.form-title {
  font-size: 28px;
  font-weight: 700;
  margin-bottom: 8px;
  color: var(--text-primary);
}

.form-subtitle {
  color: var(--text-secondary);
  font-size: 15px;
  margin-bottom: 28px;
}

/* Progress steps */
.progress-steps {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  margin-bottom: 32px;
}

.step {
  display: flex;
  align-items: center;
  gap: 8px;
}

.step-number {
  width: 32px;
  height: 32px;
  background: var(--bg-elevated);
  border: 2px solid var(--border-subtle);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 13px;
  font-weight: 700;
  color: var(--text-muted);
  transition: all 0.3s ease;
}

.step.active .step-number {
  background: var(--accent-primary);
  border-color: var(--accent-primary);
  color: white;
}

.step.completed .step-number {
  background: var(--accent-success);
  border-color: var(--accent-success);
  color: white;
}

.step-label {
  font-size: 13px;
  color: var(--text-muted);
  font-weight: 600;
}

.step.active .step-label {
  color: var(--accent-primary);
}

.step.completed .step-label {
  color: var(--accent-success);
}

.step-connector {
  width: 40px;
  height: 3px;
  background: var(--border-subtle);
  border-radius: 2px;
}

.step-connector.completed {
  background: var(--accent-success);
}

/* Info box */
.info-box {
  display: flex;
  align-items: flex-start;
  gap: 14px;
  padding: 16px 18px;
  background: rgba(14, 165, 233, 0.08);
  border: 1px solid rgba(14, 165, 233, 0.2);
  border-radius: 12px;
  margin-bottom: 20px;
}

.info-box svg {
  width: 22px;
  height: 22px;
  stroke: var(--accent-primary);
  flex-shrink: 0;
  margin-top: 2px;
}

.info-box p {
  font-size: 14px;
  color: var(--text-secondary);
  line-height: 1.6;
}

.auth-form {
  width: 100%;
}

.form-row {
  display: flex;
  gap: 16px;
  margin-bottom: 16px;
}

.form-group {
  flex: 1;
  margin-bottom: 16px;
}

.form-label {
  display: block;
  font-size: 13px;
  font-weight: 600;
  color: var(--text-secondary);
  margin-bottom: 8px;
}

.custom-input :deep(.v-field) {
  background: var(--bg-elevated);
  border: 2px solid transparent;
  border-radius: 12px;
  transition: all 0.3s ease;
}

.custom-input :deep(.v-field:hover) {
  background: var(--bg-elevated);
}

.custom-input :deep(.v-field--focused) {
  background: var(--bg-secondary);
  border-color: var(--accent-primary);
  box-shadow: 0 0 0 4px var(--accent-glow);
}

.custom-input :deep(.v-field__input) {
  font-family: var(--font-primary);
  font-size: 15px;
  padding: 14px 16px;
}

.custom-input :deep(.v-field__input::placeholder) {
  color: var(--text-muted);
}

/* Password strength */
.password-strength {
  display: flex;
  gap: 6px;
  margin-top: 10px;
}

.strength-bar {
  flex: 1;
  height: 4px;
  background: var(--border-subtle);
  border-radius: 2px;
  overflow: hidden;
}

.strength-bar.weak {
  background: #ef4444;
}

.strength-bar.medium {
  background: var(--accent-warm);
}

.strength-bar.strong {
  background: var(--accent-success);
}

.strength-text {
  font-size: 12px;
  color: var(--text-muted);
  margin-top: 6px;
  font-weight: 500;
}

.strength-text.strong {
  color: var(--accent-success);
}

/* Checkbox */
.form-checkbox {
  margin: 20px 0;
}

.checkbox-label {
  font-size: 14px;
  color: var(--text-secondary);
  line-height: 1.6;
}

.checkbox-label a {
  color: var(--accent-primary);
  text-decoration: none;
  font-weight: 500;
}

.checkbox-label a:hover {
  text-decoration: underline;
}

/* Summary card */
.summary-card {
  background: var(--bg-elevated);
  border-radius: 14px;
  padding: 20px;
  margin-bottom: 20px;
}

.summary-card h4 {
  font-size: 16px;
  font-weight: 700;
  margin-bottom: 16px;
  color: var(--text-primary);
}

.summary-row {
  display: flex;
  justify-content: space-between;
  padding: 10px 0;
  border-bottom: 1px solid var(--border-subtle);
}

.summary-row:last-child {
  border-bottom: none;
}

.summary-row span {
  color: var(--text-secondary);
  font-size: 14px;
}

.summary-row strong {
  color: var(--text-primary);
  font-size: 14px;
}

/* Form actions */
.form-actions {
  display: flex;
  gap: 12px;
  margin-top: 24px;
}

.back-btn {
  flex: 0 0 auto;
  text-transform: none;
  font-weight: 600;
  border-color: var(--border-subtle);
  color: var(--text-secondary);
}

.submit-btn {
  flex: 1;
  font-size: 16px;
  font-weight: 600;
  text-transform: none;
  letter-spacing: 0;
}

.submit-btn :deep(.v-btn__content) {
  font-family: var(--font-primary);
}
</style>
