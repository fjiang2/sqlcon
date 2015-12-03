using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sys.Data;
using Sys.CodeBuilder;


namespace Sys.Data.Manager
{
    public class TableClass
    {
        private TableName tableName;

        public string NameSpace { get; set; }
        public Func<string, string> ClassNameRule { get; set; }
        public Level Level { get; set; }
        public bool isPack { get; set; }
        public bool hasProvider { get; set; }

        public AccessModifier Modifier { get; set; }

        public Dictionary<TableName, Type> dict { get; set; }

        public TableClass(TableName tableName)
        {
            this.tableName = tableName;

            this.NameSpace = Setting.DPO_CLASS_SUB_NAMESPACE;
            this.Level = Level.Application;
            this.Modifier = AccessModifier.Public;
            this.isPack = false;
            this.hasProvider = false;

        }

        public void CreateClass(string path)
        {
            ClassTableName ctname = new ClassTableName(tableName)
            {
                Level = Level,
                Pack = isPack,
                HasProvider = hasProvider,
                ClassNameRule = ClassNameRule
            };

            ClassName cname = new ClassName(NameSpace, Modifier, ctname);

            ctname.GenTableDpo(path, true, cname, true, dict);
        }
    }
}
