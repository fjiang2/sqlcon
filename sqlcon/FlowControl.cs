using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tie;

namespace sqlcon
{
    class FlowControl
    {
        private const string GOTO = "goto";
        private const string IF = "if";

        private string[] lines;
        private Dictionary<string, int> anchors = new Dictionary<string, int>();

        private int SP = 0;


        public FlowControl(string[] lines)
        {
            this.lines = lines;
        }

        public NextStep Execute(Func<string, NextStep> run)
        {
            while (SP < lines.Length)
            {
                cout.WriteLine(ConsoleColor.DarkGray, lines[SP]);
                var next = Execute();
                switch (next)
                {
                    case NextStep.ERROR:
                        if (!cin.YesOrNo($"continue to run \"{lines[SP]}\" (y/n)?"))
                        {
                            cerr.WriteLine("interupted.");
                            return NextStep.ERROR;
                        }
                        return NextStep.ERROR;

                    case NextStep.COMPLETED:
                        break;

                    case NextStep.NEXT:
                        {
                        L2:
                            next = run(lines[SP]);
                            SP++;
                            switch (next)
                            {
                                case NextStep.EXIT:
                                    return NextStep.EXIT;

                                case NextStep.CONTINUE:
                                    cout.WriteLine(ConsoleColor.DarkGray, lines[SP]);
                                    goto L2;
                            }
                        }
                        break;
                }
            }

            return NextStep.EXIT;
        }


        private NextStep Execute()
        {
            string line = lines[SP];

            if (line.StartsWith(":"))
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
                    cerr.WriteLine($"syntax error: {line}");
                    return NextStep.ERROR;
                }

                string expr = L[0].Trim();
                string label = L[1].Trim();

                VAL result = Script.Evaluate(expr, Context.DS);
                if (result.IsBool && result.Boolcon || result.IsInt && result.Intcon != 0)
                {
                    return Goto(label);
                }

                SP++;
                return NextStep.COMPLETED;
            }

            return NextStep.NEXT;
        }

        private NextStep Goto(string label)
        {
            if (anchors.ContainsKey(label))
            {
                SP = anchors[label];
                return NextStep.COMPLETED;
            }
            else
            {
                cerr.WriteLine($"invalid label: {label}");
                return NextStep.ERROR;
            }
        }

    }
}
