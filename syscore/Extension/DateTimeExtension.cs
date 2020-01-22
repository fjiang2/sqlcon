using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sys
{
    public static class DateTimeExtension
    {

        public static string TimeAgoStamp(this DateTime time)
        {
            return string.Format("{0} ago | {1}", AgoStamp(time), TimeStamp(time));
        }

        public static string TimeStamp(this DateTime time)
        {
            return string.Format("{0:MMM dd}, {0:yyyy} @ {0:t}", time);
        }

        public static string AgoStamp(this DateTime time)
        {

            TimeSpan span = DateTime.Now - time;

            if (span.Days == 0)
            {
                if (span.Hours == 0)
                {
                    if (span.Minutes == 0)
                        return "less 1 minute";
                    else if (span.Minutes == 1 && span.TotalMinutes > 1.0)
                        return "1 minute";
                    else
                        return string.Format("{0} minutes", span.Minutes);
                }
                else if (span.Hours == 1 && span.TotalHours > 1.0)
                    return "1 hour";
                else
                    return string.Format("{0} hours", span.Hours);
            }

            else if (span.Days == 1 && span.TotalDays > 1.0)
                return "1 day";

            else if (span.Days > 1 && span.Days < 30)
                return string.Format("{0} days", span.Days);

            else if (span.Days >= 30 && span.Days < 60)
                return "1 month";

            else
                return string.Format("{0} months", span.Days / 30);
        }

    }
}
