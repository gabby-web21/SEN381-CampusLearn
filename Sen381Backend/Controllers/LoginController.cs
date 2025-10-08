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
    public class LoginController:ControllerBase
    {
        private readonly SupaBaseAuthService _supabase;

        public LoginController(SupaBaseAuthService supabase)
        {
            _supabase = supabase;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            Console.WriteLine($"Incoming login: Email='{model.Email}', Password='{model.Password}'");
      
            //Basic field validation
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { error = "Email and password are required." });

            //Email format validation
            if (!model.Email.Contains("@"))
                return BadRequest(new { error = "Invalid email format." });

            await _supabase.InitializeAsync();
            var client = _supabase.Client;

            //Hash entered password
            using var sha = SHA256.Create();
            var hashedBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(model.Password));
            var hashedPassword = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();

            //Try to find user
            var response = await client
                .From<User>()
                .Select("*")
                .Where(u => u.Email == model.Email)
                .Get();

            //Check email
            var user = response.Models.FirstOrDefault();
            if (user == null)
                return Unauthorized(new { error = "Email not found." });

            //Check password
            if (!user.IsEmailVerified)
                return Unauthorized(new { error = "Incorrect password." });

            //Check email verification
            if (!user.IsEmailVerified)
                return Unauthorized(new { error = "Email not verified." });

            return Ok(new
            {
                message = "Login successful",
                userId = user.Id,
                email = user.Email,
                role = user.RoleString
            });
        }
    }
}
