using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data.Manager
{
    public static class Extension
    {

        public static TableSchema GetSchema(this TableName tname)
        {
            var schema = new TableSchema(tname);
            return schema;
        }



        #region Table Attribute Generate


        /// <summary>
        /// [TableName.Level] is not updated in [this.tname], then parameter [level] must be passed in
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        internal static string GetTableAttribute(this ITable metaTable, ClassTableName ctname)
        {
            string comment = string.Format("//Primary Keys = {0};  Identity = {1};", metaTable.PrimaryKeys, metaTable.Identity);
            StringBuilder attr = new StringBuilder("[Table(");
            TableName tableName = metaTable.TableName;
            switch (ctname.Level)
            {

                case Level.Application:
                    attr.AppendFormat("\"{0}\", Level.Application", tableName.Name);
                    break;

                case Level.System:
                    attr.AppendFormat("\"{0}\", Level.System", tableName.Name);
                    break;

                case Level.Fixed:
                    attr = attr.AppendFormat("\"{0}..[{1}]\", Level.Fixed", tableName.DatabaseName.Name, tableName.Name);
                    break;
            }


            if (ctname.HasProvider)
            {
                if (!tableName.Provider.Equals(ConnectionProviderManager.DefaultProvider))
                {
                    attr.AppendFormat(", Provider = {0}", (int)tableName.Provider);
                }
            }

            if (!ctname.Pack)
                attr.AppendFormat(", Pack = false", tableName.Name);

            attr.Append(")]");

            return string.Format("{0}    {1}", attr, comment);
        }



        #endregion


    }
}
