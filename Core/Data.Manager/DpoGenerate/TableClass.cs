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
    public class TableClass
    {
        private TableName tableName;

        public string NameSpace { get; set; }
        public Modifier Modifier { get; set; }
        public Func<string, string> ClassNameRule { get; set; }

        public Level Level { get; set; }
        public bool IsPack { get; set; }
        public bool HasProvider { get; set; }

        
        public bool HasColumnAttribute { get; set; }
        public bool HasTableAttribute { get; set; }
        public bool MustGenerate { get; set; }


        public Dictionary<TableName, Type> dict { get; set; }

        public TableClass(TableName tableName)
        {
            this.tableName = tableName;

            this.NameSpace = Setting.DPO_CLASS_SUB_NAMESPACE;
            this.Modifier = Modifier.Public;

            this.Level = Level.Application;
            this.IsPack = false;
            this.HasProvider = false;

            this.HasColumnAttribute = true;
            this.HasTableAttribute = true;
            this.MustGenerate = true;
        }



        public bool CreateClass(string path)
        {
            ClassTableName ctname = new ClassTableName(tableName)
            {
                Level = Level,
                Pack = IsPack,
                HasProvider = HasProvider,
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
            HasTableAttribute = false;
            HasTableAttribute = false;
            return GenTableDpo(ctname, schema, path, cname);
        }


        private bool GenTableDpo(ClassTableName tname, ITable metatable, string path,  ClassName cname)
        {

            DpoGenerator gen = new DpoGenerator(tname, metatable, cname)
            {
                HasTableAttribute = HasTableAttribute,
                HasColumnAttribute = HasColumnAttribute,
                Dict = dict,
                OutputPath = path,
                MustGenerate = MustGenerate
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
