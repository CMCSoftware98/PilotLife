import { ref, computed } from 'vue';
import type {
  LicenseTypeResponse,
  UserLicenseResponse,
  LicenseShopResponse,
  LicenseShopItemResponse,
  LicenseExamResponse,
  ScheduleExamRequest
} from '../services/api';
import { api } from '../services/api';

const licenseTypes = ref<LicenseTypeResponse[]>([]);
const myLicenses = ref<UserLicenseResponse[]>([]);
const shopData = ref<LicenseShopResponse | null>(null);
const examHistory = ref<LicenseExamResponse[]>([]);
const activeExam = ref<LicenseExamResponse | null>(null);
const isLoading = ref(false);
const error = ref<string | null>(null);

export function useLicenseStore() {
  // Computed properties
  const validLicenses = computed(() =>
    myLicenses.value.filter(l => l.isValid && !l.isRevoked)
  );

  const expiredLicenses = computed(() =>
    myLicenses.value.filter(l => l.isExpired && !l.isRevoked)
  );

  const expiringLicenses = computed(() =>
    myLicenses.value.filter(l =>
      l.isValid &&
      !l.isRevoked &&
      l.daysUntilExpiry !== undefined &&
      l.daysUntilExpiry <= 30
    )
  );

  const availableLicenses = computed(() =>
    shopData.value?.licenseTypes.filter(item => item.canPurchase && !item.isOwned) ?? []
  );

  const coreLicenses = computed(() =>
    shopData.value?.licenseTypes.filter(item => item.licenseType.category === 'Core') ?? []
  );

  const endorsements = computed(() =>
    shopData.value?.licenseTypes.filter(item => item.licenseType.category === 'Endorsement') ?? []
  );

  const typeRatings = computed(() =>
    shopData.value?.licenseTypes.filter(item => item.licenseType.category === 'TypeRating') ?? []
  );

  const certifications = computed(() =>
    shopData.value?.licenseTypes.filter(item => item.licenseType.category === 'Certification') ?? []
  );

  const hasActiveExam = computed(() => activeExam.value !== null);

  // Actions
  async function loadLicenseTypes(): Promise<boolean> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.licenses.getTypes();
      if (response.data) {
        licenseTypes.value = response.data;
        return true;
      }
      error.value = response.error ?? 'Failed to load license types';
      return false;
    } catch {
      error.value = 'Failed to load license types';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  async function loadMyLicenses(worldId: string): Promise<boolean> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.licenses.getMyLicenses(worldId);
      if (response.data) {
        myLicenses.value = response.data;
        return true;
      }
      error.value = response.error ?? 'Failed to load licenses';
      return false;
    } catch {
      error.value = 'Failed to load licenses';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  async function loadShop(worldId: string): Promise<boolean> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.licenses.getShop(worldId);
      if (response.data) {
        shopData.value = response.data;
        return true;
      }
      error.value = response.error ?? 'Failed to load license shop';
      return false;
    } catch {
      error.value = 'Failed to load license shop';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  async function renewLicense(worldId: string, licenseId: string): Promise<UserLicenseResponse | null> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.licenses.renew(worldId, licenseId);
      if (response.data) {
        // Update the license in our local state
        const index = myLicenses.value.findIndex(l => l.id === licenseId);
        if (index !== -1) {
          myLicenses.value[index] = response.data;
        }
        return response.data;
      }
      error.value = response.error ?? 'Failed to renew license';
      return null;
    } catch {
      error.value = 'Failed to renew license';
      return null;
    } finally {
      isLoading.value = false;
    }
  }

  async function hasLicense(worldId: string, licenseCode: string): Promise<boolean> {
    try {
      const response = await api.licenses.checkLicense(worldId, licenseCode);
      return response.data?.hasLicense && response.data?.isValid || false;
    } catch {
      return false;
    }
  }

  // Exam functions
  async function loadExamHistory(worldId: string): Promise<boolean> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.exams.getHistory(worldId);
      if (response.data) {
        examHistory.value = response.data;
        return true;
      }
      error.value = response.error ?? 'Failed to load exam history';
      return false;
    } catch {
      error.value = 'Failed to load exam history';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  async function loadActiveExam(worldId: string): Promise<boolean> {
    try {
      const response = await api.exams.getActive(worldId);
      if (response.data) {
        activeExam.value = response.data;
        return true;
      }
      activeExam.value = null;
      return false;
    } catch {
      activeExam.value = null;
      return false;
    }
  }

  async function scheduleExam(data: ScheduleExamRequest): Promise<LicenseExamResponse | null> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.exams.schedule(data);
      if (response.data) {
        activeExam.value = response.data;
        return response.data;
      }
      error.value = response.error ?? 'Failed to schedule exam';
      return null;
    } catch {
      error.value = 'Failed to schedule exam';
      return null;
    } finally {
      isLoading.value = false;
    }
  }

  async function startExam(examId: string): Promise<LicenseExamResponse | null> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.exams.start(examId);
      if (response.data) {
        activeExam.value = response.data;
        return response.data;
      }
      error.value = response.error ?? 'Failed to start exam';
      return null;
    } catch {
      error.value = 'Failed to start exam';
      return null;
    } finally {
      isLoading.value = false;
    }
  }

  async function abandonExam(examId: string): Promise<boolean> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.exams.abandon(examId);
      if (response.data) {
        activeExam.value = null;
        return true;
      }
      error.value = response.error ?? 'Failed to abandon exam';
      return false;
    } catch {
      error.value = 'Failed to abandon exam';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  function getLicenseByCode(code: string): UserLicenseResponse | undefined {
    return myLicenses.value.find(l => l.licenseType?.code === code);
  }

  function getShopItemByCode(code: string): LicenseShopItemResponse | undefined {
    return shopData.value?.licenseTypes.find(item => item.licenseType.code === code);
  }

  function clearData() {
    licenseTypes.value = [];
    myLicenses.value = [];
    shopData.value = null;
    examHistory.value = [];
    activeExam.value = null;
    error.value = null;
  }

  return {
    // State
    licenseTypes,
    myLicenses,
    shopData,
    examHistory,
    activeExam,
    isLoading,
    error,
    // Computed
    validLicenses,
    expiredLicenses,
    expiringLicenses,
    availableLicenses,
    coreLicenses,
    endorsements,
    typeRatings,
    certifications,
    hasActiveExam,
    // Actions
    loadLicenseTypes,
    loadMyLicenses,
    loadShop,
    renewLicense,
    hasLicense,
    loadExamHistory,
    loadActiveExam,
    scheduleExam,
    startExam,
    abandonExam,
    getLicenseByCode,
    getShopItemByCode,
    clearData,
  };
}
