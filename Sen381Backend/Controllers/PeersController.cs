using Microsoft.AspNetCore.Mvc;
using Sen381.Business.Models;
using Sen381.Business.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sen381Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PeersController:ControllerBase
    {
        private readonly UserService _userService;

        public PeersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchPeers([FromQuery] string? query= " ")
        {
            try
            {
                // ✅ Get all users from Supabase
                var users = await _userService.GetAllUsersAsync();

                // ✅ Filter by name if query provided
                if (!string.IsNullOrWhiteSpace(query))
                {
                    users = users.Where(u =>
                        u.FirstName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        u.LastName.Contains(query, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                // ✅ Map only what’s needed for the peers page
                var peers = users.Select(u => new
                {
                    UserId = u.Id,
                    u.FirstName,
                    u.LastName,
                    Role = u.RoleString?.ToLower() ?? "student"
                });

                return Ok(peers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in SearchPeers: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
