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
        //   public int tab { get; set; }

        //current tab number
        private int curruent = 0;

        private readonly List<CodeLine> lines = new List<CodeLine>();


        public CodeBlock()
        {
        }

        public int Count
        {
            get { return lines.Count; }
        }

        public void Clear()
        {
            this.curruent = 0;
            lines.Clear();
        }

        public CodeBlock Indent()
        {
            curruent++;
            return this;
        }

        public CodeBlock Unindent()
        {
            curruent--;
            return this;
        }

        public void Add(CodeBlock block, int indent = 0)
        {
            foreach (var line in block.lines)
            {
                line.tab += indent;
                lines.Add(line);
            }
        }

        public void Add(IBuildable block)
        {
            Add(block.GetBlock(), 0);
        }

        public void AddWithBeginEnd(CodeBlock block)
        {
            Begin();
            Add(block, curruent);
            End();
        }

        public CodeBlock WrapByBeginEnd()
        {
            foreach (var line in lines)
            {
                line.tab++;
            }
            Insert("{");
            AppendLine("}");
            return this;
        }

        public CodeBlock Begin(string str = null)
        {
            if (str == null)
                AppendLine("{");
            else
                AppendLine(str + "{");

            curruent++;

            return this;
        }

        public CodeBlock End(string str = null)
        {
            curruent--;
            if (str == null)
                AppendLine("}");
            else
                AppendLine("}" + str);

            return this;
        }

        public CodeBlock Append(string str)
        {
            CodeLine line = null;
            if (lines.Count != 0)
                line = lines.Last();

            if (line != null)
                line.line += str;
            else
                AppendLine(str);
            return this;
        }

        public void Insert(string str, int index = 0)
        {
            var line = new CodeLine { tab = curruent, line = str };
            lines.Insert(index, line);
        }

        public CodeBlock AppendLine()
        {
            lines.Add(new CodeLine { tab = curruent });

            return this;
        }

        public CodeBlock AppendLine(string str, int indent)
        {
            this.curruent += indent;
            AppendLine(str);
            this.curruent -= indent;

            return this;
        }

        public CodeBlock AppendLine(string str)
        {
            if (str.IndexOf(Environment.NewLine) > 0)
            {
                var items = str.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in items)
                    lines.Add(new CodeLine { tab = curruent, line = item });
            }
            else
                lines.Add(new CodeLine { tab = curruent, line = str });

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

        public string ToString(int indent)
        {
            return string.Join(Environment.NewLine, lines.Select(line => CodeLine.Tab(indent) + line.ToString()));
        }

    }
}
