/**
 * Group Management Type Definitions
 * Based on design.md Section 4.2 (Database Schema) and Section 5.2 (Group Management API)
 */

export type GroupType = 'savings_pot' | 'rotating_credit' | 'burial_society';
export type GroupStatus = 'active' | 'suspended' | 'closed';
export type MemberRole = 'chairperson' | 'treasurer' | 'secretary' | 'member';
export type MemberStatus = 'active' | 'invited' | 'inactive' | 'removed';
export type PayoutType = 'year_end' | 'rotating' | 'emergency';
export type ContributionFrequency = 'weekly' | 'monthly' | 'quarterly';

// Group Constitution
export interface GroupConstitution {
  gracePeriodDays: number;
  lateFee: number;
  missedPaymentsThreshold: number;
  quorumPercentage: number;
}

// Payout Schedule
export interface PayoutSchedule {
  type: PayoutType;
  date?: string; // ISO date string for year_end
  interval?: number; // Months for rotating
}

// Group Account Information
export interface GroupAccount {
  accountNumber: string;
  balance: number;
  totalContributions: number;
  totalInterestEarned: number;
  totalPayouts?: number;
  interestRate: number;
}

// Group Member (for member list display)
export interface GroupMember {
  id: string;
  userId: string;
  groupId: string;
  name: string;
  phone: string;
  role: MemberRole;
  status: MemberStatus;
  joinedAt: string; // ISO timestamp
  leftAt?: string; // ISO timestamp
  contributionHistory?: {
    totalPaid: number;
    onTimeCount: number;
    lateCount: number;
    missedCount: number;
  };
}

// Group (full details)
export interface Group {
  id: string;
  name: string;
  description?: string;
  groupType: GroupType;
  contributionAmount: number;
  contributionFrequency: ContributionFrequency;
  status: GroupStatus;
  account?: GroupAccount;
  memberCount: number;
  nextContributionDue?: string; // ISO date
  nextPayout?: {
    type: PayoutType;
    date: string;
    estimatedAmount: number;
  };
  constitution: GroupConstitution;
  createdAt: string;
  updatedAt?: string;
}

// Group Summary (for list views)
export interface GroupSummary {
  id: string;
  name: string;
  role: MemberRole;
  balance: number;
  memberCount: number;
  nextContributionDue?: string;
  status: GroupStatus;
}

// Invitation
export interface InvitedMember {
  phoneNumber: string;
  role: MemberRole;
}

export interface InvitationLink {
  memberId: string;
  inviteToken: string;
  shareLink: string;
}

// Create Group Request
export interface CreateGroupRequest {
  name: string;
  description?: string;
  groupType: GroupType;
  contributionAmount: number;
  contributionFrequency: ContributionFrequency;
  payoutSchedule: PayoutSchedule;
  constitution: GroupConstitution;
  invitedMembers?: InvitedMember[];
}

// Create Group Response
export interface CreateGroupResponse {
  id: string;
  name: string;
  groupType: GroupType;
  contributionAmount: number;
  contributionFrequency: ContributionFrequency;
  status: GroupStatus;
  accountNumber: string;
  memberCount: number;
  createdAt: string;
  invitationLinks?: InvitationLink[];
}

// Invite Member Request
export interface InviteMemberRequest {
  phoneNumber: string;
  role: MemberRole;
}

// Invite Member Response
export interface InviteMemberResponse {
  memberId: string;
  inviteToken: string;
  shareLink: string;
  smsScheduled: boolean;
}

// Update Member Role Request
export interface UpdateMemberRoleRequest {
  role: MemberRole;
}

// Pagination
export interface Pagination {
  page: number;
  limit: number;
  total: number;
  pages: number;
}

// List Groups Response
export interface ListGroupsResponse {
  groups: GroupSummary[];
  pagination: Pagination;
}

// List Group Members Response
export interface ListGroupMembersResponse {
  members: GroupMember[];
  pagination?: Pagination;
}
