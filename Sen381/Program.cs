using Sen381.Data_Access;
using System;
using System.Threading.Tasks;

namespace Sen381
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var supa = new SupaBaseAuthService();
            bool clientOk = await supa.TestConnectionAsync();
            Console.WriteLine(clientOk ? "Connection succeeded" : "Connection failed");

            Console.ReadKey();
        }

            var login = new Login(supa);
            var register = new Register(supa);

            // Start login flow
            await login.StartLoginAsync();

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
        }
    }
}
