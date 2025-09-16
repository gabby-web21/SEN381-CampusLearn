using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business
{
    public class EmailVerificationToken
    {
        // ---------- Fields ----------
        private int id;
        private string tokenHash;
        private int userId;

        // ---------- Properties ----------
        public int Id
        {
            get => id;
            set => id = value;
        }

        public int UserId
        {
            get => userId;
            set => userId = value;
        }

        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }

        // ---------- Constructor ----------
        public EmailVerificationToken(int userId, string tokenHash, DateTime expiresAt)
        {
            this.userId = userId;
            this.tokenHash = tokenHash;
            this.CreatedAt = DateTime.Now;
            this.ExpiresAt = expiresAt;
        }

        // ---------- Methods ----------
        public void MarkUsed()
        {
            // In real implementation, mark as consumed
            Console.WriteLine("Email verification token marked as used.");
        }

        public void Revoke()
        {
            // Expire immediately
            ExpiresAt = DateTime.Now;
            Console.WriteLine("Email verification token revoked.");
        }

        public TimeSpan RemainingTime()
        {
            return ExpiresAt - DateTime.Now;
        }
    }
}
