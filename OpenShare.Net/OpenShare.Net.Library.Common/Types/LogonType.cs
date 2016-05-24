namespace OpenShare.Net.Library.Common.Types
{
    public enum LogonType
    {
        LogonInteractive = 2, // Common for access on same machine
        LogonNetwork = 3,
        LogonBatch = 4,
        LogonService = 5,
        LogonUnlock = 7,
        LogonNetworkCleartext = 8,
        LogonNewCredentials = 9 // Common for Network Share Access on Remote machine.
    }
}
