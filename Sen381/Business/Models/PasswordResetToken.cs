using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Sen381.Business.Models
{
    [Table("password_reset_tokens")]
    public class PasswordResetToken : BaseModel
    {
        [PrimaryKey("id", true)]
        [Column("id", ignoreOnInsert: true)]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("token_hash")]
        public string TokenHash { get; set; }

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Column("is_used")]
        public bool IsUsed { get; set; }

        [Column("created_at", ignoreOnInsert: true, ignoreOnUpdate: true)]
        public DateTime? CreatedAt { get; set; }

        // Helper method to check if token is expired
        public bool IsExpired()
        {
            return DateTime.UtcNow > ExpiresAt;
        }

        // Helper method to check if token is valid (not expired and not used)
        public bool IsValid()
        {
            return !IsExpired() && !IsUsed;
        }

        // Mark token as used
        public void MarkAsUsed()
        {
            IsUsed = true;
        }

        // Legacy methods for backward compatibility
        public void MarkUsed()
        {
            MarkAsUsed();
        }

        public void Revoke()
        {
            // Invalidate immediately by expiring now
            ExpiresAt = DateTime.UtcNow;
        }

        public TimeSpan RemainingTime()
        {
            return ExpiresAt - DateTime.UtcNow;
        }
    }
}
