using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace sqlcon
{
   
    class CancelableWork
    {
        public static void CanCancel(Action<CancellationTokenSource> work, string message = null)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            ConsoleCancelEventHandler cancelKeyPress = (sender, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                    Console.WriteLine("command interrupting...");
                };

            Console.CancelKeyPress += cancelKeyPress;

            
            try
            {
                work(cts);
            }
            finally
            {
                Console.CancelKeyPress -= cancelKeyPress;
            }

            if (cts.Token.IsCancellationRequested)
            {
                if (message == null)
                    message = "command interrupted";
                Console.WriteLine();
                Console.WriteLine(message);
            }

            cts.Dispose();

        }
    }
}
