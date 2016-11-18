using System;
using System.IO;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;

namespace MainConsole
{
    public class ReferenceData
    {
        public void Script(Server srv, string directoryToSaveTo, string[] referenceTableList, string dbName,
            string schema)
        {
            var scripter = new Scripter(srv);
            scripter.Options.ScriptSchema = false;
            scripter.Options.ScriptData = true;
            scripter.Options.NoCommandTerminator = true;
            scripter.Options.ToFileOnly = true;
            var serverUrn = srv.Urn;
            var urnCollection = new UrnCollection();

            var directory = Directory.CreateDirectory($@"{directoryToSaveTo}\ReferenceData");

            foreach (var table in referenceTableList)
            {
                scripter.Options.FileName = $@"{directory.FullName}\{table}.sql";
                urnCollection.Add(
                    new Urn($@"{serverUrn}/Database[@Name='{dbName}']/Table[@Name='{table}' and @Schema='{schema}']"));
                Console.WriteLine($"{scripter.Options.FileName} ");
            }

            scripter.EnumScript(urnCollection);
        }
    }
}
