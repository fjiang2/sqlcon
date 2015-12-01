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
using Sys.Data;
using System.Threading;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using Sys.Data.Log;

namespace Sys.Data.Manager
{
    public static class ManagerExtension
    {

        #region Logger

        /// <summary>
        /// register user defined transaction logee
        /// </summary>
        /// <param name="transactionType"></param>
        /// <param name="logee"></param>
        public static void Register(this TransactionLogeeType transactionType, ITransactionLogee logee)
        {
            LogManager.Instance.Register(transactionType, logee);
        }

        /// <summary>
        /// register user defined record/row logee
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="logee"></param>
        public static void Register(this TableName tableName, IRowLogee logee)
        {
            LogManager.Instance.Register(tableName, logee);
        }


        #endregion





        #region Dpo Generate


        public static bool GenTableDpo(this ClassTableName tname, 
            string path, 
            bool mustGenerate, 
            ClassName cname, 
            bool hasColumnAttribute, 
            Dictionary<TableName, Type> dict)
        {
            bool result = GenTableDpo(tname, tname.GetSchema(), path, mustGenerate, cname, true, hasColumnAttribute, dict);
            return result;
        }


        public static bool GenTableDpo(this DataTable table, string path, ClassName cname)
        {

            ITable schema = new DataTableDpoClass(table);
            ClassTableName tname = new ClassTableName(schema.TableName);
            return GenTableDpo(tname, schema, path, true, cname, false, false, new Dictionary<TableName, Type>());
        }


        private static bool GenTableDpo(this ClassTableName tname, 
            ITable metatable, 
            string path, 
            bool mustGenerate, 
            ClassName cname, 
            bool hasTableAttribute, 
            bool hasColumnAttribute, 
            Dictionary<TableName, Type> dict)
        {

            DpoGenerator gen = new DpoGenerator(tname, metatable, cname)
            { 
                HasTableAttribute =hasTableAttribute, 
                HasColumnAttribute = hasColumnAttribute,
                Dict = dict,
                OutputPath = path,
                MustGenerate = mustGenerate
            };

            gen.Generate();

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            bool result =  gen.Save();

           
            return result;
        }

    



        #endregion


    }
}
