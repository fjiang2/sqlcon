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
using System.Data;

namespace Sys.Data
{
    public class ServerName : IComparable<ServerName>, IComparable, IDataPath
    {
        private ConnectionProvider provider;
        private string name;

        internal ServerName(ConnectionProvider provider, string alias)
        {
            this.provider = provider;
            this.name = alias;
        }

        private bool? _disconnected = null;
        public bool Disconnected 
        {
            get
            {
                if (_disconnected == null)
                    _disconnected = !this.Provider.CheckConnection();

                return (bool)_disconnected;
            }
        }

        public string Path
        {
            get { return this.name; }
        }

        public string FullPath => $"\\{Path}";

        public ConnectionProvider Provider
        {
            get { return this.provider; }
        }

        public DatabaseName DefaultDatabase => provider.DefaultDatabaseName;

        public int CompareTo(object obj)
        {
            return CompareTo((ServerName)obj);
        }

        public int CompareTo(ServerName n)
        {
            return this.provider.CompareTo(n.provider);
        }

        public override int GetHashCode()
        {
            return this.provider.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            ServerName dname = (ServerName)obj;
            return this.provider.Equals(dname.provider);
        }


        public DataSet ServerSchema()
        {
            return Provider.Schema.GetServerSchema(this);
        }


        public DatabaseName[] GetDatabaseNames()
        {
            return Provider.Schema.GetDatabaseNames();
        }

        public override string ToString()
        {
            return string.Format("\\{0}", this.name);
        }
    }
}
