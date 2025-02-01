namespace ChatService.Infrastructure.Broker
{
    public class NatsConfiguration
    {
        public const string Nats = "Nats";
        public string Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int CommandTimeout { get; set; } = 5;
        public int ConnectTimeout { get; set; } = 5;
        public int RequestTimeout { get; set; } = 5;
        public int MaxReconnectRetry { get; set; } = 3;
        public int MaxAge { get; set; } = 30;
        public int MaxBytes { get; set; } = 10;
        public int DeliverPolicy { get; set; } = 0;
    }
}
