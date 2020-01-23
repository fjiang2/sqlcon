﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sys.Data.IO;

namespace Sys.Data
{
    class AssemblyFile : DbFile
    {
        DataSet data = new DataSet();

        public AssemblyFile(FileLink link)
            : base(link)
        {
        }

        public override void ReadSchema(DataSet dbSchema)
        {
            try
            {
                string assemblyFile = (fileLink as DiskFileLink).FileName;
                if (assemblyFile == null)
                    throw new Exception($"assemly must be local file, {fileLink}");
                if(!File.Exists(assemblyFile))
                    throw new Exception($"assemly file doesn't exist, {fileLink}");

                Assembly assembly = Assembly.LoadFrom(assemblyFile);
                this.data = TypeExtension.CreateDataSet(assembly, type => type.IsClass);

                var schema = new DbSchemaBuilder(dbSchema);
                schema.AddSchema(data);
            }
            catch (Exception ex)
            {
                throw new Exception($"bad data source defined {fileLink}, {ex.Message}");
            }

        }

        public override int SelectData(SelectClause select, DataSet result)
        {
            TableName tname = select.TableName;
            if (!data.Tables.Contains(tname.ShortName))
                return -1;

            DataTable dt = data.Tables[tname.ShortName];
            result.Clear();

            DataTable dt2;
            dt2 = dt.Copy();

            result.Tables.Add(dt2);
            return dt2.Rows.Count;
        }

        private static Assembly Compile(string assemblyName, string cs)
        {
            var csc = new Compiler.CSharpCompiler();
            csc.Compile(assemblyName, cs);

            if (csc.HasError)
            {
                throw new Exception(csc.GetError());
            }

            return csc.GetAssembly();
        }


    }
}
