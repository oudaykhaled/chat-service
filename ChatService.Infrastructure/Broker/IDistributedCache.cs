namespace ChatService.Infrastructure.Broker
{
    public interface IDistributedCache
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T data);
        Task DeleteAsync(string key);
        Task PurgeAsync(string key);
    }
}
