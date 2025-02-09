using NATS.Client.Core;
using NATS.Client.JetStream;

namespace ChatService.Infrastructure.Broker
{
    public class NatsPublisher : IBusPublisher, IDisposable
    {
        private IBusConnection _connection;
        private IBusStream _stream;
        private bool _disposed;

        public NatsPublisher(IBusStream stream, IBusConnection connection)
        {
            _stream = stream;
            _connection = connection;
        }

        public bool Disconnect()
        {
            return _connection.Disconnect();
        }

        private bool Connect()
        {
            if (_connection is NatsConnection)
            {
                NatsConnection natsConnection = (NatsConnection)_connection;
                if (natsConnection.Connection == null ||
                   (natsConnection.Connection != null && natsConnection.ConnectionState != NatsConnectionState.Open))
                {
                    return _connection.Connect();
                }
            }

            return false;
        }

        public async Task CreateOrUpdateStream(string stream, string subject)
        {
            if (Connect())
                await _stream.CreateOrUpdateStream(stream, subject);
        }

        public async Task<bool> Publish(byte[] data, string stream, string subject)
        {
            if (Connect())
            {
                await _stream.CreateOrUpdateStream(stream, subject);
                NatsConnection natsConnection = (NatsConnection)_connection;
                if (natsConnection.JetContext != null)
                {
                    var response = await natsConnection.JetContext.PublishAsync(subject, data);
                    return response.IsSuccess();
                }
            }
            return false;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                Disconnect();
            }
        }

        ~NatsPublisher()
        {
            Disconnect();
        }
    }
}
