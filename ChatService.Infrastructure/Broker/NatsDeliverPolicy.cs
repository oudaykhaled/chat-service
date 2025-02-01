namespace ChatService.Infrastructure.Broker
{
    public enum NatsDeliverPolicy : int
    {
        All = 0,
        ByStartSequence = 1
    }
}
