using System;
using System.Diagnostics;
using System.IO;
using CommandLine;
using CommandLine.Text;
using PgpCrypto;
using MsCrypto;
using SymmetricCrypto;

namespace MainConsole
{
    internal class EncryptProgram
    {
        /// <summary>
        /// Encrypted By Third Party
        /// -k myTestPublicKey.asc -s thirdpartyPrivatekey.asc -p mytestkey
        /// </summary>
        /// <param name="args"></param>
        //private static void Main(string[] args)
        //{
        //    var options = new Options();
        //    if (Parser.Default.ParseArguments(args, options))
        //    {
        //        var stopWatch = new Stopwatch();
        //        stopWatch.Start();
        //        var encryptionKeys = new PgpEncryptionKeys(
        //            options.PublicKeyFileName, options.PrivateKeyFileName, options.PrivateKeyPassword);

        //        var encrypter = new PgpEncrypt(encryptionKeys);

        //        using (Stream outputStream = File.Create("EncryptedFile.txt"))
        //        {
        //            encrypter.EncryptAndSign(outputStream, new FileInfo("ClearFile.txt"));
        //        }
        //        var file = new FileInfo("EncryptedFile.txt");

        //        stopWatch.Stop();
        //        Console.WriteLine("Completed encrypt and sign in {0}MS for file size {1}MB", stopWatch.ElapsedMilliseconds, (file.Length / 1000000));
        //    }
        //    else
        //    {
        //        Console.WriteLine("Exiting...bad arguments");
        //    }
        //}

        /// <summary>
        ///  Symmetric sample
        /// </summary>
        /// <param name="args"></param>
        //private static void Main(string[] args)
        //{
        //    var options = new Options();
        //    if (Parser.Default.ParseArguments(args, options))
        //    {
        //        var stopWatch = new Stopwatch();
        //        stopWatch.Start();
        //        var key = AESGCM.NewKey();
        //        var original = "Some message to encrypt.";
        //        Console.WriteLine(original);
        //        var encrypted = AESGCM.SimpleEncrypt(original, key);
        //        Console.WriteLine(encrypted);
        //        var decrypted = AESGCM.SimpleDecrypt(encrypted, key);
        //        Console.WriteLine(decrypted);
        //        stopWatch.Stop();
        //    }
        //    else
        //    {
        //        Console.WriteLine("Exiting...bad arguments");
        //    }
        //}

        /// <summary>
        /// Key container sample
        /// -k myTestPublicKey.asc -s myTestPrivateKey.asc -p mytestkey
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            var options = new Options();
            if (Parser.Default.ParseArguments(args, options))
            {
                var encryptionKeys = new PgpEncryptionKeys(
                    options.PublicKeyFileName, options.PrivateKeyFileName, options.PrivateKeyPassword);
                var sender = new KeyConverter(encryptionKeys);
                //sender.GetKeyContainer("CryptoWebContainer");
                var xml = sender.ConvertPemEncodedKeyToXml();
                File.WriteAllText("myTestPrivateKey1.xml", xml);
                Console.WriteLine(xml);
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