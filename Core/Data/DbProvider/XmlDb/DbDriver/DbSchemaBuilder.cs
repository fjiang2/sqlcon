using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Reflection;
using Sys.Data.IO;

namespace Sys.Data
{
    class DbSchemaBuilder
    {
        private readonly DataSet dbSchema = new DataSet();

        public DbSchemaBuilder()
        {

        }

        public DataSet DbSchmea => dbSchema;


        /// <summary>
        /// create dbSchema from data with DataSet format
        /// </summary>
        /// <param name="ds"></param>
        public void AddSchema(DataSet ds)
        {
            DataTable dtSchema = DbSchemaColumnExtension.CreateTable();
            dbSchema.Tables.Add(dtSchema);

            dtSchema.TableName = ds.DataSetName;

            foreach (DataTable dt in ds.Tables)
            {
                foreach (DataColumn column in dt.Columns)
                {
                    DbSchemaColumn _column = new DbSchemaColumn
                    {
                        SchemaName = TableName.dbo,
                        TableName = dt.TableName,
                        ColumnName = column.ColumnName,
                        DataType = column.DataType.ToCType().GetSqlType(),
                        Length = (short)column.MaxLength,
                        Nullable = column.AllowDBNull,
                        precision = 10,
                        scale = 0,
                        IsPrimary = column.Unique,
                        IsIdentity = column.AutoIncrement,
                        IsComputed = false,
                        definition = null,
                        PKContraintName = null,
                        PK_Schema = null,
                        PK_Table = null,
                        PK_Column = null,
                        FKContraintName = null,
                    };

                    var newRow = dtSchema.NewRow();
                    _column.UpdateRow(newRow);
                    dtSchema.Rows.Add(newRow);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lake"></param>
        public void AddSchema(DataLake lake)
        {
            foreach (DataSet ds in lake.Values)
            {
                AddSchema(ds);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        public void AddSchema(Assembly assembly)
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

            AddSchema(ds);
        }
    }
}
