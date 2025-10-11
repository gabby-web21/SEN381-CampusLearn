using Microsoft.AspNetCore.Mvc;
using Sen381.Business.Models;
using Sen381.Data_Access;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Reflection;

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
            Console.WriteLine($"[LOGIN ATTEMPT] Email='{model.Email}', Password='******'");

            // Basic validation
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { error = "Email and password are required." });
            if (!model.Email.Contains("@"))
                return BadRequest(new { error = "Invalid email format." });

            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // Hash entered password
                using var sha = SHA256.Create();
                var hashedBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(model.Password));
                var hashedPassword = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();

                // Get user from your table
                var response = await client
                    .From<Sen381.Business.Models.User>()
                    .Select("*")
                    .Where(u => u.Email == model.Email)
                    .Get();

                var user = response.Models.FirstOrDefault();
                if (user == null)
                {
                    Console.WriteLine($"[LOGIN FAILED] Email not found: {model.Email}");
                    return Unauthorized(new { error = "Email not found." });
                }

                if (!string.Equals(user.PasswordHash, hashedPassword, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"[LOGIN FAILED] Incorrect password for: {model.Email}");
                    return Unauthorized(new { error = "Incorrect password." });
                }

                if (!user.IsEmailVerified)
                {
                    Console.WriteLine($"[LOGIN FAILED] Unverified email: {model.Email}");
                    return Unauthorized(new { error = "Email not verified. Please verify your email before logging in." });
                }

                // Try to create a Supabase Auth session across SDK versions
                string? accessToken = null;
                string? refreshToken = null;

                try
                {
                    var auth = client.Auth;
                    object? signInCallResult = null;

                    // 1) Try SignInWithPassword(string email, string password)
                    var m1 = auth.GetType().GetMethod("SignInWithPassword", new[] { typeof(string), typeof(string) });
                    if (m1 != null)
                    {
                        signInCallResult = m1.Invoke(auth, new object?[] { model.Email, model.Password });
                    }
                    else
                    {
                        // 2) Try SignIn(string email, string password)
                        var m2 = auth.GetType().GetMethod("SignIn", new[] { typeof(string), typeof(string) });
                        if (m2 != null)
                        {
                            signInCallResult = m2.Invoke(auth, new object?[] { model.Email, model.Password });
                        }
                        else
                        {
                            // 3) Try SignInWithPassword(options) using reflection-created options object (if type exists)
                            var optionsType = AppDomain.CurrentDomain
                                .GetAssemblies()
                                .SelectMany(a => a.GetTypes())
                                .FirstOrDefault(t => t.FullName == "Supabase.Gotrue.SignInWithPasswordOptions");

                            var m3 = auth.GetType().GetMethod("SignInWithPassword", new[] { optionsType! });
                            if (optionsType != null && m3 != null)
                            {
                                var opts = Activator.CreateInstance(optionsType);
                                optionsType.GetProperty("Email")?.SetValue(opts, model.Email);
                                optionsType.GetProperty("Password")?.SetValue(opts, model.Password);
                                signInCallResult = m3.Invoke(auth, new object?[] { opts! });
                            }
                        }
                    }

                    // Await Task result if needed and extract tokens
                    if (signInCallResult is Task t)
                    {
                        await t;
                        var resultProp = t.GetType().GetProperty("Result");
                        var resultObj = resultProp?.GetValue(t);

                        // Try .Session property first
                        var sessionProp = resultObj?.GetType().GetProperty("Session");
                        var sessionObj = sessionProp?.GetValue(resultObj);

                        if (sessionObj == null)
                        {
                            // Sometimes the Task<T> is directly Session
                            sessionObj = resultObj;
                        }

                        if (sessionObj != null)
                        {
                            accessToken = sessionObj.GetType().GetProperty("AccessToken")?.GetValue(sessionObj)?.ToString();
                            refreshToken = sessionObj.GetType().GetProperty("RefreshToken")?.GetValue(sessionObj)?.ToString();
                        }
                    }
                    else if (signInCallResult != null)
                    {
                        // Non-task (unlikely), try read tokens directly
                        var sessionProp = signInCallResult.GetType().GetProperty("Session");
                        var sessionObj = sessionProp?.GetValue(signInCallResult) ?? signInCallResult;

                        accessToken = sessionObj?.GetType().GetProperty("AccessToken")?.GetValue(sessionObj)?.ToString();
                        refreshToken = sessionObj?.GetType().GetProperty("RefreshToken")?.GetValue(sessionObj)?.ToString();
                    }
                }
                catch (Exception authEx)
                {
                    Console.WriteLine($"[AUTH WARNING] Could not create Supabase session: {authEx.Message}");
                }

                Console.WriteLine($"[LOGIN SUCCESS] User '{model.Email}' logged in successfully.");

                return Ok(new
                {
                    message = "Login successful",
                    userId = user.Id,
                    email = user.Email,
                    role = user.RoleString,
                    accessToken,
                    refreshToken
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
