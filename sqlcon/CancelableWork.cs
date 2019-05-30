using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sys.Stdio;

namespace sqlcon
{
   
    class CancelableWork
    {
        public static void CanCancel(Action<CancellationToken> work, string message = null)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            ConsoleCancelEventHandler cancelKeyPress = (sender, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                    cout.WriteLine("command interrupting...");
                };

            Console.CancelKeyPress += cancelKeyPress;

            
            try
            {
                work(cts.Token);
            }
            finally
            {
                Console.CancelKeyPress -= cancelKeyPress;
            }

            if (cts.Token.IsCancellationRequested)
            {
                if (message == null)
                    message = "command interrupted";
                cout.WriteLine();
                cout.WriteLine(message);
            }

            cts.Dispose();

        }
    }
}
