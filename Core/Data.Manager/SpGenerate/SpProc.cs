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
using Sys.Data;
using System.Reflection;
using Tie;

namespace Sys.Data.Manager
{
    class SpProc
    {
        DatabaseName databaseName;
        string spName;
        string spDef;
        string spDefVariable; //const variable name points to stored procedure definition

        public SpProc(DatabaseName databaseName, string spName, string spDef)
        {
            this.databaseName = databaseName;
            this.spName = spName;
            this.spDef = spDef;

            this.spDefVariable = "_" + spName;
        }

        public string SpName { get { return this.spName; } }

        private string escapeSpDef()
        {
            return spDef.Replace("\"", "\"\"").Replace("{", "{{").Replace("}", "}}");
        }

        public string Proc(string nameSpace, string className, string sa, string password)
        {
            string SQL = @"
            USE [{0}]
            DECLARE @objid INT
            SELECT  @objid = object_id
            FROM sys.all_objects 
            WHERE object_id = OBJECT_ID('{1}')

            SELECT  name,
                    'type' = TYPE_NAME(user_type_id),
                    max_length,
                    precision,
                    scale,
                    is_output
            FROM    sys.all_parameters
            WHERE   object_id = @objid
            ORDER BY parameter_id
            ";

            SqlCmd cmd = new SqlCmd(databaseName.Provider, string.Format(SQL, databaseName.Name, spName));
           // cmd.ChangeConnection(sa,password);
            DataTable dt = cmd.FillDataTable();
            DPCollection<SpParamDpo> parameters = new DPCollection<SpParamDpo>(dt);

            string comment = string.Format("//Machine Generated Code by {0} at {1}", ActiveAccount.Account.UserName, DateTime.Today);
            string usingString = @"{0}
using System;
using System.Data;
using Sys.Data;
";
            comment = string.Format(usingString, comment);
            string clss = @"{0}
namespace {1}
{{
    public partial class {2}
    {{
        {3}
    }}
}}
";

        string func = @"
        private static object ___{0}(int __xtype{6}{1})
        {{
            SqlCmd cmd = new SqlCmd(""{2}..[{0}]"");
{3}
            object __result = null;
            if(__xtype == 1)
              cmd.ExecuteNonQuery();
            else if(__xtype == 2)
              __result = cmd.FillDataTable();
            else if(__xtype == 3)
              __result = cmd.FillDataSet();
{4}
            return __result;
        }}

        public static void {0}({1})
        {{
            ___{0}(1{5});
        }}

        public static DataTable dt_{0}({1})
        {{
            return (DataTable)___{0}(2{5});
        }}

        public static DataSet ds_{0}({1})
        {{
            return (DataSet)___{0}(3{5});
        }}

        public const string {7} = @""{8}"";
";
        
            string signuture1 = "";
            string code1 = "";
            string code2 = "";
            string signuture2 = "";
            string tab = "            ";
            foreach (SpParamDpo param in parameters)
            {
                SpParam p = new SpParam(param);
                if (signuture1 != "") 
                    signuture1 += ", ";
                 
                if (signuture2 != "") 
                    signuture2 += ", ";

                signuture1 += p.signuture1();
                signuture2 += p.signuture2();

                code1 += tab + p.param1();
                code2 += tab + p.param2();
                
            }

            string method = string.Format(func,
                spName, 
                signuture1, 
                databaseName.Name, 
                code1, 
                code2, 
                signuture2 == "" ? "" : ", " + signuture2, 
                signuture1 == "" ? "" : ", ",
                this.spDefVariable, 
                escapeSpDef()
                );


            return string.Format(clss, comment, nameSpace, className, method);
        }


        public bool IsSpChanged(string nameSpace, string className)
        {
            Type ty = HostType.GetType(string.Format("{0}.{1}", nameSpace, className));
            if (ty != null)
            {
                FieldInfo field = ty.GetField(this.spDefVariable);
                if (field != null)
                {
                    if((string)field.GetValue(null) != this.spDef)
                        return true;
                }
            }

            return false;
        }
    }
}
