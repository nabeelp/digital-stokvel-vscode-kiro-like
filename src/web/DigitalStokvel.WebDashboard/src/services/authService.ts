import type { LoginRequest, LoginResponse, User } from '../types/auth';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

class AuthService {
  /**
   * Login with phone number and PIN
   */
  async login(phoneNumber: string, pin: string): Promise<LoginResponse> {
    const response = await fetch(`${API_BASE_URL}/v1/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ phoneNumber, pin } as LoginRequest),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Login failed' }));
      throw new Error(error.message || 'Login failed');
    }

    return response.json();
  }

  /**
   * Get current user profile
   */
  async getCurrentUser(token: string): Promise<User> {
    const response = await fetch(`${API_BASE_URL}/v1/users/me`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error('Failed to fetch user profile');
    }

    return response.json();
  }

  /**
   * Refresh JWT token
   */
  async refreshToken(token: string): Promise<{ token: string }> {
    const response = await fetch(`${API_BASE_URL}/v1/auth/refresh`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error('Token refresh failed');
    }

    return response.json();
  }

  /**
   * Decode JWT token (simple base64 decode without verification)
   * Note: This is for UI purposes only. Server must validate tokens.
   */
  decodeToken(token: string): any {
    try {
      const payload = token.split('.')[1];
      const decoded = atob(payload);
      return JSON.parse(decoded);
    } catch {
      return null;
    }
  }

  /**
   * Check if token is expired
   */
  isTokenExpired(token: string): boolean {
    const decoded = this.decodeToken(token);
    if (!decoded || !decoded.exp) return true;
    
    const expirationTime = decoded.exp * 1000; // Convert to milliseconds
    return Date.now() >= expirationTime;
  }
}

export const authService = new AuthService();
