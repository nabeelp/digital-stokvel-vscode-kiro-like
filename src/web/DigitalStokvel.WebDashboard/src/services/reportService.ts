// Reporting service for Digital Stokvel Web Dashboard
// Task 3.4.6: Reporting and Export Functionality

import type {
  ReportType,
  ExportFormat,
  GenerateReportRequest,
  ReportMetadata,
  ContributionHistoryReport,
  PayoutHistoryReport,
  MemberActivityReport,
  GroupLedgerReport,
  AnnualSummaryReport,
  DataExportRequest,
  ListReportsResponse,
} from '../types/report';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

/**
 * Generate a report for a group
 */
export async function generateReport(
  request: GenerateReportRequest,
  token: string
): Promise<ReportMetadata> {
  const response = await fetch(`${API_BASE_URL}/v1/reports/generate`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || 'Failed to generate report');
  }

  return response.json();
}

/**
 * Get list of generated reports for a group
 */
export async function getGroupReports(
  groupId: string,
  token: string,
  page: number = 1,
  pageSize: number = 20
): Promise<ListReportsResponse> {
  const response = await fetch(
    `${API_BASE_URL}/v1/groups/${groupId}/reports?page=${page}&page_size=${pageSize}`,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || 'Failed to fetch reports');
  }

  return response.json();
}

/**
 * Get report metadata by ID
 */
export async function getReportById(
  reportId: string,
  token: string
): Promise<ReportMetadata> {
  const response = await fetch(`${API_BASE_URL}/v1/reports/${reportId}`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || 'Failed to fetch report');
  }

  return response.json();
}

/**
 * Get contribution history report data (for preview)
 */
export async function getContributionHistoryReport(
  groupId: string,
  token: string,
  fromDate?: string,
  toDate?: string
): Promise<ContributionHistoryReport> {
  const params = new URLSearchParams({ group_id: groupId });
  if (fromDate) params.append('from_date', fromDate);
  if (toDate) params.append('to_date', toDate);

  const response = await fetch(
    `${API_BASE_URL}/v1/reports/contribution-history?${params.toString()}`,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || 'Failed to fetch contribution history');
  }

  return response.json();
}

/**
 * Get payout history report data (for preview)
 */
export async function getPayoutHistoryReport(
  groupId: string,
  token: string,
  fromDate?: string,
  toDate?: string
): Promise<PayoutHistoryReport> {
  const params = new URLSearchParams({ group_id: groupId });
  if (fromDate) params.append('from_date', fromDate);
  if (toDate) params.append('to_date', toDate);

  const response = await fetch(
    `${API_BASE_URL}/v1/reports/payout-history?${params.toString()}`,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || 'Failed to fetch payout history');
  }

  return response.json();
}

/**
 * Get member activity report data (for preview)
 */
export async function getMemberActivityReport(
  groupId: string,
  token: string,
  fromDate?: string,
  toDate?: string
): Promise<MemberActivityReport> {
  const params = new URLSearchParams({ group_id: groupId });
  if (fromDate) params.append('from_date', fromDate);
  if (toDate) params.append('to_date', toDate);

  const response = await fetch(
    `${API_BASE_URL}/v1/reports/member-activity?${params.toString()}`,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || 'Failed to fetch member activity');
  }

  return response.json();
}

/**
 * Get group ledger report data (for preview)
 */
export async function getGroupLedgerReport(
  groupId: string,
  token: string,
  fromDate?: string,
  toDate?: string
): Promise<GroupLedgerReport> {
  const params = new URLSearchParams({ group_id: groupId });
  if (fromDate) params.append('from_date', fromDate);
  if (toDate) params.append('to_date', toDate);

  const response = await fetch(
    `${API_BASE_URL}/v1/reports/group-ledger?${params.toString()}`,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || 'Failed to fetch group ledger');
  }

  return response.json();
}

/**
 * Get annual summary report data (for preview)
 */
