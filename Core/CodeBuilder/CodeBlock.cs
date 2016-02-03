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
    public class CodeBlock
    {
        private int tab = 0;
        private List<CodeLine> lines = new List<CodeLine>();


        public CodeBlock()
        {
        }

        public int Count
        {
            get { return lines.Count; }
        }

        public void Clear()
        {
            this.tab = 0;
            lines.Clear();
        }

        public void Add(CodeBlock block, int indent)
        {
            foreach (var line in block.lines)
            {
                line.tab += indent;
                lines.Add(line);
            }
        }

        public void Add(ICodeBlock block, int indent)
        {
            Add(block.GetBlock(), indent);
        }

        public void AddBeginEnd(CodeBlock block)
        {
            Begin();
            Add(block, tab);
            End();
        }

        public CodeBlock Begin(string str = null)
        {
            if (str == null)
                AppendLine("{");
            else
                AppendLine(str + "{");

            tab++;

            return this;
        }

        public CodeBlock End(string str = null)
        {
            tab--;
            if (str == null)
                AppendLine("}");
            else
                AppendLine("}" + str);

            return this;
        }

        public CodeBlock Append(string str)
        {
            var line = lines.Last();
            if (line != null)
                line.line += str;
            else
                AppendLine(str);
            return this;
        }

        public void Insert(string str, int index = 0)
        {
            var line = new CodeLine { tab = tab, line = str };
            lines.Insert(index, line);
        }

        public CodeBlock AppendLine()
        {
            lines.Add(CodeLine.EmptyLine);

            return this;
        }

        public CodeBlock AppendLine(string str, int indent)
        {
            this.tab += indent;
            AppendLine(str);
            this.tab -= indent;

            return this;
        }

        public CodeBlock AppendLine(string str)
        {
            if (str.IndexOf(Environment.NewLine) > 0)
            {
                var items = str.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in items)
                    lines.Add(new CodeLine { tab = tab, line = item });
            }
            else
                lines.Add(new CodeLine { tab = tab, line = str });

            return this;
        }


        public CodeBlock AppendFormat(string format, params object[] args)
        {
            AppendLine(string.Format(format, args));

            return this;
        }


        public override string ToString()
        {
            return string.Join(Environment.NewLine, lines);
        }

    }
}
