import { ref, computed } from 'vue';
import type { User } from '../services/api';

const user = ref<User | null>(null);

export function useUserStore() {
  const isLoggedIn = computed(() => user.value !== null);

  function setUser(newUser: User) {
    user.value = newUser;
    // Persist to localStorage
    localStorage.setItem('pilotlife_user', JSON.stringify(newUser));
  }

  function clearUser() {
    user.value = null;
    localStorage.removeItem('pilotlife_user');
  }

  function loadUser() {
    const stored = localStorage.getItem('pilotlife_user');
    if (stored) {
      try {
        user.value = JSON.parse(stored);
      } catch {
        localStorage.removeItem('pilotlife_user');
      }
    }
  }

  return {
    user,
    isLoggedIn,
    setUser,
    clearUser,
    loadUser,
  };
}
