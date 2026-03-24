-- =====================================================================================
-- Rollback Migration: U004 - Drop Votes and Vote Responses Tables
-- Description: Safely removes voting functionality tables and related objects
-- Author: Digital Stokvel Team
-- Date: 2026-03-24
-- Rollback For: V004__create_votes_tables.sql
-- =====================================================================================

-- Drop view
DROP VIEW IF EXISTS vote_summary CASCADE;

-- Drop helper functions
DROP FUNCTION IF EXISTS check_vote_quorum(UUID) CASCADE;
DROP FUNCTION IF EXISTS get_vote_participation_rate(UUID) CASCADE;

-- Drop triggers (automatically dropped with tables, but explicit for clarity)
DROP TRIGGER IF EXISTS update_vote_responses_updated_at ON vote_responses;
DROP TRIGGER IF EXISTS update_votes_updated_at ON votes;

-- Drop tables in dependency order
DROP TABLE IF EXISTS vote_responses CASCADE;
DROP TABLE IF EXISTS votes CASCADE;

-- =====================================================================================
-- Confirmation Message
-- =====================================================================================
DO $$
BEGIN
    RAISE NOTICE 'Successfully rolled back V004: votes and vote_responses tables dropped';
END $$;
