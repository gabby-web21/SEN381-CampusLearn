using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Sen381.Business
{
    [Table("users")] // ⬅️ IMPORTANT: table only, no schema here
    public class User : BaseModel
    {
        [PrimaryKey("id", false)]
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

        public void ChangePassword(string newPassword) =>
            PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(newPassword));

        public bool VerifyPassword(string inputPassword) =>
            PasswordHash == Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(inputPassword));

        public void UpdateProfile(string newFirstName, string newLastName, string newPhoneNum, string newEmail, string newProfilePicturePath = null)
        {
            FirstName = newFirstName;
            LastName = newLastName;
            PhoneNum = newPhoneNum;
            Email = newEmail;
            if (!string.IsNullOrEmpty(newProfilePicturePath))
                ProfilePicturePath = newProfilePicturePath;
        }

        public void SetRole(Role role) => RoleString = role.ToString();
        public Role GetRole() => Enum.TryParse<Role>(RoleString, out var r) ? r : Role.Student;
    }

    public enum Role { Student, Admin }
}
