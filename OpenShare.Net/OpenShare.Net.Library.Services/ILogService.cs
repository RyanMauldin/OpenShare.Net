using System;

namespace OpenShare.Net.Library.Services
{
    public interface ILogService
    {
        void LogError(Exception exception);
        void EmailError(Exception exception);
        void LogAndEmailError(Exception exception);
        void LogMessage(string message);
        void EmailMessage(string message);
        void LogAndEmailMessage(string message);
    }
}
