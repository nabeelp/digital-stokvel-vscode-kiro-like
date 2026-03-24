-- =====================================================================================
-- Migration: V007 - Create Credit Profiles Table
-- Description: Implements credit profile tracking for Stokvel Score calculation
--              and optional credit bureau reporting (Phase 2 feature)
-- Author: Digital Stokvel Team
-- Date: 2026-03-24
-- Dependencies: V001 (users table)
-- =====================================================================================

-- =====================================================================================
-- CREDIT_PROFILES TABLE
-- Purpose: Stores credit profile data for members, tracking contribution behavior
--          for Stokvel Score and optional credit bureau reporting
-- Features:
--   - Stokvel Score calculation (0-100)
--   - Contribution streak tracking
--   - On-time payment percentage
--   - Optional credit bureau reporting with consent
--   - Pre-qualification eligibility tracking
-- =====================================================================================

CREATE TABLE IF NOT EXISTS credit_profiles (
    -- Primary key
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    -- Foreign key (one profile per user)
    user_id UUID NOT NULL UNIQUE REFERENCES users(id) ON DELETE CASCADE,
    
    -- Stokvel Score metrics (0-100 scale)
    stokvel_score INTEGER NOT NULL DEFAULT 0 
        CHECK (stokvel_score >= 0 AND stokvel_score <= 100),
    
    -- Contribution behavior tracking
    contribution_streak INTEGER NOT NULL DEFAULT 0 
        CHECK (contribution_streak >= 0),
    contribution_months INTEGER NOT NULL DEFAULT 0 
        CHECK (contribution_months >= 0),
    total_contributions DECIMAL(12,2) NOT NULL DEFAULT 0.00 
        CHECK (total_contributions >= 0),
    on_time_payment_percentage INTEGER NOT NULL DEFAULT 0 
        CHECK (on_time_payment_percentage >= 0 AND on_time_payment_percentage <= 100),
    
    -- Credit bureau reporting (opt-in)
    opted_in_credit_bureau BOOLEAN NOT NULL DEFAULT FALSE,
    last_bureau_report_at TIMESTAMP,
    
    -- Audit timestamps
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================================================
-- CREDIT_PROFILES INDEXES
-- Purpose: Optimize queries for credit scoring and reporting
-- =====================================================================================

-- Lookup credit profile by user
CREATE UNIQUE INDEX idx_credit_profiles_user_id 
    ON credit_profiles(user_id);

-- Find top-scoring members for pre-qualification
CREATE INDEX idx_credit_profiles_stokvel_score 
    ON credit_profiles(stokvel_score DESC);

-- Find opted-in members for credit bureau batch reporting
CREATE INDEX idx_credit_profiles_opted_in 
    ON credit_profiles(opted_in_credit_bureau, last_bureau_report_at ASC) 
    WHERE opted_in_credit_bureau = TRUE;

-- Contribution streak leaderboard
CREATE INDEX idx_credit_profiles_contribution_streak 
    ON credit_profiles(contribution_streak DESC);

-- Pre-qualification eligibility query
CREATE INDEX idx_credit_profiles_pre_qualification 
    ON credit_profiles(stokvel_score, contribution_months) 
    WHERE stokvel_score >= 75 AND contribution_months >= 12;

-- Recent profile updates
CREATE INDEX idx_credit_profiles_updated_at 
    ON credit_profiles(updated_at DESC);

-- =====================================================================================
-- AUTOMATIC TIMESTAMP TRIGGER
-- Purpose: Automatically update updated_at timestamp on record modification
-- =====================================================================================

CREATE TRIGGER update_credit_profiles_updated_at
    BEFORE UPDATE ON credit_profiles
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- =====================================================================================
-- CREDIT PROFILE STATISTICS VIEW
-- Purpose: Aggregated statistics for credit profile metrics
-- =====================================================================================

CREATE OR REPLACE VIEW credit_profile_statistics AS
SELECT 
    COUNT(*) AS total_profiles,
    COUNT(*) FILTER (WHERE opted_in_credit_bureau) AS opted_in_count,
    ROUND(AVG(stokvel_score), 2) AS avg_stokvel_score,
    ROUND(AVG(contribution_streak), 2) AS avg_contribution_streak,
    ROUND(AVG(on_time_payment_percentage), 2) AS avg_on_time_percentage,
    COUNT(*) FILTER (WHERE stokvel_score >= 75 AND contribution_months >= 12) AS pre_qualified_count,
    ROUND(SUM(total_contributions), 2) AS total_platform_contributions
FROM credit_profiles;

-- =====================================================================================
-- STOKVEL SCORE LEADERBOARD VIEW
-- Purpose: Top members by Stokvel Score for gamification
-- =====================================================================================

CREATE OR REPLACE VIEW stokvel_score_leaderboard AS
SELECT 
    cp.user_id,
    u.first_name,
    u.last_name,
    cp.stokvel_score,
    cp.contribution_streak,
    cp.contribution_months,
    cp.on_time_payment_percentage,
    cp.total_contributions,
    RANK() OVER (ORDER BY cp.stokvel_score DESC, cp.contribution_streak DESC) AS rank
FROM credit_profiles cp
JOIN users u ON u.id = cp.user_id
WHERE cp.stokvel_score > 0
ORDER BY rank ASC
LIMIT 100;

-- =====================================================================================
-- HELPER FUNCTION: Calculate Stokvel Score
-- Purpose: Algorithm to calculate Stokvel Score based on contribution behavior
--          Score = (On-time % * 0.5) + (Streak bonus * 0.3) + (Consistency * 0.2)
-- =====================================================================================

CREATE OR REPLACE FUNCTION calculate_stokvel_score(p_user_id UUID)
RETURNS INTEGER AS $$
DECLARE
    v_on_time_percentage INTEGER;
    v_contribution_streak INTEGER;
    v_contribution_months INTEGER;
    v_consistency_score DECIMAL(5,2);
    v_stokvel_score INTEGER;
BEGIN
    -- Get current profile metrics
    SELECT 
        on_time_payment_percentage,
        contribution_streak,
        contribution_months
    INTO 
        v_on_time_percentage,
        v_contribution_streak,
        v_contribution_months
    FROM credit_profiles
    WHERE user_id = p_user_id;
    
    -- If profile doesn't exist or no contributions yet, return 0
    IF v_contribution_months IS NULL OR v_contribution_months = 0 THEN
        RETURN 0;
    END IF;
    
    -- Calculate consistency score (streak / total months)
    v_consistency_score := (v_contribution_streak::DECIMAL / v_contribution_months::DECIMAL) * 100;
    
    -- Calculate Stokvel Score using weighted formula
    v_stokvel_score := ROUND(
        (v_on_time_percentage * 0.5) +                    -- 50% weight on on-time payments
        (LEAST(v_contribution_streak, 24) * 100 / 24 * 0.3) +  -- 30% weight on streak (capped at 24 months)
        (v_consistency_score * 0.2)                       -- 20% weight on consistency
    );
    
    -- Ensure score is within 0-100 range
    v_stokvel_score := GREATEST(0, LEAST(100, v_stokvel_score));
    
    RETURN v_stokvel_score;
END;
$$ LANGUAGE plpgsql;

-- =====================================================================================
-- HELPER FUNCTION: Update Credit Profile from Contributions
-- Purpose: Update credit profile metrics based on contribution history
-- =====================================================================================

CREATE OR REPLACE FUNCTION update_credit_profile_metrics(p_user_id UUID)
RETURNS BOOLEAN AS $$
DECLARE
    v_total_contributions DECIMAL(12,2);
    v_total_payments INTEGER;
    v_on_time_payments INTEGER;
    v_on_time_percentage INTEGER;
    v_contribution_months INTEGER;
    v_contribution_streak INTEGER;
    v_stokvel_score INTEGER;
    v_current_streak INTEGER := 0;
    v_last_month DATE;
    v_month_record RECORD;
BEGIN
    -- Calculate total contributions
    SELECT COALESCE(SUM(amount), 0)
    INTO v_total_contributions
    FROM contributions c
    JOIN group_members gm ON gm.id = c.member_id
    WHERE gm.user_id = p_user_id
      AND c.status = 'paid';
    
    -- Calculate on-time payment percentage
    SELECT 
        COUNT(*) AS total,
        COUNT(*) FILTER (WHERE paid_at <= due_date) AS on_time
    INTO v_total_payments, v_on_time_payments
    FROM contributions c
    JOIN group_members gm ON gm.id = c.member_id
    WHERE gm.user_id = p_user_id
      AND c.status = 'paid';
    
    IF v_total_payments > 0 THEN
        v_on_time_percentage := ROUND((v_on_time_payments::DECIMAL / v_total_payments::DECIMAL) * 100);
    ELSE
        v_on_time_percentage := 0;
    END IF;
    
    -- Calculate contribution months (distinct months with at least one contribution)
    SELECT COUNT(DISTINCT DATE_TRUNC('month', paid_at))::INTEGER
    INTO v_contribution_months
    FROM contributions c
    JOIN group_members gm ON gm.id = c.member_id
    WHERE gm.user_id = p_user_id
      AND c.status = 'paid'
      AND paid_at IS NOT NULL;
    
    -- Calculate contribution streak (consecutive months with on-time payments)
    v_contribution_streak := 0;
    v_last_month := NULL;
    
    FOR v_month_record IN (
        SELECT DATE_TRUNC('month', paid_at)::DATE AS month
        FROM contributions c
        JOIN group_members gm ON gm.id = c.member_id
        WHERE gm.user_id = p_user_id
          AND c.status = 'paid'
          AND paid_at <= due_date
        GROUP BY DATE_TRUNC('month', paid_at)
        HAVING COUNT(*) > 0
        ORDER BY month DESC
    ) LOOP
        IF v_last_month IS NULL THEN
            -- First iteration
            v_current_streak := 1;
            v_last_month := v_month_record.month;
        ELSIF v_month_record.month = (v_last_month - INTERVAL '1 month')::DATE THEN
            -- Consecutive month
            v_current_streak := v_current_streak + 1;
            v_last_month := v_month_record.month;
        ELSE
            -- Streak broken
            EXIT;
        END IF;
    END LOOP;
    
    v_contribution_streak := v_current_streak;
    
    -- Update credit profile
    UPDATE credit_profiles
    SET 
        total_contributions = v_total_contributions,
        on_time_payment_percentage = v_on_time_percentage,
        contribution_months = v_contribution_months,
        contribution_streak = v_contribution_streak,
        updated_at = CURRENT_TIMESTAMP
    WHERE user_id = p_user_id;
    
    -- Calculate and update Stokvel Score
    v_stokvel_score := calculate_stokvel_score(p_user_id);
    
    UPDATE credit_profiles
    SET 
        stokvel_score = v_stokvel_score,
        updated_at = CURRENT_TIMESTAMP
    WHERE user_id = p_user_id;
    
    RETURN TRUE;
END;
$$ LANGUAGE plpgsql;

-- =====================================================================================
-- HELPER FUNCTION: Get Pre-Qualified Members
-- Purpose: Find members eligible for loan pre-qualification
--          Criteria: Stokvel Score >= 75 AND contribution_months >= 12
-- =====================================================================================

CREATE OR REPLACE FUNCTION get_pre_qualified_members()
RETURNS TABLE (
    user_id UUID,
    stokvel_score INTEGER,
    contribution_months INTEGER,
    contribution_streak INTEGER,
    total_contributions DECIMAL,
    on_time_payment_percentage INTEGER
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        cp.user_id,
        cp.stokvel_score,
        cp.contribution_months,
        cp.contribution_streak,
        cp.total_contributions,
        cp.on_time_payment_percentage
    FROM credit_profiles cp
    WHERE cp.stokvel_score >= 75
      AND cp.contribution_months >= 12
    ORDER BY cp.stokvel_score DESC, cp.contribution_streak DESC;
END;
$$ LANGUAGE plpgsql;

-- =====================================================================================
-- HELPER FUNCTION: Get Members Due for Credit Bureau Report
-- Purpose: Find opted-in members who haven't been reported in the last 30 days
-- =====================================================================================

CREATE OR REPLACE FUNCTION get_members_due_for_bureau_report()
RETURNS TABLE (
    user_id UUID,
    stokvel_score INTEGER,
    last_bureau_report_at TIMESTAMP,
    days_since_last_report INTEGER
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        cp.user_id,
        cp.stokvel_score,
        cp.last_bureau_report_at,
        EXTRACT(DAY FROM (CURRENT_TIMESTAMP - COALESCE(cp.last_bureau_report_at, cp.created_at)))::INTEGER AS days_since_last_report
    FROM credit_profiles cp
    WHERE cp.opted_in_credit_bureau = TRUE
      AND cp.stokvel_score > 0
      AND (
          cp.last_bureau_report_at IS NULL 
          OR cp.last_bureau_report_at < (CURRENT_TIMESTAMP - INTERVAL '30 days')
      )
    ORDER BY cp.last_bureau_report_at ASC NULLS FIRST;
END;
$$ LANGUAGE plpgsql;

-- =====================================================================================
-- END OF MIGRATION V007
-- =====================================================================================
