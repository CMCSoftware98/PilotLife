const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

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

interface RegisterResponse {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  message: string;
}

interface LoginRequest {
  email: string;
  password: string;
}

interface LoginResponse {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  experienceLevel?: string;
  message: string;
}

interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  experienceLevel?: string;
}

async function request<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<ApiResponse<T>> {
  try {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        ...options.headers,
      },
    });

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
    register: (data: RegisterRequest): Promise<ApiResponse<RegisterResponse>> =>
      request<RegisterResponse>('/api/auth/register', {
        method: 'POST',
        body: JSON.stringify(data),
      }),

    login: (data: LoginRequest): Promise<ApiResponse<LoginResponse>> =>
      request<LoginResponse>('/api/auth/login', {
        method: 'POST',
        body: JSON.stringify(data),
      }),

    health: (): Promise<ApiResponse<{ status: string; timestamp: string }>> =>
      request('/api/auth/health'),
  },
};

export type { User, RegisterRequest, RegisterResponse, LoginRequest, LoginResponse };
