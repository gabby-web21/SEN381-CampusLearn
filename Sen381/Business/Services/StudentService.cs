using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sen381.Business.Models;
using Sen381.Data_Access;

namespace Sen381.Business.Services
{
    public class StudentService
    {
        private readonly SupaBaseAuthService _supabaseService;

        public StudentService(SupaBaseAuthService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        public async Task<StudentRecord?> GetStudentByUserIdAsync(int userId)
        {
            await _supabaseService.InitializeAsync();

            var response = await _supabaseService.Client
                .From<StudentRecord>()
                .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId.ToString())
                .Single();

            return response;
        }
    }
}
