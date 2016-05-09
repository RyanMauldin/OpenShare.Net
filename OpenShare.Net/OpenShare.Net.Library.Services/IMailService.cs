using System.Collections.Generic;

namespace OpenShare.Net.Library.Services
{
    public interface IMailService
    {
        void Send(Dictionary<string, string> to, string subject, string body, bool isBodyHtml, bool sendAndSaveCopy = false);
        void Send(Dictionary<string, string> to, Dictionary<string, string> cc, Dictionary<string, string> bcc, string subject, string body, bool isBodyHtml, bool sendAndSaveCopy = false);
    }
}
