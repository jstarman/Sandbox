using System;
using System.Diagnostics;
using System.IO;
using CommandLine;
using CommandLine.Text;
using PgpCrypto;

namespace MainConsole
{
    class DecryptProgram
    {
        /// <summary>
        /// Decrypted by us
        /// -k thirdPartyPublicKey.asc -s myTestPrivatekey.asc -p mytestkey
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            var options = new Options();
            if (Parser.Default.ParseArguments(args, options))
            {
                var cacheTester = new CachingTester();
                cacheTester.AddCache();
            }
            else
            {
                Console.WriteLine("Exiting...bad arguments");
            }
        }

        private class Options
        {
            [Option('k', "publicKey", Required = false, HelpText = "Public key file name")]
            public string PublicKeyFileName { get; set; }

            [Option('s', "secretkey", Required = false, HelpText = "Private key file name")]
            public string PrivateKeyFileName { get; set; }

            [Option('p', "password", Required = false, HelpText = "Private key password")]
            public string PrivateKeyPassword { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
            }
        }
    }
}
