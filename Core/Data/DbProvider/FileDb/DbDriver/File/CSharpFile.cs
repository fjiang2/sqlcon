using System;
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
    class CSharpFile : DbFile
    {
        DataSet data = new DataSet();

        public CSharpFile(FileLink link)
            : base(link)
        {
        }

        public override void ReadSchema(DataSet dbSchema)
        {
            try
            {
                string code = fileLink.ReadAllText();
                Assembly assembly = Compile(fileLink.Name, code);
                this.data = CreateSchema(assembly);

                var schema = new DbSchemaBuilder(dbSchema);
                schema.AddSchema(data);
            }
            catch (Exception ex)
            {
                throw new Exception($"bad data source defined {fileLink}, {ex.Message}");
            }

        }

        public override int SelectData(SelectClause select, DataSet ds)
        {
            TableName tname = select.TableName;
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

        private static DataSet CreateSchema(Assembly assembly)
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
                    Type type = propertyInfo.PropertyType;
                    bool isNullable = Nullable.GetUnderlyingType(type) != null;
                    if (isNullable)
                        type = Nullable.GetUnderlyingType(type);

                    DataColumn column = new DataColumn(propertyInfo.Name, type)
                    {
                        AllowDBNull = isNullable,
                        Unique = false,
                        AutoIncrement = false,
                    };

                    dt.Columns.Add(column);
                }
            }

            return ds;
        }
    }
}
