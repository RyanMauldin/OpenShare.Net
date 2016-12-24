using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenShare.Net.Library.Common
{
    public static class DateTimeParser
    {
        private static readonly char[] TrimChars = { ' ', '\t', '\n', '\r', ',', '.', '!', ':', ';', '(', ')', '[', ']', '{', '}', '+', '-' };

        public static bool TryParseUtc(string value, out DateTime dateTime)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                dateTime = default(DateTime);
                return false;
            }

            value = value.Trim().Trim(TrimChars);

            if (string.IsNullOrWhiteSpace(value))
            {
                dateTime = default(DateTime);
                return false;
            }

            var parsedDateTime = ParseDateTime(value);
            if (parsedDateTime == null)
            {
                dateTime = default(DateTime);
                return false;
            }

            dateTime = parsedDateTime.Value;
            return true;
        }

        public static bool TryParseUtc(string value, out DateTime? dateTime)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                dateTime = null;
                return false;
            }

            value = value.Trim().Trim(TrimChars);

            if (string.IsNullOrWhiteSpace(value))
            {
                dateTime = null;
                return false;
            }

            var parsedDateTime = ParseDateTime(value);
            if (parsedDateTime == null)
            {
                dateTime = null;
                return false;
            }

            dateTime = parsedDateTime.Value;
            return true;
        }

        private static DateTime? ParseDateTime(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // Trim off extra characters.
            value = value.Trim().Trim(TrimChars);

            // Go ahead and attempt to parse a valid date.
            DateTime date;
            if (DateTime.TryParse(value, out date))
                return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond, DateTimeKind.Utc);

            // Correct for AM or PM.
            if (value.EndsWith("a. m", StringComparison.InvariantCultureIgnoreCase))
            {
                value = value.Substring(0, value.Length - 4) + "am";
            }
            else if (value.EndsWith("p. m", StringComparison.InvariantCultureIgnoreCase))
            {
                value = value.Substring(0, value.Length - 4) + "pm";
            }
            else if (value.EndsWith("a.m", StringComparison.InvariantCultureIgnoreCase))
            {
                value = value.Substring(0, value.Length - 3) + "am";
            }
            else if (value.EndsWith("p.m", StringComparison.InvariantCultureIgnoreCase))
            {
                value = value.Substring(0, value.Length - 3) + "pm";
            }

            // If value ends with AM or PM, try to parse date
            if (value.EndsWith("am", StringComparison.InvariantCultureIgnoreCase)
                || value.EndsWith("pm", StringComparison.InvariantCultureIgnoreCase))
            {
                if (DateTime.TryParse(value, out date))
                    return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond, DateTimeKind.Utc);

                return null;
            }

            // Check ending of value for time zone.
            var timeZone = string.Empty;
            const string timeZoneExpression = @"^(?<DateTime>.+)(?<Space>\s+)(?<TimeZone>[a-zA-Z]+)$";
            if (Regex.IsMatch(value, timeZoneExpression, RegexOptions.IgnoreCase))
            {
                var match = Regex.Match(value, timeZoneExpression, RegexOptions.IgnoreCase);
                value = match.Groups["DateTime"].Value;
                timeZone = match.Groups["TimeZone"].Value;
            }

            // Check for custom format.
            const string dateFormatExpression = @"^(?<Month>\w+)(?<Punctuation>\.*)(?<Space>\s+)(?<Day>\d{1,2})(?<DaySuffix>\w+)(?<seperator>,{1})(?<Space2>\s+)(?<Year>\d{1,4})$";

            if (Regex.IsMatch(value, dateFormatExpression, RegexOptions.IgnoreCase))
            {
                var match = Regex.Match(value, dateFormatExpression, RegexOptions.IgnoreCase);
                var month = match.Groups["Month"].Value;
                var day = match.Groups["Day"].Value;
                var year = match.Groups["Year"].Value;
                var builder = new StringBuilder(month.Length + day.Length + year.Length + 3);
                builder.Append(month);
                builder.Append(" ");
                builder.Append(day);
                builder.Append(", ");
                builder.Append(year);
                if (DateTime.TryParse(builder.ToString(), out date))
                    return CorrectForTimeZone(date, timeZone);
            }

            // Parse date time
            if (!DateTime.TryParse(value, out date))
                return null;

            // Correct for time zone.
            return CorrectForTimeZone(date, timeZone);
        }

        private static DateTime CorrectForTimeZone(DateTime date, string timeZone)
        {
            if (string.IsNullOrWhiteSpace(timeZone))
                return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond, DateTimeKind.Utc);

            if (!TimeZoneAbbeviations.ContainsKey(timeZone))
                return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond, DateTimeKind.Utc);

            TimeZone zone;
            if (TimeZoneAbbeviations.TryGetValue(timeZone, out zone))
                date = date.Add(zone.Offset);

            return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond, DateTimeKind.Utc);
        }

        private class TimeZone
        {
            public string Abbreviation { get; set; }

            public string Name { get; set; }

            public string Location { get; set; }

            public TimeSpan Offset { get; set; }
        }

        static DateTimeParser()
        {
            // Pulled most from https://www.timeanddate.com/time/zones/
            // except for conflicts in Abbreviation
            TimeZones = new List<TimeZone>
            {
                new TimeZone
                {
                    Abbreviation = "A",
                    Name = "Alpha Time Zone",
                    Location = "Military",
                    Offset = new TimeSpan(1, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "ACDT",
                    Name = "Australian Central Daylight Time",
                    Location = "Australia",
                    Offset = new TimeSpan(10, 30, 0)
                },
                new TimeZone
                {
                    Abbreviation = "ACST",
                    Name = "Australian Central Standard Time",
                    Location = "Australia",
                    Offset = new TimeSpan(9, 30, 0)
                },
                new TimeZone
                {
                    Abbreviation = "ACT",
                    Name = "Australian Central Time",
                    Location = "Australia",
                    Offset = new TimeSpan(10, 30, 0)
                },
                new TimeZone
                {
                    Abbreviation = "ACWST",
                    Name = "Australian Central Western Standard Time",
                    Location = "Australia",
                    Offset = new TimeSpan(8, 45, 0)
                },
                new TimeZone
                {
                    Abbreviation = "ADT",
                    Name = "Atlantic Daylight Time",
                    Location = "North America",
                    Offset = new TimeSpan(-3, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "AEDT",
                    Name = "Australian Eastern Daylight Time",
                    Location = "Australia",
                    Offset = new TimeSpan(11, 0, 0)
                }
                ,new TimeZone
                {
                    Abbreviation = "AEST",
                    Name = "Australian Eastern Standard Time",
                    Location = "Australia",
                    Offset = new TimeSpan(10, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "AET",
                    Name = "Australian Eastern Time",
                    Location = "Australia",
                    Offset = new TimeSpan(11, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "AFT",
                    Name = "Afghanistan Time",
                    Location = "Asia",
                    Offset = new TimeSpan(4, 30, 0)
                },
                new TimeZone
                {
                    Abbreviation = "AKDT",
                    Name = "Alaska Daylight Time",
                    Location = "North America",
                    Offset = new TimeSpan(-9, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "AKST",
                    Name = "Alaska Standard Time",
                    Location = "North America",
                    Offset = new TimeSpan(-9, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "ALMT",
                    Name = "Alma-Ata Time",
                    Location = "Asia",
                    Offset = new TimeSpan(6, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "AMST",
                    Name = "Armenia Summer Time",
                    Location = "Asia",
                    Offset = new TimeSpan(4, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "AMT",
                    Name = "Armenia Time",
                    Location = "Asia",
                    Offset = new TimeSpan(4, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "ANAST",
                    Name = "Anadyr Summer Time",
                    Location = "Asia",
                    Offset = new TimeSpan(12, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "ANAT",
                    Name = "Anadyr Time",
                    Location = "Asia",
                    Offset = new TimeSpan(12, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "AQTT",
                    Name = "Aqtobe Time",
                    Location = "Asia",
                    Offset = new TimeSpan(5, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "ART",
                    Name = "Argentina Time",
                    Location = "Antarctica",
                    Offset = new TimeSpan(-3, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "AST",
                    Name = "Atlantic Standard Time",
                    Location = "North America",
                    Offset = new TimeSpan(-4, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "AT",
                    Name = "Atlantic Time",
                    Location = "North America",
                    Offset = new TimeSpan(-3, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "AWDT",
                    Name = "Australian Western Daylight Time",
                    Location = "Australia",
                    Offset = new TimeSpan(9, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "AWST",
                    Name = "Australian Western Standard Time",
                    Location = "Australia",
                    Offset = new TimeSpan(8, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "AZOST",
                    Name = "Azores Summer Time",
                    Location = "Atlantic",
                    Offset = new TimeSpan(0, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "AZOT",
                    Name = "Azores Time",
                    Location = "Atlantic",
                    Offset = new TimeSpan(-1, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "AZST",
                    Name = "Azerbaijan Summer Time",
                    Location = "Asia",
                    Offset = new TimeSpan(5, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "AZT",
                    Name = "Azerbaijan Time",
                    Location = "Asia",
                    Offset = new TimeSpan(4, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "AoE",
                    Name = "Anywhere on Earth",
                    Location = "Pacific",
                    Offset = new TimeSpan(-12, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "B",
                    Name = "Bravo Time Zone",
                    Location = "Military",
                    Offset = new TimeSpan(2, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "BNT",
                    Name = "Brunei Darussalam Time",
                    Location = "Asia",
                    Offset = new TimeSpan(8, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "BOT",
                    Name = "Bolivia Time",
                    Location = "South America",
                    Offset = new TimeSpan(-4, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "BRST",
                    Name = "Brasília Summer Time",
                    Location = "South America",
                    Offset = new TimeSpan(-4, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "BRT",
                    Name = "Brasília Time",
                    Location = "South America",
                    Offset = new TimeSpan(-3, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "BST",
                    Name = "British Summer Time",
                    Location = "Europe",
                    Offset = new TimeSpan(1, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "BTT",
                    Name = "Bhutan Time",
                    Location = "Asia",
                    Offset = new TimeSpan(6, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "C",
                    Name = "Charlie Time Zone",
                    Location = "Military",
                    Offset = new TimeSpan(3, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "CAST",
                    Name = "Casey Time",
                    Location = "Antarctica",
                    Offset = new TimeSpan(11, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "CAT",
                    Name = "Central Africa Time",
                    Location = "Africa",
                    Offset = new TimeSpan(2, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "CCT",
                    Name = "Cocos Islands Time",
                    Location = "Indian Ocean",
                    Offset = new TimeSpan(6, 30, 0)
                },
                new TimeZone
                {
                    Abbreviation = "CDT",
                    Name = "Central Daylight Time",
                    Location = "North America",
                    Offset = new TimeSpan(-5, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "CEST",
                    Name = "Central European Summer Time",
                    Location = "Europe",
                    Offset = new TimeSpan(2, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "CET",
                    Name = "Central European Time",
                    Location = "Europe",
                    Offset = new TimeSpan(1, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "CHADT",
                    Name = "Chatham Island Daylight Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(13, 45, 0)
                },
                new TimeZone
                {
                    Abbreviation = "CHAST",
                    Name = "Chatham Island Standard Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(12, 45, 0)
                },
                new TimeZone
                {
                    Abbreviation = "CHOST",
                    Name = "Choibalsan Summer Time",
                    Location = "Asia",
                    Offset = new TimeSpan(9, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "CHOT",
                    Name = "Choibalsan Time",
                    Location = "Asia",
                    Offset = new TimeSpan(8, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "CHUT",
                    Name = "Chuuk Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(10, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "CIDST",
                    Name = "Cayman Islands Daylight Saving Time",
                    Location = "Caribbean",
                    Offset = new TimeSpan(-4, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "CIST",
                    Name = "Cayman Islands Standard Time",
                    Location = "Caribbean",
                    Offset = new TimeSpan(-5, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "CKT",
                    Name = "Cook Island Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(-10, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "CLST",
                    Name = "Chile Summer Time",
                    Location = "South America",
                    Offset = new TimeSpan(-3, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "COT",
                    Name = "Colombia Time",
                    Location = "South America",
                    Offset = new TimeSpan(-5, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "CST",
                    Name = "Central Standard Time",
                    Location = "North America",
                    Offset = new TimeSpan(-6, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "CT",
                    Name = "Central Time",
                    Location = "North America",
                    Offset = new TimeSpan(-6, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "CVT",
                    Name = "Cape Verde Time",
                    Location = "Africa",
                    Offset = new TimeSpan(-1, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "CXT",
                    Name = "Christmas Island Time",
                    Location = "Australia",
                    Offset = new TimeSpan(7, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "ChST",
                    Name = "Chamorro Standard Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(10, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "D",
                    Name = "Delta Time Zone",
                    Location = "Military",
                    Offset = new TimeSpan(4, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "DAVT",
                    Name = "Davis Time",
                    Location = "Antarctica",
                    Offset = new TimeSpan(7, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "DDUT",
                    Name = "Dumont-d'Urville Time",
                    Location = "Antarctica",
                    Offset = new TimeSpan(10, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "E",
                    Name = "Echo Time Zone",
                    Location = "Military",
                    Offset = new TimeSpan(5, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "EASST",
                    Name = "Easter Island Summer Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(-5, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "EAST",
                    Name = "Easter Island Standard Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(-6, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "EAT",
                    Name = "Eastern Africa Time",
                    Location = "Africa",
                    Offset = new TimeSpan(3, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "ECT",
                    Name = "Ecuador Time",
                    Location = "South America",
                    Offset = new TimeSpan(-5, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "EDT",
                    Name = "Eastern Daylight Time",
                    Location = "North America",
                    Offset = new TimeSpan(-4, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "EEST",
                    Name = "Eastern European Summer Time",
                    Location = "Europe",
                    Offset = new TimeSpan(3, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "EET",
                    Name = "Eastern European Time",
                    Location = "Europe",
                    Offset = new TimeSpan(2, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "EGST",
                    Name = "Eastern Greenland Summer Time",
                    Location = "North America",
                    Offset = new TimeSpan(0, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "EGT",
                    Name = "East Greenland Time",
                    Location = "North America",
                    Offset = new TimeSpan(-1, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "EST",
                    Name = "Eastern Standard Time",
                    Location = "North America",
                    Offset = new TimeSpan(-5, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "ET",
                    Name = "Eastern Time",
                    Location = "North America",
                    Offset = new TimeSpan(-4, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "F",
                    Name = "Foxtrot Time Zone",
                    Location = "Military",
                    Offset = new TimeSpan(6, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "FET",
                    Name = "Further-Eastern European Time",
                    Location = "Europe",
                    Offset = new TimeSpan(3, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "FJST",
                    Name = "Fiji Summer Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(13, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "FJT",
                    Name = "Fiji Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(12, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "FKST",
                    Name = "Falkland Islands Summer Time",
                    Location = "South America",
                    Offset = new TimeSpan(-3, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "FKT",
                    Name = "Falkland Island Time",
                    Location = "South America",
                    Offset = new TimeSpan(-4, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "FNT",
                    Name = "Fernando de Noronha Time",
                    Location = "South America",
                    Offset = new TimeSpan(-2, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "G",
                    Name = "Golf Time Zone",
                    Location = "Military",
                    Offset = new TimeSpan(7, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "GALT",
                    Name = "Galapagos Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(-6, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "GAMT",
                    Name = "Gambier Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(-9, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "GET",
                    Name = "Georgia Standard Time",
                    Location = "Asia",
                    Offset = new TimeSpan(4, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "GFT",
                    Name = "French Guiana Time",
                    Location = "South America",
                    Offset = new TimeSpan(-3, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "GILT",
                    Name = "Gilbert Island Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(12, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "GMT",
                    Name = "Greenwich Mean Time",
                    Location = "Europe",
                    Offset = new TimeSpan(0, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "GST",
                    Name = "Gulf Standard Time",
                    Location = "Asia",
                    Offset = new TimeSpan(4, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "GYT",
                    Name = "Guyana Time",
                    Location = "South America",
                    Offset = new TimeSpan(-4, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "H",
                    Name = "Hotel Time Zone",
                    Location = "Military",
                    Offset = new TimeSpan(8, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "HADT",
                    Name = "Hawaii-Aleutian Daylight Time",
                    Location = "North America",
                    Offset = new TimeSpan(-9, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "HAST",
                    Name = "Hawaii-Aleutian Standard Time",
                    Location = "North America",
                    Offset = new TimeSpan(-10, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "HKT",
                    Name = "Hong Kong Time",
                    Location = "Asia",
                    Offset = new TimeSpan(8, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "HOVST",
                    Name = "Hovd Summer Time",
                    Location = "Asia",
                    Offset = new TimeSpan(8, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "I",
                    Name = "India Time Zone",
                    Location = "Military",
                    Offset = new TimeSpan(9, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "ICT",
                    Name = "Indochina Time",
                    Location = "Asia",
                    Offset = new TimeSpan(7, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "IDT",
                    Name = "Israel Daylight Time",
                    Location = "Asia",
                    Offset = new TimeSpan(3, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "IOT",
                    Name = "Indian Chagos Time",
                    Location = "Indian Ocean",
                    Offset = new TimeSpan(6, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "IRDT",
                    Name = "Iran Daylight Time",
                    Location = "Asia",
                    Offset = new TimeSpan(4, 30, 0)
                },
                new TimeZone
                {
                    Abbreviation = "IRKST",
                    Name = "Irkutsk Summer Time",
                    Location = "Asia",
                    Offset = new TimeSpan(9, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "IRKT",
                    Name = "Irkutsk Time",
                    Location = "Asia",
                    Offset = new TimeSpan(8, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "IRST",
                    Name = "Iran Standard Time",
                    Location = "Asia",
                    Offset = new TimeSpan(3, 30, 0)
                },
                new TimeZone
                {
                    Abbreviation = "IST",
                    Name = "Irish Standard Time",
                    Location = "Europe",
                    Offset = new TimeSpan(1, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "JST",
                    Name = "Japan Standard Time",
                    Location = "Asia",
                    Offset = new TimeSpan(9, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "K",
                    Name = "Kilo Time Zone",
                    Location = "Military",
                    Offset = new TimeSpan(10, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "KGT",
                    Name = "Kyrgyzstan Time",
                    Location = "Asia",
                    Offset = new TimeSpan(6, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "KOST",
                    Name = "Kosrae Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(11, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "KRAST",
                    Name = "Krasnoyarsk Summer Time",
                    Location = "Asia",
                    Offset = new TimeSpan(8, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "KRAT",
                    Name = "Krasnoyarsk Time",
                    Location = "Asia",
                    Offset = new TimeSpan(7, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "KST",
                    Name = "Korea Standard Time",
                    Location = "Asia",
                    Offset = new TimeSpan(9, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "KUYT",
                    Name = "Kuybyshev Time",
                    Location = "Europe",
                    Offset = new TimeSpan(4, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "L",
                    Name = "Lima Time Zone",
                    Location = "Military",
                    Offset = new TimeSpan(11, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "LHDT",
                    Name = "Lord Howe Daylight Time",
                    Location = "Australia",
                    Offset = new TimeSpan(11, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "LHST",
                    Name = "Lord Howe Standard Time",
                    Location = "Australia",
                    Offset = new TimeSpan(10, 30, 0)
                },
                new TimeZone
                {
                    Abbreviation = "LINT",
                    Name = "Line Islands Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(14, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "M",
                    Name = "Mike Time Zone",
                    Location = "Military",
                    Offset = new TimeSpan(12, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "MAGST",
                    Name = "Magadan Summer Time",
                    Location = "Asia",
                    Offset = new TimeSpan(12, 0, 0)
                },new TimeZone
                {
                    Abbreviation = "MAGT",
                    Name = "Magadan Time",
                    Location = "Asia",
                    Offset = new TimeSpan(11, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "MART",
                    Name = "Marquesas Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(-9, -30, 0)
                },
                new TimeZone
                {
                    Abbreviation = "MAWT",
                    Name = "Mawson Time",
                    Location = "Antarctica",
                    Offset = new TimeSpan(5, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "MDT",
                    Name = "Mountain Daylight Time",
                    Location = "North America",
                    Offset = new TimeSpan(-6, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "MHT",
                    Name = "Marshall Islands Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(12, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "MMT",
                    Name = "Myanmar Time",
                    Location = "Asia",
                    Offset = new TimeSpan(6, 30, 0)
                },
                new TimeZone
                {
                    Abbreviation = "MSD",
                    Name = "Moscow Daylight Time",
                    Location = "Europe",
                    Offset = new TimeSpan(4, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "MSK",
                    Name = "Moscow Standard Time",
                    Location = "Europe",
                    Offset = new TimeSpan(3, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "MST",
                    Name = "Mountain Standard Time",
                    Location = "North America",
                    Offset = new TimeSpan(-7, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "MT",
                    Name = "Mountain Time",
                    Location = "North America",
                    Offset = new TimeSpan(-6, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "MUT",
                    Name = "Mauritius Time",
                    Location = "Africa",
                    Offset = new TimeSpan(4, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "MVT",
                    Name = "Maldives Time",
                    Location = "Asia",
                    Offset = new TimeSpan(5, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "MYT",
                    Name = "Malaysia Time",
                    Location = "Asia",
                    Offset = new TimeSpan(8, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "N",
                    Name = "November Time Zone",
                    Location = "Military",
                    Offset = new TimeSpan(-1, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "NCT",
                    Name = "New Caledonia Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(11, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "NDT",
                    Name = "Newfoundland Daylight Time",
                    Location = "North America",
                    Offset = new TimeSpan(-2, -30, 0)
                },
                new TimeZone
                {
                    Abbreviation = "NFT",
                    Name = "Norfolk Time",
                    Location = "Australia",
                    Offset = new TimeSpan(11, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "NOVST",
                    Name = "Novosibirsk Summer Time",
                    Location = "Asia",
                    Offset = new TimeSpan(7, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "NOVT",
                    Name = "Novosibirsk Time",
                    Location = "Asia",
                    Offset = new TimeSpan(7, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "NPT",
                    Name = "Nepal Time",
                    Location = "Asia",
                    Offset = new TimeSpan(5, 45, 0)
                },
                new TimeZone
                {
                    Abbreviation = "NRT",
                    Name = "Nauru Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(12, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "NST",
                    Name = "Newfoundland Standard Time",
                    Location = "North America",
                    Offset = new TimeSpan(-3, -30, 0)
                },
                new TimeZone
                {
                    Abbreviation = "NUT",
                    Name = "Niue Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(-11, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "NZDT",
                    Name = "New Zealand Daylight Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(13, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "NZST",
                    Name = "New Zealand Standard Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(12, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "O",
                    Name = "Oscar Time Zone",
                    Location = "Military",
                    Offset = new TimeSpan(-2, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "OMSST",
                    Name = "Omsk Summer Time",
                    Location = "Asia",
                    Offset = new TimeSpan(7, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "OMST",
                    Name = "Omsk Standard Time",
                    Location = "Asia",
                    Offset = new TimeSpan(6, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "ORAT",
                    Name = "Oral Time",
                    Location = "Asia",
                    Offset = new TimeSpan(5, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "P",
                    Name = "Papa Time Zone",
                    Location = "Military",
                    Offset = new TimeSpan(-3, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "PDT",
                    Name = "Pacific Daylight Time",
                    Location = "North America",
                    Offset = new TimeSpan(-7, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "PET",
                    Name = "Peru Time",
                    Location = "South America",
                    Offset = new TimeSpan(-5, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "PETST",
                    Name = "Kamchatka Summer Time",
                    Location = "Asia",
                    Offset = new TimeSpan(12, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "PETT",
                    Name = "Kamchatka Time",
                    Location = "Asia",
                    Offset = new TimeSpan(12, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "PGT",
                    Name = "Papua New Guinea Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(10, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "PHOT",
                    Name = "Phoenix Island Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(13, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "PHT",
                    Name = "Philippine Time",
                    Location = "Asia",
                    Offset = new TimeSpan(8, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "PKT",
                    Name = "Pakistan Standard Time",
                    Location = "Asia",
                    Offset = new TimeSpan(5, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "PMDT",
                    Name = "Pierre & Miquelon Daylight Time",
                    Location = "North America",
                    Offset = new TimeSpan(-2, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "PMST",
                    Name = "Pierre & Miquelon Standard Time",
                    Location = "North America",
                    Offset = new TimeSpan(-3, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "PONT",
                    Name = "Pohnpei Standard Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(11, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "PST",
                    Name = "Pacific Standard Time",
                    Location = "North America",
                    Offset = new TimeSpan(-8, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "PT",
                    Name = "Pacific Time",
                    Location = "North America",
                    Offset = new TimeSpan(-7, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "PWT",
                    Name = "Palau Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(-7, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "PYST",
                    Name = "Paraguay Summer Time",
                    Location = "South America",
                    Offset = new TimeSpan(-3, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "PYT",
                    Name = "Paraguay Time",
                    Location = "South America",
                    Offset = new TimeSpan(-4, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "Q",
                    Name = "Quebec Time Zone",
                    Location = "Military",
                    Offset = new TimeSpan(-4, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "QYZT",
                    Name = "Qyzylorda Time",
                    Location = "Asia",
                    Offset = new TimeSpan(6, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "R",
                    Name = "Romeo Time Zone",
                    Location = "Military",
                    Offset = new TimeSpan(-5, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "RET",
                    Name = "Reunion Time",
                    Location = "Africa",
                    Offset = new TimeSpan(4, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "ROTT",
                    Name = "Rothera Time",
                    Location = "Antarctica",
                    Offset = new TimeSpan(-3, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "S",
                    Name = "Sierra Time Zone",
                    Location = "Military",
                    Offset = new TimeSpan(-6, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "SAKT",
                    Name = "Sakhalin Time",
                    Location = "Asia",
                    Offset = new TimeSpan(11, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "SAMT",
                    Name = "Samara Time",
                    Location = "Europe",
                    Offset = new TimeSpan(4, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "SAST",
                    Name = "South Africa Standard Time",
                    Location = "Africa",
                    Offset = new TimeSpan(2, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "SBT",
                    Name = "Solomon Islands Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(11, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "SCT",
                    Name = "Seychelles Time",
                    Location = "Africa",
                    Offset = new TimeSpan(4, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "SGT",
                    Name = "Singapore Time",
                    Location = "Asia",
                    Offset = new TimeSpan(8, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "SRET",
                    Name = "Srednekolymsk Time",
                    Location = "Asia",
                    Offset = new TimeSpan(11, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "SRT",
                    Name = "Suriname Time",
                    Location = "South America",
                    Offset = new TimeSpan(-3, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "SST",
                    Name = "Samoa Standard Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(-11, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "SYOT",
                    Name = "Syowa Time",
                    Location = "Antarctica",
                    Offset = new TimeSpan(3, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "T",
                    Name = "Tango Time Zone",
                    Location = "Military",
                    Offset = new TimeSpan(-7, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "TAHT",
                    Name = "Tahiti Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(-10, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "TFT",
                    Name = "French Southern and Antarctic Time",
                    Location = "Indian Ocean",
                    Offset = new TimeSpan(5, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "TJT",
                    Name = "Tajikistan Time",
                    Location = "Asia",
                    Offset = new TimeSpan(5, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "TKT",
                    Name = "Tokelau Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(13, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "TLT",
                    Name = "East Timor Time",
                    Location = "Asia",
                    Offset = new TimeSpan(9, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "TMT",
                    Name = "Turkmenistan Time",
                    Location = "Asia",
                    Offset = new TimeSpan(5, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "TOST",
                    Name = "Tonga Summer Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(14, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "TOT",
                    Name = "Tonga Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(13, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "TRT",
                    Name = "Turkey Time",
                    Location = "Asia",
                    Offset = new TimeSpan(3, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "TVT",
                    Name = "Tuvalu Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(12, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "U",
                    Name = "Uniform Time Zone",
                    Location = "Military",
                    Offset = new TimeSpan(-8, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "ULAST",
                    Name = "Ulaanbaatar Summer Time",
                    Location = "Asia",
                    Offset = new TimeSpan(9, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "ULAT",
                    Name = "Ulaanbaatar Time ",
                    Location = "Asia",
                    Offset = new TimeSpan(8, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "UTC",
                    Name = "Coordinated Universal Time",
                    Location = "Worldwide",
                    Offset = new TimeSpan(0, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "UYST",
                    Name = "Uruguay Summer Time",
                    Location = "South America",
                    Offset = new TimeSpan(-2, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "UYT",
                    Name = "Uruguay Time",
                    Location = "South America",
                    Offset = new TimeSpan(-3, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "UZT",
                    Name = "Uzbekistan Time",
                    Location = "Asia",
                    Offset = new TimeSpan(5, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "V",
                    Name = "Victor Time Zone",
                    Location = "Military",
                    Offset = new TimeSpan(-9, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "VET",
                    Name = "Venezuelan Standard Time",
                    Location = "South America",
                    Offset = new TimeSpan(-4, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "VLAST",
                    Name = "Vladivostok Summer Time",
                    Location = "Asia",
                    Offset = new TimeSpan(11, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "VLAT",
                    Name = "Vladivostok Time",
                    Location = "Asia",
                    Offset = new TimeSpan(10, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "VOST",
                    Name = "Vostok Time",
                    Location = "Antarctica",
                    Offset = new TimeSpan(6, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "VUT",
                    Name = "Vanuatu Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(11, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "W",
                    Name = "Whiskey Time Zone",
                    Location = "Military",
                    Offset = new TimeSpan(-10, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "WAKT",
                    Name = "Wake Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(12, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "WARST",
                    Name = "Western Argentine Summer Time",
                    Location = "South America",
                    Offset = new TimeSpan(-3, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "WAST",
                    Name = "West Africa Summer Time",
                    Location = "Africa",
                    Offset = new TimeSpan(2, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "WAT",
                    Name = "West Africa Time",
                    Location = "Africa",
                    Offset = new TimeSpan(1, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "WEST",
                    Name = "Western European Summer Time",
                    Location = "Europe",
                    Offset = new TimeSpan(1, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "WET",
                    Name = "Western European Time",
                    Location = "Europe",
                    Offset = new TimeSpan(0, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "WFT",
                    Name = "Wallis and Futuna Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(12, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "WGST",
                    Name = "Western Greenland Summer Time",
                    Location = "North America",
                    Offset = new TimeSpan(-2, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "WGT",
                    Name = "West Greenland Time",
                    Location = "North America",
                    Offset = new TimeSpan(-3, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "WIB",
                    Name = "Western Indonesian Time",
                    Location = "Asia",
                    Offset = new TimeSpan(7, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "WIT",
                    Name = "Eastern Indonesian Time",
                    Location = "Asia",
                    Offset = new TimeSpan(9, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "WITA",
                    Name = "Central Indonesian Time",
                    Location = "Asia",
                    Offset = new TimeSpan(8, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "WST",
                    Name = "West Samoa Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(14, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "WT",
                    Name = "Western Sahara Standard Time",
                    Location = "Africa",
                    Offset = new TimeSpan(0, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "X",
                    Name = "X-ray Time Zone",
                    Location = "Military",
                    Offset = new TimeSpan(-11, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "Y",
                    Name = "Yankee Time Zone",
                    Location = "Military",
                    Offset = new TimeSpan(-12, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "YAKST",
                    Name = "Yakutsk Summer Time",
                    Location = "Asia",
                    Offset = new TimeSpan(10, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "YAKT",
                    Name = "Yakutsk Time",
                    Location = "Asia",
                    Offset = new TimeSpan(9, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "YAPT",
                    Name = "Yap Time",
                    Location = "Pacific",
                    Offset = new TimeSpan(10, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "YEKST",
                    Name = "Yekaterinburg Summer Time",
                    Location = "Asia",
                    Offset = new TimeSpan(6, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "YEKT",
                    Name = "Yekaterinburg Time",
                    Location = "Asia",
                    Offset = new TimeSpan(5, 0, 0)
                },
                new TimeZone
                {
                    Abbreviation = "Z",
                    Name = "Zulu Time Zone",
                    Location = "Military",
                    Offset = new TimeSpan(0, 0, 0)
                }
            };

            TimeZoneAbbeviations = TimeZones.ToDictionary(
                p => p.Abbreviation, p => p, StringComparer.InvariantCultureIgnoreCase);
        }

        private static List<TimeZone> TimeZones { get; }

        private static Dictionary<string, TimeZone> TimeZoneAbbeviations { get; }
    }
}
