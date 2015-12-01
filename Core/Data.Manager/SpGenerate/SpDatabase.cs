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
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Sys.Data;

namespace Sys.Data.Manager
{
    public class SpDatabase
    {
        public const string SP_NAME = "name";
        public const string SP_DEFINITION = "sp";


        private DatabaseName databaseName;
        private string path;

        public SpDatabase(DatabaseName databaseName, string path)
        {
            this.databaseName = databaseName;
            this.path = string.Format("{0}\\{1}\\{2}",path, (int)databaseName.Provider, databaseName.Name);

            if (!Directory.Exists(this.path))
            {
                Directory.CreateDirectory(this.path);
            }
        }

        public int Generate(string nameSpace, string sa, string password)
        {
            string SQL = @"
            USE [{0}]
            SELECT name, OBJECT_DEFINITION(OBJECT_ID) AS sp
            FROM sys.procedures
            WHERE is_ms_shipped <> 1
            ORDER BY name
            ";

            SqlCmd cmd = new SqlCmd(databaseName.Provider, string.Format(SQL, databaseName.Name));
           // cmd.ChangeConnection(sa, password);
            DataTable dt = cmd.FillDataTable();
            
            
            foreach (DataRow row in dt.Rows)
            {
                SpProc proc = new SpProc(databaseName, (string)row[SP_NAME], row[SP_DEFINITION].IsNull<string>(""));
                
                string sourceCode = proc.Proc(nameSpace, databaseName.Name, sa, password);
                WriteFile(proc.SpName, sourceCode, nameSpace, proc.IsSpChanged(nameSpace, databaseName.Name));
            }
            
            return dt.Rows.Count;
        }


        private bool WriteFile(string spName, string sourceCode, string nameSpace, bool isSpChanged)
        {
            string fileName = string.Format("{0}\\{1}.cs",path, spName);
            
            if (File.Exists(fileName))
            {
                if ((File.GetAttributes(fileName) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)    //this file is not checked out
                {
                    if (isSpChanged)
                        throw new MessageException("Stored Procedure {0}..{1} is modified, please check out class {2}.{3} to refresh", 
                            databaseName.Name, 
                            spName, 
                            nameSpace, 
                            databaseName.Name);

                    return false;
                }
            }

            StreamWriter sw = new StreamWriter(fileName);
            sw.Write(sourceCode);
            sw.Close();


            return true;
        }
    }
}
