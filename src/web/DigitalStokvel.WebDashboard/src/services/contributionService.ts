/**
 * Contribution Service
 * Handles all contribution tracking API calls
 * Design Reference: Section 5.3 - Contribution API
 */

import type {
  Contribution,
  ContributionFilters,
  GetContributionsResponse,
  MakeContributionRequest,
  MakeContributionResponse,
  SetupRecurringPaymentRequest,
  RecurringPayment,
  ContributionStatus,
} from '../types/contribution';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

/**
 * Get contribution history for a group
 */
export async function getGroupContributions(
  groupId: string,
  token: string,
  filters?: ContributionFilters
): Promise<GetContributionsResponse> {
  const params = new URLSearchParams();
  
  if (filters?.memberId) params.append('member_id', filters.memberId);
  if (filters?.fromDate) params.append('from_date', filters.fromDate);
  if (filters?.toDate) params.append('to_date', filters.toDate);
  if (filters?.status) params.append('status', filters.status);
  if (filters?.page) params.append('page', filters.page.toString());
  if (filters?.limit) params.append('limit', filters.limit.toString());

  const queryString = params.toString();
  const url = `${API_BASE_URL}/v1/groups/${groupId}/contributions${queryString ? `?${queryString}` : ''}`;

  const response = await fetch(url, {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: 'Failed to fetch contributions' }));
    throw new Error(error.message || `HTTP ${response.status}: Failed to fetch contributions`);
  }

  return response.json();
}

/**
 * Get contribution details by ID
 */
export async function getContributionById(
  contributionId: string,
  token: string
): Promise<Contribution> {
  const response = await fetch(`${API_BASE_URL}/v1/contributions/${contributionId}`, {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: 'Failed to fetch contribution' }));
    throw new Error(error.message || `HTTP ${response.status}: Failed to fetch contribution`);
  }

  return response.json();
}

/**
 * Make a contribution
 */
export async function makeContribution(
  request: MakeContributionRequest,
  token: string
): Promise<MakeContributionResponse> {
  const response = await fetch(`${API_BASE_URL}/v1/contributions`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: 'Failed to make contribution' }));
    throw new Error(error.message || `HTTP ${response.status}: Failed to make contribution`);
  }

  return response.json();
}

/**
 * Set up recurring payment
 */
export async function setupRecurringPayment(
  request: SetupRecurringPaymentRequest,
  token: string
): Promise<RecurringPayment> {
  const response = await fetch(`${API_BASE_URL}/v1/recurring-payments`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: 'Failed to setup recurring payment' }));
    throw new Error(error.message || `HTTP ${response.status}: Failed to setup recurring payment`);
  }

  return response.json();
}

/**
 * Get user's recurring payments
 */
export async function getRecurringPayments(
  token: string,
  groupId?: string
): Promise<RecurringPayment[]> {
  const url = groupId
    ? `${API_BASE_URL}/v1/recurring-payments?group_id=${groupId}`
    : `${API_BASE_URL}/v1/recurring-payments`;

  const response = await fetch(url, {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: 'Failed to fetch recurring payments' }));
    throw new Error(error.message || `HTTP ${response.status}: Failed to fetch recurring payments`);
  }

  const data = await response.json();
  return data.recurringPayments || [];
}

/**
 * Cancel recurring payment
 */
export async function cancelRecurringPayment(
  paymentId: string,
  token: string
): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/v1/recurring-payments/${paymentId}`, {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: 'Failed to cancel recurring payment' }));
    throw new Error(error.message || `HTTP ${response.status}: Failed to cancel recurring payment`);
  }
}

/**
 * Get status display with color
 */
export function getStatusColor(status: ContributionStatus): string {
  switch (status) {
    case 'completed':
      return 'green';
    case 'pending':
      return 'blue';
    case 'late':
      return 'orange';
    case 'overdue':
      return 'red';
    case 'missed':
      return 'gray';
    default:
      return 'gray';
  }
}

/**
 * Get status display text
 */
export function getStatusDisplay(status: ContributionStatus): string {
  switch (status) {
    case 'completed':
      return '✅ Completed';
    case 'pending':
      return '⏳ Pending';
    case 'late':
      return '⚠️ Late';
    case 'overdue':
      return '❌ Overdue';
    case 'missed':
      return '⛔ Missed';
    default:
      return status;
  }
}

/**
 * Format currency amount
 */
export function formatCurrency(amount: number): string {
  return `R${amount.toFixed(2)}`;
}

/**
 * Format date for display
 */
export function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString('en-ZA', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}

/**
 * Format datetime for display
 */
export function formatDateTime(dateString: string): string {
  return new Date(dateString).toLocaleString('en-ZA', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}
