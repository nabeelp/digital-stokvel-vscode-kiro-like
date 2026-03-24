/**
 * Payout Approval Component
 * Allows Treasurers to approve or reject payouts initiated by Chairpersons
 * Design Reference: Section 5.4 - Payout API, Section 2.2.3 - Payout Service
 */

import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { getGroupById } from '../services/groupService';
import {
  getGroupPayouts,
  approvePayout,
  rejectPayout,
  getStatusColor,
  getStatusDisplay,
  formatCurrency,
  formatDateTime,
  canApprovePayout,
} from '../services/payoutService';
import type { Group } from '../types/group';
import type { Payout, PayoutStatus } from '../types/payout';
import './PayoutApproval.css';

const PayoutApproval: React.FC = () => {
  const { groupId } = useParams<{ groupId: string }>();
  const navigate = useNavigate();
  const { token, user } = useAuth();

  const [group, setGroup] = useState<Group | null>(null);
  const [payouts, setPayouts] = useState<Payout[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Filter state
  const [statusFilter, setStatusFilter] = useState<PayoutStatus | 'all'>('pending_treasurer_approval');

  // Approval state
  const [approvingId, setApprovingId] = useState<string | null>(null);
  const [rejectingId, setRejectingId] = useState<string | null>(null);
  const [pin, setPin] = useState('');
  const [comment, setComment] = useState('');
  const [rejectionReason, setRejectionReason] = useState('');
  const [actionSuccess, setActionSuccess] = useState<string | null>(null);

  useEffect(() => {
    loadData();
  }, [groupId, token, statusFilter]);

  const loadData = async () => {
    if (!groupId || !token) return;

    try {
      setLoading(true);
      setError(null);

      const [groupData, payoutsData] = await Promise.all([
        getGroupById(groupId, token),
        getGroupPayouts(groupId, token, {
          status: statusFilter === 'all' ? undefined : statusFilter,
          limit: 50,
        }),
      ]);

      setGroup(groupData);
      setPayouts(payoutsData.payouts);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load data');
    } finally {
      setLoading(false);
    }
  };

  const handleApprove = async (payoutId: string) => {
    if (!pin || !token) {
      setError('Please enter your PIN');
      return;
    }

    try {
      setError(null);
      await approvePayout(payoutId, { pin, comment }, token);
      
      setActionSuccess('Payout approved successfully!');
      setApprovingId(null);
      setPin('');
      setComment('');

      // Reload data
      setTimeout(() => {
        loadData();
        setActionSuccess(null);
      }, 2000);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to approve payout');
    }
  };

  const handleReject = async (payoutId: string) => {
    if (!pin || !rejectionReason || !token) {
      setError('Please enter your PIN and rejection reason');
      return;
    }

    try {
      setError(null);
      await rejectPayout(payoutId, { pin, reason: rejectionReason }, token);
      
      setActionSuccess('Payout rejected successfully');
      setRejectingId(null);
      setPin('');
      setRejectionReason('');

      // Reload data
      setTimeout(() => {
        loadData();
        setActionSuccess(null);
      }, 2000);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to reject payout');
    }
  };

  const pendingPayouts = payouts.filter(p => 
    p.status === 'pending_treasurer_approval' || p.status === 'pending_chairperson_approval'
  );

  if (loading && !group) {
    return (
      <div className="payout-approval">
        <div className="loading">Loading payout data...</div>
      </div>
    );
  }

  if (!group) {
    return (
      <div className="payout-approval">
        <div className="error">Group not found</div>
      </div>
    );
  }

  return (
    <div className="payout-approval">
      {/* Header */}
      <div className="approval-header">
        <button className="back-button" onClick={() => navigate('/dashboard')}>
          ← Back to Dashboard
        </button>
        <h1>Payout Approval</h1>
        <div className="group-info">
          <h2>{group.name}</h2>
          <p className="group-meta">
            {formatCurrency(group.account?.balance || 0)} available for payouts
          </p>
        </div>
      </div>

      {error && <div className="error-message">{error}</div>}
      {actionSuccess && <div className="success-message">{actionSuccess}</div>}

      {/* Filter */}
      <div className="filter-section">
        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value as PayoutStatus | 'all')}
          className="status-filter"
        >
          <option value="pending_treasurer_approval">Pending My Approval</option>
          <option value="approved">Approved</option>
          <option value="processing">Processing</option>
          <option value="completed">Completed</option>
          <option value="rejected">Rejected</option>
          <option value="all">All Payouts</option>
        </select>
      </div>

      {/* Pending Approvals Section */}
      {statusFilter === 'pending_treasurer_approval' && (
        <div className="pending-section">
          <h3>Payouts Awaiting Your Approval ({pendingPayouts.length})</h3>
          {loading ? (
            <div className="loading-text">Loading payouts...</div>
          ) : pendingPayouts.length === 0 ? (
            <div className="empty-state">
              <p>✅ No payouts pending approval</p>
            </div>
          ) : (
            <div className="payouts-grid">
              {pendingPayouts.map((payout) => (
                <div key={payout.id} className="payout-card pending">
                  <div className="card-header">
                    <span className={`type-badge ${payout.payoutType}`}>
                      {payout.payoutType === 'rotating' && '🔄'}
                      {payout.payoutType === 'year_end' && '🎉'}
                      {payout.payoutType === 'emergency' && '🚨'}
                      {' '}{payout.payoutType.replace('_', ' ')}
                    </span>
                    <span className={`status-badge status-${getStatusColor(payout.status)}`}>
                      {getStatusDisplay(payout.status)}
                    </span>
                  </div>

                  <div className="card-amount">{formatCurrency(payout.totalAmount)}</div>

                  <div className="card-details">
                    <div className="detail-row">
                      <span className="detail-label">Initiated by:</span>
                      <span className="detail-value">{payout.initiatedBy.name}</span>
                    </div>
                    <div className="detail-row">
                      <span className="detail-label">Date:</span>
                      <span className="detail-value">{formatDateTime(payout.initiatedAt)}</span>
                    </div>
                    {payout.note && (
                      <div className="detail-row">
                        <span className="detail-label">Note:</span>
                        <span className="detail-value note-text">{payout.note}</span>
                      </div>
                    )}
                  </div>

                  {canApprovePayout(payout, user?.roles[0] || '') && (
                    <div className="card-actions">
                      {approvingId === payout.id ? (
                        <div className="approval-form">
                          <input
                            type="password"
                            placeholder="Enter PIN"
                            value={pin}
                            onChange={(e) => setPin(e.target.value)}
                            maxLength={4}
                            className="pin-input"
                          />
                          <textarea
                            placeholder="Comment (optional)"
                            value={comment}
                            onChange={(e) => setComment(e.target.value)}
                            rows={2}
                            className="comment-input"
                          />
                          <div className="form-buttons">
                            <button
                              onClick={() => handleApprove(payout.id)}
                              className="confirm-button"
                            >
                              Confirm Approval
                            </button>
                            <button
                              onClick={() => {
                                setApprovingId(null);
                                setPin('');
                                setComment('');
                              }}
                              className="cancel-button"
                            >
                              Cancel
                            </button>
                          </div>
                        </div>
                      ) : rejectingId === payout.id ? (
                        <div className="rejection-form">
                          <input
                            type="password"
                            placeholder="Enter PIN"
                            value={pin}
                            onChange={(e) => setPin(e.target.value)}
                            maxLength={4}
                            className="pin-input"
                          />
                          <textarea
                            placeholder="Rejection reason (required)"
                            value={rejectionReason}
                            onChange={(e) => setRejectionReason(e.target.value)}
                            rows={3}
                            className="reason-input"
                            required
                          />
                          <div className="form-buttons">
                            <button
                              onClick={() => handleReject(payout.id)}
                              className="confirm-reject-button"
                            >
                              Confirm Rejection
                            </button>
                            <button
                              onClick={() => {
                                setRejectingId(null);
                                setPin('');
                                setRejectionReason('');
                              }}
                              className="cancel-button"
                            >
                              Cancel
                            </button>
                          </div>
                        </div>
                      ) : (
                        <div className="action-buttons">
                          <button
                            onClick={() => setApprovingId(payout.id)}
                            className="approve-button"
                          >
                            ✅ Approve
                          </button>
                          <button
                            onClick={() => setRejectingId(payout.id)}
                            className="reject-button"
                          >
                            ⛔ Reject
                          </button>
                        </div>
                      )}
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* All Payouts Section (when not filtering for pending) */}
      {statusFilter !== 'pending_treasurer_approval' && (
        <div className="all-payouts-section">
          <h3>Payouts</h3>
          {loading ? (
            <div className="loading-text">Loading payouts...</div>
          ) : payouts.length === 0 ? (
            <div className="empty-state">
              <p>No payouts found</p>
            </div>
          ) : (
            <div className="payouts-table-container">
              <table className="payouts-table">
                <thead>
                  <tr>
                    <th>Type</th>
                    <th>Amount</th>
                    <th>Status</th>
                    <th>Initiated By</th>
                    <th>Initiated At</th>
                    <th>Approved At</th>
                  </tr>
                </thead>
                <tbody>
                  {payouts.map((payout) => (
                    <tr key={payout.id}>
                      <td>
                        <span className={`type-badge ${payout.payoutType}`}>
                          {payout.payoutType.replace('_', ' ')}
                        </span>
                      </td>
                      <td className="amount-cell">{formatCurrency(payout.totalAmount)}</td>
                      <td>
                        <span className={`status-badge status-${getStatusColor(payout.status)}`}>
                          {getStatusDisplay(payout.status)}
                        </span>
                      </td>
                      <td>{payout.initiatedBy.name}</td>
                      <td className="date-cell">{formatDateTime(payout.initiatedAt)}</td>
                      <td className="date-cell">
                        {payout.approvedAt ? formatDateTime(payout.approvedAt) : '—'}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default PayoutApproval;
