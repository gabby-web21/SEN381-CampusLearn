using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Npgsql;

namespace Sen381.Data_Access
{
    public class SupaBaseAuthService
    {
        private readonly string _connectionString;

        public SupaBaseAuthService()
        {
            _connectionString = ConfigurationManager
                .ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public void TestConnection()
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    Console.WriteLine("✅ Connected to Supabase!");

                    using (var cmd = new NpgsqlCommand("SELECT current_user, current_database()", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            Console.WriteLine($"{reader.GetString(0)} @ {reader.GetString(1)}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Connection failed: " + ex.Message);
            }
        }
    }
}
