using System;
using System.IO;
using Microsoft.SqlServer.Management.Smo;

namespace MainConsole
{
    public class DbObjects
    {
        private readonly char[] _forbiddenFileChars = new char[] { '\\', '/', ':', '.', '-' };

        public void Script(Server srv, string directoryToSaveTo, string schema, Database db)
        {
            var scripter = new Scripter(srv);
            scripter.Options.ToFileOnly = true;
            scripter.Options.ExtendedProperties = true;
            scripter.Options.DriAll = true; // All the constraints
            scripter.Options.Indexes = true;
            scripter.Options.Triggers = true;
            scripter.Options.ScriptSchema = true;
            scripter.Options.SchemaQualifyForeignKeysReferences = true;
            scripter.Options.AnsiFile = true; //Save file as ANSI

            Script<Table, TableCollection>(directoryToSaveTo, schema, nameof(db.Tables), db.Tables, scripter);
            Script<StoredProcedure, StoredProcedureCollection>(directoryToSaveTo, schema, nameof(db.StoredProcedures),
                db.StoredProcedures, scripter);
            Script<View, ViewCollection>(directoryToSaveTo, schema, nameof(db.UserDefinedTypes), db.Views, scripter);
            Script<UserDefinedDataType, UserDefinedDataTypeCollection>(directoryToSaveTo, schema,
                nameof(db.UserDefinedDataTypes), db.UserDefinedDataTypes, scripter);
            Script<UserDefinedAggregate, UserDefinedAggregateCollection>(directoryToSaveTo, schema,
                nameof(db.UserDefinedAggregates), db.UserDefinedAggregates, scripter);
            Script<UserDefinedFunction, UserDefinedFunctionCollection>(directoryToSaveTo, schema,
                nameof(db.UserDefinedFunctions), db.UserDefinedFunctions, scripter);
            Script<UserDefinedTableType, UserDefinedTableTypeCollection>(directoryToSaveTo, schema,
                nameof(db.UserDefinedTableTypes), db.UserDefinedTableTypes, scripter);
            Script<UserDefinedType, UserDefinedTypeCollection>(directoryToSaveTo, schema, nameof(db.UserDefinedTypes),
                db.UserDefinedTypes, scripter);
        }

        private void Script<TObject, TCollection>(string directoryToSaveTo, string schema, string objectName, TCollection dbOjectCollection, Scripter scripter)
            where TObject : ScriptSchemaObjectBase
            where TCollection : SchemaCollectionBase
        {
            if (dbOjectCollection.Count == 0) return;

            var directory = Directory.CreateDirectory($@"{directoryToSaveTo}\{objectName}");

            foreach (TObject o in dbOjectCollection)
            {
                if (o.Schema != schema) continue;

                scripter.Options.FileName = $@"{directory.FullName}\{o.Name.Clean(_forbiddenFileChars, "-")}.sql";
                Console.WriteLine($"{objectName} {scripter.Options.FileName} ");
                scripter.Script(new[] { o.Urn });
            }
        }
    }

    public static class ExtensionMethods
    {
        public static string Clean(this string s, char[] badActors, string newVal)
        {
            var temp = s.Split(badActors, StringSplitOptions.RemoveEmptyEntries);
            return String.Join(newVal, temp);
        }
    }
}

