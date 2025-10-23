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
        private static readonly List<object> _reports = new List<object>(); // Store reports in memory
        private static int _nextReportId = 1;

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
                _logger.LogInformation($"Received report request from user {request.ReporterId} for message {request.MessageId}");

                // Validate request
                if (string.IsNullOrEmpty(request.MessageId) || string.IsNullOrEmpty(request.Reason))
                {
                    return BadRequest("MessageId and Reason are required");
                }

                // Store the report in memory
                var report = new {
                    Id = _nextReportId++,
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
                    Resolution = (string)null,
                    ResolvedAt = (DateTime?)null,
                    ResolvedBy = (int?)null,
                    ContextUrl = request.ContextUrl
                };

                _reports.Add(report);

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
                _logger.LogInformation($"GetReportsForAdmin called. Current report count: {_reports.Count}");
                
                // Return the actual stored reports
                var response = new {
                    message = "Reports retrieved successfully",
                    reports = _reports
                };
                
                _logger.LogInformation($"Returning {_reports.Count} reports to admin");
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
                _logger.LogInformation($"Admin {request.AdminId} attempted to resolve report {reportId} with resolution: {request.Resolution}");
                
                // Find and update the report
                var reportToUpdate = _reports.FirstOrDefault(r => ((dynamic)r).Id == reportId);
                if (reportToUpdate != null)
                {
                    // Create a new report object with updated resolution
                    var updatedReport = new {
                        Id = ((dynamic)reportToUpdate).Id,
                        ReporterId = ((dynamic)reportToUpdate).ReporterId,
                        ReportedUserId = ((dynamic)reportToUpdate).ReportedUserId,
                        MessageId = ((dynamic)reportToUpdate).MessageId,
                        MessageContent = ((dynamic)reportToUpdate).MessageContent,
                        MessageType = ((dynamic)reportToUpdate).MessageType,
                        Reason = ((dynamic)reportToUpdate).Reason,
                        Details = ((dynamic)reportToUpdate).Details,
                        ReporterName = ((dynamic)reportToUpdate).ReporterName,
                        SenderName = ((dynamic)reportToUpdate).SenderName,
                        CreatedAt = ((dynamic)reportToUpdate).CreatedAt,
                        IsResolved = true,
                        Resolution = request.Resolution,
                        ResolvedAt = DateTime.UtcNow,
                        ResolvedBy = request.AdminId,
                        ContextUrl = ((dynamic)reportToUpdate).ContextUrl
                    };

                    // Remove old report and add updated one
                    _reports.Remove(reportToUpdate);
                    _reports.Add(updatedReport);

                    _logger.LogWarning($"REPORT RESOLUTION - Admin {request.AdminId} resolved report {reportId} with resolution: {request.Resolution}");
                }
                
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
}
