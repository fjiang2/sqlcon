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
using System.Text;
using System.Data;
using System.IO;
using Sys.Data;
using System.Reflection;
using Tie;
using Sys.Data.Manager;
using Sys.CodeBuilder;

namespace Sys.Data.Manager
{
    public class DpoGenerator
    {
        private TableName tableName;

        
        public DpoOption Option { get; set; }

        public DpoGenerator(TableName tableName)
        {
            this.tableName = tableName;
        }

        public void CreateClass()
        {
            ClassTableName ctname = new ClassTableName(tableName)
            {
                Option = Option
            };

            ClassName cname = new ClassName(Option.NameSpace, Option.Modifier, ctname);

            ITableSchema schema = new TableSchema(tableName);

            var dpoClass = new DpoClass(schema, cname, Option);

            var sourceCode = dpoClass.Generate(cname.Modifier, ctname);

            string fileName = string.Format("{0}\\{1}.cs", Option.OutputPath, cname.Class);

            if (!Directory.Exists(Option.OutputPath))
            {
                Directory.CreateDirectory(Option.OutputPath);
            }

            StreamWriter sw = new StreamWriter(fileName);
            sw.Write(sourceCode);
            sw.Close();
        
        }

    }
}
