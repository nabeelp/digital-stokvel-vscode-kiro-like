-- Digital Stokvel Banking Platform - Database Rollback
-- Version: U003
-- Description: Rollback V003 - Drop payouts and payout_disbursements tables
-- Date: 2026-03-24
-- Author: Digital Stokvel Team
-- WARNING: This will delete all data in payouts and payout_disbursements tables

-- ============================================================================
-- ROLLBACK CONFIRMATION
-- ============================================================================

DO $$
BEGIN
    RAISE WARNING 'Rolling back V003__create_payouts_tables.sql';
    RAISE WARNING 'This will drop: payouts, payout_disbursements tables';
    RAISE WARNING 'All payout data and disbursement records will be permanently deleted';
END $$;

-- ============================================================================
-- DROP VIEWS
-- ============================================================================

DROP VIEW IF EXISTS payout_summary CASCADE;

-- ============================================================================
-- DROP TRIGGERS
-- ============================================================================

DROP TRIGGER IF EXISTS validate_disbursement_total ON payout_disbursements;
DROP TRIGGER IF EXISTS update_payout_disbursements_updated_at ON payout_disbursements;
DROP TRIGGER IF EXISTS update_payouts_updated_at ON payouts;

-- Drop trigger functions
DROP FUNCTION IF EXISTS validate_payout_disbursement_total();
DROP FUNCTION IF EXISTS get_payout_disbursement_total(UUID);

-- ============================================================================
-- DROP TABLES (in reverse dependency order)
-- ============================================================================

-- Drop tables with foreign key dependencies first
DROP TABLE IF EXISTS payout_disbursements CASCADE;
DROP TABLE IF EXISTS payouts CASCADE;

-- ============================================================================
-- ROLLBACK SUCCESS CONFIRMATION
-- ============================================================================

DO $$
BEGIN
    RAISE NOTICE 'Rollback U003__drop_payouts_tables.sql completed successfully';
    RAISE NOTICE 'Tables dropped: payouts, payout_disbursements';
    RAISE NOTICE 'Views dropped: payout_summary';
    RAISE NOTICE 'Triggers dropped: 3 triggers';
    RAISE NOTICE 'Functions dropped: 2 functions';
END $$;

-- Verify tables are dropped
SELECT 
    COUNT(*) as remaining_tables
FROM pg_tables
WHERE schemaname = 'public'
    AND tablename IN ('payouts', 'payout_disbursements');
