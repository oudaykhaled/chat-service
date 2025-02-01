namespace ChatService.Domain.Response
{
    public class ChannelResponse : IdentityResponse
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Tag { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
