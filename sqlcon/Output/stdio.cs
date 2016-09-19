using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace sqlcon
{
    sealed class stdio
    {

        private static StreamWriter writer = null;
        static stdio()
        {
            string fileName = Context.GetValue<string>("log", "sqlcon.log");
            writer = fileName.NewStreamWriter();

          //  Console.CancelKeyPress += Console_CancelKeyPress;
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            Console.WriteLine();
            Console.WriteLine("exit application...");
        }

        public static void Close()
        {
            if(writer != null)
                writer.Close();
        }

        public static void OpenEditor(string fileName)
        {
            string editor = Context.GetValue<string>("editor", "notepad.exe");
            
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.ErrorDialog = true;
            process.StartInfo.UseShellExecute = false;
            //process.StartInfo.WorkingDirectory = startin;
            process.StartInfo.FileName = editor;
            process.StartInfo.Arguments = fileName;
            if (!File.Exists(fileName))
            {
                stdio.ErrorFormat("file not found: {0}", fileName);
                return;
            }

            try
            {
                process.Start();
            }
            catch (Exception ex)
            {
                stdio.ErrorFormat("failed to lauch application: {0} {1}, {2}", editor, fileName, ex.Message);
            }
        }

        public static void Write(string format, params object[] args)
        {
            Console.Write(format, args);

            if (writer != null)
            {
                writer.Write(format, args);
                writer.Flush();
            }
        }

        public static void WriteLine()
        {
            WriteLine("");
        }

        public static void WriteLine(string value)
        {
            Console.WriteLine(value);

            if (writer != null)
            {
                writer.WriteLine(value);
                writer.Flush();
            }
        }

        public static void WriteLine(string format, params object[] args)
        {
            Console.WriteLine(format, args);
            writer.WriteLine(format, args);
            writer.Flush();
        }

        public static void DisplayTitle(string value)
        {
            var keep = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(value);
            Console.ForegroundColor =keep;

            if (writer != null)
            {
                writer.WriteLine(value);
                writer.Flush();
            }
        }

        public static void ErrorFormat(string format, params object[] args)
        {
            if (args.Length == 0)
                Error(format);
            else
            {
                string text = string.Format(format, args);
                Error(text);
            }
        }

        public static void Error(string text)
        {
            var keep = Console.ForegroundColor;
            //Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(text);
            Console.ForegroundColor = keep;

            if (writer != null)
            {
                writer.WriteLine(text);
                writer.Flush();
            }
        }

        public static void TrimWriteLine(string value)
        {
            int w = Console.WindowWidth;

            if (value.Length > w)
                Console.WriteLine(value.Substring(0, w - 1));
            else
                Console.WriteLine(value);

            if (writer != null)
            {
                writer.WriteLine(value);
                writer.Flush();
            }
        }


        public static ConsoleKey ReadKey()
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey();

            if (writer != null)
            {
                writer.Write(keyInfo.Key.ToString());
                writer.Flush();
            }

            return keyInfo.Key;
        }

        public static string ReadTabLine(ITabCompletion completion)
        {

            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            StringBuilder builder = new StringBuilder();

            while (keyInfo.Key != ConsoleKey.Enter)
            {
                char ch = keyInfo.KeyChar;

                switch (keyInfo.Key)
                {
                    case ConsoleKey.Spacebar:
                        break;

                    case ConsoleKey.Tab:
                        completion.TabCandidates(builder.ToString());
                        break;


                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.RightArrow:
                        break;

                    case ConsoleKey.Insert:
                        break;

                    case ConsoleKey.Backspace:
                    case ConsoleKey.Delete:
                        break;
                }

                    
                builder.Append(ch);
                if (writer != null)
                {
                    writer.Write(ch);
                    writer.Flush();
                }

                keyInfo = Console.ReadKey();
            } ;

            if (writer != null)
            {
                writer.WriteLine();
                writer.Flush();
            }

            return builder.ToString();
        }


        public static string ReadLine()
        {
            string line = Console.ReadLine();

            if (writer != null)
            {
                writer.WriteLine(line);
                writer.Flush();
            }

            return line;
        }

        /// <summary>
        /// return true if answer is YES
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool YesOrNo(string format, params object[] args)
        {
            stdio.Write(format, args);
            if (stdio.ReadKey() != ConsoleKey.Y)
            {
                return false;
            }

            stdio.WriteLine();
            return true;
        }

      
    }
}
