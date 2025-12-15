import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '../views/LoginView.vue'
import RegisterView from '../views/RegisterView.vue'
import WorldSelectionView from '../views/WorldSelectionView.vue'
import WorldAirportSelectionView from '../views/WorldAirportSelectionView.vue'
import DashboardView from '../views/DashboardView.vue'
import JobsView from '../views/JobsView.vue'
import HangarView from '../views/HangarView.vue'
import MarketplaceView from '../views/MarketplaceView.vue'
import LicensesView from '../views/LicensesView.vue'
import BankingView from '../views/BankingView.vue'
import SkillsView from '../views/SkillsView.vue'
import ReputationView from '../views/ReputationView.vue'
import ProfileView from '../views/ProfileView.vue'
import SettingsView from '../views/SettingsView.vue'
import ConceptView from '../views/ConceptView.vue'
import DevView from '../views/DevView.vue'
import AdminView from '../views/AdminView.vue'
import AppLayout from '../layouts/AppLayout.vue'
import { useUserStore } from '../stores/user'
import { useSettingsStore } from '../stores/settings'
import { useWorldStore } from '../stores/world'
import { api } from '../services/api'

const routes = [
  {
    path: '/',
    name: 'login',
    component: LoginView,
    meta: { requiresGuest: true },
  },
  {
    path: '/register',
    name: 'register',
    component: RegisterView,
    meta: { requiresGuest: true },
  },
  {
    path: '/select-world',
    name: 'select-world',
    component: WorldSelectionView,
    meta: { requiresAuth: true },
  },
  {
    path: '/select-world-airport',
    name: 'select-world-airport',
    component: WorldAirportSelectionView,
    meta: { requiresAuth: true, requiresNoWorldHomeAirport: true },
  },
  {
    path: '/',
    component: AppLayout,
    meta: { requiresAuth: true },
    children: [
      {
        path: 'dashboard',
        name: 'dashboard',
        component: DashboardView,
      },
      {
        path: 'jobs',
        name: 'jobs',
        component: JobsView,
      },
      {
        path: 'hangar',
        name: 'hangar',
        component: HangarView,
      },
      {
        path: 'marketplace',
        name: 'marketplace',
        component: MarketplaceView,
      },
      {
        path: 'licenses',
        name: 'licenses',
        component: LicensesView,
      },
      {
        path: 'banking',
        name: 'banking',
        component: BankingView,
      },
      {
        path: 'skills',
        name: 'skills',
        component: SkillsView,
      },
      {
        path: 'reputation',
        name: 'reputation',
        component: ReputationView,
      },
      {
        path: 'profile',
        name: 'profile',
        component: ProfileView,
      },
      {
        path: 'settings',
        name: 'settings',
        component: SettingsView,
      },
      {
        path: 'concept',
        name: 'concept',
        component: ConceptView,
      },
      {
        path: 'dev',
        name: 'dev',
        component: DevView,
        meta: { requiresDeveloperMode: true },
      },
      {
        path: 'admin',
        name: 'admin',
        component: AdminView,
        meta: { requiresAdminMode: true },
      },
    ],
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach(async (to, _from, next) => {
  const userStore = useUserStore()
  const settingsStore = useSettingsStore()
  const worldStore = useWorldStore()
  userStore.loadUser()

  const hasValidToken = api.auth.isAuthenticated()
  const hasUserData = userStore.isLoggedIn.value

  // If we have user data but no valid token, clear the session
  if (hasUserData && !hasValidToken) {
    userStore.clearUser()
    worldStore.clearWorlds()
  }

  const isAuthenticated = hasValidToken && hasUserData

  // Check if any parent route requires auth
  const requiresAuth = to.matched.some(record => record.meta.requiresAuth)
  const requiresGuest = to.matched.some(record => record.meta.requiresGuest)
  const requiresNoWorld = to.matched.some(record => record.meta.requiresNoWorld)
  const requiresNoWorldHomeAirport = to.matched.some(record => record.meta.requiresNoWorldHomeAirport)
  const requiresDeveloperMode = to.matched.some(record => record.meta.requiresDeveloperMode)
  const requiresAdminMode = to.matched.some(record => record.meta.requiresAdminMode)

  // Check developer mode requirement
  if (requiresDeveloperMode && !settingsStore.settings.value.developerMode) {
    next('/dashboard')
    return
  }

  // Check admin mode requirement
  if (requiresAdminMode && !settingsStore.settings.value.adminMode) {
    next('/dashboard')
    return
  }

  if (requiresAuth && !isAuthenticated) {
    // Redirect to login if trying to access protected route
    next('/')
    return
  }

  if (requiresGuest && isAuthenticated) {
    // User is logged in, check their world status
    // Load worlds data if not already loaded
    if (worldStore.playerWorlds.value.length === 0) {
      await worldStore.loadMyWorlds()
    }

    const hasJoinedWorld = worldStore.playerWorlds.value.length > 0
    const currentWorld = worldStore.currentPlayerWorld.value
    const hasWorldHomeAirport = currentWorld?.homeAirportId != null

    if (!hasJoinedWorld) {
      next('/select-world')
    } else if (!hasWorldHomeAirport) {
      next('/select-world-airport')
    } else {
      next('/dashboard')
    }
    return
  }

  // For authenticated routes, check world onboarding status
  if (isAuthenticated) {
    // Load worlds data if not already loaded
    if (worldStore.playerWorlds.value.length === 0) {
      await worldStore.loadMyWorlds()
    }

    const hasJoinedWorld = worldStore.playerWorlds.value.length > 0
    const currentWorld = worldStore.currentPlayerWorld.value
    const hasWorldHomeAirport = currentWorld?.homeAirportId != null

    // Skip onboarding checks for onboarding routes themselves
    const isOnboardingRoute = ['/select-world', '/select-world-airport'].includes(to.path)

    if (!isOnboardingRoute) {
      if (!hasJoinedWorld) {
        next('/select-world')
        return
      } else if (!hasWorldHomeAirport) {
        next('/select-world-airport')
        return
      }
    }

    // Handle requiresNoWorld - redirect if user already has a world (for initial selection only)
    if (requiresNoWorld && hasJoinedWorld) {
      if (!hasWorldHomeAirport) {
        next('/select-world-airport')
      } else {
        next('/dashboard')
      }
      return
    }

    // Handle requiresNoWorldHomeAirport
    if (requiresNoWorldHomeAirport && hasWorldHomeAirport) {
      next('/dashboard')
      return
    }
  }

  next()
})

export default router
