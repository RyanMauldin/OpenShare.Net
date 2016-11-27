using System.Collections.Generic;
using System.Net.Http;
using System.Web.Script.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenShare.Net.Library.Services;

namespace OpenShare.Net.UnitTest.UnitTests
{
    [TestClass]
    public class OpenShareNetLibraryServicesUnitTests : BaseUnitTest
    {
        [TestMethod]
        public void HttpService_TestMethod1()
        {
            Assert.IsTrue(true);
            //var httpService = new HttpService();
            //var response = httpService.RequestJsonAsync(
            //    HttpMethod.Post,
            //    "http://127.0.0.1:8080/api/Test/Update",
            //    new Dictionary<string, string>
            //    {
            //        { "Field1", "1029294855" },
            //        { "Field2", "12345" },
            //        { "Field3", null },
            //    }).Result;

            //Assert.IsNotNull(response);
            //Assert.IsTrue(response.Length > 0);

            //var resultLookup = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(response);

            //Assert.IsNotNull(resultLookup);
            //Assert.IsTrue(resultLookup.Count > 0);
        }

        [TestMethod]
        public void LogService_Tests()
        {
            var configurationFactory = new ConfigurationFactory();
            var configurationService = new ConfigurationService(configurationFactory);
            var mailService = new MailService();
            var logService = new LogService(configurationService, mailService);
            //logService.EmailMessage("This\nemail\nhas\nspaces");
            Assert.IsNotNull(logService);
        }

        [TestMethod]
        public void HttpService_TestHttpGet()
        {
            var apiKey = "555555555555555555555555";
            var vin = "1A1AA1A11A1111111";
            var url = $"https://api.yoursite.com/api/vehicle/vins/{vin}?fmt=json&api_key={apiKey}";
            var httpService = new HttpService();
            var response = httpService.RequestJsonAsync(
                HttpMethod.Get,
                url).Result;

            Assert.IsNotNull(response);
            Assert.IsTrue(response.Length > 0);
        }
    }
}
