using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sen381.Data_Access;

namespace Sen381
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Supabase connection test...");

            var supabaseService = new SupaBaseAuthService();
            supabaseService.TestConnection();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
