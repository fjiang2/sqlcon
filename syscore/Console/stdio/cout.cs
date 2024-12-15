using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Stdio
{
    public class Cout
    {
        /// <summary>
        /// turn command-echoing on/off on the screen
        /// </summary>
        public static bool echo { get; set; } = true;
        private static int WindowWidth { get; } = 80;

        static Cout()
        {
            if (!Console.IsOutputRedirected && IsConsole)
                WindowWidth = Console.BufferWidth;
        }

        public static bool IsConsole => Environment.UserInteractive && Console.OpenStandardInput(1) != System.IO.Stream.Null;

        public static void Write(string text)
        {
            if (echo)
                Console.Write(text);

            Clog.Write(text);
        }

        public static void WriteLine(string text)
        {
            if (echo)
                Console.WriteLine(text);

            Clog.WriteLine(text);
        }

        public static void WriteLine()
        {
            WriteLine(string.Empty);
        }

        public static void WriteLine(string format, params object[] args)
        {
            string text = string.Format(format, args);
            WriteLine(text);
        }

        public static void WriteLine(ConsoleColor color, string text)
        {
            var keep = Console.ForegroundColor;
            Console.ForegroundColor = color;
            WriteLine(text);
            Console.ForegroundColor = keep;
        }


        public static void TrimWriteLine(string text)
        {
            if (echo)
            {
                int w = -1;
                if (!Console.IsOutputRedirected && IsConsole)
                    w = Console.BufferWidth;

                if (w != -1 && text.Length > w)
                    Console.WriteLine(text.Substring(0, w - 1));
                else
                    Console.WriteLine(text);
            }

            Clog.WriteLine(text);
        }


    }
}
