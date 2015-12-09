using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sqlcon
{
    enum CancelableState
    {
        NotStarted,
        Cancelled,
        Completed,
        OnError
    }

    class CancelableWork
    {
        /// <summary>
        /// use ctrl-c to break long running work, return true if cancelled
        /// </summary>
        /// <param name="work"></param>
        /// <param name="message">interrupted message</param>
        /// <returns>return true: cancelled, return false: not cancelled</returns>
        public static CancelableState CanCancel(Func<Func<bool>, CancelableState> work, string message = null)
        {
            CancelableState cancelled = CancelableState.NotStarted;
            ConsoleCancelEventHandler cancelKeyPress = (sender, e) =>
                {
                    e.Cancel = true;
                    cancelled = CancelableState.Cancelled;
                    Console.WriteLine("command interrupting...");
                };

            Console.CancelKeyPress += cancelKeyPress;

            CancelableState result;
            try
            {
                result = work(() => cancelled == CancelableState.Cancelled);
            }
            finally
            {
                Console.CancelKeyPress -= cancelKeyPress;
            }

            if (result == CancelableState.Cancelled)
            {
                if (message == null)
                    message = "command interrupted";
                Console.WriteLine();
                Console.WriteLine(message);
            }

            return result;
        }
    }
}
