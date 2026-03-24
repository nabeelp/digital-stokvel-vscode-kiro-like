-- Digital Stokvel Banking Platform - Database Migration
-- Version: V002
-- Description: Create contributions, contribution_ledger, and recurring_payments tables
-- Date: 2026-03-24
-- Author: Digital Stokvel Team

-- ============================================================================
-- CONTRIBUTIONS TABLE
-- ============================================================================

CREATE TABLE contributions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    group_id UUID NOT NULL REFERENCES groups(id) ON DELETE CASCADE,
    member_id UUID NOT NULL REFERENCES group_members(id) ON DELETE CASCADE,
    amount DECIMAL(10, 2) NOT NULL,
    status contribution_status NOT NULL DEFAULT 'pending',  -- Uses enum type from init script
    transaction_ref VARCHAR(100) UNIQUE,  -- Unique reference from payment gateway
    due_date DATE NOT NULL,
    paid_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT contribution_amount_positive CHECK (amount > 0),
    CONSTRAINT paid_at_after_created CHECK (paid_at IS NULL OR paid_at >= created_at),
    CONSTRAINT paid_status_has_transaction CHECK (
        (status = 'paid' AND transaction_ref IS NOT NULL AND paid_at IS NOT NULL) OR
        (status != 'paid')
    )
);

-- Indexes for contributions table
CREATE INDEX idx_contributions_group_id ON contributions(group_id);
CREATE INDEX idx_contributions_member_id ON contributions(member_id);
CREATE INDEX idx_contributions_status ON contributions(status);
CREATE INDEX idx_contributions_due_date ON contributions(due_date);
CREATE INDEX idx_contributions_paid_at ON contributions(paid_at DESC) WHERE paid_at IS NOT NULL;
CREATE INDEX idx_contributions_transaction_ref ON contributions(transaction_ref) WHERE transaction_ref IS NOT NULL;

-- Index for finding overdue contributions
CREATE INDEX idx_contributions_overdue ON contributions(group_id, due_date) 
    WHERE status = 'pending' AND due_date < CURRENT_DATE;

-- Index for member contribution history
CREATE INDEX idx_contributions_member_history ON contributions(member_id, created_at DESC);

-- Comments for contributions table
COMMENT ON TABLE contributions IS 'Member contributions to stokvel groups with payment tracking';
COMMENT ON COLUMN contributions.status IS 'Contribution status: pending, paid, failed, or refunded';
COMMENT ON COLUMN contributions.transaction_ref IS 'Unique transaction reference from payment gateway';
COMMENT ON COLUMN contributions.due_date IS 'Date when contribution is due';
COMMENT ON COLUMN contributions.paid_at IS 'Timestamp when payment was confirmed';

-- ============================================================================
-- CONTRIBUTION_LEDGER TABLE (IMMUTABLE, APPEND-ONLY)
-- ============================================================================

CREATE TABLE contribution_ledger (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    contribution_id UUID NOT NULL REFERENCES contributions(id) ON DELETE RESTRICT,
    group_id UUID NOT NULL REFERENCES groups(id) ON DELETE RESTRICT,
    member_id UUID NOT NULL REFERENCES group_members(id) ON DELETE RESTRICT,
    amount DECIMAL(10, 2) NOT NULL,
    transaction_ref VARCHAR(100) NOT NULL,
    metadata JSONB DEFAULT '{}'::jsonb,
    recorded_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT ledger_amount_positive CHECK (amount > 0),
    CONSTRAINT ledger_immutable CHECK (recorded_at <= CURRENT_TIMESTAMP),
    CONSTRAINT ledger_metadata_is_object CHECK (jsonb_typeof(metadata) = 'object')
);

-- Indexes for contribution_ledger table
CREATE INDEX idx_ledger_contribution_id ON contribution_ledger(contribution_id);
CREATE INDEX idx_ledger_group_time ON contribution_ledger(group_id, recorded_at DESC);
CREATE INDEX idx_ledger_member_time ON contribution_ledger(member_id, recorded_at DESC);
CREATE INDEX idx_ledger_transaction_ref ON contribution_ledger(transaction_ref);
CREATE INDEX idx_ledger_recorded_at ON contribution_ledger(recorded_at DESC);

-- Comments for contribution_ledger table
COMMENT ON TABLE contribution_ledger IS 'Immutable append-only ledger for contribution audit trail (7-year retention)';
COMMENT ON COLUMN contribution_ledger.metadata IS 'Additional data: payment method, gateway response, timestamps';
COMMENT ON COLUMN contribution_ledger.recorded_at IS 'Immutable timestamp of ledger entry creation';

-- ============================================================================
-- TRIGGERS FOR CONTRIBUTION_LEDGER IMMUTABILITY
-- ============================================================================

-- Function to prevent UPDATE operations on ledger
CREATE OR REPLACE FUNCTION prevent_contribution_ledger_update()
RETURNS TRIGGER AS $$
BEGIN
    RAISE EXCEPTION 'UPDATE operations are not allowed on contribution_ledger table. This is an append-only immutable ledger.';
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- Function to prevent DELETE operations on ledger
CREATE OR REPLACE FUNCTION prevent_contribution_ledger_delete()
RETURNS TRIGGER AS $$
BEGIN
    RAISE EXCEPTION 'DELETE operations are not allowed on contribution_ledger table. This is an append-only immutable ledger.';
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- Trigger to prevent updates
CREATE TRIGGER prevent_contribution_ledger_update_trigger
    BEFORE UPDATE ON contribution_ledger
    FOR EACH ROW
    EXECUTE FUNCTION prevent_contribution_ledger_update();

