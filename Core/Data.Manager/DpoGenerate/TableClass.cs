using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using Sys.Data;
using Sys.CodeBuilder;


namespace Sys.Data.Manager
{

    public class Option
    {

        public Level Level { get; set; }
        public bool IsPack { get; set; }
        public bool HasProvider { get; set; }


        public bool HasColumnAttribute { get; set; }
        public bool HasTableAttribute { get; set; }
        public bool MustGenerate { get; set; }

        public bool CodeSorted { get; set; }

        public bool RegisterTable { get; set; }

        public Dictionary<TableName, Type> dict { get; set; }

        public string OutputPath { get; set; }

        public Option()
        {
            Level = Level.Application;
            IsPack = false;
            HasProvider = false;

            HasColumnAttribute = true;
            HasTableAttribute = true;
            MustGenerate = true;
            CodeSorted = false;
            dict = new Dictionary<TableName, Type>();
            OutputPath = "C:\\temp\\dpo";
        }
    }

    public class TableClass
    {
        private TableName tableName;

        public string NameSpace { get; set; }
        public Modifier Modifier { get; set; }
        public Func<string, string> ClassNameRule { get; set; }

        public Option option { get; set; } = new Option();

        public TableClass(TableName tableName)
        {
            this.tableName = tableName;

            this.NameSpace = Setting.DPO_CLASS_SUB_NAMESPACE;
            this.Modifier = Modifier.Public;
        }



        public bool CreateClass()
        {
            ClassTableName ctname = new ClassTableName(tableName)
            {
                Level = option.Level,
                Pack = option.IsPack,
                HasProvider = option.HasProvider,
                ClassNameRule = ClassNameRule
            };

            ClassName cname = new ClassName(NameSpace, Modifier, ctname);

            ITable metatable = tableName.GetSchema();

            DpoGenerator gen = new DpoGenerator(ctname, metatable, cname, option);
            gen.Generate();
            bool result = gen.Save();
            return result;
        }






    }
}
