-- =====================================================================================
-- Migration: V005 - Create Disputes Table
-- Description: Implements dispute resolution workflow for conflict management
--              within groups, supporting evidence-based resolution tracking
-- Author: Digital Stokvel Team
-- Date: 2026-03-24
-- Dependencies: V001 (groups, group_members tables)
-- =====================================================================================

-- =====================================================================================
-- DISPUTES TABLE
-- Purpose: Stores disputes raised by members for resolution by group governance
-- Features:
--   - Multiple dispute types: missed_payment, fraud, constitution_violation, other
--   - Evidence storage as JSONB array for multi-file support
--   - Status lifecycle: open -> investigating -> resolved/escalated
--   - Resolution tracking with notes and timestamps
--   - Automatic deadline calculation (14 days from raised_at)
-- =====================================================================================

CREATE TABLE IF NOT EXISTS disputes (
    -- Primary key
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    -- Foreign keys
    group_id UUID NOT NULL REFERENCES groups(id) ON DELETE CASCADE,
    raised_by UUID NOT NULL REFERENCES group_members(id) ON DELETE CASCADE,
    
    -- Dispute details
    dispute_type VARCHAR(30) NOT NULL 
        CHECK (dispute_type IN ('missed_payment', 'fraud', 'constitution_violation', 'other')),
    description TEXT NOT NULL,
    evidence JSONB DEFAULT '[]',
    
    -- Status and resolution
    status VARCHAR(20) NOT NULL DEFAULT 'open' 
        CHECK (status IN ('open', 'investigating', 'resolved', 'escalated')),
    resolution_notes TEXT,
    
    -- Timestamps
    raised_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    resolved_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    -- Business rule constraints
    CONSTRAINT resolved_disputes_have_notes 
        CHECK (status != 'resolved' OR resolution_notes IS NOT NULL),
    CONSTRAINT resolved_disputes_have_timestamp 
        CHECK (status != 'resolved' OR resolved_at IS NOT NULL),
    CONSTRAINT resolved_at_after_raised_at 
        CHECK (resolved_at IS NULL OR resolved_at >= raised_at),
    CONSTRAINT evidence_is_array 
        CHECK (jsonb_typeof(evidence) = 'array')
);

-- =====================================================================================
-- DISPUTES INDEXES
-- Purpose: Optimize queries for dispute management and reporting
-- =====================================================================================

-- Find open/investigating disputes for a group
CREATE INDEX idx_disputes_group_status 
    ON disputes(group_id, status);

-- Group dispute history (most recent first)
CREATE INDEX idx_disputes_group_raised_at 
    ON disputes(group_id, raised_at DESC);

-- Disputes raised by a specific member
CREATE INDEX idx_disputes_raised_by 
    ON disputes(raised_by);

-- Filter disputes by status
CREATE INDEX idx_disputes_status 
    ON disputes(status);

-- Resolved disputes history
CREATE INDEX idx_disputes_resolved_at 
    ON disputes(resolved_at DESC) 
    WHERE resolved_at IS NOT NULL;

-- Recently raised disputes
CREATE INDEX idx_disputes_raised_at 
    ON disputes(raised_at DESC);

-- =====================================================================================
-- AUTOMATIC TIMESTAMP TRIGGER
-- Purpose: Automatically update updated_at timestamp on record modification
-- =====================================================================================

CREATE TRIGGER update_disputes_updated_at
    BEFORE UPDATE ON disputes
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- =====================================================================================
-- DISPUTE STATISTICS VIEW
-- Purpose: Provide aggregated dispute statistics per group
-- =====================================================================================

