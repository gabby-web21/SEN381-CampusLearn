using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sen381.Business.Models;
using Sen381.Data_Access;

namespace Sen381.Business.Services
{
    public class UserService
    {
        private readonly SupaBaseAuthService _supabaseService;

        public UserService(SupaBaseAuthService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        //Fetch user by ID
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            await _supabaseService.InitializeAsync();

            var response = await _supabaseService.Client
                .From<User>()
                .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId.ToString())
                .Single();

            return response;
        }

        //Update User Profile
        public async Task<bool> UpdateUserAsync(User user)
        {
            await _supabaseService.InitializeAsync();

            var response = await _supabaseService.Client
                .From<User>()
                .Update(user);

            return response.Models.Count > 0;
        }

    }
}
