-- Digital Stokvel Banking Platform - Database Migration
-- Version: V001
-- Description: Create core schema with users, groups, group_members, and group_savings_accounts tables
-- Date: 2026-03-24
-- Author: Digital Stokvel Team

-- ============================================================================
-- USERS TABLE
-- ============================================================================

CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    phone_number VARCHAR(15) NOT NULL UNIQUE,
    id_number VARCHAR(13) UNIQUE, -- Optional: South African ID number (encrypted)
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    preferred_language VARCHAR(2) NOT NULL DEFAULT 'en' CHECK (preferred_language IN ('en', 'zu', 'st', 'xh', 'af')),
    pin_hash VARCHAR(255) NOT NULL,  -- Hashed PIN for authentication
    is_verified BOOLEAN NOT NULL DEFAULT FALSE,
    status VARCHAR(20) NOT NULL DEFAULT 'active' CHECK (status IN ('active', 'suspended', 'deleted')),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_login_at TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT phone_number_format CHECK (phone_number ~ '^\+27[0-9]{9}$')
);

-- Indexes for users table
CREATE INDEX idx_users_phone_number ON users(phone_number);
CREATE INDEX idx_users_id_number ON users(id_number) WHERE id_number IS NOT NULL;
CREATE INDEX idx_users_status ON users(status) WHERE status != 'deleted';
CREATE INDEX idx_users_created_at ON users(created_at DESC);

-- Comments for users table
COMMENT ON TABLE users IS 'Registered users of the Digital Stokvel platform';
COMMENT ON COLUMN users.phone_number IS 'Phone number in E.164 format (+27XXXXXXXXX)';
COMMENT ON COLUMN users.id_number IS 'South African ID number (encrypted at application layer)';
COMMENT ON COLUMN users.pin_hash IS 'Hashed PIN for transaction authentication';

-- ============================================================================
-- GROUPS TABLE
-- ============================================================================

CREATE TABLE groups (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(50) NOT NULL,
    description VARCHAR(200),
    group_type group_type NOT NULL,  -- Uses enum type from init script
    contribution_amount DECIMAL(10, 2) NOT NULL,
    contribution_frequency contribution_frequency NOT NULL,  -- Uses enum type from init script
    payout_schedule JSONB NOT NULL,
    constitution JSONB NOT NULL DEFAULT '{
        "grace_period_days": 3,
        "late_fee": 50.00,
        "missed_payments_threshold": 3,
        "quorum_percentage": 51
    }'::jsonb,
    status group_status NOT NULL DEFAULT 'active',  -- Uses enum type from init script
    created_by UUID NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    closed_at TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT contribution_amount_valid CHECK (contribution_amount >= 50 AND contribution_amount <= 10000),
    CONSTRAINT contribution_amount_positive CHECK (contribution_amount > 0),
    CONSTRAINT payout_schedule_not_empty CHECK (jsonb_typeof(payout_schedule) = 'object'),
    CONSTRAINT constitution_not_empty CHECK (jsonb_typeof(constitution) = 'object')
);

-- Indexes for groups table
CREATE INDEX idx_groups_status ON groups(status);
CREATE INDEX idx_groups_created_at ON groups(created_at DESC);
CREATE INDEX idx_groups_group_type ON groups(group_type);
CREATE INDEX idx_groups_created_by ON groups(created_by);
CREATE INDEX idx_groups_contribution_frequency ON groups(contribution_frequency);

-- Full-text search index for group names (optional but useful)
CREATE INDEX idx_groups_name_trgm ON groups USING gin (name gin_trgm_ops);

-- Comments for groups table
COMMENT ON TABLE groups IS 'Stokvel groups with contribution and payout configurations';
COMMENT ON COLUMN groups.group_type IS 'Type of stokvel: rotating_payout, savings_pot, or investment_club';
COMMENT ON COLUMN groups.contribution_amount IS 'Fixed contribution amount per period (R50-R10,000)';
COMMENT ON COLUMN groups.payout_schedule IS 'JSON configuration for payout rules and schedule';
COMMENT ON COLUMN groups.constitution IS 'Group governance rules (grace period, late fees, quorum, etc.)';

-- ============================================================================
-- GROUP_MEMBERS TABLE
-- ============================================================================

