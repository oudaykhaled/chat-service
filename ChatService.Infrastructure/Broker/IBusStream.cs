using NATS.Client.JetStream;

namespace ChatService.Infrastructure.Broker
{
    public interface IBusStream
    {
        Task<object> CreateOrUpdateStream(string stream, string subject);
        Task<object> Subscribe(string stream, string name);
        Task<object> Subscribe(string stream, string name, ulong? startSeq);
    }
}
