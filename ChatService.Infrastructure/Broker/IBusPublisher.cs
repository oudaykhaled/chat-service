namespace ChatService.Infrastructure.Broker
{
    public interface IBusPublisher
    {
        Task<bool> Publish(byte[] data, string stream, string subject);
    }
}
