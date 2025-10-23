using Microsoft.AspNetCore.Mvc;
using Sen381.Data_Access;
using Sen381.Business.Models;
using Sen381.Business.Services;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sen381Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PasswordResetController : ControllerBase
    {
        private readonly SupaBaseAuthService _supabase;
        private readonly EmailService _emailService;

        public PasswordResetController(SupaBaseAuthService supabase)
        {
            _supabase = supabase;
            _emailService = new EmailService();
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            Console.WriteLine("[PASSWORD RESET TEST] Controller is working!");
            return Ok(new { message = "PasswordResetController is working!" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            Console.WriteLine($"[FORGOT PASSWORD] Email='{request.Email}'");

            // Basic validation
            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
            {
                return BadRequest(new { error = "Please enter a valid email address." });
            }

            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // Check if user exists
                var userResponse = await client
                    .From<User>()
                    .Select("*")
                    .Where(u => u.Email == request.Email)
                    .Get();

                var user = userResponse.Models.FirstOrDefault();
                if (user == null)
                {
                    // Don't reveal if email exists or not for security
                    Console.WriteLine($"[FORGOT PASSWORD] Email not found: {request.Email}");
                    return Ok(new { message = "If an account with that email exists, a password reset link has been sent." });
                }

                // Generate a secure token
                var token = GenerateSecureToken();
                var tokenHash = HashToken(token);

                // Create password reset token
                var resetToken = new PasswordResetToken
                {
                    UserId = user.Id,
                    TokenHash = tokenHash,
                    ExpiresAt = DateTime.UtcNow.AddHours(1), // Token expires in 1 hour
                    IsUsed = false
                };

                // Save token to database
                await client.From<PasswordResetToken>().Insert(resetToken);

                // Send reset email
                _emailService.SendPasswordResetEmail(user.Email, token);

                Console.WriteLine($"[FORGOT PASSWORD] Reset token sent to: {user.Email}");
                return Ok(new { message = "If an account with that email exists, a password reset link has been sent." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FORGOT PASSWORD ERROR] {ex.Message}");
                return StatusCode(500, new { error = "An error occurred while processing your request." });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            Console.WriteLine($"[RESET PASSWORD] Token='{request.Token?.Substring(0, 8)}...'");

            // Basic validation
            if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { error = "Token and new password are required." });
            }

            if (request.NewPassword.Length < 6)
            {
                return BadRequest(new { error = "Password must be at least 6 characters long." });
            }

            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // Hash the token to find it in database
                var tokenHash = HashToken(request.Token);

                // Find the token
                var tokenResponse = await client
                    .From<PasswordResetToken>()
                    .Select("*")
                    .Where(t => t.TokenHash == tokenHash)
                    .Get();

                var resetToken = tokenResponse.Models.FirstOrDefault();
                if (resetToken == null)
                {
                    Console.WriteLine($"[RESET PASSWORD] Invalid token");
                    return BadRequest(new { error = "Invalid or expired reset token." });
                }

                // Check if token is valid
                if (!resetToken.IsValid())
                {
                    Console.WriteLine($"[RESET PASSWORD] Token expired or used");
                    return BadRequest(new { error = "Invalid or expired reset token." });
                }

                // Get the user
                var userResponse = await client
                    .From<User>()
                    .Select("*")
                    .Where(u => u.Id == resetToken.UserId)
                    .Get();

                var user = userResponse.Models.FirstOrDefault();
                if (user == null)
                {
                    Console.WriteLine($"[RESET PASSWORD] User not found for token");
                    return BadRequest(new { error = "User not found." });
                }

                // Check if the new password is the same as the current password
                using var sha = SHA256.Create();
                var hashedBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(request.NewPassword));
                var hashedPassword = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();

                // Check if the new password is the same as the current password
                if (string.Equals(user.PasswordHash, hashedPassword, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"[RESET PASSWORD] User tried to use the same password: {user.Email}");
                    return BadRequest(new { error = "The new password must be different from your current password." });
                }

                // Update user's password
                Console.WriteLine($"[RESET PASSWORD DEBUG] Old hash: {user.PasswordHash}");
                Console.WriteLine($"[RESET PASSWORD DEBUG] New hash: {hashedPassword}");
                
                user.PasswordHash = hashedPassword;
                
                // Try different update methods
                try
                {
                    // Method 1: Direct update
                    var updateResult = await client.From<User>().Update(user);
                    Console.WriteLine($"[RESET PASSWORD DEBUG] Update result: {updateResult.Models.Count} rows affected");
                }
                catch (Exception updateEx)
                {
                    Console.WriteLine($"[RESET PASSWORD DEBUG] Update failed: {updateEx.Message}");
                    
                    // Method 2: Try upsert
                    try
                    {
                        var upsertResult = await client.From<User>().Upsert(user);
                        Console.WriteLine($"[RESET PASSWORD DEBUG] Upsert result: {upsertResult.Models.Count} rows affected");
                    }
                    catch (Exception upsertEx)
                    {
                        Console.WriteLine($"[RESET PASSWORD DEBUG] Upsert failed: {upsertEx.Message}");
                        return StatusCode(500, new { error = "Failed to update password in database." });
                    }
                }

                // Mark token as used
                resetToken.MarkAsUsed();
                await client.From<PasswordResetToken>().Update(resetToken);

                Console.WriteLine($"[RESET PASSWORD] Password updated for user: {user.Email}");
                return Ok(new { message = "Password has been reset successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RESET PASSWORD ERROR] {ex.Message}");
                return StatusCode(500, new { error = "An error occurred while resetting your password." });
            }
        }

        private string GenerateSecureToken()
        {
            // Generate a cryptographically secure random token
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        private string HashToken(string token)
        {
            using var sha = SHA256.Create();
            var hashedBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
