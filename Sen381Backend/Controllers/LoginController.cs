using Microsoft.AspNetCore.Mvc;
using Sen381.Business.Models;
using Sen381.Data_Access;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sen381Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly SupaBaseAuthService _supabase;

        public LoginController(SupaBaseAuthService supabase)
        {
            _supabase = supabase;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            Console.WriteLine($"[LOGIN ATTEMPT] Email='{model.Email}', Password='{model.Password}'");

            // Basic field validation
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { error = "Email and password are required." });

            // Email format validation
            if (!model.Email.Contains("@"))
                return BadRequest(new { error = "Invalid email format." });

            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // Hash entered password using SHA256
                using var sha = SHA256.Create();
                var hashedBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(model.Password));
                var hashedPassword = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();

                // Retrieve user by email
                var response = await client
                    .From<User>()
                    .Select("*")
                    .Where(u => u.Email == model.Email)
                    .Get();

                var user = response.Models.FirstOrDefault();
                if (user == null)
                {
                    Console.WriteLine($"[LOGIN FAILED] Email not found: {model.Email}");
                    return Unauthorized(new { error = "Email not found." });
                }

                // Compare hashed password with stored password
                if (!string.Equals(user.PasswordHash, hashedPassword, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"[LOGIN FAILED] Incorrect password for: {model.Email}");
                    return Unauthorized(new { error = "Incorrect password." });
                }

                // Check if email is verified
                if (!user.IsEmailVerified)
                {
                    Console.WriteLine($"[LOGIN FAILED] Unverified email: {model.Email}");
                    return Unauthorized(new { error = "Email not verified. Please verify your email before logging in." });
                }

                // Success
                Console.WriteLine($"[LOGIN SUCCESS] User '{model.Email}' logged in successfully.");
                return Ok(new
                {
                    message = "Login successful",
                    userId = user.Id,
                    email = user.Email,
                    role = user.RoleString
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOGIN ERROR] {ex.Message}");
                return StatusCode(500, new { error = $"Unexpected error: {ex.Message}" });
            }
        }
    }
}
