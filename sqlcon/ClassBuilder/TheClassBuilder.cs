using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;
using Sys;
using Sys.Data;
using Sys.CodeBuilder;
using Sys.Data.Manager;
using System.Data.Common;

namespace sqlcon
{
    abstract class TheClassBuilder : ClassMaker
    {
        protected const string EXTENSION = "Extension";
        protected const string ASSOCIATION = "Association";

        protected CSharpBuilder builder;

        public TheClassBuilder(ApplicationCommand cmd)
            : base(cmd)
        {
            builder = new CSharpBuilder();

        }

        public void AddOptionalUsing()
        {
            builder.AddUsingRange(base.Usings);
        }

        public TypeInfo[] OptionalBaseType(params TypeInfo[] inherits)
        {
            List<TypeInfo> bases = new List<TypeInfo>(inherits);

            string _base = cmd.GetValue("base");
            if (_base != null)
            {
                string[] items = _base.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string item in items)
                {
                    string type = item.Replace("~", ClassName);
                    bases.Add(new TypeInfo { UserType = type });
                }
            }

            return bases.ToArray();
        }

        protected abstract void CreateClass();

        private void createClass()
        {
            builder.Namespace = NamespaceName;
            CreateClass();
        }

        public string WriteFile(string path)
        {
            createClass();

            base.PrintOutput(builder, ClassName);
            string code = $"{builder}";
            string file = Path.ChangeExtension(Path.Combine(path, ClassName), "cs");
            code.WriteIntoFile(file);

            return file;
        }

        public void Done()
        {
            createClass();
            PrintOutput(builder, ClassName);
        }

     

        private string[] optionMethods = null;
        public bool ContainsMethod(string methodName)
        {
            if (optionMethods == null)
            {
                string optionMethod = cmd.GetValue("methods");
                if (optionMethod != null)
                    optionMethods = optionMethod.Split(',');
                else
                    optionMethods = new string[] { };
            }

            if (optionMethods.Length == 0)
                return true;

            return optionMethods.Contains(methodName);
        }

        public string PropertyName(DataColumn column)
        {
            string propertyName = column.ColumnName.ToFieldName("C");
            if (propertyName == ClassName)
                propertyName = propertyName + "1";
            return propertyName;
        }
    }
}
