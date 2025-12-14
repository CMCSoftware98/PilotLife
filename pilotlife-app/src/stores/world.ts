import { ref, computed } from 'vue';
import type { WorldResponse, PlayerWorldResponse } from '../services/api';
import { api } from '../services/api';

const worlds = ref<WorldResponse[]>([]);
const playerWorlds = ref<PlayerWorldResponse[]>([]);
const currentPlayerWorld = ref<PlayerWorldResponse | null>(null);
const isLoading = ref(false);
const error = ref<string | null>(null);

const CURRENT_WORLD_KEY = 'pilotlife_current_world_id';

export function useWorldStore() {
  const hasJoinedWorlds = computed(() => playerWorlds.value.length > 0);

  const currentWorld = computed(() => {
    if (!currentPlayerWorld.value) return null;
    return worlds.value.find(w => w.id === currentPlayerWorld.value?.worldId) ?? null;
  });

  async function loadWorlds(): Promise<boolean> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.worlds.list();
      if (response.data) {
        worlds.value = response.data;
        return true;
      }
      error.value = response.error ?? 'Failed to load worlds';
      return false;
    } catch {
      error.value = 'Failed to load worlds';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  async function loadMyWorlds(): Promise<boolean> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.worlds.getMyWorlds();
      if (response.data) {
        playerWorlds.value = response.data;

        // Restore current world from localStorage or set default
        const storedWorldId = localStorage.getItem(CURRENT_WORLD_KEY);
        if (storedWorldId) {
          const savedWorld = playerWorlds.value.find(pw => pw.id === storedWorldId);
          if (savedWorld) {
            currentPlayerWorld.value = savedWorld;
          } else if (playerWorlds.value.length > 0) {
            currentPlayerWorld.value = playerWorlds.value[0];
            localStorage.setItem(CURRENT_WORLD_KEY, currentPlayerWorld.value.id);
          }
        } else if (playerWorlds.value.length > 0) {
          currentPlayerWorld.value = playerWorlds.value[0];
          localStorage.setItem(CURRENT_WORLD_KEY, currentPlayerWorld.value.id);
        }

        return true;
      }
      error.value = response.error ?? 'Failed to load your worlds';
      return false;
    } catch {
      error.value = 'Failed to load your worlds';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  async function joinWorld(worldId: string): Promise<PlayerWorldResponse | null> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.worlds.join(worldId);
      if (response.data) {
        playerWorlds.value.push(response.data);
        setCurrentWorld(response.data);
        return response.data;
      }
      error.value = response.error ?? 'Failed to join world';
      return null;
    } catch {
      error.value = 'Failed to join world';
      return null;
    } finally {
      isLoading.value = false;
    }
  }

  async function setWorldHomeAirport(airportId: number): Promise<PlayerWorldResponse | null> {
    if (!currentPlayerWorld.value) {
      error.value = 'No world selected';
      return null;
    }

    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.worlds.setHomeAirport(currentPlayerWorld.value.id, airportId);
      if (response.data) {
        // Update the player world in the list
        const index = playerWorlds.value.findIndex(pw => pw.id === response.data!.id);
        if (index !== -1) {
          playerWorlds.value[index] = response.data;
        }
        currentPlayerWorld.value = response.data;
        return response.data;
      }
      error.value = response.error ?? 'Failed to set home airport';
      return null;
    } catch {
      error.value = 'Failed to set home airport';
      return null;
    } finally {
      isLoading.value = false;
    }
  }

  function setCurrentWorld(playerWorld: PlayerWorldResponse) {
    currentPlayerWorld.value = playerWorld;
    localStorage.setItem(CURRENT_WORLD_KEY, playerWorld.id);
  }

  function clearWorlds() {
    worlds.value = [];
    playerWorlds.value = [];
    currentPlayerWorld.value = null;
    localStorage.removeItem(CURRENT_WORLD_KEY);
  }

  function updateCurrentPlayerWorld(updates: Partial<PlayerWorldResponse>) {
    if (currentPlayerWorld.value) {
      currentPlayerWorld.value = { ...currentPlayerWorld.value, ...updates };
      const index = playerWorlds.value.findIndex(pw => pw.id === currentPlayerWorld.value!.id);
      if (index !== -1) {
        playerWorlds.value[index] = currentPlayerWorld.value;
      }
    }
  }

  return {
    worlds,
    playerWorlds,
    currentPlayerWorld,
    currentWorld,
    isLoading,
    error,
    hasJoinedWorlds,
    loadWorlds,
    loadMyWorlds,
    joinWorld,
    setWorldHomeAirport,
    setCurrentWorld,
    clearWorlds,
    updateCurrentPlayerWorld,
  };
}
