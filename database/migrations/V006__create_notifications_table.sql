-- =====================================================================================
-- Migration: V006 - Create Notifications Table
-- Description: Implements multi-channel notification delivery system supporting
--              push notifications, SMS, and USSD messaging with templating
-- Author: Digital Stokvel Team
-- Date: 2026-03-24
-- Dependencies: V001 (users table)
-- =====================================================================================

-- =====================================================================================
-- NOTIFICATIONS TABLE
-- Purpose: Stores notification records for multi-channel delivery tracking
-- Features:
--   - Multi-channel support: push, sms, ussd
--   - Template-based messaging with dynamic payload
--   - Status lifecycle: pending -> sent -> delivered/failed
--   - Delivery tracking with timestamps
--   - Retry capability for failed notifications
-- =====================================================================================

CREATE TABLE IF NOT EXISTS notifications (
    -- Primary key
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    -- Foreign key
    recipient_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    
    -- Notification details
    channel VARCHAR(10) NOT NULL 
        CHECK (channel IN ('push', 'sms', 'ussd')),
    template_key VARCHAR(100) NOT NULL,
    payload JSONB NOT NULL DEFAULT '{}',
    
    -- Delivery tracking
    status VARCHAR(20) NOT NULL DEFAULT 'pending' 
        CHECK (status IN ('pending', 'sent', 'delivered', 'failed')),
    sent_at TIMESTAMP,
    delivered_at TIMESTAMP,
    
    -- Retry and error handling
    retry_count INTEGER NOT NULL DEFAULT 0 
        CHECK (retry_count >= 0 AND retry_count <= 3),
    error_message TEXT,
    
    -- Audit timestamps
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    -- Business rule constraints
    CONSTRAINT sent_notifications_have_timestamp 
        CHECK (status != 'sent' OR sent_at IS NOT NULL),
    CONSTRAINT delivered_notifications_have_timestamp 
        CHECK (status != 'delivered' OR delivered_at IS NOT NULL),
    CONSTRAINT delivered_after_sent 
        CHECK (delivered_at IS NULL OR sent_at IS NULL OR delivered_at >= sent_at),
    CONSTRAINT failed_notifications_have_error 
        CHECK (status != 'failed' OR error_message IS NOT NULL),
    CONSTRAINT payload_is_object 
        CHECK (jsonb_typeof(payload) = 'object')
);

-- =====================================================================================
-- NOTIFICATIONS INDEXES
-- Purpose: Optimize queries for notification delivery and reporting
-- =====================================================================================

-- Find pending/failed notifications for a recipient
CREATE INDEX idx_notifications_recipient_status 
    ON notifications(recipient_id, status);

-- Recipient notification history (most recent first)
CREATE INDEX idx_notifications_recipient_created 
    ON notifications(recipient_id, created_at DESC);

-- Find pending notifications for batch processing
CREATE INDEX idx_notifications_pending 
    ON notifications(status, created_at ASC) 
    WHERE status = 'pending';

-- Find failed notifications for retry
CREATE INDEX idx_notifications_failed_retry 
    ON notifications(status, retry_count, created_at ASC) 
    WHERE status = 'failed' AND retry_count < 3;

-- Channel-specific notification queries
CREATE INDEX idx_notifications_channel_status 
    ON notifications(channel, status);

-- Delivery tracking and analytics
CREATE INDEX idx_notifications_sent_at 
    ON notifications(sent_at DESC) 
    WHERE sent_at IS NOT NULL;

-- Template usage analytics
CREATE INDEX idx_notifications_template_key 
    ON notifications(template_key);

-- =====================================================================================
-- AUTOMATIC TIMESTAMP TRIGGER
-- Purpose: Automatically update updated_at timestamp on record modification
-- =====================================================================================

CREATE TRIGGER update_notifications_updated_at
    BEFORE UPDATE ON notifications
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- =====================================================================================
-- NOTIFICATION STATISTICS VIEW
-- Purpose: Provide aggregated notification statistics by channel and status
-- =====================================================================================

CREATE OR REPLACE VIEW notification_statistics AS
SELECT 
    channel,
    template_key,
    COUNT(*) AS total_notifications,
    COUNT(*) FILTER (WHERE status = 'pending') AS pending_count,
    COUNT(*) FILTER (WHERE status = 'sent') AS sent_count,
    COUNT(*) FILTER (WHERE status = 'delivered') AS delivered_count,
    COUNT(*) FILTER (WHERE status = 'failed') AS failed_count,
    ROUND(
        100.0 * COUNT(*) FILTER (WHERE status = 'delivered') / NULLIF(COUNT(*), 0), 
        2
    ) AS delivery_rate,
    AVG(
        EXTRACT(EPOCH FROM (delivered_at - sent_at)) / 60
    ) FILTER (WHERE delivered_at IS NOT NULL AND sent_at IS NOT NULL) AS avg_delivery_minutes
FROM notifications
GROUP BY channel, template_key;

-- =====================================================================================
-- RECIPIENT NOTIFICATION SUMMARY VIEW
-- Purpose: Summary of notification statistics per recipient
-- =====================================================================================

CREATE OR REPLACE VIEW recipient_notification_summary AS
SELECT 
    recipient_id,
    COUNT(*) AS total_notifications,
    COUNT(*) FILTER (WHERE status = 'pending') AS pending_notifications,
    COUNT(*) FILTER (WHERE status = 'delivered') AS delivered_notifications,
    COUNT(*) FILTER (WHERE status = 'failed') AS failed_notifications,
    MAX(created_at) AS last_notification_at,
    MAX(delivered_at) FILTER (WHERE status = 'delivered') AS last_delivered_at
