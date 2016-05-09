using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Permissions;
using Microsoft.Exchange.WebServices.Data;
using OpenShare.Net.Library.Common;

namespace OpenShare.Net.Library.Services
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public class MailService : IMailService
    {
        private static SecureString Url
        {
            get { return ConfigurationHelper.GetSecureStringFromAppSettings("ExchangeServiceUrl"); }
        }

        private static SecureString Domain
        {
            get { return ConfigurationHelper.GetSecureStringFromAppSettings("ExchangeServiceDomain"); }
        }

        private static SecureString Username
        {
            get { return ConfigurationHelper.GetSecureStringFromAppSettings("ExchangeServiceUsername"); }
        }

        private static SecureString Email
        {
            get { return ConfigurationHelper.GetSecureStringFromAppSettings("ExchangeServiceEmail"); }
        }

        private static SecureString Password
        {
            get { return ConfigurationHelper.GetSecureStringFromAppSettings("ExchangeServicePassword"); }
        }

        private static bool CertificateValidationCallBack(
         object sender,
         System.Security.Cryptography.X509Certificates.X509Certificate certificate,
         System.Security.Cryptography.X509Certificates.X509Chain chain,
         System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public void Send(
            Dictionary<string, string> to,
            string subject,
            string body,
            bool isBodyHtml,
            bool sendAndSaveCopy = false)
        {
            Send(to, new Dictionary<string, string>(), new Dictionary<string, string>(), subject, body, isBodyHtml);
        }

        public void Send(
            Dictionary<string, string> to,
            Dictionary<string, string> cc,
            Dictionary<string, string> bcc,
            string subject,
            string body,
            bool isBodyHtml,
            bool sendAndSaveCopy = false)
        {
            var exchangeService = new ExchangeService(ExchangeVersion.Exchange2010_SP2)
            {
                Credentials = new WebCredentials(Username.ToUnsecureString(), Password.ToUnsecureString(), Domain.ToUnsecureString()),
                Url = new Uri(Url.ToUnsecureString())
            };

            ServicePointManager.ServerCertificateValidationCallback = CertificateValidationCallBack;

            var emailMessage = new EmailMessage(exchangeService)
            {
                Subject = subject,
                Body = new MessageBody(isBodyHtml ? BodyType.HTML : BodyType.Text, body)
            };

            if (to.Any(p => string.IsNullOrEmpty(p.Key)))
                throw new Exception("Mail message to field has invalid entries.");

            foreach (var keyValuePair in to)
            {
                emailMessage.ToRecipients.Add(
                    string.IsNullOrEmpty(keyValuePair.Value)
                        ? new EmailAddress(keyValuePair.Key)
                        : new EmailAddress(keyValuePair.Value, keyValuePair.Key));
            }

            if (cc.Any(p => string.IsNullOrEmpty(p.Key)))
                throw new Exception("Mail message cc field has invalid entries.");

            foreach (var keyValuePair in cc)
            {
                emailMessage.CcRecipients.Add(
                    string.IsNullOrEmpty(keyValuePair.Value)
                        ? new EmailAddress(keyValuePair.Key)
                        : new EmailAddress(keyValuePair.Value, keyValuePair.Key));
            }

            if (bcc.Any(p => string.IsNullOrEmpty(p.Key)))
                throw new Exception("Mail message bcc field has invalid entries.");

            foreach (var keyValuePair in bcc)
            {
                emailMessage.BccRecipients.Add(
                    string.IsNullOrEmpty(keyValuePair.Value)
                        ? new EmailAddress(keyValuePair.Key)
                        : new EmailAddress(keyValuePair.Value, keyValuePair.Key));
            }

            if (sendAndSaveCopy)
            {
                emailMessage.SendAndSaveCopy();
                return;
            }

            emailMessage.Send();
        }
    }
}
