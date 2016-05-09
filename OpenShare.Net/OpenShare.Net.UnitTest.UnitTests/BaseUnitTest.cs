using System.Collections.Generic;
using System.Configuration;
using System.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenShare.Net.Library.Common;

namespace OpenShare.Net.UnitTest.UnitTests
{
    public class BaseUnitTest
    {
        protected static SecureString Domain
        {
            get { return ConfigurationHelper.GetSecureStringFromAppSettings("WebsiteShareDomain"); }
        }

        protected static SecureString Username
        {
            get { return ConfigurationHelper.GetSecureStringFromAppSettings("WebsiteShareUsername"); }
        }

        protected static SecureString Password
        {
            get { return ConfigurationHelper.GetSecureStringFromAppSettings("WebsiteSharePassword"); }
        }

        protected static string WebsiteSharePath
        {
            get { return ConfigurationManager.AppSettings["WebsiteSharePath"]; }
        }

        protected static SecureString AesPassword
        {
            get { return ConfigurationHelper.GetSecureStringFromAppSettings("AesPassword"); }
        }

        protected static SecureString AesSalt
        {
            get { return ConfigurationHelper.GetSecureStringFromAppSettings("AesSalt"); }
        }

        protected static SecureString AesPasswordIterations
        {
            get { return ConfigurationHelper.GetSecureStringFromAppSettings("AesPasswordIterations"); }
        }

        protected static SecureString AesInitialVector
        {
            get { return ConfigurationHelper.GetSecureStringFromAppSettings("AesInitialVector"); }
        }

        protected static SecureString AesKeySize
        {
            get { return ConfigurationHelper.GetSecureStringFromAppSettings("AesKeySize"); }
        }

        protected static List<string> FolderList
        {
            get
            {
                return new List<string>
                {
                    @"\File Sharing",
                    @"\File Sharing\Specific",
                };
            }
        }

        [TestInitialize]
        protected void TestInitialize()
        {
            // If needed for all tests.
        }

        [TestCleanup]
        protected void TestCleanup()
        {
            // If needed for all tests.
        }
    }
}
