using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Compiler
{
    public class DataSetBuilder
    {
        private Assembly assembly;
        public DataSetBuilder(Assembly assembly)
        {
            this.assembly = assembly;
        }

        public DataSet ToDataSet()
        {
            var classes = assembly.GetTypes().Where(type => type.IsClass).ToArray();
            DataSet ds = new DataSet
            {
                DataSetName = assembly.FullName
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
