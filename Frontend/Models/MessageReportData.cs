namespace Frontend.Models
{
    public class MessageReportData
    {
        public string MessageId { get; set; } = "";
        public string MessageContent { get; set; } = "";
        public int ReportedUserId { get; set; }
        public string SenderName { get; set; } = "";
        public string MessageType { get; set; } = ""; // "messages", "forum", "tutoring"
        public string? ContextUrl { get; set; }
    }
}
