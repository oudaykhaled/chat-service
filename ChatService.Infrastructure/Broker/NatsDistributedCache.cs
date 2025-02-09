using NATS.Client.KeyValueStore;

namespace ChatService.Infrastructure.Broker
{
    public class NatsDistributedCache : IDistributedCache
    {
        private const string BucketName = "cache";
        private IBusConnection _connection;
        private INatsKVStore? _kvStore;

        public NatsDistributedCache(IBusConnection connection)
        {
            _connection = connection;
        }

        private async Task<INatsKVStore> GetStore()
        {
            if (_kvStore == null)
            {
                if (_connection.Connect())
                {
                    _kvStore = await ((NatsConnection)_connection).KVContext.CreateStoreAsync(new NatsKVConfig(BucketName));
                }
            }
            return _kvStore;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            T? result = default;
            _kvStore = await GetStore();
            if (_kvStore != null)
            {
                var entry = await _kvStore.TryGetEntryAsync<T>(key);

                return entry.Value.Value;
            }
            return result;
        }

        public async Task SetAsync<T>(string key, T data)
        {
            _kvStore = await GetStore();
            if (_kvStore != null)
            {
                await _kvStore.PutAsync<T>(key, data);
            }
        }

        public async Task DeleteAsync(string key)
        {
            _kvStore = await GetStore();
            if (_kvStore != null)
            {
                await _kvStore.DeleteAsync(key);
            }
        }

        public async Task PurgeAsync(string key)
        {
            _kvStore = await GetStore();
            if (_kvStore != null)
            {
                await _kvStore.PurgeAsync(key);
            }
        }
    }
}
