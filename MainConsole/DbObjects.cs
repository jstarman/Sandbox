﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.Management.Sdk.Sfc;
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
            Script<Synonym, SynonymCollection>(directoryToSaveTo, schema, nameof(db.Synonyms), db.Synonyms, scripter);
        }

        public void ScriptTable(Server srv, string directoryToSaveTo, string schema, Database db)
        {
            var scripter = new Scripter(srv);
            scripter.Options.ToFileOnly = true;
            scripter.Options.ExtendedProperties = true;
            //scripter.Options.DriAll = true; // All the constraints
            //scripter.Options.Indexes = true;
            //scripter.Options.Triggers = true;
            scripter.Options.ScriptSchema = true;
            scripter.Options.AnsiFile = true; //Save file as ANSI

            DeconstructScripts(directoryToSaveTo, schema, db.Tables, scripter);
        }

        private void DeconstructScripts(string directoryToSaveTo, string schema, TableCollection tables, Scripter scripter)
        {
            if (tables.Count == 0) return;

            var directory = Directory.CreateDirectory($@"{directoryToSaveTo}\Deconstruct");

            foreach (Table t in tables)
            {
                if (t.Schema != schema) continue;

                CreateScript(scripter, directory, $"Create_Table_{t.Name}", new [] { t.Urn});

                var triggerUrn = t.Triggers.Cast<Trigger>().Select(tr => tr.Urn).ToArray();
                if(triggerUrn.Any())
                    CreateScript(scripter, directory, $"Create_Trigger_{t.Name}", triggerUrn);

                var urnCollection = t.Indexes.Cast<Index>().Select(i => i.Urn).ToList(); //.SingleOrDefault(index => index.IndexKeyType == IndexKeyType.DriPrimaryKey);
                CreateScript(scripter, directory, $"Create_Index_{t.Name}", urnCollection.ToArray());
            }
        }

        private void CreateScript(Scripter scripter, DirectoryInfo directory, string fileName, Urn[] urnCollection)
        {
            scripter.Options.FileName = $@"{directory.FullName}\{fileName.Clean(_forbiddenFileChars, "-")}.sql";
            Console.WriteLine($" {scripter.Options.FileName} ");
            scripter.Script(urnCollection);
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

