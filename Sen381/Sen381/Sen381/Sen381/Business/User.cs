using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business
{

    public class User
    {
        private int id;
        private string firstName;
        private string lastName;
        private string phoneNum;
        private string email;
        private string passwordHash;
        private string profilePicturePath;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public string FirstName
        {
            get { return firstName; }
            set { firstName = value; }
        }

        public string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }

        public string PhoneNum
        {
            get { return phoneNum; }
            set { phoneNum = value; }
        }

        public string Email
        {
            get { return email; }
            set { email = value; }
        }

        public bool IsEmailVerified { get; set; }

        public Role Role { get; set; }   // Uses enum below

        public DateTime CreatedAt { get; set; }

        public DateTime LastLogin { get; set; }

        public string ProfilePicturePath
        {
            get { return profilePicturePath; }
            set { profilePicturePath = value; }
        }

        // Change password method (update hashing later for better encryption)
        public void ChangePassword(string newPassword)
        {
            passwordHash = HashPassword(newPassword);
            Console.WriteLine("Password changed successfully.");
        }

        // Verify a password against stored hash
        public bool VerifyPassword(string inputPassword)
        {
            return passwordHash == HashPassword(inputPassword);
        }

        // User can change details on profile
        public void UpdateProfile(string newFirstName, string newLastName, string newPhoneNum, string newEmail, string newProfilePicturePath = null)
        {
            FirstName = newFirstName;
            LastName = newLastName;
            PhoneNum = newPhoneNum;
            Email = newEmail;

            if (!string.IsNullOrEmpty(newProfilePicturePath))
            {
                ProfilePicturePath = newProfilePicturePath;
            }

            Console.WriteLine("Profile updated successfully.");
        }

        private string HashPassword(string password)
        {
            // Simple placeholder hashing (replace with real hashing, e.g., BCrypt or SHA256)
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }

    // Enum aligned with UML
    public enum Role
    {
        Student,
        Admin
    }
}
