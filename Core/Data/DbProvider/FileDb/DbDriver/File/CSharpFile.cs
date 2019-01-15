using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sys.Data.IO;
using System.Reflection;

namespace Sys.Data
{
    public class CSharpFile : DbFile
    {
        DataSet data = new DataSet();

        public CSharpFile()
        {
        }

        public override void ReadSchema(FileLink link, DataSet dbSchema)
        {
            try
            {
                string code = link.ReadAllText();
                Assembly assembly = Compile(link.Name, code);
                this.data = GetDataSet(assembly);

                var schema = new DbSchemaBuilder(dbSchema);
                schema.AddSchema(data);
            }
            catch (Exception ex)
            {
                throw new Exception($"bad data source defined {link}, {ex.Message}");
            }

        }

        public override int ReadData(FileLink root, TableName tname, DataSet ds, string where)
        {
            if (!data.Tables.Contains(tname.ShortName))
                return -1;

            DataTable dt = data.Tables[tname.ShortName];
            ds.Clear();

            DataTable dt2;
            dt2 = dt.Copy();

            ds.Tables.Add(dt2);
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

        private static DataSet GetDataSet(Assembly assembly)
        {
            var classes = assembly.GetTypes().Where(type => type.IsClass).ToArray();

            DataSet ds = new DataSet
            {
                DataSetName = assembly.GetName().Name,
            };

            foreach (var clss in classes)
            {
                DataTable dt = new DataTable
                {
                    TableName = clss.Name,
                };

                ds.Tables.Add(dt);
                foreach (var propertyInfo in clss.GetProperties())
                {
                    dt.Columns.Add(new DataColumn(propertyInfo.Name, propertyInfo.PropertyType));
                }
            }

            return ds;
        }
    }
}
