using Microsoft.Extensions.Configuration;
using Supabase;
using System;
using System.Threading.Tasks;
using System.IO;
namespace Sen381.Data_Access
{
    public class SupaBaseAuthService
    {
        private readonly Client _client;
        public Client Client => _client;
        private bool _initialized;

        private static string SafeAppSetting(string key)
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                return config[key];
            }
            catch(Exception ex) 
            {
                Console.WriteLine($"[CONFIG ERROR] {ex.Message}");
                return null;
            }
        }

        public SupaBaseAuthService(string url = null, string anonKey = null)
        {
            // 1️⃣ Try environment variables first
            var envUrl = Environment.GetEnvironmentVariable("SUPABASE_URL");
            var envKey = Environment.GetEnvironmentVariable("SUPABASE_KEY");

            // 2️⃣ Fallback to appsettings.json if missing
            if (string.IsNullOrWhiteSpace(envUrl) || string.IsNullOrWhiteSpace(envKey))
            {
                envUrl = SafeAppSetting("Supabase:Url");
                envKey = SafeAppSetting("Supabase:Key");

                Console.WriteLine("[CONFIG] Loaded from appsettings.json");
            }
            else
            {
                Console.WriteLine("[CONFIG] Loaded from environment variables");
            }

            // 3️⃣ Use whichever values are available
            url ??= envUrl;
            anonKey ??= envKey;

            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(anonKey))
                throw new InvalidOperationException("Supabase URL or anon key is missing.");

            Console.WriteLine($"[CONFIG] Final Supabase URL: {url}");
            Console.WriteLine($"[CONFIG] Final Supabase Key (first 10 chars): {anonKey?.Substring(0, Math.Min(anonKey.Length, 10))}...");

            // 4️⃣ Initialize client
            var options = new Supabase.SupabaseOptions
            {
                AutoConnectRealtime = true,
                AutoRefreshToken = true
            };

            _client = new Supabase.Client(url, anonKey, options);
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
