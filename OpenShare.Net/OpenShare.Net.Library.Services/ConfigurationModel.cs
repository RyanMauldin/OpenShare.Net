namespace OpenShare.Net.Library.Services
{
    public class ConfigurationModel : IConfigurationModel
    {
        public string ApplicationName { get; set; }
        public string Environment { get; set; }
        public string ErrorEmailGroup { get; set; }
    }
}
