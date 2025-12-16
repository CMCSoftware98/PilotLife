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
  worldId: string;
  departureAirport: JobAirport;
  arrivalAirport: JobAirport;
  cargoType: string;
  weight: number;
  weightLbs?: number;
  volumeCuFt?: number;
  basePayout?: number;
  payout: number;
  distanceNm: number;
  estimatedFlightTimeMinutes: number;
  requiredAircraftType: string;
  expiresAt: string;
  acceptedAt?: string;
  // Enhanced fields
  type?: 'Cargo' | 'Passenger';
  status?: 'Available' | 'Accepted' | 'InProgress' | 'Completed' | 'Failed' | 'Cancelled' | 'Expired';
  urgency?: 'Standard' | 'Priority' | 'Express' | 'Urgent' | 'Critical';
  distanceCategory?: 'VeryShort' | 'Short' | 'Medium' | 'Long' | 'UltraLong';
  passengerCount?: number;
  passengerClass?: 'Economy' | 'Business' | 'First' | 'Charter' | 'Medical' | 'Vip';
  riskLevel?: number;
  requiresSpecialCertification?: boolean;
  requiredCertification?: string;
  title?: string;
  description?: string;
}

interface JobSearchParams {
  worldId: string;
  departureAirportId?: number;
  departureIcao?: string;
  arrivalAirportId?: number;
  arrivalIcao?: string;
  jobType?: 'Cargo' | 'Passenger';
  urgency?: 'Standard' | 'Priority' | 'Express' | 'Urgent' | 'Critical';
  cargoType?: string;
  passengerClass?: 'Economy' | 'Business' | 'First' | 'Charter' | 'Medical' | 'Vip';
  minDistanceNm?: number;
  maxDistanceNm?: number;
  distanceCategory?: 'VeryShort' | 'Short' | 'Medium' | 'Long' | 'UltraLong';
  minPayout?: number;
  maxPayout?: number;
  minWeightLbs?: number;
  maxWeightLbs?: number;
  minPassengers?: number;
  maxPassengers?: number;
  requiresSpecialCertification?: boolean;
  sortBy?: 'payout' | 'distance' | 'weight' | 'expiry' | 'urgency';
  sortDescending?: boolean;
  page?: number;
  pageSize?: number;
  // Vicinity search
  centerLatitude?: number;
  centerLongitude?: number;
  vicinityRadiusNm?: number;
}

