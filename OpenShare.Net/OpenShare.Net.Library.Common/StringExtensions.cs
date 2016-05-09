using System;
using System.Text;

namespace OpenShare.Net.Library.Common
{
    public static class StringExtensions
    {
        public static string ToBase64String(this string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }

        public static string ToBase64String(this string value, Encoding encoding)
        {
            return Convert.ToBase64String(encoding.GetBytes(value));
        }

        public static string FromBase64String(this string value)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }

        public static string FromBase64String(this string value, Encoding encoding)
        {
            return encoding.GetString(Convert.FromBase64String(value));
        }
    }
}
