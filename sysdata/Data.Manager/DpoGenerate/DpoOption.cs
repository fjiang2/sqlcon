using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sys.CodeBuilder;

namespace Sys.Data.Manager
{

    public class DpoOption
    {
        public string NameSpace { get; set; }

        public Modifier Modifier { get; set; }

        public string ClassNameSuffix { get; set; }

        public Level Level { get; set; }
        public bool IsPack { get; set; }
        public bool HasProvider { get; set; }


        public bool HasColumnAttribute { get; set; }
        public bool HasTableAttribute { get; set; }

        public bool CodeSorted { get; set; }

        public Dictionary<TableName, Type> dict { get; set; }

        public string OutputPath { get; set; }

        public Func<string, string> ClassNameRule { get; set; }

        public DpoOption()
        {

            NameSpace = Setting.DPO_CLASS_SUB_NAMESPACE;
            ClassNameSuffix = "Dpo";

            Modifier = Modifier.Public;

            Level = Level.Application;
            IsPack = false;
            HasProvider = false;

            HasColumnAttribute = true;
            HasTableAttribute = true;
            CodeSorted = false;
            dict = new Dictionary<TableName, Type>();
            OutputPath = "C:\\temp\\dpo";

            ClassNameRule = name => name.Substring(0, 1).ToUpper() + name.Substring(1).ToLower() + ClassNameSuffix;
        }
    }

}
