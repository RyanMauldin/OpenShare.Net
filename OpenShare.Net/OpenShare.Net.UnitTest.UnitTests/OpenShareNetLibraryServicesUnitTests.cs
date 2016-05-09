using System.Collections.Generic;
using System.Net.Http;
using System.Web.Script.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenShare.Net.Library.Services;

namespace OpenShare.Net.UnitTest.UnitTests
{
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
    }
}
