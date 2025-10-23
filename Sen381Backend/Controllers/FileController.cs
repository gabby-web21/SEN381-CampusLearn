using Microsoft.AspNetCore.Mvc;
using Sen381Backend.Models;
using Sen381.Data_Access;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sen381Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly SupaBaseAuthService _supabase;

        public FileController(SupaBaseAuthService supabase)
        {
            _supabase = supabase;
        }

        [HttpPost("uploadfile")]
        public async Task<IActionResult> UploadFile([FromForm] FileInput input)
        {
            if (input.File == null)
                return BadRequest("No file provided.");

            await _supabase.InitializeAsync();
            var client = _supabase.Client;

            using var ms = new MemoryStream();
            await input.File.CopyToAsync(ms);
            var fileBytes = ms.ToArray();

            try
            {
                var bucket = client.Storage.From("User_Uploads");
                var originalName = string.IsNullOrWhiteSpace(input.Name)
                    ? input.File.FileName
                    : input.Name;

                var fileName = $"{Guid.NewGuid()}_{originalName}";

                // ✅ Upload file to private bucket
                await bucket.Upload(fileBytes, fileName);

                // ✅ Generate signed URL (valid for 7 days)
                var signedUrl = await bucket.CreateSignedUrl(fileName, 604800);

                // ✅ Save metadata in DB
                var uploadedFile = new UploadedFile
                {
                    UploaderUserId = long.TryParse(User?.FindFirst("sub")?.Value, out var uid) ? uid : 0,
                    FileName = fileName,
                    FileType = input.File.ContentType,
                    SizeBytes = input.File.Length,
                    StorageLocation = signedUrl,
                    CreatedAt = DateTime.UtcNow
                };

                var dbResponse = await client.From<UploadedFile>().Insert(uploadedFile);
                var inserted = dbResponse.Models.FirstOrDefault();

                if (inserted == null)
                {
                    Console.WriteLine("⚠️ File inserted but no record returned from Supabase.");
                    return Ok(new
                    {
                        FileId = 0,
                        FileName = fileName,
                        FileType = input.File.ContentType,
                        SignedUrl = signedUrl
                    });
                }

                Console.WriteLine($"✅ Uploaded file '{fileName}' (FileId={inserted.FileId}) successfully.");
                return Ok(new
                {
                    inserted.FileId,
                    inserted.FileName,
                    inserted.FileType,
                    inserted.SizeBytes,
                    SignedUrl = signedUrl,
                    inserted.CreatedAt
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [FileController] Upload error: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("upload-transcript")]
        public async Task<IActionResult> UploadTranscript([FromForm] FileInput input)
        {
            if (input.File == null)
                return BadRequest("No file provided.");

            var allowedTypes = new[] { "application/pdf" };
            var allowedExtensions = new[] { ".pdf" };

            if (!allowedTypes.Contains(input.File.ContentType) &&
                !allowedExtensions.Any(ext =>
                    input.File.FileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                return BadRequest("Only PDF files are allowed for transcripts.");

            if (input.File.Length > 10 * 1024 * 1024)
                return BadRequest("File size cannot exceed 10 MB.");

            await _supabase.InitializeAsync();
            var client = _supabase.Client;

            using var ms = new MemoryStream();
            await input.File.CopyToAsync(ms);
            var fileBytes = ms.ToArray();

            try
            {
                var bucket = client.Storage.From("Transcripts");
                var originalName = string.IsNullOrWhiteSpace(input.Name)
                    ? input.File.FileName
                    : input.Name;
                var fileName = $"transcript_{Guid.NewGuid()}_{originalName}";

                await bucket.Upload(fileBytes, fileName);

                // ✅ Use signed URL for secure transcript access
                var signedUrl = await bucket.CreateSignedUrl(fileName, 604800);

                Console.WriteLine($"✅ Uploaded transcript '{fileName}' successfully.");
                return Ok(new { FileName = fileName, SignedUrl = signedUrl });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [FileController] Transcript upload error: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("get-transcript-url")]
        public async Task<IActionResult> GetTranscriptUrl([FromQuery] string filePath)
        {
            Console.WriteLine($"🔍 [FileController] GetTranscriptUrl called with filePath: '{filePath}'");
            
            if (string.IsNullOrEmpty(filePath))
            {
                Console.WriteLine("❌ [FileController] File path is null or empty");
                return BadRequest("File path is required.");
            }

            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;
                var bucket = client.Storage.From("Transcripts");

                Console.WriteLine($"🔍 [FileController] Attempting to create signed URL for: {filePath}");
                var signedUrl = await bucket.CreateSignedUrl(filePath, 604800);
                
                if (string.IsNullOrEmpty(signedUrl))
                {
                    Console.WriteLine($"❌ [FileController] Failed to generate signed URL for: {filePath}");
                    return NotFound($"File not found: {filePath}");
                }
                
                // Modify the signed URL to force inline viewing instead of download
                if (signedUrl.Contains("?"))
                {
                    signedUrl += "&inline=true&download=false";
                }
                else
                {
                    signedUrl += "?inline=true&download=false";
                }
                
                Console.WriteLine($"✅ [FileController] Generated signed URL for transcript: {filePath}");
                Console.WriteLine($"🔍 [FileController] Modified signed URL: {signedUrl}");
                return Ok(new { signedUrl });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [FileController] Error generating transcript URL: {ex.Message}");
                Console.WriteLine($"❌ [FileController] Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("view/{filePath}")]
        public async Task<IActionResult> ViewTranscript(string filePath)
        {
            Console.WriteLine($"🔍 [FileController] ViewTranscript called with filePath: '{filePath}'");
            
            if (string.IsNullOrEmpty(filePath))
            {
                Console.WriteLine("❌ [FileController] File path is null or empty");
                return BadRequest("File path is required.");
            }

            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;
                var bucket = client.Storage.From("Transcripts");

                Console.WriteLine($"🔍 [FileController] Creating signed URL for: {filePath}");
                
                // Create signed URL
                var signedUrl = await bucket.CreateSignedUrl(filePath, 604800);
                
                if (string.IsNullOrEmpty(signedUrl))
                {
                    Console.WriteLine($"❌ [FileController] Failed to generate signed URL for: {filePath}");
                    return NotFound($"File not found: {filePath}");
                }
                
                // Add inline parameter to force inline viewing
                if (signedUrl.Contains("?"))
                {
                    signedUrl += "&inline=true&download=false";
                }
                else
                {
                    signedUrl += "?inline=true&download=false";
                }
                
                Console.WriteLine($"✅ [FileController] Redirecting to signed URL: {signedUrl}");
                
                // Redirect to the signed URL
                return Redirect(signedUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [FileController] Error viewing transcript: {ex.Message}");
                Console.WriteLine($"❌ [FileController] Stack trace: {ex.StackTrace}");
                return StatusCode(500, $"Error viewing transcript: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFiles()
        {
            await _supabase.InitializeAsync();
            var client = _supabase.Client;
            var response = await client.From<UploadedFile>().Get();
            return Ok(response.Models);
        }
    }
}
