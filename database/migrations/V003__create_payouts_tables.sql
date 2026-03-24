-- Digital Stokvel Banking Platform - Database Migration
-- Version: V003
-- Description: Create payouts and payout_disbursements tables
-- Date: 2026-03-24
-- Author: Digital Stokvel Team

-- ============================================================================
-- PAYOUTS TABLE
-- ============================================================================

CREATE TABLE payouts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    group_id UUID NOT NULL REFERENCES groups(id) ON DELETE CASCADE,
    payout_type payout_type NOT NULL,  -- Uses enum type from init script (rotating, year_end_pot, emergency)
    total_amount DECIMAL(12, 2) NOT NULL,
    status payout_status NOT NULL DEFAULT 'pending_approval',  -- Uses enum type from init script
    initiated_by UUID NOT NULL REFERENCES group_members(id) ON DELETE RESTRICT,
    approved_by UUID REFERENCES group_members(id) ON DELETE RESTRICT,
    initiated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    approved_at TIMESTAMP WITH TIME ZONE,
    executed_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT payout_total_amount_positive CHECK (total_amount > 0),
    CONSTRAINT payout_approved_has_approver CHECK (
        (status IN ('approved', 'executed') AND approved_by IS NOT NULL AND approved_at IS NOT NULL) OR
        (status NOT IN ('approved', 'executed'))
    ),
    CONSTRAINT payout_executed_has_timestamp CHECK (
        (status = 'executed' AND executed_at IS NOT NULL) OR
        (status != 'executed')
    ),
    CONSTRAINT payout_timeline_logical CHECK (
        approved_at IS NULL OR approved_at >= initiated_at
    ),
    CONSTRAINT payout_execution_after_approval CHECK (
        executed_at IS NULL OR (approved_at IS NOT NULL AND executed_at >= approved_at)
    )
);

-- Indexes for payouts table
CREATE INDEX idx_payouts_group_id ON payouts(group_id);
CREATE INDEX idx_payouts_status ON payouts(status);
CREATE INDEX idx_payouts_payout_type ON payouts(payout_type);
CREATE INDEX idx_payouts_initiated_by ON payouts(initiated_by);
CREATE INDEX idx_payouts_approved_by ON payouts(approved_by) WHERE approved_by IS NOT NULL;
CREATE INDEX idx_payouts_initiated_at ON payouts(initiated_at DESC);
CREATE INDEX idx_payouts_executed_at ON payouts(executed_at DESC) WHERE executed_at IS NOT NULL;

-- Index for pending approvals
CREATE INDEX idx_payouts_pending_approval ON payouts(group_id, initiated_at DESC) 
    WHERE status = 'pending_approval';

-- Comments for payouts table
COMMENT ON TABLE payouts IS 'Group payout requests with dual approval workflow';
COMMENT ON COLUMN payouts.payout_type IS 'Type of payout: rotating, year_end_pot, or emergency';
COMMENT ON COLUMN payouts.status IS 'Payout status: pending_approval, approved, executed, or failed';
COMMENT ON COLUMN payouts.initiated_by IS 'Group member who initiated the payout (typically Chairperson)';
COMMENT ON COLUMN payouts.approved_by IS 'Group member who approved the payout (typically Treasurer)';

-- ============================================================================
-- PAYOUT_DISBURSEMENTS TABLE
-- ============================================================================

CREATE TABLE payout_disbursements (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    payout_id UUID NOT NULL REFERENCES payouts(id) ON DELETE CASCADE,
    member_id UUID NOT NULL REFERENCES group_members(id) ON DELETE RESTRICT,
    amount DECIMAL(10, 2) NOT NULL,
    transaction_ref VARCHAR(100) UNIQUE,  -- Unique reference from payment gateway/bank
    status payout_status NOT NULL DEFAULT 'pending_approval',  -- Uses enum type from init script
    executed_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT disbursement_amount_positive CHECK (amount > 0),
    CONSTRAINT disbursement_executed_has_transaction CHECK (
        (status = 'executed' AND transaction_ref IS NOT NULL AND executed_at IS NOT NULL) OR
        (status != 'executed')
    ),
    -- Unique constraint: one disbursement per member per payout
    CONSTRAINT unique_member_per_payout UNIQUE (payout_id, member_id)
);

-- Indexes for payout_disbursements table
CREATE INDEX idx_disbursements_payout_id ON payout_disbursements(payout_id);
CREATE INDEX idx_disbursements_member_id ON payout_disbursements(member_id);
CREATE INDEX idx_disbursements_status ON payout_disbursements(status);
CREATE INDEX idx_disbursements_transaction_ref ON payout_disbursements(transaction_ref) 
    WHERE transaction_ref IS NOT NULL;
CREATE INDEX idx_disbursements_executed_at ON payout_disbursements(executed_at DESC) 
    WHERE executed_at IS NOT NULL;

-- Index for pending disbursements
CREATE INDEX idx_disbursements_pending ON payout_disbursements(payout_id, status) 
    WHERE status IN ('pending_approval', 'approved');