CREATE OR REPLACE VIEW dispute_statistics AS
SELECT 
    group_id,
    COUNT(*) AS total_disputes,
    COUNT(*) FILTER (WHERE status = 'open') AS open_disputes,
    COUNT(*) FILTER (WHERE status = 'investigating') AS investigating_disputes,
    COUNT(*) FILTER (WHERE status = 'resolved') AS resolved_disputes,
    COUNT(*) FILTER (WHERE status = 'escalated') AS escalated_disputes,
    COUNT(*) FILTER (WHERE dispute_type = 'missed_payment') AS missed_payment_disputes,
    COUNT(*) FILTER (WHERE dispute_type = 'fraud') AS fraud_disputes,
    COUNT(*) FILTER (WHERE dispute_type = 'constitution_violation') AS constitution_violation_disputes,
    COUNT(*) FILTER (WHERE dispute_type = 'other') AS other_disputes,
    AVG(
        EXTRACT(EPOCH FROM (resolved_at - raised_at)) / 86400
    ) FILTER (WHERE resolved_at IS NOT NULL) AS avg_resolution_days
FROM disputes
GROUP BY group_id;

-- =====================================================================================
-- HELPER FUNCTION: Get Dispute Resolution Deadline
-- Purpose: Calculate resolution deadline (14 days from raised_at)
-- =====================================================================================

CREATE OR REPLACE FUNCTION get_dispute_resolution_deadline(p_dispute_id UUID)
RETURNS TIMESTAMP AS $$
DECLARE
    v_raised_at TIMESTAMP;
    v_deadline TIMESTAMP;
BEGIN
    -- Get the raised_at timestamp
    SELECT raised_at INTO v_raised_at
    FROM disputes
    WHERE id = p_dispute_id;
    
    IF v_raised_at IS NULL THEN
        RETURN NULL;
    END IF;
    
    -- Calculate deadline (14 days from raised_at)
    v_deadline := v_raised_at + INTERVAL '14 days';
    
    RETURN v_deadline;
END;
$$ LANGUAGE plpgsql;

-- =====================================================================================
-- HELPER FUNCTION: Get Overdue Disputes
-- Purpose: Find disputes that have exceeded resolution deadline (14 days)
-- =====================================================================================

CREATE OR REPLACE FUNCTION get_overdue_disputes(p_group_id UUID DEFAULT NULL)
RETURNS TABLE (
    dispute_id UUID,
    group_id UUID,
    raised_by UUID,
    dispute_type VARCHAR,
    description TEXT,
    status VARCHAR,
    raised_at TIMESTAMP,
    days_overdue INTEGER
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        d.id AS dispute_id,
        d.group_id,
        d.raised_by,
        d.dispute_type,
        d.description,
        d.status,
        d.raised_at,
        EXTRACT(DAY FROM (CURRENT_TIMESTAMP - (d.raised_at + INTERVAL '14 days')))::INTEGER AS days_overdue
    FROM disputes d
    WHERE d.status IN ('open', 'investigating')
      AND CURRENT_TIMESTAMP > (d.raised_at + INTERVAL '14 days')
      AND (p_group_id IS NULL OR d.group_id = p_group_id)
    ORDER BY d.raised_at ASC;
END;
$$ LANGUAGE plpgsql;

-- =====================================================================================
-- HELPER FUNCTION: Auto-Escalate Overdue Disputes
-- Purpose: Automatically escalate disputes that are overdue by more than 7 days
-- =====================================================================================

CREATE OR REPLACE FUNCTION auto_escalate_overdue_disputes()
RETURNS TABLE (
    dispute_id UUID,
    group_id UUID,
    days_overdue INTEGER
) AS $$
BEGIN
    -- Update overdue disputes that haven't been escalated
    UPDATE disputes d
    SET 
        status = 'escalated',
        updated_at = CURRENT_TIMESTAMP
    WHERE d.id IN (
        SELECT id
        FROM disputes
        WHERE status IN ('open', 'investigating')
          AND CURRENT_TIMESTAMP > (raised_at + INTERVAL '21 days')
    )
    RETURNING d.id, d.group_id, EXTRACT(DAY FROM (CURRENT_TIMESTAMP - (d.raised_at + INTERVAL '14 days')))::INTEGER
    INTO dispute_id, group_id, days_overdue;
    
    RETURN QUERY
    SELECT dispute_id, group_id, days_overdue;
END;
$$ LANGUAGE plpgsql;

-- =====================================================================================
-- END OF MIGRATION V005
-- =====================================================================================
