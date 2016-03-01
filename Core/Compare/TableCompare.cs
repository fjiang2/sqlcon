using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Sys.Data;

namespace Sys.Data.Comparison
{

  

    class TableCompare
    {
        #region Implemetation

        internal IPrimaryKeys PkColumns;
        private string[] compareColumns;
        public string[] ExceptColumns { get; set; }//don't compare these columns

        private TableSchema schema1;
        private TableSchema schema2;

        public TableCompare(TableSchema schema1, TableSchema schema2)
        {
            this.SideType = CompareSideType.compare;
            this.ExceptColumns = new string[] { };

            this.schema1 = schema1;
            this.schema2 = schema2;

        }

        public CompareSideType SideType { get; set; }

        public string Compare(IPrimaryKeys pk)
        {
            var dt1 = new TableReader(schema1.TableName).Table;
            var dt2 = new TableReader(schema2.TableName).Table;

            return Compare(pk, dt1, dt2);
        }


        public string[] CompareColumns
        {
            get
            {
                return this.compareColumns;
            }
        }

        private string Compare(IPrimaryKeys pk, DataTable table1, DataTable table2)
        {
            this.PkColumns = pk;
            this.compareColumns = table1.Columns
                .OfType<DataColumn>()
                .Select(row => row.ColumnName)
                .Except(PkColumns.Keys)
                .Except(ExceptColumns)
                .ToArray();

            StringBuilder builder = new StringBuilder();
            TableClause script = new TableClause(schema1);

            List<DataRow> R2 = new List<DataRow>();
            foreach (DataRow row1 in table1.Rows)
            {
                var row2 = table2.AsEnumerable().Where(row => RowCompare.Compare(PkColumns.Keys, row, row1)).FirstOrDefault();

                if (row2 != null)
                {
                    if (!RowCompare.Compare(compareColumns, row1, row2))
                    {
                        var compare = new RowCompare(this, row1, row2);

                        builder.AppendLine(script.UPDATE(compare));
                    }
                    R2.Add(row2);
                }
                else
                {
                    builder.Append(script.INSERT(row1));
                    builder.AppendLine();
                }
            }

            if (SideType != CompareSideType.copy)
            {
                foreach (DataRow row2 in table2.Rows)
                {
                    if (R2.IndexOf(row2) < 0)
                    {
                        builder.AppendLine(script.DELETE(row2, pk));
                    }
                }
            }

            if (builder.ToString() != string.Empty && SideType == CompareSideType.compare)
                builder.AppendLine(TableClause.GO);

            return builder.ToString();
        }

    
        #endregion


       

    }
}
