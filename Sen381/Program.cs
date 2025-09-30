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

            var register = new Register(supa);

            // Register three users via console prompts
            await register.StartRegisterAsync();
            await register.StartRegisterAsync();
            await register.StartRegisterAsync();

            register.DisplayAllUsers();

            Console.ReadLine();
        }
    }
}
