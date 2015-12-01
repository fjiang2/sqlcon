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
    public class DatabaseName : IComparable<DatabaseName>, IComparable, IDataPath
    {
        private ConnectionProvider provider;
        private string name;

        public DatabaseName(ConnectionProvider provider, string databaseName)
        {
            this.provider = provider;
            this.name = databaseName;
        }

        public string Name
        {
            get { return this.name; }
        }

        public string Path
        {
            get { return this.name; }
        }
        
        public ServerName ServerName
        {
            get { return Provider.ServerName; }
        }

        public ConnectionProvider Provider
        {
            get
            {
                this.provider.InitialCatalog = name;
                return this.provider; 
            }
        }

        public int CompareTo(object obj)
        {
            return CompareTo((DatabaseName)obj);
        }

        public int CompareTo(DatabaseName n)
        {
            if (this.ServerName.CompareTo(n.ServerName) == 0)
               return this.name.CompareTo(n.name);

            return this.ServerName.CompareTo(n.ServerName);
        }


        public override int GetHashCode()
        {
            return name.GetHashCode() + this.ServerName.GetHashCode() * 324819;
        }

        public override bool Equals(object obj)
        {
            DatabaseName dname = (DatabaseName)obj;
            return this.name.ToLower().Equals(dname.name.ToLower()) && this.ServerName.Equals(dname.ServerName);
        }


        public override string ToString()
        {
            return string.Format("{0}\\{1}", this.ServerName, this.name);
        }
    }
}
