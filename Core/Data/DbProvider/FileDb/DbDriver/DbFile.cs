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
        public DbFile()
        {

        }

        public abstract int ReadData(FileLink link, SelectClause select, DataSet ds);
        

        public virtual string WriteData(TableName tname, DataTable dt)
        {
            return null;
        }

        public abstract void ReadSchema(FileLink link, DataSet dbSchema);

        public static IDbFile Create(DbFileType type)
        {
            switch (type)
            {
                case DbFileType.XmlDb:
                    return new XmlDbFile();

                case DbFileType.XmlDataSet:
                    return new DataSetFile();

                case DbFileType.CSharp:
                    return new CSharpFile();
            }

            return null;
        }
    }
}
