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


        public Option()
        {
            Level = Level.Application;
            IsPack = false;
            HasProvider = false;

            HasColumnAttribute = true;
            HasTableAttribute = true;
            MustGenerate = true;
            CodeSorted = false;
        }
    }

    public class TableClass
    {
        private TableName tableName;

        public string NameSpace { get; set; }
        public Modifier Modifier { get; set; }
        public Func<string, string> ClassNameRule { get; set; }


        public Dictionary<TableName, Type> dict { get; set; }

        public Option option { get; set; } = new Option();

        public TableClass(TableName tableName)
        {
            this.tableName = tableName;

            this.NameSpace = Setting.DPO_CLASS_SUB_NAMESPACE;
            this.Modifier = Modifier.Public;
        }



        public bool CreateClass(string path)
        {
            ClassTableName ctname = new ClassTableName(tableName)
            {
                Level = option.Level,
                Pack = option.IsPack,
                HasProvider = option.HasProvider,
                ClassNameRule = ClassNameRule
            };

            ClassName cname = new ClassName(NameSpace, Modifier, ctname);

            return GenTableDpo(ctname, tableName.GetSchema(), path, cname);
        }


        public bool CreateClass(DataTable table, string path)
        {
            ITable schema = new DataTableDpoClass(table);
            ClassTableName ctname = new ClassTableName(schema.TableName);
            ClassName cname = new ClassName(NameSpace, Modifier, ctname);
            option.HasTableAttribute = false;
            option.HasTableAttribute = false;
            return GenTableDpo(ctname, schema, path, cname);
        }


        private bool GenTableDpo(ClassTableName tname, ITable metatable, string path,  ClassName cname)
        {

            DpoGenerator gen = new DpoGenerator(tname, metatable, cname, option)
            {
                Dict = dict,
                OutputPath = path,
            };

            gen.Generate();

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            bool result = gen.Save();


            return result;
        }






    }
}
