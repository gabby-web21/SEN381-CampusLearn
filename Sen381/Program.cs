using Sen381.Data_Access;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

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

    }
}
