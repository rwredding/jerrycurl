namespace Jerrycurl.Data.Sessions
{
    public interface ISessionOptions
    {
        IAsyncSession GetAsyncSession();
        ISyncSession GetSyncSession();
    }
}
