/**
 * Member Management Dashboard Component
 * Allows Chairpersons to view and manage group members
 * Design Reference: Section 5.2 - Group Management API, Section 7.1 - RBAC
 */

import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import {
  getGroupById,
  getGroupMembers,
  inviteMember,
  updateMemberRole,
  removeMember,
  getRoleDisplay,
  getStatusColor,
} from '../services/groupService';
import type { Group, GroupMember, MemberRole } from '../types/group';
import './MemberManagement.css';

const MemberManagement: React.FC = () => {
  const { groupId } = useParams<{ groupId: string }>();
  const navigate = useNavigate();
  const { token, user } = useAuth();

  const [group, setGroup] = useState<Group | null>(null);
  const [members, setMembers] = useState<GroupMember[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Invite form state
  const [invitePhone, setInvitePhone] = useState('');
  const [inviteRole, setInviteRole] = useState<MemberRole>('member');
  const [inviting, setInviting] = useState(false);
  const [inviteSuccess, setInviteSuccess] = useState<string | null>(null);

  // Check if current user is chairperson
  const isChairperson = user?.roles.includes('chairperson') || false;

  useEffect(() => {
    loadData();
  }, [groupId, token]);

  const loadData = async () => {
    if (!groupId || !token) return;

    try {
      setLoading(true);
      setError(null);

      const [groupData, membersData] = await Promise.all([
        getGroupById(groupId, token),
        getGroupMembers(groupId, token),
      ]);

      setGroup(groupData);
      setMembers(membersData.members);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load data');
    } finally {
      setLoading(false);
    }
  };

  const handleInviteMember = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!groupId || !token || !invitePhone) return;

    try {
      setInviting(true);
      setError(null);
      setInviteSuccess(null);

      await inviteMember(groupId, {
        phoneNumber: invitePhone,
        role: inviteRole,
      }, token);

      setInviteSuccess(`Successfully invited ${invitePhone} as ${inviteRole}`);
      setInvitePhone('');
      setInviteRole('member');

      // Reload members list
      setTimeout(() => {
        loadData();
        setInviteSuccess(null);
      }, 2000);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to invite member');
    } finally {
      setInviting(false);
    }
  };

  const handleRoleChange = async (memberId: string, newRole: MemberRole) => {
    if (!groupId || !token) return;

    try {
      setError(null);
      await updateMemberRole(groupId, memberId, { role: newRole }, token);
      
      // Update local state
      setMembers(members.map(m => 
        m.id === memberId ? { ...m, role: newRole } : m
      ));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update role');
    }
  };

  const handleRemoveMember = async (memberId: string, memberName: string) => {
    if (!groupId || !token) return;

    const confirmed = window.confirm(`Remove ${memberName} from the group?`);
    if (!confirmed) return;

    try {
      setError(null);
      await removeMember(groupId, memberId, token);
      
      // Update local state
      setMembers(members.filter(m => m.id !== memberId));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to remove member');
    }
  };

  if (loading) {
    return (
      <div className="member-management">
        <div className="loading">Loading member data...</div>
      </div>
    );
  }

  if (!group) {
    return (
      <div className="member-management">
        <div className="error">Group not found</div>
      </div>
    );
  }

  return (
    <div className="member-management">
      {/* Header */}
      <div className="management-header">
        <button className="back-button" onClick={() => navigate('/dashboard')}>
          ← Back to Dashboard
        </button>
        <h1>Member Management</h1>
        <div className="group-info">
          <h2>{group.name}</h2>
          <p className="group-meta">
            {members.length} member{members.length !== 1 ? 's' : ''} • 
            R{group.account?.balance.toFixed(2) || '0.00'} balance
          </p>
        </div>
      </div>

      {error && <div className="error-message">{error}</div>}
      {inviteSuccess && <div className="success-message">{inviteSuccess}</div>}

      {/* Invite Member Form (Chairperson only) */}
      {isChairperson && (
        <div className="invite-section">
          <h3>Invite New Member</h3>
          <form className="invite-form" onSubmit={handleInviteMember}>
            <div className="form-row">
              <input
                type="tel"
                placeholder="Phone number (e.g., +27821234567)"
                value={invitePhone}
                onChange={(e) => setInvitePhone(e.target.value)}
                required
                pattern="\+27[0-9]{9}"
                title="South African phone number starting with +27"
              />
              <select
                value={inviteRole}
                onChange={(e) => setInviteRole(e.target.value as MemberRole)}
              >
                <option value="member">Member</option>
                <option value="secretary">Secretary</option>
                <option value="treasurer">Treasurer</option>
                <option value="chairperson">Chairperson</option>
              </select>
              <button type="submit" disabled={inviting}>
                {inviting ? 'Inviting...' : 'Invite'}
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Members List */}
      <div className="members-section">
        <h3>Group Members</h3>
        <div className="members-table-container">
          <table className="members-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Phone</th>
                <th>Role</th>
                <th>Status</th>
                <th>Joined</th>
                {isChairperson && <th>Actions</th>}
              </tr>
            </thead>
            <tbody>
              {members.map((member) => (
                <tr key={member.id}>
                  <td className="member-name">{member.name}</td>
                  <td className="member-phone">{member.phone}</td>
                  <td className="member-role">
                    {isChairperson && member.status === 'active' ? (
                      <select
                        value={member.role}
                        onChange={(e) => handleRoleChange(member.id, e.target.value as MemberRole)}
                        className="role-select"
                      >
                        <option value="member">👤 Member</option>
                        <option value="secretary">📝 Secretary</option>
                        <option value="treasurer">💰 Treasurer</option>
                        <option value="chairperson">👑 Chairperson</option>
                      </select>
                    ) : (
                      <span className="role-badge">{getRoleDisplay(member.role)}</span>
                    )}
                  </td>
                  <td>
                    <span className={`status-badge status-${getStatusColor(member.status)}`}>
                      {member.status}
                    </span>
                  </td>
                  <td className="member-date">
                    {new Date(member.joinedAt).toLocaleDateString()}
                  </td>
                  {isChairperson && (
                    <td className="member-actions">
                      {member.role !== 'chairperson' && member.status === 'active' && (
                        <button
                          className="remove-button"
                          onClick={() => handleRemoveMember(member.id, member.name)}
                          title="Remove member"
                        >
                          Remove
                        </button>
                      )}
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default MemberManagement;
