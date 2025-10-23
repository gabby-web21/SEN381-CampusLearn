-- SQL script to create the message_reports table in Supabase
-- This table stores reports submitted by users against messages

CREATE TABLE IF NOT EXISTS public.message_reports (
    id SERIAL PRIMARY KEY,
    reporter_id INTEGER NOT NULL,
    reported_user_id INTEGER NOT NULL,
    message_id VARCHAR(255) NOT NULL,
    message_content TEXT NOT NULL,
    message_type VARCHAR(50) NOT NULL, -- "messages", "forum", "tutoring"
    reason VARCHAR(50) NOT NULL, -- "nsfw", "harassment", "foul_language", "spam", "other"
    details TEXT,
    context_url TEXT,
    sender_name VARCHAR(255),
    reporter_name VARCHAR(255),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    is_resolved BOOLEAN DEFAULT FALSE,
    resolution VARCHAR(50), -- "approved", "dismissed", "user_suspended"
    resolved_at TIMESTAMP WITH TIME ZONE,
    resolved_by INTEGER -- Admin who resolved it
    
    -- Foreign key constraints (optional - uncomment if you want referential integrity)
    -- FOREIGN KEY (reporter_id) REFERENCES public.users(id),
    -- FOREIGN KEY (reported_user_id) REFERENCES public.users(id),
    -- FOREIGN KEY (resolved_by) REFERENCES public.users(id)
);

-- Create indexes for better query performance
CREATE INDEX IF NOT EXISTS idx_message_reports_reporter_id ON public.message_reports(reporter_id);
CREATE INDEX IF NOT EXISTS idx_message_reports_reported_user_id ON public.message_reports(reported_user_id);
CREATE INDEX IF NOT EXISTS idx_message_reports_message_type ON public.message_reports(message_type);
CREATE INDEX IF NOT EXISTS idx_message_reports_reason ON public.message_reports(reason);
CREATE INDEX IF NOT EXISTS idx_message_reports_is_resolved ON public.message_reports(is_resolved);
CREATE INDEX IF NOT EXISTS idx_message_reports_created_at ON public.message_reports(created_at DESC);

-- Add comments for documentation
COMMENT ON TABLE public.message_reports IS 'Stores reports submitted by users against inappropriate messages';
COMMENT ON COLUMN public.message_reports.id IS 'Primary key - unique identifier for each report';
COMMENT ON COLUMN public.message_reports.reporter_id IS 'ID of the user who submitted the report';
COMMENT ON COLUMN public.message_reports.reported_user_id IS 'ID of the user whose message was reported';
COMMENT ON COLUMN public.message_reports.message_id IS 'ID of the reported message';
COMMENT ON COLUMN public.message_reports.message_content IS 'Content of the reported message';
COMMENT ON COLUMN public.message_reports.message_type IS 'Type of message: messages, forum, or tutoring';
COMMENT ON COLUMN public.message_reports.reason IS 'Reason for reporting: nsfw, harassment, foul_language, spam, other';
COMMENT ON COLUMN public.message_reports.details IS 'Additional details provided by the reporter';
COMMENT ON COLUMN public.message_reports.context_url IS 'URL where the message was reported from';
COMMENT ON COLUMN public.message_reports.sender_name IS 'Name of the person who sent the message';
COMMENT ON COLUMN public.message_reports.reporter_name IS 'Name of the person who reported the message';
COMMENT ON COLUMN public.message_reports.created_at IS 'Timestamp when the report was created';
COMMENT ON COLUMN public.message_reports.is_resolved IS 'Whether the report has been resolved by an admin';
COMMENT ON COLUMN public.message_reports.resolution IS 'Resolution status: approved, dismissed, user_suspended';
COMMENT ON COLUMN public.message_reports.resolved_at IS 'Timestamp when the report was resolved';
COMMENT ON COLUMN public.message_reports.resolved_by IS 'ID of the admin who resolved the report';

-- Grant permissions (adjust as needed for your setup)
-- GRANT ALL ON public.message_reports TO authenticated;
-- GRANT ALL ON public.message_reports TO service_role;
-- GRANT USAGE, SELECT ON SEQUENCE public.message_reports_id_seq TO authenticated;
-- GRANT USAGE, SELECT ON SEQUENCE public.message_reports_id_seq TO service_role;
