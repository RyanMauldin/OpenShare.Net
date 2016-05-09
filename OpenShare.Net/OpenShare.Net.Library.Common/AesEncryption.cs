using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenShare.Net.Library.Common
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public static class AesEncryption
    {
        private static void SecureStringCleanup(
            SecureString password,
            SecureString salt,
            SecureString passwordIterations,
            SecureString initialVector,
            SecureString keySize)
        {
            if (!password.IsReadOnly())
                password.Clear();
            if (!salt.IsReadOnly())
                salt.Clear();
            if (!passwordIterations.IsReadOnly())
                passwordIterations.Clear();
            if (!keySize.IsReadOnly())
                keySize.Clear();
            if (!initialVector.IsReadOnly())
                initialVector.Clear();
        }

        public static byte[] ToAesEncryptedBytes(
            this byte[] value,
            SecureString password,
            SecureString salt,
            SecureString passwordIterations,
            SecureString initialVector,
            SecureString keySize)
        {
            return EncryptBytes(value, password, salt, passwordIterations, initialVector, keySize);
        }

        public static string ToAesEncryptedString(
            this string value,
            SecureString password,
            SecureString salt,
            SecureString passwordIterations,
            SecureString initialVector,
            SecureString keySize)
        {
            return Encrypt(value, password, salt, passwordIterations, initialVector, keySize);
        }

        public static async Task<string> ToAesEncryptedStringAsync(
            this string value,
            SecureString password,
            SecureString salt,
            SecureString passwordIterations,
            SecureString initialVector,
            SecureString keySize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await EncryptAsync(value, password, salt, passwordIterations, initialVector, keySize, cancellationToken);
        }

        public static async Task<byte[]> ToAesEncryptedBytesAsync(
            this byte[] value,
            SecureString password,
            SecureString salt,
            SecureString passwordIterations,
            SecureString initialVector,
            SecureString keySize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await EncryptBytesAsync(value, password, salt, passwordIterations, initialVector, keySize, cancellationToken);
        }

        public static string ToAesDecryptedString(
            this string value,
            SecureString password,
            SecureString salt,
            SecureString passwordIterations,
            SecureString initialVector,
            SecureString keySize)
        {
            return Decrypt(value, password, salt, passwordIterations, initialVector, keySize);
        }

        public static byte[] ToAesDecryptedBytes(
            this byte[] value,
            SecureString password,
            SecureString salt,
            SecureString passwordIterations,
            SecureString initialVector,
            SecureString keySize)
        {
            return DecryptBytes(value, password, salt, passwordIterations, initialVector, keySize);
        }

        public static async Task<string> ToAesDecryptedStringAsync(
            this string value,
            SecureString password,
            SecureString salt,
            SecureString passwordIterations,
            SecureString initialVector,
            SecureString keySize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await DecryptAsync(value, password, salt, passwordIterations, initialVector, keySize, cancellationToken);
        }

        public static async Task<byte[]> ToAesDecryptedBytesAsync(
            this byte[] value,
            SecureString password,
            SecureString salt,
            SecureString passwordIterations,
            SecureString initialVector,
            SecureString keySize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await DecryptBytesAsync(value, password, salt, passwordIterations, initialVector, keySize, cancellationToken);
        }

        public static string Encrypt(
            string value,
            SecureString password,
            SecureString salt,
            SecureString passwordIterations,
            SecureString initialVector,
            SecureString keySize)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                    return value;

                byte[] buffer = null;
                byte[] saltBytes = null;
                byte[] initialVectorBytes = null;
                byte[] derivedBytes = null;
                byte[] valueBytes = null;

                try
                {
                    saltBytes = Encoding.ASCII.GetBytes(salt.ToUnsecureString());
                    initialVectorBytes = Encoding.ASCII.GetBytes(initialVector.ToUnsecureString());

                    using (var rfcDeriveBytes = new Rfc2898DeriveBytes(password.ToUnsecureString(), saltBytes, Convert.ToInt32(passwordIterations.ToUnsecureString())))
                    using (var rijndaelManaged = new RijndaelManaged { Mode = CipherMode.CBC })
                    {
                        derivedBytes = rfcDeriveBytes.GetBytes(Convert.ToInt32(keySize.ToUnsecureString()) / 8);
                        using (var encryptor = rijndaelManaged.CreateEncryptor(derivedBytes, initialVectorBytes))
                        using (var memoryStream = new MemoryStream())
                        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            valueBytes = Encoding.UTF8.GetBytes(value);
                            cryptoStream.Write(valueBytes, 0, valueBytes.Length);
                            cryptoStream.FlushFinalBlock();
                            buffer = memoryStream.ToArray();
                            memoryStream.Close();
                            cryptoStream.Close();
                            rijndaelManaged.Clear();
                            return Convert.ToBase64String(buffer);
                        }
                    }
                }
                finally
                {
                    if (valueBytes != null)
                        Array.Clear(valueBytes, 0, valueBytes.Length);
                    if (buffer != null)
                        Array.Clear(buffer, 0, buffer.Length);
                    if (saltBytes != null)
                        Array.Clear(saltBytes, 0, saltBytes.Length);
                    if (initialVectorBytes != null)
                        Array.Clear(initialVectorBytes, 0, initialVectorBytes.Length);
                    if (derivedBytes != null)
                        Array.Clear(derivedBytes, 0, derivedBytes.Length);
                }
            }
            finally
            {
                SecureStringCleanup(password, salt, passwordIterations, initialVector, keySize);
            }
        }

        public static byte[] EncryptBytes(
            byte[] value,
            SecureString password,
            SecureString salt,
            SecureString passwordIterations,
            SecureString initialVector,
            SecureString keySize)
        {
            try
            {
                if (value == null || value.Length == 0)
                    return value;

                byte[] buffer = null;
                byte[] saltBytes = null;
                byte[] initialVectorBytes = null;
                byte[] derivedBytes = null;

                try
                {
                    saltBytes = Encoding.ASCII.GetBytes(salt.ToUnsecureString());
                    initialVectorBytes = Encoding.ASCII.GetBytes(initialVector.ToUnsecureString());

                    using (var rfcDeriveBytes = new Rfc2898DeriveBytes(password.ToUnsecureString(), saltBytes, Convert.ToInt32(passwordIterations.ToUnsecureString())))
                    using (var rijndaelManaged = new RijndaelManaged { Mode = CipherMode.CBC })
                    {
                        derivedBytes = rfcDeriveBytes.GetBytes(Convert.ToInt32(keySize.ToUnsecureString()) / 8);
                        using (var encryptor = rijndaelManaged.CreateEncryptor(derivedBytes, initialVectorBytes))
                        using (var memoryStream = new MemoryStream())
                        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(value, 0, value.Length);
                            cryptoStream.FlushFinalBlock();
                            buffer = memoryStream.ToArray();
                            memoryStream.Close();
                            cryptoStream.Close();
                            rijndaelManaged.Clear();
                            return Encoding.ASCII.GetBytes(Convert.ToBase64String(buffer));
                        }
                    }
                }
                finally
                {
                    if (buffer != null)
                        Array.Clear(buffer, 0, buffer.Length);
                    if (saltBytes != null)
                        Array.Clear(saltBytes, 0, saltBytes.Length);
                    if (initialVectorBytes != null)
                        Array.Clear(initialVectorBytes, 0, initialVectorBytes.Length);
                    if (derivedBytes != null)
                        Array.Clear(derivedBytes, 0, derivedBytes.Length);
                }
            }
            finally
            {
                SecureStringCleanup(password, salt, passwordIterations, initialVector, keySize);
            }
        }

        public static async Task<string> EncryptAsync(
            string value,
            SecureString password,
            SecureString salt,
            SecureString passwordIterations,
            SecureString initialVector,
            SecureString keySize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (string.IsNullOrEmpty(value))
                    return value;

                byte[] buffer = null;
                byte[] saltBytes = null;
                byte[] initialVectorBytes = null;
                byte[] derivedBytes = null;
                byte[] valueBytes = null;

                try
                {
                    saltBytes = Encoding.ASCII.GetBytes(salt.ToUnsecureString());
                    initialVectorBytes = Encoding.ASCII.GetBytes(initialVector.ToUnsecureString());

                    using (var rfcDeriveBytes = new Rfc2898DeriveBytes(password.ToUnsecureString(), saltBytes, Convert.ToInt32(passwordIterations.ToUnsecureString())))
                    using (var rijndaelManaged = new RijndaelManaged { Mode = CipherMode.CBC })
                    {
                        derivedBytes = rfcDeriveBytes.GetBytes(Convert.ToInt32(keySize.ToUnsecureString()) / 8);
                        using (var encryptor = rijndaelManaged.CreateEncryptor(derivedBytes, initialVectorBytes))
                        using (var memoryStream = new MemoryStream())
                        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            valueBytes = Encoding.UTF8.GetBytes(value);
                            await cryptoStream.WriteAsync(valueBytes, 0, valueBytes.Length, cancellationToken);
                            cryptoStream.FlushFinalBlock();
                            buffer = memoryStream.ToArray();
                            memoryStream.Close();
                            cryptoStream.Close();
                            rijndaelManaged.Clear();
                            return Convert.ToBase64String(buffer);
                        }
                    }
                }
                finally
                {
                    if (valueBytes != null)
                        Array.Clear(valueBytes, 0, valueBytes.Length);
                    if (buffer != null)
                        Array.Clear(buffer, 0, buffer.Length);
                    if (saltBytes != null)
                        Array.Clear(saltBytes, 0, saltBytes.Length);
                    if (initialVectorBytes != null)
                        Array.Clear(initialVectorBytes, 0, initialVectorBytes.Length);
                    if (derivedBytes != null)
                        Array.Clear(derivedBytes, 0, derivedBytes.Length);
                }
            }
            finally
            {
                SecureStringCleanup(password, salt, passwordIterations, initialVector, keySize);
            }
        }

        public static async Task<byte[]> EncryptBytesAsync(
            byte[] value,
            SecureString password,
            SecureString salt,
            SecureString passwordIterations,
            SecureString initialVector,
            SecureString keySize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (value == null || value.Length == 0)
                    return value;

                byte[] buffer = null;
                byte[] saltBytes = null;
                byte[] initialVectorBytes = null;
                byte[] derivedBytes = null;

                try
                {
                    saltBytes = Encoding.ASCII.GetBytes(salt.ToUnsecureString());
                    initialVectorBytes = Encoding.ASCII.GetBytes(initialVector.ToUnsecureString());

                    using (var rfcDeriveBytes = new Rfc2898DeriveBytes(password.ToUnsecureString(), saltBytes, Convert.ToInt32(passwordIterations.ToUnsecureString())))
                    using (var rijndaelManaged = new RijndaelManaged { Mode = CipherMode.CBC })
                    {
                        derivedBytes = rfcDeriveBytes.GetBytes(Convert.ToInt32(keySize.ToUnsecureString()) / 8);
                        using (var encryptor = rijndaelManaged.CreateEncryptor(derivedBytes, initialVectorBytes))
                        using (var memoryStream = new MemoryStream())
                        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            await cryptoStream.WriteAsync(value, 0, value.Length, cancellationToken);
                            cryptoStream.FlushFinalBlock();
                            buffer = memoryStream.ToArray();
                            memoryStream.Close();
                            cryptoStream.Close();
                            rijndaelManaged.Clear();
                            return Encoding.ASCII.GetBytes(Convert.ToBase64String(buffer));
                        }
                    }
                }
                finally
                {
                    if (buffer != null)
                        Array.Clear(buffer, 0, buffer.Length);
                    if (saltBytes != null)
                        Array.Clear(saltBytes, 0, saltBytes.Length);
                    if (initialVectorBytes != null)
                        Array.Clear(initialVectorBytes, 0, initialVectorBytes.Length);
                    if (derivedBytes != null)
                        Array.Clear(derivedBytes, 0, derivedBytes.Length);
                }
            }
            finally
            {
                SecureStringCleanup(password, salt, passwordIterations, initialVector, keySize);
            }
        }

        public static string Decrypt(
            string value,
            SecureString password,
            SecureString salt,
            SecureString passwordIterations,
            SecureString initialVector,
            SecureString keySize)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                    return value;

                byte[] buffer = null;
                byte[] saltBytes = null;
                byte[] initialVectorBytes = null;
                byte[] derivedBytes = null;
                byte[] valueBytes = null;

                try
                {
                    saltBytes = Encoding.ASCII.GetBytes(salt.ToUnsecureString());
                    initialVectorBytes = Encoding.ASCII.GetBytes(initialVector.ToUnsecureString());
                    valueBytes = Convert.FromBase64String(value);
                    buffer = new byte[valueBytes.Length];
                    using (var rfcDeriveBytes = new Rfc2898DeriveBytes(password.ToUnsecureString(), saltBytes, Convert.ToInt32(passwordIterations.ToUnsecureString())))
                    using (var rijndaelManaged = new RijndaelManaged { Mode = CipherMode.CBC })
                    {
                        derivedBytes = rfcDeriveBytes.GetBytes(Convert.ToInt32(keySize.ToUnsecureString()) / 8);
                        using (var decryptor = rijndaelManaged.CreateDecryptor(derivedBytes, initialVectorBytes))
                        using (var memoryStream = new MemoryStream(valueBytes))
                        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            var count = cryptoStream.Read(buffer, 0, buffer.Length);
                            memoryStream.Close();
                            cryptoStream.Close();
                            rijndaelManaged.Clear();

                            return Encoding.UTF8.GetString(buffer, 0, count);
                        }
                    }
                }
                finally
                {
                    if (valueBytes != null)
                        Array.Clear(valueBytes, 0, valueBytes.Length);
                    if (buffer != null)
                        Array.Clear(buffer, 0, buffer.Length);
                    if (saltBytes != null)
                        Array.Clear(saltBytes, 0, saltBytes.Length);
                    if (initialVectorBytes != null)
                        Array.Clear(initialVectorBytes, 0, initialVectorBytes.Length);
                    if (derivedBytes != null)
                        Array.Clear(derivedBytes, 0, derivedBytes.Length);
                }
            }
            finally
            {
                SecureStringCleanup(password, salt, passwordIterations, initialVector, keySize);
            }
        }

        public static byte[] DecryptBytes(
            byte[] value,
            SecureString password,
            SecureString salt,
            SecureString passwordIterations,
            SecureString initialVector,
            SecureString keySize)
        {
            try
            {
                if (value == null || value.Length == 0)
                    return value;

                byte[] saltBytes = null;
                byte[] initialVectorBytes = null;
                byte[] derivedBytes = null;
                byte[] valueBytes = null;

                try
                {
                    saltBytes = Encoding.ASCII.GetBytes(salt.ToUnsecureString());
                    initialVectorBytes = Encoding.ASCII.GetBytes(initialVector.ToUnsecureString());
                    valueBytes = Convert.FromBase64String(Encoding.ASCII.GetString(value));
                    var buffer = new byte[valueBytes.Length];
                    using (var rfcDeriveBytes = new Rfc2898DeriveBytes(password.ToUnsecureString(), saltBytes, Convert.ToInt32(passwordIterations.ToUnsecureString())))
                    using (var rijndaelManaged = new RijndaelManaged { Mode = CipherMode.CBC })
                    {
                        derivedBytes = rfcDeriveBytes.GetBytes(Convert.ToInt32(keySize.ToUnsecureString()) / 8);
                        using (var decryptor = rijndaelManaged.CreateDecryptor(derivedBytes, initialVectorBytes))
                        using (var memoryStream = new MemoryStream(valueBytes))
                        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            var count = cryptoStream.Read(buffer, 0, buffer.Length);
                            memoryStream.Close();
                            cryptoStream.Close();
                            rijndaelManaged.Clear();

                            Array.Resize(ref buffer, count);
                            return buffer;
                        }
                    }
                }
                finally
                {
                    if (valueBytes != null)
                        Array.Clear(valueBytes, 0, valueBytes.Length);
                    if (saltBytes != null)
                        Array.Clear(saltBytes, 0, saltBytes.Length);
                    if (initialVectorBytes != null)
                        Array.Clear(initialVectorBytes, 0, initialVectorBytes.Length);
                    if (derivedBytes != null)
                        Array.Clear(derivedBytes, 0, derivedBytes.Length);
                }
            }
            finally
            {
                SecureStringCleanup(password, salt, passwordIterations, initialVector, keySize);
            }
        }

        public static async Task<string> DecryptAsync(
            string value,
            SecureString password,
            SecureString salt,
            SecureString passwordIterations,
            SecureString initialVector,
            SecureString keySize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (string.IsNullOrEmpty(value))
                    return value;

                byte[] buffer = null;
                byte[] saltBytes = null;
                byte[] initialVectorBytes = null;
                byte[] derivedBytes = null;
                byte[] valueBytes = null;

                try
                {
                    saltBytes = Encoding.ASCII.GetBytes(salt.ToUnsecureString());
                    initialVectorBytes = Encoding.ASCII.GetBytes(initialVector.ToUnsecureString());
                    valueBytes = Convert.FromBase64String(value);
                    buffer = new byte[valueBytes.Length];
                    using (var rfcDeriveBytes = new Rfc2898DeriveBytes(password.ToUnsecureString(), saltBytes, Convert.ToInt32(passwordIterations.ToUnsecureString())))
                    using (var rijndaelManaged = new RijndaelManaged { Mode = CipherMode.CBC })
                    {
                        derivedBytes = rfcDeriveBytes.GetBytes(Convert.ToInt32(keySize.ToUnsecureString()) / 8);
                        using (var decryptor = rijndaelManaged.CreateDecryptor(derivedBytes, initialVectorBytes))
                        using (var memoryStream = new MemoryStream(valueBytes))
                        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            var count = await cryptoStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                            memoryStream.Close();
                            cryptoStream.Close();
                            rijndaelManaged.Clear();

                            return Encoding.UTF8.GetString(buffer, 0, count);
                        }
                    }
                }
                finally
                {
                    if (valueBytes != null)
                        Array.Clear(valueBytes, 0, valueBytes.Length);
                    if (buffer != null)
                        Array.Clear(buffer, 0, buffer.Length);
                    if (saltBytes != null)
                        Array.Clear(saltBytes, 0, saltBytes.Length);
                    if (initialVectorBytes != null)
                        Array.Clear(initialVectorBytes, 0, initialVectorBytes.Length);
                    if (derivedBytes != null)
                        Array.Clear(derivedBytes, 0, derivedBytes.Length);
                }
            }
            finally
            {
                SecureStringCleanup(password, salt, passwordIterations, initialVector, keySize);
            }
        }

        public static async Task<byte[]> DecryptBytesAsync(
            byte[] value,
            SecureString password,
            SecureString salt,
            SecureString passwordIterations,
            SecureString initialVector,
            SecureString keySize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (value == null || value.Length == 0)
                    return value;

                byte[] saltBytes = null;
                byte[] initialVectorBytes = null;
                byte[] derivedBytes = null;
                byte[] valueBytes = null;

                try
                {
                    saltBytes = Encoding.ASCII.GetBytes(salt.ToUnsecureString());
                    initialVectorBytes = Encoding.ASCII.GetBytes(initialVector.ToUnsecureString());
                    valueBytes = Convert.FromBase64String(Encoding.ASCII.GetString(value));
                    var buffer = new byte[valueBytes.Length];
                    using (var rfcDeriveBytes = new Rfc2898DeriveBytes(password.ToUnsecureString(), saltBytes, Convert.ToInt32(passwordIterations.ToUnsecureString())))
                    using (var rijndaelManaged = new RijndaelManaged { Mode = CipherMode.CBC })
                    {
                        derivedBytes = rfcDeriveBytes.GetBytes(Convert.ToInt32(keySize.ToUnsecureString()) / 8);
                        using (var decryptor = rijndaelManaged.CreateDecryptor(derivedBytes, initialVectorBytes))
                        using (var memoryStream = new MemoryStream(valueBytes))
                        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            var count = await cryptoStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                            memoryStream.Close();
                            cryptoStream.Close();
                            rijndaelManaged.Clear();

                            Array.Resize(ref buffer, count);
                            return buffer;
                        }
                    }
                }
                finally
                {
                    if (valueBytes != null)
                        Array.Clear(valueBytes, 0, valueBytes.Length);
                    if (saltBytes != null)
                        Array.Clear(saltBytes, 0, saltBytes.Length);
                    if (initialVectorBytes != null)
                        Array.Clear(initialVectorBytes, 0, initialVectorBytes.Length);
                    if (derivedBytes != null)
                        Array.Clear(derivedBytes, 0, derivedBytes.Length);
                }
            }
            finally
            {
                SecureStringCleanup(password, salt, passwordIterations, initialVector, keySize);
            }
        }
    }
}
