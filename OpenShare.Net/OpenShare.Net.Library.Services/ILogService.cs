using System;

namespace OpenShare.Net.Library.Services
{
    public interface ILogService
    {
        void LogError(Exception exception);
        void LogAndEmailError(Exception exception);
    }
}
