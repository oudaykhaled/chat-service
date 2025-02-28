using ChatService.Infrastructure.Models;
using NATS.Client.JetStream;

namespace ChatService.Infrastructure.Broker
{
    public class NatsConsumer : IBusConsumer, IDisposable
    {
        private IBusConnection _connection;
        private IBusStream _stream;
        private INatsJSConsumer _natsJSConsumer;
        private bool _disposed;

        public NatsConsumer(IBusStream stream, IBusConnection connection)
        {
            _stream = stream;
            _connection = connection;
        }

        public bool Disconnect()
        {
            return _connection.Disconnect();
        }

        public async Task CreateOrUpdateStream(string stream, string subject)
        {
            if (_connection.Connect())
                await _stream.CreateOrUpdateStream(stream, subject);
        }

        public async Task<bool> Subscribe(string stream, string durable)
        {
            return await Subscribe(stream, durable, null);
        }

        public async Task<bool> Subscribe(string stream, string durable, ulong? startSeq)
        {
            if (_connection.Connect())
            {
                _natsJSConsumer = (INatsJSConsumer)await _stream.Subscribe(stream, durable, startSeq);
            }
            return _natsJSConsumer != null;
        }

        public async Task<MessageStream> NextAsync()
        {
            MessageStream result = new MessageStream();

            if (_natsJSConsumer != null)
            {
                var opts = new NatsJSNextOpts
                {
                    Expires = TimeSpan.FromSeconds(1),
                };

                NatsJSMsg<byte[]>? message = await _natsJSConsumer.NextAsync<byte[]>(opts: opts);
                if (message != null)
                {
                    result.Data = message.Value.Data;

                    if (message.Value.Metadata != null)
                    {
                        result.Offset = message.Value.Metadata.Value.Sequence.Stream;
                    }
                    await message.Value.AckAsync();
                }
            }

            return result;
        }

        public async Task<List<byte[]>> FetchAsync(int size)
        {
            List<byte[]> result = new List<byte[]>();
            if (_natsJSConsumer != null)
            {
                if (size < 0)
                    size = 100;

                var opts = new NatsJSFetchOpts
                {
                    MaxMsgs = size,
                    Expires = TimeSpan.FromSeconds(1),
                };
                var messages = _natsJSConsumer.FetchAsync<byte[]>(opts: opts);
                await foreach (var message in messages)
                {
                    if (message.Data != null)
                        result.Add(message.Data);
                }
            }

            return result;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                Disconnect();
            }
        }

        ~NatsConsumer()
        {
            Disconnect();
        }
    }
}
