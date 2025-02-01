namespace ChatService.Domain.Request
{
    public class CreateChannelRequest
    {
        public string MemberID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Tag { get; set; }
    }
}
