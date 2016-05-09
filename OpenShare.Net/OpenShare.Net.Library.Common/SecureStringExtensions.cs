using System;
using System.Security;
using System.Security.Permissions;

namespace OpenShare.Net.Library.Common
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public static class SecureStringExtensions
    {
        public static SecureString ToSecureString(this char[] value)
        {
            if (value == null)
                return null;

            var secureString = new SecureString();
            for (int index = 0, length = value.Length; index < length; index++)
            {
                secureString.AppendChar(value[index]);
                value[index] = (char)0;
            }

            secureString.MakeReadOnly();
            Array.Clear(value, 0, value.Length);
            return secureString;
        }

        public static SecureString ToSecureString(this string value)
        {
            if (value == null)
                return null;

            var secureString = new SecureString();
            for (int index = 0, length = value.Length; index < length; index++)
                secureString.AppendChar(value[index]);

            secureString.MakeReadOnly();
            return secureString;
        }

        public static string ToUnsecureString(this SecureString value)
        {
            if (value == null)
                return null;

            var ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(value);
            try
            {
                return System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
            }
        }
    }
}
