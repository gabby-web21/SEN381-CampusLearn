using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Sen381.Business.Models
{
    [Table("email_verification_tokens")]
    public class EmailVerificationToken : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("token_hash")]
        public string TokenHash { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        public void MarkUsed()
        {
            // In real implementation, update in DB
            ExpiresAt = DateTime.UtcNow; // expire immediately
            Console.WriteLine("Email verification token marked as used.");
        }

        public void Revoke()
        {
            ExpiresAt = DateTime.UtcNow;
            Console.WriteLine("Email verification token revoked.");
        }

        public TimeSpan RemainingTime()
        {
            return ExpiresAt - DateTime.UtcNow;
        }
    }
}
