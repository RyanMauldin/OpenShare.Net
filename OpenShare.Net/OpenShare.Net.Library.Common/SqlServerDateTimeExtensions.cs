using System;

namespace OpenShare.Net.Library.Common
{
    /// <summary>
    /// Extension methods that could be helpful for dealing with SQL Server DateTime datatype.
    /// </summary>
    public static class SqlServerDateTimeExtensions
    {
        /// <summary>
        /// The minimum DateTime value possible to be stored in SQL Server.
        /// </summary>
        private static readonly DateTime SqlServerMinDateTime = new DateTime(1753, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// The maxiumum DateTime value possible to be stored in SQL Server.
        /// </summary>
        private static readonly DateTime SqlServerMaxDateTime = new DateTime(9999, 12, 31, 23, 59, 59, 997, DateTimeKind.Utc);
        
        /// <summary>
        /// Makes sure the DateTime paramater <paramref name="value"/> given can fit within the
        /// min and max values of SQL Server DateTime data type.
        /// </summary>
        /// <param name="value">The value to check and fix.</param>
        /// <returns>
        /// A DateTime value that can be stored safely in SQL Server.
        /// </returns>
        public static DateTime ToSqlServerSafeDateTime(this DateTime value)
        {
            if (value < SqlServerMinDateTime)
            {
                value = SqlServerMinDateTime;
                return value;
            }

            if (value <= SqlServerMaxDateTime)
                return value;

            value = SqlServerMaxDateTime;
            return value;
        }

        /// <summary>
        /// Makes sure the DateTime paramater <paramref name="value"/> given can fit within the
        /// min and max values of SQL Server DateTime data type. If the paramater <paramref name="value"/>
        /// is provided as a null value, this method returns null.
        /// </summary>
        /// <param name="value">The value to check and fix.</param>
        /// <returns>
        /// A DateTime value that can be stored safely in SQL Server.
        /// </returns>
        public static DateTime? ToSqlServerSafeDateTime(this DateTime? value)
        {
            if (value == null)
                return null;

            return Convert.ToDateTime(value).ToSqlServerSafeDateTime();
        }

        /// <summary>
        /// Makes sure the DateTime paramater <paramref name="value"/> given can fit within the
        /// min and max values of SQL Server DateTime data type. This method internally calls
        /// <seealso cref="ToSqlServerSafeDateTime(System.DateTime)"/> first and then if it
        /// is equal to the minimum value Sql Server can store it returns DateTime.UtcNow. 
        /// This is almost like a Sql Server safe default value provider extension method.
        /// </summary>
        /// <param name="value">The value to check and fix.</param>
        /// <returns>
        /// A DateTime value that can be stored safely in SQL Server.
        /// </returns>
        public static DateTime ToSqlServerSafeDateTimeModifier(this DateTime value)
        {
            value = value.ToSqlServerSafeDateTime();
            if (value == SqlServerMinDateTime)
                value = DateTime.UtcNow;

            return value;
        }

        /// <summary>
        /// Makes sure the DateTime paramater <paramref name="value"/> given can fit within the
        /// min and max values of SQL Server DateTime data type. This method internally calls
        /// <seealso cref="ToSqlServerSafeDateTime(System.DateTime)"/> first and then if it
        /// is equal to the minimum value Sql Server can store it returns DateTime.UtcNow. 
        /// This is almost like a Sql Server safe default value provider extension method.
        /// If the paramater <paramref name="value"/> is provided as a null value, this method returns null.
        /// </summary>
        /// <param name="value">The value to check and fix.</param>
        /// <returns>
        /// A DateTime value that can be stored safely in SQL Server.
        /// </returns>
        public static DateTime? ToSqlServerSafeDateTimeModifier(this DateTime? value)
        {
            if (value == null)
                return null;

            return Convert.ToDateTime(value).ToSqlServerSafeDateTimeModifier();
        }

        /// <summary>
        /// This method gets the Date portion of the DateTime paramerter <paramref name="value"/>, which effectively
        /// removes the time for that date. Next the extension method <seealso cref="ToSqlServerSafeDateTime(System.DateTime)"/>
        /// is then called to make sure the value is safe for SQL Server consumption.
        /// </summary>
        /// <param name="value">The value to get the start of the day from.</param>
        /// <returns>
        /// The start of the given day.
        /// </returns>
        public static DateTime SqlServerSafeStartOfDay(this DateTime value)
        {
            return value.StartOfDay().ToSqlServerSafeDateTime();
        }

        /// <summary>
        /// This method gets the Date portion of the DateTime paramerter <paramref name="value"/>, which effectively
        /// removes the time for that date. Next the extension method <seealso cref="ToSqlServerSafeDateTime(System.DateTime)"/>
        /// is then called to make sure the value is safe for SQL Server consumption.
        /// If the paramater <paramref name="value"/> is provided as a null value, this method returns null.
        /// </summary>
        /// <param name="value">The value to get the start of the day from.</param>
        /// <returns>
        /// The start of the given day.
        /// </returns>
        public static DateTime? SqlServerSafeStartOfDay(this DateTime? value)
        {
            return value?.StartOfDay().ToSqlServerSafeDateTime();
        }

        /// <summary>
        /// This method converts the value of a DateTime object from Coordinated Universal Time (UTC) to the time zone specified in
        /// parameter <paramref name="timeZoneId"/>. Next the method gets the Date portion of the DateTime paramerter
        /// <paramref name="value"/>, which effectively removes the time for that date. Note that this method can throw an exception in the
        /// event that a given parameter <paramref name="timeZoneId"/> is not a valid system time zone, or when converting from UTC time
        /// to the given time zone, the date falls out of range from DateTime.MinDate. Next the extension method <seealso cref="ToSqlServerSafeDateTime(System.DateTime)"/>
        /// is then called to make sure the value is safe for SQL Server consumption.
        /// </summary>
        /// <param name="value">The value to get the start of the day from.</param>
        /// <param name="timeZoneId">The .NET Time Zone specification, e.g. "Eastern Standard Time" represented as a TimeZoneInfo object.</param>
        /// <returns>
        /// The start of a given UTC day specified by the <paramref name="value"/> parameter, but represented in the time zone specified by parameter <paramref name="timeZoneId"/>.
        /// </returns>
        public static DateTime SqlServerSafeStartOfDayFromUtc(this DateTime value, string timeZoneId)
        {
            return value.StartOfDayFromUtc(timeZoneId).ToSqlServerSafeDateTime();
        }

        /// <summary>
        /// This method converts the value of a DateTime object from Coordinated Universal Time (UTC) to the time zone specified in
        /// parameter <paramref name="timeZoneId"/>. Next the method gets the Date portion of the DateTime paramerter
        /// <paramref name="value"/>, which effectively removes the time for that date. Note that this method can throw an exception in the
        /// event that a given parameter <paramref name="timeZoneId"/> is not a valid system time zone, or when converting from UTC time
        /// to the given time zone, the date falls out of range from DateTime.MinDate. Next the extension method <seealso cref="ToSqlServerSafeDateTime(System.DateTime)"/>
        /// is then called to make sure the value is safe for SQL Server consumption. If the paramater <paramref name="value"/> is provided as a null value, this method returns null.
        /// </summary>
        /// <param name="value">The value to get the start of the day from.</param>
        /// <param name="timeZoneId">The .NET Time Zone specification, e.g. "Eastern Standard Time" represented as a TimeZoneInfo object.</param>
        /// <returns>
        /// The start of a given UTC day specified by the <paramref name="value"/> parameter, but represented in the time zone specified by parameter <paramref name="timeZoneId"/>.
        /// </returns>
        public static DateTime? SqlServerSafeStartOfDayFromUtc(this DateTime? value, string timeZoneId)
        {
            return value?.StartOfDayFromUtc(timeZoneId).ToSqlServerSafeDateTime();
        }

        /// <summary>
        /// This method grabs the Date portion of the DateTime parameter <paramref name="value"/>, which effetively
        /// gives you the start of the day specified. Next the extension method <seealso cref="ToSqlServerSafeDateTime(System.DateTime)"/>
        /// is then called to make sure the value is safe for SQL Server consumption.
        /// <remarks>
        /// SQL Server's DateTime data type can have a maxium amount of milliseconds up to 0.997, as in the given value '9999-12-31T23:59:59.997'.
        /// I commonly use this method with entity framework, dapper, and ADO when using dates against SQL Server.
        /// If I provide the value '9999-12-31T23:59:59.999' to SQL Server for a DateTime type it will try to round the value up to the next day
        /// and will error. That being said I need to be able to get the 0.997 of the last day so I do not want my end of day to end up being
        /// the next day in most cases.
        /// </remarks>
        /// </summary>
        /// <param name="value">The value to get the end of the day from.</param>
        /// <returns>
        /// The end of the given day with respect to the resolution provided in milliseconds.
        /// </returns>
        public static DateTime SqlServerSafeEndOfDay(this DateTime value)
        {
            return value.EndOfDay(3d).ToSqlServerSafeDateTime();
        }

        /// <summary>
        /// This method grabs the Date portion of the DateTime parameter <paramref name="value"/>, which effetively
        /// gives you the start of the day specified. Next the extension method <seealso cref="ToSqlServerSafeDateTime(System.DateTime)"/>
        /// is then called to make sure the value is safe for SQL Server consumption. If the paramater <paramref name="value"/> is provided as a null value,
        /// this method returns null.
        /// <remarks>
        /// SQL Server's DateTime data type can have a maxium amount of milliseconds up to 0.997, as in the given value '9999-12-31T23:59:59.997'.
        /// I commonly use this method with entity framework, dapper, and ADO when using dates against SQL Server.
        /// If I provide the value '9999-12-31T23:59:59.999' to SQL Server for a DateTime type it will try to round the value up to the next day
        /// and will error. That being said I need to be able to get the 0.997 of the last day so I do not want my end of day to end up being
        /// the next day in most cases.
        /// </remarks>
        /// </summary>
        /// <param name="value">The value to get the end of the day from.</param>
        /// <returns>
        /// The end of the given day with respect to the resolution provided in milliseconds.
        /// </returns>
        public static DateTime? SqlServerSafeEndOfDay(this DateTime? value)
        {
            return value?.EndOfDay(3d).ToSqlServerSafeDateTime();
        }

        /// <summary>
        /// This method converts the value of a DateTime object from Coordinated Universal Time (UTC) to the time zone specified in
        /// parameter <paramref name="timeZoneId"/>. Next the method grabs the Date portion of the DateTime parameter <paramref name="value"/>, which effetively
        /// gives you the start of the day specified. Next the extension method <seealso cref="ToSqlServerSafeDateTime(System.DateTime)"/>
        /// is then called to make sure the value is safe for SQL Server consumption.
        /// <remarks>
        /// SQL Server's DateTime data type can have a maxium amount of milliseconds up to 0.997, as in the given value '9999-12-31T23:59:59.997'.
        /// I commonly use this method with entity framework, dapper, and ADO when using dates against SQL Server.
        /// If I provide the value '9999-12-31T23:59:59.999' to SQL Server for a DateTime type it will try to round the value up to the next day
        /// and will error. That being said I need to be able to get the 0.997 of the last day so I do not want my end of day to end up being
        /// the next day in most cases.
        /// </remarks>
        /// </summary>
        /// <param name="value">The value to get the end of the day from.</param>
        /// <param name="timeZoneId">The .NET Time Zone specification, e.g. "Eastern Standard Time" represented as a TimeZoneInfo object.</param>
        /// <returns>
        /// The end of the given UTC day specified by the <paramref name="value"/> parameter, but represented in the time zone specified by parameter <paramref name="timeZoneId"/>, with respect to the resolution provided in milliseconds.
        /// </returns>
        public static DateTime SqlServerSafeEndOfDayFromUtc(this DateTime value, string timeZoneId)
        {
            return value.EndOfDayFromUtc(timeZoneId, 3d).ToSqlServerSafeDateTime();
        }

        /// <summary>
        /// This method converts the value of a DateTime object from Coordinated Universal Time (UTC) to the time zone specified in
        /// parameter <paramref name="timeZoneId"/>. Next the method grabs the Date portion of the DateTime parameter <paramref name="value"/>, which effetively
        /// gives you the start of the day specified. Next the extension method <seealso cref="ToSqlServerSafeDateTime(System.DateTime)"/>
        /// is then called to make sure the value is safe for SQL Server consumption. If the paramater <paramref name="value"/> is provided as a null value,
        /// this method returns null.
        /// <remarks>
        /// SQL Server's DateTime data type can have a maxium amount of milliseconds up to 0.997, as in the given value '9999-12-31T23:59:59.997'.
        /// I commonly use this method with entity framework, dapper, and ADO when using dates against SQL Server.
        /// If I provide the value '9999-12-31T23:59:59.999' to SQL Server for a DateTime type it will try to round the value up to the next day
        /// and will error. That being said I need to be able to get the 0.997 of the last day so I do not want my end of day to end up being
        /// the next day in most cases.
        /// </remarks>
        /// </summary>
        /// <param name="value">The value to get the end of the day from.</param>
        /// <param name="timeZoneId">The .NET Time Zone specification, e.g. "Eastern Standard Time" represented as a TimeZoneInfo object.</param>
        /// <returns>
        /// The end of the given UTC day specified by the <paramref name="value"/> parameter, but represented in the time zone specified by parameter <paramref name="timeZoneId"/>, with respect to the resolution provided in milliseconds.
        /// </returns>
        public static DateTime? SqlServerSafeEndOfDayFromUtc(this DateTime? value, string timeZoneId)
        {
            return value?.EndOfDayFromUtc(timeZoneId, 3d).ToSqlServerSafeDateTime();
        }
    }
}
