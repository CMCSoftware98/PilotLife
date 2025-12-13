<template>
  <div class="auth-layout">
    <BackgroundDecoration />

    <BrandPanel />

    <div class="auth-panel">
      <AuthTabs v-model="activeTab" />

      <h2 class="form-title">Welcome back, Pilot!</h2>
      <p class="form-subtitle">Sign in to continue your flight career</p>

      <!-- Error Message -->
      <div v-if="errorMessage" class="error-message">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <circle cx="12" cy="12" r="10"/>
          <line x1="12" y1="8" x2="12" y2="12"/>
          <line x1="12" y1="16" x2="12.01" y2="16"/>
        </svg>
        <span>{{ errorMessage }}</span>
      </div>

      <v-form @submit.prevent="handleLogin" class="auth-form">
        <div class="form-group">
          <label class="form-label">Email Address</label>
          <v-text-field
            v-model="email"
            type="email"
            placeholder="captain@pilotlife.io"
            hide-details
            class="custom-input"
          />
        </div>

        <div class="form-group">
          <label class="form-label">Password</label>
          <v-text-field
            v-model="password"
            :type="showPassword ? 'text' : 'password'"
            placeholder="Enter your password"
            hide-details
            class="custom-input"
            :append-inner-icon="showPassword ? 'mdi-eye-off' : 'mdi-eye'"
            @click:append-inner="showPassword = !showPassword"
          />
        </div>

        <div class="form-options">
          <v-checkbox
            v-model="rememberMe"
            label="Keep me signed in"
            hide-details
            color="primary"
            class="custom-checkbox"
          />
          <a href="#" class="forgot-link">Forgot password?</a>
        </div>

        <v-btn
          type="submit"
          block
          size="x-large"
          class="submit-btn gradient-btn"
          :loading="loading"
        >
          Sign In to Cockpit
        </v-btn>
      </v-form>

      <SocialLogin divider-text="or continue with" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import BackgroundDecoration from '../components/BackgroundDecoration.vue'
import BrandPanel from '../components/BrandPanel.vue'
import AuthTabs from '../components/AuthTabs.vue'
import SocialLogin from '../components/SocialLogin.vue'
import { api } from '../services/api'
import { useUserStore } from '../stores/user'

const router = useRouter()
const userStore = useUserStore()

const activeTab = ref<'login' | 'register'>('login')
const email = ref('')
const password = ref('')
const rememberMe = ref(true)
const showPassword = ref(false)
const loading = ref(false)
const errorMessage = ref('')

watch(activeTab, (newTab) => {
  if (newTab === 'register') {
    router.push('/register')
  }
})

const handleLogin = async () => {
  loading.value = true
  errorMessage.value = ''

  const response = await api.auth.login({
    email: email.value,
    password: password.value,
  })

  loading.value = false

  if (response.error) {
    errorMessage.value = response.error
    return
  }

  if (response.data) {
    userStore.setUserFromAuthResponse(response.data)
    router.push('/dashboard')
  }
}
</script>

<style scoped>
.auth-layout {
  display: flex;
  min-height: 100vh;
  overflow: hidden;
}

.auth-panel {
  width: 520px;
  background: var(--bg-secondary);
  border-left: 1px solid var(--border-subtle);
  display: flex;
  flex-direction: column;
  justify-content: center;
  padding: 60px;
  position: relative;
  z-index: 2;
  box-shadow: -10px 0 40px rgba(0, 0, 0, 0.05);
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
  margin-bottom: 32px;
}

.auth-form {
  width: 100%;
}

.form-group {
  margin-bottom: 20px;
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
  padding: 16px 18px;
}

.custom-input :deep(.v-field__input::placeholder) {
  color: var(--text-muted);
}

.form-options {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin: 24px 0;
}

.custom-checkbox :deep(.v-label) {
  font-size: 14px;
  color: var(--text-secondary);
}

.forgot-link {
  font-size: 14px;
  color: var(--accent-primary);
  text-decoration: none;
  font-weight: 500;
}

.forgot-link:hover {
  text-decoration: underline;
}

.submit-btn {
  width: 100%;
  padding: 18px;
  font-size: 16px;
  font-weight: 600;
  text-transform: none;
  letter-spacing: 0;
}

.submit-btn :deep(.v-btn__content) {
  font-family: var(--font-primary);
}

.error-message {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 14px 16px;
  background: rgba(239, 68, 68, 0.1);
  border: 1px solid rgba(239, 68, 68, 0.3);
  border-radius: 12px;
  margin-bottom: 24px;
  color: #ef4444;
  font-size: 14px;
}

.error-message svg {
  width: 20px;
  height: 20px;
  flex-shrink: 0;
}
</style>
