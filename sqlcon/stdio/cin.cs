using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sqlcon
{
    public class cin
    {

        public static ConsoleKey ReadKey()
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey();

            clog.Write(keyInfo.KeyChar.ToString());

            return keyInfo.Key;
        }


        public static string ReadLine()
        {
            string line = Console.ReadLine();

            clog.WriteLine(line);

            return line;
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
                cout.Write(ch.ToString());

                keyInfo = Console.ReadKey();
            };

            cout.WriteLine();

            return builder.ToString();
        }



        /// <summary>
        /// return true if answer is YES
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool YesOrNo(string text)
        {
            cout.Write(text);
            if (ReadKey() != ConsoleKey.Y)
            {
                return false;
            }

            cout.WriteLine();
            return true;
        }

        public static bool IsKeyPressed(ConsoleKey key)
        {
            return Console.KeyAvailable && Console.ReadKey(true).Key == key;
        }

    }
}
