-- =====================================================================================
-- Migration: V004 - Create Votes and Vote Responses Tables
-- Description: Implements democratic decision-making within groups through voting
--              mechanisms with support for various vote types and quorum requirements
-- Author: Digital Stokvel Team
-- Date: 2026-03-24
-- Dependencies: V001 (users, groups, group_members tables)
-- =====================================================================================

-- =====================================================================================
-- VOTES TABLE
-- Purpose: Stores group votes for democratic decision-making
-- Features:
--   - Subject-based voting with configurable options
--   - Start and end timestamps for voting period
--   - Status lifecycle: draft -> active -> closed
--   - Results stored as JSONB for flexibility
--   - Audit trail with initiator tracking
-- =====================================================================================

CREATE TABLE IF NOT EXISTS votes (
    -- Primary key
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    -- Foreign keys
    group_id UUID NOT NULL REFERENCES groups(id) ON DELETE CASCADE,
    initiated_by UUID NOT NULL REFERENCES users(id) ON DELETE SET NULL,
    
    -- Vote details
    subject VARCHAR(200) NOT NULL,
    options JSONB NOT NULL DEFAULT '[]',
    
    -- Voting timeline
    voting_starts_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    voting_ends_at TIMESTAMP NOT NULL,
    
    -- Status and results
    status VARCHAR(20) NOT NULL DEFAULT 'draft' 
        CHECK (status IN ('draft', 'active', 'closed')),
    results JSONB DEFAULT '{}',
    
    -- Audit timestamps
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    -- Business rule constraints
    CONSTRAINT voting_period_valid CHECK (voting_ends_at > voting_starts_at),
    CONSTRAINT options_not_empty CHECK (jsonb_array_length(options) >= 2)
);

-- =====================================================================================
-- VOTES INDEXES
-- Purpose: Optimize queries for vote management and reporting
-- =====================================================================================

-- Find active/draft votes for a group
CREATE INDEX idx_votes_group_status 
    ON votes(group_id, status);

-- Group vote history (most recent first)
CREATE INDEX idx_votes_group_created 
    ON votes(group_id, created_at DESC);

-- Votes initiated by a specific user
CREATE INDEX idx_votes_initiated_by 
    ON votes(initiated_by);

-- Find votes ending soon (for notifications)
CREATE INDEX idx_votes_ending_soon 
    ON votes(voting_ends_at) 
    WHERE status = 'active';

-- Vote status for filtering
CREATE INDEX idx_votes_status 
    ON votes(status);

-- =====================================================================================
-- VOTE_RESPONSES TABLE
-- Purpose: Stores individual member vote responses
-- Features:
--   - One vote per member per vote (enforced by unique constraint)
--   - Selected option must match vote options (validated at application level)
--   - Timestamp tracking for when vote was cast
--   - Immutable once cast (updated_at for audit only)
-- =====================================================================================

CREATE TABLE IF NOT EXISTS vote_responses (
    -- Primary key
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    -- Foreign keys
    vote_id UUID NOT NULL REFERENCES votes(id) ON DELETE CASCADE,
    member_id UUID NOT NULL REFERENCES group_members(id) ON DELETE CASCADE,
    
    -- Response details
    selected_option VARCHAR(100) NOT NULL,
    voted_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    -- Audit timestamps
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    -- Business rule: one vote per member per vote
    CONSTRAINT unique_member_vote UNIQUE (vote_id, member_id)
);

-- =====================================================================================
-- VOTE_RESPONSES INDEXES
-- Purpose: Optimize queries for vote counting and member voting history
-- =====================================================================================

-- Find all responses for a vote (for counting)
CREATE INDEX idx_vote_responses_vote_id 
    ON vote_responses(vote_id);

-- Find member's voting history
CREATE INDEX idx_vote_responses_member_id 
    ON vote_responses(member_id);

-- Recent voting activity
CREATE INDEX idx_vote_responses_voted_at 
    ON vote_responses(voted_at DESC);

