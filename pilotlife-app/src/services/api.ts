const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:5001';

interface ApiResponse<T> {
  data?: T;
  error?: string;
}

interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  experienceLevel?: string;
  newsletterSubscribed: boolean;
}

interface LoginRequest {
  email: string;
  password: string;
}

interface AirportResponse {
  id: number;
  ident: string;
  name: string;
  iataCode?: string;
  latitude: number;
  longitude: number;
}

interface UserResponse {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  experienceLevel?: string;
  balance: number;
  totalFlightMinutes: number;
  currentAirportId?: number;
  currentAirport?: AirportResponse;
  homeAirportId?: number;
  homeAirport?: AirportResponse;
}

interface AuthResponse {
  user: UserResponse;
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  experienceLevel?: string;
  currentAirportId?: number;
  currentAirport?: Airport;
  homeAirportId?: number;
  homeAirport?: Airport;
  balance: number;
  totalFlightMinutes: number;
}

interface Airport {
  id: number;
  ident: string;
  name: string;
  iataCode?: string;
  type: string;
  latitude: number;
  longitude: number;
  elevationFt?: number;
  country?: string;
  municipality?: string;
}

interface AirportListResponse {
  airports: Airport[];
  totalCount: number;
  page: number;
  pageSize: number;
}

interface JobAirport {
  id: number;
  ident: string;
  name: string;
  iataCode?: string;
  latitude: number;
  longitude: number;
}

interface Job {
  id: string;
  departureAirport: JobAirport;
  arrivalAirport: JobAirport;
  cargoType: string;
  weight: number;
  payout: number;
  distanceNm: number;
  estimatedFlightTimeMinutes: number;
  requiredAircraftType: string;
  expiresAt: string;
  // Enhanced fields
  type?: 'Cargo' | 'Passenger';
  status?: 'Available' | 'Accepted' | 'InProgress' | 'Completed' | 'Failed' | 'Cancelled' | 'Expired';
  urgency?: 'Standard' | 'Priority' | 'Express' | 'Urgent' | 'Critical';
  distanceCategory?: 'VeryShort' | 'Short' | 'Medium' | 'Long' | 'UltraLong';
  passengerCount?: number;
  passengerClass?: 'Economy' | 'Business' | 'First' | 'Charter' | 'Medical' | 'Vip';
  riskLevel?: number;
  title?: string;
  description?: string;
}

// Marketplace types
interface DealerAirport {
  icao: string;
  name: string;
  latitude: number;
  longitude: number;
}

interface DealerResponse {
  id: string;
  airportIcao: string;
  airport?: DealerAirport;
  dealerType: 'ManufacturerShowroom' | 'CertifiedPreOwned' | 'RegionalDealer' | 'BudgetLot' | 'SpecialtyDealer' | 'ExecutiveDealer' | 'CargoSpecialist' | 'FlightSchool';
  name: string;
  description?: string;
  priceMultiplier: number;
  offersFinancing: boolean;
  financingDownPaymentPercent?: number;
  financingInterestRate?: number;
  minCondition: number;
  maxCondition: number;
  reputationScore: number;
  isActive: boolean;
}

interface MarketplaceSearchParams {
  aircraftType?: string;
  maxDistance?: number;
  maxPrice?: number;
  minCondition?: number;
  limit?: number;
}

interface MarketplaceSearchResult {
  inventory: DealerInventoryResponse[];
  totalCount: number;
  searchedAirports: number;
}

interface DealerInventoryResponse {
  id: string;
  dealerId: string;
  dealer?: DealerResponse;
  aircraftId: string;
  aircraft?: AircraftResponse;
  registration?: string;
  condition: number;
  totalFlightMinutes: number;
  totalFlightHours: number;
  basePrice: number;
  listPrice: number;
  discountPercent: number;
  isNew: boolean;
  hasWarranty: boolean;
  warrantyMonths?: number;
  avionicsPackage?: string;
  notes?: string;
  listedAt: string;
}

