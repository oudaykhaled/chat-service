namespace ChatService.Infrastructure.Broker
{
    public interface IBusConnection
    {
        bool Connect();
        bool Disconnect();
    }
}
