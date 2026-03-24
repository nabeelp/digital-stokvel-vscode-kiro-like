-- Digital Stokvel Banking - Primary Database Initialization
-- Version: 1.0
-- Date: 2026-03-24

-- Enable required PostgreSQL extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "pg_stat_statements";

-- Create enum types
CREATE TYPE group_type AS ENUM ('rotating_payout', 'savings_pot', 'investment_club');
CREATE TYPE contribution_frequency AS ENUM ('weekly', 'monthly');
CREATE TYPE contribution_status AS ENUM ('pending', 'paid', 'failed', 'refunded');
CREATE TYPE payout_type AS ENUM ('rotating', 'year_end_pot', 'emergency');
CREATE TYPE payout_status AS ENUM ('pending_approval', 'approved', 'executed', 'failed');
CREATE TYPE member_role AS ENUM ('member', 'secretary', 'treasurer', 'chairperson');
CREATE TYPE member_status AS ENUM ('active', 'suspended', 'left');
CREATE TYPE group_status AS ENUM ('active', 'suspended', 'closed');
CREATE TYPE dispute_type AS ENUM ('missed_payment', 'fraud', 'constitution_violation', 'other');
CREATE TYPE dispute_status AS ENUM ('open', 'investigating', 'resolved', 'escalated');
CREATE TYPE vote_status AS ENUM ('draft', 'active', 'closed');
CREATE TYPE notification_channel AS ENUM ('push', 'sms', 'ussd');
CREATE TYPE notification_status AS ENUM ('pending', 'sent', 'delivered', 'failed');

-- Success message
SELECT 'Primary database initialized successfully' AS status;
