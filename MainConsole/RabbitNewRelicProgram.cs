using System;
using System.Diagnostics;


namespace MainConsole
{
    class RabbitNewRelicProgram
    {
        private static void Main()
        {
            var rubyExePath = @"C:\Ruby23-x64\bin\ruby.exe";
            var pivotalAgentPath = @"C:\Ruby_newrelic\newrelic_pivotal_agent-pivotal_agent-1.0.5\plugins\pivotal_rabbitmq_plugin\pivotal_rabbitmq_plugin.rb";
            var sslCertFilePath = @"C:\Ruby_newrelic\newrelic_pivotal_agent-pivotal_agent-1.0.5\cacert.pem";

            if (string.IsNullOrWhiteSpace(rubyExePath) ||
                string.IsNullOrWhiteSpace(pivotalAgentPath) ||
                string.IsNullOrWhiteSpace(sslCertFilePath))
                throw new Exception("AppSettings RubyExePath, PivotalAgentPath, and/or SslCertFilePath are missing.");

            // Start the Pivotal agent, keeping a reference to it, so it can be stopped easily.
            var process = Process.Start(rubyExePath, pivotalAgentPath);

            Console.WriteLine("New Relic Monitor running");
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();

            process?.Dispose();
        }
    }
}
