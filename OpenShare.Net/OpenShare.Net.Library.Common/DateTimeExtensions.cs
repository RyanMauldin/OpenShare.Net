using System;

namespace OpenShare.Net.Library.Common
{
    public static class DateTimeExtensions
    {
        private const double OneDayInMilliseconds = 24d * 60d * 60d * 1000d;
        private const double OneMillisecondInTicks = 10000;
        private const double OneDayInTicks = OneDayInMilliseconds * OneMillisecondInTicks;

        /// <summary>
        /// This method gets the Date portion of the DateTime paramerter <paramref name="value"/>, which effectively
        /// removes the time for that date.
        /// </summary>
        /// <param name="value">The value to get the start of the day from.</param>
        /// <returns>
        /// The start of the given day.
        /// </returns>
        public static DateTime StartOfDay(this DateTime value)
        {
            return value.Date;
        }

        /// <summary>
        /// This method gets the Date portion of the DateTime paramerter <paramref name="value"/>, which effectively
        /// removes the time for that date. If the paramater <paramref name="value"/>
        /// is provided as a null value, this method returns null.
        /// </summary>
        /// <param name="value">The value to get the start of the day from.</param>
        /// <returns>
        /// The start of the given day.
        /// </returns>
        public static DateTime? StartOfDay(this DateTime? value)
        {
            if (value == null)
                return null;

            return Convert.ToDateTime(value).StartOfDay();
        }

        /// <summary>
        /// This method converts the value of a DateTime object from Coordinated Universal Time (UTC) to the time zone specified in
        /// parameter <paramref name="timeZoneId"/>. Next the method gets the Date portion of the DateTime paramerter
        /// <paramref name="value"/>, which effectively removes the time for that date. Note that this method can throw an exception in the
        /// event that a given parameter <paramref name="timeZoneId"/> is not a valid system time zone, or when converting from UTC time
        /// to the given time zone, the date falls out of range from DateTime.MinDate. 
        /// </summary>
        /// <param name="value">The value to get the start of the day from.</param>
        /// <param name="timeZoneId">The .NET Time Zone specification, e.g. "Eastern Standard Time" represented as a TimeZoneInfo object.</param>
        /// <returns>
        /// The start of a given UTC day specified by the <paramref name="value"/> parameter, but represented in the time zone specified by parameter <paramref name="timeZoneId"/>.
        /// </returns>
        public static DateTime StartOfDayFromUtc(this DateTime value, string timeZoneId)
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var timeZoneDate = TimeZoneInfo.ConvertTimeFromUtc(value, timeZoneInfo);
            var startOfDay = timeZoneDate.StartOfDay();
            return startOfDay;
        }

        /// <summary>
        /// This method converts the value of a DateTime object from Coordinated Universal Time (UTC) to the time zone specified in
        /// parameter <paramref name="timeZoneId"/>. Next the method gets the Date portion of the DateTime paramerter
        /// <paramref name="value"/>, which effectively removes the time for that date. Note that this method can throw an exception in the
        /// event that a given parameter <paramref name="timeZoneId"/> is not a valid system time zone, or when converting from UTC time
        /// to the given time zone, the date falls out of range from DateTime.MinDate. If the paramater <paramref name="value"/>
        /// is provided as a null value, this method returns null.
        /// </summary>
        /// <param name="value">The value to get the start of the day from.</param>
        /// <param name="timeZoneId">The .NET Time Zone specification, e.g. "Eastern Standard Time" represented as a TimeZoneInfo object.</param>
        /// <returns>
        /// The start of a given UTC day specified by the <paramref name="value"/> parameter, but represented in the time zone specified by parameter <paramref name="timeZoneId"/>.
        /// </returns>
        public static DateTime? StartOfDayFromUtc(this DateTime? value, string timeZoneId)
        {
            if (value == null)
                return null;

            return Convert.ToDateTime(value).StartOfDayFromUtc(timeZoneId);
        }

