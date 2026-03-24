-- Digital Stokvel Banking - Ledger Database Initialization
-- Version: 1.0
-- Date: 2026-03-24

-- Enable required PostgreSQL extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Create immutable ledger table for contributions
CREATE TABLE IF NOT EXISTS contribution_ledger (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    contribution_id UUID NOT NULL,
    group_id UUID NOT NULL,
    member_id UUID NOT NULL,
    amount DECIMAL(10, 2) NOT NULL CHECK (amount > 0),
    transaction_ref VARCHAR(100) NOT NULL,
    metadata JSONB NOT NULL DEFAULT '{}',
    recorded_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- Create indexes for query performance
CREATE INDEX idx_contribution_ledger_contribution_id ON contribution_ledger(contribution_id);
CREATE INDEX idx_contribution_ledger_group_id ON contribution_ledger(group_id);
CREATE INDEX idx_contribution_ledger_member_id ON contribution_ledger(member_id);
CREATE INDEX idx_contribution_ledger_recorded_at ON contribution_ledger(recorded_at DESC);
CREATE INDEX idx_contribution_ledger_transaction_ref ON contribution_ledger(transaction_ref);

-- Create immutable audit log table
CREATE TABLE IF NOT EXISTS audit_log (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    entity_type VARCHAR(50) NOT NULL,
    entity_id UUID NOT NULL,
    action VARCHAR(20) NOT NULL,
    actor_id UUID,
    changes JSONB NOT NULL DEFAULT '{}',
    ip_address INET,
    user_agent TEXT,
    recorded_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- Create indexes for audit log
CREATE INDEX idx_audit_log_entity_type_id ON audit_log(entity_type, entity_id);
CREATE INDEX idx_audit_log_actor_id ON audit_log(actor_id);
CREATE INDEX idx_audit_log_recorded_at ON audit_log(recorded_at DESC);
CREATE INDEX idx_audit_log_action ON audit_log(action);

-- Prevent updates and deletes on contribution_ledger (enforce append-only)
CREATE OR REPLACE FUNCTION prevent_contribution_ledger_modifications()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'UPDATE' OR TG_OP = 'DELETE' THEN
        RAISE EXCEPTION 'Cannot modify immutable contribution_ledger records';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER prevent_contribution_ledger_update_delete
    BEFORE UPDATE OR DELETE ON contribution_ledger
    FOR EACH ROW
    EXECUTE FUNCTION prevent_contribution_ledger_modifications();

-- Prevent updates and deletes on audit_log (enforce append-only)
CREATE OR REPLACE FUNCTION prevent_audit_log_modifications()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'UPDATE' OR TG_OP = 'DELETE' THEN
        RAISE EXCEPTION 'Cannot modify immutable audit_log records';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER prevent_audit_log_update_delete
    BEFORE UPDATE OR DELETE ON audit_log
    FOR EACH ROW
    EXECUTE FUNCTION prevent_audit_log_modifications();

-- Success message
SELECT 'Ledger database initialized successfully' AS status;
