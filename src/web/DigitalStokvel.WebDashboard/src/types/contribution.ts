/**
 * Contribution Type Definitions
 * Based on design.md Section 5.3 (Contribution API) and Section 4.2 (Database Schema)
 */

export type ContributionStatus = 'completed' | 'pending' | 'overdue' | 'late' | 'missed';
export type PaymentMethod = 'linked_account' | 'debit_order' | 'instant_eft' | 'ussd';
export type RecurringFrequency = 'weekly' | 'monthly' | 'quarterly';

// Member Info (minimal for contribution display)
export interface ContributionMember {
  id: string;
  name: string;
  phone: string;
}

// Contribution Receipt
export interface ContributionReceipt {
  receiptNumber: string;
  date: string; // ISO timestamp
  groupName: string;
  amount: number;
  balanceAfter: number;
  pdfUrl?: string;
}

// Contribution (individual contribution record)
export interface Contribution {
  id: string;
  member: ContributionMember;
  groupId?: string;
  amount: number;
  transactionRef: string;
  status: ContributionStatus;
  dueDate: string; // ISO date
  paidAt?: string; // ISO timestamp
  receipt?: ContributionReceipt;
  lateFee?: number;
  notes?: string;
}

// Contribution Summary (for group overview)
export interface ContributionSummary {
  totalContributions: number;
  totalMembersPaid: number;
  totalMembersPending: number;
  totalMembersOverdue: number;
  totalMembersMissed?: number;
}

// Make Contribution Request
export interface MakeContributionRequest {
  groupId: string;
  amount: number;
  paymentMethod: PaymentMethod;
  pin: string; // Encrypted PIN
}

// Make Contribution Response
export interface MakeContributionResponse {
  id: string;
  groupId: string;
  amount: number;
  transactionRef: string;
  status: ContributionStatus;
  receipt: ContributionReceipt;
  paidAt: string;
}

// Recurring Payment
export interface RecurringPayment {
  id: string;
  groupId: string;
  amount: number;
  frequency: RecurringFrequency;
  status: 'active' | 'paused' | 'cancelled';
  nextPaymentDate: string; // ISO date
  startDate: string;
  endDate?: string;
  createdAt: string;
}

// Set Up Recurring Payment Request
export interface SetupRecurringPaymentRequest {
  groupId: string;
  amount: number;
  frequency: RecurringFrequency;
  startDate: string; // ISO date
  mandateAuthorization: string;
}

// Get Contributions Filters
export interface ContributionFilters {
  memberId?: string;
  fromDate?: string; // ISO date
  toDate?: string; // ISO date
  status?: ContributionStatus;
  page?: number;
  limit?: number;
}

// Pagination (from group.ts, repeated for convenience)
export interface Pagination {
  page: number;
  limit: number;
  total: number;
  pages: number;
}

// Get Contribution History Response
export interface GetContributionsResponse {
  contributions: Contribution[];
  summary: ContributionSummary;
  pagination: Pagination;
}

// Contribution Statistics (for dashboard charts)
export interface ContributionStats {
  totalAmount: number;
  averageAmount: number;
  onTimePercentage: number;
  latePercentage: number;
  missedPercentage: number;
  contributionsByMonth: {
    month: string; // YYYY-MM
    amount: number;
    count: number;
  }[];
  topContributors: {
    memberId: string;
    memberName: string;
    totalContributions: number;
    onTimeCount: number;
  }[];
}
