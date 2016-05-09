using System.Configuration;

namespace OpenShare.Net.Library.Services
{
    public class ConfigurationFactory : IConfigurationFactory
    {
        public IConfigurationModel Create()
        {
            return new ConfigurationModel
            {
                ApplicationName = ConfigurationManager.AppSettings["ApplicationName"],
                Environment = ConfigurationManager.AppSettings["Environment"],
                ErrorEmailGroup = ConfigurationManager.AppSettings["ErrorEmailGroup"]
            };
        }
    }
}
