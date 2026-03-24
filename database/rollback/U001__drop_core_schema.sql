-- Digital Stokvel Banking Platform - Database Rollback
-- Version: U001
-- Description: Rollback V001 - Drop core schema tables
-- Date: 2026-03-24
-- Author: Digital Stokvel Team
-- WARNING: This will delete all data in users, groups, group_members, and group_savings_accounts tables

-- ============================================================================
-- ROLLBACK CONFIRMATION
-- ============================================================================

DO $$
BEGIN
    RAISE WARNING 'Rolling back V001__create_core_schema.sql';
    RAISE WARNING 'This will drop: users, groups, group_members, group_savings_accounts tables';
    RAISE WARNING 'All data in these tables will be permanently deleted';
END $$;

-- ============================================================================
-- DROP TRIGGERS
-- ============================================================================

DROP TRIGGER IF EXISTS update_users_updated_at ON users;
DROP TRIGGER IF EXISTS update_groups_updated_at ON groups;
DROP TRIGGER IF EXISTS update_group_savings_accounts_updated_at ON group_savings_accounts;

-- Drop the trigger function
DROP FUNCTION IF EXISTS update_updated_at_column();

-- ============================================================================
-- DROP TABLES (in reverse dependency order)
-- ============================================================================

-- Drop tables with foreign key dependencies first
DROP TABLE IF EXISTS group_savings_accounts CASCADE;
DROP TABLE IF EXISTS group_members CASCADE;
DROP TABLE IF EXISTS groups CASCADE;
DROP TABLE IF EXISTS users CASCADE;

-- ============================================================================
-- ROLLBACK SUCCESS CONFIRMATION
-- ============================================================================

DO $$
BEGIN
    RAISE NOTICE 'Rollback U001__drop_core_schema.sql completed successfully';
    RAISE NOTICE 'Tables dropped: users, groups, group_members, group_savings_accounts';
    RAISE NOTICE 'Triggers dropped: 3 updated_at triggers';
    RAISE NOTICE 'Functions dropped: update_updated_at_column';
END $$;

-- Verify tables are dropped
SELECT 
    COUNT(*) as remaining_tables
FROM pg_tables
WHERE schemaname = 'public'
    AND tablename IN ('users', 'groups', 'group_members', 'group_savings_accounts');
