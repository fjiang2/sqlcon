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
            SqlClauseParser parset = new SqlClauseParser(connection.Provider, sql);
            SelectClause select = parset.ParseSelect();

            var dataFile = (provider as FileDbConnectionProvider).DataFile;
            return dataFile.SelectData(select, ds);
        }
        
    }
}