interface OwnedAircraftResponse {
  id: string;
  worldId: string;
  playerWorldId: string;
  aircraftId: string;
  aircraft?: AircraftResponse;
  registration: string;
  nickname?: string;
  condition: number;
  totalFlightMinutes: number;
  totalFlightHours: number;
  totalCycles: number;
  hoursSinceLastInspection: number;
  currentLocationIcao: string;
  isAirworthy: boolean;
  isInMaintenance: boolean;
  isInUse: boolean;
  isListedForSale: boolean;
  hasWarranty: boolean;
  warrantyExpiresAt?: string;
  hasInsurance: boolean;
  insuranceExpiresAt?: string;
  purchasePrice: number;
  purchasedAt: string;
  estimatedValue: number;
}

interface PurchaseAircraftRequest {
  inventoryId: string;
  useFinancing?: boolean;
  downPayment?: number;
  loanTermMonths?: number;
}

interface PurchaseAircraftResponse {
  success: boolean;
  message: string;
  ownedAircraft?: OwnedAircraftResponse;
  newBalance: number;
}

// Cargo types
interface CargoTypeResponse {
  id: string;
  category: 'GeneralCargo' | 'Perishable' | 'Hazardous' | 'LiveAnimals' | 'Oversized' | 'Medical' | 'HighValue' | 'Fragile' | 'Mail' | 'Parcels';
  subcategory: string;
  name: string;
  description?: string;
  baseRatePerLb: number;
  requiresSpecialHandling: boolean;
  specialHandlingType?: string;
  isTemperatureSensitive: boolean;
  isTimeCritical: boolean;
}

interface AcceptJobResponse extends Job {}

interface CompleteJobResponse {
  message: string;
  payout: number;
  newBalance: number;
  newLocation: string;
}

interface CreateAircraftRequestData {
  aircraftTitle: string;
  atcType?: string;
  atcModel?: string;
  category?: string;
  engineType: number;
  engineTypeStr?: string;
  numberOfEngines: number;
  maxGrossWeightLbs: number;
  emptyWeightLbs: number;
  cruiseSpeedKts: number;
  simulatorVersion?: string;
}

interface AircraftRequestResponse {
  id: string;
  aircraftTitle: string;
  atcType?: string;
  atcModel?: string;
  category?: string;
  engineType: number;
  engineTypeStr?: string;
  numberOfEngines: number;
  maxGrossWeightLbs: number;
  emptyWeightLbs: number;
  cruiseSpeedKts: number;
  simulatorVersion?: string;
  status: string;
  reviewNotes?: string;
  requestedByUserName?: string;
  createdAt: string;
  reviewedAt?: string;
}

interface AircraftResponse {
  id: string;
  title: string;
  atcType?: string;
  atcModel?: string;
  category?: string;
  engineType: number;
  engineTypeStr?: string;
  numberOfEngines: number;
  maxGrossWeightLbs: number;
  emptyWeightLbs: number;
  cruiseSpeedKts: number;
  simulatorVersion?: string;
  isApproved: boolean;
  createdAt: string;
  updatedAt?: string;
}

// Token management
const TOKEN_KEY = 'pilotlife_access_token';
const REFRESH_TOKEN_KEY = 'pilotlife_refresh_token';
const TOKEN_EXPIRY_KEY = 'pilotlife_token_expiry';

function getAccessToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

function getRefreshToken(): string | null {
  return localStorage.getItem(REFRESH_TOKEN_KEY);
}

function setTokens(accessToken: string, refreshToken: string, expiresAt: string): void {
  localStorage.setItem(TOKEN_KEY, accessToken);
  localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
  localStorage.setItem(TOKEN_EXPIRY_KEY, expiresAt);
}

function clearTokens(): void {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
  localStorage.removeItem(TOKEN_EXPIRY_KEY);
}

function isTokenExpired(): boolean {
  const expiry = localStorage.getItem(TOKEN_EXPIRY_KEY);
  if (!expiry) return true;
  return new Date(expiry) <= new Date();
}

