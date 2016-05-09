using System;
using System.Linq;
using System.Reflection;

namespace OpenShare.Net.Library.Common
{
    public static class ModelExtensions
    {
        public static void TrimStrings<T>(this T model)
            where T : class
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).
                Where(p => p.PropertyType == typeof(string));

            foreach (var p in properties)
            {
                if (!p.CanRead || !p.CanWrite)
                    continue;

                var value = (string)p.GetValue(model, null);
                if (string.IsNullOrEmpty(value))
                    value = string.Empty;

                value = value.Trim();
                p.SetValue(model, value, null);
            }
        }

        public static void CleanStrings<T>(this T model)
            where T : class
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.TrimStrings();

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).
                Where(p => p.PropertyType == typeof(string));

            foreach (var p in properties)
            {
                if (!p.CanRead || !p.CanWrite)
                    continue;

                var value = (string)p.GetValue(model, null);
                if (string.IsNullOrWhiteSpace(value))
                    value = null;

                p.SetValue(model, value, null);
            }
        }
    }
}
