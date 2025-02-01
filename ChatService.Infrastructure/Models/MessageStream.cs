namespace ChatService.Infrastructure.Models
{
    public class MessageStream
    {
        public byte[] Data { get; set; }
        public ulong Offset { get; set; }
    }
}
