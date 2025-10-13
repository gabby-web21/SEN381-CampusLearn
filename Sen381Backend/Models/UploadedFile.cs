using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Sen381Backend.Models
{
    [Table("uploaded_files")] // <-- must match your Supabase table name
    public class UploadedFile : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("file_name")]
        public string FileName { get; set; }

        [Column("storage_url")]
        public string StorageUrl { get; set; }

        [Column("uploader_id")]
        public string UploaderId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
