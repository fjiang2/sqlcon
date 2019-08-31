using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Sys.Data;

namespace sqlcon.Windows
{
    public class DbColumnNodeUI : DbTreeNodeUI
    {
        private DbTreeUI tree;

        public DbColumnNodeUI(DbTreeUI tree, ColumnSchema column)
            : base(GetSQLField(column), GetImage(column))
        {
            this.tree = tree;
            Path = column;
        }


        private static string GetImage(ColumnSchema column)
        {
            string image = "AlignHorizontalTop_16x16.png";
            if (column.IsPrimary || column.IsForeignKey)
                image = "key.png";
            return image;
        }

        private static string GetSQLField(ColumnSchema column)
        {
            string ty = column.GetSQLType();
            List<string> list = new List<string>();
            if (column.IsPrimary)
                list.Add("PK");

            if (column.IsForeignKey)
                list.Add("FK");

            if (column.IsIdentity)
                list.Add("++");

            list.Add(ty);
            list.Add(column.Nullable ? "null" : "not null");

            if (column.IsComputed)
            {
                list.Add($"={column.Definition}");
            }

            string line = string.Join(", ", list);
            return $"{column.ColumnName} ({line})";
        }
    }
}