export async function getAnnualSummaryReport(
  groupId: string,
  token: string,
  year: number
): Promise<AnnualSummaryReport> {
  const params = new URLSearchParams({ 
    group_id: groupId,
    year: year.toString()
  });

  const response = await fetch(
    `${API_BASE_URL}/v1/reports/annual-summary?${params.toString()}`,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || 'Failed to fetch annual summary');
  }

  return response.json();
}

/**
 * Export user's personal data (POPIA compliance)
 */
export async function exportUserData(
  request: DataExportRequest,
  token: string
): Promise<ReportMetadata> {
  const response = await fetch(`${API_BASE_URL}/v1/users/me/data-export`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || 'Failed to export user data');
  }

  return response.json();
}

/**
 * Download a report file
 * @param fileUrl - Presigned URL from report metadata
 */
export async function downloadReport(fileUrl: string): Promise<void> {
  // Create a temporary link and trigger download
  const link = document.createElement('a');
  link.href = fileUrl;
  link.download = ''; // Filename comes from Content-Disposition header
  link.target = '_blank';
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
}

/**
 * Get display name for report type
 */
export function getReportTypeDisplay(type: ReportType): string {
  const displayNames: Record<ReportType, string> = {
    contribution_history: 'Contribution History',
    payout_history: 'Payout History',
    member_activity: 'Member Activity',
    group_ledger: 'Group Ledger',
    annual_summary: 'Annual Summary',
  };
  return displayNames[type];
}

/**
 * Get icon for report type
 */
export function getReportTypeIcon(type: ReportType): string {
  const icons: Record<ReportType, string> = {
    contribution_history: '💰',
    payout_history: '📤',
    member_activity: '👥',
    group_ledger: '📊',
    annual_summary: '📅',
  };
  return icons[type];
}

/**
 * Get icon for export format
 */
export function getFormatIcon(format: ExportFormat): string {
  const icons: Record<ExportFormat, string> = {
    pdf: '📄',
    csv: '📋',
    excel: '📗',
  };
  return icons[format];
}

/**
 * Format file size for display
 */
export function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

/**
 * Format date for display
 */
export function formatDate(dateString: string): string {
  const date = new Date(dateString);
  return date.toLocaleDateString('en-ZA', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });
}

/**
 * Format date/time for display
 */
export function formatDateTime(dateString: string): string {
  const date = new Date(dateString);
  return date.toLocaleString('en-ZA', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

/**
 * Format currency for display
 */
export function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('en-ZA', {
    style: 'currency',
    currency: 'ZAR',
    minimumFractionDigits: 2,
  }).format(amount);
}

/**
 * Get date range presets
 */
export interface DateRangePreset {
  label: string;
  from: string;
  to: string;
}

export function getDateRangePresets(): DateRangePreset[] {
  const today = new Date();
  const firstDayOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);
  const lastMonthFirstDay = new Date(today.getFullYear(), today.getMonth() - 1, 1);
  const lastMonthLastDay = new Date(today.getFullYear(), today.getMonth(), 0);
  const firstDayOfYear = new Date(today.getFullYear(), 0, 1);
  const lastYearFirstDay = new Date(today.getFullYear() - 1, 0, 1);
  const lastYearLastDay = new Date(today.getFullYear() - 1, 11, 31);

  return [
    {
      label: 'This Month',
      from: firstDayOfMonth.toISOString().split('T')[0],
      to: today.toISOString().split('T')[0],
    },
    {
      label: 'Last Month',
      from: lastMonthFirstDay.toISOString().split('T')[0],
      to: lastMonthLastDay.toISOString().split('T')[0],
    },
    {
      label: 'This Year',
      from: firstDayOfYear.toISOString().split('T')[0],
      to: today.toISOString().split('T')[0],
    },
    {
      label: 'Last Year',
      from: lastYearFirstDay.toISOString().split('T')[0],
      to: lastYearLastDay.toISOString().split('T')[0],
    },
  ];
}
