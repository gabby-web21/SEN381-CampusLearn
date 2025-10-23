using Microsoft.AspNetCore.Mvc;
using Sen381Backend.Models;
using Sen381.Business.Models;
using Sen381.Data_Access;
using Supabase;
using Supabase.Postgrest;
using System.Security.Cryptography;
using System.Text;

namespace Sen381Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageReportController : ControllerBase
    {
        private readonly SupaBaseAuthService _supabaseService;
        private readonly ILogger<MessageReportController> _logger;

        public MessageReportController(SupaBaseAuthService supabaseService, ILogger<MessageReportController> logger)
        {
            _supabaseService = supabaseService;
            _logger = logger;
        }

        [HttpPost("report")]
        public async Task<IActionResult> ReportMessage([FromBody] MessageReportRequest request)
        {
            try
            {
                await _supabaseService.InitializeAsync();
                var client = _supabaseService.Client;

                _logger.LogInformation($"Received report request from user {request.ReporterId} for message {request.MessageId}");

                // Validate request
                if (string.IsNullOrEmpty(request.MessageId) || string.IsNullOrEmpty(request.Reason))
                {
                    return BadRequest("MessageId and Reason are required");
                }

                // Create report in database
                var report = new MessageReport
                {
                    ReporterId = request.ReporterId,
                    ReportedUserId = request.ReportedUserId,
                    MessageId = request.MessageId,
                    MessageContent = request.MessageContent,
                    MessageType = request.MessageType,
                    Reason = request.Reason,
                    Details = request.Details,
                    ReporterName = request.ReporterName,
                    SenderName = request.SenderName,
                    CreatedAt = DateTime.UtcNow,
                    IsResolved = false,
                    ContextUrl = request.ContextUrl
                };

                var response = await client.From<MessageReport>().Insert(report);

                // Log the report
                _logger.LogWarning($"MESSAGE REPORT - Reporter: {request.ReporterName} (ID: {request.ReporterId}) reported user: {request.SenderName} (ID: {request.ReportedUserId}) for message: '{request.MessageContent}' in {request.MessageType}. Reason: {request.Reason}. Details: {request.Details}");

                // Return success response
                return Ok(new { 
                    success = true, 
                    message = "Report received and stored.",
                    reportId = report.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating message report");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("admin")]
        public async Task<IActionResult> GetReportsForAdmin()
        {
            try
            {
                await _supabaseService.InitializeAsync();
                var client = _supabaseService.Client;

                _logger.LogInformation("GetReportsForAdmin called - fetching from database");
                
                // Get reports from database
                var reportsResponse = await client
                    .From<MessageReport>()
                    .Select("*")
                    .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
                    .Get();

                var reports = reportsResponse.Models.Select(r => new {
                    Id = r.Id,
                    ReporterId = r.ReporterId,
                    ReportedUserId = r.ReportedUserId,
                    MessageId = r.MessageId,
                    MessageContent = r.MessageContent,
                    MessageType = r.MessageType,
                    Reason = r.Reason,
                    Details = r.Details,
                    ReporterName = r.ReporterName,
                    SenderName = r.SenderName,
                    CreatedAt = r.CreatedAt,
                    IsResolved = r.IsResolved,
                    Resolution = r.Resolution,
                    ResolvedAt = r.ResolvedAt,
                    ResolvedBy = r.ResolvedBy,
                    ContextUrl = r.ContextUrl
                }).ToList();
                
                var response = new {
                    message = "Reports retrieved successfully",
                    reports = reports
                };
                
                _logger.LogInformation($"Returning {reports.Count} reports to admin");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching reports for admin");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("resolve/{reportId}")]
        public async Task<IActionResult> ResolveReport(int reportId, [FromBody] ResolveReportRequest request)
        {
            try
            {
                await _supabaseService.InitializeAsync();
                var client = _supabaseService.Client;

                _logger.LogInformation($"Admin {request.AdminId} attempted to resolve report {reportId} with resolution: {request.Resolution}");
                
                // Get the report from database
                var reportResponse = await client
                    .From<MessageReport>()
                    .Select("*")
                    .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, reportId)
                    .Get();

                var report = reportResponse.Models.FirstOrDefault();
                if (report == null)
                {
                    return NotFound(new { error = "Report not found" });
                }

                // Update the report
                report.IsResolved = true;
                report.Resolution = request.Resolution;
                report.ResolvedAt = DateTime.UtcNow;
                report.ResolvedBy = request.AdminId;

                await client.From<MessageReport>().Update(report);

                _logger.LogWarning($"REPORT RESOLUTION - Admin {request.AdminId} resolved report {reportId} with resolution: {request.Resolution}");
                
                return Ok(new { 
                    success = true, 
                    message = "Report resolved successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving report");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("cancel/{reportId}")]
        public async Task<IActionResult> CancelReport(int reportId, [FromBody] CancelReportRequest request)
        {
            try
            {
                await _supabaseService.InitializeAsync();
                var client = _supabaseService.Client;

                _logger.LogInformation($"Admin {request.AdminId} cancelled report {reportId}");
                
                // Delete the report from database
                await client
                    .From<MessageReport>()
                    .Where(r => r.Id == reportId)
                    .Delete();

                _logger.LogWarning($"REPORT CANCELLED - Admin {request.AdminId} cancelled report {reportId}");
                
                return Ok(new { 
                    success = true, 
                    message = "Report cancelled successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling report");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("ban-user")]
        public async Task<IActionResult> BanUser([FromBody] BanUserRequest request)
        {
            try
            {
                await _supabaseService.InitializeAsync();
                var client = _supabaseService.Client;

                _logger.LogInformation($"Admin {request.AdminId} attempting to ban user {request.UserId}");

                // Get the user to ban
                var userResponse = await client
                    .From<User>()
                    .Select("*")
                    .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, request.UserId)
                    .Get();

                var user = userResponse.Models.FirstOrDefault();
                if (user == null)
                {
                    return NotFound(new { error = "User not found" });
                }

                // Update user ban status
                user.IsBanned = true;
                user.BannedAt = DateTime.UtcNow;
                user.BannedBy = request.AdminId;
                user.BanReason = request.Reason;

                await client.From<User>().Update(user);

                // Delete all reports related to this user from the database
                _logger.LogInformation($"Looking for reports to delete for user {request.UserId}");
                var reportsToDelete = await client
                    .From<MessageReport>()
                    .Select("*")
                    .Filter("reported_user_id", Supabase.Postgrest.Constants.Operator.Equals, request.UserId)
                    .Get();

                _logger.LogInformation($"Found {reportsToDelete.Models.Count} reports to delete for user {request.UserId}");

                var deletedCount = 0;
                foreach (var report in reportsToDelete.Models)
                {
                    try
                    {
                        _logger.LogInformation($"Attempting to delete report {report.Id} for banned user {request.UserId}");
                        await client
                            .From<MessageReport>()
                            .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, report.Id)
                            .Delete();
                        deletedCount++;
                        _logger.LogInformation($"Successfully deleted report {report.Id} for banned user {request.UserId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to delete report {report.Id}: {ex.Message}");
                    }
                }

                _logger.LogWarning($"USER BANNED - Admin {request.AdminId} banned user {request.UserId} ({user.FirstName} {user.LastName}) for reason: {request.Reason}. Deleted {deletedCount} related reports.");

                return Ok(new { 
                    success = true, 
                    message = $"User {user.FirstName} {user.LastName} has been banned successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error banning user");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("unban-user")]
        public async Task<IActionResult> UnbanUser([FromBody] UnbanUserRequest request)
        {
            try
            {
                await _supabaseService.InitializeAsync();
                var client = _supabaseService.Client;

                _logger.LogInformation($"Admin {request.AdminId} attempting to unban user {request.UserId}");

                // Get the user to unban
                var userResponse = await client
                    .From<User>()
                    .Select("*")
                    .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, request.UserId)
                    .Get();

                var user = userResponse.Models.FirstOrDefault();
                if (user == null)
                {
                    return NotFound(new { error = "User not found" });
                }

                // Update user ban status
                user.IsBanned = false;
                user.BannedAt = null;
                user.BannedBy = null;
                user.BanReason = null;

                await client.From<User>().Update(user);

                _logger.LogWarning($"USER UNBANNED - Admin {request.AdminId} unbanned user {request.UserId} ({user.FirstName} {user.LastName})");

                return Ok(new { 
                    success = true, 
                    message = $"User {user.FirstName} {user.LastName} has been unbanned successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unbanning user");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("banned-users")]
        public async Task<IActionResult> GetBannedUsers()
        {
            try
            {
                Console.WriteLine("🔍 [DEBUG] Starting GetBannedUsers API call");
                
                await _supabaseService.InitializeAsync();
                var client = _supabaseService.Client;

                Console.WriteLine("🔍 [DEBUG] Supabase client initialized");

                // Get all banned users
                Console.WriteLine("🔍 [DEBUG] Querying database for banned users...");
                var bannedUsersResponse = await client
                    .From<User>()
                    .Select("*")
                    .Where(u => u.IsBanned == true)
                    .Get();

                Console.WriteLine($"🔍 [DEBUG] Raw response from database: {bannedUsersResponse.Models.Count} users found");
                
                // Debug: Log each user found
                foreach (var user in bannedUsersResponse.Models)
                {
                    Console.WriteLine($"🔍 [DEBUG] User found - ID: {user.Id}, Name: {user.FirstName} {user.LastName}, Email: {user.Email}, IsBanned: {user.IsBanned}");
                }

                var bannedUsers = bannedUsersResponse.Models.Select(user => new {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    BannedAt = user.BannedAt,
                    BannedBy = user.BannedBy,
                    BanReason = user.BanReason
                }).ToList();

                Console.WriteLine($"🔍 [DEBUG] Processed {bannedUsers.Count} banned users for response");
                
                // Debug: Log the final response
                var response = new { 
                    success = true,
                    bannedUsers = bannedUsers
                };
                
                Console.WriteLine($"🔍 [DEBUG] Final response: {System.Text.Json.JsonSerializer.Serialize(response)}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [DEBUG] Error in GetBannedUsers: {ex.Message}");
                Console.WriteLine($"❌ [DEBUG] Stack trace: {ex.StackTrace}");
                _logger.LogError(ex, "Error fetching banned users");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }
    }

    public class MessageReportRequest
    {
        public int ReporterId { get; set; }
        public int ReportedUserId { get; set; }
        public string MessageId { get; set; } = "";
        public string MessageContent { get; set; } = "";
        public string MessageType { get; set; } = "";
        public string Reason { get; set; } = "";
        public string? Details { get; set; }
        public string? ContextUrl { get; set; }
        public string? SenderName { get; set; }
        public string? ReporterName { get; set; }
    }

    public class ResolveReportRequest
    {
        public string Resolution { get; set; } = ""; // "approved", "dismissed", "user_suspended"
        public int AdminId { get; set; }
    }

    public class CancelReportRequest
    {
        public int AdminId { get; set; }
    }

    public class BanUserRequest
    {
        public int UserId { get; set; }
        public int AdminId { get; set; }
        public string Reason { get; set; } = "";
    }

    public class UnbanUserRequest
    {
        public int UserId { get; set; }
        public int AdminId { get; set; }
    }
}
