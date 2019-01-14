using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data
{
    public class FileDbDataAdapter : DbDataAdapter
    {
        FileDbCommand command;
        FileDbConnection connection;
        ConnectionProvider provider;

        public FileDbDataAdapter()
        {
        }

        public override int Fill(DataSet dataSet)
        {
            command = (FileDbCommand)this.SelectCommand;
            connection = (FileDbConnection)command.Connection;
            provider = connection.Provider;

            string sql = command.CommandText;
            return FillTable(sql, dataSet);
        }

        private int FillTable(string sql, DataSet ds)
        {
            sql = ToUpperCase(sql);
            string[] items = sql.Split(new string[] { "SELECT", "FROM", "WHERE" }, StringSplitOptions.RemoveEmptyEntries);

            string name = null;
            string where = null;
            if (items.Length > 1)
                name = items[1].Trim();
            else
                throw new InvalidDataException($"cannot extract table name from SQL:{sql}");

            if (items.Length > 2)
                where = items[2].Trim();

            var tname = new TableName(connection.Provider, name);

            var file = (provider as FileDbConnectionProvider).DataFile;

            return file.ReadData(connection.FileLink, tname, ds, where);
        }

        private static string ToUpperCase(string sql)
        {
            string[] keywords = new string[] { "SELECT", "FROM", "WHERE" };
            string[] items = sql.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < items.Length; i++)
            {
                foreach (string keyword in keywords)
                {
                    if (string.Compare(items[i], keyword, ignoreCase: true) == 0)
                        items[i] = keyword;
                }
            }

            return string.Join(" ", items);
        }
    }
}
