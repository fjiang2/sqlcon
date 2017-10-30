using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace sqlcon
{
    class clog
    {
        private static StreamWriter writer = null;

        static clog()
        {
            string fileName = Context.GetValue<string>("log", "sqlcon.log");
            writer = fileName.NewStreamWriter();
        }

        ~clog()
        {
            if (writer != null)
                writer.Close();
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
