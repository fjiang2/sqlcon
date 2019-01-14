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
    public class XmlDbDataAdapter : DbDataAdapter
    {
        XmlDbCommand command;
        XmlDbConnection connection;
        ConnectionProvider provider;

        public XmlDbDataAdapter()
        {
        }

        public override int Fill(DataSet dataSet)
        {
            command = (XmlDbCommand)this.SelectCommand;
            connection = (XmlDbConnection)command.Connection;
            provider = connection.Provider;

            string sql = command.CommandText;
            TableName tname = getTableName(sql);
            return FillDataTable(tname, dataSet);
        }

        private TableName getTableName(string sql)
        {
            string[] items = sql.Trim().Split(new string[] { "SELECT", "FROM", "WHERE" }, StringSplitOptions.RemoveEmptyEntries);

            string name = null;
            if (items.Length > 1)
                name = items[1].Trim();
            else
                throw new InvalidDataException($"cannot extract table name from SQL:{sql}");

            return new TableName(connection.Provider, name);
        }

        private int FillDataTable(TableName tname, DataSet ds)
        {
            var file = new XmlDbFile();
            return file.ReadData(connection.FileLink, tname, ds);
        }
    }
}
