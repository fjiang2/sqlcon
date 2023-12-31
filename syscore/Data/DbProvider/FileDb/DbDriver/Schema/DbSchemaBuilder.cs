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
            dtSchema.Namespace = ds.Namespace;

            foreach (DataTable dt in ds.Tables)
            {
                AddSchema(dtSchema, dt);
            }
        }

        public void AddSchema(DataTable dt)
        {
            DataTable dtSchema = DbSchemaColumnExtension.CreateTable();
            dbSchema.Tables.Add(dtSchema);

            dtSchema.TableName = dt.TableName;
            dtSchema.Namespace = dt.Namespace;
            AddSchema(dtSchema, dt);
        }

        private static void AddSchema(DataTable dtSchema, DataTable dt)
        {
            foreach (DataColumn column in dt.Columns)
            {
                CType cty = column.DataType.ToCType();
                DbSchemaColumn _column = new DbSchemaColumn
                {
                    SchemaName = dt.GetSchemaName(),
                    TableName = dt.TableName,
                    ColumnName = column.ColumnName,
                    DataType = cty.GetSqlType(),
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

                switch (cty)
                {
                    case CType.Int:
                        _column.Length = 4;
                        break;

                    case CType.BigInt:
                        _column.Length = 8;
                        break;

                    case CType.NVarChar:
                    case CType.NText:
                    case CType.NChar:
                        _column.Length *= 2;
                        _column.precision = 0;
                        break;

                    case CType.Bit:
                        _column.Length = 1;
                        _column.precision = 1;
                        break;
                }

                if (_column.IsPrimary)
                    _column.PKContraintName = $"PK_{dtSchema.TableName.ToIdent()}";

                var newRow = dtSchema.NewRow();
                _column.UpdateRow(newRow);
                dtSchema.Rows.Add(newRow);
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
