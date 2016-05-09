using System;

namespace OpenShare.Net.Library.Common
{
    public static class DateTimeExtensions
    {
        public static DateTime StartOfDay(this DateTime value)
        {
            return value.Date;
        }

        public static DateTime EndOfDay(this DateTime value)
        {
            return value == DateTime.MaxValue
                ? value.AddMilliseconds(-3)
                : value.Date.AddDays(1).AddMilliseconds(-3);
        }

        public static string ToLongDateTimeString(this DateTime value)
        {
            return string.Format(
                "{0}/{1}/{2} {3}:{4}:{5}.{6}",
                value.Year.ToString("D4"),
                value.Month.ToString("D2"),
                value.Day.ToString("D2"),
                value.Hour.ToString("D2"),
                value.Minute.ToString("D2"),
                value.Second.ToString("D2"),
                value.Millisecond);
        }
    }
}
