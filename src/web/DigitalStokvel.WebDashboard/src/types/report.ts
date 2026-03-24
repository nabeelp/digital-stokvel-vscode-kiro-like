// Report types and interfaces for Digital Stokvel Web Dashboard
// Task 3.4.6: Reporting and Export Functionality

export type ReportType = 
  | 'contribution_history'
  | 'payout_history'
  | 'member_activity'
  | 'group_ledger'
  | 'annual_summary';

export type ExportFormat = 'pdf' | 'csv' | 'excel';

export interface ReportFilter {
  from_date?: string; // ISO 8601 date string
  to_date?: string; // ISO 8601 date string
  member_id?: string;
  status?: string;
}

export interface GenerateReportRequest {
  group_id: string;
  report_type: ReportType;
  filters?: ReportFilter;
  format?: ExportFormat; // Default: 'pdf'
}

export interface ReportMetadata {
  id: string;
  group_id: string;
  report_type: ReportType;
  format: ExportFormat;
  filters: ReportFilter;
  generated_at: string;
  generated_by: {
    id: string;
    name: string;
    role: string;
  };
  file_url: string;
  file_size_bytes: number;
  expires_at: string; // Presigned URL expiration
}

export interface ContributionHistoryReport {
  group_name: string;
  period: {
    from: string;
    to: string;
  };
  summary: {
    total_contributions: number;
    total_members: number;
    members_paid: number;
    members_pending: number;
    members_overdue: number;
    average_contribution: number;
  };
  contributions: Array<{
    member_name: string;
    phone: string;
    amount: number;
    status: string;
    due_date: string;
    paid_at?: string;
    transaction_ref?: string;
  }>;
}

export interface PayoutHistoryReport {
  group_name: string;
  period: {
    from: string;
    to: string;
  };
  summary: {
    total_payouts: number;
    total_amount: number;
    average_payout: number;
    payout_types: {
      rotating: number;
      year_end: number;
      emergency: number;
    };
  };
  payouts: Array<{
    payout_type: string;
    amount: number;
    recipient_name?: string;
    status: string;
    initiated_by: string;
    approved_by?: string;
    initiated_at: string;
    executed_at?: string;
  }>;
}

export interface MemberActivityReport {
  group_name: string;
  period: {
    from: string;
    to: string;
  };
  members: Array<{
    member_name: string;
    phone: string;
    role: string;
    joined_at: string;
    total_contributions: number;
    on_time_payments: number;
    late_payments: number;
    missed_payments: number;
    contribution_streak: number;
    last_contribution_date?: string;
  }>;
}

export interface GroupLedgerReport {
  group_name: string;
  account_number: string;
 period: {
    from: string;
    to: string;
  };
  opening_balance: number;
  closing_balance: number;
  transactions: Array<{
    date: string;
    type: 'contribution' | 'payout' | 'interest' | 'fee';
    description: string;
    member_name?: string;
    debit?: number;
    credit?: number;
    balance: number;
    reference: string;
  }>;
  summary: {
    total_contributions: number;
    total_payouts: number;
    total_interest: number;
    total_fees: number;
  };
}

export interface AnnualSummaryReport {
  group_name: string;
  year: number;
  member_summary: {
    member_name: string;
    total_contributions: number;
    total_payouts: number;
    interest_earned: number;
    on_time_percentage: number;
    contribution_streak: number;
  };
  group_summary: {
    total_members: number;
    total_contributions: number;
    total_payouts: number;
    total_interest_earned: number;
    average_contribution: number;
    group_performance_score: number;
  };
  monthly_breakdown: Array<{
    month: string;
    contributions: number;
    payouts: number;
    interest: number;
    balance: number;
  }>;
}

export interface DataExportRequest {
  format: ExportFormat;
  include_sections?: Array<'profile' | 'contributions' | 'payouts' | 'groups' | 'votes' | 'disputes'>;
}

export interface ListReportsResponse {
  reports: ReportMetadata[];
  pagination: {
    page: number;
    page_size: number;
    total_count: number;
    total_pages: number;
  };
}
