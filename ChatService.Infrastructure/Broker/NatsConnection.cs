using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.KeyValueStore;

namespace ChatService.Infrastructure.Broker
{
    public class NatsConnection : IBusConnection, IDisposable
    {
        private readonly NatsConnectionFactory _natsConnectionFactory;
        private INatsConnection _connection;
        private INatsJSContext _jetContext;
        private INatsKVContext _kvContext;
        private bool _disposed;

        public NatsConnection(NatsConnectionFactory natsConnectionFactory)
        {
            _natsConnectionFactory = natsConnectionFactory;
        }

        public INatsConnection Connection
        {
            get
            {
                return _connection;
            }
        }

        public NatsConnectionState ConnectionState
        {
            get
            {
                if (_connection != null)
                    return _connection.ConnectionState;
                return NatsConnectionState.Closed;
            }
        }

        public INatsJSContext JetContext
        {
            get
            {
                return _jetContext;
            }
        }

        public INatsKVContext KVContext
        {
            get
            {
                return _kvContext;
            }
        }

        public NatsConnectionFactory ConnectionFactory
        {
            get
            {
                return _natsConnectionFactory;
            }
        }

        public bool Connect()
        {
            bool result = false;
            try
            {
                var options = _natsConnectionFactory.GetOptions(BusConstant.ServerName);
                var _connection = _natsConnectionFactory.CreateConnection(options);
                if (_connection != null)
                {
                    _jetContext = new NatsJSContext(_connection);
                    _kvContext = new NatsKVContext(_jetContext);
                    result = true;
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public bool Disconnect()
        {
            bool result = false;

            try
            {
                if (_connection != null)
                {
                    _connection.DisposeAsync();
                    result = true;
                }
            }
            catch (Exception ex)
            {
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

        ~NatsConnection()
        {
            Disconnect();
        }
    }
}
