namespace OpenShare.Net.Library.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfigurationFactory _configurationFactory;
        private IConfigurationModel _configuration;

        public ConfigurationService(
            IConfigurationFactory configurationFactory)
        {
            _configurationFactory = configurationFactory;
        }

        public IConfigurationModel Configuration
        {
            get
            {
                return _configuration
                    ?? (_configuration = _configurationFactory.Create());
            }
        }
    }
}
