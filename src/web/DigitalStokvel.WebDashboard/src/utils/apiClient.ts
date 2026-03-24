const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

export interface ApiClientOptions {
  method?: string;
  headers?: Record<string, string>;
  body?: any;
  token?: string | null;
}

class ApiClient {
  private baseUrl: string;

  constructor(baseUrl: string = API_BASE_URL) {
    this.baseUrl = baseUrl;
  }

  private getHeaders(token?: string | null, customHeaders?: Record<string, string>): Record<string, string> {
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      ...customHeaders,
    };

    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    return headers;
  }

  async request<T>(endpoint: string, options: ApiClientOptions = {}): Promise<T> {
    const {
      method = 'GET',
      headers: customHeaders,
      body,
      token,
    } = options;

    const url = `${this.baseUrl}${endpoint}`;
    const headers = this.getHeaders(token, customHeaders);

    const config: RequestInit = {
      method,
      headers,
    };

    if (body) {
      config.body = JSON.stringify(body);
    }

    try {
      const response = await fetch(url, config);

      if (!response.ok) {
        const error = await response.json().catch(() => ({
          message: `Request failed with status ${response.status}`,
        }));
        throw new Error(error.message || `Request failed with status ${response.status}`);
      }

      // Handle 204 No Content
      if (response.status === 204) {
        return {} as T;
      }

      return await response.json();
    } catch (error) {
      console.error(`API request failed: ${endpoint}`, error);
      throw error;
    }
  }

  async get<T>(endpoint: string, token?: string | null): Promise<T> {
    return this.request<T>(endpoint, { method: 'GET', token });
  }

  async post<T>(endpoint: string, body: any, token?: string | null): Promise<T> {
    return this.request<T>(endpoint, { method: 'POST', body, token });
  }

  async put<T>(endpoint: string, body: any, token?: string | null): Promise<T> {
    return this.request<T>(endpoint, { method: 'PUT', body, token });
  }

  async patch<T>(endpoint: string, body: any, token?: string | null): Promise<T> {
    return this.request<T>(endpoint, { method: 'PATCH', body, token });
  }

  async delete<T>(endpoint: string, token?: string | null): Promise<T> {
    return this.request<T>(endpoint, { method: 'DELETE', token });
  }
}

export const apiClient = new ApiClient();
