using System;
using System.IO;

namespace MainConsole
{
    public class FileWatcherProgram
    {
        private static void Main(string[] args)
        {
            var watcher = new FileSystemWatcher
            {
                Path = @"C:\LoyaltyFTP\LocalUser\9618\Upload",
                NotifyFilter = NotifyFilters.FileName,
                Filter = "*.*",
                EnableRaisingEvents = true
            };

            watcher.Created += OnChanged;
            
            Console.WriteLine("Watching: {0}", watcher.Path);
            Console.WriteLine("Press enter to exit file watcher");
            Console.ReadLine();
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            var s = File.ReadAllText(e.FullPath);

            Console.WriteLine("==Fired==");
            Console.WriteLine(e.ChangeType);
            Console.WriteLine(e.FullPath);
            Console.WriteLine(e.Name);
            Console.WriteLine("");

            //Check file is usuable, not being used by another process.
        }

    }
}
