using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Sys.Stdio
{
    public class clog
    {
        private static StreamWriter writer = null;

        static clog()
        {
            string fileName = Context.GetValue<string>("log", "clog.log");
            writer = NewStreamWriter(fileName);
        }

        ~clog()
        {
            if (writer != null)
                writer.Close();
        }


        private static StreamWriter NewStreamWriter(string fileName)
        {
            try
            {
                string folder = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
            }
            catch (ArgumentException)
            {
            }

            return new StreamWriter(fileName);
        }

        public static void Write(string text)
        {
            if (writer == null)
                return;


            writer.Write(text);
            writer.Flush();
        }

        public static void WriteLine(string text)
        {
            if (writer == null)
                return;


            writer.WriteLine(text);
            writer.Flush();
        }
       
    }
}
