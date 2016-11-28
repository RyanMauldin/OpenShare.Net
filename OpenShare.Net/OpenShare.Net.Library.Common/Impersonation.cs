using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;
using OpenShare.Net.Library.Common.Types;

namespace OpenShare.Net.Library.Common
{
    /// <summary>
    /// Class to impersonate a windows account. This is gernally good to use when
    /// elevation of privaleges is needed for service accounts in applications.
    /// </summary>
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public class Impersonation : IDisposable
    {
        /// <summary>
        /// Field to determine if this class has already been disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// The lock object for thread safety.
        /// </summary>
        public readonly object LockObject = new object();

        private readonly SafeTokenHandle _handle;
        private readonly WindowsImpersonationContext _context;

        public Impersonation(
            SecureString domain,
            SecureString username,
            SecureString password,
            LogonType logonType = LogonType.LogonInteractive,
            LogonProviderType logonProviderType = LogonProviderType.Default)
        {
            lock (LockObject)
            {
                //#if DEBUG
                if (!LogonUser(username.ToUnsecureString(), domain.ToUnsecureString(), password.ToUnsecureString(),
                        (int) logonType, (int) logonProviderType, out _handle))
                    throw new ApplicationException(
                        $"Could not impersonate the elevated user. LogonUser returned error code {Marshal.GetLastWin32Error()}.");
                //#endif

                if (!domain.IsReadOnly())
                    domain.Clear();

                if (!username.IsReadOnly())
                    username.Clear();

                if (!password.IsReadOnly())
                    password.Clear();

                //#if DEBUG
                _context = WindowsIdentity.Impersonate(_handle.DangerousGetHandle());
                //#endif
            }
        }

        /// <summary>
        /// Internal dispose, to be called from IDisposable override.
        /// </summary>
        /// <param name="disposing">If Dispose() is manually invoked.</param>
        protected virtual void Dispose(bool disposing)
        {
            lock (LockObject)
            {
                if (!_disposed && disposing)
                {
                    //#if DEBUG
                    try
                    {
                        _context.Undo();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    try
                    {
                        _context.Dispose();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    try
                    {
                        _handle.Dispose();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                    //#endif
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            lock (LockObject)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool LogonUser(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            out SafeTokenHandle phToken);

        public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private SafeTokenHandle()
                : base(true)
            {

            }

            [SuppressUnmanagedCodeSecurity]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [DllImport("kernel32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CloseHandle(IntPtr handle);

            protected override bool ReleaseHandle()
            {
                return CloseHandle(handle);
            }
        }
    }
}