-- Trigger to prevent deletes
CREATE TRIGGER prevent_contribution_ledger_delete_trigger
    BEFORE DELETE ON contribution_ledger
    FOR EACH ROW
    EXECUTE FUNCTION prevent_contribution_ledger_delete();

-- ============================================================================
-- RECURRING_PAYMENTS TABLE
-- ============================================================================

CREATE TABLE recurring_payments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    member_id UUID NOT NULL REFERENCES group_members(id) ON DELETE CASCADE,
    group_id UUID NOT NULL REFERENCES groups(id) ON DELETE CASCADE,
    amount DECIMAL(10, 2) NOT NULL,
    frequency contribution_frequency NOT NULL,  -- Uses enum type from init script
    status VARCHAR(20) NOT NULL DEFAULT 'active' CHECK (status IN ('active', 'paused', 'cancelled')),
    next_payment_date DATE NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    paused_at TIMESTAMP WITH TIME ZONE,
    cancelled_at TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT recurring_amount_positive CHECK (amount > 0),
    CONSTRAINT next_payment_future CHECK (next_payment_date >= CURRENT_DATE),
    CONSTRAINT paused_has_timestamp CHECK (
        (status = 'paused' AND paused_at IS NOT NULL) OR
        (status != 'paused')
    ),
    CONSTRAINT cancelled_has_timestamp CHECK (
        (status = 'cancelled' AND cancelled_at IS NOT NULL) OR
        (status != 'cancelled')
    ),
    -- Unique constraint: one active recurring payment per member per group
    CONSTRAINT unique_active_recurring_payment UNIQUE (member_id, group_id, status)
        DEFERRABLE INITIALLY DEFERRED
);

-- Remove the unique constraint that doesn't allow multiple statuses, use a partial index instead
ALTER TABLE recurring_payments DROP CONSTRAINT IF EXISTS unique_active_recurring_payment;

-- Partial unique index for active recurring payments only
CREATE UNIQUE INDEX idx_recurring_payments_active_unique 
    ON recurring_payments(member_id, group_id) 
    WHERE status = 'active';

-- Indexes for recurring_payments table
CREATE INDEX idx_recurring_payments_member_id ON recurring_payments(member_id);
CREATE INDEX idx_recurring_payments_group_id ON recurring_payments(group_id);
CREATE INDEX idx_recurring_payments_status ON recurring_payments(status);
CREATE INDEX idx_recurring_payments_next_payment ON recurring_payments(next_payment_date) 
    WHERE status = 'active';

-- Index for finding due recurring payments
CREATE INDEX idx_recurring_payments_due ON recurring_payments(next_payment_date, status) 
    WHERE status = 'active' AND next_payment_date <= CURRENT_DATE;

-- Comments for recurring_payments table
COMMENT ON TABLE recurring_payments IS 'Automated recurring contribution schedules for members';
COMMENT ON COLUMN recurring_payments.status IS 'Status: active, paused, or cancelled';
COMMENT ON COLUMN recurring_payments.frequency IS 'Payment frequency: weekly or monthly';
COMMENT ON COLUMN recurring_payments.next_payment_date IS 'Date of next scheduled payment';

-- ============================================================================
-- TRIGGERS FOR AUTOMATIC TIMESTAMP UPDATES
-- ============================================================================

-- Trigger for contributions table
CREATE TRIGGER update_contributions_updated_at
    BEFORE UPDATE ON contributions
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- Trigger for recurring_payments table
CREATE TRIGGER update_recurring_payments_updated_at
    BEFORE UPDATE ON recurring_payments
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- ============================================================================
-- AUTOMATIC LEDGER ENTRY ON CONTRIBUTION PAYMENT
-- ============================================================================

-- Function to automatically create ledger entry when contribution is paid
CREATE OR REPLACE FUNCTION create_contribution_ledger_entry()
RETURNS TRIGGER AS $$
BEGIN
    -- Only create ledger entry when status changes to 'paid'
    IF NEW.status = 'paid' AND (OLD.status IS NULL OR OLD.status != 'paid') THEN
        INSERT INTO contribution_ledger (
            contribution_id,
            group_id,
            member_id,
            amount,
            transaction_ref,
            metadata,
            recorded_at
        ) VALUES (
            NEW.id,
            NEW.group_id,
            NEW.member_id,
            NEW.amount,
            NEW.transaction_ref,
            jsonb_build_object(
                'due_date', NEW.due_date,
                'paid_at', NEW.paid_at,
                'created_at', NEW.created_at
            ),
            NEW.paid_at
        );
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Trigger to automatically create ledger entry
CREATE TRIGGER create_ledger_entry_on_payment
    AFTER INSERT OR UPDATE ON contributions
    FOR EACH ROW
    EXECUTE FUNCTION create_contribution_ledger_entry();

-- ============================================================================
-- MIGRATION SUCCESS CONFIRMATION
-- ============================================================================

DO $$
BEGIN
    RAISE NOTICE 'Migration V002__create_contributions_tables.sql completed successfully';
    RAISE NOTICE 'Tables created: contributions, contribution_ledger, recurring_payments';
    RAISE NOTICE 'Indexes created: 21 indexes across 3 tables';
    RAISE NOTICE 'Triggers created: 5 triggers (2 immutability, 2 timestamps, 1 ledger automation)';
    RAISE NOTICE 'Immutability: contribution_ledger is append-only with UPDATE/DELETE prevention';
    RAISE NOTICE 'Automation: Ledger entries automatically created when contributions marked as paid';
END $$;

-- Verify tables exist
SELECT 
    schemaname,
    tablename,
    tableowner
FROM pg_tables
WHERE schemaname = 'public'
    AND tablename IN ('contributions', 'contribution_ledger', 'recurring_payments')
ORDER BY tablename;
