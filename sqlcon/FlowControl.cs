using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tie;
using Sys.Stdio;

namespace SqlCon
{
    class FlowControl
    {
        private const string COLON = ":";
        private const string GOTO = "goto";
        private const string IF = "if";


        private string[] lines;
        private Dictionary<string, int> anchors = new Dictionary<string, int>();

        private int SP = 0;


        public FlowControl(string[] lines)
        {
            this.lines = lines;
        }

        public static bool IsFlowStatement(string line)
        {
            return line.StartsWith(COLON) || line.StartsWith(IF) || line.StartsWith(GOTO);
        }

        public NextStep Execute(Func<string, NextStep> run)
        {
            NextStep next;

            while (SP < lines.Length)
            {
                string line = GetLine();
                if (IsFlowStatement(line))
                {
                    //ERROR|COMPLETED|NEXT
                    next = Execute();
                }
                else
                {
                //ERROR|COMPLETED|CONTINUE|EXIT
                L2:
                    next = run(line);
                    ++SP;
                    if (next == NextStep.CONTINUE)
                    {
                        line = GetLine();
                        if (string.IsNullOrEmpty(line))
                            return NextStep.ERROR;

                        goto L2;
                    }
                }

                switch (next)
                {
                    case NextStep.COMPLETED:
                        break;

                    case NextStep.EXIT:
                        return NextStep.EXIT;

                    case NextStep.ERROR:
                        if (OnError())
                            return NextStep.ERROR;
                        break;
                }
            }

            return NextStep.EXIT;
        }

        private string GetLine()
        {
            if (SP >= lines.Length)
            {
                Cerr.WriteLine("command is not completed. Letter ';' expected.");
                return string.Empty;
            }

            string line = lines[SP];
            Cout.WriteLine(ConsoleColor.DarkGray, line);
            return line;
        }

        private bool OnError()
        {
            if (SP >= lines.Length)
                return true;

            if (!Cin.YesOrNo($"continue to run \"{lines[SP]}\" (y/n)?"))
            {
                Cerr.WriteLine("interupted.");
                return true;
            }

            return false;
        }

        private NextStep Execute()
        {
            string line = lines[SP];

            if (line.StartsWith(COLON))
            {
                string label = line.Substring(1).Trim();
                if (anchors.ContainsKey(label))
                    anchors[label] = SP;
                else
                    anchors.Add(label, SP);

                SP++;
                return NextStep.COMPLETED;
            }

            if (line.StartsWith(GOTO))
            {
                string label = line.Substring(4).Trim();
                return Goto(label);
            }

            if (line.StartsWith(IF))
            {
                string _line = line.Substring(2).Trim();
                string[] L = _line.Split(new string[] { GOTO }, StringSplitOptions.RemoveEmptyEntries);
                if (L.Length != 2)
                {
                    Cerr.WriteLine($"syntax error: {line}");
                    return NextStep.ERROR;
                }

                string expr = L[0].Trim();
                string label = L[1].Trim();

                try
                {
                    VAL result = Script.Evaluate(expr, Context.DS);
                    if (result.IsBool && result.Boolcon || result.IsInt && result.Intcon != 0)
                    {
                        return Goto(label);
                    }
                }
                catch (Exception ex)
                {
                    Cerr.WriteLine($"error on: {expr},  {ex.Message}");
                    return NextStep.ERROR;
                }

                SP++;
                return NextStep.COMPLETED;
            }

            return NextStep.NEXT;
        }

        private NextStep Goto(string label)
        {
            if (label.IndexOf(' ') >= 0)
            {
                Cerr.WriteLine($"invalid goto label: {label}");
                return NextStep.ERROR;
            }

            if (anchors.ContainsKey(label))
            {
                SP = anchors[label];
                return NextStep.COMPLETED;
            }
            else
            {
                Cerr.WriteLine($"undefined goto label: {label}");
                return NextStep.ERROR;
            }
        }

    }
}
