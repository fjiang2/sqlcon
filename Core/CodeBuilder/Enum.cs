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
using System.Linq;
using System.Text;

namespace Sys.CodeBuilder
{
    public class Enum : Declare, ICodeBlock
    {
        public List<Feature> features { get; } = new List<Feature>();


        public Enum(string enumName)
            : base(enumName)
        {
            type = new TypeInfo { userType = "enum" };
        }

        public void Add(string feature)
        {
            this.Add(new Feature(feature));
        }

        public void Add(string feature, int value)
        {
            this.Add(new Feature(feature) { value = value });
        }

        public void Add(Feature feature)
        {
            features.Add(feature);
            feature.Parent = this;
        }

        public void Add(string feature, int value, string label)
        {
            var _feature = new Feature(feature) { value = value };
            if (label != null)
            {
                _feature.AddAttribute(new AttributeInfo("Description") { args = new string[] { label } });
            }

            this.Add(_feature);
        }

        protected override void BuildBlock(CodeBlock block)
        {
            base.BuildBlock(block);

            block.AppendLine(Signature);
            var body = new CodeBlock();

            features.ForEach(
                    item => body.Add(item),
                    item =>
                    {
                        if (item.Count == 1)
                            return;

                        body.AppendLine();
                    }
                    );

            block.AddWithBeginEnd(body);
        }
    }


}
