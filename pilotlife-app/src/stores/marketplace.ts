import { ref, computed } from 'vue';
import type {
  DealerResponse,
  DealerInventoryResponse,
  OwnedAircraftResponse,
  PurchaseAircraftRequest,
  CargoTypeResponse
} from '../services/api';
import { api } from '../services/api';

const dealers = ref<DealerResponse[]>([]);
const inventory = ref<DealerInventoryResponse[]>([]);
const ownedAircraft = ref<OwnedAircraftResponse[]>([]);
const cargoTypes = ref<CargoTypeResponse[]>([]);
const selectedDealer = ref<DealerResponse | null>(null);
const selectedInventoryItem = ref<DealerInventoryResponse | null>(null);
const isLoading = ref(false);
const error = ref<string | null>(null);

export function useMarketplaceStore() {
  const hasOwnedAircraft = computed(() => ownedAircraft.value.length > 0);

  const airworthyAircraft = computed(() =>
    ownedAircraft.value.filter(a => a.isAirworthy && !a.isInMaintenance)
  );

  const availableInventory = computed(() =>
    inventory.value.filter(i => !selectedDealer.value || i.dealerId === selectedDealer.value.id)
  );

  const dealersByType = computed(() => {
    const grouped: Record<string, DealerResponse[]> = {};
    for (const dealer of dealers.value) {
      if (!grouped[dealer.dealerType]) {
        grouped[dealer.dealerType] = [];
      }
      grouped[dealer.dealerType].push(dealer);
    }
    return grouped;
  });

  async function loadDealers(airportIcao?: string): Promise<boolean> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.marketplace.getDealers({ airportIcao });
      if (response.data) {
        dealers.value = response.data;
        return true;
      }
      error.value = response.error ?? 'Failed to load dealers';
      return false;
    } catch {
      error.value = 'Failed to load dealers';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  async function loadInventory(params?: {
    dealerId?: string;
    aircraftId?: string;
    minCondition?: number;
    maxPrice?: number
  }): Promise<boolean> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.marketplace.getInventory(params);
      if (response.data) {
        inventory.value = response.data;
        return true;
      }
      error.value = response.error ?? 'Failed to load inventory';
      return false;
    } catch {
      error.value = 'Failed to load inventory';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  async function loadMyAircraft(worldId?: string): Promise<boolean> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.hangar.getMyAircraft({ worldId });
      if (response.data) {
        ownedAircraft.value = response.data;
        return true;
      }
      error.value = response.error ?? 'Failed to load your aircraft';
      return false;
    } catch {
      error.value = 'Failed to load your aircraft';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  async function loadCargoTypes(category?: string): Promise<boolean> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.cargo.getTypes({ category, activeOnly: true });
      if (response.data) {
        cargoTypes.value = response.data;
        return true;
      }
      error.value = response.error ?? 'Failed to load cargo types';
      return false;
    } catch {
      error.value = 'Failed to load cargo types';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  async function purchaseAircraft(request: PurchaseAircraftRequest): Promise<OwnedAircraftResponse | null> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.marketplace.purchase(request);
      if (response.data?.success && response.data.ownedAircraft) {
        // Add to owned aircraft list
        ownedAircraft.value.push(response.data.ownedAircraft);

        // Remove from inventory if it was there
        const inventoryIndex = inventory.value.findIndex(i => i.id === request.inventoryId);
        if (inventoryIndex !== -1) {
          inventory.value.splice(inventoryIndex, 1);
        }

        return response.data.ownedAircraft;
      }
      error.value = response.data?.message ?? response.error ?? 'Failed to purchase aircraft';
      return null;
    } catch {
      error.value = 'Failed to purchase aircraft';
      return null;
    } finally {
      isLoading.value = false;
    }
  }

  async function updateAircraftNickname(id: string, nickname: string): Promise<OwnedAircraftResponse | null> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.hangar.updateNickname(id, nickname);
      if (response.data) {
        // Update in the local list
        const index = ownedAircraft.value.findIndex(a => a.id === id);
        if (index !== -1) {
          ownedAircraft.value[index] = response.data;
        }
        return response.data;
      }
      error.value = response.error ?? 'Failed to update nickname';
      return null;
    } catch {
      error.value = 'Failed to update nickname';
      return null;
    } finally {
      isLoading.value = false;
    }
  }

  async function listAircraftForSale(id: string, askingPrice: number): Promise<OwnedAircraftResponse | null> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.hangar.listForSale(id, askingPrice);
      if (response.data) {
        const index = ownedAircraft.value.findIndex(a => a.id === id);
        if (index !== -1) {
          ownedAircraft.value[index] = response.data;
        }
        return response.data;
      }
      error.value = response.error ?? 'Failed to list for sale';
      return null;
    } catch {
      error.value = 'Failed to list for sale';
      return null;
    } finally {
      isLoading.value = false;
    }
  }

  async function cancelAircraftSale(id: string): Promise<OwnedAircraftResponse | null> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.hangar.cancelSale(id);
      if (response.data) {
        const index = ownedAircraft.value.findIndex(a => a.id === id);
        if (index !== -1) {
          ownedAircraft.value[index] = response.data;
        }
        return response.data;
      }
      error.value = response.error ?? 'Failed to cancel sale';
      return null;
    } catch {
      error.value = 'Failed to cancel sale';
      return null;
    } finally {
      isLoading.value = false;
    }
  }

  function selectDealer(dealer: DealerResponse | null) {
    selectedDealer.value = dealer;
  }

  function selectInventoryItem(item: DealerInventoryResponse | null) {
    selectedInventoryItem.value = item;
  }

  function clearMarketplace() {
    dealers.value = [];
    inventory.value = [];
    ownedAircraft.value = [];
    cargoTypes.value = [];
    selectedDealer.value = null;
    selectedInventoryItem.value = null;
    error.value = null;
  }

  function getAircraftById(id: string): OwnedAircraftResponse | undefined {
    return ownedAircraft.value.find(a => a.id === id);
  }

  function getAircraftAtLocation(icao: string): OwnedAircraftResponse[] {
    return ownedAircraft.value.filter(a => a.currentLocationIcao === icao);
  }

  return {
    // State
    dealers,
    inventory,
    ownedAircraft,
    cargoTypes,
    selectedDealer,
    selectedInventoryItem,
    isLoading,
    error,

    // Computed
    hasOwnedAircraft,
    airworthyAircraft,
    availableInventory,
    dealersByType,

    // Actions
    loadDealers,
    loadInventory,
    loadMyAircraft,
    loadCargoTypes,
    purchaseAircraft,
    updateAircraftNickname,
    listAircraftForSale,
    cancelAircraftSale,
    selectDealer,
    selectInventoryItem,
    clearMarketplace,
    getAircraftById,
    getAircraftAtLocation,
  };
}
