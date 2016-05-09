using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenShare.Net.Library.Common
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public static class FileExtensions
    {
        public static bool IsFileLocked(this FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                // The file is unavailable because it is:
                // 1.) Still being written to.
                // 2.) Being processed by another thread.
                // 3.) Does not exist (has already been processed).
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        private static string GetDomainUsername(SecureString domain, SecureString username)
        {
            return domain.ToUnsecureString() + @"\" + username.ToUnsecureString();
        }

        public static void CheckDirectoryFilesForLocks(string path)
        {
            foreach (var filePath in Directory.EnumerateFiles(path))
            {
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.IsFileLocked())
                    throw new Exception("File is opened by another process: " + fileInfo.FullName);
            }
        }

        public static bool CheckFileSecurity(string path, SecureString domain, SecureString username)
        {
            var fileSecurity = File.GetAccessControl(path);

            foreach (FileSystemAccessRule fileSystemAccessRule in fileSecurity.
                GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
            {
                if (fileSystemAccessRule.FileSystemRights.Equals(FileSystemRights.FullControl))
                    return true;
            }

            return false;
        }

        public static void AddFileSecurity(string path, SecureString domain, SecureString username)
        {
            if (!CheckFileSecurity(path, domain, username))
                return;

            var fileSecurity = File.GetAccessControl(path);
            fileSecurity.AddAccessRule(
                new FileSystemAccessRule(
                    GetDomainUsername(domain, username),
                    FileSystemRights.FullControl,
                    AccessControlType.Allow));
            File.SetAccessControl(path, fileSecurity);
        }

        public static void AddFileSecurityToDirectory(string path, SecureString domain, SecureString username)
        {
            foreach (var file in Directory.GetFiles(path))
                AddFileSecurity(file, domain, username);
        }

        public static void CheckDirectoryForLocks(string path)
        {
            if (string.IsNullOrEmpty(path)
                || !Directory.Exists(path))
                throw new DirectoryNotFoundException("Directory not found: " + path);

            CheckDirectoryFilesForLocks(path);

            foreach (var directoryPath in Directory.EnumerateDirectories(path))
                CheckDirectoryForLocks(directoryPath);
        }

        public static bool CheckDirectorySecurity(string path, SecureString domain, SecureString username)
        {
            var directorySecurity = Directory.GetAccessControl(path);

            foreach (FileSystemAccessRule fileSystemAccessRule in directorySecurity.
                GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
            {
                if (fileSystemAccessRule.FileSystemRights.Equals(FileSystemRights.FullControl))
                    return true;
            }

            return false;
        }

        public static void AddDirectorySecurity(string path, SecureString domain, SecureString username)
        {
            if (CheckDirectorySecurity(path, domain, username))
            {
                AddFileSecurityToDirectory(path, domain, username);
                return;
            }

            var directorySecurity = Directory.GetAccessControl(path);
            directorySecurity.AddAccessRule(
                new FileSystemAccessRule(
                    GetDomainUsername(domain, username),
                    FileSystemRights.FullControl,
                    AccessControlType.Allow));
            Directory.SetAccessControl(path, directorySecurity);

            AddFileSecurityToDirectory(path, domain, username);
        }

        public static void AddDirectorySecurityWithSubfolders(string path, SecureString domain, SecureString username)
        {
            if (string.IsNullOrEmpty(path)
                || !Directory.Exists(path))
                throw new DirectoryNotFoundException("Directory not found: " + path);

            AddDirectorySecurity(path, domain, username);

            foreach (var directoryPath in Directory.EnumerateDirectories(path))
                AddDirectorySecurityWithSubfolders(directoryPath, domain, username);
        }

        public static void RemapDirectory(string originalPath, string newPath, SecureString domain, SecureString username)
        {
            if (!Directory.Exists(originalPath))
                return;

            CheckDirectoryForLocks(originalPath);
            AddDirectorySecurityWithSubfolders(originalPath, domain, username);
            Directory.Move(originalPath, newPath);

            if (Directory.Exists(originalPath))
                Directory.Delete(originalPath, true);
        }

        public static void CreateDirectories(string path, List<string> folderList, SecureString domain, SecureString username)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);

                AddDirectorySecurity(path, domain, username);
            }

            foreach (var folder in folderList.OrderBy(p => p).Where(p => !Directory.Exists(path + p)))
            {
                if (!Directory.Exists(path + folder))
                    Directory.CreateDirectory(path + folder);

                AddDirectorySecurity(path + folder, domain, username);
            }
        }

        public static void DeleteDirectories(string path, SecureString domain, SecureString username)
        {
            if (!Directory.Exists(path))
                return;

            CheckDirectoryForLocks(path);
            AddDirectorySecurity(path, domain, username);

            Directory.Delete(path, true);
        }

        public static string ReadFile(
            string path, SecureString domain, SecureString username)
        {
            if (!File.Exists(path))
                throw new Exception(string.Format("Could not find file: {0}.", path));

            AddFileSecurity(path, domain, username);

            using (var streamReader = File.OpenText(path))
            {
                var value = streamReader.ReadToEnd();
                streamReader.Close();
                return value;
            }
        }

        public static async Task<string> ReadFileAsync(
            string path, SecureString domain, SecureString username,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!File.Exists(path))
                throw new Exception(string.Format("Could not find file: {0}.", path));

            AddFileSecurity(path, domain, username);

            using (var streamReader = File.OpenText(path))
            {
                var value = await streamReader.ReadToEndAsync();
                streamReader.Close();
                return value;
            }
        }

        public static byte[] ReadFileBytes(
            string path, SecureString domain, SecureString username)
        {
            if (!File.Exists(path))
                throw new Exception(string.Format("Could not find file: {0}.", path));

            AddFileSecurity(path, domain, username);

            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.SequentialScan))
            {
                var buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, buffer.Length);
                fileStream.Close();
                return buffer;
            }
        }

        public static async Task<byte[]> ReadFileBytesAsync(
            string path, SecureString domain, SecureString username,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!File.Exists(path))
                throw new Exception(string.Format("Could not find file: {0}.", path));

            AddFileSecurity(path, domain, username);

            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.Asynchronous))
            {
                var buffer = new byte[fileStream.Length];
                await fileStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                fileStream.Close();
                return buffer;
            }
        }

        public static string ReadAesEncryptedFile(
            string path, SecureString domain, SecureString username,
            SecureString aesPassword, SecureString aesSalt, SecureString aesPasswordIterations, SecureString aesInitialVector, SecureString aesKeySize)
        {
            return ReadFile(path, domain, username).ToAesDecryptedString(
                aesPassword, aesSalt, aesPasswordIterations, aesInitialVector, aesKeySize);
        }

        public static async Task<string> ReadAesEncryptedFileAsync(
            string path, SecureString domain, SecureString username,
            SecureString aesPassword, SecureString aesSalt, SecureString aesPasswordIterations, SecureString aesInitialVector, SecureString aesKeySize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await (await ReadFileAsync(path, domain, username, cancellationToken)).ToAesDecryptedStringAsync(
                aesPassword, aesSalt, aesPasswordIterations, aesInitialVector, aesKeySize, cancellationToken);
        }

        public static byte[] ReadAesEncryptedFileBytes(
            string path, SecureString domain, SecureString username,
            SecureString aesPassword, SecureString aesSalt, SecureString aesPasswordIterations, SecureString aesInitialVector, SecureString aesKeySize)
        {
            return ReadFileBytes(path, domain, username).ToAesDecryptedBytes(
                aesPassword, aesSalt, aesPasswordIterations, aesInitialVector, aesKeySize);
        }

        public static async Task<byte[]> ReadAesEncryptedFileBytesAsync(
            string path, SecureString domain, SecureString username,
            SecureString aesPassword, SecureString aesSalt, SecureString aesPasswordIterations, SecureString aesInitialVector, SecureString aesKeySize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await (await ReadFileBytesAsync(path, domain, username, cancellationToken)).ToAesDecryptedBytesAsync(
                aesPassword, aesSalt, aesPasswordIterations, aesInitialVector, aesKeySize, cancellationToken);
        }

        public static void CreateFile(
            string path, SecureString domain, SecureString username, string value)
        {
            if (File.Exists(path))
            {
                AddFileSecurity(path, domain, username);
                File.Delete(path);
            }

            using (var streamWriter = File.CreateText(path))
            {
                streamWriter.Write(value);
                streamWriter.Flush();
                streamWriter.Close();
            }

            AddFileSecurity(path, domain, username);
        }

        public static async Task CreateFileAsync(
            string path, SecureString domain, SecureString username, string value,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (File.Exists(path))
            {
                AddFileSecurity(path, domain, username);
                File.Delete(path);
            }

            using (var streamWriter = File.CreateText(path))
            {
                await streamWriter.WriteAsync(value);
                await streamWriter.FlushAsync();
                streamWriter.Close();
            }

            AddFileSecurity(path, domain, username);
        }

        public static void CreateFileBytes(
            string path, SecureString domain, SecureString username, byte[] value)
        {
            if (File.Exists(path))
            {
                AddFileSecurity(path, domain, username);
                File.Delete(path);
            }

            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.SequentialScan))
            {
                fileStream.Write(value, 0, value.Length);
                fileStream.Flush(true);
                fileStream.Close();
            }

            AddFileSecurity(path, domain, username);
        }

        public static async Task CreateFileBytesAsync(
            string path, SecureString domain, SecureString username, byte[] value,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (File.Exists(path))
            {
                AddFileSecurity(path, domain, username);
                File.Delete(path);
            }

            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous))
            {
                await fileStream.WriteAsync(value, 0, value.Length, cancellationToken);
                await fileStream.FlushAsync(cancellationToken);
                fileStream.Close();
            }

            AddFileSecurity(path, domain, username);
        }

        public static void CreateAesEncrytpedFile(
            string path, SecureString domain, SecureString username, string value,
            SecureString aesPassword, SecureString aesSalt, SecureString aesPasswordIterations, SecureString aesInitialVector, SecureString aesKeySize)
        {
            CreateFile(path, domain, username, value.ToAesEncryptedString(
                aesPassword, aesSalt, aesPasswordIterations, aesInitialVector, aesKeySize));
        }

        public static async Task CreateAesEncrytpedFileAsync(
            string path, SecureString domain, SecureString username, string value,
            SecureString aesPassword, SecureString aesSalt, SecureString aesPasswordIterations, SecureString aesInitialVector, SecureString aesKeySize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            await CreateFileAsync(path, domain, username, await value.ToAesEncryptedStringAsync(
                    aesPassword, aesSalt, aesPasswordIterations, aesInitialVector, aesKeySize, cancellationToken), cancellationToken);
        }

        public static void CreateAesEncrytpedFileBytes(
            string path, SecureString domain, SecureString username, byte[] value,
            SecureString aesPassword, SecureString aesSalt, SecureString aesPasswordIterations, SecureString aesInitialVector, SecureString aesKeySize)
        {
            CreateFileBytes(path, domain, username, value.ToAesEncryptedBytes(
                aesPassword, aesSalt, aesPasswordIterations, aesInitialVector, aesKeySize));
        }

        public static async Task CreateAesEncrytpedFileBytesAsync(
            string path, SecureString domain, SecureString username, byte[] value,
            SecureString aesPassword, SecureString aesSalt, SecureString aesPasswordIterations, SecureString aesInitialVector, SecureString aesKeySize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            await CreateFileBytesAsync(path, domain, username, await value.ToAesEncryptedBytesAsync(
                    aesPassword, aesSalt, aesPasswordIterations, aesInitialVector, aesKeySize, cancellationToken), cancellationToken);
        }

        public static void AppendFile(
            string path, SecureString domain, SecureString username, string value)
        {
            if (!File.Exists(path))
                throw new Exception(string.Format("{0} does not exist.", path));

            AddFileSecurity(path, domain, username);

            using (var streamWriter = File.AppendText(path))
            {
                streamWriter.Write(value);
                streamWriter.Flush();
                streamWriter.Close();
            }
        }

        public static async Task AppendFileAsync(
            string path, SecureString domain, SecureString username, string value,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!File.Exists(path))
                throw new Exception(string.Format("{0} does not exist.", path));

            AddFileSecurity(path, domain, username);

            using (var streamWriter = File.AppendText(path))
            {
                await streamWriter.WriteAsync(value);
                await streamWriter.FlushAsync();
                streamWriter.Close();
            }
        }

        public static void AppendFileBytes(
            string path, SecureString domain, SecureString username, byte[] value)
        {
            if (!File.Exists(path))
                throw new Exception(string.Format("{0} does not exist.", path));

            AddFileSecurity(path, domain, username);

            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.None, 4096, FileOptions.SequentialScan))
            {
                fileStream.Seek(fileStream.Length, SeekOrigin.Begin);
                fileStream.Write(value, 0, value.Length);
                fileStream.Flush(true);
                fileStream.Close();
            }
        }

        public static async Task AppendFileBytesAsync(
            string path, SecureString domain, SecureString username, byte[] value,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!File.Exists(path))
                throw new Exception(string.Format("{0} does not exist.", path));

            AddFileSecurity(path, domain, username);

            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous))
            {
                fileStream.Seek(fileStream.Length, SeekOrigin.Begin);
                await fileStream.WriteAsync(value, 0, value.Length, cancellationToken);
                await fileStream.FlushAsync(cancellationToken);
                fileStream.Close();
            }
        }

        public static void AppendAesEncrytpedFile(
            string path, SecureString domain, SecureString username, string value,
            SecureString aesPassword, SecureString aesSalt, SecureString aesPasswordIterations, SecureString aesInitialVector, SecureString aesKeySize)
        {
            var builder = new StringBuilder(
                ReadAesEncryptedFile(path, domain, username,
                aesPassword, aesSalt, aesPasswordIterations, aesInitialVector, aesKeySize));
            builder.Append(value);
            File.Delete(path);
            CreateAesEncrytpedFile(path, domain, username, builder.ToString(),
                aesPassword, aesSalt, aesPasswordIterations, aesInitialVector, aesKeySize);
        }

        public static async Task AppendAesEncrytpedFileAsync(
            string path, SecureString domain, SecureString username, string value,
            SecureString aesPassword, SecureString aesSalt, SecureString aesPasswordIterations, SecureString aesInitialVector, SecureString aesKeySize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var builder = new StringBuilder(
                await ReadAesEncryptedFileAsync(path, domain, username,
                aesPassword, aesSalt, aesPasswordIterations, aesInitialVector, aesKeySize, cancellationToken));
            builder.Append(value);
            File.Delete(path);
            await CreateAesEncrytpedFileAsync(path, domain, username, builder.ToString(),
                aesPassword, aesSalt, aesPasswordIterations, aesInitialVector, aesKeySize, cancellationToken);
        }

        public static void AppendAesEncrytpedFileBytes(
            string path, SecureString domain, SecureString username, byte[] value,
            SecureString aesPassword, SecureString aesSalt, SecureString aesPasswordIterations, SecureString aesInitialVector, SecureString aesKeySize)
        {
            var valueLength = value.Length;
            var bytes = ReadAesEncryptedFileBytes(path, domain, username,
                 aesPassword, aesSalt, aesPasswordIterations, aesInitialVector, aesKeySize);
            var bytesLength = bytes.Length;
            Array.Resize(ref bytes, valueLength + bytesLength);
            Array.Copy(value, 0, bytes, bytesLength, value.Length);
            File.Delete(path);
            CreateAesEncrytpedFileBytes(path, domain, username, bytes,
                aesPassword, aesSalt, aesPasswordIterations, aesInitialVector, aesKeySize);
        }

        public static async Task AppendAesEncrytpedFileBytesAsync(
            string path, SecureString domain, SecureString username, byte[] value,
            SecureString aesPassword, SecureString aesSalt, SecureString aesPasswordIterations, SecureString aesInitialVector, SecureString aesKeySize,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var valueLength = value.Length;
            var bytes = await ReadAesEncryptedFileBytesAsync(path, domain, username,
                 aesPassword, aesSalt, aesPasswordIterations, aesInitialVector, aesKeySize, cancellationToken);
            var bytesLength = bytes.Length;
            Array.Resize(ref bytes, valueLength + bytesLength);
            Array.Copy(value, 0, bytes, bytesLength, value.Length);
            File.Delete(path);
            await CreateAesEncrytpedFileBytesAsync(path, domain, username, bytes,
                aesPassword, aesSalt, aesPasswordIterations, aesInitialVector, aesKeySize, cancellationToken);
        }
    }
}
