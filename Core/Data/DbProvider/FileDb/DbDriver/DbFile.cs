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

        public abstract int SelectData(SelectClause select, DataSet result);

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

                case DbFileType.XmlDataLake:
                    return new DataLakeFile(link);

                case DbFileType.CSharp:
                    return new CSharpFile(link);
            }

            return null;
        }

        public override string ToString() => fileLink.ToString();


        protected static int QueryData(DataSet data, SelectClause select, DataSet result)
        {
            TableName tname = select.TableName;
            if (!data.Tables.Contains(tname.ShortName))
                return -1;

            DataTable dt = data.Tables[tname.ShortName];
            result.Clear();

            DataView dv = new DataView(dt)
            {
                RowFilter = select.Where,
            };

            DataTable dt2;
            if (select.Columns != null)
                dt2 = dv.ToTable(dt.TableName, false, select.Columns);
            else
                dt2 = dv.ToTable(dt.TableName);

            result.Tables.Add(dt2);

            return dt2.Rows.Count;
        }
    }
}
