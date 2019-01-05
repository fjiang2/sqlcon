﻿using Sys.CodeBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tie;

namespace sqlcon
{
    internal class Json2CSharp
    {
        private Memory DS = new Memory();
        private CSharpBuilder builder;
        public Json2CSharp(CSharpBuilder builder, string code)
        {
            this.builder = builder;
            Script.Execute(code, DS);
        }

        public void Generate(string cname)
        {
            Class clss = new Class(cname)
            {
                modifier = Modifier.Public | Modifier.Partial
            };

            builder.AddClass(clss);

            foreach (VAR var in DS.Names)
            {
                VAL val = DS[var];
                createClass(clss, string.Empty, (string)var, val, classOnly: false);
            }
        }


        private void createClass(Class clss, string prefix, string key, VAL val, bool classOnly)
        {
            TypeInfo ty = null;
            string var = null;

            if (val.IsAssociativeArray())
            {
                var clss1 = new Class(key)
                {
                    modifier = Modifier.Public | Modifier.Partial
                };

                builder.AddClass(clss1);

                prefix = MakeVariableName(prefix, key);

                foreach (var member in val.Members)
                {
                    createClass(clss1, prefix, member.Name, member.Value, classOnly: false);
                }

                if (classOnly)
                    return;

                ty = new TypeInfo(key);
            }
            else if (val.IsList)
            {
                Dictionary<string, VAL> dict = new Dictionary<string, VAL>();
                foreach (var item in val)
                {
                    if (item.IsAssociativeArray())
                    {
                        foreach (var member in item.Members)
                        {
                            if (!dict.ContainsKey(member.Name))
                                dict.Add(member.Name, member.Value);
                        }
                    }
                }

                VAL _val = new VAL();
                foreach (var kvp in dict)
                    _val.AddMember(kvp.Key, kvp.Value);

                if (dict.Count > 0)
                {
                    //if (key.EndsWith("s"))
                    //    key = key.Substring(0, key.Length - 1);

                    createClass(clss, prefix, key, _val, classOnly: true);

                    ty = new TypeInfo(key)
                    {
                        isArray = true
                    };

                    var = MakeVariableName(prefix, $"{key}[]");
                }
            }

            if (ty == null)
            {
                Type type = typeof(string);
                if (val.HostValue != null)
                {
                    type = val.HostValue.GetType();
                }

                ty = new TypeInfo(type);
            }

            //if (var == null)
                var = MakeVariableName(prefix, key);

            Property prop = createProperty(key, ty, var);
            clss.Add(prop);
        }

        private string MakeVariableName(string prefix, string key)
        {
            if (prefix == string.Empty)
                return key;

            if (key.StartsWith("[") && key.EndsWith("]"))
                return $"{prefix}{key}";
            else
                return $"{prefix}.{key}";


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