-- =====================================================================================
-- AUTOMATIC TIMESTAMP TRIGGERS
-- Purpose: Automatically update updated_at timestamp on record modification
-- =====================================================================================

-- Trigger for votes table
CREATE TRIGGER update_votes_updated_at
    BEFORE UPDATE ON votes
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- Trigger for vote_responses table
CREATE TRIGGER update_vote_responses_updated_at
    BEFORE UPDATE ON vote_responses
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- =====================================================================================
-- VOTE COUNTING VIEW
-- Purpose: Simplified view for counting votes per option
-- =====================================================================================

CREATE OR REPLACE VIEW vote_summary AS
SELECT 
    v.id AS vote_id,
    v.group_id,
    v.subject,
    v.status,
    v.voting_starts_at,
    v.voting_ends_at,
    COUNT(DISTINCT vr.id) AS total_responses,
    v.options,
    JSONB_AGG(
        JSONB_BUILD_OBJECT(
            'option', vr.selected_option, 
            'count', COUNT(vr.id)
        )
    ) FILTER (WHERE vr.selected_option IS NOT NULL) AS response_counts
FROM votes v
LEFT JOIN vote_responses vr ON vr.vote_id = v.id
GROUP BY v.id, v.group_id, v.subject, v.status, v.voting_starts_at, v.voting_ends_at, v.options;

-- =====================================================================================
-- HELPER FUNCTION: Get Vote Participation Rate
-- Purpose: Calculate percentage of members who have voted
-- =====================================================================================

CREATE OR REPLACE FUNCTION get_vote_participation_rate(p_vote_id UUID)
RETURNS DECIMAL(5,2) AS $$
DECLARE
    v_group_id UUID;
    v_total_members INTEGER;
    v_total_responses INTEGER;
    v_participation_rate DECIMAL(5,2);
BEGIN
    -- Get the group_id for the vote
    SELECT group_id INTO v_group_id
    FROM votes
    WHERE id = p_vote_id;
    
    IF v_group_id IS NULL THEN
        RETURN 0;
    END IF;
    
    -- Count active members in the group
    SELECT COUNT(*) INTO v_total_members
    FROM group_members
    WHERE group_id = v_group_id 
      AND status = 'active';
    
    -- Count responses for this vote
    SELECT COUNT(*) INTO v_total_responses
    FROM vote_responses
    WHERE vote_id = p_vote_id;
    
    -- Calculate participation rate
    IF v_total_members > 0 THEN
        v_participation_rate := (v_total_responses::DECIMAL / v_total_members::DECIMAL) * 100;
    ELSE
        v_participation_rate := 0;
    END IF;
    
    RETURN ROUND(v_participation_rate, 2);
END;
$$ LANGUAGE plpgsql;

-- =====================================================================================
-- HELPER FUNCTION: Check if Vote Reached Quorum
-- Purpose: Verify if a vote has reached the required quorum based on constitution
-- =====================================================================================

CREATE OR REPLACE FUNCTION check_vote_quorum(p_vote_id UUID)
RETURNS BOOLEAN AS $$
DECLARE
    v_group_id UUID;
    v_quorum_percentage INTEGER;
    v_participation_rate DECIMAL(5,2);
BEGIN
    -- Get the group_id for the vote
    SELECT group_id INTO v_group_id
    FROM votes
    WHERE id = p_vote_id;
    
    IF v_group_id IS NULL THEN
        RETURN FALSE;
    END IF;
    
    -- Get quorum percentage from group constitution
    SELECT COALESCE((constitution->>'quorum_percentage')::INTEGER, 51) 
    INTO v_quorum_percentage
    FROM groups
    WHERE id = v_group_id;
    
    -- Get participation rate
    v_participation_rate := get_vote_participation_rate(p_vote_id);
    
    -- Check if quorum is reached
    RETURN v_participation_rate >= v_quorum_percentage;
END;
$$ LANGUAGE plpgsql;

-- =====================================================================================
-- END OF MIGRATION V004
-- =====================================================================================
