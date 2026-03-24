/**
 * Group Service
 * Handles all group management API calls
 * Design Reference: Section 5.2 - Group Management API
 */

import type {
  Group,
  GroupMember,
  CreateGroupRequest,
  CreateGroupResponse,
  InviteMemberRequest,
  InviteMemberResponse,
  UpdateMemberRoleRequest,
  ListGroupsResponse,
  ListGroupMembersResponse,
} from '../types/group';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

/**
 * Get all groups for the current user
 */
export async function getUserGroups(
  token: string,
  status?: 'active' | 'suspended' | 'closed',
  page: number = 1,
  limit: number = 20
): Promise<ListGroupsResponse> {
  const params = new URLSearchParams({
    page: page.toString(),
    limit: limit.toString(),
  });
  
  if (status) {
    params.append('status', status);
  }

  const response = await fetch(`${API_BASE_URL}/v1/users/me/groups?${params}`, {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: 'Failed to fetch groups' }));
    throw new Error(error.message || `HTTP ${response.status}: Failed to fetch groups`);
  }

  return response.json();
}

/**
 * Get group details by ID
 */
export async function getGroupById(groupId: string, token: string): Promise<Group> {
  const response = await fetch(`${API_BASE_URL}/v1/groups/${groupId}`, {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: 'Failed to fetch group' }));
    throw new Error(error.message || `HTTP ${response.status}: Failed to fetch group`);
  }

  return response.json();
}

/**
 * Get all members of a group
 */
export async function getGroupMembers(
  groupId: string,
  token: string,
  page?: number,
  limit?: number
): Promise<ListGroupMembersResponse> {
  let url = `${API_BASE_URL}/v1/groups/${groupId}/members`;
  
  if (page !== undefined && limit !== undefined) {
    const params = new URLSearchParams({
      page: page.toString(),
      limit: limit.toString(),
    });
    url += `?${params}`;
  }

  const response = await fetch(url, {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: 'Failed to fetch members' }));
    throw new Error(error.message || `HTTP ${response.status}: Failed to fetch members`);
  }

  return response.json();
}

/**
 * Create a new group
 */
export async function createGroup(
  request: CreateGroupRequest,
  token: string
): Promise<CreateGroupResponse> {
  const response = await fetch(`${API_BASE_URL}/v1/groups`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: 'Failed to create group' }));
    throw new Error(error.message || `HTTP ${response.status}: Failed to create group`);
  }

  return response.json();
}

/**
 * Invite a new member to a group
 */
export async function inviteMember(
  groupId: string,
  request: InviteMemberRequest,
  token: string
): Promise<InviteMemberResponse> {
  const response = await fetch(`${API_BASE_URL}/v1/groups/${groupId}/members`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: 'Failed to invite member' }));
    throw new Error(error.message || `HTTP ${response.status}: Failed to invite member`);
  }

  return response.json();
}

/**
 * Update a member's role in a group
 * Requires Chairperson permission
 */
export async function updateMemberRole(
  groupId: string,
  memberId: string,
  request: UpdateMemberRoleRequest,
  token: string
): Promise<GroupMember> {
  const response = await fetch(`${API_BASE_URL}/v1/groups/${groupId}/members/${memberId}`, {
    method: 'PATCH',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: 'Failed to update member role' }));
    throw new Error(error.message || `HTTP ${response.status}: Failed to update member role`);
  }

  return response.json();
}

/**
 * Remove a member from a group
 * Requires Chairperson permission
 */
export async function removeMember(
  groupId: string,
  memberId: string,
  token: string
): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/v1/groups/${groupId}/members/${memberId}`, {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: 'Failed to remove member' }));
    throw new Error(error.message || `HTTP ${response.status}: Failed to remove member`);
  }
}

/**
 * Get role display name with emoji
 */
export function getRoleDisplay(role: string): string {
  switch (role) {
    case 'chairperson':
      return '👑 Chairperson';
    case 'treasurer':
      return '💰 Treasurer';
    case 'secretary':
      return '📝 Secretary';
    case 'member':
      return '👤 Member';
    default:
      return role;
  }
}

/**
 * Get status badge color
 */
export function getStatusColor(status: string): string {
  switch (status) {
    case 'active':
      return 'green';
    case 'invited':
      return 'blue';
    case 'inactive':
      return 'gray';
    case 'removed':
      return 'red';
    default:
      return 'gray';
  }
}
