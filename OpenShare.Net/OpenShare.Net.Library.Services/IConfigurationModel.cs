namespace OpenShare.Net.Library.Services
{
    public interface IConfigurationModel
    {
        string ApplicationName { get; }
        string Environment { get; }
        string ErrorEmailGroup { get; }
    }
}
