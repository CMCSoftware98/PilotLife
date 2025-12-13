import { ref, computed } from 'vue';
import type { User, UserResponse, AuthResponse } from '../services/api';
import { api } from '../services/api';

const user = ref<User | null>(null);
const isLoading = ref(false);

function mapUserResponseToUser(response: UserResponse): User {
  return {
    id: response.id,
    email: response.email,
    firstName: response.firstName,
    lastName: response.lastName,
    experienceLevel: response.experienceLevel,
    balance: response.balance,
    totalFlightMinutes: response.totalFlightMinutes,
    currentAirportId: response.currentAirportId,
    currentAirport: response.currentAirport ? {
      id: response.currentAirport.id,
      ident: response.currentAirport.ident,
      name: response.currentAirport.name,
      iataCode: response.currentAirport.iataCode,
      type: 'airport',
      latitude: response.currentAirport.latitude,
      longitude: response.currentAirport.longitude,
    } : undefined,
    homeAirportId: response.homeAirportId,
    homeAirport: response.homeAirport ? {
      id: response.homeAirport.id,
      ident: response.homeAirport.ident,
      name: response.homeAirport.name,
      iataCode: response.homeAirport.iataCode,
      type: 'airport',
      latitude: response.homeAirport.latitude,
      longitude: response.homeAirport.longitude,
    } : undefined,
  };
}

export function useUserStore() {
  const isLoggedIn = computed(() => user.value !== null);

  function setUserFromAuthResponse(authResponse: AuthResponse) {
    user.value = mapUserResponseToUser(authResponse.user);
    localStorage.setItem('pilotlife_user', JSON.stringify(user.value));
  }

  function setUser(newUser: User) {
    user.value = newUser;
    localStorage.setItem('pilotlife_user', JSON.stringify(newUser));
  }

  function updateUser(updates: Partial<User>) {
    if (user.value) {
      user.value = { ...user.value, ...updates };
      localStorage.setItem('pilotlife_user', JSON.stringify(user.value));
    }
  }

  function clearUser() {
    user.value = null;
    localStorage.removeItem('pilotlife_user');
    api.auth.clearSession();
  }

  function loadUser() {
    // First check if we have a valid session
    if (!api.auth.isAuthenticated()) {
      // Try to load from localStorage for offline scenarios
      const stored = localStorage.getItem('pilotlife_user');
      if (stored) {
        try {
          user.value = JSON.parse(stored);
        } catch {
          localStorage.removeItem('pilotlife_user');
        }
      }
      return;
    }

    // Load from localStorage if available
    const stored = localStorage.getItem('pilotlife_user');
    if (stored) {
      try {
        user.value = JSON.parse(stored);
      } catch {
        localStorage.removeItem('pilotlife_user');
      }
    }
  }

  async function refreshUserData(): Promise<boolean> {
    if (!api.auth.isAuthenticated()) {
      return false;
    }

    isLoading.value = true;
    try {
      const response = await api.auth.me();
      if (response.data) {
        user.value = mapUserResponseToUser(response.data);
        localStorage.setItem('pilotlife_user', JSON.stringify(user.value));
        return true;
      }
      return false;
    } catch {
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  return {
    user,
    isLoggedIn,
    isLoading,
    setUser,
    setUserFromAuthResponse,
    updateUser,
    clearUser,
    loadUser,
    refreshUserData,
  };
}
