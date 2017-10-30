using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sys;
using Sys.CodeBuilder;
using Tie;


namespace sqlcon
{
    class TieClassBuilder : TheClassBuilder
    {

        public TieClassBuilder(Command cmd)
            : base(cmd)
        {

            builder.AddUsing("System");
            builder.AddUsing("System.Collections.Generic");
            builder.AddUsing("System.Data");
            builder.AddUsing("System.Linq");
            AddOptionalUsing();

        }


        protected override void CreateClass()
        {
            var clss = new Class(ClassName)
            {
                modifier = Modifier.Public | Modifier.Partial,
                Sorted = true

            };

            builder.AddClass(clss);
            string code = ReadAllText(cmd.arg1);

            Memory DS = new Memory();
            try
            {
                Script.Execute(code, DS);
            }
            catch (Exception ex)
            {
                cerr.WriteLine(ex.Message);
                return;
            }

            foreach (VAR var in DS.Names)
            {
                VAL val = DS[var];
                create(clss, string.Empty, (string)var, val);
            }

        }

        private void create(Class clss, string prefix, string key, VAL val)
        {
            TypeInfo ty;
            Property prop;

            if (val.IsAssociativeArray())
            {
                var clss1 = new Class(key)
                {
                    modifier = Modifier.Public,
                    Sorted = true
                };

                clss.Add(clss1);

                if (prefix == string.Empty)
                    prefix = key;
                else
                    prefix = $"{prefix}.{key}";

                foreach (var member in val.Members)
                {
                    create(clss1, prefix, member.Name, member.Value);
                    continue;
                }
                ty = new TypeInfo { userType = key };
            }
            else
            {
                Type type = typeof(string);
                if (val.HostValue != null)
                {
                    type = val.HostValue.GetType();
                }

                ty = new TypeInfo(type);
            }

            string var = $"{prefix}.{key}";
            if (prefix == string.Empty)
                var = key;

            prop = createProperty(key, ty, var);
            clss.Add(prop);
        }

        private Property createProperty(string name, TypeInfo ty, string var)
        {
            Comment comment = new Comment(var) { alignment = Alignment.Top };
            return new Property(ty, name)
            {
                modifier = Modifier.Public,
                comment = comment
            };
        }
    }
}
