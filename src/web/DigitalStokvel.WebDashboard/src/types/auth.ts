export interface User {
  id: string;
  phoneNumber: string;
  firstName: string;
  lastName: string;
  preferredLanguage: string;
  roles: string[];
  groups: string[];
}

export interface AuthToken {
  token: string;
  expiresAt: number;
}

export interface LoginRequest {
  phoneNumber: string;
  pin: string;
}

export interface LoginResponse {
  token: string;
  user: User;
}

export interface AuthContextType {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (phoneNumber: string, pin: string) => Promise<void>;
  logout: () => void;
}
