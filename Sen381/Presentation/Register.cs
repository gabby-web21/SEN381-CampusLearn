using Sen381.Business.Models;
using Sen381.Data_Access;
using Supabase.Postgrest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sen381
{
    public class Register
    {
        private readonly List<User> _users = new();
        private readonly SupaBaseAuthService _supabaseService;
        private readonly HttpClient _httpClient;

        public Register(SupaBaseAuthService supabaseService)
        {
            _supabaseService = supabaseService;
            _httpClient = new HttpClient();
        }

        public async Task StartRegisterAsync(RegisterModel model)
        {
            try
            {
                // ✅ Validate input
                if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                {
                    Console.WriteLine("❌ Email and Password are required.");
                    return;
                }

                if (model.Password != model.ConfirmPassword)
                {
                    Console.WriteLine("❌ Passwords do not match.");
                    return;
                }

                await _supabaseService.InitializeAsync();

                // ✅ Check if email already exists
                var existingUsers = await _supabaseService.Client
                    .From<User>()
                    .Where(u => u.Email == model.Email)
                    .Get();

                if (existingUsers.Models.Any())
                {
                    Console.WriteLine("⚠️ A user with this email already exists.");
                    return;
                }

                // ✅ Create new user object
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNum = model.PhoneNum,
                    Email = model.Email,
                    ProfilePicturePath = "",
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow,
                    IsEmailVerified = false
                };

                user.SetRole(Role.student);
                user.ChangePassword(model.Password);

                _users.Add(user);

                // ✅ Insert user into Supabase
                var insertResponse = await _supabaseService.Client
                    .From<User>()
                    .Insert(user, new QueryOptions
                    {
                        Returning = QueryOptions.ReturnType.Representation
                    });

                var insertedUser = insertResponse.Models.FirstOrDefault();
                if (insertedUser == null)
                {
                    Console.WriteLine("❌ Failed to insert user into database.");
                    return;
                }

                Console.WriteLine($"✅ User '{insertedUser.Email}' registered with ID {insertedUser.Id}.");

                // ✅ Generate and store email verification token
                string rawToken = await CreateVerificationTokenAsync(insertedUser);

                // ✅ Send verification email using backend
                await SendVerificationEmailAsync(insertedUser.Email, rawToken);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Registration failed: {ex.Message}");
            }
        }

        // 🔑 Create & store email verification token
        private async Task<string> CreateVerificationTokenAsync(User user)
        {
            string rawToken = Guid.NewGuid().ToString();
            string tokenHash = HashToken(rawToken);

            var token = new EmailVerificationToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            var tokenResponse = await _supabaseService.Client
                .From<EmailVerificationToken>()
                .Insert(token, new QueryOptions
                {
                    Returning = QueryOptions.ReturnType.Representation
                });

            if (!tokenResponse.Models.Any())
                Console.WriteLine("❌ Failed to store verification token in database.");
            else
                Console.WriteLine($"🔑 Verification token stored for {user.Email}");

            return rawToken; // ✅ return this so we send the same token
        }

        // 📧 Send verification email via backend API
        private async Task SendVerificationEmailAsync(string email, string token)
        {
            try
            {
                var apiUrl = "https://localhost:7228/api/email/send-verification"; // backend endpoint
                var response = await _httpClient.PostAsJsonAsync(apiUrl, new
                {
                    Email = email,
                    Token = token
                });

                if (response.IsSuccessStatusCode)
                    Console.WriteLine($"📧 Verification email sent to {email} via backend");
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Failed to send verification email: {response.StatusCode} - {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error calling backend email API: {ex.Message}");
            }
        }

        // 🔒 Hash token before storing it in DB
        private string HashToken(string token)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        // 👀 Display all users (debug use)
        public void DisplayAllUsers()
        {
            if (_users.Count == 0)
            {
                Console.WriteLine("No users registered yet.");
                return;
            }

            foreach (var user in _users)
            {
                Console.WriteLine($"ID: {user.Id}, Name: {user.FirstName} {user.LastName}, " +
                                  $"Email: {user.Email}, Verified: {user.IsEmailVerified}, Role: {user.GetRole()}");
            }
        }
    }
}
