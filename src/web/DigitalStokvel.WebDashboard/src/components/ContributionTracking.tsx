/**
 * Contribution Tracking Component
 * Displays contribution history with filters and summary statistics
 * Design Reference: Section 5.3 - Contribution API
 */

import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { getGroupById } from '../services/groupService';
import {
  getGroupContributions,
  getStatusColor,
  getStatusDisplay,
  formatCurrency,
  formatDate,
  formatDateTime,
} from '../services/contributionService';
import type { Group } from '../types/group';
import type {
  Contribution,
  ContributionStatus,
  ContributionSummary,
  ContributionFilters,
} from '../types/contribution';
import './ContributionTracking.css';

const ContributionTracking: React.FC = () => {
  const { groupId } = useParams<{ groupId: string }>();
  const navigate = useNavigate();
  const { token } = useAuth();

  const [group, setGroup] = useState<Group | null>(null);
  const [contributions, setContributions] = useState<Contribution[]>([]);
  const [summary, setSummary] = useState<ContributionSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Filter state
  const [statusFilter, setStatusFilter] = useState<ContributionStatus | 'all'>('all');
  const [searchQuery, setSearchQuery] = useState('');
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');

  // Pagination
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  useEffect(() => {
    loadData();
  }, [groupId, token, statusFilter, fromDate, toDate, currentPage]);

  const loadData = async () => {
    if (!groupId || !token) return;

    try {
      setLoading(true);
      setError(null);

      const filters: ContributionFilters = {
        page: currentPage,
        limit: 20,
      };

      if (statusFilter !== 'all') {
        filters.status = statusFilter;
      }

      if (fromDate) {
        filters.fromDate = fromDate;
      }

      if (toDate) {
        filters.toDate = toDate;
      }

      const [groupData, contributionsData] = await Promise.all([
        getGroupById(groupId, token),
        getGroupContributions(groupId, token, filters),
      ]);

      setGroup(groupData);
      setContributions(contributionsData.contributions);
      setSummary(contributionsData.summary);
      setTotalPages(contributionsData.pagination.pages);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load data');
    } finally {
      setLoading(false);
    }
  };

  const handleClearFilters = () => {
    setStatusFilter('all');
    setSearchQuery('');
    setFromDate('');
    setToDate('');
    setCurrentPage(1);
  };

  // Filter contributions by search query (client-side)
  const filteredContributions = contributions.filter((contribution) => {
    if (!searchQuery) return true;
    const query = searchQuery.toLowerCase();
    return (
      contribution.member.name.toLowerCase().includes(query) ||
      contribution.member.phone.includes(query) ||
      contribution.transactionRef.toLowerCase().includes(query)
    );
  });

  if (loading && !group) {
    return (
      <div className="contribution-tracking">
        <div className="loading">Loading contribution data...</div>
      </div>
    );
  }

  if (!group) {
    return (
      <div className="contribution-tracking">
        <div className="error">Group not found</div>
      </div>
    );
  }

  return (
    <div className="contribution-tracking">
      {/* Header */}
      <div className="tracking-header">
        <button className="back-button" onClick={() => navigate('/dashboard')}>
          ← Back to Dashboard
        </button>
        <h1>Contribution Tracking</h1>
        <div className="group-info">
          <h2>{group.name}</h2>
          <p className="group-meta">
            {group.memberCount} member{group.memberCount !== 1 ? 's' : ''} • 
            {formatCurrency(group.account?.balance || 0)} balance
          </p>
        </div>
      </div>

      {error && <div className="error-message">{error}</div>}

      {/* Summary Cards */}
      {summary && (
        <div className="summary-cards">
          <div className="summary-card total">
            <div className="card-icon">💰</div>
            <div className="card-content">
              <span className="card-label">Total Contributions</span>
              <span className="card-value">{formatCurrency(summary.totalContributions)}</span>
            </div>
          </div>
          <div className="summary-card paid">
            <div className="card-icon">✅</div>
            <div className="card-content">
              <span className="card-label">Members Paid</span>
              <span className="card-value">{summary.totalMembersPaid}</span>
            </div>
          </div>
          <div className="summary-card pending">
            <div className="card-icon">⏳</div>
            <div className="card-content">
              <span className="card-label">Pending</span>
              <span className="card-value">{summary.totalMembersPending}</span>
            </div>
          </div>
          <div className="summary-card overdue">
            <div className="card-icon">⚠️</div>
            <div className="card-content">
              <span className="card-label">Overdue</span>
              <span className="card-value">{summary.totalMembersOverdue}</span>
            </div>
          </div>
        </div>
      )}

      {/* Filters */}
      <div className="filters-section">
        <div className="filters-row">
          <input
            type="text"
            placeholder="Search by name, phone, or transaction ref"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="search-input"
          />
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value as ContributionStatus | 'all')}
            className="filter-select"
          >
            <option value="all">All Statuses</option>
            <option value="completed">Completed</option>
            <option value="pending">Pending</option>
            <option value="late">Late</option>
            <option value="overdue">Overdue</option>
            <option value="missed">Missed</option>
          </select>
          <input
            type="date"
            value={fromDate}
            onChange={(e) => setFromDate(e.target.value)}
            placeholder="From date"
            className="date-input"
          />
          <input
            type="date"
            value={toDate}
            onChange={(e) => setToDate(e.target.value)}
            placeholder="To date"
            className="date-input"
          />
          {(statusFilter !== 'all' || searchQuery || fromDate || toDate) && (
            <button onClick={handleClearFilters} className="clear-button">
              Clear Filters
            </button>
          )}
        </div>
      </div>

      {/* Contributions Table */}
      <div className="contributions-section">
        <h3>Contribution History</h3>
        {loading ? (
          <div className="loading-text">Loading contributions...</div>
        ) : filteredContributions.length === 0 ? (
          <div className="empty-state">
            <p>No contributions found.</p>
            {(statusFilter !== 'all' || searchQuery || fromDate || toDate) && (
              <p>Try adjusting your filters.</p>
            )}
          </div>
        ) : (
          <>
            <div className="table-container">
              <table className="contributions-table">
                <thead>
                  <tr>
                    <th>Member</th>
                    <th>Amount</th>
                    <th>Status</th>
                    <th>Due Date</th>
                    <th>Paid At</th>
                    <th>Transaction Ref</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredContributions.map((contribution) => (
                    <tr key={contribution.id}>
                      <td className="member-cell">
                        <div className="member-name">{contribution.member.name}</div>
                        <div className="member-phone">{contribution.member.phone}</div>
                      </td>
                      <td className="amount-cell">
                        {formatCurrency(contribution.amount)}
                        {contribution.lateFee && (
                          <div className="late-fee">+ {formatCurrency(contribution.lateFee)} late fee</div>
                        )}
                      </td>
                      <td>
                        <span className={`status-badge status-${getStatusColor(contribution.status)}`}>
                          {getStatusDisplay(contribution.status)}
                        </span>
                      </td>
                      <td className="date-cell">{formatDate(contribution.dueDate)}</td>
                      <td className="date-cell">
                        {contribution.paidAt ? formatDateTime(contribution.paidAt) : '—'}
                      </td>
                      <td className="ref-cell">{contribution.transactionRef}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {/* Pagination */}
            {totalPages > 1 && (
              <div className="pagination">
                <button
                  onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
                  disabled={currentPage === 1}
                  className="page-button"
                >
                  Previous
                </button>
                <span className="page-info">
                  Page {currentPage} of {totalPages}
                </span>
                <button
                  onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
                  disabled={currentPage === totalPages}
                  className="page-button"
                >
                  Next
                </button>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
};

export default ContributionTracking;
