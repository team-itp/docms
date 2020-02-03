using System;

namespace Docms.Web.Extensions
{
    public static class DateTimeExensions
    {
        public static string ToRelativeString(this DateTime dt)
        {
            var jst = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            var localDt = TimeZoneInfo.ConvertTimeFromUtc(dt.ToUniversalTime(), jst);
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, jst);
            TimeSpan span = (localNow - localDt);

            // Normalize time span
            var future = false;
            if (span.TotalSeconds < 0)
            {
                // In the future
                span = -span;
                future = true;
            }

            // Test for Now
            var totalSeconds = span.TotalSeconds;
            if (totalSeconds < 0.9)
            {
                return "たった今";
            }

            // Date/time near current date/time
            var format = (future) ? "{0}{1}後" : "{0}{1}前";
            if (totalSeconds < 55)
            {
                // Seconds
                var seconds = Math.Max(1, span.Seconds);
                return string.Format(format, seconds, "秒");
            }

            if (totalSeconds < (55 * 60))
            {
                // Minutes
                var minutes = Math.Max(1, span.Minutes);
                return string.Format(format, minutes, "分");
            }
            if (totalSeconds < (24 * 60 * 60))
            {
                // Hours
                var hours = Math.Max(1, span.Hours);
                return string.Format(format, hours, "時間");
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
                format = string.Format(format, 2, "日");
            }
            else
            {
                // Absolute date
                if (localDt.Year == localNow.Year)
                    format = localDt.ToString(@"M/d");
                else
                    format = localDt.ToString(@"yyyy/M/d");
            }

            // Add time
            return string.Format("{0} {1:H:mm}", format, localDt);
        }
    }
}
