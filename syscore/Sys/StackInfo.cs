using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace Sys
{
    class StackInfo
    {
        public string FileName { get; } = string.Empty;
        public int LineNumber { get; } = 0;
        public string ObjectName { get; } = "Unknown";
        public string MethodName { get; } = "Unknown";

        public StackInfo()
        {
            StackTrace st = new StackTrace();

            StackFrame sf = new StackFrame(4, true);

            if (sf == null)
                return;

            try
            {
                int p;

                LineNumber = sf.GetFileLineNumber();

                System.Reflection.MethodBase method = sf.GetMethod();
                if (method != null)
                {
                    if (method.ReflectedType != null)
                        ObjectName = method.ReflectedType.ToString();
                    else
                        ObjectName = "global";

                    MethodName = method.ToString();
                    p = MethodName.IndexOf(" ") + 1;
                    MethodName = MethodName.Substring(p, MethodName.Length - p);
                }

                FileName = sf.GetFileName();
                if (FileName != null)
                {
                    p = FileName.LastIndexOf(@"\") + 1;
                    FileName = FileName.Substring(p, FileName.Length - p);
                }

            }
            catch
            {
            }
        }

        public override string ToString()
        {
            return $"File:{FileName} line:{LineNumber} {ObjectName}.{MethodName}";
        }
    }
}
