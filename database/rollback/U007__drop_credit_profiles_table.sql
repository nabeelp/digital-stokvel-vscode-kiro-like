-- =====================================================================================
-- Rollback Migration: U007 - Drop Credit Profiles Table
-- Description: Safely removes credit profile functionality
-- Author: Digital Stokvel Team
-- Date: 2026-03-24
-- Rollback For: V007__create_credit_profiles_table.sql
-- =====================================================================================

-- Drop views
DROP VIEW IF EXISTS stokvel_score_leaderboard CASCADE;
DROP VIEW IF EXISTS credit_profile_statistics CASCADE;

-- Drop helper functions
DROP FUNCTION IF EXISTS get_members_due_for_bureau_report() CASCADE;
DROP FUNCTION IF EXISTS get_pre_qualified_members() CASCADE;
DROP FUNCTION IF EXISTS update_credit_profile_metrics(UUID) CASCADE;
DROP FUNCTION IF EXISTS calculate_stokvel_score(UUID) CASCADE;

-- Drop trigger (automatically dropped with table, but explicit for clarity)
DROP TRIGGER IF EXISTS update_credit_profiles_updated_at ON credit_profiles;

-- Drop table
DROP TABLE IF EXISTS credit_profiles CASCADE;

-- =====================================================================================
-- Confirmation Message
-- =====================================================================================
DO $$
BEGIN
    RAISE NOTICE 'Successfully rolled back V007: credit_profiles table and related objects dropped';
END $$;
