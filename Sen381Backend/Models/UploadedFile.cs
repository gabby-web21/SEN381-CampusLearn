using Microsoft.Extensions.Configuration;
using Supabase;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
namespace Sen381Backend.Models
{

    public class UploadedFile
    {
        
        public string FileName { get; set; }
        public string StorageUrl { get; set; }
        public string UploaderId { get; set; }
        public DateTime CreatedAt { get; set; }
        

    }
}
