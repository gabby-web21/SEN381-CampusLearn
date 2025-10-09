using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Sen381.Business.Models
{
    [Table("users")]
    public class User : BaseModel
    {
        // ✅ Correct primary key mapping
        // "true" means it's auto-generated (serial/identity)
        [PrimaryKey("user_id", true)]
        [Column("user_id", ignoreOnInsert: true)]
        public int Id { get; set; }

        [Column("first_name")]
        public string FirstName { get; set; }

        [Column("last_name")]
        public string LastName { get; set; }

        [Column("phone_num")]
        public string PhoneNum { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("password_hash")]
        public string PasswordHash { get; set; }

        [Column("is_email_verified")]
        public bool IsEmailVerified { get; set; }

        [Column("role")]
        public string RoleString { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("last_login")]
        public DateTime LastLogin { get; set; }

        [Column("profile_picture_path")]
        public string ProfilePicturePath { get; set; }

        // 🔒 Password handling
        public void ChangePassword(string newPassword)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var hashedBytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(newPassword));
            PasswordHash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }

        public bool VerifyPassword(string inputPassword)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var hashedBytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(inputPassword));
            var hashedInput = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            return PasswordHash == hashedInput;
        }

        // 👤 Profile updates
        public void UpdateProfile(string newFirstName, string newLastName, string newPhoneNum, string newEmail, string newProfilePicturePath = null)
        {
            FirstName = newFirstName;
            LastName = newLastName;
            PhoneNum = newPhoneNum;
            Email = newEmail;
            if (!string.IsNullOrEmpty(newProfilePicturePath))
                ProfilePicturePath = newProfilePicturePath;
        }

        // 🧭 Role management
        public void SetRole(Role role) => RoleString = role.ToString();
        public Role GetRole() => Enum.TryParse<Role>(RoleString, out var r) ? r : Role.student;
    }

    public enum Role
    {
        student,
        admin
    }
}
