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

namespace Sys.Data.Manager
{
    class DpoGenerator
    {
        private string sourceCode;
        private ClassTableName ctname;
        private ClassName cname;

        private ITable schema;
        private DpoClass dpoClass;


        public bool RegisterTable { get; set; }
        public Dictionary<TableName, Type> Dict { get; set; }
        
        public string OutputPath { get; set; }
        

        private Option option { get; set; }

        public DpoGenerator(ClassTableName ctname, ITable schema, ClassName cname, Option option)
        {
            this.option = option;
            this.Dict = new Dictionary<TableName, Type>();
            this.RegisterTable = true;
            this.OutputPath = "C:\\temp\\dpo";

            this.ctname = ctname;
            this.cname = cname;
            this.schema = schema;
        }

        public void Generate()
        {
            if (schema.TableID == -1 && RegisterTable)
            {
                //###DictTable.MustRegister(ctname);
            }

            dpoClass = new DpoClass(schema, cname, option, Dict);
            this.sourceCode = dpoClass.Generate(cname.Modifier, ctname);

        }

        public bool Save()
        {
            string fileName = string.Format("{0}\\{1}.cs", OutputPath, cname.Class);

            if (!option.MustGenerate)
            {
                if (File.Exists(fileName))
                {
                    if ((File.GetAttributes(fileName) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)    //this file is not checked out
                    {
                        if (dpoClass.IsTableChanged(ctname))
                            throw new MessageException("{0} is modified, please check out class {1} to refresh", ctname, cname.Class);

                        return false;
                    }
                }

                if (!dpoClass.IsTableChanged(ctname))
                    return false;
            }

            //if (schema.TableID == -1 && RegisterTable)
            //    throw new MessageException("Table ID {0} is not defined", ctname);

            if (!Directory.Exists(OutputPath))
            {
                Directory.CreateDirectory(OutputPath);
            }

            StreamWriter sw = new StreamWriter(fileName);
            sw.Write(sourceCode);
            sw.Close();
        
            return true;
        }

    }
}
