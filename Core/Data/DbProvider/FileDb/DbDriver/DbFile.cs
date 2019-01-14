using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sys.Data.IO;

namespace Sys.Data
{
    public abstract class DbFile : IDbFile
    {
        public abstract int ReadData(FileLink link, TableName tname, DataSet ds, string where);
        public abstract void ReadSchema(FileLink link, DataSet dbSchema);


        public virtual string WriteData(TableName tname, DataTable dt)
        {
            return null;
        }

        public virtual string WriteSchema(DatabaseName dname)
        {
            return null;
        }

        public virtual string WriteSchema(ServerName sname)
        {
            return null;
        }

        public static IDbFile Create(DbFileType type)
        {
            switch (type)
            {
                case DbFileType.XmlDb:
                    return new XmlDbFile();

                case DbFileType.XmlDataSet:
                    return new DataSetFile();
            }

            return null;
        }
    }
}