async function refreshAccessToken(): Promise<boolean> {
  const refreshToken = getRefreshToken();
  if (!refreshToken) return false;

  try {
    const response = await fetch(`${API_BASE_URL}/api/auth/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken }),
    });

    if (!response.ok) {
      clearTokens();
      return false;
    }

    const data: AuthResponse = await response.json();
    setTokens(data.accessToken, data.refreshToken, data.expiresAt);
    return true;
  } catch {
    clearTokens();
    return false;
  }
}

async function request<T>(
  endpoint: string,
  options: RequestInit = {},
  requiresAuth = false
): Promise<ApiResponse<T>> {
  try {
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      ...(options.headers as Record<string, string>),
    };

    if (requiresAuth) {
      // Check if token needs refresh
      if (isTokenExpired()) {
        const refreshed = await refreshAccessToken();
        if (!refreshed) {
          return { error: 'Session expired. Please log in again.' };
        }
      }

      const token = getAccessToken();
      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }
    }

    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      ...options,
      headers,
    });

    // Handle 401 by trying to refresh token once
    if (response.status === 401 && requiresAuth) {
      const refreshed = await refreshAccessToken();
      if (refreshed) {
        headers['Authorization'] = `Bearer ${getAccessToken()}`;
        const retryResponse = await fetch(`${API_BASE_URL}${endpoint}`, {
          ...options,
          headers,
        });
        const data = await retryResponse.json();
        if (!retryResponse.ok) {
          return { error: data.message || 'An error occurred' };
        }
        return { data };
      }
      return { error: 'Session expired. Please log in again.' };
    }

    const data = await response.json();

    if (!response.ok) {
      return { error: data.message || 'An error occurred' };
    }

    return { data };
  } catch (error) {
    console.error('API Error:', error);
    return { error: 'Unable to connect to server. Please try again.' };
  }
}

export const api = {
  auth: {
    register: async (data: RegisterRequest): Promise<ApiResponse<AuthResponse>> => {
      const response = await request<AuthResponse>('/api/auth/register', {
        method: 'POST',
        body: JSON.stringify(data),
      });
      if (response.data) {
        setTokens(response.data.accessToken, response.data.refreshToken, response.data.expiresAt);
      }
      return response;
    },

    login: async (data: LoginRequest): Promise<ApiResponse<AuthResponse>> => {
      const response = await request<AuthResponse>('/api/auth/login', {
        method: 'POST',
        body: JSON.stringify(data),
      });
      if (response.data) {
        setTokens(response.data.accessToken, response.data.refreshToken, response.data.expiresAt);
      }
      return response;
    },

    logout: async (): Promise<ApiResponse<{ message: string }>> => {
      const refreshToken = getRefreshToken();
      if (refreshToken) {
        await request<{ message: string }>('/api/auth/logout', {
          method: 'POST',
          body: JSON.stringify({ refreshToken }),
        }, true);
      }
      clearTokens();
      return { data: { message: 'Logged out successfully' } };
    },

    me: (): Promise<ApiResponse<UserResponse>> =>
      request<UserResponse>('/api/auth/me', {}, true),

    refresh: async (): Promise<ApiResponse<AuthResponse>> => {
      const refreshToken = getRefreshToken();
      if (!refreshToken) {
        return { error: 'No refresh token available' };
      }
      const response = await request<AuthResponse>('/api/auth/refresh', {
        method: 'POST',
        body: JSON.stringify({ refreshToken }),
      });
      if (response.data) {
        setTokens(response.data.accessToken, response.data.refreshToken, response.data.expiresAt);
      }
      return response;
    },

    health: (): Promise<ApiResponse<{ status: string; timestamp: string }>> =>
      request('/api/auth/health'),

    setHomeAirport: (airportId: number): Promise<ApiResponse<UserResponse>> =>
      request<UserResponse>('/api/auth/set-home-airport', {
        method: 'POST',
        body: JSON.stringify({ airportId }),
      }, true),

    isAuthenticated: (): boolean => {
      return getAccessToken() !== null && !isTokenExpired();
    },

    getStoredTokens: () => ({
      accessToken: getAccessToken(),
      refreshToken: getRefreshToken(),
    }),

    clearSession: clearTokens,
  },

  airports: {
    list: (params?: { search?: string; type?: string; page?: number; pageSize?: number }): Promise<ApiResponse<AirportListResponse>> => {
      const searchParams = new URLSearchParams();
      if (params?.search) searchParams.set('search', params.search);
      if (params?.type) searchParams.set('type', params.type);
      if (params?.page) searchParams.set('page', params.page.toString());
      if (params?.pageSize) searchParams.set('pageSize', params.pageSize.toString());
      const query = searchParams.toString();
      return request<AirportListResponse>(`/api/airports${query ? `?${query}` : ''}`, {}, true);
    },

    get: (id: number): Promise<ApiResponse<Airport>> =>
      request<Airport>(`/api/airports/${id}`, {}, true),

    search: (q: string, limit?: number): Promise<ApiResponse<Airport[]>> => {
      const params = new URLSearchParams({ q });
      if (limit) params.set('limit', limit.toString());
      return request<Airport[]>(`/api/airports/search?${params}`, {}, true);
    },

    getByIdent: (ident: string): Promise<ApiResponse<Airport>> =>
      request<Airport>(`/api/airports/by-ident/${ident}`, {}, true),
  },

  jobs: {
    getAvailable: (params: { airportId: number; cargoType?: string; aircraftType?: string; maxDistance?: number }): Promise<ApiResponse<Job[]>> => {
      const searchParams = new URLSearchParams();
      searchParams.set('airportId', params.airportId.toString());
      if (params.cargoType) searchParams.set('cargoType', params.cargoType);
      if (params.aircraftType) searchParams.set('aircraftType', params.aircraftType);
      if (params.maxDistance) searchParams.set('maxDistance', params.maxDistance.toString());
      return request<Job[]>(`/api/jobs/available?${searchParams}`, {}, true);
    },

    get: (id: string): Promise<ApiResponse<Job>> =>
      request<Job>(`/api/jobs/${id}`, {}, true),

    accept: (jobId: string, userId: string): Promise<ApiResponse<AcceptJobResponse>> =>
      request<AcceptJobResponse>(`/api/jobs/${jobId}/accept`, {
        method: 'POST',
        body: JSON.stringify({ userId }),
      }, true),

    complete: (jobId: string, userId: string): Promise<ApiResponse<CompleteJobResponse>> =>
      request<CompleteJobResponse>(`/api/jobs/${jobId}/complete`, {
        method: 'POST',
        body: JSON.stringify({ userId }),
      }, true),
  },

  aircraftRequests: {
    list: (status?: string): Promise<ApiResponse<AircraftRequestResponse[]>> => {
      const params = status ? `?status=${status}` : '';
      return request<AircraftRequestResponse[]>(`/api/aircraftrequests${params}`, {}, true);
    },

    get: (id: string): Promise<ApiResponse<AircraftRequestResponse>> =>
      request<AircraftRequestResponse>(`/api/aircraftrequests/${id}`, {}, true),

    create: (data: CreateAircraftRequestData): Promise<ApiResponse<AircraftRequestResponse>> =>
      request<AircraftRequestResponse>('/api/aircraftrequests', {
        method: 'POST',
        body: JSON.stringify(data),
      }, true),

    approve: (id: string, reviewNotes?: string): Promise<ApiResponse<AircraftRequestResponse>> =>
      request<AircraftRequestResponse>(`/api/aircraftrequests/${id}/approve`, {
        method: 'POST',
        body: JSON.stringify({ reviewNotes }),
      }, true),

    reject: (id: string, reviewNotes?: string): Promise<ApiResponse<AircraftRequestResponse>> =>
      request<AircraftRequestResponse>(`/api/aircraftrequests/${id}/reject`, {
        method: 'POST',
        body: JSON.stringify({ reviewNotes }),
      }, true),

    delete: (id: string): Promise<ApiResponse<void>> =>
      request<void>(`/api/aircraftrequests/${id}`, {
        method: 'DELETE',
      }, true),
  },

  aircraft: {
    list: (approvedOnly?: boolean): Promise<ApiResponse<AircraftResponse[]>> => {
      const params = approvedOnly !== undefined ? `?approvedOnly=${approvedOnly}` : '';
      return request<AircraftResponse[]>(`/api/aircraft${params}`, {}, true);
    },

    get: (id: string): Promise<ApiResponse<AircraftResponse>> =>
      request<AircraftResponse>(`/api/aircraft/${id}`, {}, true),

    create: (data: Omit<AircraftResponse, 'id' | 'createdAt' | 'updatedAt'>): Promise<ApiResponse<AircraftResponse>> =>
      request<AircraftResponse>('/api/aircraft', {
        method: 'POST',
        body: JSON.stringify(data),
      }, true),

    update: (id: string, data: Omit<AircraftResponse, 'id' | 'createdAt' | 'updatedAt'>): Promise<ApiResponse<AircraftResponse>> =>
      request<AircraftResponse>(`/api/aircraft/${id}`, {
        method: 'PUT',
        body: JSON.stringify(data),
      }, true),

    delete: (id: string): Promise<ApiResponse<void>> =>
      request<void>(`/api/aircraft/${id}`, {
        method: 'DELETE',
      }, true),
  },

  worlds: {
    list: (): Promise<ApiResponse<WorldResponse[]>> =>
      request<WorldResponse[]>('/api/worlds', {}, true),

    get: (id: string): Promise<ApiResponse<WorldResponse>> =>
      request<WorldResponse>(`/api/worlds/${id}`, {}, true),

    getBySlug: (slug: string): Promise<ApiResponse<WorldResponse>> =>
      request<WorldResponse>(`/api/worlds/by-slug/${slug}`, {}, true),

    getMyWorlds: (): Promise<ApiResponse<PlayerWorldResponse[]>> =>
      request<PlayerWorldResponse[]>('/api/worlds/my-worlds', {}, true),

    join: (worldId: string): Promise<ApiResponse<PlayerWorldResponse>> =>
      request<PlayerWorldResponse>(`/api/worlds/${worldId}/join`, {
        method: 'POST',
      }, true),

    setHomeAirport: (playerWorldId: string, airportId: number): Promise<ApiResponse<PlayerWorldResponse>> =>
      request<PlayerWorldResponse>(`/api/worlds/my-worlds/${playerWorldId}/set-home-airport`, {
        method: 'POST',
        body: JSON.stringify({ airportId }),
      }, true),
  },

  marketplace: {
    getDealers: (params?: { airportIcao?: string; dealerType?: string }): Promise<ApiResponse<DealerResponse[]>> => {
      const searchParams = new URLSearchParams();
      if (params?.airportIcao) searchParams.set('airportIcao', params.airportIcao);
      if (params?.dealerType) searchParams.set('dealerType', params.dealerType);
      const query = searchParams.toString();
      return request<DealerResponse[]>(`/api/marketplace/dealers${query ? `?${query}` : ''}`, {}, true);
    },

    getDealer: (id: string): Promise<ApiResponse<DealerResponse>> =>
      request<DealerResponse>(`/api/marketplace/dealers/${id}`, {}, true),

    getInventory: (params?: { dealerId?: string; aircraftId?: string; minCondition?: number; maxPrice?: number }): Promise<ApiResponse<DealerInventoryResponse[]>> => {
      const searchParams = new URLSearchParams();
      if (params?.dealerId) searchParams.set('dealerId', params.dealerId);
      if (params?.aircraftId) searchParams.set('aircraftId', params.aircraftId);
      if (params?.minCondition) searchParams.set('minCondition', params.minCondition.toString());
      if (params?.maxPrice) searchParams.set('maxPrice', params.maxPrice.toString());
      const query = searchParams.toString();
      return request<DealerInventoryResponse[]>(`/api/marketplace/inventory${query ? `?${query}` : ''}`, {}, true);
    },

    getInventoryItem: (id: string): Promise<ApiResponse<DealerInventoryResponse>> =>
      request<DealerInventoryResponse>(`/api/marketplace/inventory/${id}`, {}, true),

    purchase: (data: PurchaseAircraftRequest): Promise<ApiResponse<PurchaseAircraftResponse>> =>
      request<PurchaseAircraftResponse>('/api/marketplace/purchase', {
        method: 'POST',
        body: JSON.stringify(data),
      }, true),

    search: (params: MarketplaceSearchParams & { fromAirportIcao: string }): Promise<ApiResponse<MarketplaceSearchResult>> => {
      const searchParams = new URLSearchParams();
      searchParams.set('fromAirportIcao', params.fromAirportIcao);
      if (params.aircraftType) searchParams.set('aircraftType', params.aircraftType);
      if (params.maxDistance) searchParams.set('maxDistance', params.maxDistance.toString());
      if (params.maxPrice) searchParams.set('maxPrice', params.maxPrice.toString());
      if (params.minCondition) searchParams.set('minCondition', params.minCondition.toString());
      if (params.limit) searchParams.set('limit', params.limit.toString());
      return request<MarketplaceSearchResult>(`/api/marketplace/search?${searchParams}`, {}, true);
    },

    getLocalInventory: (airportIcao: string): Promise<ApiResponse<DealerInventoryResponse[]>> =>
      request<DealerInventoryResponse[]>(`/api/marketplace/inventory/local/${airportIcao}`, {}, true),
  },

  hangar: {
    getMyAircraft: (params?: { worldId?: string; locationIcao?: string }): Promise<ApiResponse<OwnedAircraftResponse[]>> => {
      const searchParams = new URLSearchParams();
      if (params?.worldId) searchParams.set('worldId', params.worldId);
      if (params?.locationIcao) searchParams.set('locationIcao', params.locationIcao);
      const query = searchParams.toString();
      return request<OwnedAircraftResponse[]>(`/api/hangar/my-aircraft${query ? `?${query}` : ''}`, {}, true);
    },

    getAircraft: (id: string): Promise<ApiResponse<OwnedAircraftResponse>> =>
      request<OwnedAircraftResponse>(`/api/hangar/${id}`, {}, true),

    updateNickname: (id: string, nickname: string): Promise<ApiResponse<OwnedAircraftResponse>> =>
      request<OwnedAircraftResponse>(`/api/hangar/${id}/nickname`, {
        method: 'PUT',
        body: JSON.stringify({ nickname }),
      }, true),

    listForSale: (id: string, askingPrice: number): Promise<ApiResponse<OwnedAircraftResponse>> =>
      request<OwnedAircraftResponse>(`/api/hangar/${id}/list-for-sale`, {
        method: 'POST',
        body: JSON.stringify({ askingPrice }),
      }, true),

    cancelSale: (id: string): Promise<ApiResponse<OwnedAircraftResponse>> =>
      request<OwnedAircraftResponse>(`/api/hangar/${id}/cancel-sale`, {
        method: 'POST',
      }, true),
  },

  cargo: {
    getTypes: (params?: { category?: string; activeOnly?: boolean }): Promise<ApiResponse<CargoTypeResponse[]>> => {
      const searchParams = new URLSearchParams();
      if (params?.category) searchParams.set('category', params.category);
      if (params?.activeOnly !== undefined) searchParams.set('activeOnly', params.activeOnly.toString());
      const query = searchParams.toString();
      return request<CargoTypeResponse[]>(`/api/cargo/types${query ? `?${query}` : ''}`, {}, true);
    },

    getType: (id: string): Promise<ApiResponse<CargoTypeResponse>> =>
      request<CargoTypeResponse>(`/api/cargo/types/${id}`, {}, true),
  },
};

// World types
interface WorldResponse {
  id: string;
  name: string;
  slug: string;
  description?: string;
  difficulty: string;
  startingCapital: number;
  jobPayoutMultiplier: number;
  aircraftPriceMultiplier: number;
  maintenanceCostMultiplier: number;
  isDefault: boolean;
  maxPlayers: number;
  currentPlayers: number;
}

interface PlayerWorldResponse {
  id: string;
  worldId: string;
  worldName: string;
  worldSlug: string;
  worldDifficulty: string;
  balance: number;
  creditScore: number;
  reputationScore: number;
  totalFlights: number;
  totalJobsCompleted: number;
  totalFlightMinutes: number;
  totalEarnings: number;
  currentAirportId?: number;
  currentAirportIdent?: string;
  homeAirportId?: number;
  homeAirportIdent?: string;
  joinedAt: string;
  lastActiveAt: string;
}

export type {
  User,
  Airport,
  AirportListResponse,
  Job,
  JobAirport,
  AcceptJobResponse,
  CompleteJobResponse,
  AuthResponse,
  UserResponse,
  RegisterRequest,
  LoginRequest,
  AircraftRequestResponse,
  AircraftResponse,
  CreateAircraftRequestData,
  WorldResponse,
  PlayerWorldResponse,
  DealerResponse,
  DealerAirport,
  DealerInventoryResponse,
  OwnedAircraftResponse,
  PurchaseAircraftRequest,
  PurchaseAircraftResponse,
  CargoTypeResponse,
  MarketplaceSearchParams,
  MarketplaceSearchResult
};
