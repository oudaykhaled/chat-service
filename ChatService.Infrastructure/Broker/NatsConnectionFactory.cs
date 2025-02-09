using Microsoft.Extensions.Options;
using NATS.Client.Core;

namespace ChatService.Infrastructure.Broker
{
    public class NatsConnectionFactory
    {
        private readonly NatsConfiguration _natsConfiguration;

        public NatsConnectionFactory(IOptions<NatsConfiguration> options)
        {
            _natsConfiguration = options.Value;
        }

        public NatsOpts GetOptions(string name)
        {
            var options = NatsOpts.Default with
            {
                Url = _natsConfiguration.Url,
                Name = name,
                CommandTimeout = TimeSpan.FromSeconds(_natsConfiguration.CommandTimeout),
                ConnectTimeout = TimeSpan.FromSeconds(_natsConfiguration.ConnectTimeout),
                RequestTimeout = TimeSpan.FromSeconds(_natsConfiguration.RequestTimeout),
                MaxReconnectRetry = _natsConfiguration.MaxReconnectRetry,
                AuthOpts = NatsAuthOpts.Default with { Username = _natsConfiguration.Username, Password = _natsConfiguration.Password }
            };

            return options;
        }

        public NATS.Client.Core.NatsConnection CreateConnection(NatsOpts options)
        {
            return new NATS.Client.Core.NatsConnection(options);
        }
    }
}
