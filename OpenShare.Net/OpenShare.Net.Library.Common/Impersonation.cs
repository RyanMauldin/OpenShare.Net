using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;

namespace OpenShare.Net.Library.Common
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public class Impersonation : IDisposable
    {
        private const int Logon32LogonNewCredentials = 2; // Was 9?
        private readonly SafeTokenHandle _handle;
        private readonly WindowsImpersonationContext _context;

        public Impersonation(SecureString domain, SecureString username, SecureString password)
        {
            //#if DEBUG
            if (!LogonUser(username.ToUnsecureString(), domain.ToUnsecureString(), password.ToUnsecureString(), Logon32LogonNewCredentials, 0, out _handle))
                throw new ApplicationException(
                    string.Format(
                    "Could not impersonate the elevated user. LogonUser returned error code {0}.",
                    Marshal.GetLastWin32Error()));
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

        public void Dispose()
        {
            //#if DEBUG
            _context.Undo();
            _context.Dispose();
            _handle.Dispose();
            //#endif
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
