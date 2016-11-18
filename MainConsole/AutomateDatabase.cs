using Microsoft.SqlServer.Management.Smo;

namespace MainConsole
{
    public class AutomateDatabase
    {
        
        private static void Main(string[] args)
        {
            var schema = "Orders";
            var dbName = "GDB"; 
            var directoryToSaveTo = $@"C:\code\swag\DatabaseAutomation\{dbName}\{schema}";
            var serverName = "POR1100031XVS52";
            var referenceTableList = new[] {"OrderStatuses"};

            Server srv = new Server(serverName);
            Database db = srv.Databases[dbName];
             
            new DbObjects().Script(srv, directoryToSaveTo, schema, db);
            new ReferenceData().Script(srv, directoryToSaveTo, referenceTableList, dbName, schema);
        }
        
    }
}
