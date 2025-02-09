using Microsoft.Extensions.Options;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using NATS.Client.KeyValueStore;

namespace ChatService.Infrastructure.Broker
{
    public class NatsStream : IBusStream
    {
        private readonly IBusConnection _connection;
        private readonly NatsConfiguration _natsConfiguration;

        public NatsStream(IBusConnection connection, IOptions<NatsConfiguration> options)
        {
            _connection = connection;
            _natsConfiguration = options.Value;
        }

        public async Task<object> CreateOrUpdateStream(string stream, string subject)
        {
            long sizeInBytes = 1024 * 1024;  // 1 MB in bytes
            sizeInBytes = sizeInBytes * _natsConfiguration.MaxBytes * 1024; // MaxBytes GB

            // Create a stream to store the messages
            StreamConfig streamConfig = new StreamConfig(name: stream, subjects: new[] { subject });
            streamConfig.Retention = StreamConfigRetention.Limits;
            streamConfig.MaxBytes = sizeInBytes;
            streamConfig.MaxAge = TimeSpan.FromDays(_natsConfiguration.MaxAge);

            return await ((NatsConnection)_connection).JetContext.CreateOrUpdateStreamAsync(streamConfig);
        }
        public async Task<object> Subscribe(string stream, string name)
        {
            return await Subscribe(stream, name, null);
        }

        public async Task<object> Subscribe(string stream, string name, ulong? startSeq)
        {
            ConsumerConfig consumerConfig = new ConsumerConfig(name);

            if (startSeq.HasValue)
            {
                if (startSeq <= 0)
                    startSeq = 1;

                consumerConfig = new ConsumerConfig(name)
                {
                    DeliverPolicy = ConsumerConfigDeliverPolicy.ByStartSequence,
                    OptStartSeq = startSeq.Value,
                };
                try
                {
                    var consumer = await ((NatsConnection)_connection).JetContext.GetConsumerAsync(stream, name);
                    if (consumer != null)
                    {
                        await ((NatsConnection)_connection).JetContext.DeleteConsumerAsync(stream, name);
                    }
                }
                catch (NatsJSApiException ex) when (ex.Error.Code == 404) { }
            }
            else
            {
                try
                {
                    var consumer = await ((NatsConnection)_connection).JetContext.GetConsumerAsync(stream, name);
                    if (consumer != null)
                    {
                        if (consumer.Info.Config.DeliverPolicy != ConsumerConfigDeliverPolicy.All)
                            await ((NatsConnection)_connection).JetContext.DeleteConsumerAsync(stream, name);
                    }
                }
                catch (NatsJSApiException ex) when (ex.Error.Code == 404) { }
            }

            return await ((NatsConnection)_connection).JetContext.CreateOrUpdateConsumerAsync(stream, consumerConfig);
        }
    }
}
