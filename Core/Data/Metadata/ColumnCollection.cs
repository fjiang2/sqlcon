//--------------------------------------------------------------------------------------------------//
//                                                                                                  //
//        DPO(Data Persistent Object)                                                               //
//                                                                                                  //
//          Copyright(c) Datum Connect Inc.                                                         //
//                                                                                                  //
// This source code is subject to terms and conditions of the Datum Connect Software License. A     //
// copy of the license can be found in the License.html file at the root of this distribution. If   //
// you cannot locate the  Datum Connect Software License, please send an email to                   //
// datconn@gmail.com. By using this source code in any fashion, you are agreeing to be bound        //
// by the terms of the Datum Connect Software License.                                              //
//                                                                                                  //
// You must not remove this notice, or any other, from this software.                               //
//                                                                                                  //
//                                                                                                  //
//--------------------------------------------------------------------------------------------------//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sys.Data
{
    public class ColumnCollection : List<IColumn>
    {
        ITable metaTable;

        public ColumnCollection(ITable metaTable)
        {
            this.metaTable = metaTable;
        }

        public IColumn this[string columnName]
        {
            get
            {
                foreach (ColumnSchema column in this)
                {
                    if (column.ColumnName == columnName)
                        return column;
                }

                return null;
            }

        }

        public bool Exists(string columnName)
        {
            return this[columnName] != null;
        }


        public void UpdatePrimary(IPrimaryKeys primary)
        {

            var columns = this.Where(column => Array.IndexOf<string>(primary.Keys, column.ColumnName) >= 0);
            foreach (ColumnSchema column in columns)
            {
                column.IsPrimary = true;
            }

        }

        internal void UpdateForeign(IForeignKeys foreign)
        {
            foreach (ForeignKey key in foreign.Keys)
            {
                IColumn column = this.Find(col => col.ColumnName == key.FK_Column);
                column.ForeignKey = key;
            }
        }

        /// <summary>
        /// Create SQL INSERT INTO ... VALUES() command 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public string InsertCommand(string line, char[] separator)
        {
            string DELIMETER = "'";
            string[] items = line.Split(separator);

            if (items.Length < this.Count)
                throw new MessageException("#line(#{0}) data not match to table(#{1})", items.Length, this.Count);

            string[] values = new string[this.Count];
            int i = 0;
            foreach (ColumnSchema column in this)
            {
                object obj = column.Parse(items[i]);
                if (obj == null)
                    values[i] = "NULL";
                else if (obj is DateTime)
                    values[i] = DELIMETER + ((DateTime)obj).ToShortDateString() + DELIMETER;
                else if (obj is string)
                    values[i] = "N" + DELIMETER + (obj as string).Replace("'", "''") + DELIMETER;
                else
                    values[i] = obj.ToString();

                i++;
            }

            return string.Format("INSERT INTO {0} VALUES ({1})", this.metaTable.TableName, string.Join(",", values));
        }
    }
}
