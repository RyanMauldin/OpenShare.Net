using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Permissions;
using System.Text;
using System.Web;

namespace OpenShare.Net.Library.Services
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public class LogService : ILogService
    {
        private readonly string _applicationName;
        private readonly string _environment;
        private readonly string _errorEmailGroup;
        private readonly IMailService _mailService;

        public LogService(
            IConfigurationService configurationService,
            IMailService mailService)
        {
            _applicationName = configurationService.Configuration.ApplicationName;
            _environment = configurationService.Configuration.Environment;
            _errorEmailGroup = configurationService.Configuration.ErrorEmailGroup;
            _mailService = mailService;
        }

        private static string GetErrorMessage(Exception exception, DateTime date)
        {
            var dateString = date.ToString("MM-dd-yyyy HH:mm:ss");
            try
            {
                var exceptionMessage = string.IsNullOrEmpty(exception.Message)
                    ? string.Empty
                    : exception.Message;
                if (exceptionMessage.Length == 0)
                    throw new Exception("Exception Message is a null or empty string.");

                var innerExceptionMessage = exception.InnerException == null
                    ? string.Empty
                    : string.IsNullOrEmpty(exception.InnerException.Message)
                        ? string.Empty
                        : exception.InnerException.Message;

                var stackTrace = string.IsNullOrEmpty(exception.StackTrace)
                    ? string.Empty
                    : exception.StackTrace;

                var builder = new StringBuilder(dateString.Length + exceptionMessage.Length +
                    innerExceptionMessage.Length + stackTrace.Length + 63);

                builder.AppendFormat("{0} - Exception: {1}\n", dateString, exceptionMessage);
                if (!string.IsNullOrEmpty(innerExceptionMessage))
                    builder.AppendFormat("Inner Exception: {0}\n", innerExceptionMessage);
                if (!string.IsNullOrEmpty(stackTrace))
                    builder.AppendFormat("Stack Trace: {0}\n", stackTrace);

                return builder.ToString();
            }
            catch (Exception ex)
            {
                var exceptionMessage = string.IsNullOrEmpty(ex.Message)
                   ? "Unknown Error"
                   : ex.Message;
                return string.Format("<p>{0} - Log Exception: {1}</p>", dateString, exceptionMessage);
            }
        }

        private static string GetWebFriendlyErrorMessage(Exception exception, DateTime date)
        {
            var dateString = date.ToString("MM-dd-yyyy HH:mm:ss");
            try
            {
                var exceptionMessage = string.IsNullOrEmpty(exception.Message)
                    ? string.Empty
                    : exception.Message;
                if (exceptionMessage.Length == 0)
                    throw new Exception("Exception Message is a null or empty string.");

                var innerExceptionMessage = exception.InnerException == null
                    ? string.Empty
                    : string.IsNullOrEmpty(exception.InnerException.Message)
                        ? string.Empty
                        : exception.InnerException.Message;

                var stackTrace = string.IsNullOrEmpty(exception.StackTrace)
                    ? string.Empty
                    : exception.StackTrace;

                var builder = new StringBuilder(dateString.Length + exceptionMessage.Length +
                    innerExceptionMessage.Length + stackTrace.Length + 63);

                builder.AppendFormat("<p>{0}</p>", HttpUtility.HtmlEncode(HttpUtility.HtmlDecode(dateString)));
                builder.AppendFormat("<p>Exception: {0}<p>", HttpUtility.HtmlEncode(HttpUtility.HtmlDecode(exceptionMessage)));
                if (!string.IsNullOrEmpty(innerExceptionMessage))
                    builder.AppendFormat("<p>Inner Exception: {0}<p>", HttpUtility.HtmlEncode(HttpUtility.HtmlDecode(innerExceptionMessage)));
                if (!string.IsNullOrEmpty(stackTrace))
                    builder.AppendFormat("<p>Stack Trace: {0}<p>", HttpUtility.HtmlEncode(HttpUtility.HtmlDecode(stackTrace)));

                return builder.ToString();
            }
            catch (Exception ex)
            {
                var exceptionMessage = string.IsNullOrEmpty(ex.Message)
                   ? "Unknown Error"
                   : ex.Message;
                return string.Format("<p>{0} - Log Exception: {1}</p>",
                    HttpUtility.HtmlEncode(HttpUtility.HtmlDecode(dateString)),
                    HttpUtility.HtmlEncode(HttpUtility.HtmlDecode(exceptionMessage)));
            }
        }

        private static string GetRunningApplicationName()
        {
            return HttpContext.Current.ApplicationInstance.GetType().Assembly.GetName().ToString();
        }

        private string GetErrorMessageSubject()
        {
            var applicationName = string.IsNullOrEmpty(_applicationName)
                ? GetRunningApplicationName()
                : _applicationName.Trim();
            var environment = string.IsNullOrEmpty(_environment)
                ? string.Empty
                : _environment.Trim();
            return string.Format("{0}{1}",
                applicationName,
                string.IsNullOrEmpty(environment)
                    ? string.Empty
                    : string.Format(" - {0}", environment));
        }

        private static void LogErrorInternal(Exception exception, DateTime date)
        {
            try
            {
                Trace.Write(GetErrorMessage(exception, date));
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void EmailErrorInternal(Exception exception, DateTime date)
        {
            try
            {
                _mailService.Send(
                    new Dictionary<string, string>
                    {
                        { _errorEmailGroup, null }
                    },
                    GetErrorMessageSubject(),
                    GetWebFriendlyErrorMessage(exception, date),
                    true);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void LogError(Exception exception)
        {
            LogErrorInternal(exception, DateTime.Now);
        }

        public void EmailError(Exception exception)
        {
            EmailErrorInternal(exception, DateTime.Now);
        }

        public void LogAndEmailError(Exception exception)
        {
            var date = DateTime.Now;
            LogErrorInternal(exception, date);
            EmailErrorInternal(exception, date);
        }
    }
}
