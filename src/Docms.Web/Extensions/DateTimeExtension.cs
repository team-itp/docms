using System;

namespace Docms.Web.Extensions
{
    public static class DateTimeExensions
    {
        public static string ToRelativeString(this DateTime dt)
        {
            TimeSpan span = (DateTime.Now - dt);

            // Normalize time span
            bool future = false;
            if (span.TotalSeconds < 0)
            {
                // In the future
                span = -span;
                future = true;
            }

            // Test for Now
            double totalSeconds = span.TotalSeconds;
            if (totalSeconds < 0.9)
            {
                return "たった今";
            }

            // Date/time near current date/time
            string format = (future) ? "{0}{1}後" : "{0}{1}前";
            if (totalSeconds < 55)
            {
                // Seconds
                int seconds = Math.Max(1, span.Seconds);
                return String.Format(format, seconds, "秒");
            }

            if (totalSeconds < (55 * 60))
            {
                // Minutes
                int minutes = Math.Max(1, span.Minutes);
                return String.Format(format, minutes, "分");
            }
            if (totalSeconds < (24 * 60 * 60))
            {
                // Hours
                int hours = Math.Max(1, span.Hours);
                return String.Format(format, hours, "時間");
            }

            // Format both date and time
            if (totalSeconds < (48 * 60 * 60))
            {
                // 1 Day
                format = (future) ? "明日" : "昨日";
            }
            else if (totalSeconds < (3 * 24 * 60 * 60))
            {
                // 2 Days
                format = String.Format(format, 2, "日");
            }
            else
            {
                // Absolute date
                if (dt.Year == DateTime.Now.Year)
                    format = dt.ToString(@"M/d");
                else
                    format = dt.ToString(@"yyyy/M/d");
            }

            // Add time
            return String.Format("{0} {1:H:mm}", format, dt);
        }
    }
}
