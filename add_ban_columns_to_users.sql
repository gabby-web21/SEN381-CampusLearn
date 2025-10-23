-- Add ban status columns to users table for admin functionality
-- Run this script in your Supabase SQL Editor

-- Add ban status columns to users table
ALTER TABLE users 
ADD COLUMN IF NOT EXISTS is_banned BOOLEAN DEFAULT FALSE,
ADD COLUMN IF NOT EXISTS banned_at TIMESTAMP WITH TIME ZONE,
ADD COLUMN IF NOT EXISTS banned_by INTEGER,
ADD COLUMN IF NOT EXISTS ban_reason TEXT;

-- Add foreign key constraint for banned_by (references users table)
ALTER TABLE users 
ADD CONSTRAINT fk_users_banned_by 
FOREIGN KEY (banned_by) REFERENCES users(user_id) ON DELETE SET NULL;

-- Create index for better performance on ban queries
CREATE INDEX IF NOT EXISTS idx_users_is_banned ON users(is_banned);
CREATE INDEX IF NOT EXISTS idx_users_banned_at ON users(banned_at);

-- Add comments to document the new columns
COMMENT ON COLUMN users.is_banned IS 'Whether the user is currently banned';
COMMENT ON COLUMN users.banned_at IS 'When the user was banned';
COMMENT ON COLUMN users.banned_by IS 'Admin user ID who banned this user';
COMMENT ON COLUMN users.ban_reason IS 'Reason for the ban';

-- Grant permissions to all roles
GRANT ALL ON users TO postgres;
GRANT ALL ON users TO authenticated;
GRANT ALL ON users TO anon;
GRANT ALL ON users TO service_role;
GRANT ALL ON users TO PUBLIC;

