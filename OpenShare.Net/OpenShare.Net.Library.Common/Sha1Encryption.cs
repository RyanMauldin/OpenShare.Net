using System;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace OpenShare.Net.Library.Common
{
    public static class Sha1Encryption
    {
        public static string ToSha1Hash(this string value, bool removeHashes = true)
        {
            return Sha1Hash(value, removeHashes);
        }

        public static string ToSha1Hash(this SecureString value, bool removeHashes = true)
        {
            return Sha1Hash(value, removeHashes);
        }

        public static string Sha1Hash(string value, bool removeHashes = true)
        {
            byte[] valueBytes = null;
            try
            {
                var cryptoTransform = new SHA1CryptoServiceProvider();
                valueBytes = Encoding.ASCII.GetBytes(value);
                var cryptoBytes = cryptoTransform.ComputeHash(valueBytes);
                return removeHashes
                    ? BitConverter.ToString(cryptoBytes).Replace("-", "")
                    : BitConverter.ToString(cryptoBytes);
            }
            finally
            {
                if (valueBytes != null)
                    Array.Clear(valueBytes, 0, valueBytes.Length);
            }
        }

        public static string Sha1Hash(SecureString value, bool removeHashes = true)
        {
            byte[] valueBytes = null;
            try
            {
                var cryptoTransform = new SHA1CryptoServiceProvider();
                valueBytes = Encoding.ASCII.GetBytes(value.ToUnsecureString());
                var cryptoBytes = cryptoTransform.ComputeHash(valueBytes);
                return removeHashes
                    ? BitConverter.ToString(cryptoBytes).Replace("-", "")
                    : BitConverter.ToString(cryptoBytes);
            }
            finally
            {
                if (valueBytes != null)
                    Array.Clear(valueBytes, 0, valueBytes.Length);
                if (!value.IsReadOnly())
                    value.Clear();
            }
        }
    }
}
