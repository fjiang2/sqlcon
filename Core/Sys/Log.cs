using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Sys
{
    public class Log
    {
        private static ReaderWriterLock rwLock = new ReaderWriterLock();
        private static int lockTimeout = 5 * 1000;
        private static string path = "sys.log";

        private static void Write(string path, string text)
        {
            try
            {
                rwLock.AcquireWriterLock(lockTimeout);

                int attempts = 0;
                StreamWriter writer = null;

                L1:
                try
                {
                    if (attempts < 3)
                        writer = File.AppendText(path);
                    else
                        return;
                }
                catch (Exception)
                {
                    attempts++;
                    goto L1;
                }


                if (writer == null)
                    return;

                try
                {
                    writer.WriteLine(text);
                }
                catch (Exception)
                {
                }
                finally
                {
                    writer.Close();
                }

            }
            catch (Exception)
            {
            }
            finally
            {
                rwLock.ReleaseWriterLock();
            }
        }


        public static void Error(string text)
        {
            text = History(text);
            Write(path, text);
        }

        public static void Error(string text, Exception ex)
        {

        }

        public static void Warn(string text)
        {

        }


        private static string History(string text)
        {
            string now = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.ffff");
            return $"{now} {text}";
        }



        private string GetStackInfo(SeverityLevel level)
        {
            StringBuilder sb = new StringBuilder();
            var stackInfo = new StackInfo();
            sb.Append($"\t-{level}-\t")
            .Append("T[")
            .Append(Thread.CurrentThread.GetHashCode().ToString())
            .Append("]\t")
            .Append($"{stackInfo}")
            .Append(": ");


            return sb.ToString();
        }
    }
}