        /// <summary>
        /// This method grabs the Date portion of the DateTime parameter <paramref name="value"/>, which effetively
        /// gives you the start of the day specified, and then adds the absolute value of the <paramref name="resolutionInMilliseconds"/>,
        /// converted into ticks. e.g. If parameter <paramref name="value"/> is passed DateTime.MaxValue, and a resolution of 0.0002,
        /// this will result in a returned DateTime with 3155378975999999998 ticks at 12/31/9999 and a time resolution of 23:59:59.9999998.
        /// This in turn does not equal DateTime.MaxValue without passing a resolution of 0.0001 or 0.
        /// <remarks>
        /// If you wish for your end of day to be the beginning of the same day, set <paramref name="resolutionInMilliseconds"/>
        /// to <seealso cref="double.MaxValue"/>, or set it to the appropriate time tolerance your application can handle. Do note that the
        /// <paramref name="resolutionInMilliseconds"/> can not exceed one day's worth of milliseconds: 86,400,000.
        /// </remarks>
        /// </summary>
        /// <param name="value">The value to get the end of the day from.</param>
        /// <param name="resolutionInMilliseconds">The resolution in milliseconds to remove from the start of the next day.</param>
        /// <returns>
        /// The end of the given day with respect to the resolution provided in milliseconds.
        /// </returns>
        public static DateTime EndOfDay(this DateTime value, double? resolutionInMilliseconds = 0d)
        {
            var concreteResolution = resolutionInMilliseconds == null
                ? 0d
                : Convert.ToDouble(resolutionInMilliseconds);
            var proposedResolution = Math.Round(Math.Abs(concreteResolution) * OneMillisecondInTicks, 0, MidpointRounding.AwayFromZero);
            var validResolution = Math.Max(0d, OneDayInTicks - proposedResolution);
            if (Math.Abs(validResolution - OneDayInTicks) < 1d)
                validResolution -= 1d;

            var endOfDay = value.Date.AddTicks(Convert.ToInt64(validResolution));
            return endOfDay;
        }

        /// <summary>
        /// This method grabs the Date portion of the DateTime parameter <paramref name="value"/>, which effetively
        /// gives you the start of the day specified, and then adds the absolute value of the <paramref name="resolutionInMilliseconds"/>,
        /// converted into ticks. e.g. If parameter <paramref name="value"/> is passed DateTime.MaxValue, and a resolution of 0.0002,
        /// this will result in a returned DateTime with 3155378975999999998 ticks at 12/31/9999 and a time resolution of 23:59:59.9999998.
        /// This in turn does not equal DateTime.MaxValue without passing a resolution of 0.0001 or 0. If the paramater <paramref name="value"/>
        /// is provided as a null value, this method returns null.
        /// <remarks>
        /// If you wish for your end of day to be the beginning of the same day, set <paramref name="resolutionInMilliseconds"/>
        /// to <seealso cref="double.MaxValue"/>, or set it to the appropriate time tolerance your application can handle. Do note that the
        /// <paramref name="resolutionInMilliseconds"/> can not exceed one day's worth of milliseconds: 86,400,000.
        /// </remarks>
        /// </summary>
        /// <param name="value">The value to get the end of the day from.</param>
        /// <param name="resolutionInMilliseconds">The resolution in milliseconds to remove from the start of the next day.</param>
        /// <returns>
        /// The end of the given day with respect to the resolution provided in milliseconds.
        /// </returns>
        public static DateTime? EndOfDay(this DateTime? value, double? resolutionInMilliseconds = 0d)
        {
            if (value == null)
                return null;

            return Convert.ToDateTime(value).EndOfDay(resolutionInMilliseconds);
        }

