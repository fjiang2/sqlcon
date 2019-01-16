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
        protected readonly FileLink fileLink;

        public DbFile(FileLink link)
        {
            this.fileLink = link;
        }

        public abstract void ReadSchema(DataSet dbSchema);

        public abstract int SelectData(SelectClause select, DataSet ds);

        public virtual int InsertData(InsertClause insertt)
        {
            return -1;
        }

        public virtual int UpdateData(UpdateClause update)
        {
            return -1;
        }

        public virtual int DeleteData(DeleteClause delete)
        {
            return -1;
        }

        public static IDbFile Create(DbFileType type, FileLink link)
        {
            switch (type)
            {
                case DbFileType.XmlDb:
                    return new XmlDbFile(link);

                case DbFileType.XmlDataSet:
                    return new DataSetFile(link);

                case DbFileType.CSharp:
                    return new CSharpFile(link);
            }

            return null;
        }
    }
}
