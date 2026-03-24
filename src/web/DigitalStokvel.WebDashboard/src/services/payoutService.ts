/**
 * Payout Service
 * Handles all payout approval API calls
 * Design Reference: Section 5.4 - Payout API
 */

import type {
  Payout,
  PayoutStatus,
  PayoutFilters,
  InitiatePayoutRequest,
  InitiatePayoutResponse,
  ApprovePayoutRequest,
  ApprovePayoutResponse,
  RejectPayoutRequest,
  ListPayoutsResponse,
} from '../types/payout';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

/**
 * Get payouts for a group
 */
export async function getGroupPayouts(
  groupId: string,
  token: string,
  filters?: PayoutFilters
): Promise<ListPayoutsResponse> {
  const params = new URLSearchParams({
    group_id: groupId,
  });
  
  if (filters?.status) params.append('status', filters.status);
  if (filters?.fromDate) params.append('from_date', filters.fromDate);
  if (filters?.toDate) params.append('to_date', filters.toDate);
  if (filters?.page) params.append('page', filters.page.toString());
  if (filters?.limit) params.append('limit', filters.limit.toString());

  const queryString = params.toString();
  const url = `${API_BASE_URL}/v1/payouts${queryString ? `?${queryString}` : ''}`;

  const response = await fetch(url, {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: 'Failed to fetch payouts' }));
    throw new Error(error.message || `HTTP ${response.status}: Failed to fetch payouts`);
  }

  return response.json();
}

/**
 * Get pending payouts for approval (for current user's role)
 */
export async function getPendingPayouts(
  token: string,
  groupId?: string
): Promise<Payout[]> {
  const params = new URLSearchParams({
    status: 'pending_treasurer_approval',
  });
  
  if (groupId) {
    params.append('group_id', groupId);
  }

  const response = await fetch(`${API_BASE_URL}/v1/payouts?${params}`, {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: 'Failed to fetch pending payouts' }));
    throw new Error(error.message || `HTTP ${response.status}: Failed to fetch pending payouts`);
  }

  const data = await response.json();
  return data.payouts || [];
}

/**
 * Get payout details by ID
 */
export async function getPayoutById(
  payoutId: string,
  token: string
): Promise<Payout> {
  const response = await fetch(`${API_BASE_URL}/v1/payouts/${payoutId}`, {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: 'Failed to fetch payout' }));
    throw new Error(error.message || `HTTP ${response.status}: Failed to fetch payout`);
  }

  return response.json();
}

/**
 * Initiate a new payout (Chairperson only)
 */
export async function initiatePayout(
  request: InitiatePayoutRequest,
  token: string
): Promise<InitiatePayoutResponse> {
  const response = await fetch(`${API_BASE_URL}/v1/payouts`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: 'Failed to initiate payout' }));
    throw new Error(error.message || `HTTP ${response.status}: Failed to initiate payout`);
  }

  return response.json();
}

/**
 * Approve a payout (Treasurer only)
 */
export async function approvePayout(
  payoutId: string,
  request: ApprovePayoutRequest,
  token: string
): Promise<ApprovePayoutResponse> {
  const response = await fetch(`${API_BASE_URL}/v1/payouts/${payoutId}/approve`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: 'Failed to approve payout' }));
    throw new Error(error.message || `HTTP ${response.status}: Failed to approve payout`);
  }

  return response.json();
}

/**
 * Reject a payout (Treasurer only)
 */
export async function rejectPayout(
  payoutId: string,
  request: RejectPayoutRequest,
  token: string
): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/v1/payouts/${payoutId}/reject`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: 'Failed to reject payout' }));
    throw new Error(error.message || `HTTP ${response.status}: Failed to reject payout`);
  }
}

/**
 * Get status display with color
 */
export function getStatusColor(status: PayoutStatus): string {
  switch (status) {
    case 'completed':
      return 'green';
    case 'approved':
    case 'processing':
      return 'blue';
    case 'pending_chairperson_approval':
    case 'pending_treasurer_approval':
      return 'orange';
    case 'failed':
    case 'rejected':
      return 'red';
    default:
      return 'gray';
  }
}

/**
 * Get status display text
 */
export function getStatusDisplay(status: PayoutStatus): string {
  switch (status) {
    case 'pending_chairperson_approval':
      return '⏳ Pending Chairperson';
    case 'pending_treasurer_approval':
      return '⏳ Pending Treasurer';
    case 'approved':
      return '✅ Approved';
    case 'processing':
      return '⚙️ Processing';
    case 'completed':
      return '✅ Completed';
    case 'failed':
      return '❌ Failed';
    case 'rejected':
      return '⛔ Rejected';
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

/**
 * Check if user can approve payout
 */
export function canApprovePayout(payout: Payout, userRole: string): boolean {
  if (payout.status === 'pending_treasurer_approval' && userRole === 'treasurer') {
    return true;
  }
  if (payout.status === 'pending_chairperson_approval' && userRole === 'chairperson') {
    return true;
  }
  return false;
}
