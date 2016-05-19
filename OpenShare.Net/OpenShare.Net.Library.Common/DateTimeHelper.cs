using System;
using System.Text.RegularExpressions;

namespace OpenShare.Net.Library.Common
{
    public static class DateTimeHelper
    {
        public static DateTime GetDateTimeFromShortDateString(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new Exception("Invalid DateTime value.");

            var dateMatch = Regex.Match(value, @"^([0-3]?[0-9])/([0-3]?[0-9])/([0-9]{2}?[0-9]{2})$");
            if (!dateMatch.Success)
                throw new Exception("Invalid DateTime value.");

            return new DateTime(
                Convert.ToInt32(dateMatch.Groups[3].Value),
                Convert.ToInt32(dateMatch.Groups[1].Value),
                Convert.ToInt32(dateMatch.Groups[2].Value));
        }
    }
}