-- Comments for payout_disbursements table
COMMENT ON TABLE payout_disbursements IS 'Individual member disbursements for approved payouts';
COMMENT ON COLUMN payout_disbursements.transaction_ref IS 'Unique transaction reference from payment gateway or bank';
COMMENT ON COLUMN payout_disbursements.status IS 'Disbursement status: pending_approval, approved, executed, or failed';

-- ============================================================================
-- TRIGGERS FOR AUTOMATIC TIMESTAMP UPDATES
-- ============================================================================

-- Trigger for payouts table
CREATE TRIGGER update_payouts_updated_at
    BEFORE UPDATE ON payouts
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- Trigger for payout_disbursements table
CREATE TRIGGER update_payout_disbursements_updated_at
    BEFORE UPDATE ON payout_disbursements
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- ============================================================================
-- VALIDATION TRIGGERS
-- ============================================================================

-- Function to validate total payout amount matches sum of disbursements
CREATE OR REPLACE FUNCTION validate_payout_disbursement_total()
RETURNS TRIGGER AS $$
DECLARE
    calculated_total DECIMAL(12, 2);
    payout_total DECIMAL(12, 2);
BEGIN
    -- Get the sum of all disbursement amounts for this payout
    SELECT COALESCE(SUM(amount), 0) INTO calculated_total
    FROM payout_disbursements
    WHERE payout_id = NEW.payout_id;
    
    -- Get the payout total amount
    SELECT total_amount INTO payout_total
    FROM payouts
    WHERE id = NEW.payout_id;
    
    -- Allow if total matches or if still adding disbursements
    -- This is a warning check, not a hard constraint (allows for gradual building)
    IF calculated_total > payout_total THEN
        RAISE EXCEPTION 'Total disbursement amount (%) exceeds payout total amount (%)', 
            calculated_total, payout_total;
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Trigger to validate disbursement totals
CREATE TRIGGER validate_disbursement_total
    AFTER INSERT OR UPDATE ON payout_disbursements
    FOR EACH ROW
    EXECUTE FUNCTION validate_payout_disbursement_total();

-- ============================================================================
-- HELPER FUNCTION: Calculate Payout Totals
-- ============================================================================

-- Function to get sum of disbursements for a payout
CREATE OR REPLACE FUNCTION get_payout_disbursement_total(payout_uuid UUID)
RETURNS DECIMAL(12, 2) AS $$
DECLARE
    disbursement_total DECIMAL(12, 2);
BEGIN
    SELECT COALESCE(SUM(amount), 0) INTO disbursement_total
    FROM payout_disbursements
    WHERE payout_id = payout_uuid;
    
    RETURN disbursement_total;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION get_payout_disbursement_total IS 'Calculate total of all disbursements for a given payout';

-- ============================================================================
-- VIEW: Payout Summary
-- ============================================================================

CREATE OR REPLACE VIEW payout_summary AS
SELECT 
    p.id,
    p.group_id,
    p.payout_type,
    p.total_amount,
    p.status,
    p.initiated_by,
    p.approved_by,
    p.initiated_at,
    p.approved_at,
    p.executed_at,
    COUNT(pd.id) as disbursement_count,
    COALESCE(SUM(pd.amount), 0) as disbursed_amount,
    COALESCE(SUM(pd.amount), 0) as disbursement_total,
    p.total_amount - COALESCE(SUM(pd.amount), 0) as remaining_amount,
    COUNT(CASE WHEN pd.status = 'executed' THEN 1 END) as completed_disbursements,
    COUNT(CASE WHEN pd.status = 'failed' THEN 1 END) as failed_disbursements
FROM payouts p
LEFT JOIN payout_disbursements pd ON p.id = pd.payout_id
GROUP BY p.id, p.group_id, p.payout_type, p.total_amount, p.status, p.initiated_by, p.approved_by, 
    p.initiated_at, p.approved_at, p.executed_at;

COMMENT ON VIEW payout_summary IS 'Aggregated view of payouts with disbursement statistics';

-- ============================================================================
-- MIGRATION SUCCESS CONFIRMATION
-- ============================================================================

DO $$
BEGIN
    RAISE NOTICE 'Migration V003__create_payouts_tables.sql completed successfully';
    RAISE NOTICE 'Tables created: payouts, payout_disbursements';
    RAISE NOTICE 'Indexes created: 15 indexes across 2 tables';
    RAISE NOTICE 'Triggers created: 3 triggers (2 timestamps, 1 validation)';
    RAISE NOTICE 'Views created: 1 view (payout_summary)';
    RAISE NOTICE 'Functions created: 2 functions (validation and helper)';
    RAISE NOTICE 'Dual approval workflow: Chairperson initiates, Treasurer approves';
END $$;

-- Verify tables exist
SELECT 
    schemaname,
    tablename,
    tableowner
FROM pg_tables
WHERE schemaname = 'public'
    AND tablename IN ('payouts', 'payout_disbursements')
ORDER BY tablename;
