using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Services
{
    public class EmailService
    {
        // ---------- Properties ----------
        public string FromAddress { get; set; } = "noreply@example.com";
        public string BaseUrl { get; set; } = "https://example.com";

        // ---------- Methods ----------
        public void SendVerificationEmail(string toEmail, string token)
        {
            // TODO: build verification URL using BaseUrl + token
            string verificationUrl = $"{BaseUrl}/verify?token={token}";
            Console.WriteLine($"Verification email sent to {toEmail} from {FromAddress} with link {verificationUrl}");
        }

        public void SendPasswordResetEmail(string toEmail, string token)
        {
            // TODO: build reset URL using BaseUrl + token
            string resetUrl = $"{BaseUrl}/reset-password?token={token}";
            Console.WriteLine($"Password reset email sent to {toEmail} from {FromAddress} with link {resetUrl}");
        }

        public void SendNotification(string toEmail, string subject, string message)
        {
            // TODO: general notification logic
            Console.WriteLine($"Notification email sent to {toEmail} from {FromAddress}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Message: {message}");
        }
    }
}
