-- =====================================================================================
-- Rollback Migration: U006 - Drop Notifications Table
-- Description: Safely removes notification delivery system
-- Author: Digital Stokvel Team
-- Date: 2026-03-24
-- Rollback For: V006__create_notifications_table.sql
-- =====================================================================================

-- Drop views
DROP VIEW IF EXISTS recipient_notification_summary CASCADE;
DROP VIEW IF EXISTS notification_statistics CASCADE;

-- Drop helper functions
DROP FUNCTION IF EXISTS retry_failed_notification(UUID) CASCADE;
DROP FUNCTION IF EXISTS mark_notification_failed(UUID, TEXT) CASCADE;
DROP FUNCTION IF EXISTS mark_notification_delivered(UUID) CASCADE;
DROP FUNCTION IF EXISTS mark_notification_sent(UUID) CASCADE;
DROP FUNCTION IF EXISTS get_failed_notifications_for_retry(INTEGER) CASCADE;
DROP FUNCTION IF EXISTS get_pending_notifications(VARCHAR, INTEGER) CASCADE;

-- Drop trigger (automatically dropped with table, but explicit for clarity)
DROP TRIGGER IF EXISTS update_notifications_updated_at ON notifications;

-- Drop table
DROP TABLE IF EXISTS notifications CASCADE;

-- =====================================================================================
-- Confirmation Message
-- =====================================================================================
DO $$
BEGIN
    RAISE NOTICE 'Successfully rolled back V006: notifications table and related objects dropped';
END $$;
