using CommandLine;
using CommandLine.Text;
using Microsoft.Owin.Hosting;
using System;
using OwinApp;


namespace MainConsole
{
    class OwinProgram
    {
        private static void MainOther(string[] args)
        {
            var url = "http://+:8000";

            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("Running on {0}", url);
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
            }
        }

        private static void Main()
        {
            var url = "http://+:8000";

            using (WebApp.Start<StartupBare>(url))
            {
                Console.WriteLine("Running on {0}", url);
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
            }
        }
    }
}