        /// <summary>
        /// This method converts the value of a DateTime object from Coordinated Universal Time (UTC) to the time zone specified in
        /// parameter <paramref name="timeZoneId"/>. Next the method grabs the Date portion of the DateTime parameter <paramref name="value"/>, which effetively
        /// gives you the start of the day specified, and then adds the absolute value of the <paramref name="resolutionInMilliseconds"/>,
        /// converted into ticks. e.g. If parameter <paramref name="value"/> is passed DateTime.MaxValue, and a resolution of 0.0002,
        /// this will result in a returned DateTime with 3155378975999999998 ticks at 12/31/9999 and a time resolution of 23:59:59.9999998.
        /// This in turn does not equal DateTime.MaxValue without passing a resolution of 0.0001 or 0.
        /// Note that this method can throw an exception in the event that a given parameter <paramref name="timeZoneId"/> is not a valid system
        /// time zone, or when converting from UTC time to the given time zone, the date falls out of range from DateTime.MaxDate.
        /// <remarks>
        /// If you wish for your end of day to be the beginning of the same day, set <paramref name="resolutionInMilliseconds"/>
        /// to <seealso cref="double.MaxValue"/>, or set it to the appropriate time tolerance your application can handle. Do note that the
        /// <paramref name="resolutionInMilliseconds"/> can not exceed one day's worth of milliseconds: 86,400,000.
        /// </remarks>
        /// </summary>
        /// <param name="value">The value to get the end of the day from.</param>
        /// <param name="timeZoneId">The .NET Time Zone specification, e.g. "Eastern Standard Time" represented as a TimeZoneInfo object.</param>
        /// <param name="resolutionInMilliseconds">The resolution in milliseconds to remove from the start of the next day.</param>
        /// <returns>
        /// The end of the given UTC day specified by the <paramref name="value"/> parameter, but represented in the time zone specified by parameter <paramref name="timeZoneId"/>, with respect to the resolution provided in milliseconds.
        /// </returns>
        public static DateTime EndOfDayFromUtc(this DateTime value, string timeZoneId, double? resolutionInMilliseconds = 0d)
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var timeZoneDate = TimeZoneInfo.ConvertTimeFromUtc(value, timeZoneInfo);
            var endOfDay = timeZoneDate.EndOfDay(resolutionInMilliseconds);
            return endOfDay;
        }

        /// <summary>
        /// This method converts the value of a DateTime object from Coordinated Universal Time (UTC) to the time zone specified in
        /// parameter <paramref name="timeZoneId"/>. Next the method grabs the Date portion of the DateTime parameter <paramref name="value"/>, which effetively
        /// gives you the start of the day specified, and then adds the absolute value of the <paramref name="resolutionInMilliseconds"/>,
        /// converted into ticks. e.g. If parameter <paramref name="value"/> is passed DateTime.MaxValue, and a resolution of 0.0002,
        /// this will result in a returned DateTime with 3155378975999999998 ticks at 12/31/9999 and a time resolution of 23:59:59.9999998.
        /// This in turn does not equal DateTime.MaxValue without passing a resolution of 0.0001 or 0.
        /// Note that this method can throw an exception in the event that a given parameter <paramref name="timeZoneId"/> is not a valid system
        /// time zone, or when converting from UTC time to the given time zone, the date falls out of range from DateTime.MaxDate.
        /// If the paramater <paramref name="value"/> is provided as a null value, this method returns null.
        /// <remarks>
        /// If you wish for your end of day to be the beginning of the same day, set <paramref name="resolutionInMilliseconds"/>
        /// to <seealso cref="double.MaxValue"/>, or set it to the appropriate time tolerance your application can handle. Do note that the
        /// <paramref name="resolutionInMilliseconds"/> can not exceed one day's worth of milliseconds: 86,400,000.
        /// </remarks>
        /// </summary>
        /// <param name="value">The value to get the end of the day from.</param>
        /// <param name="timeZoneId">The .NET Time Zone specification, e.g. "Eastern Standard Time" represented as a TimeZoneInfo object.</param>
        /// <param name="resolutionInMilliseconds">The resolution in milliseconds to remove from the start of the next day.</param>
        /// <returns>
        /// The end of the given UTC day specified by the <paramref name="value"/> parameter, but represented in the time zone specified by parameter <paramref name="timeZoneId"/>, with respect to the resolution provided in milliseconds.
        /// </returns>
        public static DateTime? EndOfDayFromUtc(this DateTime? value, string timeZoneId, double? resolutionInMilliseconds = 0d)
        {
            if (value == null)
                return null;

            return Convert.ToDateTime(value).EndOfDayFromUtc(timeZoneId, resolutionInMilliseconds);
        }

        /// <summary>
        /// This method turns a date into the following string format: yyyy/dd/mm hh:mm:ss.ms
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToLongDateTimeString(this DateTime value)
        {
            return $"{value.Year:D4}/{value.Month:D2}/{value.Day:D2} {value.Hour:D2}:{value.Minute:D2}:{value.Second:D2}.{value.Millisecond}";
        }
    }
}