FROM notifications
GROUP BY recipient_id;

-- =====================================================================================
-- HELPER FUNCTION: Get Pending Notifications for Batch Processing
-- Purpose: Retrieve pending notifications ordered by priority (oldest first)
-- =====================================================================================

CREATE OR REPLACE FUNCTION get_pending_notifications(
    p_channel VARCHAR DEFAULT NULL,
    p_limit INTEGER DEFAULT 100
)
RETURNS TABLE (
    notification_id UUID,
    recipient_id UUID,
    channel VARCHAR,
    template_key VARCHAR,
    payload JSONB,
    created_at TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        n.id AS notification_id,
        n.recipient_id,
        n.channel,
        n.template_key,
        n.payload,
        n.created_at
    FROM notifications n
    WHERE n.status = 'pending'
      AND (p_channel IS NULL OR n.channel = p_channel)
    ORDER BY n.created_at ASC
    LIMIT p_limit;
END;
$$ LANGUAGE plpgsql;

-- =====================================================================================
-- HELPER FUNCTION: Get Failed Notifications for Retry
-- Purpose: Retrieve failed notifications that haven't exceeded retry limit
-- =====================================================================================

CREATE OR REPLACE FUNCTION get_failed_notifications_for_retry(
    p_limit INTEGER DEFAULT 50
)
RETURNS TABLE (
    notification_id UUID,
    recipient_id UUID,
    channel VARCHAR,
    template_key VARCHAR,
    payload JSONB,
    retry_count INTEGER,
    error_message TEXT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        n.id AS notification_id,
        n.recipient_id,
        n.channel,
        n.template_key,
        n.payload,
        n.retry_count,
        n.error_message
    FROM notifications n
    WHERE n.status = 'failed'
      AND n.retry_count < 3
    ORDER BY n.created_at ASC
    LIMIT p_limit;
END;
$$ LANGUAGE plpgsql;

-- =====================================================================================
-- HELPER FUNCTION: Mark Notification as Sent
-- Purpose: Update notification status to sent with timestamp
-- =====================================================================================

CREATE OR REPLACE FUNCTION mark_notification_sent(
    p_notification_id UUID
)
RETURNS BOOLEAN AS $$
DECLARE
    v_updated_count INTEGER;
BEGIN
    UPDATE notifications
    SET 
        status = 'sent',
        sent_at = CURRENT_TIMESTAMP,
        updated_at = CURRENT_TIMESTAMP
    WHERE id = p_notification_id
      AND status = 'pending';
    
    GET DIAGNOSTICS v_updated_count = ROW_COUNT;
    
    RETURN v_updated_count > 0;
END;
$$ LANGUAGE plpgsql;

-- =====================================================================================
-- HELPER FUNCTION: Mark Notification as Delivered
-- Purpose: Update notification status to delivered with timestamp
-- =====================================================================================

CREATE OR REPLACE FUNCTION mark_notification_delivered(
    p_notification_id UUID
)
RETURNS BOOLEAN AS $$
DECLARE
    v_updated_count INTEGER;
BEGIN
    UPDATE notifications
    SET 
        status = 'delivered',
        delivered_at = CURRENT_TIMESTAMP,
        updated_at = CURRENT_TIMESTAMP
    WHERE id = p_notification_id
      AND status IN ('pending', 'sent');
    
    GET DIAGNOSTICS v_updated_count = ROW_COUNT;
    
    RETURN v_updated_count > 0;
END;
$$ LANGUAGE plpgsql;

-- =====================================================================================
-- HELPER FUNCTION: Mark Notification as Failed
-- Purpose: Update notification status to failed with error message and retry count
-- =====================================================================================

CREATE OR REPLACE FUNCTION mark_notification_failed(
    p_notification_id UUID,
    p_error_message TEXT
)
RETURNS BOOLEAN AS $$
DECLARE
    v_updated_count INTEGER;
BEGIN
    UPDATE notifications
    SET 
        status = 'failed',
        error_message = p_error_message,
        retry_count = retry_count + 1,
        updated_at = CURRENT_TIMESTAMP
    WHERE id = p_notification_id
      AND status IN ('pending', 'sent');
    
    GET DIAGNOSTICS v_updated_count = ROW_COUNT;
    
    RETURN v_updated_count > 0;
END;
$$ LANGUAGE plpgsql;

-- =====================================================================================
-- HELPER FUNCTION: Retry Failed Notification
-- Purpose: Reset failed notification to pending for retry
-- =====================================================================================

CREATE OR REPLACE FUNCTION retry_failed_notification(
    p_notification_id UUID
)
RETURNS BOOLEAN AS $$
DECLARE
    v_updated_count INTEGER;
BEGIN
    UPDATE notifications
    SET 
        status = 'pending',
        updated_at = CURRENT_TIMESTAMP
    WHERE id = p_notification_id
      AND status = 'failed'
      AND retry_count < 3;
    
    GET DIAGNOSTICS v_updated_count = ROW_COUNT;
    
    RETURN v_updated_count > 0;
END;
$$ LANGUAGE plpgsql;

-- =====================================================================================
-- END OF MIGRATION V006
-- =====================================================================================
