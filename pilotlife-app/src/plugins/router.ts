import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '../views/LoginView.vue'
import RegisterView from '../views/RegisterView.vue'
import AirportSelectionView from '../views/AirportSelectionView.vue'
import DashboardView from '../views/DashboardView.vue'
import JobsView from '../views/JobsView.vue'
import HangarView from '../views/HangarView.vue'
import SkillsView from '../views/SkillsView.vue'
import ProfileView from '../views/ProfileView.vue'
import SettingsView from '../views/SettingsView.vue'
import ConceptView from '../views/ConceptView.vue'
import AppLayout from '../layouts/AppLayout.vue'
import { useUserStore } from '../stores/user'
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
    path: '/select-airport',
    name: 'select-airport',
    component: AirportSelectionView,
    meta: { requiresAuth: true, requiresNoHomeAirport: true },
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
        path: 'skills',
        name: 'skills',
        component: SkillsView,
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
    ],
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach((to, _from, next) => {
  const userStore = useUserStore()
  userStore.loadUser()

  const hasValidToken = api.auth.isAuthenticated()
  const hasUserData = userStore.isLoggedIn.value

  // If we have user data but no valid token, clear the session
  if (hasUserData && !hasValidToken) {
    userStore.clearUser()
  }

  const isAuthenticated = hasValidToken && hasUserData
  const hasHomeAirport = userStore.user.value?.homeAirportId != null

  // Check if any parent route requires auth
  const requiresAuth = to.matched.some(record => record.meta.requiresAuth)
  const requiresGuest = to.matched.some(record => record.meta.requiresGuest)
  const requiresNoHomeAirport = to.matched.some(record => record.meta.requiresNoHomeAirport)

  if (requiresAuth && !isAuthenticated) {
    // Redirect to login if trying to access protected route
    next('/')
  } else if (requiresGuest && isAuthenticated) {
    // Redirect based on whether user has home airport
    if (hasHomeAirport) {
      next('/dashboard')
    } else {
      next('/select-airport')
    }
  } else if (requiresNoHomeAirport && hasHomeAirport) {
    // User already has home airport, redirect to dashboard
    next('/dashboard')
  } else if (isAuthenticated && !hasHomeAirport && !requiresNoHomeAirport && to.path !== '/select-airport') {
    // User is authenticated but doesn't have home airport, redirect to selection
    next('/select-airport')
  } else {
    next()
  }
})

export default router
