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
                string assemblyFile = fileLink.FileName;
                if (assemblyFile == null)
                    throw new Exception($"assemly must be local file, {fileLink}");
                if (!File.Exists(assemblyFile))
                    throw new Exception($"assemly file doesn't exist, {fileLink}");

                string ns = fileLink.Options.Contains("namespace") ? fileLink.Options["namespace"] as string : null;
                string clss = fileLink.Options.Contains("class") ? fileLink.Options["class"] as string : null;

                bool typeFilter(Type type)
                {
                    if (!type.IsClass)
                        return false;

                    if (ns != null && !type.Namespace.IsMatch(ns))
                        return false;

                    if (clss != null && !type.Name.IsMatch(clss))
                        return false;

                    return true;
                }

                Assembly assembly = Assembly.LoadFrom(assemblyFile);
                this.data = TypeExtension.CreateDataSet(assembly, typeFilter);

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

    }
}