interface JobSearchResult {
  jobs: Job[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

interface JobStatsResult {
  availableJobs: number;
  averagePayout: number;
  byType: Record<string, number>;
  byUrgency: Record<string, number>;
  byDistanceCategory: Record<string, number>;
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

// Skills types
interface PlayerSkillResponse {
  skillId: string;
  skillType: string;
  skillName: string;
  description: string;
  currentXp: number;
  level: number;
  levelName: string;
  xpForNextLevel: number;
  xpForCurrentLevel: number;
  progressToNextLevel: number;
  isMaxLevel: boolean;
}

interface SkillXpEventResponse {
  id: string;
  skillType: string;
  xpGained: number;
  resultingXp: number;
  resultingLevel: number;
  causedLevelUp: boolean;
  source: string;
  description: string;
  occurredAt: string;
  relatedFlightId?: string;
  relatedJobId?: string;
}

// Reputation types
interface ReputationStatusResponse {
  playerWorldId: string;
  score: number;
  level: number;
  levelName: string;
  progressToNextLevel: number;
  onTimeDeliveries: number;
  lateDeliveries: number;
  failedDeliveries: number;
  jobCompletionRate: number;
  onTimeRate: number;
  payoutBonus: number;
  benefits: ReputationBenefitResponse[];
}

interface ReputationBenefitResponse {
  name: string;
  description: string;
  isUnlocked: boolean;
  requiredLevel: number;
}

interface ReputationEventResponse {
  id: string;
  eventType: string;
  pointChange: number;
  resultingScore: number;
  description: string;
  occurredAt: string;
  relatedJobId?: string;
  relatedFlightId?: string;
}

// License types
interface LicenseTypeResponse {
  id: string;
  code: string;
  name: string;
  description?: string;
  category: 'Core' | 'Endorsement' | 'TypeRating' | 'Certification';
  aircraftCategory?: 'SEP' | 'MEP' | 'Turboprop' | 'RegionalJet' | 'NarrowBody' | 'WideBody' | 'Helicopter';
  baseExamCost: number;
  renewalCost: number;
  examDurationMinutes: number;
  passingScore: number;
  validityGameDays: number;
  prerequisiteLicenses: string[];
  isActive: boolean;
  sortOrder: number;
}

interface UserLicenseResponse {
  id: string;
  playerWorldId: string;
  licenseTypeId: string;
  licenseType?: LicenseTypeResponse;
  earnedAt: string;
  expiresAt?: string;
  isValid: boolean;
  isExpired: boolean;
  isRevoked: boolean;
  examScore: number;
  examAttempts: number;
  renewalCount: number;
  lastRenewedAt?: string;
  daysUntilExpiry?: number;
}

interface LicenseShopResponse {
  licenseTypes: LicenseShopItemResponse[];
  worldCostMultiplier: number;
}

interface LicenseShopItemResponse {
  licenseType: LicenseTypeResponse;
  adjustedExamCost: number;
  adjustedRenewalCost: number;
  isOwned: boolean;
  isValid: boolean;
  canPurchase: boolean;
  missingPrerequisites: string[];
  userLicense?: UserLicenseResponse;
}

interface LicenseCheckResponse {
  hasLicense: boolean;
  isValid: boolean;
  license?: UserLicenseResponse;
}

interface LicenseExamResponse {
  id: string;
  playerWorldId: string;
  licenseTypeId: string;
  licenseType?: LicenseTypeResponse;
  status: 'Scheduled' | 'InProgress' | 'Passed' | 'Failed' | 'Abandoned' | 'Expired';
  scheduledAt: string;
  startedAt?: string;
  completedAt?: string;
  departureAirportIcao: string;
  arrivalAirportIcao?: string;
  routeWaypointsJson?: string;
  examCost: number;
  currentScore: number;
  finalScore?: number;
  passed: boolean;
  failureReason?: string;
  attemptNumber: number;
  violations: ExamViolationResponse[];
  landings: ExamLandingResponse[];
}

interface ExamViolationResponse {
  id: string;
  violationType: string;
  severity: 'Minor' | 'Major' | 'Critical';
  pointDeduction: number;
  description?: string;
  occurredAt: string;
  altitude?: number;
  airspeed?: number;
  latitude?: number;
  longitude?: number;
}

interface ExamLandingResponse {
  id: string;
  landingType: 'TouchAndGo' | 'FullStop';
  airportIcao: string;
  landingRateFpm: number;
  pointsAwarded: number;
  isSuccessful: boolean;
  notes?: string;
  occurredAt: string;
}

interface ScheduleExamRequest {
  playerWorldId: string;
  licenseTypeCode: string;
  departureAirportIcao: string;
  arrivalAirportIcao?: string;
}

interface RecordViolationRequest {
  violationType: string;
  severity: 'Minor' | 'Major' | 'Critical';
  pointDeduction: number;
  description?: string;
  altitude?: number;
  airspeed?: number;
  latitude?: number;
  longitude?: number;
}

interface RecordLandingRequest {
  landingType: 'TouchAndGo' | 'FullStop';
  airportIcao: string;
  landingRateFpm: number;
}

interface ExamCompletionResponse {
  exam: LicenseExamResponse;
  passed: boolean;
  finalScore: number;
  license?: UserLicenseResponse;
  message: string;
}

// Banking/Loan types
interface LoanResponse {
  id: string;
  loanType: 'StarterLoan' | 'AircraftFinancing' | 'Personal' | 'Business' | 'Emergency';
  status: 'Pending' | 'Active' | 'PaidOff' | 'Defaulted' | 'Rejected' | 'Cancelled';
  principalAmount: number;
  interestRatePerMonth: number;
  interestRatePercent: number;
  termMonths: number;
  monthlyPayment: number;
  totalRepaymentAmount: number;
  remainingPrincipal: number;
  totalPaid: number;
  paymentsMade: number;
  paymentsRemaining: number;
  nextPaymentDue?: string;
  approvedAt?: string;
  paidOffAt?: string;
  purpose?: string;
  bankName?: string;
  collateralAircraftRegistration?: string;
}

interface LoanDetailResponse extends LoanResponse {
  accruedInterest: number;
  latePaymentCount: number;
  missedPaymentCount: number;
  disbursedAt?: string;
  defaultedAt?: string;
  notes?: string;
  collateralAircraftTitle?: string;
  payoffAmount: number;
  payments: PaymentResponse[];
  createdAt: string;
}

interface LoanSummaryResponse {
  totalLoans: number;
  activeLoans: number;
  paidOffLoans: number;
  defaultedLoans: number;
  totalDebt: number;
  totalMonthlyPayment: number;
  nextPaymentDue?: string;
  hasStarterLoan: boolean;
}

interface StarterLoanEligibilityResponse {
  isEligible: boolean;
  reason?: string;
  requiresCpl: boolean;
  maxAmount?: number;
  interestRate?: number;
  interestRatePercent?: number;
  availableTerms?: number[];
  currentBalance?: number;
  currentNetWorth?: number;
  bankName?: string;
}

interface LoanApplicationResponse {
  success: boolean;
  message: string;
  loan?: LoanResponse;
  newBalance: number;
}

interface LoanPaymentResultResponse {
  success: boolean;
  message: string;
  payment?: PaymentResponse;
  loanPaidOff: boolean;
  newBalance: number;
}

interface PaymentResponse {
  id: string;
  paymentNumber: number;
  amount: number;
  principalPortion: number;
  interestPortion: number;
  lateFee: number;
  remainingBalanceAfter: number;
  dueDate: string;
  paidAt: string;
  isLate: boolean;
  daysLate: number;
}

interface CreditScoreResponse {
  score: number;
  rating: string;
  minPossible: number;
  maxPossible: number;
  activeLoans: number;
  totalDebt: number;
  paidOffLoans: number;
  defaultedLoans: number;
  onTimePaymentPercent: number;
  recentPositiveChanges: number;
  recentNegativeChanges: number;
  lastUpdated: string;
}

interface CreditScoreEventResponse {
  id: string;
  eventType: string;
  scoreBefore: number;
  scoreAfter: number;
  scoreChange: number;
  description: string;
  occurredAt: string;
}

interface BankResponse {
  id: string;
  name: string;
  description?: string;
  offersStarterLoan: boolean;
  starterLoanMaxAmount: number;
  starterLoanInterestRatePercent: number;
  baseInterestRatePercent: number;
  maxInterestRatePercent: number;
  minCreditScore: number;
  maxLoanToNetWorthRatio: number;
  minDownPaymentPercent: number;
  maxTermMonths: number;
}

interface StarterLoanApplicationRequest {
  amount: number;
  termMonths: number;
}

interface LoanApplicationRequest {
  loanType: string;
  amount: number;
  termMonths: number;
  purpose?: string;
  collateralAircraftId?: string;
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
  // Raw file contents
  manifestJsonRaw?: string;
  aircraftCfgRaw?: string;
  // Manifest fields
  manifestContentType?: string;
  manifestTitle?: string;
  manifestManufacturer?: string;
  manifestCreator?: string;
  manifestPackageVersion?: string;
  manifestMinimumGameVersion?: string;
  manifestTotalPackageSize?: string;
  manifestContentId?: string;
  // Aircraft.cfg [FLTSIM.0] fields
  cfgTitle?: string;
  cfgModel?: string;
  cfgPanel?: string;
  cfgSound?: string;
  cfgTexture?: string;
  cfgAtcType?: string;
  cfgAtcModel?: string;
  cfgAtcId?: string;
  cfgAtcAirline?: string;
  cfgUiManufacturer?: string;
  cfgUiType?: string;
  cfgUiVariation?: string;
  cfgIcaoAirline?: string;
  // Aircraft.cfg [GENERAL] fields
  cfgGeneralAtcType?: string;
  cfgGeneralAtcModel?: string;
  cfgEditable?: string;
  cfgPerformance?: string;
  cfgCategory?: string;
}

interface ManifestDataResponse {
  contentType?: string;
  title?: string;
  manufacturer?: string;
  creator?: string;
  packageVersion?: string;
  minimumGameVersion?: string;
  totalPackageSize?: string;
  contentId?: string;
}

interface AircraftCfgDataResponse {
  // [FLTSIM.0] section
  title?: string;
  model?: string;
  panel?: string;
  sound?: string;
  texture?: string;
  atcType?: string;
  atcModel?: string;
  atcId?: string;
  atcAirline?: string;
  uiManufacturer?: string;
  uiType?: string;
  uiVariation?: string;
  icaoAirline?: string;
  // [GENERAL] section
  generalAtcType?: string;
  generalAtcModel?: string;
  editable?: string;
  performance?: string;
  category?: string;
}

interface AircraftRequestCheckResponse {
  status: 'none' | 'pending' | 'approved' | 'rejected' | 'exists';
  requestId?: string;
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
  // File data
  hasFileData: boolean;
  manifestData?: ManifestDataResponse;
  aircraftCfgData?: AircraftCfgDataResponse;
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

// Callback for handling authentication failures globally
let onAuthFailureCallback: (() => void) | null = null;

function setOnAuthFailure(callback: () => void): void {
  onAuthFailureCallback = callback;
}

function handleAuthFailure(): void {
  clearTokens();
  localStorage.removeItem('pilotlife_user');
  localStorage.removeItem('pilotlife_current_world_id');
  if (onAuthFailureCallback) {
    onAuthFailureCallback();
  }
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
          handleAuthFailure();
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
        if (retryResponse.status === 401) {
          // Still getting 401 after refresh - user no longer exists or token is invalid
          handleAuthFailure();
          return { error: 'Session expired. Please log in again.' };
        }
        const data = await retryResponse.json();
        if (!retryResponse.ok) {
          return { error: data.message || 'An error occurred' };
        }
        return { data };
      }
      handleAuthFailure();
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

// Export the auth failure handler setter for router integration
export { setOnAuthFailure };

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

    getInBounds: (params: { north: number; south: number; east: number; west: number; zoomLevel?: number; limit?: number }): Promise<ApiResponse<Airport[]>> => {
      const searchParams = new URLSearchParams();
      searchParams.set('north', params.north.toString());
      searchParams.set('south', params.south.toString());
      searchParams.set('east', params.east.toString());
      searchParams.set('west', params.west.toString());
      if (params.zoomLevel) searchParams.set('zoomLevel', params.zoomLevel.toString());
      if (params.limit) searchParams.set('limit', params.limit.toString());
      return request<Airport[]>(`/api/airports/in-bounds?${searchParams}`, {}, true);
    },

    getNearby: (params: { latitude: number; longitude: number; radiusNm?: number; types?: string; limit?: number }): Promise<ApiResponse<Airport[]>> => {
      const searchParams = new URLSearchParams();
      searchParams.set('latitude', params.latitude.toString());
      searchParams.set('longitude', params.longitude.toString());
      if (params.radiusNm) searchParams.set('radiusNm', params.radiusNm.toString());
      if (params.types) searchParams.set('types', params.types);
      if (params.limit) searchParams.set('limit', params.limit.toString());
      return request<Airport[]>(`/api/airports/nearby?${searchParams}`, {}, true);
    },
  },

  jobs: {
    search: (params: JobSearchParams): Promise<ApiResponse<JobSearchResult>> => {
      const searchParams = new URLSearchParams();
      searchParams.set('worldId', params.worldId);
      if (params.departureAirportId) searchParams.set('departureAirportId', params.departureAirportId.toString());
      if (params.departureIcao) searchParams.set('departureIcao', params.departureIcao);
      if (params.arrivalAirportId) searchParams.set('arrivalAirportId', params.arrivalAirportId.toString());
      if (params.arrivalIcao) searchParams.set('arrivalIcao', params.arrivalIcao);
      if (params.jobType) searchParams.set('jobType', params.jobType);
      if (params.urgency) searchParams.set('urgency', params.urgency);
      if (params.cargoType) searchParams.set('cargoType', params.cargoType);
      if (params.passengerClass) searchParams.set('passengerClass', params.passengerClass);
      if (params.minDistanceNm) searchParams.set('minDistanceNm', params.minDistanceNm.toString());
      if (params.maxDistanceNm) searchParams.set('maxDistanceNm', params.maxDistanceNm.toString());
      if (params.distanceCategory) searchParams.set('distanceCategory', params.distanceCategory);
      if (params.minPayout) searchParams.set('minPayout', params.minPayout.toString());
      if (params.maxPayout) searchParams.set('maxPayout', params.maxPayout.toString());
      if (params.minWeightLbs) searchParams.set('minWeightLbs', params.minWeightLbs.toString());
      if (params.maxWeightLbs) searchParams.set('maxWeightLbs', params.maxWeightLbs.toString());
      if (params.minPassengers) searchParams.set('minPassengers', params.minPassengers.toString());
      if (params.maxPassengers) searchParams.set('maxPassengers', params.maxPassengers.toString());
      if (params.requiresSpecialCertification !== undefined) searchParams.set('requiresSpecialCertification', params.requiresSpecialCertification.toString());
      if (params.sortBy) searchParams.set('sortBy', params.sortBy);
      if (params.sortDescending !== undefined) searchParams.set('sortDescending', params.sortDescending.toString());
      if (params.page) searchParams.set('page', params.page.toString());
      if (params.pageSize) searchParams.set('pageSize', params.pageSize.toString());
      if (params.centerLatitude) searchParams.set('centerLatitude', params.centerLatitude.toString());
      if (params.centerLongitude) searchParams.set('centerLongitude', params.centerLongitude.toString());
      if (params.vicinityRadiusNm) searchParams.set('vicinityRadiusNm', params.vicinityRadiusNm.toString());
      return request<JobSearchResult>(`/api/jobs/search?${searchParams}`, {}, true);
    },

    getAvailable: (params: { worldId: string; airportId: number; limit?: number }): Promise<ApiResponse<Job[]>> => {
      const searchParams = new URLSearchParams();
      searchParams.set('worldId', params.worldId);
      searchParams.set('airportId', params.airportId.toString());
      if (params.limit) searchParams.set('limit', params.limit.toString());
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

    cancel: (jobId: string, userId: string): Promise<ApiResponse<{ message: string }>> =>
      request<{ message: string }>(`/api/jobs/${jobId}/cancel`, {
        method: 'POST',
        body: JSON.stringify({ userId }),
      }, true),

    getMyJobs: (params: { worldId: string; userId: string; includeCompleted?: boolean }): Promise<ApiResponse<Job[]>> => {
      const searchParams = new URLSearchParams();
      searchParams.set('worldId', params.worldId);
      searchParams.set('userId', params.userId);
      if (params.includeCompleted) searchParams.set('includeCompleted', 'true');
      return request<Job[]>(`/api/jobs/my-jobs?${searchParams}`, {}, true);
    },

    getStats: (worldId: string, airportId?: number): Promise<ApiResponse<JobStatsResult>> => {
      const searchParams = new URLSearchParams();
      searchParams.set('worldId', worldId);
      if (airportId) searchParams.set('airportId', airportId.toString());
      return request<JobStatsResult>(`/api/jobs/stats?${searchParams}`, {}, true);
    },

    getCargoTypes: (): Promise<ApiResponse<CargoTypeResponse[]>> =>
      request<CargoTypeResponse[]>('/api/jobs/cargo-types', {}, true),
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

    checkExistingRequest: (aircraftTitle: string): Promise<ApiResponse<AircraftRequestCheckResponse>> =>
      request<AircraftRequestCheckResponse>(`/api/aircraftrequests/check/${encodeURIComponent(aircraftTitle)}`, {}, true),
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

  skills: {
    getAll: (worldId: string): Promise<ApiResponse<PlayerSkillResponse[]>> =>
      request<PlayerSkillResponse[]>(`/api/skills/${worldId}`, {}, true),

    get: (worldId: string, skillType: string): Promise<ApiResponse<PlayerSkillResponse>> =>
      request<PlayerSkillResponse>(`/api/skills/${worldId}/${skillType}`, {}, true),

    getHistory: (worldId: string, params?: { skillType?: string; limit?: number }): Promise<ApiResponse<SkillXpEventResponse[]>> => {
      const searchParams = new URLSearchParams();
      if (params?.skillType) searchParams.set('skillType', params.skillType);
      if (params?.limit) searchParams.set('limit', params.limit.toString());
      const query = searchParams.toString();
      return request<SkillXpEventResponse[]>(`/api/skills/${worldId}/history${query ? `?${query}` : ''}`, {}, true);
    },

    getTotal: (worldId: string): Promise<ApiResponse<{ totalLevel: number }>> =>
      request<{ totalLevel: number }>(`/api/skills/${worldId}/total`, {}, true),
  },

  reputation: {
    getStatus: (worldId: string): Promise<ApiResponse<ReputationStatusResponse>> =>
      request<ReputationStatusResponse>(`/api/reputation/${worldId}`, {}, true),

    getHistory: (worldId: string, limit?: number): Promise<ApiResponse<ReputationEventResponse[]>> => {
      const params = limit ? `?limit=${limit}` : '';
      return request<ReputationEventResponse[]>(`/api/reputation/${worldId}/history${params}`, {}, true);
    },

    getBonus: (worldId: string): Promise<ApiResponse<{ bonusPercent: number }>> =>
      request<{ bonusPercent: number }>(`/api/reputation/${worldId}/bonus`, {}, true),
  },

  licenses: {
    getTypes: (): Promise<ApiResponse<LicenseTypeResponse[]>> =>
      request<LicenseTypeResponse[]>('/api/licenses/types', {}, true),

    getShop: (worldId: string): Promise<ApiResponse<LicenseShopResponse>> =>
      request<LicenseShopResponse>(`/api/licenses/shop/${worldId}`, {}, true),

    getMyLicenses: (worldId: string): Promise<ApiResponse<UserLicenseResponse[]>> =>
      request<UserLicenseResponse[]>(`/api/licenses/my/${worldId}`, {}, true),

    checkLicense: (worldId: string, licenseCode: string): Promise<ApiResponse<LicenseCheckResponse>> =>
      request<LicenseCheckResponse>(`/api/licenses/check/${worldId}/${licenseCode}`, {}, true),

    renew: (worldId: string, licenseId: string): Promise<ApiResponse<UserLicenseResponse>> =>
      request<UserLicenseResponse>(`/api/licenses/renew/${worldId}/${licenseId}`, {
        method: 'POST',
      }, true),
  },

  exams: {
    schedule: (data: ScheduleExamRequest): Promise<ApiResponse<LicenseExamResponse>> =>
      request<LicenseExamResponse>('/api/exams/schedule', {
        method: 'POST',
        body: JSON.stringify(data),
      }, true),

    start: (examId: string): Promise<ApiResponse<LicenseExamResponse>> =>
      request<LicenseExamResponse>(`/api/exams/${examId}/start`, {
        method: 'POST',
      }, true),

    recordViolation: (examId: string, data: RecordViolationRequest): Promise<ApiResponse<ExamViolationResponse>> =>
      request<ExamViolationResponse>(`/api/exams/${examId}/violation`, {
        method: 'POST',
        body: JSON.stringify(data),
      }, true),

    recordLanding: (examId: string, data: RecordLandingRequest): Promise<ApiResponse<ExamLandingResponse>> =>
      request<ExamLandingResponse>(`/api/exams/${examId}/landing`, {
        method: 'POST',
        body: JSON.stringify(data),
      }, true),

    complete: (examId: string): Promise<ApiResponse<ExamCompletionResponse>> =>
      request<ExamCompletionResponse>(`/api/exams/${examId}/complete`, {
        method: 'POST',
      }, true),

    abandon: (examId: string): Promise<ApiResponse<{ message: string }>> =>
      request<{ message: string }>(`/api/exams/${examId}/abandon`, {
        method: 'POST',
      }, true),

    get: (examId: string): Promise<ApiResponse<LicenseExamResponse>> =>
      request<LicenseExamResponse>(`/api/exams/${examId}`, {}, true),

    getHistory: (worldId: string): Promise<ApiResponse<LicenseExamResponse[]>> =>
      request<LicenseExamResponse[]>(`/api/exams/history/${worldId}`, {}, true),

    getActive: (worldId: string): Promise<ApiResponse<LicenseExamResponse>> =>
      request<LicenseExamResponse>(`/api/exams/active/${worldId}`, {}, true),
  },

  loans: {
    getMyLoans: (): Promise<ApiResponse<LoanResponse[]>> =>
      request<LoanResponse[]>('/api/loans', {}, true),

    getLoan: (id: string): Promise<ApiResponse<LoanDetailResponse>> =>
      request<LoanDetailResponse>(`/api/loans/${id}`, {}, true),

    getSummary: (): Promise<ApiResponse<LoanSummaryResponse>> =>
      request<LoanSummaryResponse>('/api/loans/summary', {}, true),

    getStarterLoanEligibility: (): Promise<ApiResponse<StarterLoanEligibilityResponse>> =>
      request<StarterLoanEligibilityResponse>('/api/loans/starter-loan/eligibility', {}, true),

    applyForStarterLoan: (data: StarterLoanApplicationRequest): Promise<ApiResponse<LoanApplicationResponse>> =>
      request<LoanApplicationResponse>('/api/loans/starter-loan/apply', {
        method: 'POST',
        body: JSON.stringify(data),
      }, true),

    applyForLoan: (data: LoanApplicationRequest): Promise<ApiResponse<LoanApplicationResponse>> =>
      request<LoanApplicationResponse>('/api/loans/apply', {
        method: 'POST',
        body: JSON.stringify(data),
      }, true),

    makePayment: (loanId: string, amount: number): Promise<ApiResponse<LoanPaymentResultResponse>> =>
      request<LoanPaymentResultResponse>(`/api/loans/${loanId}/pay`, {
        method: 'POST',
        body: JSON.stringify({ amount }),
      }, true),

    payOffLoan: (loanId: string): Promise<ApiResponse<LoanPaymentResultResponse>> =>
      request<LoanPaymentResultResponse>(`/api/loans/${loanId}/payoff`, {
        method: 'POST',
      }, true),

    getCreditScore: (): Promise<ApiResponse<CreditScoreResponse>> =>
      request<CreditScoreResponse>('/api/loans/credit-score', {}, true),

    getCreditHistory: (limit?: number): Promise<ApiResponse<CreditScoreEventResponse[]>> => {
      const params = limit ? `?limit=${limit}` : '';
      return request<CreditScoreEventResponse[]>(`/api/loans/credit-score/history${params}`, {}, true);
    },

    getBanks: (): Promise<ApiResponse<BankResponse[]>> =>
      request<BankResponse[]>('/api/loans/banks', {}, true),
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
  JobSearchParams,
  JobSearchResult,
  JobStatsResult,
  AcceptJobResponse,
  CompleteJobResponse,
  AuthResponse,
  UserResponse,
  RegisterRequest,
  LoginRequest,
  AircraftRequestResponse,
  AircraftResponse,
  CreateAircraftRequestData,
  ManifestDataResponse,
  AircraftCfgDataResponse,
  AircraftRequestCheckResponse,
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
  MarketplaceSearchResult,
  PlayerSkillResponse,
  SkillXpEventResponse,
  ReputationStatusResponse,
  ReputationBenefitResponse,
  ReputationEventResponse,
  LicenseTypeResponse,
  UserLicenseResponse,
  LicenseShopResponse,
  LicenseShopItemResponse,
  LicenseCheckResponse,
  LicenseExamResponse,
  ExamViolationResponse,
  ExamLandingResponse,
  ScheduleExamRequest,
  RecordViolationRequest,
  RecordLandingRequest,
  ExamCompletionResponse,
  LoanResponse,
  LoanDetailResponse,
  LoanSummaryResponse,
  StarterLoanEligibilityResponse,
  LoanApplicationResponse,
  LoanPaymentResultResponse,
  PaymentResponse,
  CreditScoreResponse,
  CreditScoreEventResponse,
  BankResponse,
  StarterLoanApplicationRequest,
  LoanApplicationRequest
};
