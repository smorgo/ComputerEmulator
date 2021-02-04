using System;
using System.Threading.Tasks;

namespace Repl
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting the ComputerEmulator REPL environment");

            var host = new ReplHost();
            await host.Initialise();
            await host.Run();

            Console.WriteLine("REPL terminated");
        }
    }
}
