import { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import type { AuthContextType, User } from '../types/auth';
import { authService } from '../services/authService';

const AuthContext = createContext<AuthContextType | undefined>(undefined);

const TOKEN_KEY = 'digital_stokvel_token';
const USER_KEY = 'digital_stokvel_user';

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Load authentication state from localStorage on mount
  useEffect(() => {
    const loadAuthState = () => {
      const storedToken = localStorage.getItem(TOKEN_KEY);
      const storedUser = localStorage.getItem(USER_KEY);

      if (storedToken && storedUser) {
        try {
          // Check if token is expired
          if (!authService.isTokenExpired(storedToken)) {
            setToken(storedToken);
            setUser(JSON.parse(storedUser));
          } else {
            // Token expired, clear storage
            localStorage.removeItem(TOKEN_KEY);
            localStorage.removeItem(USER_KEY);
          }
        } catch (error) {
          console.error('Error loading auth state:', error);
          localStorage.removeItem(TOKEN_KEY);
          localStorage.removeItem(USER_KEY);
        }
      }

      setIsLoading(false);
    };

    loadAuthState();
  }, []);

  const login = async (phoneNumber: string, pin: string) => {
    try {
      setIsLoading(true);
      const response = await authService.login(phoneNumber, pin);
      
      // Store token and user
      localStorage.setItem(TOKEN_KEY, response.token);
      localStorage.setItem(USER_KEY, JSON.stringify(response.user));
      
      setToken(response.token);
      setUser(response.user);
    } catch (error) {
      console.error('Login error:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const logout = () => {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    setToken(null);
    setUser(null);
  };

  const value: AuthContextType = {
    user,
    token,
    isAuthenticated: !!token && !!user,
    isLoading,
    login,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