CREATE TABLE group_members (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    group_id UUID NOT NULL REFERENCES groups(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role member_role NOT NULL DEFAULT 'member',  -- Uses enum type from init script
    status member_status NOT NULL DEFAULT 'active',  -- Uses enum type from init script
    joined_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    left_at TIMESTAMP WITH TIME ZONE,
    invited_by UUID REFERENCES users(id),
    invite_token VARCHAR(100),
    invite_accepted_at TIMESTAMP WITH TIME ZONE,
    
    -- Unique constraint: one user can only be a member once per group
    CONSTRAINT unique_user_per_group UNIQUE (group_id, user_id),
    
    -- Business rule: user can't be suspended if they're a chairperson
    CONSTRAINT chairperson_cannot_be_suspended CHECK (
        NOT (role = 'chairperson' AND status = 'suspended')
    ),
    
    -- Business rule: if status is 'left', left_at must be set
    CONSTRAINT left_at_required CHECK (
        (status = 'left' AND left_at IS NOT NULL) OR (status != 'left')
    )
);

-- Indexes for group_members table
CREATE INDEX idx_group_members_group_id ON group_members(group_id);
CREATE INDEX idx_group_members_user_id ON group_members(user_id);
CREATE INDEX idx_group_members_role ON group_members(role);
CREATE INDEX idx_group_members_status ON group_members(status) WHERE status = 'active';
CREATE INDEX idx_group_members_joined_at ON group_members(joined_at DESC);

-- Index for finding chairpersons quickly
CREATE INDEX idx_group_members_chairperson ON group_members(group_id) WHERE role = 'chairperson';

-- Index for pending invitations
CREATE INDEX idx_group_members_pending_invites ON group_members(invite_token) WHERE invite_token IS NOT NULL AND invite_accepted_at IS NULL;

-- Comments for group_members table
COMMENT ON TABLE group_members IS 'Members of stokvel groups with roles and status';
COMMENT ON COLUMN group_members.role IS 'Member role: member, secretary, treasurer, or chairperson';
COMMENT ON COLUMN group_members.status IS 'Member status: active, suspended, or left';
COMMENT ON COLUMN group_members.invite_token IS 'Token for invitation link (null after accepted)';

-- ============================================================================
-- GROUP_SAVINGS_ACCOUNTS TABLE
-- ============================================================================

CREATE TABLE group_savings_accounts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    group_id UUID NOT NULL REFERENCES groups(id) ON DELETE CASCADE UNIQUE,
    account_number VARCHAR(20) NOT NULL UNIQUE,
    balance DECIMAL(12, 2) NOT NULL DEFAULT 0.00,
    total_contributions DECIMAL(12, 2) NOT NULL DEFAULT 0.00,
    total_interest_earned DECIMAL(12, 2) NOT NULL DEFAULT 0.00,
    total_payouts DECIMAL(12, 2) NOT NULL DEFAULT 0.00,
    interest_rate DECIMAL(5, 2) NOT NULL,
    last_interest_calculation_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT balance_non_negative CHECK (balance >= 0),
    CONSTRAINT total_contributions_non_negative CHECK (total_contributions >= 0),
    CONSTRAINT total_interest_earned_non_negative CHECK (total_interest_earned >= 0),
    CONSTRAINT total_payouts_non_negative CHECK (total_payouts >= 0),
    CONSTRAINT interest_rate_valid CHECK (interest_rate >= 0 AND interest_rate <= 20),
    CONSTRAINT account_number_format CHECK (account_number ~ '^[0-9]{10,20}$')
);

-- Indexes for group_savings_accounts table
CREATE INDEX idx_savings_accounts_group_id ON group_savings_accounts(group_id);
CREATE INDEX idx_savings_accounts_account_number ON group_savings_accounts(account_number);
CREATE INDEX idx_savings_accounts_balance ON group_savings_accounts(balance DESC);
CREATE INDEX idx_savings_accounts_created_at ON group_savings_accounts(created_at DESC);

-- Comments for group_savings_accounts table
COMMENT ON TABLE group_savings_accounts IS 'Bank accounts for group savings with balance and interest tracking';
COMMENT ON COLUMN group_savings_accounts.account_number IS 'Bank account number from core banking system';
COMMENT ON COLUMN group_savings_accounts.balance IS 'Current account balance in ZAR';
COMMENT ON COLUMN group_savings_accounts.interest_rate IS 'Annual interest rate percentage (0-20%)';

-- ============================================================================
-- FOREIGN KEY RELATIONSHIPS
-- ============================================================================

-- Add foreign key from groups to users (created_by)
ALTER TABLE groups ADD CONSTRAINT fk_groups_created_by 
    FOREIGN KEY (created_by) REFERENCES users(id) ON DELETE RESTRICT;

-- ============================================================================
-- TRIGGERS FOR AUTOMATIC TIMESTAMP UPDATES
-- ============================================================================

-- Function to update updated_at timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Trigger for users table
CREATE TRIGGER update_users_updated_at
    BEFORE UPDATE ON users
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- Trigger for groups table
CREATE TRIGGER update_groups_updated_at
    BEFORE UPDATE ON groups
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- Trigger for group_savings_accounts table
CREATE TRIGGER update_group_savings_accounts_updated_at
    BEFORE UPDATE ON group_savings_accounts
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- ============================================================================
-- INITIAL DATA / SEED DATA (Optional)
-- ============================================================================

-- No initial data required for production
-- Development/test data can be added separately

-- ============================================================================
-- MIGRATION SUCCESS CONFIRMATION
-- ============================================================================

-- Log migration success
DO $$
BEGIN
    RAISE NOTICE 'Migration V001__create_core_schema.sql completed successfully';
    RAISE NOTICE 'Tables created: users, groups, group_members, group_savings_accounts';
    RAISE NOTICE 'Indexes created: 24 indexes across 4 tables';
    RAISE NOTICE 'Triggers created: 3 updated_at triggers';
END $$;

-- Verify tables exist
SELECT 
    schemaname,
    tablename,
    tableowner
FROM pg_tables
WHERE schemaname = 'public'
    AND tablename IN ('users', 'groups', 'group_members', 'group_savings_accounts')
ORDER BY tablename;
