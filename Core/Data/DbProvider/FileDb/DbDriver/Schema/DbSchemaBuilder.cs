﻿using System;
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
        private readonly DataSet dbSchema;

        public DbSchemaBuilder()
        {
            this.dbSchema = new DataSet();
        }

        public DbSchemaBuilder(DataSet dbSchema)
        {
            this.dbSchema = dbSchema;
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
                        IsPrimary = dt.PrimaryKey.Contains(column),
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
      
    }
}
