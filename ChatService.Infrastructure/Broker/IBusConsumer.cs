using ChatService.Infrastructure.Models;

namespace ChatService.Infrastructure.Broker
{
    public interface IBusConsumer
    {
        Task<bool> Subscribe(string stream, string durable);
        Task<bool> Subscribe(string stream, string durable, ulong? startSeq);
        Task<MessageStream> NextAsync();
        Task CreateOrUpdateStream(string stream, string subject);
    }
}
