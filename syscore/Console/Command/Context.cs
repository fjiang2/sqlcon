using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tie;

namespace Sys.Stdio
{
    public static class Context
    {
        public const string DATAREADER = "DataReader";
        public const string SHELL = "$SHELL";

        public static Memory DS = new Memory();

        static Context()
        {
            Script.FunctionChain.Add(functions);

            DS.Add(DATAREADER, new VAL(false));
        }

        public static void Execute(string statement)
        {
            Script.Execute(statement, DS);
        }

        public static VAL Evaluate(string expression)
        {
            return Script.Evaluate(expression, DS);
        }

        public static T GetValue<T>(VAR variable, T defaultValue = default(T))
        {
            VAL val = DS[variable];

            if (val.Defined && val.HostValue is T)
                return (T)val.HostValue;
            else
                return defaultValue;
        }

        public static void SetValue(VAR variable, object value)
        {
            DS.AddObject(variable, value);
        }

        public static void SetHostValue(VAR variable, object value)
        {
            DS.AddHostObject(variable, value);
        }

        public static VAL binding
        {
            get
            {
                return DS["binding"];
            }
        }


        static VAL functions(string func, VAL parameters, Memory DS)
        {
            int size = parameters.Size;
            VAL L0 = size > 0 ? parameters[0] : null;
            VAL L1 = size > 1 ? parameters[1] : null;
            VAL L2 = size > 2 ? parameters[2] : null;


            switch (func)
            {
                //run("command");
                case "run":
                    {
                        string line = null;
                        if (size == 1 && L0.VALTYPE == VALTYPE.stringcon)
                        {
                            line = L0.Str;
                        }
                        else
                        {
                            cerr.WriteLine("invalid arguments on function void run(string)");
                        }

                        if (line != null)
                        {
                            IShell shell = DS[SHELL].Value as IShell;
                            if (shell != null)
                            {
                                int result = (int)shell.Run(line);
                                return new VAL(result);
                            }
                            else
                            {
                                cerr.WriteLine("shell not found");
                                return new VAL();
                            }
                        }
                    }
                    break;

                default:
                    var query = DS[func];
                    if (query.VALTYPE == VALTYPE.stringcon)
                    {
                        VAL val = VAL.Array(0);
                        for (int i = 0; i < parameters.Size; i++)
                        {
                            VAL parameter = parameters[i];
                            string name = parameter.GetName();

                            if (name == null)
                            {
                                cout.WriteLine("require parameter name at arguments({0}), run func(id=20,x=2);", i + 1);
                                return new VAL(2);
                            }
                            val.AddMember(name, parameter);
                        }

                        VAL result = VAL.Array(0);
                        result.Add(query);
                        result.Add(val);
                        return result;
                    }
                    break;
            }

            return null;

        }
    }
}
