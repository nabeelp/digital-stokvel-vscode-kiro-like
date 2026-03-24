/**
 * Payout Type Definitions
 * Based on design.md Section 5.4 (Payout API) and Section 4.2 (Database Schema)
 */

export type PayoutType = 'rotating' | 'year_end' | 'emergency';
export type PayoutStatus = 
  | 'pending_chairperson_approval'
  | 'pending_treasurer_approval' 
  | 'approved'
  | 'processing'
  | 'completed'
  | 'failed'
  | 'rejected';
export type DisbursementStatus = 'pending' | 'processing' | 'completed' | 'failed';

// Initiator/Approver Info
export interface PayoutPerson {
  id: string;
  name: string;
  role: 'chairperson' | 'treasurer' | 'secretary' | 'member';
}

// Disbursement (payment to individual member)
export interface PayoutDisbursement {
  id?: string;
  member: {
    id: string;
    name: string;
  };
  amount: number;
  transactionRef?: string;
  status: DisbursementStatus;
  executedAt?: string; // ISO timestamp
  failureReason?: string;
}

// Payout (full payout request)
export interface Payout {
  id: string;
  groupId: string;
  payoutType: PayoutType;
  totalAmount: number;
  status: PayoutStatus;
  initiatedBy: PayoutPerson;
  initiatedAt: string; // ISO timestamp
  approvedBy?: PayoutPerson;
  approvedAt?: string; // ISO timestamp
  executedAt?: string; // ISO timestamp
  requiresApprovalFrom?: 'chairperson' | 'treasurer';
  disbursements?: PayoutDisbursement[];
  note?: string;
  comment?: string; // Approval comment
  executionStatus?: string;
  rejectedBy?: PayoutPerson;
  rejectedAt?: string; // ISO timestamp
  rejectionReason?: string;
}

// Initiate Payout Request
export interface InitiatePayoutRequest {
  groupId: string;
  payoutType: PayoutType;
  recipientMemberId?: string; // Required for rotating, optional for year_end
  amount: number;
  note?: string;
}

// Initiate Payout Response
export interface InitiatePayoutResponse {
  id: string;
  groupId: string;
  payoutType: PayoutType;
  totalAmount: number;
  status: PayoutStatus;
  initiatedBy: PayoutPerson;
  initiatedAt: string;
  requiresApprovalFrom?: string;
}

// Approve Payout Request
export interface ApprovePayoutRequest {
  pin: string; // Encrypted PIN
  comment?: string;
}

// Approve Payout Response
export interface ApprovePayoutResponse {
  id: string;
  status: PayoutStatus;
  approvedBy: PayoutPerson;
  approvedAt: string;
  executionStatus: string;
}

// Reject Payout Request
export interface RejectPayoutRequest {
  pin: string; // Encrypted PIN
  reason: string;
}

// Get Payouts Filters
export interface PayoutFilters {
  groupId?: string;
  status?: PayoutStatus;
  fromDate?: string; // ISO date
  toDate?: string; // ISO date
  page?: number;
  limit?: number;
}

// Pagination
export interface Pagination {
  page: number;
  limit: number;
  total: number;
  pages: number;
}

// List Payouts Response
export interface ListPayoutsResponse {
  payouts: Payout[];
  pagination: Pagination;
}

// Payout Summary (for dashboard)
export interface PayoutSummary {
  totalPayouts: number;
  pendingApprovals: number;
  approvedAwaitingExecution: number;
  completed: number;
  totalAmountDisbursed: number;
}
