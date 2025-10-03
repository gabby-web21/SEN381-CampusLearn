using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sen381.Business;
using Sen381.Data_Access;

namespace Sen381
{
    public class Register
    {
        private readonly List<User> _users = new List<User>();
        private readonly SupaBaseAuthService _supabaseService;

        public Register(SupaBaseAuthService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        public async Task StartRegisterAsync()
        {
            Console.WriteLine("Enter your first name:");
            string firstName = Console.ReadLine();

            Console.WriteLine("Enter your last name:");
            string lastName = Console.ReadLine();

            Console.WriteLine("Enter your phone number:");
            string phoneNum = Console.ReadLine();

            Console.WriteLine("Enter your email address:");
            string email = Console.ReadLine();

            string password;
            while (true)
            {
                Console.WriteLine("Enter your password:");
                password = Console.ReadLine();

                Console.WriteLine("Confirm your password:");
                string confirmPassword = Console.ReadLine();

                if (password == confirmPassword)
                    break;

                Console.WriteLine("Passwords do not match. Please try again.");
            }

            Role role;
            while (true)
            {
                Console.WriteLine("Select role (1 = Student, 2 = Admin):");
                string input = Console.ReadLine();
                if (input == "1") { role = Role.student; break; }
                if (input == "2") { role = Role.admin; break; }
                Console.WriteLine("Invalid input. Please enter 1 or 2.");
            }

            // Generate user
            var random = new Random();
            int id = random.Next(1, 1_000_000);

            var user = new User
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                PhoneNum = phoneNum,
                Email = email,
                ProfilePicturePath = "",
                CreatedAt = DateTime.UtcNow,
                IsEmailVerified = false,   // ⬅️ starts unverified
                LastLogin = DateTime.UtcNow
            };
            user.SetRole(role);
            user.ChangePassword(password);

            _users.Add(user);

            await PushUserToDatabase(user);

            // After user is saved, generate token + send email
            await SendVerificationEmail(user);
        }

        private async Task PushUserToDatabase(User user)
        {
            await _supabaseService.InitializeAsync();

            await _supabaseService.Client
                .From<User>()
                .Insert(user);

            Console.WriteLine($"User {user.FirstName} {user.LastName} pushed to database!");
        }

        private async Task SendVerificationEmail(User user)
        {
            // Generate raw token (send to user) + store hashed version
            string rawToken = Guid.NewGuid().ToString();
            string tokenHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(rawToken));

            var token = new EmailVerificationToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            await _supabaseService.Client
                .From<EmailVerificationToken>()
                .Insert(token);

            var emailService = new EmailService();
            emailService.SendVerificationEmail(user.Email, rawToken);

            Console.WriteLine($"📧 Verification email sent to {user.Email}");
        }

        public void DisplayAllUsers()
        {
            if (_users.Count == 0)
            {
                Console.WriteLine("No users registered yet.");
                return;
            }

            foreach (var user in _users)
            {
                Console.WriteLine($"ID: {user.Id}, Name: {user.FirstName} {user.LastName}, Email: {user.Email}, Verified: {user.IsEmailVerified}, Role: {user.GetRole()}");
            }
        }
    }
}
