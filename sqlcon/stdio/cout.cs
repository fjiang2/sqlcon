using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sqlcon
{
    class cout
    {
        /// <summary>
        /// turn command-echoing on/off
        /// </summary>
        public static bool echo { get; set; } = true;


        public static void Write(string text)
        {
            if (echo)
                Console.Write(text);

            clog.Write(text);
        }



        public static void WriteLine(string text)
        {
            if (echo)
                Console.WriteLine(text);

            clog.WriteLine(text);
        }


        public static void Write(char ch)
        {
            string text = ch.ToString();
            Write(text);
        }


        public static void WriteLine()
        {
            WriteLine("");
        }

        public static void WriteLine(string format, params object[] args)
        {
            string text = string.Format(format, args);
            WriteLine(text);
        }

        public static void DisplayTitle(string text)
        {
            var keep = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            WriteLine(text);
            Console.ForegroundColor = keep;
        }

     
        public static void TrimWriteLine(string text)
        {
            if (echo)
            {
                int w = -1;
                if (!Console.IsOutputRedirected)
                    w = Console.BufferWidth;

                if (w != -1 && text.Length > w)
                    Console.WriteLine(text.Substring(0, w - 1));
                else
                    Console.WriteLine(text);
            }

            clog.WriteLine(text);
        }


    }
}
