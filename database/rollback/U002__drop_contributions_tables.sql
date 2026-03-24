-- Digital Stokvel Banking Platform - Database Rollback
-- Version: U002
-- Description: Rollback V002 - Drop contributions, contribution_ledger, and recurring_payments tables
-- Date: 2026-03-24
-- Author: Digital Stokvel Team
-- WARNING: This will delete all data in contributions, contribution_ledger, and recurring_payments tables

-- ============================================================================
-- ROLLBACK CONFIRMATION
-- ============================================================================

DO $$
BEGIN
    RAISE WARNING 'Rolling back V002__create_contributions_tables.sql';
    RAISE WARNING 'This will drop: contributions, contribution_ledger, recurring_payments tables';
    RAISE WARNING 'All contribution data and immutable ledger entries will be permanently deleted';
END $$;

-- ============================================================================
-- DROP TRIGGERS
-- ============================================================================

DROP TRIGGER IF EXISTS create_ledger_entry_on_payment ON contributions;
DROP TRIGGER IF EXISTS update_recurring_payments_updated_at ON recurring_payments;
DROP TRIGGER IF EXISTS update_contributions_updated_at ON contributions;
DROP TRIGGER IF EXISTS prevent_contribution_ledger_delete_trigger ON contribution_ledger;
DROP TRIGGER IF EXISTS prevent_contribution_ledger_update_trigger ON contribution_ledger;

-- Drop trigger functions
DROP FUNCTION IF EXISTS create_contribution_ledger_entry();
DROP FUNCTION IF EXISTS prevent_contribution_ledger_delete();
DROP FUNCTION IF EXISTS prevent_contribution_ledger_update();

-- ============================================================================
-- DROP TABLES (in reverse dependency order)
-- ============================================================================

-- Drop tables with foreign key dependencies first
DROP TABLE IF EXISTS recurring_payments CASCADE;
DROP TABLE IF EXISTS contribution_ledger CASCADE;
DROP TABLE IF EXISTS contributions CASCADE;

-- ============================================================================
-- ROLLBACK SUCCESS CONFIRMATION
-- ============================================================================

DO $$
BEGIN
    RAISE NOTICE 'Rollback U002__drop_contributions_tables.sql completed successfully';
    RAISE NOTICE 'Tables dropped: contributions, contribution_ledger, recurring_payments';
    RAISE NOTICE 'Triggers dropped: 5 triggers';
    RAISE NOTICE 'Functions dropped: 3 functions';
END $$;

-- Verify tables are dropped
SELECT 
    COUNT(*) as remaining_tables
FROM pg_tables
WHERE schemaname = 'public'
    AND tablename IN ('contributions', 'contribution_ledger', 'recurring_payments');
