using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class New : Buildable
    {
        private TypeInfo type;
        private Arguments args;
        private List<PropertyInfo> properties { get; } = new List<PropertyInfo>();

        public ValueOutputFormat Format { get; set; } = ValueOutputFormat.SingleLine;

        public New(TypeInfo type)
          : this(type, null)
        {
        }

        public New(TypeInfo type, Arguments args)
        {
            this.type = type;
            this.args = args;
        }
        public New(TypeInfo type, Arguments args, IEnumerable<PropertyInfo> properties)
        {
            this.type = type;
            this.args = args;
            this.properties.AddRange(properties);
        }


        public void AddProperty(Identifier propertyName, Value value)
        {
            var property = new PropertyInfo(propertyName, value);
            AddProperty(property);
        }

        public void AddProperty(PropertyInfo property)
        {
            if (properties.Find(x => x.PropertyName == property.PropertyName) != null)
                throw new Exception($"duplicated property name:{property.PropertyName}");

            properties.Add(property);
        }

        protected override void BuildBlock(CodeBlock block)
        {
            if (properties == null || properties.Count() == 0)
            {
                if (args != null)
                    block.Append($"new {type}({args})");
                else
                    block.Append($"new {type}()");

                return;
            }

            if (args != null)
                block.Append($"new {type}({args})");
            else
                block.Append($"new {type}");

            WriteProperties(block);
        }

        private void WriteProperties(CodeBlock block)
        {
            switch (Format)
            {
                case ValueOutputFormat.SingleLine:
                    block.Append(" { ");
                    properties.ForEach(
                         property =>
                         {
                             block.Append($"{property.PropertyName} = {property.Value}");
                         },
                         _ => block.Append(", ")
                         );

                    block.Append(" }");
                    break;

                default:
                    block.Begin();
                    properties.ForEach(
                          property =>
                          {
                              block.AppendLine();
                              block.Append($"{property.PropertyName} =  = {property.Value}");
                          },
                           _ => block.Append(",")
                        );

                    block.End();
                    break;
            }
        }
    }
}
