using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Sen381.Business.Models
{
    //Mapping to Supabase table
    [Table("students")]
    public class StudentRecord:BaseModel
    {
        [PrimaryKey("user_id", false)]
        [Column("user_id")]
        public int UserId { get; set; }
        [Column("student_no")]
        public string StudentNo { get; set; }

        [Column("major")]
        public string Major { get; set; }

        [Column("year_of_study")]
        public int YearOfStudy { get; set; }

        [Column("notify_email")]
        public bool NotifyEmail { get; set; }

        [Column("notify_sms")]
        public bool NotifySms { get; set; }

        [Column("notify_whatsapp")]
        public bool NotifyWhatsapp { get; set; }

    }
}
