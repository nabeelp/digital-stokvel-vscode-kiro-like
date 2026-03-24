-- =====================================================================================
-- Rollback Migration: U005 - Drop Disputes Table
-- Description: Safely removes dispute resolution functionality
-- Author: Digital Stokvel Team
-- Date: 2026-03-24
-- Rollback For: V005__create_disputes_table.sql
-- =====================================================================================

-- Drop view
DROP VIEW IF EXISTS dispute_statistics CASCADE;

-- Drop helper functions
DROP FUNCTION IF EXISTS auto_escalate_overdue_disputes() CASCADE;
DROP FUNCTION IF EXISTS get_overdue_disputes(UUID) CASCADE;
DROP FUNCTION IF EXISTS get_dispute_resolution_deadline(UUID) CASCADE;

-- Drop trigger (automatically dropped with table, but explicit for clarity)
DROP TRIGGER IF EXISTS update_disputes_updated_at ON disputes;

-- Drop table
DROP TABLE IF EXISTS disputes CASCADE;

-- =====================================================================================
-- Confirmation Message
-- =====================================================================================
DO $$
BEGIN
    RAISE NOTICE 'Successfully rolled back V005: disputes table and related objects dropped';
END $$;
