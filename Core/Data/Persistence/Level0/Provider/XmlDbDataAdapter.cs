using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data;
using System.IO;

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
            string[] items = sql.Trim().Split(new string[] {"SELECT", "FROM" , "WHERE"}, StringSplitOptions.RemoveEmptyEntries);
            string name = items.Last().Trim();
             return new TableName(connection.Provider, name);
        }

        private int FillDataTable(TableName tname, DataSet ds)
        {
            var file = Path.Combine(connection.Directory, tname.DatabaseName.Name, tname.ShortName);
            file = file + ".xml";

            if (!File.Exists(file))
                throw new InvalidDataException($"table {tname.FormalName} data file \"{file}\" not exist");
            
            using (var reader = new StreamReader(file))
            {
                ds.ReadXml(reader);
            }

            return ds.Tables[0].Rows.Count;
        }
    }
}
