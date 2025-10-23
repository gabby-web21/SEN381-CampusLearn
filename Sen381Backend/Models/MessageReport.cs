using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Sen381Backend.Models
{
    [Table("message_reports")]
    public class MessageReport : BaseModel
    {
        [PrimaryKey("id", true)]
        [Column("id", ignoreOnInsert: true)]
        public int Id { get; set; }
        
        [Column("reporter_id")]
        public int ReporterId { get; set; }
        
        [Column("reported_user_id")]
        public int ReportedUserId { get; set; }
        
        [Column("message_id")]
        public string MessageId { get; set; } = "";
        
        [Column("message_content")]
        public string MessageContent { get; set; } = "";
        
        [Column("message_type")]
        public string MessageType { get; set; } = ""; // "messages", "forum", "tutoring"
        
        [Column("reason")]
        public string Reason { get; set; } = ""; // "nsfw", "harassment", "foul_language", "spam", "other"
        
        [Column("details")]
        public string? Details { get; set; }
        
        [Column("created_at", ignoreOnInsert: true, ignoreOnUpdate: true)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Column("is_resolved")]
        public bool IsResolved { get; set; } = false;
        
        [Column("resolution")]
        public string? Resolution { get; set; } // "approved", "dismissed", "user_suspended"
        
        [Column("resolved_at")]
        public DateTime? ResolvedAt { get; set; }
        
        [Column("resolved_by")]
        public int? ResolvedBy { get; set; } // Admin who resolved it
        
        // Additional context
        [Column("context_url")]
        public string? ContextUrl { get; set; } // URL where the message was reported from
        [Column("sender_name")]
        public string? SenderName { get; set; } // Name of the person who sent the message
        [Column("reporter_name")]
        public string? ReporterName { get; set; } // Name of the person who reported
    }
}
