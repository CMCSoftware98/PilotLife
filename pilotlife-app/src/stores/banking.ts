import { ref, computed } from 'vue';
import type {
  LoanResponse,
  LoanDetailResponse,
  LoanSummaryResponse,
  StarterLoanEligibilityResponse,
  CreditScoreResponse,
  CreditScoreEventResponse,
  BankResponse,
  StarterLoanApplicationRequest,
  LoanApplicationRequest
} from '../services/api';
import { api } from '../services/api';

const loans = ref<LoanResponse[]>([]);
const selectedLoan = ref<LoanDetailResponse | null>(null);
const loanSummary = ref<LoanSummaryResponse | null>(null);
const starterLoanEligibility = ref<StarterLoanEligibilityResponse | null>(null);
const creditScore = ref<CreditScoreResponse | null>(null);
const creditHistory = ref<CreditScoreEventResponse[]>([]);
const banks = ref<BankResponse[]>([]);
const isLoading = ref(false);
const error = ref<string | null>(null);

export function useBankingStore() {
  // Computed properties
  const activeLoans = computed(() =>
    loans.value.filter(l => l.status === 'Active')
  );

  const paidOffLoans = computed(() =>
    loans.value.filter(l => l.status === 'PaidOff')
  );

  const defaultedLoans = computed(() =>
    loans.value.filter(l => l.status === 'Defaulted')
  );

  const totalDebt = computed(() =>
    activeLoans.value.reduce((sum, loan) => sum + loan.remainingPrincipal, 0)
  );

  const totalMonthlyPayment = computed(() =>
    activeLoans.value.reduce((sum, loan) => sum + loan.monthlyPayment, 0)
  );

  const nextPaymentDue = computed(() => {
    const activeLoansDates = activeLoans.value
      .filter(l => l.nextPaymentDue)
      .map(l => new Date(l.nextPaymentDue!))
      .sort((a, b) => a.getTime() - b.getTime());
    return activeLoansDates.length > 0 ? activeLoansDates[0] : null;
  });

  const creditRating = computed(() => creditScore.value?.rating ?? 'Unknown');

  const isEligibleForStarterLoan = computed(() =>
    starterLoanEligibility.value?.isEligible ?? false
  );

  // Actions
  async function loadLoans(): Promise<boolean> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.loans.getMyLoans();
      if (response.data) {
        loans.value = response.data;
        return true;
      }
      error.value = response.error ?? 'Failed to load loans';
      return false;
    } catch {
      error.value = 'Failed to load loans';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  async function loadLoanDetails(loanId: string): Promise<LoanDetailResponse | null> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.loans.getLoan(loanId);
      if (response.data) {
        selectedLoan.value = response.data;
        return response.data;
      }
      error.value = response.error ?? 'Failed to load loan details';
      return null;
    } catch {
      error.value = 'Failed to load loan details';
      return null;
    } finally {
      isLoading.value = false;
    }
  }

  async function loadSummary(): Promise<boolean> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.loans.getSummary();
      if (response.data) {
        loanSummary.value = response.data;
        return true;
      }
      error.value = response.error ?? 'Failed to load loan summary';
      return false;
    } catch {
      error.value = 'Failed to load loan summary';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  async function loadStarterLoanEligibility(): Promise<boolean> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.loans.getStarterLoanEligibility();
      if (response.data) {
        starterLoanEligibility.value = response.data;
        return true;
      }
      error.value = response.error ?? 'Failed to check starter loan eligibility';
      return false;
    } catch {
      error.value = 'Failed to check starter loan eligibility';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  async function applyForStarterLoan(data: StarterLoanApplicationRequest): Promise<{ success: boolean; message: string; newBalance?: number }> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.loans.applyForStarterLoan(data);
      if (response.data?.success) {
        // Refresh loans and summary
        await loadLoans();
        await loadSummary();
        await loadStarterLoanEligibility();
        return { success: true, message: response.data.message, newBalance: response.data.newBalance };
      }
      error.value = response.error ?? response.data?.message ?? 'Failed to apply for starter loan';
      return { success: false, message: error.value };
    } catch {
      error.value = 'Failed to apply for starter loan';
      return { success: false, message: error.value };
    } finally {
      isLoading.value = false;
    }
  }

  async function applyForLoan(data: LoanApplicationRequest): Promise<{ success: boolean; message: string; newBalance?: number }> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.loans.applyForLoan(data);
      if (response.data?.success) {
        // Refresh loans and summary
        await loadLoans();
        await loadSummary();
        return { success: true, message: response.data.message, newBalance: response.data.newBalance };
      }
      error.value = response.error ?? response.data?.message ?? 'Failed to apply for loan';
      return { success: false, message: error.value };
    } catch {
      error.value = 'Failed to apply for loan';
      return { success: false, message: error.value };
    } finally {
      isLoading.value = false;
    }
  }

  async function makePayment(loanId: string, amount: number): Promise<{ success: boolean; message: string; loanPaidOff?: boolean; newBalance?: number }> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.loans.makePayment(loanId, amount);
      if (response.data?.success) {
        // Refresh data
        await loadLoans();
        await loadSummary();
        await loadCreditScore();
        return {
          success: true,
          message: response.data.message,
          loanPaidOff: response.data.loanPaidOff,
          newBalance: response.data.newBalance
        };
      }
      error.value = response.error ?? response.data?.message ?? 'Failed to make payment';
      return { success: false, message: error.value };
    } catch {
      error.value = 'Failed to make payment';
      return { success: false, message: error.value };
    } finally {
      isLoading.value = false;
    }
  }

  async function payOffLoan(loanId: string): Promise<{ success: boolean; message: string; newBalance?: number }> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.loans.payOffLoan(loanId);
      if (response.data?.success) {
        // Refresh data
        await loadLoans();
        await loadSummary();
        await loadCreditScore();
        return { success: true, message: response.data.message, newBalance: response.data.newBalance };
      }
      error.value = response.error ?? response.data?.message ?? 'Failed to pay off loan';
      return { success: false, message: error.value };
    } catch {
      error.value = 'Failed to pay off loan';
      return { success: false, message: error.value };
    } finally {
      isLoading.value = false;
    }
  }

  async function loadCreditScore(): Promise<boolean> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.loans.getCreditScore();
      if (response.data) {
        creditScore.value = response.data;
        return true;
      }
      error.value = response.error ?? 'Failed to load credit score';
      return false;
    } catch {
      error.value = 'Failed to load credit score';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  async function loadCreditHistory(limit?: number): Promise<boolean> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.loans.getCreditHistory(limit);
      if (response.data) {
        creditHistory.value = response.data;
        return true;
      }
      error.value = response.error ?? 'Failed to load credit history';
      return false;
    } catch {
      error.value = 'Failed to load credit history';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  async function loadBanks(): Promise<boolean> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.loans.getBanks();
      if (response.data) {
        banks.value = response.data;
        return true;
      }
      error.value = response.error ?? 'Failed to load banks';
      return false;
    } catch {
      error.value = 'Failed to load banks';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  function clearData() {
    loans.value = [];
    selectedLoan.value = null;
    loanSummary.value = null;
    starterLoanEligibility.value = null;
    creditScore.value = null;
    creditHistory.value = [];
    banks.value = [];
    error.value = null;
  }

  return {
    // State
    loans,
    selectedLoan,
    loanSummary,
    starterLoanEligibility,
    creditScore,
    creditHistory,
    banks,
    isLoading,
    error,
    // Computed
    activeLoans,
    paidOffLoans,
    defaultedLoans,
    totalDebt,
    totalMonthlyPayment,
    nextPaymentDue,
    creditRating,
    isEligibleForStarterLoan,
    // Actions
    loadLoans,
    loadLoanDetails,
    loadSummary,
    loadStarterLoanEligibility,
    applyForStarterLoan,
    applyForLoan,
    makePayment,
    payOffLoan,
    loadCreditScore,
    loadCreditHistory,
    loadBanks,
    clearData,
  };
}
