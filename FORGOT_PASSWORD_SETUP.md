# Forgot Password Functionality Setup

This document explains the forgot password functionality that has been implemented for the CampusLearn application.

## Overview

The forgot password feature allows users to reset their password by receiving a secure reset link via email. The implementation includes:

1. **Frontend Pages**: `ForgotPassword.razor` and `ResetPassword.razor`
2. **Backend Controller**: `PasswordResetController.cs`
3. **Database Model**: `PasswordResetToken.cs`
4. **Email Service**: Updated `EmailService.cs`

## Database Setup

Before using the forgot password functionality, you need to create the required database table:

```sql
-- Run this SQL script in your Supabase database
CREATE TABLE IF NOT EXISTS password_reset_tokens (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    token_hash VARCHAR(64) NOT NULL UNIQUE,
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    is_used BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_password_reset_tokens_token_hash ON password_reset_tokens(token_hash);
CREATE INDEX IF NOT EXISTS idx_password_reset_tokens_user_id ON password_reset_tokens(user_id);
CREATE INDEX IF NOT EXISTS idx_password_reset_tokens_expires_at ON password_reset_tokens(expires_at);
```

## How It Works

### 1. User Requests Password Reset
- User clicks "Forgot Password?" on the login page
- User is redirected to `/forgotpassword`
- User enters their email address
- System sends a password reset email (if the email exists in the database)

### 2. User Receives Email
- Email contains a secure reset link: `https://localhost:7228/reset-password?token=...`
- Token expires after 1 hour for security
- Token can only be used once

### 3. User Resets Password
- User clicks the link in their email
- User is redirected to `/reset-password` with the token
- User enters their new password
- System validates the token and updates the password
- User is redirected to the login page

## Security Features

- **Secure Token Generation**: Uses cryptographically secure random tokens
- **Token Hashing**: Tokens are hashed before storage in the database
- **Time Expiration**: Tokens expire after 1 hour
- **Single Use**: Tokens can only be used once
- **Email Validation**: Only sends reset emails to existing users (no user enumeration)
- **Password Hashing**: New passwords are hashed using SHA256

## API Endpoints

### POST `/api/PasswordReset/forgot-password`
**Request Body:**
```json
{
  "email": "user@example.com"
}
```

**Response:**
```json
{
  "message": "If an account with that email exists, a password reset link has been sent."
}
```

### POST `/api/PasswordReset/reset-password`
**Request Body:**
```json
{
  "token": "reset_token_from_email",
  "newPassword": "new_password"
}
```

**Response:**
```json
{
  "message": "Password has been reset successfully."
}
```

## Frontend Pages

### ForgotPassword.razor (`/forgotpassword`)
- Email input form
- Validates email format
- Shows success/error messages
- Links back to login page

### ResetPassword.razor (`/reset-password`)
- New password input form
- Password confirmation
- Validates token from URL parameter
- Shows success/error messages
- Redirects to login after successful reset

## Configuration

The email service is configured in `Sen381/Business/Services/EmailService.cs`:

```csharp
private readonly string _fromAddress = "xavierbarnard10@gmail.com";
private readonly string _password = "ivdq zduu tsik tnoy";
private readonly string _baseUrl = "https://localhost:7228";
```

**Important**: Update these values for your production environment.

## Testing

1. **Start the backend server** (Sen381Backend project)
2. **Start the frontend** (Frontend project)
3. **Navigate to login page** and click "Forgot Password?"
4. **Enter a valid email** that exists in your database
5. **Check your email** for the reset link
6. **Click the reset link** and set a new password
7. **Try logging in** with the new password

## Troubleshooting

### Common Issues:

1. **Email not sending**: Check SMTP configuration in EmailService
2. **Token not working**: Ensure database table is created and token hasn't expired
3. **CORS errors**: Check CORS configuration in Program.cs
4. **Database connection**: Verify Supabase connection settings

### Logs to Check:

- Backend console logs show password reset attempts
- Check Supabase logs for database operations
- Email service logs show email sending status

## Security Considerations

- Tokens are cryptographically secure and unpredictable
- Tokens are hashed before database storage
- Tokens expire after 1 hour
- Tokens are single-use only
- No user enumeration (same response for valid/invalid emails)
- Passwords are properly hashed using SHA256

## Future Enhancements

- Add rate limiting to prevent abuse
- Add email templates for better user experience
- Add password strength requirements
- Add account lockout after multiple failed attempts
- Add audit logging for security events
