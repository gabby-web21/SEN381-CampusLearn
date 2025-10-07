using System;
using System.Configuration;
using System.Threading.Tasks;
using Supabase;


namespace Sen381.Data_Access
{
    public class SupaBaseAuthService
    {
        private readonly Client _client;
        public Client Client => _client;
        private bool _initialized;

        private static string SafeAppSetting(string key)
        {
            try { return ConfigurationManager.AppSettings[key]; }
            catch { return null; }
        }

        public SupaBaseAuthService(string url = null, string anonKey = null)
        {
            var fromEnvUrl = Environment.GetEnvironmentVariable("SUPABASE_URL");
            var fromEnvKey = Environment.GetEnvironmentVariable("SUPABASE_KEY");

            url ??= fromEnvUrl ?? SafeAppSetting("SUPABASE_URL");
            anonKey ??= fromEnvKey ?? SafeAppSetting("SUPABASE_KEY");

            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(anonKey))
                throw new InvalidOperationException("Supabase URL or anon key is missing.");

            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = false,
                Schema = "public" // ⬅️ KEY LINE: target the public schema
            };

            _client = new Client(url, anonKey, options);
        }

        public async Task InitializeAsync()
        {
            if (_initialized) return;
            await _client.InitializeAsync();
            _initialized = true;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await InitializeAsync();
                Console.WriteLine("✅ Supabase client initialized and reachable.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Connection failed: {ex.Message}");
                return false;
            }
        }
    }
}
