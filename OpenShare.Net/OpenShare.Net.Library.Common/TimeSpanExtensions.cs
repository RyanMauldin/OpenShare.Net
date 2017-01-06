using System;
using System.Text;

namespace OpenShare.Net.Library.Common
{
    public static class TimeSpanExtensions
    {
        public static string ToFriendlyString(this TimeSpan value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            const string negativeSign = "-";
            var isNegative = false;
            var timeSpan = new TimeSpan(value.Ticks);
            var builder = new StringBuilder(64);

            if (value.TotalMilliseconds < 0)
            {
                isNegative = true;
                timeSpan = timeSpan.Negate();
            }

            if (timeSpan.Days > 0)
                builder.Append(
                    isNegative
                        ? timeSpan.Days == 1
                            ? $"{negativeSign}{timeSpan.Days} day, "
                            : $"{negativeSign}{timeSpan.Days} days, "
                        : timeSpan.Days == 1
                            ? $"{timeSpan.Days} day, "
                            : $"{timeSpan.Days} days, ");
            else
                builder.Append(
                    isNegative
                        ? negativeSign
                        : string.Empty);

            builder.Append($"{timeSpan.Hours:d2}:{timeSpan.Minutes:d2}:{timeSpan.Seconds:d2}.{timeSpan.Milliseconds:d3}");

            return builder.ToString();
        }
    }
}
