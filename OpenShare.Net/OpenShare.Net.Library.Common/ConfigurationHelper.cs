using System;
using System.Configuration;
using System.Security;
using System.Security.Permissions;

namespace OpenShare.Net.Library.Common
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public static class ConfigurationHelper
    {
        public static SecureString GetSecureStringFromAppSettings(string key)
        {
            return ConfigurationManager.AppSettings[key].ToCharArray().ToSecureString();
        }

        public static DateTime GetDateFromAppSettings(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new Exception("App Settings key not found.");

            return DateTimeHelper.GetDateTimeFromShortDateString(
                ConfigurationManager.AppSettings[key]);
        }
    }
}
