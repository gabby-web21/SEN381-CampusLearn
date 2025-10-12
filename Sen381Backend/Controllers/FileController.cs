using Microsoft.AspNetCore.Mvc;
using Sen381.Data_Access;
using Sen381Backend.Models;
using Supabase;
using Supabase.Postgrest;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Sen381Backend.Controllers
{
    [ApiController]
    [Route("api/[Controller")]

    public class FileController: ControllerBase
    {
        public async Task<IActionResult> UploadFile([FromForm] FileInput input, [FromServices] Supabase.Client client)
        {
            using var ms = new MemoryStream();
            await input.File.CopyToAsync(ms);
            var fileBytes = ms.ToArray();

            var bucket = client.Storage.From("User_Uploads");
            var fileName = $"{Guid.NewGuid()}_{input.File.FileName}";
            await bucket.Upload(fileBytes, fileName);

            var urlResult = await bucket.CreateSignedUrl(fileName, 300);

            

            // Check both Url and SignedUrl
            var signedUrl = urlResult;


            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("user_id")?.Value;

            // Construct your metadata object
            var uploadedFile = new UploadedFile
            {
                FileName = fileName,
                StorageUrl = signedUrl, // or public URL if not signed
                UploaderId = userId,    // grab from auth/session context
                CreatedAt = DateTime.UtcNow
            };

            // Insert using Supabase client (example assumes your table is "uploaded_files")
            await client.Table<UploadedFile>("uploaded_files").Insert(uploadedFile);


            return Ok(new { SignedUrl = signedUrl });
        }
    }
    


}
