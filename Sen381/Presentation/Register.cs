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
                if (input == "1") { role = Role.Student; break; }
                if (input == "2") { role = Role.Admin; break; }
                Console.WriteLine("Invalid input. Please enter 1 or 2.");
            }

            // In practice you'd let the DB assign IDs, but we'll keep the random for now.
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
                IsEmailVerified = true,
                LastLogin = DateTime.UtcNow
            };
            user.SetRole(role);
            user.ChangePassword(password);

            _users.Add(user);

            await PushUserToDatabase(user);
        }

        private async Task PushUserToDatabase(User user)
        {
            await _supabaseService.InitializeAsync();

            // Table is inferred from [Table("users")] on User
            await _supabaseService.Client
                .From<User>()
                .Insert(user);

            Console.WriteLine($"User {user.FirstName} {user.LastName} pushed to database!");
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
                Console.WriteLine($"ID: {user.Id}, Name: {user.FirstName} {user.LastName}, Email: {user.Email}, Role: {user.GetRole()}");
            }
        }
    }
}
