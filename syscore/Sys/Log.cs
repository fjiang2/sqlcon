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
        private static ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();
        private static int timeout = 5 * 1000;
        public static string Path { get; set; } = "sys.log";

        private static bool Append(string path, string text)
        {
            if (rwLock.TryEnterWriteLock(timeout))
            {
                try
                {
                    File.AppendAllText(path, text);
                }
                catch (Exception)
                {
                }
                finally
                {
                    rwLock.ExitWriteLock();
                }
                return true;
            }
            else
            {
                return false;
            }
        }


        public static void Error(string text)
        {
            text = History(SeverityLevel.Error, text);
            Append(Path, text);
        }

        public static void Error(string text, Exception ex)
        {
            text = History(SeverityLevel.Error, text);
            StringBuilder builder = new StringBuilder(text);
            builder.AppendLine(ex.AllMessages())
                .AppendLine(ex.StackTrace);
            Append(Path, builder.ToString());
        }

        public static void Warn(string text)
        {
            text = History(SeverityLevel.Warn, text);
            Append(Path, text);
        }

        public static void Info(string text)
        {
            text = History(SeverityLevel.Information, text);
            Append(Path, text);
        }

        public static void Debug(string text)
        {
            text = History(SeverityLevel.Debug, text);
            Append(Path, text);
        }

        private static string History(SeverityLevel level, string text)
        {
            string now = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.ffff");

            int threadId = Thread.CurrentThread.ManagedThreadId;
            var stackInfo = new StackInfo();
            return $"{now} [{threadId}] {level} {stackInfo} - {text} {Environment.NewLine}";
        }
    }
}
